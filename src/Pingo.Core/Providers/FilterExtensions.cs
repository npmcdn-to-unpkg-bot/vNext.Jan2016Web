﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using Pingo.Core.Settings;

namespace Pingo.Core.Providers
{
    public static class FilterExtensions
    {
        public static bool ContainsMatch(this List<AreaNode> routeTree, FilterProviderContext context)
        {
            ControllerActionDescriptor cad = (ControllerActionDescriptor)context.ActionContext.ActionDescriptor;
            string area = (string)context.ActionContext.RouteData.Values["area"];

            var areaNode = routeTree.Find(x =>
            {
                if (string.IsNullOrEmpty(x.Area) && string.IsNullOrEmpty(area))
                    return true;
                return String.Compare(area, x.Area, StringComparison.OrdinalIgnoreCase) == 0;

            });

            if (areaNode != null)
            {
                if (areaNode.Controllers == null || !areaNode.Controllers.Any())
                    return true;

                var controllerNode = areaNode.Controllers.Find(x => String.Compare(cad.ControllerName, x.Controller, StringComparison.OrdinalIgnoreCase) == 0);
                if (controllerNode != null)
                {
                    if (controllerNode.Actions == null || !controllerNode.Actions.Any())
                        return true;

                    var action = controllerNode.Actions.Find(x => String.Compare(cad.Name, x.Action, StringComparison.OrdinalIgnoreCase) == 0);
                    return action != null;
                }
            }
            return false;
        }
    }
}
