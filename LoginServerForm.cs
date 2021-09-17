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
            pipeServer.GetAdminDetails += pipeServer_GetAdminDetails;
            pipeServer.AllowMessaging += pipeServer_AllowMessaging;
            NoClients();
        }


        private void pipeServer_AllowMessaging()
        {
            Invoke(new PipeServer.AllowMessagingHandler(AllowMessaging));
        }

        private void AllowMessaging()
        {
            SendBtn.Enabled = true;
            ClearBtn.Enabled = true;
            SendMessageTB.Enabled = true;
            SendMessageTB.Focus();

            string str = "Total Clients: " + pipeServer.TotalConnectedClients;

            MessageLogTB.Text += str + "\r\n";
        }

        private void NoClients()
        {
            SendBtn.Enabled = false;
            SendMessageTB.Enabled = false;
            ClearBtn.Enabled = false;
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

        private string pipeServer_GetAdminDetails()
        {
            return (string) Invoke(new PipeServer.GetAdminDetailsHandler(GetAdminDetails));   
        }

        private String GetAdminDetails()
        {
            return AdminUsernameTB.Text + "," + AdminPasswordTB.Text;
        }

        private void pipeServer_ClientDisconnected()
        {
            Invoke(new PipeServer.ClientDisconnectedHandler(ClientDisconnected));
        }

        private void ClientDisconnected()
        {
            string str = "Total Clients: " + pipeServer.TotalConnectedClients;
            
            MessageLogTB.Text += str + "\r\n";
            
            if (pipeServer.TotalConnectedClients == 0)
            {
                NoClients();
            }
        }

        private void pipeServer_MessageReceived(byte[] message)
        {
            Invoke(new PipeServer.MessageReceivedHandler(DisplayMessageReceived),
                new object[] { message });
        }

        private void DisplayMessageReceived(byte[] message)
        {
            //ASCIIEncoding encoder = new ASCIIEncoding();
            //string str = encoder.GetString(message, 0, message.Length);
            string str = Utility.ConvertToString(message);

            MessageLogTB.Text += str + "\r\n";
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(SendMessageTB.Text))
            {
                byte[] messageBuffer = Utility.ConvertToBytes(SendMessageTB.Text);

                pipeServer.SendMessage(messageBuffer);
            }
            SendMessageTB.Clear();
            SendMessageTB.Focus();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            MessageLogTB.Clear();
            SendMessageTB.Focus();
        }
    }
}
