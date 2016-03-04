using Microsoft.Data.Entity.Infrastructure;

namespace Pingo.Core.Startup
{
    public abstract class ConfigureEntityFrameworkRegistrant : IConfigureEntityFrameworkRegistrant
    {
        public abstract void OnAddDbContext(EntityFrameworkServicesBuilder builder);
    }
}