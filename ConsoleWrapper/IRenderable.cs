using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace ConsoleWrapper
{
    interface IRenderable
    {
        void Draw(Device device);
        void Invalidate();
        void Rebuild(Device device);
    }
}
