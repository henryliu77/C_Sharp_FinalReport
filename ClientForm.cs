using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net;

namespace TCPIP_Chat_Room_Client
{
    public partial class ClientForm : Form
    {
        private int curr_x;
        private int curr_y;
        private bool isWndMove = false;

        TcpClient client;
        NetworkStream stream;
        Thread receiveThread;
        private bool isReceiving = false;
        private string IPAddress;
        private string portText;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void btn_Connected_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress = tb_ServerIP.Text.Trim();
                portText = txtPort.Text.Trim();
                if (!int.TryParse(portText, out int port))
                {
                    MessageBox.Show("�п�J���Ī� Port ���X");
                    return;
                }
                client = new TcpClient(IPAddress, port);
                stream = client.GetStream();
                AppendChatMessage("�w�s�u����A��");
                lbl_Status.Text = "�w�s�u����A��";
                CPB_Connected.FillColor = Color.Green;

                receiveThread = new Thread(ReceiveMessage);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                btn_Connected.Enabled = false;
            }
            catch (Exception ex)
            {
                AppendChatMessage("�s�u���~: " + ex.Message);

            }
        }

        private void AppendChatMessage(string message)
        {
            if (flowLayoutPanel.InvokeRequired)
            {
                flowLayoutPanel.Invoke(new MethodInvoker(() => AppendChatMessage(message)));
                return;
            }

            string senderLabel = "";
            string content = message;

            // �����o�e��
            if (message.StartsWith("Client: "))
            {
                senderLabel = "Client";
                content = message.Substring("Client: ".Length);
            }
            else if (message.StartsWith("Server: "))
            {
                senderLabel = "Server";
                content = message.Substring("Server: ".Length);
            }
            else
            {

                senderLabel = "Server" +
                    "";
                content = message;
            }


            if (!string.IsNullOrEmpty(senderLabel))
            {
                Label nameLabel = new Label
                {
                    Text = senderLabel + "�G",
                    AutoSize = true,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                flowLayoutPanel.Controls.Add(nameLabel);
            }

            if (content.StartsWith("[Sticker]"))
            {
                string fileName = content.Substring("[Sticker]".Length).Trim(); ;
                string stickerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stickers", fileName);

                if (File.Exists(stickerPath))
                {
                    try
                    {
                        using (FileStream fs = new FileStream(stickerPath, FileMode.Open, FileAccess.Read))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            ms.Position = 0;

                            PictureBox pb = new PictureBox
                            {
                                Image = Image.FromStream(ms),
                                SizeMode = PictureBoxSizeMode.Zoom,
                                Size = new Size(200, 200),
                                Margin = new Padding(3)
                            };
                            flowLayoutPanel.Controls.Add(pb);
                        }
                    }
                    catch (Exception ex)
                    {
                        flowLayoutPanel.Controls.Add(new Label { Text = "[�K�ϸ��J���~] " + ex.Message });
                    }
                }
                else
                {
                    flowLayoutPanel.Controls.Add(new Label { Text = "[�䤣��K��] " + fileName });
                }
            }
            else
            {
                int currentIndex = 0;
                while (currentIndex < content.Length)
                {
                    int startIndex = content.IndexOf(':', currentIndex);
                    if (startIndex == -1)
                    {
                        string text = content.Substring(currentIndex);
                        if (!string.IsNullOrEmpty(text))
                        {
                            flowLayoutPanel.Controls.Add(new Label
                            {
                                Text = text,
                                AutoSize = true,
                                Font = new Font("Arial", 12, FontStyle.Regular)
                            });
                        }
                        break;
                    }

                    int endIndex = content.IndexOf(':', startIndex + 1);
                    if (endIndex == -1)
                    {
                        string text = content.Substring(currentIndex);
                        Label lbl = new Label { Text = text, AutoSize = true };
                        flowLayoutPanel.Controls.Add(lbl);
                        break;
                    }

                    string beforeText = content.Substring(currentIndex, startIndex - currentIndex);
                    if (!string.IsNullOrEmpty(beforeText))
                    {
                        Label lbl = new Label { Text = beforeText, AutoSize = true };
                        flowLayoutPanel.Controls.Add(lbl);
                    }

                    string emojiTag = content.Substring(startIndex, endIndex - startIndex + 1);
                    string emojiName = emojiTag.Trim(':');
                    string emojiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emojis", emojiName + ".png");

                    if (File.Exists(emojiPath))
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(emojiPath, FileMode.Open, FileAccess.Read))
                            using (MemoryStream ms = new MemoryStream())
                            {
                                fs.CopyTo(ms);
                                ms.Position = 0;

                                PictureBox pb = new PictureBox
                                {
                                    Image = Image.FromStream(ms),
                                    SizeMode = PictureBoxSizeMode.Zoom,
                                    Size = new Size(24, 24),
                                    Margin = new Padding(1, 3, 1, 3)
                                };
                                flowLayoutPanel.Controls.Add(pb);
                            }
                        }
                        catch
                        {
                            flowLayoutPanel.Controls.Add(new Label { Text = emojiTag, AutoSize = true });
                        }
                    }
                    else
                    {
                        flowLayoutPanel.Controls.Add(new Label { Text = emojiTag, AutoSize = true });
                    }

                    currentIndex = endIndex + 1;
                }
            }

            flowLayoutPanel.VerticalScroll.Value = flowLayoutPanel.VerticalScroll.Maximum;
        }


        private void ReceiveMessage()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    // --- �P�_�O�_ [FILE]| �}�Y ---
                    // �� peek ���覡��Ū 7 bytes �ˬd
                    byte[] peekBuf = new byte[7];
                    int peekLen = stream.Read(peekBuf, 0, 7);
                    if (peekLen == 7 && Encoding.UTF8.GetString(peekBuf, 0, 7) == "[FILE]|")
                    {
                        // �wŪ 7 bytes�A�A����Ū header�]�ѤU���'|'�A�Y�ɦW�M���ס^
                        StringBuilder headerBuilder = new StringBuilder("[FILE]|");
                        int pipeCount = 1; // �w�g���@��'|'
                        while (pipeCount < 3)
                        {
                            int b = stream.ReadByte();
                            if (b == -1) throw new IOException("Stream closed.");
                            char c = (char)b;
                            headerBuilder.Append(c);
                            if (c == '|') pipeCount++;
                        }
                        string header = headerBuilder.ToString();
                        string[] headParts = header.Split('|');
                        string fileName = headParts[1];
                        if (!int.TryParse(headParts[2], out int fileLen))
                        {
                            AppendChatMessage("������~���Ϥ����׮榡: " + headParts[2]);
                            continue;
                        }

                        // 2. Ū���Ϥ����e
                        List<byte> fileData = new List<byte>();
                        while (fileData.Count < fileLen)
                        {
                            int toRead = Math.Min(buffer.Length, fileLen - fileData.Count);
                            int count = stream.Read(buffer, 0, toRead);
                            if (count <= 0) throw new IOException("Stream closed during file receive.");
                            fileData.AddRange(buffer.Take(count));
                        }

                        // 3. �x�s����ܹϤ�
                        string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReceivedImages");
                        if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
                        string savePath = Path.Combine(saveDir, fileName);
                        File.WriteAllBytes(savePath, fileData.ToArray());

                        flowLayoutPanel.Invoke(new MethodInvoker(() =>
                        {
                            flowLayoutPanel.Controls.Add(new Label
                            {
                                Text = "Client�G",
                                AutoSize = true,
                                Font = new Font("Arial", 12, FontStyle.Bold)
                            });
                            PictureBox pb = new PictureBox
                            {
                                Image = Image.FromFile(savePath),
                                SizeMode = PictureBoxSizeMode.Zoom,
                                Size = new Size(120, 120),
                                Margin = new Padding(3)
                            };
                            flowLayoutPanel.Controls.Add(pb);
                        }));
                    }
                    else
                    {
                        // ���O [FILE]|�A�^��i buffer�A��������r�T����
                        int offset = 0;
                        if (peekLen > 0)
                        {
                            Array.Copy(peekBuf, 0, buffer, 0, peekLen);
                            offset = peekLen;
                        }
                        int bytesRead = stream.Read(buffer, offset, buffer.Length - offset);
                        bytesRead += offset;

                        if (bytesRead > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            AppendChatMessage(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendChatMessage("Client �w���u " + ex.Message);
            }
        }


        // ���A�ɤ@��Ū���Ƶ������p��k�]�Ω�D�ɮר�w�ɡ^
        private string ReadStringUntilEndOfBuffer(NetworkStream stream)
        {
            byte[] buf = new byte[4096];
            int bytesRead = stream.Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf, 0, bytesRead);
        }



        private void ShowDisconnected()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ShowDisconnected));
                return;
            }
            MessageBox.Show("�w�M���A���_�u�I");
            lbl_Status.Text = "�w�_�u";
            btn_Connected.Enabled = true;
            btn_Disconnected.Enabled = false;


        }
        private void ClientForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void ClientForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void ClientForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }

        private void TopForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void TopForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void TopForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }

        private void btn_Send_Click(object sender, EventArgs e)
        {


            if (stream == null) return;

            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            SendMessage(message);
            AppendChatMessage("Client: " + message);
            txtMessage.Clear();
        }

        private void SendMessage(string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                AppendChatMessage("�o�e���~: " + ex.Message);
            }
        }

        private void btn_Stickers_Click(object sender, EventArgs e)
        {
            if (stream == null) return;

            StickerForm stickerForm = new StickerForm();
            if (stickerForm.ShowDialog() == DialogResult.OK)
            {
                string fileName = stickerForm.SelectedSticker;
                if (!string.IsNullOrEmpty(fileName))
                {
                    string stickerMsg = "[Sticker]" + fileName.Trim();
                    SendMessage(stickerMsg);
                    AppendChatMessage(stickerMsg);

                }
            }
        }

        private void ClientForm_Minimized_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_Emoji_Click(object sender, EventArgs e)
        {
            EmojiForm emojiForm = new EmojiForm();
            if (emojiForm.ShowDialog() == DialogResult.OK)
            {
                string emojiTag = emojiForm.SelectedEmojiTag;
                if (!string.IsNullOrEmpty(emojiTag))
                {
                    int pos = txtMessage.SelectionStart;
                    txtMessage.Text = txtMessage.Text.Insert(pos, emojiTag);
                    txtMessage.SelectionStart = pos + emojiTag.Length;
                    txtMessage.Focus();
                }
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true; // ����X�{����
                btn_Send.PerformClick();   // �I�s�e�X���s�ƥ�
            }
        }

        private void btn_Disconnected_Click(object sender, EventArgs e)
        {
            // ����� thread
            isReceiving = false;
            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }
            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Join(200);

            // UI ��s
            lbl_Status.Text = "�w�_�u";
            btn_Connected.Enabled = true;
            btn_Disconnected.Enabled = false;

        }

        private void btn_favoriteDay_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "�@�~��365�ѡA�A�q�ڳ̳��w���@�ѡH";
        }

        private void btn_favoriteDay_2_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "�M�A�b�@�_���C�@��";
        }

        private void btn_hello_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "Hello�A�A�n!";
        }

        private void btn_bye_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "ByeBye�A�U����!";
        }

        private void btn_OpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "�Ϥ��ɮ�|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "��ܭn�ǰe���Ϥ�";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                        if (fileInfo.Length > 5 * 1024 * 1024)
                        {
                            MessageBox.Show("�Ϥ��j�p����W�L 5MB", "���~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Ū�ɮ׬��줸��
                        byte[] imgBytes = File.ReadAllBytes(openFileDialog.FileName);
                        string fileName = Path.GetFileName(openFileDialog.FileName);
                        // �]�p��w�G[FILE]|�ɦW|�ɮת���|
                        string header = $"[FILE]|{fileName}|{imgBytes.Length}|";
                        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

                        // �ǰe header
                        stream.Write(headerBytes, 0, headerBytes.Length);
                        // �ǰe�Ϥ����e
                        stream.Write(imgBytes, 0, imgBytes.Length);

                        // ���a��ѫ����
                        Image selectedImage = Image.FromFile(openFileDialog.FileName);
                        PictureBox pb = new PictureBox
                        {
                            Image = selectedImage,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Size = new Size(120, 120),
                            Margin = new Padding(3)
                        };
                        flowLayoutPanel.Controls.Add(new Label
                        {
                            Text = "Client�G",
                            AutoSize = true,
                            Font = new Font("Arial", 12, FontStyle.Bold)
                        });
                        flowLayoutPanel.Controls.Add(pb);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("�Ϥ�Ū������: " + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
   
