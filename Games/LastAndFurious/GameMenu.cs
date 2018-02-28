using System;
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

    public enum RacePhysicsMode
    {
        Safe,
        Wild
    }

    public struct GameConfig
    {
        public int MusicVolume;
    }

    public struct RaceEventConfig
    {
        public int PlayerDriver;
        public int Opponents;
        public int Laps;
        public RacePhysicsMode Physics;
        public bool CarCollisions;
    }

    // TODO: remake into non-static class
    public static class GameMenu
    {
        private const int STARTMENU_OPTION_POS_TOP = 303;
        private const int STARTMENU_OPTION_X = 255;
        private const int STARTMENU_OPTION_SPACING = 32;
        private const int DIAMOND_X = 219;

        private const int GAMEMENU_OPTION_POS_TOP = 50;
        private const int GAMEMENU_OPTION_X = 97;
        private const int GAMEMENU_OPTION_SPACING = 50;
        private const int GAMEMENU_OPTION_VALUE_X = 371 - GAMEMENU_OPTION_X;

        private static IGame _game = null;
        private static IObject _menuObject = null;
        private static GameMenuComponent _menu = null;
        private static MenuClass _menuClass = MenuClass.eMenuNone;
        private static MenuClass _prevMenuClass = MenuClass.eMenuNone;
        private static IObject _vbar = null;
        private static IPanel _underlay = null;

        // TODO: move to game config controller, or something
        private static GameConfig _gameCfg;
        private static RaceEventConfig _raceCfg;

        public static bool IsShown { get => _menuObject != null && _menuObject.Visible; }
        
        public static void Init(IGame game)
        {
            _game = game;
            _gameCfg = new GameConfig();
            _raceCfg = new RaceEventConfig();
            SetConfigToDefault();
        }

        public static void SetConfigToDefault()
        {
            setMusicVol(100);
            /*
# ifdef DEBUG
                IsDebugMode = true;
#endif
# ifndef DEBUG
                IsDebugMode = false;
#endif
*/
            _raceCfg.PlayerDriver = 0;
            _raceCfg.Laps = 3;
            _raceCfg.Opponents = 3;
            _raceCfg.Physics = RacePhysicsMode.Safe;
            _raceCfg.CarCollisions = true;
        }

        public static void ShowMenu(MenuClass menuClass, bool pausedInGame = false)
        {
            _prevMenuClass = _menuClass;
            _menuClass = menuClass;

            if (pausedInGame && !LF.GameState.Paused)
            {
                LF.GameState.Paused = true;
                /* TODO: 
                gRaceOverlay.Visible = false;
                gBanner.Visible = false;
                */
            }

            if (_menuObject == null)
            {
                _menuObject = _game.Factory.Object.GetObject("GameMenu");
                _menuObject.RenderLayer = AGSLayers.UI;
                _menuObject.IgnoreViewport = true;
                _game.State.UI.Add(_menuObject);
                // NOTE: we need to add component after all of the object's properties
                // are set, because the component creates child object(s), which may
                // refer to these properties.
                _menu = new GameMenuComponent(_game, AGSGame.GLUtils);
                _menuObject.AddComponent<GameMenuComponent>(_menu); // TODO: or use resolver?
            }

            setupMenuLook();
            arrangeMenu();
            _menuObject.Visible = true;

            _game.Input.KeyDown.Subscribe(onKeyDown);
        }

        public static void HideMenu()
        {
            if (!IsShown)
                return;
            /* TODO:
            SaveOptions();
            gUnderlay.Visible = false;
            */
            _menu.ClearItems();
            _menuObject.Visible = false;
            _menuClass = MenuClass.eMenuNone;
            _prevMenuClass = MenuClass.eMenuNone;

            if (LF.GameState.Paused)
            {
                /* TODO:
                gRaceOverlay.Visible = true;
                gBanner.Visible = true;
                */
                LF.GameState.Paused = false;
            }

            _game.Input.KeyDown.Unsubscribe(onKeyDown);
        }

        public static void Dispose()
        {
            if (_menuObject == null)
                return;
            HideMenu();
            _game.State.UI.Remove(_menuObject);
            _menuObject.Dispose();
            _menuObject = null;
            if (_vbar != null)
            {
                _game.State.UI.Remove(_vbar);
                _vbar.Dispose();
                _vbar = null;
            }
            if (_underlay != null)
            {
                _game.State.UI.Remove(_underlay);
                _underlay.Dispose();
                _underlay = null;
            }
        }

        private static void setupMenuLook()
        {
            // Need to revert Y coordinates ported from original game
            int rx = _game.Settings.VirtualResolution.Width;
            int ry = _game.Settings.VirtualResolution.Height;

            // TODO: review the ties between objects (selector is created by GameMenuComponent)
            // because it is bit messy atm.
            switch (_menuClass)
            {
                case MenuClass.eMenuStart:
                    _menuObject.X = STARTMENU_OPTION_X;
                    _menuObject.Y = ry - STARTMENU_OPTION_POS_TOP;
                    _menu.Font = LF.Fonts.SilverFont;
                    _menu.OptionSpacing = STARTMENU_OPTION_SPACING;
                    _menu.SelectorGraphic = LF.StartMenu.Selector;
                    _menu.SelectorX = DIAMOND_X - STARTMENU_OPTION_X;
                    break;
                case MenuClass.eMenuMain:
                case MenuClass.eMenuMainInGame:
                case MenuClass.eMenuSetupRace:
                    _menuObject.X = GAMEMENU_OPTION_X;
                    _menuObject.Y = ry - GAMEMENU_OPTION_POS_TOP;
                    _menu.Font = LF.Fonts.SilverFont;
                    _menu.OptionSpacing = GAMEMENU_OPTION_SPACING;
                    _menu.SelectorGraphic = LF.RaceMenu.Selector;
                    _menu.SelectorX = -GAMEMENU_OPTION_X;
                    if (_vbar == null)
                    {
                        IObject vbar = _game.Factory.Object.GetObject("GameMenuVBar");
                        vbar.Pivot = new PointF();
                        vbar.Image = LF.RaceMenu.VBar;
                        _menuObject.TreeNode.AddChild(vbar);
                        vbar.RenderLayer = _menuObject.RenderLayer;
                        vbar.X = -GAMEMENU_OPTION_X;
                        vbar.Y = -_menuObject.Y;
                        vbar.Z = 10;
                        vbar.IgnoreViewport = true;
                        _game.State.UI.Add(vbar);
                        _vbar = vbar;
                    }
                    if (_underlay == null)
                    {
                        IPanel panel = _game.Factory.UI.GetPanel("GameMenuUnderlay", rx, ry, -GAMEMENU_OPTION_X, -_menuObject.Y);
                        _menuObject.TreeNode.AddChild(panel);
                        panel.Tint = Color.FromArgb(255 - 70, 72, 20, 0);
                        panel.Z = 20;
                        _underlay = panel;
                    }
                    break;
            }
        }

        private static void arrangeMenu()
        {
            _menu.ClearItems();
            switch (_menuClass)
            {
                case MenuClass.eMenuStart:
                    _menu.AddItem("Start", onStart);
                    _menu.AddItem("Credits");
                    _menu.AddItem("Quit", onQuit);
                    break;
                case MenuClass.eMenuMain:
                    _menu.AddItem("Race");
                    _menu.AddItem("Watch Demo");
                    _menu.AddItem("Music");
                    _menu.AddItem("Quit");
                    break;
                case MenuClass.eMenuMainInGame:
                    _menu.AddItem("Continue", HideMenu);
                    _menu.AddItem("Restart", ()=> { switchToMenu(MenuClass.eMenuSetupRace); });
                    _menu.AddItem("Music", () => changeMusicVol(true), () => changeMusicVol(false), () => changeMusicVol(true));
                    _menu.AddItem("Quit", onQuit);
                    break;
                case MenuClass.eMenuSetupRace:
                    _menu.AddItem("Go!", onGo);
                    _menu.AddItem("Driver", ()=> changeDriver(true), () => changeDriver(false), () => changeDriver(true));
                    _menu.AddItem("Laps", () => changeLaps(true), () => changeLaps(false), () => changeLaps(true));
                    _menu.AddItem("Opponents", () => changeOpponents(true), () => changeOpponents(false), () => changeOpponents(true));
                    _menu.AddItem("Physics", changePhysics, changePhysics, changePhysics);
                    _menu.AddItem("Collisions", changeCollisions, changeCollisions, changeCollisions);
                    _menu.AddItem("Back", ()=> { switchToMenu(_prevMenuClass); });
                    break;
            }

            if (_menuClass != MenuClass.eMenuStart && _menuClass != MenuClass.eMenuCredits)
            {
                _menu.OptionValueX = GAMEMENU_OPTION_VALUE_X;
                updateOptionValues();
            }

            /* TODO:
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

        private static void updateOptionValues()
        {
            switch (_menuClass)
            {
                case MenuClass.eMenuMain:
                case MenuClass.eMenuMainInGame:
                    {
                        string value;
                        if (_gameCfg.MusicVolume == 0)
                            value = " OFF >";
                        else if (_gameCfg.MusicVolume == 100)
                            value = "< FULL";
                        else
                            value = String.Format("< {0,0:D2} >", _gameCfg.MusicVolume);
                        _menu.SetSubItem(2, 0, value);
                    }
                    break;
                case MenuClass.eMenuSetupRace:
                    {
                        string value = "<   >";
                        int subwidth = _menu.Font.GetTextWidth(value);
                        string driverName = LF.RaceAssets.Names[_raceCfg.PlayerDriver];
                        DriverCharacter driver = LF.RaceAssets.Drivers[driverName];
                        IImage frame = LF.RaceMenu.PortraitFrame;
                        IImage face_pic = driver.Portrait;
                        float portrait_x = (subwidth - frame.Width) / 2 - 5;
                        float portrait_y = _menu.Font.Height / 2 - frame.Height / 2;
                        IImage car_pic = driver.CarModel;
                        float car_x = (subwidth + frame.Width) / 2 + 5;
                        float car_y = portrait_y;
                        
                        _menu.SetSubItem(1, 0, value);
                        _menu.SetSubItemPic(1, 1, frame, portrait_x, portrait_y, -3);
                        _menu.SetSubItemPic(1, 2, face_pic, portrait_x, portrait_y, -4);
                        _menu.SetSubItemPic(1, 3, frame, car_x, car_y, -3);
                        _menu.SetSubItemPic(1, 4, car_pic, car_x, car_y, -4);
                        _menu.SetSubItem(2, 0, String.Format("< {0,0:D1} >", _raceCfg.Laps));
                        _menu.SetSubItem(3, 0, String.Format("< {0,0:D1} >", _raceCfg.Opponents));
                        if (_raceCfg.Physics == RacePhysicsMode.Wild)
                            value = "Wild";
                        else
                            value = "Safe";
                        _menu.SetSubItem(4, 0, value);
                        if (_raceCfg.CarCollisions)
                            value = "ON";
                        else
                            value = "OFF";
                        _menu.SetSubItem(5, 0, value);
                    }
                    break;
            }
        }

        private static void cancelMenu()
        {
            if (_menuClass == MenuClass.eMenuMain || _menuClass == MenuClass.eMenuMainInGame)
            {
                HideMenu();
            }
            else if (_prevMenuClass != MenuClass.eMenuNone)
            {
                switchToMenu(_prevMenuClass);
            }
        }

        private static void switchToMenu(MenuClass menu)
        {
            ShowMenu(menu, LF.GameState.Paused);
        }

        private static void onKeyDown(KeyboardEventArgs args)
        {
            Key key = args.Key;
            switch (key)
            {
                case Key.Escape: cancelMenu(); break;
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
            Dispose();
            await LF.RaceAssets.LoadAll(_game);
            await LF.RaceMenu.LoadAll(_game);
            await LF.Rooms.RaceRoom.GotoAsync();
        }

        private static void onQuit()
        {
            _game.Quit();
        }

        private static void onGo()
        {
            /* TODO:
            HideGameMenu();
            CallRoomScript(eRoom305_StartSinglePlayer);
            */
        }

        private static void changeMusicVol(bool up)
        {
            if (up)
                setMusicVol(_gameCfg.MusicVolume + 5);
            else
                setMusicVol(_gameCfg.MusicVolume - 5);
            updateOptionValues();
        }

        private static void changeDriver(bool up)
        {
            _raceCfg.PlayerDriver = up ? _raceCfg.PlayerDriver + 1 : _raceCfg.PlayerDriver - 1;
            if (_raceCfg.PlayerDriver < 0) _raceCfg.PlayerDriver = LF.RaceAssets.Drivers.Count - 1;
            else if (_raceCfg.PlayerDriver == LF.RaceAssets.Drivers.Count) _raceCfg.PlayerDriver = 0;
            updateOptionValues();
        }

        private static void changeLaps(bool up)
        {
            _raceCfg.Laps = up ? _raceCfg.Laps + 1 : _raceCfg.Laps - 1;
            _raceCfg.Laps = MathHelper.Clamp(_raceCfg.Laps, 1, 9);
            updateOptionValues();
        }

        private static void changeOpponents(bool up)
        {
            _raceCfg.Opponents = up ? _raceCfg.Opponents + 1 : _raceCfg.Opponents - 1;
            _raceCfg.Opponents = MathHelper.Clamp(_raceCfg.Opponents, 0, Race.MAX_RACING_CARS - 1);
            updateOptionValues();
        }

        private static void changePhysics()
        {
            if (_raceCfg.Physics == RacePhysicsMode.Safe)
                _raceCfg.Physics = RacePhysicsMode.Wild;
            else
                _raceCfg.Physics = RacePhysicsMode.Safe;
            updateOptionValues();
        }

        private static void changeCollisions()
        {
            _raceCfg.CarCollisions = !_raceCfg.CarCollisions;
            updateOptionValues();
        }

        // TODO: move to game config controller, or something
        private static void setMusicVol(int vol)
        {
            _gameCfg.MusicVolume = MathHelper.Clamp(vol, 0, 100);
            // TODO: separate volume for music tracks
            _game.AudioSettings.MasterVolume = _gameCfg.MusicVolume * 0.01f;
        }
    }
}
