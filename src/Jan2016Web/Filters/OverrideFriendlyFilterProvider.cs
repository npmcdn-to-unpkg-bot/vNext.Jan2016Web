using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Framework.DependencyInjection;

namespace Jan2016Web.Filters
{
    public class OverrideFriendlyFilterProvider : IFilterProvider
    { 
        IOptions<FiltersConfig> _settings;

        public OverrideFriendlyFilterProvider(IOptions<FiltersConfig> settings)
        {
            _settings = settings;
        }

        bool RequiresAuth(FilterProviderContext context)
        {
            ControllerActionDescriptor cad = (ControllerActionDescriptor)context.ActionContext.ActionDescriptor;
            string area = (string) context.ActionContext.RouteData.Values["area"];
            
            // Authorization
            if (!string.IsNullOrEmpty(area))
            {
                if (_settings.Value.Authorization.OptOut.AreasMap != null && _settings.Value.Authorization.OptOut.AreasMap.ContainsKey(area))
                {
                    // got a hit, this action will not get an Authorization Filter.
                    System.Diagnostics.Debug.WriteLine("AreasMap: got a hit, this action will not get an Authorization Filter. ");
                    return false;
                }
            }
            if (_settings.Value.Authorization.OptOut.ControllersMap != null && _settings.Value.Authorization.OptOut.ControllersMap.ContainsKey(cad.ControllerTypeInfo.FullName))
            {
                // got a hit, this action will not get an Authorization Filter.
                System.Diagnostics.Debug.WriteLine("ControllersMap: got a hit, this action will not get an Authorization Filter. ");
                return false;
            }
            if (_settings.Value.Authorization.OptOut.ActionsMap != null && _settings.Value.Authorization.OptOut.ActionsMap.ContainsKey(cad.DisplayName))
            {
                // got a hit, this action will not get an Authorization Filter.
                System.Diagnostics.Debug.WriteLine("ActionsMap: got a hit, this action will not get an Authorization Filter. ");
                return false;
            }
            return true;
        }
        //all framework providers have negative orders, so ours will come later
        public void OnProvidersExecuting(FilterProviderContext context)
        {
            ControllerActionDescriptor cad = (ControllerActionDescriptor) context.ActionContext.ActionDescriptor;
            System.Diagnostics.Debug.WriteLine("Controller: " + cad.ControllerTypeInfo.FullName);
            System.Diagnostics.Debug.WriteLine("ActionName: " + cad.Name);
            System.Diagnostics.Debug.WriteLine("DisplayName: " + cad.DisplayName);
            System.Diagnostics.Debug.WriteLine("Area: " + context.ActionContext.RouteData.Values["area"]);

            var requiresAuth = RequiresAuth(context);



        }

        public void OnProvidersExecuted(FilterProviderContext context)
        {
        }

        public int Order => 0;

        public  void Invoke(FilterProviderContext context, Action callNext)
        {
            //let the whole provider pipeline run first
            if (callNext != null)
            {
                callNext();
            }

            if (context.ActionContext.ActionDescriptor.FilterDescriptors != null)
            {
                var overrideFilters = context.Results.Where(filterItem => filterItem.Filter is OverrideFilter).ToArray();
                foreach (var overrideFilter in overrideFilters)
                {
//                    context.Results.RemoveAll(filterItem =>
//                    filterItem.Descriptor.Filter.GetType() == ((OverrideFilter)overrideFilter.Filter).Type &&
//                    filterItem.Descriptor.Scope <= overrideFilter.Descriptor.Scope);
                }
            }
        }
    }
}
