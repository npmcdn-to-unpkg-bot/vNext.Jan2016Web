using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.OptionsModel;
using Pingo.Core.Reflection;
using Pingo.Core.Settings;

namespace Pingo.Core.Providers
{
    public class OptOutOptInFilterProvider : IFilterProvider
    {
        IOptions<FiltersConfig> _settings;
        IServiceProvider _serviceProvider;
        private static readonly object locker = new object();
        private static Dictionary<string, List<FilterItem>> ActionFilterMap = new Dictionary<string, List<FilterItem>>();
        private static Dictionary<string, FilterItem> TypeToFilterItem = new Dictionary<string, FilterItem>();
        private static bool _frontLoaded;

        public OptOutOptInFilterProvider(IServiceProvider serviceProvider, IOptions<FiltersConfig> settings)
        {
            _settings = settings;
            _serviceProvider = serviceProvider;
            FrontLoadFilterItems();
        }

        private void FrontLoadFilterItems()
        {
            if (_settings.Value.SimpleMany != null)
            {
                FilterItem filterItem;
                if (_settings.Value.SimpleMany.OptOut != null)
                {
                    foreach (var record in _settings.Value.SimpleMany.OptOut)
                    {
                        filterItem = CreateFilterItem(record.Filter);
                        TypeToFilterItem.Add(record.Filter, filterItem);
                    }
                }

                if (_settings.Value.SimpleMany.OptIn != null)
                {
                    foreach (var record in _settings.Value.SimpleMany.OptIn)
                    {
                        filterItem = CreateFilterItem(record.Filter);
                        TypeToFilterItem.Add(record.Filter, filterItem);
                    }
                }
            }
        }

        private FilterItem CreateFilterItem(string filterType)
        {
            var type = TypeHelper<Type>.GetTypeByFullName(filterType);
            var typeFilterAttribute = new TypeFilterAttribute(type) {Order = 0};
            var filterDescriptor = new FilterDescriptor(typeFilterAttribute, 0);
            var filterInstance = _serviceProvider.GetService(type);
            var filterMetaData = (IFilterMetadata) filterInstance;
            var fi = new FilterItem(filterDescriptor, filterMetaData);
            return fi;
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