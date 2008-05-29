using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private StringBuilder _currentLine;
        private StringBuilder _currentInput;
        private bool _render = true;

        delegate void CloseForm();
        
        public const int LINEWIDTH = 80;

        public DXGUI()
        {
            InitializeComponent();
            _currentLine = new StringBuilder();
            _currentInput = new StringBuilder();
            _graphics = new WrapperGraphics(this);
            _graphics.AddLine(new ConsoleString("Welcome to the DirectX Console Wrapper!", Color.FromArgb(255, 63, 63)));
            _graphics.AddLine(new ConsoleString("Use Ctrl-(Up/Down/Home/End) :: PageUp/PageDown :: MouseWheel for navigation.", Color.FromArgb(255, 127, 0)));
            _graphics.AddLine(new ConsoleString("ConsoleWrapper Copyright (c) Tom Mitchell 2007-2008", Color.FromArgb(255, 255, 0)));
            _graphics.AddLine(new ConsoleString(" "));
            _wrapper = new Wrapper("cmd.exe");
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
                if (_currentLines.Count > 1)
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

                _graphics.CurrentLine = new ConsoleString(_currentLine.ToString() + _currentInput.ToString(), ConsoleString.StringType.Normal);
            }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (_render)
                this.Render(); // Render on painting
        }

        #region IWrapperListener Members

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

        public void WrapperFinished()
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

        #endregion

        private void DXGUI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter) || e.KeyCode.Equals(Keys.Return))
            {
                _wrapper.SendLine(_currentInput.ToString(), ConsoleString.StringType.Input);
                _currentInput = new StringBuilder();
            }
            else if (e.KeyCode.Equals(Keys.Back))
            {
                if (_currentInput.Length > 0)
                {
                    _currentInput.Remove(_currentInput.Length - 1, 1);
                }
            }
            //else if (e.KeyCode.Equals(Keys.F10))
            //{
            //    ConfigGUI config = new ConfigGUI();
            //    config.Show();
            //    Application.DoEvents();
            //}
            else if (e.Control && e.KeyCode.Equals(Keys.Up))
            {
                _graphics.MoveView(-2);
            }
            else if (e.Control && e.KeyCode.Equals(Keys.Down))
            {
                _graphics.MoveView(2);
            }
            else if (e.KeyCode.Equals(Keys.PageUp))
            {
                _graphics.MoveView(-30);
            }
            else if (e.KeyCode.Equals(Keys.PageDown))
            {
                _graphics.MoveView(30);
            }
            else if (e.Control && e.KeyCode.Equals(Keys.Home))
            {
                _graphics.MoveViewHome();
            }
            else if (e.Control && e.KeyCode.Equals(Keys.End))
            {
                _graphics.MoveViewEnd();
            }
        }

        private void DXGUI_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar) || char.IsSymbol(e.KeyChar) || e.KeyChar == ' ')
            {
                if (e.KeyChar == '\\')
                {
                    _currentInput.Append('\\');
                }
                else
                {
                    _currentInput.Append(e.KeyChar);
                }
            }
            _graphics.CurrentLine.Text = _currentLine.ToString() + _currentInput.ToString();

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

    }
}