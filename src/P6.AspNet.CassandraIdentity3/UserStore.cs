using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.AspNet.Identity;
using p6.AspNet.Identity3.Common;

namespace P6.AspNet.CassandraIdentity3
{
    
    public class UserStore<TUser, TRole> : UserStore<TUser, TRole, Guid>
    where TUser : IdentityUser,new()
    where TRole : IdentityRole<Guid>
    {
        public UserStore(IIdentityCassandraContext<TUser, TRole, Guid> databaseContext, ILookupNormalizer normalizer = null, IdentityErrorDescriber describer = null) : base(databaseContext, normalizer, describer) { }
    }

    public class UserStore<TUser, TRole, TKey> : UserStoreCommon<TUser, TRole, TKey>
        where TUser : IdentityUser, new()
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    { 
        private ISession _session;
        // Reusable prepared statements, lazy evaluated
        private readonly AsyncLazy<PreparedStatement> _createUserByUserName;
        private readonly AsyncLazy<PreparedStatement> _createUserByEmail;
        private readonly AsyncLazy<PreparedStatement> _deleteUserByUserName;
        private readonly AsyncLazy<PreparedStatement> _deleteUserByEmail;

        private readonly AsyncLazy<PreparedStatement[]> _createUser;
        private readonly AsyncLazy<PreparedStatement[]> _updateUser;
        private readonly AsyncLazy<PreparedStatement[]> _deleteUser;

        private readonly AsyncLazy<PreparedStatement> _findById;
        private readonly AsyncLazy<PreparedStatement> _findByName;
        private readonly AsyncLazy<PreparedStatement> _findByEmail;

        private readonly AsyncLazy<PreparedStatement[]> _addLogin;
        private readonly AsyncLazy<PreparedStatement[]> _removeLogin;
        private readonly AsyncLazy<PreparedStatement> _getLogins;
        private readonly AsyncLazy<PreparedStatement> _getLoginsByProvider;

        private readonly AsyncLazy<PreparedStatement> _getClaims;
        private readonly AsyncLazy<PreparedStatement> _addClaim;
        private readonly AsyncLazy<PreparedStatement> _removeClaim;

        protected IIdentityCassandraContext<TUser, TRole, TKey> DatabaseContext { get; set; }
        public UserStore(IIdentityCassandraContext<TUser, TRole, TKey> databaseContext, ILookupNormalizer normalizer = null, IdentityErrorDescriber describer = null) : 
            base( normalizer, describer)
        {
            if (databaseContext == null)
            {
                throw new ArgumentNullException(nameof(databaseContext));
            }

            DatabaseContext = databaseContext;

            _session = databaseContext.CassandraDAO.GetSession();
            // Create some reusable prepared statements so we pay the cost of preparing once, then bind multiple times
            _createUserByUserName = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync(
                "INSERT INTO users_by_username (username, userid, password_hash, security_stamp, two_factor_enabled, access_failed_count, " +
                "lockout_enabled, lockout_end_date, phone_number, phone_number_confirmed, email, email_confirmed) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"));
            _createUserByEmail = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync(
                "INSERT INTO users_by_email (email, userid, username, password_hash, security_stamp, two_factor_enabled, access_failed_count, " +
                "lockout_enabled, lockout_end_date, phone_number, phone_number_confirmed, email_confirmed) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"));

