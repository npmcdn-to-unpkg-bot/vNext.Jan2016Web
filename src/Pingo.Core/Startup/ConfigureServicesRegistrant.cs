using Microsoft.Extensions.DependencyInjection;

namespace Pingo.Core.Startup
{
    public abstract class ConfigureServicesRegistrant: IConfigureServicesRegistrant
    {
        public abstract void OnConfigureServices(IServiceCollection serviceCollection);
    }
}
