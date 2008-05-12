using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleWrapper
{
    public interface IWrapper : IDisposable
    {
        void AddListener(IWrapperListener listener);
        ConsoleString[] GetText();
        ConsoleString[] PeekText();
        string GetCurrentLine();
        void SendLine(String line, ConsoleString.StringType type);
        void SendCharacter(char character, bool flush);
        void Send(String text, bool flush);
    }
}
