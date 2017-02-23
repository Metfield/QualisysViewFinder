namespace Arqus
{
    public sealed class Singleton
    {
        private static readonly Singleton instance = new Singleton();

        // Explicit static constructor to tell C# compilers
        // not to mark type as beforefieldinit
        //
        // See http://csharpindepth.com/Articles/General/Singleton.aspx for more detail (Accessed: 2017-02-23)
        static Singleton() { }

        private Singleton() { }

        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
