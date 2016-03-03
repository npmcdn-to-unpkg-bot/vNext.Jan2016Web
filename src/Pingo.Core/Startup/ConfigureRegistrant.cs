using Microsoft.AspNet.Builder;

namespace Pingo.Core.Startup
{
    public abstract class ConfigureRegistrant : IConfigureRegistrant
    {
        public abstract void OnConfigure(IApplicationBuilder app);
    }
}