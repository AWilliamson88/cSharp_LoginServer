using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginServer
{
    public partial class LoginServerForm : Form
    {
        private PipeServer pipeServer = new PipeServer();

        public LoginServerForm()
        {
            InitializeComponent();
        }

        private void LoginServerForm_Load(object sender, EventArgs e)
        {
            pipeServer.MessageRecieved += pipeServer_MessageReceived;
            pipeServer.ClientDisconnected += pipeServer_ClientDisconnected;
            DisableMessaging();
        }


        private void DisableMessaging()
        {
            SendBtn.Enabled = false;
            SendMessageTB.Enabled = false;
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            // If the server is not running start it.
            if(pipeServer.IsRunning())
            {
                MessageBox.Show("Server is already running", "Error");
            }
            else
            {
                pipeServer.Start(PipeNameTB.Text);
                StartBtn.Enabled = false;
            }
        }

        private void pipeServer_ClientDisconnected()
        {
            Invoke(new PipeServer.ClientDisconnectedHandler(ClientDisconnected));
        }

        private void ClientDisconnected()
        {
            MessageBox.Show("Total connected clients: " + pipeServer.TotalConnectedClients);
        }

        private void pipeServer_MessageReceived(byte[] message)
        {
            Invoke(new PipeServer.MessageReceivedHandler(DisplayMessageReceived),
                new object[] { message });
        }

        void DisplayMessageReceived(byte[] message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            string str = encoder.GetString(message, 0, message.Length);

            MessageLogTB.Text += str + "\r\n";
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] messageBuffer = encoder.GetBytes(SendMessageTB.Text);

            pipeServer.SendMessage(messageBuffer);
            SendMessageTB.Clear();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            MessageLogTB.Clear();
        }
    }
}
