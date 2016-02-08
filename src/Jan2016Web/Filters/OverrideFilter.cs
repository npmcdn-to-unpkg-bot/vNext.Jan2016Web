using System;
using Microsoft.AspNet.Mvc.Filters;

namespace Jan2016Web.Filters
{
    public class OverrideFilter : ActionFilterAttribute
    {
        public Type Type { get; set; }
    }
}