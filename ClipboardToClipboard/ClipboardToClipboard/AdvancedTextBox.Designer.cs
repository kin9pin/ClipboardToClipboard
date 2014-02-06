namespace ClipboardToClipboard
{
    partial class AdvancedTextBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox = new System.Windows.Forms.TextBox();
            this.labelPlaceholder = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Location = new System.Drawing.Point(0, 0);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(150, 20);
            this.textBox.TabIndex = 0;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
            // 
            // labelPlaceholder
            // 
            this.labelPlaceholder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPlaceholder.AutoSize = true;
            this.labelPlaceholder.BackColor = System.Drawing.Color.White;
            this.labelPlaceholder.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.labelPlaceholder.ForeColor = System.Drawing.Color.DarkGray;
            this.labelPlaceholder.Location = new System.Drawing.Point(3, 3);
            this.labelPlaceholder.Name = "labelPlaceholder";
            this.labelPlaceholder.Size = new System.Drawing.Size(85, 13);
            this.labelPlaceholder.TabIndex = 1;
            this.labelPlaceholder.Text = "labelPlaceholder";
            this.labelPlaceholder.Click += new System.EventHandler(this.labelPlaceholder_Click);
            // 
            // AdvancedTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelPlaceholder);
            this.Controls.Add(this.textBox);
            this.Name = "AdvancedTextBox";
            this.Size = new System.Drawing.Size(150, 42);
            this.Load += new System.EventHandler(this.AdvancedTextBox_Load);
            this.SizeChanged += new System.EventHandler(this.AdvancedTextBox_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Label labelPlaceholder;
    }
}
