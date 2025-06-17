namespace TCPIP_Chat_Room_Client
{
    partial class StickerForm
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StickerForm));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            panel1 = new Panel();
            btn_Exit = new Guna.UI2.WinForms.Guna2Button();
            label1 = new Label();
            flowLayoutPanel = new FlowLayoutPanel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(64, 64, 80);
            panel1.Controls.Add(btn_Exit);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(-1, -3);
            panel1.Name = "panel1";
            panel1.Size = new Size(449, 51);
            panel1.TabIndex = 0;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            // 
            // btn_Exit
            // 
            btn_Exit.CustomizableEdges = customizableEdges5;
            btn_Exit.DisabledState.BorderColor = Color.DarkGray;
            btn_Exit.DisabledState.CustomBorderColor = Color.DarkGray;
            btn_Exit.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btn_Exit.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btn_Exit.FillColor = Color.FromArgb(64, 64, 80);
            btn_Exit.Font = new Font("Segoe UI", 9F);
            btn_Exit.ForeColor = Color.White;
            btn_Exit.Image = (Image)resources.GetObject("btn_Exit.Image");
            btn_Exit.ImageSize = new Size(25, 25);
            btn_Exit.Location = new Point(405, 3);
            btn_Exit.Name = "btn_Exit";
            btn_Exit.ShadowDecoration.CustomizableEdges = customizableEdges6;
            btn_Exit.Size = new Size(41, 35);
            btn_Exit.TabIndex = 1;
            btn_Exit.Click += btn_Exit_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 136);
            label1.ForeColor = Color.White;
            label1.Location = new Point(17, 13);
            label1.Name = "label1";
            label1.Size = new Size(73, 20);
            label1.TabIndex = 0;
            label1.Text = "我的貼圖";
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Location = new Point(-1, 38);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(449, 341);
            flowLayoutPanel.TabIndex = 1;
            // 
            // StickerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(447, 378);
            ControlBox = false;
            Controls.Add(flowLayoutPanel);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "StickerForm";
            MouseDown += StickerForm_MouseDown;
            MouseMove += StickerForm_MouseMove;
            MouseUp += StickerForm_MouseUp;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Label label1;
        private FlowLayoutPanel flowLayoutPanel;
        private Guna.UI2.WinForms.Guna2Button btn_Exit;
    }
}