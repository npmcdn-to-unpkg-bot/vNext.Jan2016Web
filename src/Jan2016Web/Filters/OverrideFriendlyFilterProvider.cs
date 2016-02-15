using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jan2016Web.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.OptionsModel;
using Pingo.Core.Settings;
using Pingo.Filters.Settings;

namespace Jan2016Web.Filters
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
    public class OverrideFriendlyFilterProvider : IFilterProvider
    { 
        IOptions<FiltersConfig> _settings;
        private IServiceProvider _serviceProvider;
        private static readonly object locker = new object();
        private static Dictionary<string, List<FilterItem>> ActionFilterMap = new Dictionary<string, List<FilterItem>>();
        private static Dictionary<string, FilterItem> TypeToFilterItem = new Dictionary<string, FilterItem>();
        private static bool _frontLoaded;
        public OverrideFriendlyFilterProvider(IServiceProvider serviceProvider,IOptions<FiltersConfig> settings)
        {
            _settings = settings;
            _serviceProvider = serviceProvider;
        }

        public static void Configure(IConfigurationRoot configurationRoot)
        {
            FrontLoadFilterItems(configurationRoot);
        }

        private static void FrontLoadFilterItems(IConfigurationRoot configurationRoot)
        {
            /*

            var filterItem = CreateFilterItem(settings.Value.Authorization.Filter);
            TypeToFilterItem.Add(settings.Value.Authorization.Filter, filterItem);

            if (settings.Value.SimpleMany != null)
            {
                if (settings.Value.SimpleMany.OptOut != null)
                {
                    foreach (var record in settings.Value.SimpleMany.OptOut)
                    {
                        filterItem = CreateFilterItem(record.Filter);
                        TypeToFilterItem.Add(record.Filter, filterItem);
                    }
                }

                if (settings.Value.SimpleMany.OptIn != null)
                {
                    foreach (var record in settings.Value.SimpleMany.OptIn)
                    {
                        filterItem = CreateFilterItem(record.Filter);
                        TypeToFilterItem.Add(record.Filter, filterItem);
                    }
                }
            }
            */
        }

        private bool RequiresAuth(FilterProviderContext context)
        {
            return !_settings.Value.Authorization.OptOut.RouteTree.ContainsMatch(context);  
        }
        private static FilterItem CreateFilterItem(string filterType)
        {
            var authFilter = TypeHelper<Type>.GetTypeByFullName(filterType);
            var typeFilterAttribute = new TypeFilterAttribute(authFilter) {Order = -1};
            var filterDescriptor = new FilterDescriptor(typeFilterAttribute, -1);
            var filterMetaData = (IFilterMetadata) Activator.CreateInstance(authFilter);
            var fi = new FilterItem(filterDescriptor, filterMetaData);
            return fi;
        }

        private bool TryFetchAuthFilterItem(FilterProviderContext context, out FilterItem filterItem)
        {
            if (RequiresAuth(context))
            {
                filterItem = TypeToFilterItem[_settings.Value.Authorization.Filter];
                return true;
            }
            filterItem = null;
            return false;
        }

        private List<FilterItem> FetchFilters(FilterProviderContext context)
        {
            lock (locker)
            {
                List<FilterItem> filters;
                if (!ActionFilterMap.TryGetValue(context.ActionContext.ActionDescriptor.DisplayName, out filters))
                {
                    filters = new List<FilterItem>();

                    FilterItem filterItem;

                    // Authorzation is a special case.
                    if (RequiresAuth(context))
                    {
                        filterItem = TypeToFilterItem[_settings.Value.Authorization.Filter];
                        filters.Add(filterItem);
                    }

                    // the following are generic optin and optout
                    foreach (var record in _settings.Value.SimpleMany.OptOut)
                    {
                        var match = record.RouteTree.ContainsMatch(context);
                        if (!match)
                        {
                            filterItem = TypeToFilterItem[record.Filter];
                            filters.Add(filterItem);
                        }
                    }

                    foreach (var record in _settings.Value.SimpleMany.OptIn)
                    {
                        var match = record.RouteTree.ContainsMatch(context);
                        if (match)
                        {
                            filterItem = TypeToFilterItem[record.Filter];
                            filters.Add(filterItem);
                        }
                    }
                    ActionFilterMap.Add(context.ActionContext.ActionDescriptor.DisplayName, filters);
                }
                return filters;
            }
        }
       

        //all framework providers have negative orders, so ours will come later
        public void OnProvidersExecuting(FilterProviderContext context)
        {
            ControllerActionDescriptor cad = (ControllerActionDescriptor) context.ActionContext.ActionDescriptor;
            System.Diagnostics.Debug.WriteLine("Controller: " + cad.ControllerTypeInfo.FullName);
            System.Diagnostics.Debug.WriteLine("ActionName: " + cad.Name);
            System.Diagnostics.Debug.WriteLine("DisplayName: " + cad.DisplayName);
            System.Diagnostics.Debug.WriteLine("Area: " + context.ActionContext.RouteData.Values["area"]);

            var filters = FetchFilters(context);
            foreach (var filter in filters)
            {
                context.Results.Add(filter);
            }

        }

        public void OnProvidersExecuted(FilterProviderContext context)
        {
        }

        public int Order => 0;
    }
}
