using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Pingo.Core
{
    public class Global
    {
        public Global(IConfigurationRoot configurationRoot,IMemoryCache memoryCache)
        {
            ConfigurationRoot = configurationRoot;
            MemoryCache = memoryCache;
        }
        public IMemoryCache MemoryCache { get; private set; }
        public IConfigurationRoot ConfigurationRoot { get; private set; }
    }
}
