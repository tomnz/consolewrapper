using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace ConsoleWrapper
{
    public partial class DXGUI : Form, IWrapperListener
    {
        private WrapperGraphics _graphics;
        private IWrapper _wrapper;
        private IList<ConsoleString> _currentLines = new List<ConsoleString>();
        private IList<string> _prevLines = new List<string>();
        private int _prevLineNum = 0;
        private StringBuilder _currentLine;
        private StringBuilder _currentInput;
        private int _currentInputLocation;
        private bool _render = true;

        delegate void CloseForm();
        
        public const int LINEWIDTH = 80;

        public DXGUI()
        {
            InitializeComponent();
            _currentLine = new StringBuilder();
            _prevLines.Add("");
            _currentInput = new StringBuilder();
            _currentInputLocation = 0;
            _graphics = new WrapperGraphics(this);
            _graphics.AddLine(new ConsoleString("Welcome to the DirectX Console Wrapper!", Color.FromArgb(255, 63, 63)));
            _graphics.AddLine(new ConsoleString("Use Ctrl-(Up/Down/Home/End) :: PageUp/PageDown :: MouseWheel for navigation.", Color.FromArgb(255, 127, 0)));
            _graphics.AddLine(new ConsoleString("ConsoleWrapper Copyright (c) Tom Mitchell 2007-2008", Color.FromArgb(255, 255, 0)));
            _graphics.AddLine(new ConsoleString(" "));
            _wrapper = new WrapperShell();
            _wrapper.AddListener(this);
        }

        public void Render()
        {
            if (_graphics == null || !_render)
                return;
            else
                _graphics.Render();
        }

        private void ProcessText()
        {
            lock (_currentLines)
            {
                if (_currentLines.Count > 0)
                {
                    ConsoleString[] lines = new ConsoleString[_currentLines.Count];
                    _currentLines.CopyTo(lines, 0);

                    foreach (ConsoleString line in lines)
                    {
                        if (line.Finalised)
                        {
                            _graphics.AddLine(line);
                            _currentLines.Remove(line);
                        }
                    }
                }

                UpdateCurrentLine();
            }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            Render(); // Render on painting
        }

        public void TextReady(IWrapper sender)
        {
            lock (_currentLines)
            {
                _currentLine = new StringBuilder(_wrapper.GetCurrentLine());

                ConsoleString[] strings = sender.GetText();
                foreach (ConsoleString str in strings)
                {
                    _currentLines.Add(str);
                }
            }
            ProcessText();
        }

        public void WrapperFinished(IWrapper sender)
        {
            WrapperFinished();
        }

        private void WrapperFinished()
        {
            if (this.InvokeRequired)
            {
                CloseForm closeForm = new CloseForm(WrapperFinished);
                this.Invoke(closeForm, null);
            }
            else
            {
                this.Close();
            }
        }

        private void UpdateCurrentLine()
        {
            _graphics.CurrentLine.Text = CurrentLineWithCaret();
        }

        private string CurrentLineWithCaret()
        {
            return _currentLine.ToString() + _currentInput.ToString().Insert(_currentInputLocation, "|");
        }

        private void DXGUI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter) || e.KeyCode.Equals(Keys.Return))
            {
                _prevLines.Insert(_prevLines.Count - 1, _currentInput.ToString());
                _prevLineNum = _prevLines.Count - 1;
                _wrapper.SendLine(_currentInput.ToString(), ConsoleString.StringType.Input);
                _currentInput = new StringBuilder();
                _currentInputLocation = 0;
            }
            else if (e.KeyCode.Equals(Keys.Back))
            {
                if (_currentInput.Length > 0 && _currentInputLocation > 0)
                {
                    _currentInput.Remove(_currentInputLocation - 1, 1);
                    _currentInputLocation--;
                    UpdateCurrentLine();
                }
            }
            else if (e.KeyCode.Equals(Keys.Delete))
            {
                if (_currentInput.Length > 0 && _currentInputLocation < _currentInput.Length)
                {
                    _currentInput.Remove(_currentInputLocation, 1);
                    UpdateCurrentLine();
                }
            }
            else if (e.KeyCode.Equals(Keys.Left))
            {
                if (_currentInputLocation > 0)
                {
                    _currentInputLocation--;
                    UpdateCurrentLine();
                }
            }
            else if (e.KeyCode.Equals(Keys.Right))
            {
                if (_currentInputLocation < _currentInput.Length)
                {
                    _currentInputLocation++;
                    UpdateCurrentLine();
                }
            }
            else if (e.Control && e.KeyCode.Equals(Keys.Home))
            {
                _graphics.MoveViewHome();
            }
            else if (e.Control && e.KeyCode.Equals(Keys.End))
            {
                _graphics.MoveViewEnd();
            }
            else if (e.KeyCode.Equals(Keys.Home))
            {
                _currentInputLocation = 0;
                UpdateCurrentLine();
            }
            else if (e.KeyCode.Equals(Keys.End))
            {
                _currentInputLocation = _currentInput.Length;
                UpdateCurrentLine();
            }
            else if (e.Control && e.KeyCode.Equals(Keys.Up))
            {
                _graphics.MoveView(-2);
            }
            else if (e.Control && e.KeyCode.Equals(Keys.Down))
            {
                _graphics.MoveView(2);
            }
            else if (e.KeyCode.Equals(Keys.Up))
            {
                _prevLineNum--;
                _prevLineNum = Math.Max(0, _prevLineNum);
                _currentInput = new StringBuilder(_prevLines[_prevLineNum]);
                _currentInputLocation = _currentInput.Length;
                _graphics.CurrentLine.Text = CurrentLineWithCaret();
            }
            else if (e.KeyCode.Equals(Keys.Down))
            {
                _prevLineNum++;
                _prevLineNum = Math.Min(_prevLines.Count - 1, _prevLineNum);
                _currentInput = new StringBuilder(_prevLines[_prevLineNum]);
                _currentInputLocation = _currentInput.Length;
                _graphics.CurrentLine.Text = CurrentLineWithCaret();
            }
            else if (e.KeyCode.Equals(Keys.PageUp))
            {
                _graphics.MoveView(-30);
            }
            else if (e.KeyCode.Equals(Keys.PageDown))
            {
                _graphics.MoveView(30);
            }
            else if (e.Control && e.KeyCode.Equals(Keys.C))
            {
                DirectoryInfo currentDirectory = ((WrapperShell)_wrapper).CurrentDirectory;
                _wrapper.Dispose();
                _wrapper = new WrapperShell(currentDirectory);
                _wrapper.AddListener(this);
            }
        }

        private void DXGUI_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar) || char.IsSymbol(e.KeyChar) || e.KeyChar == ' ')
            {
                if (e.KeyChar == '\\')
                {
                    _currentInput.Insert(_currentInputLocation, '\\');
                }
                else
                {
                    _currentInput.Insert(_currentInputLocation, e.KeyChar);
                }
                _currentInputLocation++;
            }
            UpdateCurrentLine();
        }

        private void DXGUI_ResizeBegin(object sender, EventArgs e)
        {
            _render = false;
        }

        private void DXGUI_ResizeEnd(object sender, EventArgs e)
        {
            _render = true;
        }

        private void DXGUI_Move(object sender, EventArgs e)
        {
            if (_graphics != null)
                _graphics.ResetTimer();
        }

        private void DXGUI_MouseWheel(object sender, MouseEventArgs e)
        {
            _graphics.MoveView(Math.Sign(e.Delta) * -3);
        }

        private void DXGUI_Resize(object sender, EventArgs e)
        {
            _graphics.InitializeGraphics(this);
        }

        private void DXGUI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Tab))
            {
                //_wrapper.SendKey(e, true);
            }

        }

    }
}