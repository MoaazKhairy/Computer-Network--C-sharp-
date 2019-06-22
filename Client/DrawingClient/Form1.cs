using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace DrawingClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            tbFilename.Text = openFileDialog.FileName;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            // Open a TCP/IP Connection and send the data
            TcpClient clientSocket = new TcpClient(tbServer.Text, 8080);
            NetworkStream networkStream = clientSocket.GetStream();

            int slashPosition = tbFilename.Text.LastIndexOf(@"\");
            int dotPosition = tbFilename.Text.LastIndexOf(@".");

            int length = dotPosition - slashPosition;

            string name = tbFilename.Text.Substring(slashPosition + 1, length - 1);
            Console.WriteLine(name);

            string extension = tbFilename.Text.Substring(dotPosition);
            Console.WriteLine(extension);


            byte[] lines = File.ReadAllBytes(tbFilename.Text);

            files m = new files();
            m.fileName = name;
            m.fileExtension = extension;

            Envelope_File en = new Envelope_File(m);
            string obj = JsonConvert.SerializeObject(en);
            //send message
            byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(obj);
            networkStream.Write(messageToSend, 0, messageToSend.Length);
            networkStream.Close();
            // MessageBox.Show("1 send");
            Thread.Sleep(1000);
            send_data();
        }
        private void send_data()
        {
            TcpClient clientSocket2 = new TcpClient(tbServer.Text, 8080);
            NetworkStream networkStream2 = clientSocket2.GetStream();

            Stream fileStream = File.OpenRead(tbFilename.Text); //Save Data to be sent in a text file
            // Alocate memory space for the file
            byte[] fileBuffer = new byte[fileStream.Length]; //Use the sent data's length 
            fileStream.Read(fileBuffer, 0, (int)fileStream.Length);

            networkStream2.Write(fileBuffer, 0, fileBuffer.GetLength(0));
            tbFilename.Text = String.Empty;
            networkStream2.Close();
            MessageBox.Show("File sent");
        }
    }
}
