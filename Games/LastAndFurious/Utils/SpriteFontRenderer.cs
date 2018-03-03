using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    // TODO: should be a component implementing IRenderer, not custom renderer class
    public class SpriteFontRenderer : IImageRenderer
    {
        private SpriteFont _font;
        private string _text;
        private SizeF _size;
        private readonly IGLUtils _glUtils;

        public SpriteFont Font
        {
            get => _font;
            set { _font = value; _size = new SizeF(_font.GetTextWidth(_text), _font.Height); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; _size = new SizeF(_font.GetTextWidth(_text), _font.Height); }
        }

        public SpriteFontRenderer(SpriteFont font, string text, IGLUtils glUtils)
        {
            _font = font;
            _text = text;
            _size = new SizeF(font.GetTextWidth(text), font.Height);
            
            _glUtils = glUtils;
        }

        public SizeF? CustomImageSize => _size;

        public PointF? CustomImageResolutionFactor => null;

        public void Prepare(IObject obj, IDrawableInfoComponent drawable, IViewport viewport)
        {
        }

        public void Render(IObject obj, IViewport viewport)
        {
            if (!obj.Visible) return;

            // TODO: find out how to setup proper RenderBox for the custom object
            var bottomLeft = obj.GetBoundingBoxes(viewport).RenderBox.BottomLeft;
            var x = bottomLeft.X;
            var y = bottomLeft.Y - _size.Height;

            // TODO: aligned (e.g. centered) draw
            // TODO: probably move drawtext out of SpriteFont to SpriteFontRenderer?
            _font.DrawText(_text, _glUtils, new Vector2(x, y));

            if (obj.DebugDrawPivot)
            {
                _glUtils.DrawCross(x, y, 10, 10, 1f, 0f, 0f, 1f);
            }
        }
    }
}
