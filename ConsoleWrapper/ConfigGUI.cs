using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Data.Odbc;
using System.Diagnostics;
using System.Threading;

namespace ConsoleWrapper
{
    public partial class ConfigGUI : Form
    {
        WrapperConfig _config;
        WrapperGraphics _preview;
        Thread _previewRender;

        public ConfigGUI()
        {
            InitializeComponent();
            _config = new WrapperConfig();
            _preview = new WrapperGraphics(this.preview);
            
            int width = int.Parse(_config.GetSettingValue("consoleWidth"));
            StringBuilder str = new StringBuilder();
            for (int j = 0; j < width; j++)
            {
                str.Append('X');
            }

            for (int i = 0; i < 20; i++)
            {
                _preview.AddLine(new ConsoleString(str.ToString()));
            }
            
            SetupForm();

            _previewRender = new Thread(new ThreadStart(UpdatePreview));
            _previewRender.IsBackground = true;
            _previewRender.Priority = ThreadPriority.Lowest;
            _previewRender.Start();
        }

        void UpdatePreview()
        {
            while (true)
            {
                _preview.Render();
                Thread.Sleep(100);
            }
        }

        private void SetupForm()
        {
            foreach(Control control in this.Controls)
            {
                if (control.GetType() == typeof(TabControl))
                {
                    foreach (Control subControl in control.Controls)
                    {
                        if (subControl.GetType() == typeof(TabPage))
                        {
                            foreach (Control subSubControl in subControl.Controls)
                            {
                                if (subSubControl.GetType() == typeof(TextBox))
                                {
                                    subSubControl.Text = _config.GetSettingValue(subSubControl.Name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}