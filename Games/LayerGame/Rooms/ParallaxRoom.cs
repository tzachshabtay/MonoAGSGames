using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LayerGame
{
    public class ParallaxRoom : RoomScript
    {
        private const string ROOM_ID = "ParallaxRoom";
        private IRenderLayer[] _backgroundLayers;

        public ParallaxRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        protected async Task<IObject> addTerra(string name, string gfile, int x, int y)
        {
            IObject o = await addObject(name, gfile, x, y);
            o.Pivot = new PointF();
            return o;
        }

        protected override async Task<IRoom> loadAsync()
        {
            IGameFactory f = _game.Factory;
            _room = f.Room.GetRoom(ROOM_ID);

            PointF[] parallaxVals = new PointF[4]
            {
                new PointF(0.01f, 0.2f), new PointF(0.15f, 0.4f), new PointF(0.1f, 0.6f), new PointF(0.3f, 0.8f)
            };

            Point[] coordVals = new Point[4]
            {
                new Point(-200, 0), new Point(-320, 250), new Point(-320, 100), new Point(-240, -50)
            };

            _backgroundLayers = new IRenderLayer[4];
            for (int i = 0, z = AGSLayers.Background.Z - 1; i < 4; ++i, --z)
            {
                _backgroundLayers[i] = new AGSRenderLayer(z, parallaxVals[i]);
                IObject o = await addTerra("layer1", $"parallax_l{i + 1}.png", coordVals[i].X, coordVals[i].Y);
                o.RenderLayer = _backgroundLayers[i];
            }

            int rx = _game.Settings.VirtualResolution.Width;
            IObject foreground = await addTerra("foreground", "parallax_l5.png", 0, -55);
            _room.RoomLimitsProvider = AGSRoomLimits.Infinite;

            _game.Events.OnRepeatedlyExecute.Subscribe(onRepExec);

            return _room;
        }

        private void onRepExec(IRepeatedlyExecuteEventArgs args)
        {
            var input = _game.Input;
            var view = _game.State.Viewport;
            float viewspeed = 100f * (float)args.DeltaTime;
            if (input.IsKeyDown(Key.Left))
                view.X -= viewspeed;
            if (input.IsKeyDown(Key.Right))
                view.X += viewspeed;
            if (input.IsKeyDown(Key.Up))
                view.Y += viewspeed;
            if (input.IsKeyDown(Key.Down))
                view.Y -= viewspeed;
        }
    }
}
