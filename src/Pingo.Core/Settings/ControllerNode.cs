using System.Collections.Generic;

namespace Pingo.Core.Settings
{
    public class ControllerNode
    {
        public string Controller { get; set; }
        public List<ActionNode> Actions { get; set; }
    }
}