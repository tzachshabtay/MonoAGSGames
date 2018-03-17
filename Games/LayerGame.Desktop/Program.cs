using System;
using AGS.Engine.Desktop;

namespace LayerGame.Desktop
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // TODO: properly pass parameters
            string asset_path = "Assets/";
            if (args.Length > 0)
                asset_path = args[0];

            AGSEngineDesktop.Init();
            GameStarter.Run(asset_path,
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/LayersGame/");
        }
    }
}
