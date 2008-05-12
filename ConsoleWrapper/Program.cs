using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ConsoleWrapper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DXGUI frm = new DXGUI();
            frm.Show();

            while (frm.Created)
            {
                frm.Render();
                Application.DoEvents();
            }
        }
    }
}