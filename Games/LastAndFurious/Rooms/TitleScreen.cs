using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    //
    // TODO:
    // IDEAS to think over and discuss.
    //
    // Component - RoomHandler / RoomScript / RoomBehavior.
    // * Automatically subscribes for room events, and to room loading call.
    // * Possible usage: create a dummy entity in game and add this component. It will async load
    //   room by command and change to it, then subscribe to repexec, etc.
    // * Inheriting this component will possibly let to create simple room script classes.
    //

    public class TitleScreen : RoomScript
    {
        private const string ROOM_ID = "TitleScreen";
        private IAudioClip _music;

        public TitleScreen(IGame game) : base(game, ROOM_ID)
        {
        }

        protected override async Task<IRoom> loadAsync()
        {
            IGameFactory factory = _game.Factory;
            _room = factory.Room.GetRoom(ROOM_ID);
            _room.Background = await addObject("TitleScreen.BG", "gradiented-title.png", 0, -80);

            // TODO: label with the game version number in the corner of the title screen
            var label = factory.UI.GetLabel("VersionLabel", LF.GAME_VERSION, 0, 0, 0, 0, addToUi: false);
            label.TextConfig.Font = AGSGameSettings.DefaultTextFont;
            label.TextConfig.AutoFit = AutoFit.LabelShouldFitText;
            _room.Objects.Add(label);

            _room.RoomLimitsProvider = AGSRoomLimits.FromBackground;

            _music = await factory.Sound.LoadAudioClipAsync(LF.MusicAssetFolder + "Car-Theft-101.ogg");

            _room.Events.OnBeforeFadeIn.Subscribe(onLoad);
            _room.Events.OnAfterFadeIn.Subscribe(onAfterFadeIn);
            _room.Events.OnAfterFadeOut.Subscribe(onLeave);

            return _room;
        }

        private void onLoad()
        {
            // TODO: stop all sounds

            GameMenu.ShowMenu(MenuClass.eMenuStart);
        }

        private void onAfterFadeIn()
        {
            //_music.Play(true);
        }

        private void onLeave()
        {
            // TODO: stop all sounds
            foreach (var sound in _music.CurrentlyPlayingSounds)
                sound.Stop();
        }
    }
}
