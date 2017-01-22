using PropertyChanged;

namespace AmmySEA.StackExchangeApi
{
    [ImplementPropertyChanged]
    public class StackExchangeSite
    {
        public string name { get; set; }
        public string api_site_parameter { get; set; }
        public string site_url { get; set; }
        public string favicon_url { get; set; }
        public string icon_url { get; set; }
        public string high_resolution_icon_url { get; set; }

        public bool IsSelected { get; set; }
    }
}
