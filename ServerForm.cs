using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TCPIP_Arduino_ChatRoom;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Ports;
using static System.Windows.Forms.DataFormats;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TCPIP_Chat_Room_Server
{
    public partial class ServerForm : Form
    {

        private int curr_x;
        private int curr_y;
        private bool isWndMove = false;
        

        TcpListener server;
        TcpClient client;
        NetworkStream stream;
        Thread listenerThread;
        Thread receiveThread;



        private volatile bool isServerRunning = false; // 旗標，Thread 安全
        private volatile bool isReceiving = false;     // 旗標，Thread 安全
        


        FlowLayoutPanel flowPanelOnline = new FlowLayoutPanel();

        public ServerForm()
        {
            InitializeComponent();
            GetIPAddress();
        }

        private void GetIPAddress()
        {
            string ipList = "";
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !ip.ToString().StartsWith("127."))
                {
                    ipList += ip.ToString() + Environment.NewLine;
                }
            }
            if (string.IsNullOrEmpty(ipList))
                ipList = "無法取得本機IP（可能未連接網路）";
            tb_ClientIP.Text = ipList;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }

        private void Top_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void Top_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void Top_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }

        private void btn_Connected_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPort.Text.Trim(), out int port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("請輸入1024~65535間的有效通訊埠號", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                isServerRunning = true; // 設定伺服器運行狀態
                listenerThread = new Thread(ListenForClient) { IsBackground = true };
                listenerThread.Start();

                btn_Connected.Enabled = false;
                btn_Disconnected.Enabled = true;
                lbl_Status.Text = "伺服器已啟動";
                pb_Connected_Status_color.FillColor = Color.Green;
            }
            catch (Exception ex)
            {
                lbl_Status.Text = "啟動錯誤: " + ex.Message;
                pb_Connected_Status_color.BackColor = Color.Red;
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
                senderLabel = "Client";
                content = message;
            }

            Label nameLabel = new Label
            {
                Text = senderLabel + "：",
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            flowLayoutPanel.Controls.Add(nameLabel);

            if (content.StartsWith("[Sticker]"))
            {
                string stickerFile = content.Substring("[Sticker]".Length).Trim();
                string stickerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stickers", stickerFile);

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
                                Margin = new Padding(1, 3, 1, 3)
                            };
                            flowLayoutPanel.Controls.Add(pb);
                        }
                    }
                    catch
                    {
                        flowLayoutPanel.Controls.Add(new Label { Text = "[載入貼圖失敗: " + stickerFile + "]", AutoSize = true });
                    }
                }
                else
                {
                    flowLayoutPanel.Controls.Add(new Label { Text = "[找不到貼圖: " + stickerFile + "]", AutoSize = true });
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
                        flowLayoutPanel.Controls.Add(new Label { Text = text, AutoSize = true });
                        break;
                    }

                    string beforeText = content.Substring(currentIndex, startIndex - currentIndex);
                    if (!string.IsNullOrEmpty(beforeText))
                    {
                        flowLayoutPanel.Controls.Add(new Label { Text = beforeText, AutoSize = true });
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

        private void ListenForClient()
        {
            try
            {
                client = server.AcceptTcpClient();
                stream = client.GetStream();
                AppendChatMessage("Client 已連線");

                receiveThread = new Thread(ReceiveMessage);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                AppendChatMessage("連線錯誤: " + ex.Message);
            }

        }

        private void ReceiveMessage()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    // --- 判斷是否 [FILE]| 開頭 ---
                    byte[] peekBuf = new byte[7];
                    int peekLen = stream.Read(peekBuf, 0, 7);
                    if (peekLen == 7 && Encoding.UTF8.GetString(peekBuf, 0, 7) == "[FILE]|")
                    {
                        // 已讀 7 bytes，再接著讀 header（剩下兩個'|'，即檔名和長度）
                        StringBuilder headerBuilder = new StringBuilder("[FILE]|");
                        int pipeCount = 1;
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
                            AppendChatMessage("收到錯誤的圖片長度格式: " + headParts[2]);
                            continue;
                        }

                        // 2. 讀取圖片內容
                        List<byte> fileData = new List<byte>();
                        while (fileData.Count < fileLen)
                        {
                            int toRead = Math.Min(buffer.Length, fileLen - fileData.Count);
                            int count = stream.Read(buffer, 0, toRead);
                            if (count <= 0) throw new IOException("Stream closed during file receive.");
                            fileData.AddRange(buffer.Take(count));
                        }

                        // 3. 儲存並顯示圖片
                        string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReceivedImages");
                        if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
                        string savePath = Path.Combine(saveDir, fileName);
                        File.WriteAllBytes(savePath, fileData.ToArray());

                        flowLayoutPanel.Invoke(new MethodInvoker(() =>
                        {
                            flowLayoutPanel.Controls.Add(new Label
                            {
                                Text = "Client：", // Server端顯示來自Client
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
                        // 不是 [FILE]|，回填進 buffer，直接當成文字訊息收
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
                            AppendChatMessage(message); // 會自動判斷Client/Server
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendChatMessage("Client 已離線 " + ex.Message);
            }
        }


        // 幫你補一個讀到資料結束的小方法（用於非檔案協定時）
        private string ReadStringUntilEndOfBuffer(NetworkStream stream)
        {
            byte[] buf = new byte[4096];
            int bytesRead = stream.Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf, 0, bytesRead);
        }


        private void btn_Send_Click(object sender, EventArgs e)
        {
            if (stream == null) return;

            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            SendMessage(message);
            AppendChatMessage("Server: " + message);
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
                AppendChatMessage("發送錯誤: " + ex.Message);
            }
        }

        private void btn_Emojis_Click(object sender, EventArgs e)
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

        private void WinFormMinimized_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
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
                    string stickerMsg = "[Sticker]" + fileName;
                    SendMessage(stickerMsg);
                    AppendChatMessage("Server: " + stickerMsg);
                }
            }

        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true; // 防止出現換行
                btn_Send.PerformClick();   // 呼叫送出按鈕事件
            }
        }


        private void StopServer()
        {
            isServerRunning = false; // 讓 ListenerForClient thread 停止
            try
            {
                if (server != null)
                {
                    server.Stop(); // 停止監聽
                    server = null;
                }
            }
            catch { }



            // 關閉主 socket/stream/thread
            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }
            try { listenerThread?.Join(200); } catch { }
            try { receiveThread?.Join(200); } catch { }

            // UI 更新
            if (InvokeRequired)
                Invoke(new Action(() =>
                {
                    lbl_Status.Text = "伺服器已斷線";
                    pb_Connected_Status_color.FillColor = Color.Gray;
                    btn_Connected.Enabled = true;
                    btn_Disconnected.Enabled = false;
                }));
            else
            {
                lbl_Status.Text = "伺服器已斷線";
                pb_Connected_Status_color.FillColor = Color.Gray;
                btn_Connected.Enabled = true;
                btn_Disconnected.Enabled = false;
            }
            // 清空聊天區或在線人員顯示（如有）
            if (flowPanelOnline.InvokeRequired)
                flowPanelOnline.Invoke(new Action(() => flowPanelOnline.Controls.Clear()));
            else
                flowPanelOnline.Controls.Clear();
        }

        private void btn_Disconnected_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void btn_cold1_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "知道世界上最冷的地方是哪嗎？";
        }

        private void btn_cold2_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "沒有你的地方";
        }

        private void btn_Hello_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "Hello，你好!";
        }

        private void btn_bye_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "ByeBye，下次見!";
        }

        private void btn_OpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "圖片檔案|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "選擇要傳送的圖片";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                        if (fileInfo.Length > 5 * 1024 * 1024)
                        {
                            MessageBox.Show("圖片大小不能超過 5MB", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // 讀檔案為位元組
                        byte[] imgBytes = File.ReadAllBytes(openFileDialog.FileName);
                        string fileName = Path.GetFileName(openFileDialog.FileName);
                        // 設計協定：[FILE]|檔名|檔案長度|
                        string header = $"[FILE]|{fileName}|{imgBytes.Length}|";
                        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

                        // 傳送 header
                        stream.Write(headerBytes, 0, headerBytes.Length);
                        // 傳送圖片內容
                        stream.Write(imgBytes, 0, imgBytes.Length);

                        // 本地聊天室顯示
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
                            Text = "Server：",
                            AutoSize = true,
                            Font = new Font("Arial", 12, FontStyle.Bold)
                        });
                        flowLayoutPanel.Controls.Add(pb);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("圖片讀取失敗: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


   