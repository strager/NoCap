namespace NoCap {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.screenshot = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.clipboard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // screenshot
            // 
            this.screenshot.AutoSize = true;
            this.screenshot.Location = new System.Drawing.Point(12, 12);
            this.screenshot.Name = "screenshot";
            this.screenshot.Size = new System.Drawing.Size(90, 27);
            this.screenshot.TabIndex = 0;
            this.screenshot.Text = "Screenshot";
            this.screenshot.UseVisualStyleBackColor = true;
            this.screenshot.Click += new System.EventHandler(this.ScreenshotClicked);
            // 
            // log
            // 
            this.log.Location = new System.Drawing.Point(12, 92);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(258, 151);
            this.log.TabIndex = 1;
            // 
            // clipboard
            // 
            this.clipboard.AutoSize = true;
            this.clipboard.Location = new System.Drawing.Point(180, 12);
            this.clipboard.Name = "clipboard";
            this.clipboard.Size = new System.Drawing.Size(90, 27);
            this.clipboard.TabIndex = 2;
            this.clipboard.Text = "Clipboard";
            this.clipboard.UseVisualStyleBackColor = true;
            this.clipboard.Click += new System.EventHandler(this.ClipboardClicked);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.clipboard);
            this.Controls.Add(this.log);
            this.Controls.Add(this.screenshot);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button screenshot;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Button clipboard;
    }
}

