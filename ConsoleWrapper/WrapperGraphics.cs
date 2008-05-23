using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;

namespace ConsoleWrapper
{
    class WrapperGraphics : IDisposable
    {
        // D3D device
        private Device _device;
        private PresentParameters _presentParams;

        private Font _uiFont;
        private String _fontFace = "Lucida Console";
        private int _fontSize = 12;
        private int _lineGap = 5;
        private bool _paused = false;
        private bool _deviceLost = false;

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
        private IList<LineSprite> _lines;
        
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
        private float _camLookAtDiff = 290.0f;
        private float _camLocationDiff = 290.0f;
        // The current line that the camera is at
        private int _viewLine = 0;
        // Keeps record of the number of lines
        private int _numLines = 0;
        // The last line which has a line in the buffer
        private int _minLine = 0;
        // The number of lines to keep in the buffer
        private int _bufferSize = 500;

        public WrapperGraphics(Control parent)
        {
            _lines = new List<LineSprite>();

            if (!InitializeGraphics(parent))
            {
                MessageBox.Show("Error: DirectX failed to initialize.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                this.OnCreateDevice(_device, null);

                if (_camera == null)
                    _camera = new Camera(new Vector3(_viewX, _viewY, _camLocationDiff), new Vector3(_viewX, 0, _camLookAtDiff), (float)parent.Width / parent.Height);
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

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.Lighting = true;
            dev.RenderState.CullMode = Cull.Clockwise;
            dev.RenderState.ZBufferEnable = true;
            dev.RenderState.DitherEnable = true;
            dev.RenderState.SpecularEnable = true;
            
            //dev.Lights[0].Type = LightType.Directional;
            //dev.Lights[0].Diffuse = System.Drawing.Color.White;
            //dev.Lights[0].Direction = new Vector3(0.5f, -1.0f, -0.5f);
            //dev.Lights[0].Enabled = true;

            //dev.Lights[1].Type = LightType.Spot;
            //dev.Lights[1].Diffuse = System.Drawing.Color.White;
            //dev.Lights[1].InnerConeAngle = (float)Math.PI / 8.0f;
            //dev.Lights[1].OuterConeAngle = (float)Math.PI / 2.0f;
            //dev.Lights[1].Attenuation1 = 0.0f;
            //dev.Lights[1].Falloff = 0.0f;
            //dev.Lights[1].Range = 4500;
            //dev.Lights[1].Enabled = true;

            _uiFont = new Font(_device, 11, 0, FontWeight.Normal, 0, false, CharacterSet.Default, Precision.Default, FontQuality.ClearType, PitchAndFamily.DefaultPitch, _fontFace);

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

        public void OnLostDevice(object sender, EventArgs e)
        {
            _deviceLost = true;
            //OnDeviceDispose(sender, e);
            RecoverDevice();
        }

        public void OnDeviceDispose(object sender, EventArgs e)
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
            if (_uiFont != null)
            {
                _uiFont.Dispose();
                //_uiFont = null;
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
                    foreach (LineSprite line in _lines)
                    {
                        line.Animate((float)frameTime);
                    }
                }
            }
            else 
            { // Do something 
            }

            // Setup the view
            _camera.SetupMatrices(_device);

            //_device.Transform.View = Matrix.LookAtLH(new Vector3(0, 3000, -1000),
            //    new Vector3(0, 0, -1000), new Vector3(0.0f, 0.0f, 1.0f));
            //_device.Transform.Projection = Matrix.PerspectiveFovLH((float)(Math.PI / 4), 1.0f, 1.0f, 5000.0f);
            //_device.Lights[0].Type = LightType.Directional;
            //_device.Lights[0].Direction = new Vector3(0, -1, 0);
            //_device.Lights[0].Diffuse = System.Drawing.Color.Red;
            //_device.Lights[0].Enabled = true;

            // Get the viewing frustum
            Plane[] viewingFrustum = _camera.GetFrustum();
            
            int numLinesRendered = 0;

            lock (_lines)
            {
                foreach (LineSprite line in _lines)
                {
                    _device.Transform.World = transform * baseLine;

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
            //_uiFont.DrawText(null, numLinesRendered.ToString(), 10, 10, System.Drawing.Color.Red);

            // End the scene
            _device.EndScene();
            
            try
            {
                _device.Present();
            }
            catch (DeviceLostException)
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
                    _device.Reset(_presentParams);
                    _deviceLost = false;
                }
                catch (DeviceLostException)
                {
                    // If it's still lost or lost again, just do 
                    // nothing
                }
            }
        }

        public void AddLine(ConsoleString line)
        {
            if (line.Text == "")
                line.Text = " ";

            LineSprite lineSprite = new LineSprite(line, _fontFace, _fontSize, _device, !_deviceLost);

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
        }
    }
}
