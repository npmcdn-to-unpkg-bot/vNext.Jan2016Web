namespace Pingo.Core.Settings
{
    public class FiltersConfig
    {
        public const string WellKnown_FilterSectionName = "Filters";
        public SimpleManyConfig SimpleMany { get; set; }
        public GlobalPathConfig GlobalPath { get; set; }
    }


}
