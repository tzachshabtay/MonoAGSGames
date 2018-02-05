using System.Threading.Tasks;
using AGS.API;

namespace LastAndFurious
{
    public abstract class RoomScript
    {
        protected readonly IGame _game;
        protected readonly string _roomAssetFolder;

        protected IRoom _room;

        protected virtual async Task<IRoom> LoadAsync() { return null; }

        public IRoom Room { get => _room; }

        public RoomScript(IGame game, string assetFolder)
        {
            _game = game;
            _roomAssetFolder = LF.RoomAssetFolder + assetFolder + "/";
        }

        public async Task<IRoom> PrepareRoom()
        {
            if (_room != null)
                return _room;

            _room = await LoadAsync();
            _game.State.Rooms.Add(_room);
            return _room;
        }

        public async Task GotoAsync()
        {
            await PrepareRoom();
            await _game.State.ChangeRoomAsync(_room);
        }

        protected async Task<IObject> addObject(string name, string gfile, int x = 0, int y = 0)
        {
            IObject o = _game.Factory.Object.GetObject(name);
            o.Image = await _game.Factory.Graphics.LoadImageAsync(_roomAssetFolder + gfile);
            o.X = x;
            o.Y = y;
            return o;
        }
    }
}
