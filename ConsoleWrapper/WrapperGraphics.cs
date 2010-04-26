using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.IO;

namespace ConsoleWrapper
{
    class WrapperGraphics : IDisposable
    {
        // D3D device
        private Device _device;
        private PresentParameters _presentParams;

        private Microsoft.DirectX.Direct3D.Font _uiFont;
        private String _fontFace = "Lucida Console";
        private int _fontSize = 12;
        private int _lineGap = 5;
		private int _maxImageWidth = 800;
		private int _maxImageHeight = 800;
        private bool _paused = false;
        private bool _deviceLost = false;

        private bool _recovering = false;

        public bool Paused
        {
            get 
            {
                return _paused; 
            }
            set 
            {
                Render();
                _paused = value; 
            }
        }

        public bool IsValid
        {
            get
            {
                if (_deviceLost)
                    return false;
                else
                    return true;
            }
        }

        // Helper classes
        private Camera _camera;
        private GraphicsTimer _timer;

        // All the lines which have been output so far
        private IList<Renderable> _lines;
        
        // Keeps record of the current line being input
        private ConsoleString _currentLine = new ConsoleString("");
        public ConsoleString CurrentLine
        {
            get { return _currentLine; }
            set { _currentLine = value; }
        }
        
        // Used to keep the console centered and at the correct
        // width in the view
        private float _viewY = 780.2f; // Within the view
        private float _viewX = 400.0f; // Centred
        // The following two are used to make sure the text
        // goes to the bottom of the view and that we are
        // viewing at an angle
        private float _camLookAtDiff = 270.0f;
        private float _camLocationDiff = 270.0f;
        // The current line that the camera is at
        private int _viewLine = 0;
        // Keeps record of the number of lines
        private int _numLines = 0;
        // The last line which has a line in the buffer
        private int _minLine = 0;
        // The number of lines to keep in the buffer
        private int _bufferSize = 500;

		// Helper mesh for rendering letters
		private static Mesh _letterSprite = null;
		public static Mesh LetterSprite
		{
			get { return _letterSprite; }
		}
		
        public WrapperGraphics(Control parent)
        {
            _lines = new List<Renderable>();

            if (!InitializeGraphics(parent))
            {
                MessageBox.Show("Error: DirectX failed to initialize.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {

                System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.Stream logoStream = ass.GetManifestResourceStream("ConsoleWrapper.Resources.logoImage.gif");

                if (logoStream != null)
                {
                    // Display logo
                    Bitmap b = (Bitmap)Bitmap.FromStream(logoStream);
                    _lines.Add(new ImageSprite(b, _device));
                    _numLines++;
                }
            }
        }

        public bool InitializeGraphics(Control parent)
        {
            try
            {
                _presentParams = new PresentParameters();
                _presentParams.Windowed = true;
                _presentParams.SwapEffect = SwapEffect.Discard;
                _presentParams.EnableAutoDepthStencil = true;
                _presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                _device = new Device(0, DeviceType.Hardware, parent, CreateFlags.SoftwareVertexProcessing, _presentParams);
                _device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                _device.DeviceLost += new System.EventHandler(this.OnLostDevice);
                _device.Disposing += new System.EventHandler(this.OnDeviceDispose);
                _device.DeviceResizing += new System.ComponentModel.CancelEventHandler(this.OnResizeDevice);

                this.OnCreateDevice(_device, null);

                if (_camera == null)
                    _camera = new Camera(new Vector3(_viewX, _viewY, _camLocationDiff), new Vector3(_viewX, 0, _camLookAtDiff), (float)parent.Width / parent.Height, parent.Width, parent.Height);
                else
                    _camera.AspectRatio = (float)parent.Width / parent.Height;

                return true;
            }
            catch (DirectXException)
            {
                return false;
            }
        }
        
        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            _timer = new GraphicsTimer();

            this.OnResetDevice(sender, e);
        }

        public void OnResizeDevice(object sender, CancelEventArgs e)
        {
            // Having DirectX resize for us causes problems - 
            // we will handle resizing ourselves
            e.Cancel = true;
        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            if (_deviceLost) return;

            Device dev = (Device)sender;
            dev.RenderState.Lighting = true;
            dev.RenderState.CullMode = Cull.Clockwise;
            dev.RenderState.ZBufferEnable = true;
            dev.RenderState.DitherEnable = true;

            _uiFont = new Microsoft.DirectX.Direct3D.Font(_device, 11, 0, FontWeight.Normal, 0, false, CharacterSet.Default, Precision.Default, FontQuality.ClearType, PitchAndFamily.DefaultPitch, _fontFace);
			_letterSprite = new Mesh(2, 4, 0, CustomVertex.PositionNormalTextured.Format, dev);

            lock (_lines)
            {
                if (_lines != null)
                {
                    for (int i = 0; i < _lines.Count; i++)
                    {
                        if (_lines[i] != null)
                        {
                            _lines[i].Invalidate();
                        }
                    }
                }
            }

            FontTextureFactory.Rebuild(dev);

            ResetTimer();
        }

