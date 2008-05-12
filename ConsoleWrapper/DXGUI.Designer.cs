namespace ConsoleWrapper
{
    partial class DXGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (_wrapper != null)
                {
                    _wrapper.Dispose();
                    _wrapper = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DXGUI
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "DXGUI";
            this.Text = "ConsoleWrapper";
            this.ResizeBegin += new System.EventHandler(this.DXGUI_ResizeBegin);
            this.Move += new System.EventHandler(this.DXGUI_Move);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DXGUI_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DXGUI_KeyDown);
            this.ResizeEnd += new System.EventHandler(this.DXGUI_ResizeEnd);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DXGUI_MouseWheel);
            this.ResumeLayout(false);

        }

        #endregion

    }
}