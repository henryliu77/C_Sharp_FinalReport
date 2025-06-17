namespace TCPIP_Chat_Room_Client
{
    partial class MessageBubble
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            panelBubble = new Panel();
            picSticker = new PictureBox();
            lblTitle = new Label();
            flowPanelContent = new FlowLayoutPanel();
            panelBubble.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSticker).BeginInit();
            SuspendLayout();
            // 
            // panelBubble
            // 
            panelBubble.AutoSize = true;
            panelBubble.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelBubble.BackColor = Color.LightSkyBlue;
            panelBubble.Controls.Add(picSticker);
            panelBubble.Location = new Point(15, 20);
            panelBubble.Margin = new Padding(0);
            panelBubble.Name = "panelBubble";
            panelBubble.Padding = new Padding(10);
            panelBubble.Size = new Size(113, 63);
            panelBubble.TabIndex = 0;
            // 
            // picSticker
            // 
            picSticker.Location = new Point(0, 0);
            picSticker.Name = "picSticker";
            picSticker.Size = new Size(100, 50);
            picSticker.SizeMode = PictureBoxSizeMode.AutoSize;
            picSticker.TabIndex = 2;
            picSticker.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Arial", 9.75F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(2);
            lblTitle.Size = new Size(76, 20);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Andy 20:56";
            // 
            // flowPanelContent
            // 
            flowPanelContent.AutoSize = true;
            flowPanelContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowPanelContent.Dock = DockStyle.Top;
            flowPanelContent.Location = new Point(0, 20);
            flowPanelContent.Margin = new Padding(0);
            flowPanelContent.Name = "flowPanelContent";
            flowPanelContent.Size = new Size(653, 0);
            flowPanelContent.TabIndex = 2;
            flowPanelContent.WrapContents = false;
            // 
            // MessageBubble
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowPanelContent);
            Controls.Add(lblTitle);
            Controls.Add(panelBubble);
            Margin = new Padding(0, 2, 0, 2);
            Name = "MessageBubble";
            Size = new Size(653, 450);
            panelBubble.ResumeLayout(false);
            panelBubble.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picSticker).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelBubble;
        private Label lblTitle;
        private PictureBox picSticker;
        private FlowLayoutPanel flowPanelContent;
    }
}