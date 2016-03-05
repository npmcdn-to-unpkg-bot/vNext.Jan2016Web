using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Claims;

namespace Pingo.Filters
{
    public class AuthActionFilter : ActionFilterAttribute
    {
        public static string Area { get; set; }
        public static string Controller { get; set; }
        public static string Action { get; set; }
        private static IConfigurationRoot _configurationRoot;
        public AuthActionFilter(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
            Area = _configurationRoot["Filters:Configuration:AuthActionFilter:Area"];
            Controller = _configurationRoot["Filters:Configuration:AuthActionFilter:Controller"];
            Action = _configurationRoot["Filters:Configuration:AuthActionFilter:Action"];
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                var rtrr = new RouteValueDictionary(
                    new
                    {
                        action = Action,
                        controller = Controller,
                        area = Area,
                        ReturnUrl = context.HttpContext.Request.Path
                    });

                context.Result = new RedirectToRouteResult(rtrr);
            }
            else
            {
                var result = from claim in context.HttpContext.User.Claims
                             where claim.Type == ClaimTypes.NameIdentifier
                             select claim;
                if (!result.Any())
                {
                    context.Result = new HttpUnauthorizedResult();
                }
            }

            base.OnActionExecuting(context);
        }
    }
}