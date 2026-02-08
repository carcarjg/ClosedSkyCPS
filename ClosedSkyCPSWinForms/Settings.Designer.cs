namespace ClosedSkyCPSWinForms
{
    partial class Settings
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
            serialportCMB = new ComboBox();
            label1 = new Label();
            saveBUT = new Button();
            SuspendLayout();
            // 
            // serialportCMB
            // 
            serialportCMB.FormattingEnabled = true;
            serialportCMB.Location = new Point(127, 25);
            serialportCMB.Name = "serialportCMB";
            serialportCMB.Size = new Size(182, 33);
            serialportCMB.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 28);
            label1.Name = "label1";
            label1.Size = new Size(91, 25);
            label1.TabIndex = 1;
            label1.Text = "Serial Port";
            // 
            // saveBUT
            // 
            saveBUT.Location = new Point(114, 91);
            saveBUT.Name = "saveBUT";
            saveBUT.Size = new Size(112, 34);
            saveBUT.TabIndex = 2;
            saveBUT.Text = "Save";
            saveBUT.UseVisualStyleBackColor = true;
            saveBUT.Click += saveBUT_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(347, 137);
            Controls.Add(saveBUT);
            Controls.Add(label1);
            Controls.Add(serialportCMB);
            Name = "Settings";
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox serialportCMB;
        private Label label1;
        private Button saveBUT;
    }
}