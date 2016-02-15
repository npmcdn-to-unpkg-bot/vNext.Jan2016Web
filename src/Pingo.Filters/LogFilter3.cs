using System;
using Microsoft.AspNet.Mvc.Filters;

namespace Pingo.Filters
{
    public class LogFilter3 : ActionFilterAttribute
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