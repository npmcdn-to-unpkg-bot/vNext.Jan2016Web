using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using p6.AspNet.Identity3.Common;

namespace P6.AspNet.CassandraIdentity3
{
    public class RoleStore<TUser, TRole> : RoleStore<TUser, TRole, string>
    where TUser : IdentityUser<string>
    where TRole : IdentityRole<string>
    {
        public RoleStore(IIdentityCassandraContext<TUser, TRole, string> databaseContext, 
            ILookupNormalizer normalizer = null, IdentityErrorDescriber describer = null) : base(databaseContext, normalizer, describer) { }
    }

    public class RoleStore<TUser, TRole, TKey> :
        IQueryableRoleStore<TRole>,
        IRoleClaimStore<TRole>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public RoleStore(IIdentityCassandraContext<TUser, TRole, TKey> databaseContext, ILookupNormalizer normalizer = null, IdentityErrorDescriber describer = null)
        {
            if (databaseContext == null) throw new ArgumentNullException(nameof(databaseContext));

            DatabaseContext = databaseContext;
            Normalizer = normalizer;
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
        }

        protected IIdentityCassandraContext<TUser, TRole, TKey> DatabaseContext { get; set; }

        protected ILookupNormalizer Normalizer { get; set; }
        /// <summary>
		/// Used to generate public API error messages
		/// </summary>
		protected IdentityErrorDescriber ErrorDescriber { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task GetClaimsAsync(TRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(TRole role, System.Security.Claims.Claim claim,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task AddClaimAsync(TRole role, System.Security.Claims.Claim claim,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        Task<IList<Claim>> IRoleClaimStore<TRole>.GetClaimsAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TRole> Roles { get; }
  
    }
}
