using System.Collections.Generic;

namespace Pingo.Core.Settings
{
    public class SimpleManyConfig
    {
        public List<SimpleManyRecord> OptOut { get; set; }
        public List<SimpleManyRecord> OptIn { get; set; }
    }
}