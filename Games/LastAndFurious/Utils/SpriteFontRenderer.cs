using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class SpriteFontRenderer : IImageRenderer
    {
        private readonly SpriteFont _font;
        private readonly string _text;
        private readonly SizeF _size;
        private readonly IGLUtils _glUtils;

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

            var bottomLeft = obj.GetBoundingBoxes(viewport).RenderBox.BottomLeft;
            var x = bottomLeft.X;
            var y = bottomLeft.Y - _size.Height;

            // TODO: aligned (e.g. centered) draw
            _font.DrawText(_text, _glUtils, new Vector2(x, y));

            if (obj.DebugDrawPivot)
            {
                _glUtils.DrawCross(x, y, 10, 10, 1f, 0f, 0f, 1f);
            }
        }
    }
}
