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
        private static int _musicVolume;

        public static bool IsShown { get => _menuObject != null && _menuObject.Visible; }
        
        public static void Init(IGame game)
        {
            _game = game;
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

                ThisRace.PlayerDriver = 0;
                ThisRace.Laps = 3;
                ThisRace.Opponents = 3;
                ThisRace.AiAndPhysics = ePhysicsSafe;
                ThisRace.CarCollisions = true;
                */
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
                    _menu.AddItem("Driver");
                    _menu.AddItem("Laps");
                    _menu.AddItem("Opponents");
                    _menu.AddItem("Physics");
                    _menu.AddItem("Collisions");
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
            string value;
            switch (_menuClass)
            {
                case MenuClass.eMenuMain:
                case MenuClass.eMenuMainInGame:
                    if (_musicVolume == 0)
                        value = " OFF >";
                    else if (_musicVolume == 100)
                        value = "< FULL";
                    else
                        value = String.Format("< {0,0:D2} >", _musicVolume);
                    _menu.SetSubItem(2, value);
                    break;
                    /*
                case eMenuSetupRace:
                    SilverFont.DrawText("<   >", ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING);
                    ds.DrawImage(portrait_x, portrait_y, 2);
                    ds.DrawImage(portrait_x + 2, portrait_y + 2, portrait_sprite);
                    value = String.Format("< %d >", ThisRace.Laps);
                    SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    value = String.Format("< %d >", ThisRace.Opponents);
                    SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    if (ThisRace.AiAndPhysics == ePhysicsWild)
                        value = "Wild";
                    else
                        value = "Safe";
                    SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 4);
                    if (ThisRace.CarCollisions)
                        value = "ON";
                    else
                        value = "OFF";
                    SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 5);
                    ds.DrawImage(car_x, car_y, 2);
                    ds.DrawImage(car_x + car_xoff, car_y + car_yoff, car_sprite);
                    break;
                    */
            }
            /* TODO:
             * 
             * 
            DrawingSurface* ds = SprOptions.GetDrawingSurface();
            ds.DrawingColor = COLOR_TRANSPARENT;
            ds.DrawRectangle(OPTION_VALUE_X, 0, SprOptions.Width - 1, SprOptions.Height - 1);

            String value;
            int portrait_sprite = 100 + ThisRace.PlayerDriver;
            int portrait_x = OPTION_VALUE_X + SilverFont.GlyphWidth * 5 / 2 - 10 - Game.SpriteWidth[2] - 5;
            int portrait_y = OPTION_POS_TOP + OPTION_SPACING + OPTION_HEIGHT / 2 - Game.SpriteHeight[2] / 2;
            int car_sprite = 7 + ThisRace.PlayerDriver;
            int car_x = OPTION_VALUE_X + SilverFont.GlyphWidth * 5 / 2 - 10 + 5;
            int car_y = portrait_y;
            int car_xoff = (Game.SpriteWidth[2] - Game.SpriteWidth[car_sprite]) / 2;
            int car_yoff = (Game.SpriteHeight[2] - Game.SpriteHeight[car_sprite]) / 2;

            
            ds.Release();
            btnMenuOptions.NormalGraphic = SprOptions.Graphic; // poke button to force AGS redraw it
            */
        }

        private static void cancelMenu()
        {

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

        private static void changeMusicVol(bool up = true)
        {
            if (up)
                setMusicVol(_musicVolume + 5);
            else
                setMusicVol(_musicVolume - 5);
            if (IsShown)
                updateOptionValues();
        }

        // TODO: move to game config controller, or something
        private static void setMusicVol(int vol)
        {
            _musicVolume = MathHelper.Clamp(vol, 0, 100);
            _game.AudioSettings.MasterVolume = _musicVolume * 0.01f;
        }
    }
}