        public void OnLostDevice(object sender, EventArgs e)
        {
            if (_recovering) return;

            _deviceLost = true;

            _recovering = true;
            RecoverDevice();
            _recovering = false;
        }

        public void OnDeviceDispose(object sender, EventArgs e)
        {
            lock (_lines)
            {
                if (_lines != null)
                {
                    for (int i = 0; i < _lines.Count; i++)
                    {
                        if (_lines[i] != null)
                        {
                            _lines[i].Dispose();
                            _lines[i] = null;
                        }
                    }
                }
            }
            if (_uiFont != null)
            {
                _uiFont.Dispose();
                //_uiFont = null;
            }
			if (_letterSprite != null)
			{
				_letterSprite.Dispose();
				_letterSprite = null;
			}
        }

        public void Render()
        {
            if (_device == null)
                return;

            if (_deviceLost)
                RecoverDevice();

            if (_deviceLost)
                return;

            double frameTime = _timer.TickFrame();

            //Clear the backbuffer
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Black, 1.0f, 0);

            //Begin the scene
            _device.BeginScene();

            _device.Transform.World = Matrix.Identity;
            Matrix transform = Matrix.Identity;
            Matrix baseLine = Matrix.Translation(0, 0, 0 - _minLine);

            // Animation
            if (!_paused)
            {
                _camera.Animate((float)frameTime, _device.Lights[1]);

                lock (_lines)
                {
                    foreach (Object line in _lines)
                    {
                        if (line is IAnimatable)
                            ((IAnimatable)line).Animate((float)frameTime);
                    }
                }
            }
            else 
            { // Do something 
            }

            // Setup the view
            _camera.SetupMatrices(_device);

            // Get the viewing frustum
            Plane[] viewingFrustum = _camera.GetFrustum();
            
            int numLinesRendered = 0;

            lock (_lines)
            {
                foreach (Renderable line in _lines)
                {
                    _device.Transform.World = transform * baseLine;

                    // Get some minification happening
                    if (_device.DeviceCaps.TextureFilterCaps.SupportsMinifyAnisotropic)
                        _device.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
                    else if (_device.DeviceCaps.TextureFilterCaps.SupportsMinifyGaussianQuad)
                        _device.SamplerState[0].MinFilter = TextureFilter.GaussianQuad;
                    else if (_device.DeviceCaps.TextureFilterCaps.SupportsMinifyLinear)
                        _device.SamplerState[0].MinFilter = TextureFilter.Linear;

                    // Stop texture wrapping
                    if (_device.DeviceCaps.TextureAddressCaps.SupportsClamp)
                    {
                        _device.SamplerState[0].AddressU = TextureAddress.Clamp;
                        _device.SamplerState[0].AddressV = TextureAddress.Clamp;
                    }
                    else if (_device.DeviceCaps.TextureAddressCaps.SupportsBorder)
                    {
                        _device.SamplerState[0].AddressU = TextureAddress.Border;
                        _device.SamplerState[0].AddressV = TextureAddress.Border;
                    }

                    if (!CullLine(viewingFrustum, Vector3.TransformCoordinate(new Vector3(0, 0, -line.Height), transform), Vector3.TransformCoordinate(new Vector3(line.Width, 0, 0), transform)))
                    {
                        line.Draw(_device);
                        numLinesRendered++;
                    }

                    transform *= Matrix.Translation(0, 0, -1 * (line.Height + _lineGap));
                }
            }
            
            // Display textual information
            _uiFont.DrawText(null, _currentLine.Text, 
                new System.Drawing.Rectangle(10, _device.PresentationParameters.BackBufferHeight - 40,
                    _device.PresentationParameters.BackBufferWidth - 20, 30),
                DrawTextFormat.Left | DrawTextFormat.WordBreak | DrawTextFormat.ExpandTabs,
                _currentLine.Color);
            _uiFont.DrawText(null, "FPS: " + _timer.FPS.ToString("0.00"), _device.PresentationParameters.BackBufferWidth - 85, 10, System.Drawing.Color.LightSeaGreen);
            _uiFont.DrawText(null, numLinesRendered.ToString(), 10, 10, System.Drawing.Color.Red);

