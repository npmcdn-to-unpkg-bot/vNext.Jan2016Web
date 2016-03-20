#define ENTITY_IDENTITY
#undef ENTITY_IDENTITY

using System;
using Microsoft.Data.Entity;
using Microsoft.Extensions.OptionsModel;
using p6.CassandraStore.Settings;
using P6.AspNet.CassandraIdentity3;

namespace Pingo.Authorization.Models
{
#if ENTITY_IDENTITY
    public class ApplicationDbContext : Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
#else
    public class ApplicationDbContext : IdentityCassandraContext<ApplicationUser,CassandraIdentityRole, Guid>
    {
        public ApplicationDbContext(IOptions<CassandraConfig> options) : base(options.Value)
        {
        }
    }
#endif
}
