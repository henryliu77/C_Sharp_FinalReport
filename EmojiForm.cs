using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPIP_Chat_Room_Client
{
    public partial class EmojiForm : Form
    {
        private int curr_x;
        private int curr_y;
        private bool isWndMove = false;

        public string SelectedEmojiTag { get; private set; }

        public EmojiForm()
        {
            InitializeComponent();
            LoadEmojis();
        }

        public void LoadEmojis()
        {
            string emojiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emojis");
            if (!Directory.Exists(emojiPath))
            {
                MessageBox.Show("Emojis 資料夾不存在！");
                return;
            }

            string[] files = Directory.GetFiles(emojiPath, "*.png");
            foreach (string file in files)
            {
                PictureBox pb = new PictureBox();
                pb.Image = Image.FromFile(file);
                pb.Size = new Size(24, 24);
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Margin = new Padding(3);
                pb.Cursor = Cursors.Hand;

                string emojiName = Path.GetFileNameWithoutExtension(file);

                pb.Click += (s, e) =>
                {
                    SelectedEmojiTag = $":{emojiName}:";
                    DialogResult = DialogResult.OK;
                    Close();
                };

                flowLayoutPanel1.Controls.Add(pb);
            }
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }

        private void EmojiForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void EmojiForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void EmojiForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }
    }
}
