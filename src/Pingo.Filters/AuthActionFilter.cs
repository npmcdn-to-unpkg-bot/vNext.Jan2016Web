using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace Pingo.Filters
{
    public class AuthActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result =  new ChallengeResult();
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