using System.Threading.Tasks;
using AGS.API;

namespace AudioMixerGame
{
    public abstract class RoomScript
    {
        protected readonly IGame _game;
        protected readonly string _roomAssetFolder;

        protected IRoom _room;

        public IRoom Room { get => _room; }

        public RoomScript(IGame game, string assetFolder)
        {
            _game = game;
            _roomAssetFolder = AMG.RoomAssetFolder + assetFolder + "/";
        }

        public async Task<IRoom> PrepareRoom()
        {
            if (_room != null)
                return _room;

            _room = await loadAsync();
            _game.State.Rooms.Add(_room);
            return _room;
        }

        public async Task GotoAsync()
        {
            await PrepareRoom();
            await _game.State.ChangeRoomAsync(_room);
        }

        protected virtual async Task<IRoom> loadAsync() { return null; }

        protected IObject addObject(string name, IImage image, int x = 0, int y = 0)
        {
            IObject o = _game.Factory.Object.GetObject(name);
            o.Image = image;
            o.X = x;
            o.Y = y;
            return o;
        }

        protected async Task<IObject> addObject(string name, string gfile, int x = 0, int y = 0)
        {
            IObject o = _game.Factory.Object.GetObject(name);
            o.Image = await _game.Factory.Graphics.LoadImageAsync(_roomAssetFolder + gfile);
            o.X = x;
            o.Y = y;
            return o;
        }

        /// <summary>
        /// Inverts vector Y coordinate, transforming it from AGS to MonoAGS-compatible
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected Vector2 compatVector(int x, int y)
        {
            return new Vector2(x, _room.Limits.Height - y);
        }
    }
}
