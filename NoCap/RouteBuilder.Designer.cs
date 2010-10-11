namespace NoCap
{
    partial class RouteBuilder
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.routes = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // routes
            // 
            this.routes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.routes.Location = new System.Drawing.Point(0, 0);
            this.routes.Name = "routes";
            this.routes.Size = new System.Drawing.Size(660, 572);
            this.routes.TabIndex = 0;
            this.routes.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.RouteExpanded);
            // 
            // RouteBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 572);
            this.Controls.Add(this.routes);
            this.Name = "RouteBuilder";
            this.Text = "RouteBuilder";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView routes;

    }
}