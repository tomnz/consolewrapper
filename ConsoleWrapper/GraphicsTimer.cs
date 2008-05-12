using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ConsoleWrapper
{
    class GraphicsTimer
    {
        private double _lastFrame;
        private int _lastTime;
        private double _deltaTime;
        private double _currentFrame;
        private int _currentTime;
        private double _fps;
        public double FPS
        {
            get { return _fps; }
        }

        public GraphicsTimer()
        {
            _lastFrame = 0;
            _lastTime = Environment.TickCount;
            _currentFrame = 0;
            _currentTime = _lastTime;
            _fps = 0;
        }

        public double TickFrame()
        {
            int tempTime = Environment.TickCount;
            _deltaTime = (double)(tempTime - _currentTime) / 1000.0;
            _currentFrame++;
            _currentTime = tempTime;

            // Calculate FPS - minimum of every 2 seconds or every 60 frames
            if (_currentTime - _lastTime > 2000 || _currentFrame - _lastFrame > 60)
            {
                _fps = (double)(_currentFrame - _lastFrame) / ((double)(_currentTime - _lastTime) / 1000.0);
                _lastTime = _currentTime;
                _lastFrame = _currentFrame;
            }

            return _deltaTime;
        }

        public void Reset()
        {
            _lastTime = Environment.TickCount;
            _lastFrame = 0;
            _currentFrame = 0;
            _currentTime = 0;
        }
    }
}
