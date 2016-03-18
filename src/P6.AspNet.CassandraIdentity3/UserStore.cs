using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.AspNet.Identity;
using p6.AspNet.Identity3.Common;

namespace P6.AspNet.CassandraIdentity3
{
    
    public class UserStore<TUser, TRole> : UserStore<TUser, TRole, string>
    where TUser : IdentityUser<string>,new()
    where TRole : IdentityRole<string>
    {
        public UserStore(IIdentityCassandraContext<TUser, TRole, string> databaseContext, ILookupNormalizer normalizer = null, IdentityErrorDescriber describer = null) : base(databaseContext, normalizer, describer) { }
    }

    public class UserStore<TUser, TRole, TKey> :
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
        where TUser : IdentityUser<TKey>,new()
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly bool _disposeOfSession = false;
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

        public UserStore(IIdentityCassandraContext<TUser, TRole, TKey> databaseContext,
            ILookupNormalizer normalizer = null,
            IdentityErrorDescriber describer = null)
        {
            if (databaseContext == null)
            {
                throw new ArgumentNullException(nameof(databaseContext));
            }

            DatabaseContext = databaseContext;
            Normalizer = normalizer;
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
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
        protected static TKey ConvertIdFromString(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return default(TKey);
            }
            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }
        protected IIdentityCassandraContext<TUser, TRole, TKey> DatabaseContext { get; set; }

        protected ILookupNormalizer Normalizer { get; set; }
        /// <summary>
        /// Used to generate public API error messages
        /// </summary>
        protected IdentityErrorDescriber ErrorDescriber { get; set; }
        public void Dispose()
        {
            if (_disposeOfSession)
                _session.Dispose();
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            PreparedStatement prepared = await _findById;
            BoundStatement bound = prepared.Bind(userId);

            RowSet rows = await _session.ExecuteAsync(bound).ConfigureAwait(false);
            var row = rows.SingleOrDefault();
            var user = FromRow(row);
            return user;
        }

        private static TUser FromRow(Row row)
        {
            if (row == null)
                return null;
            return new TUser
            {
                AccessFailedCount = row.GetValue<int>("access_failed_count"),
                Email = row.GetValue<string>("email"),
                EmailConfirmed = row.GetValue<bool>("email_confirmed"),
                Id = ConvertIdFromString(row.GetValue<Guid>("userid").ToString()),
                LockoutEnabled = row.GetValue<bool>("lockout_enabled"),
                LockoutEnd = row.GetValue<DateTimeOffset>("lockout_end_date"),
                PasswordHash = row.GetValue<string>("password_hash"),
                PhoneNumber = row.GetValue<string>("phone_number"),
                PhoneNumberConfirmed = row.GetValue<bool>("phone_number_confirmed"),
                SecurityStamp = row.GetValue<string>("security_stamp"),
                TwoFactorEnabled = row.GetValue<bool>("two_factor_enabled"),
                UserName = row.GetValue<string>("username"),
            };
        }

        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(System.Security.Claims.Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(TUser user, System.Security.Claims.Claim claim, System.Security.Claims.Claim newClaim,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddClaimsAsync(TUser user, IEnumerable claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TUser> Users { get; }
    }
}
