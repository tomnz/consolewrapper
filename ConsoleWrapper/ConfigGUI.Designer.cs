namespace ConsoleWrapper
{
    partial class ConfigGUI
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
                    components = null;
                }
                if (_previewRender != null)
                {
                    if (_previewRender.IsAlive)
                    {
                        _previewRender.Abort();
                        _previewRender.Join();
                    }
                    _previewRender = null;
                }
                if (_preview != null)
                {
                    _preview.Dispose();
                    _preview = null;
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
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabApplication = new System.Windows.Forms.TabPage();
            this.lbldefaultApp = new System.Windows.Forms.Label();
            this.lblconsoleWidth = new System.Windows.Forms.Label();
            this.lblbufferSize = new System.Windows.Forms.Label();
            this.lblwindowHeight = new System.Windows.Forms.Label();
            this.lblwindowWidth = new System.Windows.Forms.Label();
            this.defaultApp = new System.Windows.Forms.TextBox();
            this.consoleWidth = new System.Windows.Forms.TextBox();
            this.bufferSize = new System.Windows.Forms.TextBox();
            this.windowHeight = new System.Windows.Forms.TextBox();
            this.windowWidth = new System.Windows.Forms.TextBox();
            this.tabCamera = new System.Windows.Forms.TabPage();
            this.accelFactorLocation = new System.Windows.Forms.TrackBar();
            this.preview = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.tabApplication.SuspendLayout();
            this.tabCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accelFactorLocation)).BeginInit();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabApplication);
            this.tabs.Controls.Add(this.tabCamera);
            this.tabs.Location = new System.Drawing.Point(6, 4);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(407, 346);
            this.tabs.TabIndex = 0;
            // 
            // tabApplication
            // 
            this.tabApplication.Controls.Add(this.lbldefaultApp);
            this.tabApplication.Controls.Add(this.lblconsoleWidth);
            this.tabApplication.Controls.Add(this.lblbufferSize);
            this.tabApplication.Controls.Add(this.lblwindowHeight);
            this.tabApplication.Controls.Add(this.lblwindowWidth);
            this.tabApplication.Controls.Add(this.defaultApp);
            this.tabApplication.Controls.Add(this.consoleWidth);
            this.tabApplication.Controls.Add(this.bufferSize);
            this.tabApplication.Controls.Add(this.windowHeight);
            this.tabApplication.Controls.Add(this.windowWidth);
            this.tabApplication.Location = new System.Drawing.Point(4, 21);
            this.tabApplication.Name = "tabApplication";
            this.tabApplication.Padding = new System.Windows.Forms.Padding(3);
            this.tabApplication.Size = new System.Drawing.Size(399, 321);
            this.tabApplication.TabIndex = 0;
            this.tabApplication.Text = "Application";
            this.tabApplication.UseVisualStyleBackColor = true;
            // 
            // lbldefaultApp
            // 
            this.lbldefaultApp.AutoSize = true;
            this.lbldefaultApp.Location = new System.Drawing.Point(14, 115);
            this.lbldefaultApp.Name = "lbldefaultApp";
            this.lbldefaultApp.Size = new System.Drawing.Size(58, 13);
            this.lbldefaultApp.TabIndex = 9;
            this.lbldefaultApp.Text = "defaultApp";
            // 
            // lblconsoleWidth
            // 
            this.lblconsoleWidth.AutoSize = true;
            this.lblconsoleWidth.Location = new System.Drawing.Point(12, 89);
            this.lblconsoleWidth.Name = "lblconsoleWidth";
            this.lblconsoleWidth.Size = new System.Drawing.Size(72, 13);
            this.lblconsoleWidth.TabIndex = 8;
            this.lblconsoleWidth.Text = "consoleWidth";
            // 
            // lblbufferSize
            // 
            this.lblbufferSize.AutoSize = true;
            this.lblbufferSize.Location = new System.Drawing.Point(12, 63);
            this.lblbufferSize.Name = "lblbufferSize";
            this.lblbufferSize.Size = new System.Drawing.Size(54, 13);
            this.lblbufferSize.TabIndex = 7;
            this.lblbufferSize.Text = "bufferSize";
            // 
            // lblwindowHeight
            // 
            this.lblwindowHeight.AutoSize = true;
            this.lblwindowHeight.Location = new System.Drawing.Point(12, 37);
            this.lblwindowHeight.Name = "lblwindowHeight";
            this.lblwindowHeight.Size = new System.Drawing.Size(74, 13);
            this.lblwindowHeight.TabIndex = 6;
            this.lblwindowHeight.Text = "windowHeight";
            // 
            // lblwindowWidth
            // 
            this.lblwindowWidth.AutoSize = true;
            this.lblwindowWidth.Location = new System.Drawing.Point(12, 11);
            this.lblwindowWidth.Name = "lblwindowWidth";
            this.lblwindowWidth.Size = new System.Drawing.Size(71, 13);
            this.lblwindowWidth.TabIndex = 5;
            this.lblwindowWidth.Text = "windowWidth";
            // 
            // defaultApp
            // 
            this.defaultApp.Location = new System.Drawing.Point(127, 115);
            this.defaultApp.Name = "defaultApp";
            this.defaultApp.Size = new System.Drawing.Size(140, 20);
            this.defaultApp.TabIndex = 4;
            // 
            // consoleWidth
            // 
            this.consoleWidth.Location = new System.Drawing.Point(127, 89);
            this.consoleWidth.Name = "consoleWidth";
            this.consoleWidth.Size = new System.Drawing.Size(67, 20);
            this.consoleWidth.TabIndex = 3;
            // 
            // bufferSize
            // 
            this.bufferSize.Location = new System.Drawing.Point(127, 63);
            this.bufferSize.Name = "bufferSize";
            this.bufferSize.Size = new System.Drawing.Size(67, 20);
            this.bufferSize.TabIndex = 2;
            // 
            // windowHeight
            // 
            this.windowHeight.Location = new System.Drawing.Point(127, 37);
            this.windowHeight.Name = "windowHeight";
            this.windowHeight.Size = new System.Drawing.Size(67, 20);
            this.windowHeight.TabIndex = 1;
            // 
            // windowWidth
            // 
            this.windowWidth.Location = new System.Drawing.Point(127, 11);
            this.windowWidth.Name = "windowWidth";
            this.windowWidth.Size = new System.Drawing.Size(67, 20);
            this.windowWidth.TabIndex = 0;
            // 
            // tabCamera
            // 
            this.tabCamera.Controls.Add(this.accelFactorLocation);
            this.tabCamera.Location = new System.Drawing.Point(4, 21);
            this.tabCamera.Name = "tabCamera";
            this.tabCamera.Padding = new System.Windows.Forms.Padding(3);
            this.tabCamera.Size = new System.Drawing.Size(399, 321);
            this.tabCamera.TabIndex = 1;
            this.tabCamera.Text = "Camera";
            this.tabCamera.UseVisualStyleBackColor = true;
            // 
            // accelFactorLocation
            // 
            this.accelFactorLocation.Location = new System.Drawing.Point(6, 6);
            this.accelFactorLocation.Name = "accelFactorLocation";
            this.accelFactorLocation.Size = new System.Drawing.Size(387, 34);
            this.accelFactorLocation.TabIndex = 0;
            // 
            // preview
            // 
            this.preview.Location = new System.Drawing.Point(420, 52);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(160, 120);
            this.preview.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(419, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Preview";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(422, 324);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(77, 26);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(505, 324);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(77, 26);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ConfigGUI
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(588, 354);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.preview);
            this.Controls.Add(this.tabs);
            this.Name = "ConfigGUI";
            this.Text = "Configure ConsoleWrapper";
            this.tabs.ResumeLayout(false);
            this.tabApplication.ResumeLayout(false);
            this.tabApplication.PerformLayout();
            this.tabCamera.ResumeLayout(false);
            this.tabCamera.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accelFactorLocation)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabApplication;
        private System.Windows.Forms.TextBox windowWidth;
        private System.Windows.Forms.TabPage tabCamera;
        private System.Windows.Forms.TextBox consoleWidth;
        private System.Windows.Forms.TextBox bufferSize;
        private System.Windows.Forms.TextBox windowHeight;
        private System.Windows.Forms.Label lbldefaultApp;
        private System.Windows.Forms.Label lblconsoleWidth;
        private System.Windows.Forms.Label lblbufferSize;
        private System.Windows.Forms.Label lblwindowHeight;
        private System.Windows.Forms.Label lblwindowWidth;
        private System.Windows.Forms.TextBox defaultApp;
        private System.Windows.Forms.TrackBar accelFactorLocation;
        private System.Windows.Forms.Panel preview;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}