using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace ConsoleWrapper
{
    abstract class Renderable
    {
        public abstract void Draw(Device device);
        public abstract void Invalidate();
        public abstract void Rebuild(Device device);
        public abstract void Dispose();
        public virtual int Width { get { return 0; } }
        public virtual int Height { get { return 0; } }
    }
}
