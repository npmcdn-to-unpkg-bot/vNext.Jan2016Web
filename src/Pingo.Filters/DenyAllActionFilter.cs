using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace Pingo.Filters
{
    public class DenyAllActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.Result = new HttpUnauthorizedResult();
            base.OnActionExecuting(context);
        }
    }
}