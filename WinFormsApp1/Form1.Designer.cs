namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            installButton = new Button();
            label2 = new Label();
            runTempButton = new Button();
            label3 = new Label();
            linkLabel1 = new LinkLabel();
            SuspendLayout();
            // 
            // installButton
            // 
            installButton.Location = new Point(104, 12);
            installButton.Name = "installButton";
            installButton.Size = new Size(139, 29);
            installButton.TabIndex = 0;
            installButton.Text = "button1";
            installButton.UseVisualStyleBackColor = true;
            installButton.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 16);
            label2.Name = "label2";
            label2.Size = new Size(98, 20);
            label2.TabIndex = 2;
            label2.Text = "Install status";
            // 
            // runTempButton
            // 
            runTempButton.Location = new Point(104, 55);
            runTempButton.Name = "runTempButton";
            runTempButton.Size = new Size(139, 29);
            runTempButton.TabIndex = 3;
            runTempButton.Text = "button2";
            runTempButton.UseVisualStyleBackColor = true;
            runTempButton.Click += button2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(9, 59);
            label3.Name = "label3";
            label3.Size = new Size(84, 20);
            label3.TabIndex = 4;
            label3.Text = "Run status";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(104, 91);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(105, 20);
            linkLabel1.TabIndex = 5;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Github Pages";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(255, 120);
            Controls.Add(linkLabel1);
            Controls.Add(label3);
            Controls.Add(runTempButton);
            Controls.Add(label2);
            Controls.Add(installButton);
            MaximizeBox = false;
            Name = "Form1";
            Text = "ChromeBook BatteryManage GUI";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button installButton;
        private Label label2;
        private Button runTempButton;
        private Label label3;
        private LinkLabel linkLabel1;
    }
}
