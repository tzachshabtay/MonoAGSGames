using System;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// Class for managing a font where each glyph is represented by a bitmap.
    /// </summary>
    // TODO: make a IFont implementation, if I figure out how that is possible.
    public class SpriteFont
    {
        // Images of symbols
        // TODO: learn how to use spritesheet
        IImage[] _glyphs;
        // First and last glyphs are symbol codes that define supported range for this font
        int _firstGlyph;
        int _lastGlyph;
        int _glyphWidth;
        // Individual offsets and widths for each glyth; they are set to 0 and GlyphWidth when
        // the font is created, but can be modified.
        int[] _offs;
        int[] _widths;
        int _height;
        int _baseline;

        /// <summary>
        /// Glyph width for monospaced (fixed-width) fonts.
        /// </summary>
        public int GlyphWidth { get => _glyphWidth; }
        /// <summary>
        /// Gets the full height of the font.
        /// </summary>
        public int Height { get => _height; }
        /// <summary>
        /// Gets the font's baseline.
        /// </summary>
        public int Baseline { get => _baseline; }

        /// <summary>
        /// Create font from the bitmap.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="gl_width"></param>
        /// <param name="height"></param>
        /// <param name="gl_first"></param>
        /// <param name="gl_last"></param>
        /// <param name="offs"></param>
        /// <param name="widths"></param>
        public static SpriteFont CreateFromBitmap(IBitmap bitmap, IGraphicsFactory factory,
                                    int gl_width, int height, int baseline, int gl_first, int gl_last,
                                    int[] offs, int[] widths)
        {
            int sprw = bitmap.Width;
            int sprh = bitmap.Height;
            int in_row = sprw / gl_width;
            int total = in_row * (sprh / height);

            total = Math.Min(total, gl_last - gl_first + 1);
            //Display("Total: %d",total);
            if (total <= 0)
                return null;

            SpriteFont font = new SpriteFont();
            font._glyphs = new IImage[total];
            font._firstGlyph = gl_first;
            font._lastGlyph = gl_last;
            font._glyphWidth = gl_width;
            font._height = height;
            font._baseline = baseline;
            
            if (offs != null)
            {
                font._offs = offs.Clone() as int[];
            }
            else
            {
                font._offs = new int[total];
                for (int i = 0; i < total; ++i)
                    font._offs[i] = 0;
            }

            if (widths != null)
            {
                font._widths = widths.Clone() as int[];
            }
            else
            {
                font._widths = new int[total];
                for (int i = 0; i < total; i++)
                {
                    font._widths[i] = gl_width;
                }
            }

            int x, y;
            int gl = gl_first;
            for (y = 0; y < sprh && gl <= gl_last; y = y + height)
            {
                for (x = 0; x < sprw && gl <= gl_last; x = x + gl_width)
                {
                    //Display("Cut glyph %d: %d,%d -- %d,%d", gl, x + this.Offs[gl], y, this.Widths[gl], height);
                    IBitmap glbmp = bitmap.Crop(new Rectangle(x + font._offs[gl], y, font._widths[gl], height));
                    font._glyphs[gl] = factory.LoadImage(glbmp);
                    gl++;
                }
            }

            return font;
        }

        /// <summary>
        /// Calculates the width of the line of text.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int GetTextWidth(string s)
        {
            int width = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                int gl = s[i];
                if (gl >= _firstGlyph && gl <= _lastGlyph)
                    width += _widths[gl];
            }
            return width;
        }

        /// <summary>
        /// Draws single line of text.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ds"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawText(String s, IGLUtils ds, Vector2 at)
        {
            if (_glyphs == null)
                return;
            
            for (int i = 0; i < s.Length; ++i)
            {
                int gl = s[i];
                if (gl >= _firstGlyph && gl <= _lastGlyph)
                {
                    int glw = _widths[gl];
                    // TODO: more convenient utility function? (is it okay to setup new box every time?)
                    AGSBoundingBox box = new AGSBoundingBox(at.X, at.X + glw, at.Y, at.Y + Height);
                    ds.DrawQuad(_glyphs[gl].Texture.ID, box, 1, 1, 1, 1); // TODO: support text color
                    at.X += _widths[gl];
                }
            }
        }
    }
}
