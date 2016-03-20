#define ENTITY_IDENTITY
#undef ENTITY_IDENTITY
namespace Pingo.Authorization.Models
{
#if ENTITY_IDENTITY
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : Microsoft.AspNet.Identity.EntityFramework.IdentityUser
    {
    }
#else 
    public class ApplicationUser : p6.AspNet.Identity3.Common.IdentityUser
    {
    }
    public class CassandraIdentityRole : p6.AspNet.Identity3.Common.IdentityRole
    {
    }
#endif
}
