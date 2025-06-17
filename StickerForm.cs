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
    public partial class StickerForm : Form
    {
        private int curr_x;
        private int curr_y;
        private bool isWndMove = false;

        public string SelectedSticker { get; private set; }

        public StickerForm()
        {
            InitializeComponent();
            LoadStickers();
        }

        
        private void LoadStickers()
        {
            string stickerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stickers");
            if (!Directory.Exists(stickerPath))
            {
                MessageBox.Show("Stickers 資料夾不存在！");
                return;
            }

            string[] files = Directory.GetFiles(stickerPath, "*.png");
            foreach (string file in files)
            {
                PictureBox pb = new PictureBox();
                pb.Image = Image.FromFile(file);
                pb.Size = new Size(100, 100);
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Margin = new Padding(5);
                pb.Cursor = Cursors.Hand;

                pb.Click += (s, e) =>
                {
                    SelectedSticker = Path.GetFileName(file);
                    DialogResult = DialogResult.OK;
                    Close();
                };

                flowLayoutPanel.Controls.Add(pb);
            }
        }

        private void StickerForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void StickerForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void StickerForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
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
    }
}

