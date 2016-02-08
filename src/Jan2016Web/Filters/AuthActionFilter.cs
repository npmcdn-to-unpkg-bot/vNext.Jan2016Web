using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Routing;

namespace Jan2016Web.Filters
{
    public class AuthActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {

                var rtrr = new RouteValueDictionary(
                    new
                    {
                        action = "Login",
                        controller = "Account",
                        area = "Identity",
                        ReturnUrl = context.HttpContext.Request.Path
                    });

                context.Result = new RedirectToRouteResult(rtrr);

            }
            base.OnActionExecuting(context);
        }
    }
}
