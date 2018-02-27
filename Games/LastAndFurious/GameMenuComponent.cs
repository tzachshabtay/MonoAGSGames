using System;
using System.Collections.Generic;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// Describes a single item of a game menu
    /// </summary>
    /// TODO: make MenuItem a component
    /// TODO: a proper menu item tree structure, with functions to add/remove subitems
    public class MenuItem
    {
        /// <summary>
        /// Gets/sets displayed text.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Gets an event which is dispatched when the player hits cofirmation key.
        /// </summary>
        /// TODO: check if should be blocking events
        public IEvent OnConfirm { get; private set; }
        /// <summary>
        /// Gets an event which is dispatched when the player hits left direction key.
        /// </summary>
        public IEvent OnScrollLeft { get; private set; }
        /// <summary>
        /// Gets an event which is dispatched when the player hits right direction key.
        /// </summary>
        public IEvent OnScrollRight { get; private set; }

        public IObject SubItem { get; set; }

        public MenuItem(string text, Action onConfirm = null, Action onScrollLeft = null, Action onScrollRight = null)
        {
            Text = text;
            OnConfirm = new AGSEvent();
            OnScrollLeft = new AGSEvent();
            OnScrollRight = new AGSEvent();

            if (onConfirm != null) OnConfirm.Subscribe(onConfirm);
            if (onScrollLeft != null) OnScrollLeft.Subscribe(onScrollLeft);
            if (onScrollRight != null) OnScrollRight.Subscribe(onScrollRight);
        }
    }

    // TODO: review this
    // probably make MenuItem an entity's behavior component
    public struct MenuObject
    {
        public MenuItem Item { get; set; }
        public IObject Label { get; set; }

        public MenuObject(MenuItem item, IObject label)
        {
            Item = item;
            Label = label;
        }
    }

    /// <summary>
    /// Component that describes on-screen menu. Menu options are depicted by labels,
    /// and there is a sprite that indicates player selection.
    /// Menu is keyboard-controlled only.
    /// </summary>
    public class GameMenuComponent : AGSComponent
    {
        private IGame _game;
        private IInObjectTreeComponent _tree;
        private IDrawableInfoComponent _drawable;
        private readonly IGLUtils _glUtils;
        private IVisibleComponent _visible;

        // TODO: use Binding List and expose Items in class API?
        // TODO: use actual labels when custom or bitmap fonts are supported
        // TODO: a proper menu item tree, where an item may have subitems to display values
        private List<MenuObject> _items = new List<MenuObject>();
        private IObject _selector;

        private int _selection;

        /// <summary>
        /// Get/set the sprite font used in the game menu.
        /// </summary>
        /// TODO: recreate existing items when the font is changed (or rather modify their custom renderer?)
        public SpriteFont Font { get; set; }
        /// <summary>
        /// Get/set the vertical step between two menu items.
        /// </summary>
        public int OptionSpacing { get; set; }
        /// <summary>
        /// Get/set the relative option value position
        /// </summary>
        public int OptionValueX { get; set; }
        /// <summary>
        /// Get/set the image used as a menu option selector
        /// </summary>
        public IImage SelectorGraphic { get => _selector.Image; set => _selector.Image = value; }
        /// <summary>
        /// Get/set the relative selector position
        /// </summary>
        public float SelectorX { get => _selector.X; set => _selector.X = value; }
        /// <summary>
        /// Get/set current selection index.
        /// </summary>
        public int Selection
        {
            get => _selection;
            set
            {
                // TODO: integer version of Clamp
                int sel = Math.Max(-1, Math.Min(value, _items.Count - 1));
                if (sel >= 0)
                {
                    if (_selection < 0)
                        _selector.Visible = true;
                    _selector.Y = sel * -OptionSpacing - (Font.Baseline / 2 + _selector.Height / 2);
                }
                else if (_selection >= 0)
                {
                    _selector.Visible = false;
                }
                _selection = sel;
            }
        }


        public GameMenuComponent(IGame game, IGLUtils glUtils)
        {
            _game = game;
            _glUtils = glUtils;
            _selection = -1;

            _selector = _game.Factory.Object.GetObject("GameMenuSelector");
            _selector.Pivot = new PointF();
            _selector.Visible = false;
            _game.State.UI.Add(_selector);
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            entity.Bind<IInObjectTreeComponent>(c => { _tree = c; _tree.TreeNode.AddChild(_selector); _selector.Z = 1; }, _ => _tree = null);
            entity.Bind<IDrawableInfoComponent>(c => { _drawable = c; _selector.RenderLayer = _drawable.RenderLayer; _selector.IgnoreViewport = true; }, _ => _drawable = null);
            entity.Bind<IVisibleComponent>(c => _visible = c, _ => _visible = null);
        }

        public override void Dispose()
        {
            _game.State.UI.Remove(_selector);
            ClearItems();
            base.Dispose();
        }

        /// <summary>
        /// Adds new menu item.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onConfirm"></param>
        /// <param name="onScrollLeft"></param>
        /// <param name="onScrollRight"></param>
        public void AddItem(string text, Action onConfirm = null, Action onScrollLeft = null, Action onScrollRight = null)
        {
            MenuItem item = new MenuItem(text, onConfirm, onScrollLeft, onScrollRight);
            float y = _items.Count * -OptionSpacing;
            IObject label = setupItem(string.Format("GameMenuItem{0}", _items.Count), text, 0f, y);
            _items.Add(new MenuObject(item, label));

            if (Selection < 0)
                Selection = 0;
        }

        private IObject setupItem(string id, string text, float x, float y)
        {
            IObject label = _game.Factory.Object.GetObject(id);
            _game.State.UI.Add(label);
            _tree?.TreeNode.AddChild(label);
            if (_drawable != null)
            {
                label.RenderLayer = _drawable.RenderLayer;
                label.IgnoreViewport = true;
            }
            
            //label.DebugDrawPivot = true;
            label.X = x;
            label.Y = y;
            label.Z = -2;
            var image = CreateItemImage(text);
            label.CustomRenderer = image;
            return label;
        }

        /// <summary>
        /// Adds a subitem for the item, displayed to the right of its parent.
        /// </summary>
        /// <param name="itemIndex">A zero-based index of the parent item</param>
        /// <param name="text">Subitem's text</param>
        public void SetSubItem(int itemIndex, string text)
        {
            var item = _items[itemIndex];
            if (item.Item.SubItem == null)
            {
                IObject label = setupItem(string.Format("GameMenuItem{0}_subitem", itemIndex), text, OptionValueX, item.Label.Y);
                item.Item.SubItem = label;
            }
            else
            {
                var image = CreateItemImage(text);
                item.Item.SubItem.CustomRenderer = image;
            }
        }

        /// <summary>
        /// Deletes all menu items.
        /// </summary>
        public void ClearItems()
        {
            foreach (var tuple in _items)
            {
                _game.State.UI.Remove(tuple.Label);
                if (tuple.Item.SubItem != null)
                    _game.State.UI.Remove(tuple.Item.SubItem);
            }
            if (_tree != null)
                foreach (var tuple in _items)
                {
                    _tree.TreeNode.RemoveChild(tuple.Label);
                    if (tuple.Item.SubItem != null)
                        _tree.TreeNode.RemoveChild(tuple.Item.SubItem);
                }
            _items.Clear();

            Selection = -1;
        }

        public void Confirm()
        {
            if (_selection < 0)
                return;
            _items[_selection].Item.OnConfirm.InvokeAsync(); // TODO: check if should be blocking
        }

        public void ScrollLeft()
        {
            if (_selection < 0)
                return;
            _items[_selection].Item.OnScrollLeft.InvokeAsync(); // TODO: check if should be blocking
        }

        public void ScrollRight()
        {
            if (_selection < 0)
                return;
            _items[_selection].Item.OnScrollRight.InvokeAsync(); // TODO: check if should be blocking
        }

        private IImageRenderer CreateItemImage(string text)
        {
            return new SpriteFontRenderer(Font, text, _glUtils);
        }
    }
}
