using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Filters;

namespace Pingo.Filters
{
    public class AntiForgeryActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
              
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}