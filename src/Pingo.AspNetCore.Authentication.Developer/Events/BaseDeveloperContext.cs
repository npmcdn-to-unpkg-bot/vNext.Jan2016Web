using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;

namespace Pingo.AspNetCore.Authentication.Developer
{
    /// <summary>
    /// Base class for other Developer contexts.
    /// </summary>
    public class BaseDeveloperContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="BaseDeveloperContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="options">The options for Twitter</param>
        public BaseDeveloperContext(HttpContext context, DeveloperOptions options)
            : base(context)
        {
            Options = options;
        }

        public DeveloperOptions Options { get; }
    }
}
