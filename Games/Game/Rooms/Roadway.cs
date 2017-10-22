using System.Collections.Generic;
using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace Game
{
    public class Roadway
    {
        private IRoom _room;
        private IGame _game;
        private ObjectPool<IObject> _townObj;
        private List<IImage> _townImages;
        private IObject _roadBar;

        public const int MaxTownObjects = 20;

        private const string _baseFolder = "../../Assets/Rooms/Roadway/";
        private const string _roomId = "Roadway";

        public Roadway(IGame game)
        {
            _game = game;
        }

        private async Task<IObject> AddObject(string name, string gfile, int x, int y)
        {
            IObject o = _game.Factory.Object.GetObject(name);
            o.Image = await _game.Factory.Graphics.LoadImageAsync(_baseFolder + gfile);
            o.X = x;
            o.Y = y;
            return o;
        }

        private async Task CreateTownObjects()
        {
            int index = 0;
            _townObj = new ObjectPool<IObject>(_ =>
            {
                IObject o = _game.Factory.Object.GetObject("townobj" + index++.ToString());
                o.Visible = false;
                _room.Objects.Add(o);
                return o;
            }, MaxTownObjects, o => { o.Visible = false; });

            _townImages = new List<IImage>();
            for (int i = 0; ; ++i)
            {
                IImage image = await _game.Factory.Graphics.LoadImageAsync(_baseFolder + "town" + i.ToString() + ".png");
                if (image == null || image is EmptyImage)
                    break;
                _townImages.Add(image);
            }
        }

        public async Task<IRoom> LoadAsync()
        {
            _room = _game.Factory.Room.GetRoom(_roomId);
            _room.Background = await AddObject("Roadway BG", "bg.png", 0, 0);
            _room.Objects.Add(await AddObject("Road", "roadpave.png", 640, 100));

            var bar = await AddObject("RoadBar", "roadbar2.png", -1280, 356);
            bar.Anchor = new PointF(0f, 0f);
            bar.ResetBaseSize(1280 * 2, bar.Height);
            bar.Image.Texture.Config = new AGSTextureConfig(wrapX: TextureWrap.MirroredRepeat, wrapY: TextureWrap.Clamp);
            _room.Objects.Add(bar);
            _roadBar = bar;

            await CreateTownObjects();

            _room.RoomLimitsProvider = AGSRoomLimits.FromBackground;

            _room.Events.OnBeforeFadeIn.Subscribe(onBeforeFadeIn);
            _game.Events.OnRepeatedlyExecute.Subscribe(onRepeatedlyExecute);

            return _room;
        }

        private Tween cameraTween;

        private void onBeforeFadeIn()
        {
            if (_townImages.Count == 0)
                return;
            loopBuildingAnimation();
            _roadBar.TweenX(-1280 % (_roadBar.Image.Width * 2), 1f).RepeatForever();
        }

        private async void loopBuildingAnimation()
        {
            const int minY = 360;
            const int maxY = 440;
            const float minTime = 3f;
            const float defTime = 6f;
            const float farScaleMod = 0.4f;

            var o = _townObj.Acquire();
            o.X = -500;
            o.Y = MathUtils.Random().Next(minY, maxY);
            o.Image = _townImages[MathUtils.Random().Next(_townImages.Count)];
            float distanceFactor = (float)(o.Y - minY) / (float)(maxY - minY);
            float scale = 1f - farScaleMod * distanceFactor;
            o.ScaleBy(scale, scale);
            o.Visible = true;
            float tweenTime = distanceFactor * defTime + minTime;
            fireBuildingTween(o, tweenTime);

            await Task.Delay(MathUtils.Random().Next(200, 1500));
            loopBuildingAnimation();
        }

        private async void fireBuildingTween(IObject o, float tweenTime)
        {
            await o.TweenX(1500, tweenTime, Ease.Linear).Task;
            _townObj.Release(o);
        }

        private void onRepeatedlyExecute()
        {
            if (_game.State.Room != _room)
                return;
            
            if (cameraTween?.Task?.IsCompleted ?? true)
            {
                float tweenTime = (float)MathUtils.Random().NextDouble() * 8f + 2f;
                if (cameraTween == null)
                    cameraTween = _game.State.Viewport.TweenY(40, tweenTime, Ease.SineInOut);
                else if (cameraTween.To == 40)
                {
                    if (MathUtils.Random().NextDouble() < 0.5)
                        cameraTween = _game.State.Viewport.TweenY(-40, tweenTime, Ease.SineInOut);
                    else
                        cameraTween = _game.State.Viewport.TweenY(0, tweenTime, Ease.SineInOut);
                }
                else
                {
                    if (MathUtils.Random().NextDouble() < 0.5)
                        cameraTween = _game.State.Viewport.TweenY(40, tweenTime, Ease.SineInOut);
                    else
                        cameraTween = _game.State.Viewport.TweenY(0, tweenTime, Ease.SineInOut);
                }
            }
        }
    }
}
