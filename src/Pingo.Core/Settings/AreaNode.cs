using System.Collections.Generic;

namespace Pingo.Core.Settings
{
    public class AreaNode
    {
        public string Area { get; set; }
        public List<ControllerNode> Controllers { get; set; }
    }
}