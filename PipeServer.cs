using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LoginServer
{


    class PipeServer
    {

        [StructLayoutAttribute(LayoutKind.Sequential)]
        struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        private const uint SECURITY_DESCRIPTOR_REVISION = 1;

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool InitializeSecurityDescriptor(ref SECURITY_DESCRIPTOR sd, uint dwRevision);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool SetSecurityDescriptorDacl(ref SECURITY_DESCRIPTOR sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DisconnectNamedPipe(SafeFileHandle hHandle);



        Thread listenThread;
        readonly List<Client> clients = new List<Client>();

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
            public bool isLoggedIn;


        }

        /// <summary>
        /// Variable for if the server is currently running.
        /// </summary>
        private bool running;

        /// <summary>
        /// Variable to hold the name of the pipe.
        /// </summary>
        private string pipeName;

        public const int BUFFER_SIZE = 4096;

        #region Accessors


        /// <summary>
        /// Returns true if the server is running.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsRunning()
        {
            return running;
        }

        /// <summary>
        /// Set whether the Server is currently running or not.
        /// Displays the server is running message.
        /// </summary>
        /// <param name="b">Takes a boolean value</param>
        private void IsRunning(bool b)
        {
            running = b;
            //if (b)
            //{
            //    string str = "The Server is Running.";
            //    MessageRecieved(Utility.ConvertToBytes(str));
            //}
        }

        /// <summary>
        /// Returns the pipe name.
        /// </summary>
        /// <returns>string</returns>
        public string PipeNameIs()
        {
            return pipeName;
        }

        /// <summary>
        /// Set the name of the pipe.<br></br>
        /// Takes a string.
        /// </summary>
        /// <param name="newPipeName">new name for the pipe</param>
        private void PipeNameIs(string newPipeName)
        {
            pipeName = newPipeName;
        }

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        public int TotalConnectedClients
        {
            get
            {
                lock (clients)
                {
                    return clients.Count;
                }
            }
        }

        #endregion

        

        /// <summary>
        /// Handles the messages recieved from the clients pipe.
        /// </summary>
        /// <param name="message"></param>
        public delegate void MessageReceivedHandler(byte[] message);

        /// <summary>
        /// Event is called when ever a message is recieved from client pipe event.
        /// </summary>
        public event MessageReceivedHandler MessageRecieved;

        /// <summary>
        /// Handles the client disconnected massages.
        /// </summary>
        public delegate void ClientDisconnectedHandler();

        /// <summary>
        /// Event is called when a client pipe is severed.
        /// </summary>
        public event ClientDisconnectedHandler ClientDisconnected;

        /// <summary>
        /// Handles the get admin details method call.
        /// </summary>
        /// <returns></returns>
        public delegate string GetAdminDetailsHandler();

        /// <summary>
        /// Event is called when the GetAdminDetails method is called.
        /// </summary>
        public event GetAdminDetailsHandler GetAdminDetails;

        /// <summary>
        /// Handles the AllowMessaging method calls.
        /// </summary>
        public delegate void AllowMessagingHandler();

        /// <summary>
        /// Event is called when the client has been validated.
        /// </summary>
        public event AllowMessagingHandler AllowMessaging;

        public void Start(string pipename)
        {
            PipeNameIs(pipename);

            listenThread = new Thread(ListenForClients)
            {
                IsBackground = true
            };

            listenThread.Start();

            IsRunning(true);

            
        }

        public void ListenForClients()
        {
            SECURITY_DESCRIPTOR sd = new SECURITY_DESCRIPTOR();

            // Set the Security Descriptor to be completely permissive. ////////////////////////////////////////////////
            InitializeSecurityDescriptor(ref sd, SECURITY_DESCRIPTOR_REVISION);
            SetSecurityDescriptorDacl(ref sd, true, IntPtr.Zero, false);

            IntPtr ptrSD = Marshal.AllocCoTaskMem(Marshal.SizeOf(sd));
            Marshal.StructureToPtr(sd, ptrSD, false);

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES
            {
                nLength = Marshal.SizeOf(sd),
                lpSecurityDescriptor = ptrSD,
                bInheritHandle = 1
            };

            IntPtr ptrSA = Marshal.AllocCoTaskMem(Marshal.SizeOf(sa));
            Marshal.StructureToPtr(sa, ptrSA, false);

            while (true)
            {
                // Create an instance of a named pipe for one client.
                SafeFileHandle clientHandle = CreateNamedPipe(
                    PipeNameIs(),
                    // DUPLEX | FILE_FLAG_OVERLAPPED = 0x00000003 | 0x40000000;
                    0x40000003,
                    0,
                    255,
                    BUFFER_SIZE,
                    BUFFER_SIZE,
                    0,
                    ptrSA);

                // If a new named pipe can't be created restart loop.
                if (clientHandle.IsInvalid)
                {
                    continue;
                }

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                // couldn't connect to client.
                if (success == 0)
                {
                    // close the handle and wait for the next client.
                    clientHandle.Close();
                    continue;
                }

                Client client = new Client
                {
                    handle = clientHandle
                };

                Console.WriteLine(clientHandle.GetType().ToString());

                lock (clients)
                    clients.Add(client);

                Thread readThread = new Thread(Read)
                {
                    IsBackground = true
                };

                Console.WriteLine("New thread created." + readThread.ManagedThreadId);

                readThread.Start(client);

                Console.WriteLine(PipeNameIs());
            }

            // Free up the pointers.
            // Never reashed here due to the infinite loop.
            Marshal.FreeCoTaskMem(ptrSD);
            Marshal.FreeCoTaskMem(ptrSA);
        }

        private void Read(Object clientObj)
        {
            Client client = (Client)clientObj;
            client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = new byte[BUFFER_SIZE];

            while (true)
            {
                int bytesRead = 0;

                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {

                        // Read the stream length.
                        int totalSize = client.stream.Read(buffer, 0, 4);

                        // Client has disconnected.
                        if (totalSize == 0)
                        {
                            break;
                        }

                        totalSize = BitConverter.ToInt32(buffer, 0);

                        do
                        {
                            int numBytes = client.stream.Read(buffer, 0, Math.Min(totalSize - bytesRead, BUFFER_SIZE));

                            ms.Write(buffer, 0, numBytes);

                            bytesRead += numBytes;

                        } while (bytesRead < totalSize);

                    }
                    catch
                    {
                        break;
                    }

                    // Client has disconnected.
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    if (MessageRecieved != null)
                    {
                        /////////////////////////////

                        Console.WriteLine(client.handle.ToString());
                        Console.WriteLine(client.ToString());
                        Console.WriteLine(client.stream.Name);

                        ////////////////////////////
                        if (client.isLoggedIn)
                        {
                            MessageRecieved(ms.ToArray());
                        }
                        else
                        {
                            ValidateClient(ms.ToArray(), client);
                        }
                    }
                }
            }

            // The clients must be locked, otherwise "stream.Close()"
            // could be called while SendMessage(byte[]) is being called on another thread.
            // This would lead to an ID error.
            lock (clients)
            {
                // Clean up the resources
                DisconnectNamedPipe(client.handle);
                client.stream.Close();
                client.handle.Close();

                // remove the client from the list of clients.
                clients.Remove(client);
            }

            // invoke the event, a client disconnected.
            if (ClientDisconnected != null)
            {
                ClientDisconnected();
            }

            Console.WriteLine("Aborting Thread.");
            Thread.CurrentThread.Abort();


        }

        #region ClientValidation

        private void ValidateClient(byte[] adminDetailsFromClient, Client client)
        {
            //ASCIIEncoding encoder = new ASCIIEncoding();
            //string str = encoder.GetString(adminDetailsFromClient, 0, adminDetailsFromClient.Length);
            string str = Utility.ConvertToString(adminDetailsFromClient);

            string[] clientAdminDetails = str.Split(',');
            string[] adminDetails = GetAdminDetails().Split(',');

            bool username = ValidateUsername(adminDetails[0], clientAdminDetails[0]);

            PasswordTester LH = new PasswordTester();
            bool password = LH.TestPasswords(adminDetails[1], clientAdminDetails[1]);

            if (username && password)
            {
                client.isLoggedIn = true;
                AllowMessaging();
            }
            else
            {
                client.isLoggedIn = false;
                //str = "Login atempt failed.";
                //byte[] message = encoder.GetBytes(str);
            }

            SendClientValidationMessage(client);
        }

        private bool ValidateUsername(string adminName, string testName)
        {
            if (adminName.CompareTo(testName) == 0)
            {
                return true;
            }

            return false;
        }


        private void SendClientValidationMessage(Client client)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] message;

            if (client.isLoggedIn)
            {
                message = encoder.GetBytes("You are now logged in.");

            }
            else
            {
                message = encoder.GetBytes("Incorrect username or password.");
            }

            SendMessage(message);
        }

        #endregion

        public void SendMessage(byte[] message)
        {
            lock (clients)
            {
                // Get the entire stream length.
                byte[] messageLength = BitConverter.GetBytes(message.Length);

                foreach (Client client in clients)
                {
                    // length
                    client.stream.Write(messageLength, 0, 4);

                    client.stream.Write(message, 0, message.Length);
                    client.stream.Flush();
                }
            }
        }





    }
}
