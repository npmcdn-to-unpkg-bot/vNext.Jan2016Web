using System;
using Pingo.Core.Attributes;

namespace Pingo.Filters.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ScopeAttribute : StringArrayAttribute
    {
        public ScopeAttribute(string[] tags) : base(tags)
        {
        }
    }
}
