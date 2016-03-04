using Microsoft.Data.Entity.Infrastructure;

namespace Pingo.Core.Startup
{
    public interface IConfigureEntityFrameworkRegistrant
    {
        void OnAddDbContext(EntityFrameworkServicesBuilder builder);
    }
}