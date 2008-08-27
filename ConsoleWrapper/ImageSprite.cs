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
    class ImageSprite : Renderable, IDisposable
    {
        private Mesh _imageSprite;
        private Texture _imageTexture;
        public Texture ImageTexture
        {
            get { return _imageTexture; }
        }
        private Material _imageMaterial;
        public Material ImageMaterial
        {
            get { return _imageMaterial; }
        }

        private int _width;
        public override int Width
        {
            get { return _width; }
        }
        private int _height;
        public override int Height
        {
            get { return _height; }
        }

        private Color _color = Color.White;

        private Bitmap _bitmap = new Bitmap(1, 1);

        // Allows for line invalidation to rebuild with the D3D device
        private bool _valid = false;
        private bool _building = false;

        public ImageSprite(Bitmap bitmap, Device device)
        {
            _bitmap = bitmap;

            Rebuild(device);
        }

        public ImageSprite(Bitmap bitmap, Device device, Color color)
        {
            _color = color;
            _bitmap = bitmap;

            Rebuild(device);
        }

        public ImageSprite(Bitmap bitmap, Device device, Color color, bool deviceValid)
        {
            _bitmap = bitmap;
            _color = color;

            if (deviceValid)
                Rebuild(device);
        }

        public override void Invalidate()
        {
            _valid = false;
        }

        // Rebuilds the object if it happens to get lost eg on a device reset
        public override void Rebuild(Device device)
        {
            if (_valid || _building)
                return;

            _building = true;

            // Teardown
            this.Dispose();

            if (_bitmap == null)
                return;

            lock (_bitmap)
            {
                _width = Size.Truncate(_bitmap.Size).Width;
                _height = Size.Truncate(_bitmap.Size).Height;
            }

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
                if (_bitmap == null)
                {
                    _bitmap = new Bitmap(1, 1);
                    _valid = true;
                }
                else
                {
                    try
                    {
                        _imageTexture = Texture.FromBitmap(device, _bitmap, Usage.Dynamic, Pool.Default);

                        // Set up the material
                        _imageMaterial = new Material();
                        _imageMaterial.Diffuse = _color;

                        // Set up the rectangular mesh
                        CustomVertex.PositionNormalTextured[] verts = new CustomVertex.PositionNormalTextured[4];

                        verts[0].Position = new Vector3(0, 0, -_height);
                        verts[0].Normal = new Vector3(0, 1, 0);
                        verts[0].Tu = 0; verts[0].Tv = 1;

                        verts[1].Position = new Vector3(_width, 0, -_height);
                        verts[1].Normal = new Vector3(0, 1, 0);
                        verts[1].Tu = 1; verts[1].Tv = 1;

                        verts[2].Position = new Vector3(_width, 0, 0);
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

                        _imageSprite = new Mesh(2, 4, 0, CustomVertex.PositionNormalTextured.Format, device);

                        _imageSprite.SetVertexBufferData(verts, LockFlags.Discard);
                        _imageSprite.SetIndexBufferData(indices, LockFlags.Discard);
                        _imageSprite.SetAttributeTable(attributes);

                        _valid = true;
                    }
                    catch (Exception)
                    {
                        _valid = false;
                        _imageTexture = null;
                        _imageSprite = null;
                        return;
                    }
                }
            }
            catch (Exception)
            {
                _valid = false;
                _imageTexture = null;
                _imageSprite = null;
            }
            finally
            {
                _building = false;
            }
        }

        public override void Draw(Device device)
        {
            if (!_valid && !_building)
                Rebuild(device);

            if (!_valid || _building)
                return;

            if (_imageSprite != null)
            {
                // Set the material and texture
                device.Material = _imageMaterial;
                device.SetTexture(0, _imageTexture);

                int numSubSets = _imageSprite.GetAttributeTable().Length;

                // Draw the primitive
                for (int i = 0; i < numSubSets; i++)
                {
                    try
                    {
                        _imageSprite.DrawSubset(i);
                    }
                    catch (Exception)
                    {
                        _valid = false;
                    }
                }
            }
        }

        #region IDisposable Members

        public override void Dispose()
        {
            if (_imageTexture != null)
            {
                _imageTexture.Dispose();
                _imageTexture = null;
            }
            if (_imageSprite != null)
            {
                _imageSprite.Dispose();
            }
        }

        #endregion
    }
}
