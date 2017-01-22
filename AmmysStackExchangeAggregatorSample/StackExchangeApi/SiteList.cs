namespace AmmySEA.StackExchangeApi
{
    public class SiteList
    {
        public StackExchangeSite[] items { get; set; }
        public bool has_more { get; set; }
        public int quota_max { get; set; }
        public int quota_remaining { get; set; }
    }
}