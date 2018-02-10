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

            // Find drawing location
            // TODO: should not there be a global algorithm for this??
            IObject parent = obj.TreeNode.Parent;
            float x = obj.X;
            float y = obj.Y;
            while (parent != null)
            {
                x += parent.X;
                y += parent.Y;
                parent = parent.TreeNode.Parent;
            }
            x -= viewport.X;
            y -= viewport.Y;

            // TODO: aligned (e.g. centered) draw
            _font.DrawText(_text, _glUtils, new Vector2(x, y));

            if (obj.DebugDrawPivot)
            {
                _glUtils.DrawCross(x, y, 10, 10, 1f, 0f, 0f, 1f);
            }
        }
    }
}
