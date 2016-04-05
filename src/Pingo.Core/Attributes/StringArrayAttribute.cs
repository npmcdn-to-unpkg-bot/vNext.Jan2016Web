using System;

namespace Pingo.Core.Attributes
{
    public abstract class StringArrayAttribute : System.Attribute
    {
        public string[] Values { get; set; }
        public StringArrayAttribute(string[] values)
        {
            Values = values;
        }
    }
}