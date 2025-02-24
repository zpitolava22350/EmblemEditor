namespace EmblemEditorCPU {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            label1 = new Label();
            colorDialog1 = new ColorDialog();
            label2 = new Label();
            clrdisplay = new Label();
            label3 = new Label();
            btn_Generate = new Button();
            progressBar1 = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(320, 320);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.MouseClick += pictureBox1_MouseClick;
            // 
            // pictureBox2
            // 
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.Location = new Point(338, 12);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(320, 320);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.Location = new Point(12, 335);
            label1.Name = "label1";
            label1.Size = new Size(95, 15);
            label1.TabIndex = 2;
            label1.Text = "Reference Image";
            // 
            // colorDialog1
            // 
            colorDialog1.AnyColor = true;
            colorDialog1.FullOpen = true;
            // 
            // label2
            // 
            label2.Location = new Point(561, 335);
            label2.Name = "label2";
            label2.Size = new Size(97, 15);
            label2.TabIndex = 3;
            label2.Text = "Generated Image";
            // 
            // clrdisplay
            // 
            clrdisplay.BackColor = Color.Black;
            clrdisplay.Location = new Point(12, 364);
            clrdisplay.Name = "clrdisplay";
            clrdisplay.Size = new Size(15, 15);
            clrdisplay.TabIndex = 4;
            clrdisplay.Click += clrdisplay_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 364);
            label3.Name = "label3";
            label3.Size = new Size(103, 15);
            label3.TabIndex = 5;
            label3.Text = "Background Color";
            // 
            // btn_Generate
            // 
            btn_Generate.Font = new Font("Segoe UI", 12F);
            btn_Generate.Location = new Point(520, 383);
            btn_Generate.Name = "btn_Generate";
            btn_Generate.Size = new Size(139, 32);
            btn_Generate.TabIndex = 6;
            btn_Generate.Text = "Generate";
            btn_Generate.UseVisualStyleBackColor = true;
            btn_Generate.Click += btn_Generate_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(175, 338);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(320, 23);
            progressBar1.TabIndex = 7;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(671, 427);
            Controls.Add(progressBar1);
            Controls.Add(btn_Generate);
            Controls.Add(label3);
            Controls.Add(clrdisplay);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Label label1;
        private ColorDialog colorDialog1;
        private Label label2;
        private Label clrdisplay;
        private Label label3;
        private Button btn_Generate;
        private ProgressBar progressBar1;
    }
}
