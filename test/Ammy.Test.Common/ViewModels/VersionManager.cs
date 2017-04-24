namespace AmmyTest.Common.ViewModels
{
    public class VersionManager
    {
        public static string FilterString { get; set; }
        public static VersionManager Instance { get { return new VersionManager(); } }
    }
}