            _deleteUserByUserName = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("DELETE FROM users_by_username WHERE username = ?"));
            _deleteUserByEmail = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("DELETE FROM users_by_email WHERE email = ?"));

            // All the statements needed by the CreateAsync method
            _createUser = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
            {
                _session.PrepareAsync("INSERT INTO users (userid, username, password_hash, security_stamp, two_factor_enabled, access_failed_count, " +
                                      "lockout_enabled, lockout_end_date, phone_number, phone_number_confirmed, email, email_confirmed) " +
                                      "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"),
                _createUserByUserName.Value,
                _createUserByEmail.Value
            }));

            // All the statements needed by the DeleteAsync method
            _deleteUser = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
            {
                _session.PrepareAsync("DELETE FROM users WHERE userid = ?"),
                _deleteUserByUserName.Value,
                _deleteUserByEmail.Value
            }));

            // All the statements needed by the UpdateAsync method
            _updateUser = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
            {
                _session.PrepareAsync("UPDATE users SET username = ?, password_hash = ?, security_stamp = ?, two_factor_enabled = ?, access_failed_count = ?, " +
                                      "lockout_enabled = ?, lockout_end_date = ?, phone_number = ?, phone_number_confirmed = ?, email = ?, email_confirmed = ? " +
                                      "WHERE userid = ?"),
                _session.PrepareAsync("UPDATE users_by_username SET password_hash = ?, security_stamp = ?, two_factor_enabled = ?, access_failed_count = ?, " +
                                      "lockout_enabled = ?, lockout_end_date = ?, phone_number = ?, phone_number_confirmed = ?, email = ?, email_confirmed = ? " +
                                      "WHERE username = ?"),
                _deleteUserByUserName.Value,
                _createUserByUserName.Value,
                _session.PrepareAsync("UPDATE users_by_email SET username = ?, password_hash = ?, security_stamp = ?, two_factor_enabled = ?, access_failed_count = ?, " +
                                      "lockout_enabled = ?, lockout_end_date = ?, phone_number = ?, phone_number_confirmed = ?, email_confirmed = ? " +
                                      "WHERE email = ?"),
                _deleteUserByEmail.Value,
                _createUserByEmail.Value
            }));

            _findById = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("SELECT * FROM users WHERE userid = ?"));
            _findByName = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("SELECT * FROM users_by_username WHERE username = ?"));
            _findByEmail = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("SELECT * FROM users_by_email WHERE email = ?"));

            _addLogin = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
            {
                _session.PrepareAsync("INSERT INTO logins (userid, login_provider, provider_key) VALUES (?, ?, ?)"),
                _session.PrepareAsync("INSERT INTO logins_by_provider (login_provider, provider_key, userid) VALUES (?, ?, ?)")
            }));
            _removeLogin = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
            {
                _session.PrepareAsync("DELETE FROM logins WHERE userId = ? and login_provider = ? and provider_key = ?"),
                _session.PrepareAsync("DELETE FROM logins_by_provider WHERE login_provider = ? AND provider_key = ?")
            }));
            _getLogins = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("SELECT * FROM logins WHERE userId = ?"));
            _getLoginsByProvider = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync(
                "SELECT * FROM logins_by_provider WHERE login_provider = ? AND provider_key = ?"));

            _getClaims = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync("SELECT * FROM claims WHERE userId = ?"));
            _addClaim = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync(
                "INSERT INTO claims (userid, type, value) VALUES (?, ?, ?)"));
            _removeClaim = new AsyncLazy<PreparedStatement>(() => _session.PrepareAsync(
                "DELETE FROM claims WHERE userId = ? AND type = ? AND value = ?"));
            // Create the schema if necessary
            if (false)
                SchemaCreationHelper.CreateSchemaIfNotExists(_session);
        }
        public void Dispose()
        {
            _disposed = true;
            _session.Dispose();
        }

        /// <summary>
		/// Attempts to remove the provided login information from the specified <paramref name="user"/>, as an asynchronous operation.
		/// and returns a flag indicating whether the removal succeed or not.
		/// </summary>
		/// <param name="user">The user to remove the login information from.</param>
		/// <param name="loginProvider">The login provide whose information should be removed.</param>
		/// <param name="providerKey">The key given by the external login provider for the specified user.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that contains a flag the result of the asynchronous removing operation. The flag will be true if
		/// the login information was existed and removed, otherwise false.
		/// </returns>
		public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            EnsureLoginsNotNull(user);

            var userGuid = user.Id;

            PreparedStatement[] prepared = await _removeLogin;
            var batch = new BatchStatement();

            // DELETE FROM logins ...
            batch.Add(prepared[0].Bind(userGuid, loginProvider, providerKey));

            // DELETE FROM logins_by_provider ...
            batch.Add(prepared[1].Bind(loginProvider, providerKey));

            await _session.ExecuteAsync(batch).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds an external <see cref="UserLoginInfo"/> to the specified <paramref name="user"/>, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to add the login to.</param>
        /// <param name="login">The external <see cref="UserLoginInfo"/> to add to the specified <paramref name="user"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public override async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));
            EnsureLoginsNotNull(user);

            var userGuid = user.Id;

            // check if login already exists for this provider and remove old details
            await RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey, cancellationToken);

            // add new login details to user object in memory and DB
            user.Logins.Add(login);

            PreparedStatement[] prepared = await _addLogin;
            var batch = new BatchStatement();

            // INSERT INTO logins ...
            batch.Add(prepared[0].Bind(userGuid, login.LoginProvider, login.ProviderKey));

            // INSERT INTO logins_by_provider ...
            batch.Add(prepared[1].Bind(login.LoginProvider, login.ProviderKey, userGuid));

            await _session.ExecuteAsync(batch).ConfigureAwait(false);

        }


        /// <summary>
        /// User userNames are distinct, and should never have two users with the same name
        /// </summary>
        /// <remarks>
        /// Can override to have different "distinct user details" implementation if necessary.
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<bool> UserDetailsAlreadyExists(TUser user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ConfigureDefaults(user);
            // if the result does exist, make sure its not for the same user object (ie same userName, but different Ids)

            var existingUser = await FindByNameAsync(Normalize(user.UserName), cancellationToken);
            return existingUser != null;
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            PreparedStatement prepared = await _getLogins;
            BoundStatement bound = prepared.Bind(user.Id);

            RowSet rows = await _session.ExecuteAsync(bound).ConfigureAwait(false);
            
            return rows.Select(row =>
            {
                var lp = row.GetValue<string>("login_provider");
                var pk = row.GetValue<string>("provider_key");
                var uli = new UserLoginInfo(lp, pk, lp);
                return uli;
            }).ToList();
        }

        public override async Task<IdentityResult> UpdateAsync(TUser user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (await UserDetailsAlreadyExists(user, cancellationToken))
                return IdentityResult.Failed(ErrorDescriber.DuplicateUserName(user.ToString()));
            ConfigureDefaults(user);

            PreparedStatement[] prepared = await _updateUser;
            var batch = new BatchStatement();

            // UPDATE users ...
            batch.Add(prepared[0].Bind(user.UserName, user.PasswordHash, user.SecurityStamp,
                user.TwoFactorEnabled, user.AccessFailedCount,
                user.LockoutEnabled, user.LockoutEnd,
                user.PhoneNumber, user.PhoneNumberConfirmed,
                user.Email, user.EmailConfirmed, user.Id));

            // See if the username changed so we can decide whether we need a different users_by_username record
            string oldUserName;
            if (user.HasUserNameChanged(out oldUserName) == false && string.IsNullOrEmpty(user.UserName) == false)
            {
                // UPDATE users_by_username ... (since username hasn't changed)
                batch.Add(prepared[1].Bind(user.PasswordHash, user.SecurityStamp, user.TwoFactorEnabled, user.AccessFailedCount,
                                           user.LockoutEnabled, user.LockoutEnd, user.PhoneNumber, user.PhoneNumberConfirmed, user.Email,
                                           user.EmailConfirmed, user.UserName));
            }
            else
            {
                // DELETE FROM users_by_username ... (delete old record since username changed)
                if (string.IsNullOrEmpty(oldUserName) == false)
                {
                    batch.Add(prepared[2].Bind(oldUserName));
                }

                // INSERT INTO users_by_username ... (insert new record since username changed)
                if (string.IsNullOrEmpty(user.UserName) == false)
                {
                    batch.Add(prepared[3].Bind(user.UserName, user.Id, user.PasswordHash, user.SecurityStamp, user.TwoFactorEnabled,
                                               user.AccessFailedCount, user.LockoutEnabled, user.LockoutEnd, user.PhoneNumber,
                                               user.PhoneNumberConfirmed, user.Email, user.EmailConfirmed));
                }
            }

            // See if the email changed so we can decide if we need a different users_by_email record
            string oldEmail;
            if (user.HasEmailChanged(out oldEmail) == false && string.IsNullOrEmpty(user.Email) == false)
            {
                // UPDATE users_by_email ... (since email hasn't changed)
                batch.Add(prepared[4].Bind(user.UserName, user.PasswordHash, user.SecurityStamp, user.TwoFactorEnabled, user.AccessFailedCount,
                                           user.LockoutEnabled, user.LockoutEnd, user.PhoneNumber, user.PhoneNumberConfirmed,
                                           user.EmailConfirmed, user.Email));
            }
            else
            {
                // DELETE FROM users_by_email ... (delete old record since email changed)
                if (string.IsNullOrEmpty(oldEmail) == false)
                {
                    batch.Add(prepared[5].Bind(oldEmail));
                }

                // INSERT INTO users_by_email ... (insert new record since email changed)
                if (string.IsNullOrEmpty(user.Email) == false)
                {
                    batch.Add(prepared[6].Bind(user.Email, user.Id, user.UserName, user.PasswordHash, user.SecurityStamp, user.TwoFactorEnabled,
                                           user.AccessFailedCount, user.LockoutEnabled, user.LockoutEnd, user.PhoneNumber,
                                           user.PhoneNumberConfirmed, user.EmailConfirmed));
                }
            }

            await _session.ExecuteAsync(batch).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        public override  async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            PreparedStatement[] prepared = await _createUser;
            var batch = new BatchStatement();

            var userGuid = user.Id;
            // INSERT INTO users ...
            batch.Add(prepared[0].Bind(
                userGuid, user.UserName,
                user.PasswordHash, user.SecurityStamp,
                user.TwoFactorEnabled, user.AccessFailedCount,
                user.LockoutEnabled, user.LockoutEnd,
                user.PhoneNumber, user.PhoneNumberConfirmed, user.Email,
                user.EmailConfirmed));

            // Only insert into username and email tables if those have a value
            if (string.IsNullOrEmpty(user.UserName) == false)
            {
                // INSERT INTO users_by_username ...
                batch.Add(prepared[1].Bind(user.UserName, userGuid, user.PasswordHash, user.SecurityStamp,
                    user.TwoFactorEnabled, user.AccessFailedCount,
                    user.LockoutEnabled, user.LockoutEnd,
                    user.PhoneNumber, user.PhoneNumberConfirmed, user.Email,
                    user.EmailConfirmed));
            }

            if (string.IsNullOrEmpty(user.Email) == false)
            {
                // INSERT INTO users_by_email ...
                batch.Add(prepared[2].Bind(user.Email, userGuid, user.UserName, user.PasswordHash,
                    user.SecurityStamp, user.TwoFactorEnabled,
                    user.AccessFailedCount, user.LockoutEnabled,
                    user.LockoutEnd, user.PhoneNumber,
                    user.PhoneNumberConfirmed, user.EmailConfirmed));
            }

            await _session.ExecuteAsync(batch).ConfigureAwait(false);
            return IdentityResult.Success;
        }
        /// <summary>
        /// Gets the user, if any, associated with the specified, normalized email address, as an asynchronous operation.
        /// </summary>
        /// <param name="normalizedEmail">The normalized email address to return the user for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>
        /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified normalized email address.
        /// </returns>
        public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            PreparedStatement prepared = await _findByEmail;
            BoundStatement bound = prepared.Bind(normalizedEmail);

            RowSet rows = await _session.ExecuteAsync(bound).ConfigureAwait(false);
            var row = rows.SingleOrDefault();
            var user = FromRow(row);
            return user;
        }

        public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                ThrowIfDisposed();

                Guid guid = Guid.Parse(userId);
                var cguid = ToCqlString(guid);
                PreparedStatement prepared = await _findById;
                BoundStatement bound = prepared.Bind(guid);

                RowSet rows = await _session.ExecuteAsync(bound).ConfigureAwait(false);
                var row = rows.SingleOrDefault();
                var user = FromRow(row);
                return user;

            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(normalizedUserName))
                return await Task.FromResult((TUser)null);

            PreparedStatement prepared = await _findByName;
            BoundStatement bound = prepared.Bind(normalizedUserName);

            RowSet rows = await _session.ExecuteAsync(bound).ConfigureAwait(false);
            var row = rows.SingleOrDefault();
            var user = FromRow(row);
            return user;
        }

        public override async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
         CancellationToken cancellationToken = default(CancellationToken))
        {
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();


            PreparedStatement prepared = await _getLoginsByProvider;
            BoundStatement bound = prepared.Bind(loginProvider, providerKey);

            RowSet loginRows = await _session.ExecuteAsync(bound).ConfigureAwait(false);
            Row loginResult = loginRows.FirstOrDefault();
            if (loginResult == null)
                return null;

            var user = await FindByIdAsync(loginResult.GetValue<Guid>("userid").ToString(),
                cancellationToken);
            return user;
        }
        
        private static TUser FromRow(Row row)
        {
            if (row == null)
                return null;
            var offset = row.GetValue<DateTimeOffset?>("lockout_end_date");

            var user = new TUser
            {
                AccessFailedCount = row.GetValue<int>("access_failed_count"),
                Email = row.GetValue<string>("email"),
                EmailConfirmed = row.GetValue<bool>("email_confirmed"),
                Id = row.GetValue<Guid>("userid"),
                LockoutEnabled = row.GetValue<bool>("lockout_enabled"),
                LockoutEnd = row.GetValue<DateTimeOffset?>("lockout_end_date"),
                PasswordHash = row.GetValue<string>("password_hash"),
                PhoneNumber = row.GetValue<string>("phone_number"),
                PhoneNumberConfirmed = row.GetValue<bool>("phone_number_confirmed"),
                SecurityStamp = row.GetValue<string>("security_stamp"),
                TwoFactorEnabled = row.GetValue<bool>("two_factor_enabled"),
                UserName = row.GetValue<string>("username")
            };
            user.MakeOriginal();
            return user;
        }

        protected override string ConvertIdToString(Guid id)
        {
            if (id == null || id.Equals(default(TKey)))
            {
                return null;
            }
            return id.ToString();
        }
    }
    public abstract class UserStoreCommon<TUser, TRole, TKey> :
        IUserLoginStore<TUser>,
        IUserRoleStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserTwoFactorStore<TUser>
        where TUser : IdentityUser, new()
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {


        public UserStoreCommon( 
            ILookupNormalizer normalizer = null,
            IdentityErrorDescriber describer = null)
        {
            Normalizer = normalizer;
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
        }

        protected ILookupNormalizer Normalizer { get; set; }
        /// <summary>
        /// Used to generate public API error messages
        /// </summary>
        protected IdentityErrorDescriber ErrorDescriber { get; set; }


        /// <summary>
        /// Gets the user identifier for the specified <paramref name="user"/>, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose identifier should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the identifier for the specified <paramref name="user"/>.</returns>
        public Task<string> GetUserIdAsync(TUser user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(ConvertIdToString(user.Id));
        }

        public abstract Task<IdentityResult> CreateAsync(TUser user,
            CancellationToken cancellationToken);

        /// <summary>
        /// Configure any default settings for the user (Default fills in missing NormalizedEmail and NormalizedUserName from Email and UserName)
        /// </summary>
        /// <returns></returns>
        protected virtual void ConfigureDefaults(TUser user)
        {
            if (string.IsNullOrWhiteSpace(user.NormalizedUserName) || !user.NormalizedUserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase)) user.NormalizedUserName = Normalize(user.UserName);
            if (string.IsNullOrWhiteSpace(user.NormalizedEmail) || !user.NormalizedEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase)) user.NormalizedEmail = Normalize(user.Email);
        }
        /// <summary>
        /// Gets the user name for the specified <paramref name="user"/>, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose name should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the name for the specified <paramref name="user"/>.</returns>
        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.UserName);
        }

        /// <summary>
		/// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose name should be set.</param>
		/// <param name="userName">The user name to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.UserName = userName;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Gets the normalized user name for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose normalized name should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the normalized user name for the specified <paramref name="user"/>.</returns>
		public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.NormalizedUserName ?? Normalize(user.UserName));
        }

        /// <summary>
		/// Sets the given normalized name for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose name should be set.</param>
		/// <param name="normalizedUserName">The normalized name to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetNormalizedUserNameAsync(TUser user, string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.NormalizedUserName = Normalize(normalizedUserName);
            return Task.FromResult(0);
        }

        public abstract Task<IdentityResult> UpdateAsync(TUser user,
            CancellationToken cancellationToken);

        public abstract Task<IdentityResult> DeleteAsync(TUser user,
            CancellationToken cancellationToken);

        public abstract Task<TUser> FindByIdAsync(string userId,
            CancellationToken cancellationToken);

        public virtual Task<TUser> FindByNameAsync(string normalizedUserName,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public abstract Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken);

        public abstract Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the associated logins for the specified <param ref="user"/>, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose associated logins to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>
        /// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="UserLoginInfo"/> for the specified <paramref name="user"/>, if any.
        /// </returns>
        public abstract Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user,
            CancellationToken cancellationToken);

        public abstract Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken);

        public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
		/// Gets a list of role names the specified <paramref name="user"/> belongs to, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose role names to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a list of role names.</returns>
		public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            EnsureRolesNotNull(user);

            IList<string> roleNames = user.Roles.Select(r => r.Name).ToList();
            return Task.FromResult(roleNames);
        }


        /// <summary>
        /// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the give named role, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose role membership should be checked.</param>
        /// <param name="normalizedRoleName">The normalized name of the role to be checked.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <see cref="user"/> is
        /// a member of the named role.
        /// </returns>
        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));
            EnsureRolesNotNull(user);

            return Task.FromResult(user.Roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a list of all (ie both User.Claims and User.Roles.Claims) <see cref="Claim"/>s to be belonging to the specified <paramref name="user"/> as an asynchronous operation.
        /// </summary>
        /// <param name="user">The role whose claims to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a list of <see cref="Claim"/>s.
        /// </returns>
        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            EnsureClaimsNotNull(user);
            EnsureRolesNotNull(user);

            IList<Claim> result = user.AllClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(System.Security.Claims.Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(TUser user, System.Security.Claims.Claim claim, System.Security.Claims.Claim newClaim,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task AddClaimsAsync(TUser user, IEnumerable claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
		/// Sets a flag indicating whether the specified <paramref name="user "/>has two factor authentication enabled or not,
		/// as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose two factor authentication enabled status should be set.</param>
		/// <param name="enabled">A flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Returns a flag indicating whether the specified <paramref name="user "/>has two factor authentication enabled or not,
		/// as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose two factor authentication enabled status should be set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified 
		/// <paramref name="user "/>has two factor authentication enabled or not.
		/// </returns>
		public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.TwoFactorEnabled);
        }

        /// <summary>
		/// Sets the password hash for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose password hash to set.</param>
		/// <param name="passwordHash">The password hash to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Gets the password hash for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose password hash to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning the password hash for the specified <paramref name="user"/>.</returns>
		public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
		/// Gets a flag indicating whether the specified <paramref name="user"/> has a password, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to return a flag for, indicating whether they have a password or not.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified <paramref name="user"/> has a password
		/// otherwise false.
		/// </returns>
		public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }
        #region IUserSecurityStampStore<TUser>

        /// <summary>
        /// Sets the provided security <paramref name="stamp"/> for the specified <paramref name="user"/>, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose security stamp should be set.</param>
        /// <param name="stamp">The security stamp to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Get the security stamp for the specified <paramref name="user" />, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose security stamp should be set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }

        #endregion
        /// <summary>
		/// Sets the <paramref name="email"/> address for a <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email should be set.</param>
		/// <param name="email">The email to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.Email = email;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Gets the email address for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object containing the results of the asynchronous operation, the email address for the specified <paramref name="user"/>.</returns>
		public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Email);
        }

        /// <summary>
		/// Gets a flag indicating whether the email address for the specified <paramref name="user"/> has been verified, true if the email address is verified otherwise
		/// false, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email confirmation status should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The task object containing the results of the asynchronous operation, a flag indicating whether the email address for the specified <paramref name="user"/>
		/// has been confirmed or not.
		/// </returns>
		public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.EmailConfirmed);
        }

        /// <summary>
		/// Sets the flag indicating whether the specified <paramref name="user"/>'s email address has been confirmed or not, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email confirmation status should be set.</param>
		/// <param name="confirmed">A flag indicating if the email address has been confirmed, true if the address is confirmed otherwise false.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public abstract Task<TUser> FindByEmailAsync(string normalizedEmail,
            CancellationToken cancellationToken);

        /// <summary>
		/// Returns the normalized email for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email address to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The task object containing the results of the asynchronous lookup operation, the normalized email address if any associated with the specified user.
		/// </returns>
		public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedEmail ?? Normalize(user.Email));

        }

        /// <summary>
		/// Sets the normalized email for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email address to set.</param>
		/// <param name="normalizedEmail">The normalized email to set for the specified <paramref name="user"/>.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.NormalizedEmail = Normalize(normalizedEmail);
            return Task.FromResult(0);
        }

        /// <summary>
		/// Gets the last <see cref="DateTimeOffset"/> a user's last lockout expired, if any, as an asynchronous operation.
		/// Any time in the past should be indicates a user is not locked out.
		/// </summary>
		/// <param name="user">The user whose lockout date should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a <see cref="DateTimeOffset"/> containing the last time
		/// a user's lockout expired, if any.
		/// </returns>
		public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEnd);
        }

        /// <summary>
		/// Locks out a user until the specified end date has passed, as an asynchronous operation. Setting a end date in the past immediately unlocks a user.
		/// </summary>
		/// <param name="user">The user whose lockout date should be set.</param>
		/// <param name="lockoutEnd">The <see cref="DateTimeOffset"/> after which the <paramref name="user"/>'s lockout should end.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.LockoutEnd = lockoutEnd;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Records that a failed access has occurred, incrementing the failed access count, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose cancellation count should be incremented.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the incremented failed access count.</returns>
		public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
		/// Resets a user's failed access count, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose failed access count should be reset.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		/// <remarks>This is typically called after the account is successfully accessed.</remarks>
		public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Retrieves the current failed access count for the specified <paramref name="user"/>, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose failed access count should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the failed access count.</returns>
		public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
		/// Set the flag indicating if the specified <paramref name="user"/> can be locked out, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose ability to be locked out should be set.</param>
		/// <param name="enabled">A flag indicating if lock out can be enabled for the specified <paramref name="user"/>.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>
		/// Set the flag indicating if the specified <paramref name="user"/> can be locked out, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose ability to be locked out should be set.</param>
		/// <param name="enabled">A flag indicating if lock out can be enabled for the specified <paramref name="user"/>.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Sets the telephone number for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose telephone number should be set.</param>
		/// <param name="phoneNumber">The telephone number to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        /// <summary>
		/// Gets the telephone number, if any, for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose telephone number should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telephone number, if any.</returns>
		public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        /// <summary>
		/// Gets a flag indicating whether the specified <paramref name="user"/>'s telephone number has been confirmed, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to return a flag for, indicating whether their telephone number is confirmed.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified <paramref name="user"/> has a confirmed
		/// telephone number otherwise false.
		/// </returns>
		public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        /// <summary>
		/// Sets a flag indicating if the specified <paramref name="user"/>'s phone number has been confirmed, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose telephone number confirmation status should be set.</param>
		/// <param name="confirmed">A flag indicating whether the user's telephone number has been confirmed.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public IQueryable<TUser> Users { get; }

        #region IDisposable

        protected bool _disposed = false; // To detect redundant calls


        public virtual void Dispose() { }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        protected virtual void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion

        protected static TKey ConvertIdFromString(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return default(TKey);
            }
            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }
        protected virtual string ConvertIdToString(Guid id)
        {
            if (id == null || id.Equals(default(TKey)))
            {
                return null;
            }
            return id.ToString();
        }

        protected static string ToCqlString(Guid guid)
        {
            var bytes = guid.ToByteArray();
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < 16; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    sb.Append("-");
                }
                var b = bytes[i];
                sb.AppendFormat("{0:x2}", b);

            }
            return sb.ToString();
        }
        /// <summary>
        /// Used to ensure consistent formatting of normalized string values. Uses the ILookupNormalizer if its supplied, otherwise converts strings to lowercase and trims;
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected virtual string Normalize(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return Normalizer == null ? str.ToLower().Trim() : Normalizer.Normalize(str);
        }
        protected virtual void EnsureLoginsNotNull(TUser user)
        {
            if (user.Logins == null) user.Logins = new List<UserLoginInfo>();
        }

        protected virtual void EnsureClaimsNotNull(TUser user)
        {
            if (user.Claims == null) user.Claims = new List<IdentityClaim>();
        }

        protected virtual void EnsureRolesNotNull(TUser user)
        {
            if (user.Roles == null)
                user.Roles = new List<IdentityRole<Guid>>();

               // user.Roles = new List<TRole>().Cast<IdentityRole<TKey>>().ToList();
        }

    }
}
