using Microsoft.Extensions.DependencyInjection;

namespace Pingo.Core.Startup
{
    interface IConfigureServicesRegistrant
    {
        void OnConfigureServices(IServiceCollection serviceCollection);
    }
}