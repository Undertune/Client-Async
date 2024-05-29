using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace async_client
{
    public partial class Form1 : Form
    {
        private ChatClient chatClient;
        private const int Port = 12345;
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text;
            chatClient = new ChatClient();
            chatClient.MessageReceived += AppendMessage;
            chatClient.ConnectionStatusChanged += AppendMessage;
            await chatClient.ConnectAsync(ipAddress, Port);
        }
        private void AppendMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendMessage), message);
                return;
            }

            textBox3.AppendText(message + Environment.NewLine);
        }

        private void UpdateStatus(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), status);
                return;
            }

            textBox2.AppendText(status + Environment.NewLine);
        }


        private async void button2_Click(object sender, EventArgs e)
        {
            string message = textBox2.Text;
            await chatClient.SendMessageAsync(message);
            textBox2.Clear();
        }

      

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
    public class ChatClient
    {
        private TcpClient client;
        private NetworkStream stream;
        public event Action<string> MessageReceived;
        public event Action<string> ConnectionStatusChanged;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                stream = client.GetStream();
                ConnectionStatusChanged?.Invoke("Connected to the server.");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke($"Connection failed: {ex.Message}");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (stream == null) return;

            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            while (client.Connected)
            {
                try
                {
                    var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                        break;

                    var message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    MessageReceived?.Invoke(message);
                }
                catch
                {
                    break;
                }
            }

            ConnectionStatusChanged?.Invoke("Disconnected from the server.");
            stream.Close();
            client.Close();
        }
    }
}
