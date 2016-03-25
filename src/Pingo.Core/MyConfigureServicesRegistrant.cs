using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pingo.Core.Settings;
using Pingo.Core.Startup;

namespace Pingo.Core
{
    public class MyConfigureServicesRegistrant : ConfigureServicesRegistrant
    {
        public override void OnConfigureServices(IServiceCollection services)
        {
            services.Configure<FiltersConfig>(Configuration.GetSection(FiltersConfig.WellKnown_SectionName));

        }

        public MyConfigureServicesRegistrant(IConfigurationRoot configuration) : base(configuration)
        {
        }
    }
}
