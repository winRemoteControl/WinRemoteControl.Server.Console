namespace WinRemoteControl.WinTestApp
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.lbOut = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbOut
            // 
            this.lbOut.ContextMenuStrip = this.contextMenuStrip1;
            this.lbOut.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbOut.FormattingEnabled = true;
            this.lbOut.ItemHeight = 15;
            this.lbOut.Location = new System.Drawing.Point(2, 1);
            this.lbOut.Name = "lbOut";
            this.lbOut.Size = new System.Drawing.Size(274, 49);
            this.lbOut.TabIndex = 1;
            this.lbOut.DoubleClick += new System.EventHandler(this.lbOut_DoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem,
            this.traceToolStripMenuItem,
            this.toolStripMenuItem2,
            this.aboutToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(175, 104);
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(174, 24);
            this.configurationToolStripMenuItem.Text = "Configuration";
            // 
            // setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem
            // 
            this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem.Name = "setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem";
            this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem.Size = new System.Drawing.Size(511, 24);
            this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem.Text = "Set SuppressWhenMinimized In Registry For Remote Desktop";
            this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem.Click += new System.EventHandler(this.setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem_Click);
            // 
            // traceToolStripMenuItem
            // 
            this.traceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toUIToolStripMenuItem,
            this.toFileToolStripMenuItem,
            this.toolStripMenuItem1,
            this.clearToolStripMenuItem});
            this.traceToolStripMenuItem.Name = "traceToolStripMenuItem";
            this.traceToolStripMenuItem.Size = new System.Drawing.Size(174, 24);
            this.traceToolStripMenuItem.Text = "Trace";
            // 
            // toUIToolStripMenuItem
            // 
            this.toUIToolStripMenuItem.Name = "toUIToolStripMenuItem";
            this.toUIToolStripMenuItem.Size = new System.Drawing.Size(162, 24);
            this.toUIToolStripMenuItem.Text = "To UI";
            this.toUIToolStripMenuItem.Click += new System.EventHandler(this.toUIToolStripMenuItem_Click);
            // 
            // toFileToolStripMenuItem
            // 
            this.toFileToolStripMenuItem.Name = "toFileToolStripMenuItem";
            this.toFileToolStripMenuItem.Size = new System.Drawing.Size(162, 24);
            this.toFileToolStripMenuItem.Text = "To File";
            this.toFileToolStripMenuItem.Click += new System.EventHandler(this.toFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(159, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(162, 24);
            this.clearToolStripMenuItem.Text = "Clear UI";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click_1);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(171, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(174, 24);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 25;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 54);
            this.Controls.Add(this.lbOut);
            this.Name = "Form1";
            this.Text = "win Remote Control Server";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbOut;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem traceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toUIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

