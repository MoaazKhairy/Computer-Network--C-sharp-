using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace DrawingClient
{
    public partial class Form2 : Form
    {
        private ArrayList nSockets;
        public string path;
        public bool flag;
        public Form2()
        {
            InitializeComponent();
            flag = false;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            lblStatus.Text = "My IP address is " + IPHost.AddressList[0].ToString();
            nSockets = new ArrayList();
            Thread thdListener = new Thread(new ThreadStart(listenerThread));
            thdListener.Start(); //this thread to hear all time
        }

        private void lbConnections_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(8080);
            tcpListener.Start();
            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();   // Accepting a new client
                if (handlerSocket.Connected)
                {
                    Control.CheckForIllegalCrossThreadCalls = false;
                    lbConnections.Items.Add(handlerSocket.RemoteEndPoint.ToString() + " connected.");
                    // lock statement for recieve data from one client at a time only
                    lock (this)
                    {
                        nSockets.Add(handlerSocket); // put in ArrayList of sockets
                    }
                    ThreadStart thdstHandler = new ThreadStart(handlerThread);
                    Thread thdHandler = new Thread(thdstHandler); //make thread for every client
                    thdHandler.Start();
                }
            }
        }
        public void handlerThread()
        {
            Socket handlerSocket = (Socket)nSockets[nSockets.Count - 1];
            NetworkStream networkStream = new NetworkStream(handlerSocket);
            int thisRead = 0;
            int blockSize = 1024;
            Byte[] dataByte = new Byte[blockSize];

            if (flag == false)
            {
                lock (this)
                {
                    // Only one process can access
                    // the same file at any given time
                    File.Create("c:\\my documents\\SubmittedFile.txt").Close();                 
                    Stream fileStream = File.OpenWrite("c:\\my documents\\SubmittedFile.txt");
                    while (true)
                    {
                        thisRead = networkStream.Read(dataByte, 0, blockSize);
                        fileStream.Write(dataByte, 0, thisRead);
                        if (thisRead == 0)
                        {
                            break;
                        }
                    }
                    fileStream.Close();
                }

                lbConnections.Items.Add("File Written");
                handlerSocket = null;

                string lines = File.ReadAllText(@"c:\\my documents\\SubmittedFile.txt");

                int nameIndex = lines.IndexOf("fileName");
                int extensionLength = (nameIndex - 3) - 25;
                string extension = lines.Substring(25, extensionLength);
                Console.WriteLine(extension);
                int lastIndex = lines.IndexOf("}}");
                int nameLength = (lastIndex - 1) - (nameIndex + 11);
                int x = nameIndex + 11;
                string name = lines.Substring(x, nameLength);
                Console.WriteLine(name);
                path = "c:\\my documents\\" + name + extension;
                name = "";
                extension = "";
                Console.WriteLine(path);
                File.Create(path).Close();
                File.Delete("c:\\my documents\\SubmittedFile.txt");
                flag = true;
            }
            else if (flag == true)
            {
                lock (this)
                {
                    // Only one process can access
                    // the same file at any given time
                    Stream fileStream = File.OpenWrite(path);
                    while (true)
                    {
                        thisRead = networkStream.Read(dataByte, 0, blockSize);
                        fileStream.Write(dataByte, 0, thisRead);
                        if (thisRead == 0)
                        {
                            break;
                        }
                    }
                    fileStream.Close();
                }

                lbConnections.Items.Add("File Written");
                handlerSocket = null;
                flag = false;
            }
        }
    }
}
