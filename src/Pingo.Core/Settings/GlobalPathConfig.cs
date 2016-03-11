using System.Collections.Generic;

namespace Pingo.Core.Settings
{
    public class GlobalPathConfig
    {
        public List<GlobalPathRecord> OptOut { get; set; }
        public List<GlobalPathRecord> OptIn { get; set; }
    }
}