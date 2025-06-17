using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TCPIP_Chat_Room_Client
{
    public partial class MessageBubble : UserControl
    {
        public MessageBubble()
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
        }

        public void SetMessage(string nickname, string message, DateTime time, bool isSelf)
        {
            lblTitle.Text = $"{nickname}  {time:HH:mm}";
            panelBubble.Controls.Clear();

            // 判斷是否貼圖訊息
            if (message.StartsWith("[Sticker]"))
            {
                string stickerFile = message.Substring("[Sticker]".Length).Trim();
                string stickerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stickers", stickerFile);
                if (File.Exists(stickerPath))
                {
                    var pic = new PictureBox
                    {
                        Image = Image.FromFile(stickerPath),
                        Width = 200,
                        Height = 200,
                        SizeMode = PictureBoxSizeMode.Zoom
                    };
                    panelBubble.Controls.Add(pic);
                }
                else
                {
                    panelBubble.Controls.Add(new Label { Text = "[貼圖不存在]", AutoSize = true, Font = new Font("微軟正黑體", 12) });
                }
            }
            else
            {
                // 文字與 emoji 混合處理
                List<Control> controls = ParseMessageWithEmojis(message);
                foreach (var ctrl in controls)
                    panelBubble.Controls.Add(ctrl);
            }

            // 右邊/左邊對齊與泡泡顏色
            if (isSelf)
            {
                this.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                this.Padding = new Padding(80, 4, 8, 4); // 右邊靠齊
                panelBubble.BackColor = Color.LightGreen;
            }
            else
            {
                this.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                this.Padding = new Padding(8, 4, 80, 4); // 左邊靠齊
                panelBubble.BackColor = Color.WhiteSmoke;
            }

            // 圓角泡泡
            panelBubble.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, panelBubble.Width, panelBubble.Height, 18, 18));
        }

        // emoji文字混合解析
        public List<Control> ParseMessageWithEmojis(string message)
        {
            var controls = new List<Control>();
            string emojiFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emojis");
            var emojiDict = EmojiHelper.LoadEmojiDict(emojiFolder);

            int lastPos = 0;
            var matches = System.Text.RegularExpressions.Regex.Matches(message, "(:[a-zA-Z0-9_]+:)");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Index > lastPos)
                {
                    string txt = message.Substring(lastPos, match.Index - lastPos);
                    controls.Add(new Label
                    {
                        Text = txt,
                        AutoSize = true,
                        Font = new Font("微軟正黑體", 12),
                        Margin = new Padding(8, 8, 8, 8)
                    });
                }

                string code = match.Value;
                if (emojiDict.TryGetValue(code, out string imgPath) && File.Exists(imgPath))
                {
                    controls.Add(new PictureBox
                    {
                        Image = Image.FromFile(imgPath),
                        Width = 100,
                        Height = 100,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Margin = new Padding(0, 0, 0, 0)
                    });
                }
                else
                {
                    controls.Add(new Label { Text = code, AutoSize = true });
                }
                lastPos = match.Index + match.Length;
            }

            if (lastPos < message.Length)
            {
                string txt = message.Substring(lastPos);
                controls.Add(new Label
                {
                    Text = txt,
                    AutoSize = true,
                    Font = new Font("微軟正黑體", 12)
                });
            }
            return controls;
        }

        // 圓角自動重繪
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (panelBubble != null)
            {
                panelBubble.Region = System.Drawing.Region.FromHrgn(
                    NativeMethods.CreateRoundRectRgn(0, 0, panelBubble.Width, panelBubble.Height, 18, 18));
            }
        }
    }

    // 共用 NativeMethods
    public class NativeMethods
    {
        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }
}
