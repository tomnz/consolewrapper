using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleWrapper
{
    public interface IWrapperListener
    {
        void TextReady(IWrapper sender);
        void WrapperFinished(IWrapper sender);
    }
}
