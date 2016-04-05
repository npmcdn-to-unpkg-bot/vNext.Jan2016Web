using System;
using System.Collections.Generic;

namespace Pingo.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TaggerAttribute: StringArrayAttribute
    {
        public TaggerAttribute(string[] tags) : base(tags)
        {
        }
    }

}
