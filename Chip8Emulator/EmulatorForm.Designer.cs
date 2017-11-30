namespace Chip8Emulator
{
    partial class EmulatorForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.openRomButton = new System.Windows.Forms.Button();
            this.renderPanel = new System.Windows.Forms.PictureBox();
            this.chipRenderPanel = new OpenTK.GLControl();
            ((System.ComponentModel.ISupportInitialize)(this.renderPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(13, 13);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(981, 20);
            this.textBox1.TabIndex = 0;
            // 
            // openRomButton
            // 
            this.openRomButton.Location = new System.Drawing.Point(1000, 11);
            this.openRomButton.Name = "openRomButton";
            this.openRomButton.Size = new System.Drawing.Size(37, 23);
            this.openRomButton.TabIndex = 1;
            this.openRomButton.Text = "...";
            this.openRomButton.UseVisualStyleBackColor = true;
            this.openRomButton.Click += new System.EventHandler(this.openRomButton_Click);
            // 
            // renderPanel
            // 
            this.renderPanel.Location = new System.Drawing.Point(13, 40);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(1024, 512);
            this.renderPanel.TabIndex = 2;
            this.renderPanel.TabStop = false;
            // 
            // chipRenderPanel
            // 
            this.chipRenderPanel.BackColor = System.Drawing.Color.Black;
            this.chipRenderPanel.Location = new System.Drawing.Point(13, 40);
            this.chipRenderPanel.Name = "chipRenderPanel";
            this.chipRenderPanel.Size = new System.Drawing.Size(1024, 512);
            this.chipRenderPanel.TabIndex = 3;
            this.chipRenderPanel.VSync = false;
            // 
            // EmulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1049, 562);
            this.Controls.Add(this.chipRenderPanel);
            this.Controls.Add(this.renderPanel);
            this.Controls.Add(this.openRomButton);
            this.Controls.Add(this.textBox1);
            this.KeyPreview = true;
            this.Name = "EmulatorForm";
            this.Text = "CHIP8 Emulator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EmulatorForm_FormClosing);
            this.Load += new System.EventHandler(this.EmulatorForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EmulatorForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.EmulatorForm_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.renderPanel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button openRomButton;
        private System.Windows.Forms.PictureBox renderPanel;
        private OpenTK.GLControl chipRenderPanel;
    }
}

