using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Pingo.Filters.Attributes;

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
                var scopeAttribute = (ScopeAttribute)context.Controller.GetType().GetTypeInfo().GetCustomAttribute(typeof(ScopeAttribute));

                var result = from claim in context.HttpContext.User.Claims
                    where claim.Type == "scope"
                    select claim;
                if (!result.Any())
                {
                    context.Result = new HttpStatusCodeResult((int) System.Net.HttpStatusCode.Forbidden);
                    return;
                }
                var scopeClaim = result.First();
                if (!scopeAttribute.Values.Contains(scopeClaim.Value))
                {
                    context.Result = new HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}