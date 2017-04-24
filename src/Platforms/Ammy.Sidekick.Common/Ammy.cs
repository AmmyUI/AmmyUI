namespace AmmySidekick
{
    public static partial class Ammy
    {
        public static string GetAssemblyName(object app)
        {
            return app.GetType().Assembly.GetName().Name;
        }
    }
}