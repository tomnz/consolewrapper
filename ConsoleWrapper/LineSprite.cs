using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace ConsoleWrapper
{
    class LineSprite : IDisposable, IRenderable, IAnimatable
    {
        public static readonly int MaxLineWidth = 80;

        private readonly int mipLevels = 2;

        private Mesh _lineSprite;
        private Texture _lineTexture;
        public Texture LineTexture
        {
            get { return _lineTexture; }
        }
        private Material _lineMaterial;
        public Material LineMaterial
        {
            get { return _lineMaterial; }
        }

        private ConsoleString _line;
        private String _fontFace;
        private int _fontSize;

        private int _lineWidth;
        public int Width
        {
            get { return _lineWidth; }
        }
        private int _lineHeight;
        public int Height
        {
            get { return _lineHeight; }
        }

        // Allows for line invalidation to rebuild with the D3D device
        private bool _valid = false;
        private bool _building = false;

        // Animation for expanding lines
        private double _expandTime = 0.5f; // Time in seconds - set to 0 to disable
        private double _liveTime = 0;
        private double _widthFactor = 1;

        public LineSprite(ConsoleString line, String fontFace, int fontSize, Device device)
        {
            _line = line;
            _fontFace = fontFace;
            _fontSize = fontSize;

            Rebuild(device);
        }

        public LineSprite(ConsoleString line, String fontFace, int fontSize, Device device, bool deviceValid)
        {
            _line = line;
            _fontFace = fontFace;
            _fontSize = fontSize;

            if (deviceValid)
                Rebuild(device);
        }

        private string _displayString;

        public void Invalidate()
        {
            _valid = false;
        }

        // Rebuilds the object if it happens to get lost eg on a device reset
        public void Rebuild(Device device)
        {
            if (_valid || _building)
                return;

            _building = true;

            // Teardown
            this.Dispose();

            // Set up the line texture
            Bitmap b = new Bitmap(1, 1);
            System.Drawing.Font font = new System.Drawing.Font(_fontFace, _fontSize);

            Graphics g = Graphics.FromImage(b);

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            _displayString = "";
            string tempString = _line.Text;

            while (tempString.Length > MaxLineWidth)
            {
                _displayString += tempString.Substring(0, MaxLineWidth) + Environment.NewLine;
                tempString = tempString.Substring(MaxLineWidth);
            }
            _displayString += tempString;

            SizeF stringSize = g.MeasureString(_displayString, font);
            //Size stringSize = TextRenderer.MeasureText(_line.Text, font);

            _lineWidth = Size.Truncate(stringSize).Width;
            _lineHeight = Size.Truncate(stringSize).Height;

            Thread rebuildThread = new Thread(new ParameterizedThreadStart(RebuildAsync));
            rebuildThread.Priority = ThreadPriority.Lowest;
            rebuildThread.Start(device);
        }

        private void RebuildAsync(Object deviceObj)
        {
            try
            {
                Device device = (Device)deviceObj;

                // Rebuild
                if (_line.Equals(""))
                {
                    _lineSprite = null;
                    _valid = true;
                }
                else
                {
                    try
                    {
                        Animate(0);

                        int firstTick = Environment.TickCount;
                        System.Drawing.Font font = new System.Drawing.Font(_fontFace, _fontSize);
                        Bitmap b = new Bitmap(_lineWidth, _lineHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        Graphics g = Graphics.FromImage(b);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, b.Width, b.Height));

                        g.DrawString(_displayString, font, new SolidBrush(Color.White), new PointF(0, 0));
                        //TextRenderer.DrawText(g, _displayString, font, new Point(0, 0), Color.White);

                        _lineTexture = Texture.FromBitmap(device, b, Usage.Dynamic, Pool.Default);

                        //_lineTexture = new Texture(device, (int)_lineWidth * (int)Math.Pow(2.0, (double)mipLevels), (int)_lineHeight * (int)Math.Pow(2.0, (double)mipLevels), mipLevels + 1, Usage.RenderTarget, Format.Unknown, Pool.Default);

                        //Surface s = _lineTexture.GetSurfaceLevel(mipLevels);
                        //SurfaceLoader.FromSurface(s, Surface.FromBitmap(device, b, Pool.Default), Filter.Box, 0xFF0000);

                        g.Dispose();

                        //for (int i = 1; i <= mipLevels; i++)
                        //{
                        //    int width = _lineWidth * (int)Math.Pow(2.0, (double)i);
                        //    int height = _lineHeight * (int)Math.Pow(2, (double)i);

                        //    b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        //    font = new System.Drawing.Font(_fontFace, _fontSize * (float)Math.Pow(2, (double)i));

                        //    g = Graphics.FromImage(b);
                        //    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        //    g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, b.Width, b.Height));
                        //    g.DrawString(displayString, font, new SolidBrush(Color.White), new PointF(0, 0));

                        //    s = _lineTexture.GetSurfaceLevel(mipLevels - i);
                        //    SurfaceLoader.FromSurface(s, Surface.FromBitmap(device, b, Pool.Default), Filter.Box, 0xFF0000);

                        //    g.Dispose();
                        //}

                        // Set up the material
                        _lineMaterial = new Material();
                        _lineMaterial.Diffuse = _line.Color;
                        //_lineMaterial.Ambient = GetColor(_line.Type);

                        // Set up the rectangular mesh
                        CustomVertex.PositionNormalTextured[] verts = new CustomVertex.PositionNormalTextured[4];

                        verts[0].Position = new Vector3(0, 0, -_lineHeight);
                        verts[0].Normal = new Vector3(0, 1, 0);
                        verts[0].Tu = 0; verts[0].Tv = 1;

                        verts[1].Position = new Vector3(_lineWidth, 0, -_lineHeight);
                        verts[1].Normal = new Vector3(0, 1, 0);
                        verts[1].Tu = 1; verts[1].Tv = 1;

                        verts[2].Position = new Vector3(_lineWidth, 0, 0);
                        verts[2].Normal = new Vector3(0, 1, 0);
                        verts[2].Tu = 1; verts[2].Tv = 0;

                        verts[3].Position = new Vector3(0, 0, 0);
                        verts[3].Normal = new Vector3(0, 1, 0);
                        verts[3].Tu = 0; verts[3].Tv = 0;

                        AttributeRange[] attributes = new AttributeRange[1];
                        attributes[0].AttributeId = 0;
                        attributes[0].FaceCount = 2;
                        attributes[0].FaceStart = 0;
                        attributes[0].VertexCount = 4;
                        attributes[0].VertexStart = 0;

                        short[] indices = new short[]
                        {
                            0, 1, 2,
                            0, 2, 3
                        };

                        _lineSprite = new Mesh(2, 4, 0, CustomVertex.PositionNormalTextured.Format, device);

                        _lineSprite.SetVertexBufferData(verts, LockFlags.Discard);
                        _lineSprite.SetIndexBufferData(indices, LockFlags.Discard);
                        _lineSprite.SetAttributeTable(attributes);

                        int[] adjacency = new int[_lineSprite.NumberFaces * 3];
                        _lineSprite.GenerateAdjacency(0.01F, adjacency);
                        _lineSprite.OptimizeInPlace(MeshFlags.OptimizeVertexCache, adjacency);

                        _valid = true;
                    }
                    catch (Exception e)
                    {
                        _valid = false;
                        _lineTexture = null;
                        _lineSprite = null;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                _valid = false;
                _lineTexture = null;
                _lineSprite = null;
            }
            finally
            {
                _building = false;
            }
        }

        public void Animate(double time)
        {
            _liveTime += time;

            if (_liveTime > _expandTime)
            {
                _widthFactor = 1;
            }
            else
            {
                // Gompertz curve
                double a, b, c, t;

                a = 1;
                b = -0.5;
                c = -1;
                t = ((_liveTime) / _expandTime) * 6;

                _widthFactor = a * Math.Exp(b * Math.Exp(c * t));
            }
        }

        public void Draw(Device device)
        {
            if (!_valid && !_building)
                Rebuild(device);

            if (!_valid || _building)
                return;

            if (_lineSprite != null)
            {
                // Set up the expansion
                Matrix worldBackup = device.Transform.World;
                if (_widthFactor != 1)
                {
                    device.Transform.World *= Matrix.Scaling(new Vector3((float)_widthFactor, 1, 1));
                }

                // Set the material and texture
                device.Material = _lineMaterial;
                device.SetTexture(0, _lineTexture);

                int numSubSets = _lineSprite.GetAttributeTable().Length;


                // Mip maps
                //device.SamplerState[0].MipFilter = TextureFilter.Anisotropic;

                // Draw the primitive
                for (int i = 0; i < numSubSets; i++)
                {
                    try
                    {
                        _lineSprite.DrawSubset(i);
                    }
                    catch (Exception)
                    {
                        _valid = false;
                    }
                }

                // Restore the world matrix
                device.Transform.World = worldBackup;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_lineTexture != null)
            {
                _lineTexture.Dispose();
                _lineTexture = null;
            }
            if (_lineSprite != null)
            {
                _lineSprite.Dispose();
            }
        }

        #endregion
    }
}
