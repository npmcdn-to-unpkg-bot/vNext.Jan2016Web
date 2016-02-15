using System.Collections.Generic;

namespace Pingo.Core.Settings
{
    public class ControllerNode
    {
        public string Filter { get; set; }
        public string Controller { get; set; }
        public List<ActionNode> Actions { get; set; }
    }
}