            // End the scene
            _device.EndScene();
            
            try
            {
                _device.Present();
            }
            catch (Exception)
            {
                _deviceLost = true;
                _paused = true;
            }
        }

        private void RecoverDevice()
        {
            try
            {
                _device.TestCooperativeLevel();
                _deviceLost = false;
            }
            catch (DeviceLostException)
            {
            }
            catch (DeviceNotResetException)
            {
                try
                {
                    _deviceLost = false;
                    _paused = false;
                    _device.Reset(_presentParams);
                }
                catch (DeviceLostException)
                {
                    _deviceLost = true;
                    _paused = true;
                }
            }
        }

        public void AddLine(ConsoleString line)
        {
            if (line.Text == "")
                line.Text = " ";

			Renderable lineSprite = null;

			if (line.Type == ConsoleString.StringType.Image)
			{
				Bitmap b = (Bitmap)Bitmap.FromFile(line.Text);
				lineSprite = new ImageSprite(b, _device, _maxImageWidth, _maxImageHeight);
			}
			else
			{
				lineSprite = new LineSprite(line, _fontFace, _fontSize, _device, !_deviceLost);
			}

			lock (_lines)
			{
				_lines.Add(lineSprite);
				_numLines++;
				_viewLine = _numLines;
			}
			
			SetCameraLocation();
        }

        // Returns true if the line is outside the frustum
        private bool CullLine(Plane[] frustum, Vector3 bbMin, Vector3 bbMax)
        {
            bool intersect = false;
            bool result = true;
            Vector3 minExtreme, maxExtreme;
            
            for (int i = 0; i < 6; i++)
            {
                //if (frustum[i].A >= 0)
                //{
                //    minExtreme.X = bbMin.X;
                //    maxExtreme.X = bbMax.X;
                //}
                //else
                //{
                //    minExtreme.X = bbMax.X;
                //    maxExtreme.X = bbMin.X;
                //}

                minExtreme.X = _viewX;
                maxExtreme.X = _viewX;

                if (frustum[i].B >= 0)
                {
                    minExtreme.Y = bbMin.Y;
                    maxExtreme.Y = bbMax.Y;
                }
                else
                {
                    minExtreme.Y = bbMax.Y;
                    maxExtreme.Y = bbMin.Y;
                }

                if (frustum[i].C >= 0)
                {
                    minExtreme.Z = bbMin.Z;
                    maxExtreme.Z = bbMax.Z;
                }
                else
                {
                    minExtreme.Z = bbMax.Z;
                    maxExtreme.Z = bbMin.Z;
                }

                if (frustum[i].Dot(maxExtreme) < 0)
                {
                    result = true;
                    return result;
                }

                if (frustum[i].Dot(minExtreme) <= 0)
                    intersect = false;
            }

            if (intersect)
                result = true;
            else
                result = false;

            return result;
        }

        private void SetCameraLocation()
        {
            int viewZ = 0;
            for (int i = 0; i < _viewLine; i++)
            {
                viewZ += _lines[i].Height + _lineGap;
            }

            _camera.TargetLocation = new Vector3(_viewX, _viewY, _camLocationDiff - (viewZ));
            _camera.TargetLookAt = new Vector3(_viewX, 0, _camLookAtDiff - (viewZ));

            // Make sure that we are looking at the right number of lines
            if (_lines.Count > _bufferSize)
            {
                int heightRemoved = 0;
                lock (_lines)
                {
                    while (_lines.Count > _bufferSize)
                    {
                        heightRemoved += _lines[0].Height + _lineGap;
                        _lines.RemoveAt(0);
                        _viewLine--;
                        _numLines--;
                        //_minLine++;
                    }
                }

                _camera.MoveCamera(new Vector3(0, 0, heightRemoved));
            }
        }

        public void MoveView(int numLines)
        {
            _viewLine += numLines;
            if (_viewLine < _minLine)
            {
                _viewLine = _minLine;
            }
            if (_viewLine > _numLines)
            {
                _viewLine = _numLines;
            }

            SetCameraLocation();
        }

        public void MoveViewHome()
        {
            _viewLine = _minLine;
            SetCameraLocation();
        }

        public void MoveViewEnd()
        {
            _viewLine = _numLines;
            SetCameraLocation();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
        }

        #endregion

        internal void ResetTimer()
        {
            _timer.Reset();
            _timer.TickFrame();
        }
    }
}
