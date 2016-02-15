using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Filters;

namespace Jan2016Web.Filters
{
    public class LogFilter : ActionFilterAttribute
    {
        /*
        private readonly ILog logger;

        public LogFilter(ILog logger)
        {
            this.logger = logger;
        }
*/
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            Console.WriteLine(actionContext.HttpContext.Request);
            //            this.logger.Log(actionContext.HttpContext.Request);
        }
    }
}
