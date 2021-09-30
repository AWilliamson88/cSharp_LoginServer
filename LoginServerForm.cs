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

        /// <summary>
        /// Sets up the program when the form first starts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginServerForm_Load(object sender, EventArgs e)
        {
            pipeServer.MessageRecieved += pipeServer_MessageReceived;
            pipeServer.ClientDisconnected += pipeServer_ClientDisconnected;
            pipeServer.GetAdminDetails += pipeServer_GetAdminDetails;
            pipeServer.AllowMessaging += pipeServer_AllowMessaging;
            NoClients();
        }

        /// <summary>
        /// This method returns the admin details.
        /// </summary>
        /// <returns></returns>
        private String GetAdminDetails()
        {
            return AdminUsernameTB.Text + "," + AdminPasswordTB.Text;
        }

        /// <summary>
        /// Methods used to allow the server class to call methods on the form.
        /// </summary>
        /// <returns></returns>
        #region HandlerMethods
        private string pipeServer_GetAdminDetails()
        {
            return (string)Invoke(new PipeServer.GetAdminDetailsHandler(GetAdminDetails));
        }

        private void pipeServer_AllowMessaging()
        {
            Invoke(new PipeServer.AllowMessagingHandler(AllowMessaging));
        }

        private void pipeServer_MessageReceived(byte[] message)
        {
            Invoke(new PipeServer.MessageReceivedHandler(DisplayMessageReceived),
                new object[] { message });
        }

        private void pipeServer_ClientDisconnected()
        {
            Invoke(new PipeServer.ClientDisconnectedHandler(ClientDisconnected));
        }
        #endregion

        // DisplayMessage() and AllowMessaging().
        #region Messaging

        /// <summary>
        /// Allows messaging after atleast one client is connected.
        /// </summary>
        private void AllowMessaging()
        {
            SendBtn.Enabled = true;
            ClearBtn.Enabled = true;
            SendMessageTB.Enabled = true;
            SendMessageTB.Focus();

            string str = "Total Clients: " + pipeServer.TotalConnectedClients;

            MessageLogTB.Text += str + "\r\n";
        }

        /// <summary>
        /// Display the received message.
        /// </summary>
        /// <param name="message">The message received.</param>
        private void DisplayMessageReceived(byte[] message)
        {
            string str = Utility.ConvertToString(message);

            MessageLogTB.Text += str + "\r\n";
        }
        #endregion

        // ClientDisconnected() & NoClients().
        #region ClientDisconnectedMethods
        /// <summary>
        /// Prints the number of connected clients everytime one connects or disconnects.
        /// Calls the NoClients() when there are no clients.
        /// </summary>
        private void ClientDisconnected()
        {
            string str = "Total Clients: " + pipeServer.TotalConnectedClients;
            
            MessageLogTB.Text += str + "\r\n";
            
            if (pipeServer.TotalConnectedClients == 0)
            {
                NoClients();
            }
        }

        /// <summary>
        /// This method dissables the messaging functions when there is no clients connected.
        /// </summary>
        private void NoClients()
        {
            SendBtn.Enabled = false;
            SendMessageTB.Enabled = false;
            ClearBtn.Enabled = false;
        }
        #endregion

        #region ButtonClickMethods

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBtn_Click(object sender, EventArgs e)
        {
            pipeServer.Start(PipeNameTB.Text);
            StartBtn.Enabled = false;
            MessageLogTB.Text += "The Server is Now Running: \r\n";
        }

        /// <summary>
        /// Send the text in the send message box to all the clients.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendBtn_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(SendMessageTB.Text))
            {
                byte[] messageBuffer = Utility.ConvertToBytes(SendMessageTB.Text);

                pipeServer.SendMessageToAll(messageBuffer);
            }
            SendMessageTB.Clear();
            SendMessageTB.Focus();
        }

        /// <summary>
        /// Clears the message log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            MessageLogTB.Clear();
            SendMessageTB.Focus();
        }
        #endregion
    }
}
