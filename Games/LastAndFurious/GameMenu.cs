using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    // TODO: replace with actual classes? or maybe not
    public enum MenuClass
    {
        eMenuNone,
        eMenuStart,
        eMenuMain,
        eMenuMainInGame,
        eMenuSetupRace,
        eMenuCredits
    };

    public static class GameMenu
    {
        private const int STARTMENU_OPTION_POS_TOP = 303;
        private const int STARTMENU_OPTION_X = 255;
        private const int STARTMENU_OPTION_SPACING = 32;
        private const int DIAMOND_X = 219;

        private static IGame _game;
        private static IObject _menuObject;
        private static GameMenuComponent _menu;
        private static MenuClass _menuClass;

        public static void ShowMenu(IGame game, MenuClass menuClass, bool pausedInGame = false)
        {
            HideMenu();

            _game = game;
            _menuClass = menuClass;

            if (pausedInGame && !game.State.Paused)
            {
                game.State.Paused = true;
                /* TODO: 
                gRaceOverlay.Visible = false;
                gBanner.Visible = false;
                */
            }

            if (_menuObject == null)
            {
                _menuObject = game.Factory.Object.GetObject("GameMenu");
                _menu = new GameMenuComponent(game, AGSGame.GLUtils);
                _menuObject.AddComponent<GameMenuComponent>(_menu); // TODO: or use resolver?
                game.State.UI.Add(_menuObject);
            }

            ArrangeMenu();
            _menuObject.Visible = true;

            game.Input.KeyDown.Subscribe(onKeyDown);
        }

        public static void HideMenu()
        {
            if (_game == null || _menuObject == null || !_menuObject.Visible)
                return;
            /* TODO:
            SaveOptions();
            gUnderlay.Visible = false;
            */
            _menu.ClearItems();
            _menuObject.Visible = false;
            _menuClass = MenuClass.eMenuNone;

            if (_game.State.Paused)
            {
                /* TODO:
                gRaceOverlay.Visible = true;
                gBanner.Visible = true;
                */
                _game.State.Paused = false;
            }

            _game.Input.KeyDown.Unsubscribe(onKeyDown);
        }

        public static void Dispose()
        {
            if (_game == null || _menuObject == null)
                return;
            _game.State.UI.Remove(_menuObject);
            _menuObject.Dispose();
            _menuObject = null;
        }

        private static void ArrangeMenu()
        {
            _menu.ClearItems();

            // Need to revert Y coordinates ported from original game
            int ry = _game.Settings.VirtualResolution.Height;

            switch (_menuClass)
            {
                case MenuClass.eMenuStart:
                    _menuObject.X = STARTMENU_OPTION_X;
                    _menuObject.Y = ry - STARTMENU_OPTION_POS_TOP;
                    _menu.Font = LF.Fonts.SilverFont;
                    _menu.OptionSpacing = STARTMENU_OPTION_SPACING;
                    _menu.SelectorGraphic = LF.StartMenu.Selector;
                    _menu.SelectorX = DIAMOND_X - STARTMENU_OPTION_X;
                    _menu.AddItem("Start", onStart);
                    _menu.AddItem("Credits");
                    _menu.AddItem("Quit", onQuit);
                    break;
            }
            /* TODO:

            
                case eMenuMain:
                    SilverFont.DrawText("Race", ds, OPTION_X, OPTION_POS_TOP);
                    SilverFont.DrawText("Watch Demo", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING);
                    SilverFont.DrawText("Music", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    SilverFont.DrawText("Quit", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    MMOptionCount = 4;
                    break;
                case eMenuMainInGame:
                    SilverFont.DrawText("Continue", ds, OPTION_X, OPTION_POS_TOP);
                    SilverFont.DrawText("Restart", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING);
                    SilverFont.DrawText("Music", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    SilverFont.DrawText("Quit", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    MMOptionCount = 4;
                    break;
                case eMenuSetupRace:
                    SilverFont.DrawText("Go!", ds, OPTION_X, OPTION_POS_TOP);
                    SilverFont.DrawText("Driver", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING);
                    SilverFont.DrawText("Laps", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    SilverFont.DrawText("Opponents", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    SilverFont.DrawText("Physics", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 4);
                    SilverFont.DrawText("Collisions", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 5);
                    SilverFont.DrawText("Back", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 6);
                    MMOptionCount = 7;
                    break;
                case eMenuCredits:
                    ds.DrawingColor = Game.GetColorFromRGB(11, 15, 54);
                    ds.DrawRectangle(0, 0, 640, 400);
                    y = 40;
                    PurpleItalicFont.DrawTextCentered("CODE", ds, 0, y, ds.Width); y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("Crimson Wizard", ds, 0, y, ds.Width); y += 40;
                    PurpleItalicFont.DrawTextCentered("ART & TECH IDEAS", ds, 0, y, ds.Width); y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("Jim Reed", ds, 0, y, ds.Width); y += 40;
                    PurpleItalicFont.DrawTextCentered("MUSIC", ds, 0, y, ds.Width); y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("\"Car Theft 101\" by Eric Matyas", ds, 0, y, ds.Width); y += AzureItalicFont.Height;
                    AzureItalicFont.DrawTextCentered("www.soundimage.org", ds, 0, y, ds.Width); y += AzureItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("\"Welcome to the Show\" by Kevin MacLeod", ds, 0, y, ds.Width); y += AzureItalicFont.Height;
                    AzureItalicFont.DrawTextCentered("incompetech.com", ds, 0, y, ds.Width); y += AzureItalicFont.Height + 10;
                    PurpleItalicFont.DrawTextCentered("Press any key to continue", ds, 0, STARTMENU_OPTION_POS_TOP + STARTMENU_OPTION_SPACING * 2, ds.Width);
                    break;
            }
            ds.Release();

            if (MenuType != eMenuCredits)
                UpdateOptionValues();

            btnMenuOptions.NormalGraphic = SprOptions.Graphic;
            btnMenuOptions.Visible = true;
            if (MenuType == eMenuStart)
            {
                btnMMSelector.NormalGraphic = 1;
                btnMMSelector.Visible = true;
                btnMMVrStrip.Visible = false;
                gUnderlay.Visible = false;
            }
            else if (MenuType == eMenuCredits)
            {
                btnMMSelector.Visible = false;
                btnMMVrStrip.Visible = false;
                gUnderlay.Visible = false;
            }
            else
            {
                btnMMSelector.NormalGraphic = 4;
                btnMMSelector.Visible = true;
                btnMMVrStrip.Visible = true;
                gUnderlay.Visible = true;
            }
            */



            /* TODO:
                MMSelection = 0;
                UpdateSelection();
            */
        }

        private static void CancelMenu()
        {

        }

        private static void onKeyDown(KeyboardEventArgs args)
        {
            if (!_menuObject.Visible)
                return;
            
            Key key = args.Key;
            switch (key)
            {
                case Key.Escape: CancelMenu(); break;
                case Key.Up: if (_menu.Selection > 0) _menu.Selection--; break;
                case Key.Down: _menu.Selection++; break;
                case Key.Left: _menu.ScrollLeft(); break;
                case Key.Right: _menu.ScrollRight(); break;
                case Key.Enter:
                case Key.Space:
                    _menu.Confirm(); break;
            }

            /* TODO:

            if (MenuType == eMenuCredits)
            {
                SwitchToMenu(eMenuStart);
                return;
            }
            */
        }

        private static async void onStart()
        {
            HideMenu();
            await LF.RaceAssets.LoadAll(_game);
            await LF.Rooms.RaceRoom.GotoAsync();
        }

        private static void onQuit()
        {
            _game.Quit();
        }
    }
}
