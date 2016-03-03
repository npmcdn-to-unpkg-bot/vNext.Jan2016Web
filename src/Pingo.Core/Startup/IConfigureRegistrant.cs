using Microsoft.AspNet.Builder;

namespace Pingo.Core.Startup
{
    public interface IConfigureRegistrant
    {
        void OnConfigure(IApplicationBuilder app);
    }
}