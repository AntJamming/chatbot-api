using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ChatSocketClient1
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient clientSocket;
        NetworkStream serverStream = default(NetworkStream);
        Thread ctThread;
        string readData = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void getMessage()
        {
            try
            {
                while (true)
                {
                    serverStream = clientSocket.GetStream();
                    int buffSize = 0;
                    byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                    buffSize = clientSocket.ReceiveBufferSize;
                    serverStream.Read(inStream, 0, buffSize);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    readData = "" + returndata;
                    msg();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Desconexion();
            }
        }

        private void msg()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
                textBox2.Text = textBox2.Text + Environment.NewLine + " >> " + readData;
        }

        private void Desconexion()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(Desconexion));
            else
            {
                //Se cierra y reinicia el socket del cliente
                clientSocket.Close();
                clientSocket.Dispose();
                clientSocket = new System.Net.Sockets.TcpClient(AddressFamily.InterNetwork);

                button2.Enabled = false;
                textBox3.Enabled = false;
                textBox2.Text = textBox2.Text + Environment.NewLine + " >> " + "Desconectado del servidor";
                button1.Text = "Conectar";
                button1.Focus();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.OK;

            if (button1.Text == "Conectar")
            {
                if (textBox1.Text.Trim().Length == 0)
                {
                    MessageBox.Show("Se debe capturar un nombre", "Mensaje", buttons);
                    textBox1.Focus();
                    return;
                }
                if (textBox4.Text.Trim().Length == 0)
                {
                    MessageBox.Show("Se debe capturar una dirección IP valida", "Mensaje", buttons);
                    textBox4.Focus();
                    return;
                }
                button2.Enabled = true;
                textBox3.Enabled = true;

                try 
                {
                    IPAddress ipAddress = IPAddress.Parse(textBox4.Text);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 8888);
                    clientSocket.Connect(ipEndPoint);

                } catch (Exception ex)
                {
                    MessageBox.Show("Error al conectarse al servidor." + Environment.NewLine + ex.Message , "Mensaje", buttons);
                    Desconexion();
                    return;
                }

                readData = "Conectado al servidor Chat ...";
                msg();

                serverStream = clientSocket.GetStream();

                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox1.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                ctThread = new Thread(getMessage);
                ctThread.Start();

                button1.Text = "Desconectar";
            }
            else
            {
                //Se cierra y reinicia el socket del cliente
                Desconexion();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox3.Text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            textBox3.Text = "";
            textBox3.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clientSocket = new System.Net.Sockets.TcpClient(AddressFamily.InterNetwork);

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = ipHostInfo.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            textBox4.Text = ipAddress.ToString();

            button2.Enabled = false;
            textBox3.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            clientSocket.Close();
            clientSocket.Dispose();
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2_Click(this, new EventArgs());
            }
        }
    }
}

