using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Threading;

namespace ConsoleWrapper
{
    class FontTexture : IDisposable
    {
        private const char _unknownChar = '█';

        // Member variables
        private char[] _renderableLetters = 
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '|', '\\', '[', ']', '{', '}',
                ';', ':', '\'', '"', ',', '<', '.', '>', '/', '?', '`', '~', ' ', _unknownChar
            };

        public struct LetterInfo
        {
            public double ul;
            public double ur;
            public double vt;
            public double vb;
            public int w;
            public int h;
        }

        private Dictionary<char, LetterInfo> _letters;

        private bool _valid = false;
        public bool Valid
        {
            get { return _valid; }
        }
        private bool _building = false;

        private Texture _texture;
        public Texture Texture
        {
            get { if (_valid) return _texture; else return null; }
        }

        private String _fontFace;
        public String FontFace
        {
            get { return _fontFace; }
        }

        private int _fontSize;
        public int FontSize
        {
            get { return _fontSize; }
        }

        private System.Drawing.Font _font;
        private int _letterHeight;
        private int _letterWidth;

        // Constructors
        public FontTexture(string fontFace, int fontSize, Device device):
            this(fontFace, fontSize, null, device, true)
        {
        }

        public FontTexture(string fontFace, int fontSize, char[] renderableLetters, Device device, bool deviceValid)
        {
            // Determine if a fixed width font is passed
            System.Drawing.Font tempFont = new System.Drawing.Font(fontFace, fontSize);
            Bitmap b = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(b);
            if (g.MeasureString("iii", tempFont).Width != g.MeasureString("WWW", tempFont).Width)
                throw new Exception("Must use a fixed pitch font.");
            g.Dispose();

            _font = tempFont;

            // Initialise members
            _fontFace = fontFace;
            _fontSize = fontSize;

            if (renderableLetters != null)
                _renderableLetters = renderableLetters;

            _letters = new Dictionary<char, LetterInfo>();

            if (deviceValid)
                Rebuild(device);
        }

        public LetterInfo Letter(char letter)
        {
            if (_letters.ContainsKey(letter))
            {
                return _letters[letter];
            }
            else
            {
                return Letter(_unknownChar);
            }
        }

        public void Rebuild(Device device)
        {
            if (_valid || _building)
                return;

            _building = true;

            // Teardown
            this.Dispose();

            // Build the texture asynchronously so as to 
            // not delay the render
            Thread rebuildThread = new Thread(new ParameterizedThreadStart(RebuildAsync));
            rebuildThread.Priority = ThreadPriority.Lowest;
            rebuildThread.Start(device);
        }

        private void RebuildAsync(Object deviceObj)
        {
            Device device = (Device)deviceObj;

            try
            {
                // Work out how big each letter is - assuming a fixed pitch font
                Bitmap b = new Bitmap(1, 1);
                Graphics g = Graphics.FromImage(b);

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                SizeF charSize = g.MeasureString("█", _font);

                _letterHeight = (int)charSize.Height;
                _letterWidth = (int)charSize.Width;

                // Work out the best size for the texture
                int textureSideDimension = (int)Math.Ceiling(Math.Sqrt(_renderableLetters.Length));

                // Draw the texture
                b = new Bitmap(textureSideDimension * _letterWidth, textureSideDimension * _letterHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = Graphics.FromImage(b);

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, b.Width, b.Height));

                int i = 0;
                foreach (char letter in _renderableLetters)
                {
                    double ul = (double)i % (double)textureSideDimension / (double)textureSideDimension;
                    double ur = ((double)i % (double)textureSideDimension * (double)_letterWidth + ((double)_letterWidth - 1)) / ((double)textureSideDimension * (double)_letterWidth);
                    double vt = (double)(i / textureSideDimension) / (double)textureSideDimension;
                    double vb = ((double)(i / textureSideDimension) * (double)_letterHeight + ((double)_letterHeight - 1)) / ((double)textureSideDimension * (double)_letterHeight);
                    int x = i % textureSideDimension * _letterWidth;
                    int y = i / textureSideDimension * _letterHeight;

                    g.DrawString(letter.ToString(), _font, new SolidBrush(Color.White), new PointF(x, y));

                    LetterInfo letterInfo;
                    letterInfo.h = _letterHeight;
                    letterInfo.w = _letterWidth;
                    letterInfo.ul = ul;
                    letterInfo.ur = ur;
                    letterInfo.vt = vt;
                    letterInfo.vb = vb;

                    _letters.Add(letter, letterInfo);

                    i++;
                }

                _texture = Texture.FromBitmap(device, b, Usage.None, Pool.Managed);
                
                g.Dispose();

                _valid = true;
            }
            catch (Exception e)
            {
                _valid = false;
                _texture = null;
            }
            finally
            {
                _building = false;
            }
        }

        private string RenderableString()
        {
            StringBuilder str = new StringBuilder(_renderableLetters.Length);

            foreach (char c in _renderableLetters)
            {
                str.Append(c);
            }

            return str.ToString();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
        }

        #endregion
    }
}
