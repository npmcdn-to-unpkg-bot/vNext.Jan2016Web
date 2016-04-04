using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Configuration;

namespace Pingo.Filters
{
    public class AuthApiActionFilter : ActionFilterAttribute
    {
        public static string Area { get; set; }
        public static string Controller { get; set; }
        public static string Action { get; set; }
        private static IConfigurationRoot _configurationRoot;
        public AuthApiActionFilter(IConfigurationRoot configurationRoot)
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
                context.Result = new HttpUnauthorizedResult();
            }
            else
            {
                var result = from claim in context.HttpContext.User.Claims
                    where claim.Type == ClaimTypes.NameIdentifier
                    select claim;
                if (!result.Any())
                {
                    context.Result = new HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}