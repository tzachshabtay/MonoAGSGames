using AGS.Engine.Desktop;

namespace LastAndFurious.Desktop
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            AGSEngineDesktop.Init();
            GameStarter.Run();
        }
    }
}
