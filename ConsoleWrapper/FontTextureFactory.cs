using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace ConsoleWrapper
{
    class FontTextureFactory
    {
		private static IDictionary<string, FontTexture> _fontTextures;
		private static FontTexture _lastFontTexture;

		public static int LastLetterHeight
		{
			get 
			{
				if (_lastFontTexture != null)
				{
					return _lastFontTexture.LetterHeight;
				}
				else
				{
					return 0;
				}
			}
		}

        public static FontTexture GetFontTexture(string fontFace, int fontSize, Device device)
        {
            if (_fontTextures == null)
            {
                _fontTextures = new Dictionary<string, FontTexture>();
            }

            string fontKey = fontFace + "_" + fontSize + "_" + device.GetHashCode();
            if (!_fontTextures.ContainsKey(fontKey))
            {
				_lastFontTexture = new FontTexture(fontFace, fontSize, device);
				_fontTextures.Add(fontKey, _lastFontTexture);
            }

            return _fontTextures[fontKey];
        }

        public static void Rebuild(Device device)
        {
            if (_fontTextures == null)
                return;

            foreach (FontTexture font in _fontTextures.Values)
            {
                font.Invalidate();
                font.Rebuild(device);
            }
        }

        public static void DisposeTextures()
        {
            if (_fontTextures == null)
                return;

            string[] keys = new string[_fontTextures.Keys.Count];
            _fontTextures.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                _fontTextures[key].Dispose();
                _fontTextures[key] = null;
            }

			_lastFontTexture = null;
        }
    }
}
