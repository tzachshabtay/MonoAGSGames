using AGS.Engine.Desktop;
using Game;

namespace DemoQuest.Desktop
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
