using System;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using ChatSocketServer1;

namespace chatSocketServer1
{
    class Program
    {
        public static Hashtable clientsList = new Hashtable();
        

        static void Main(string[] args)
        {

            
            //Se crear el socket principal con la direccion local del host del servidor
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = ipHostInfo.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            TcpListener serverSocket = new TcpListener(ipAddress, 8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;
            ApiHelper.Initializer();
            //Se inicializa el socket principal
            serverSocket.Start();
            Console.WriteLine("Servidor Chat Iniciado ....");
            counter = 0;
            while ((true))
            {
                counter += 1;

                //Llego una solicitud de conexion nueva de un cliente, se acepta la conexion
                clientSocket = serverSocket.AcceptTcpClient();

                //Se reserva memoria para los mensajes del cliente
                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                string dataFromClient = null;

                //Leemos el mensaje recibido del cliente
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                //Si el nombre del cliente ya existe se borra de la lista de clientes
                clientsList.Remove(dataFromClient);
                clientsList.Add(dataFromClient, clientSocket);

                //Se lanza la una nueva conexion para aceptar al cliente nuevo 
                HandleClient client = new HandleClient();
                client.startClient(clientSocket, dataFromClient, clientsList);               
                Console.WriteLine(dataFromClient + " se unió a la sala de chat ");

                //Le reenviamos el mensaje al resto de los clientes conectados
                broadcast(dataFromClient + " se ha unido ", dataFromClient, false);
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            //Se reenvia el mensaje a todos los clientes conectados
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                if (broadcastSocket.Connected)
                {
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;

                    if (flag == true)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(uName + " dijo : " + msg);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
                else
                {
                    clientsList.Remove(uName);
                }

            }
        }  //end broadcast function

        public static void singleCast(string msg, string uName, bool flag)
        {
            //Se reenvia el mensaje al cliente
           
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)clientsList[uName];
                if (broadcastSocket.Connected)
                {
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;

                    if (flag == true)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
                else
                {
                    clientsList.Remove(uName);
                }

            
        }  //end singleCast function

       

        public static void QA(string msg, string uName, bool flag)
        {
            String question = msg.TrimStart('-','b',' ');
            question = question.ToLower();
            String answer="";

            switch (question)
            {
                case String a when a.Contains("cuantos paises hay en el mundo"):
                    answer = "194";
                break;

                case String b when b.Contains("nombre del mamifero que pone huevos"):
                    answer = "El ornitorrinco";
                    break;

                case String c when c.Contains("cual es el mejor juego del 2022"):
                    answer = "Elden Ring";
                    break;

                case String d when d.Contains("nombre del lider de desarrollo de elden ring"):
                    answer = "Hidetaka Miyazaki";
                    break;

                case String d when d.Contains("cuando fue la independencia de mexico"):
                    answer = "16 de septiembre de 1810";
                    break;

                case String e when e.Contains("hora"):
                    answer = DateTime.Now.ToString("HH:mm");
                    break;

                case String f when f.Contains("estrella mas cercana al sol"):
                    answer = "Próxima Centauri";
                    break;

                case String g when g.Contains("dia"):
                    answer = DateTime.Now.ToString("dd/MM/yyyy");
                    break;

                case String h when h.Contains("empresa mas valiosa del mundo"):
                    answer = "Apple con 660 mil millones de dolares";
                    break;

                case String i when i.Contains("nombre del hombre mas rico del mundo"):
                    answer = "Elon Musk con 223 mil millones de dolares";
                    break;

                case String j when j.Contains("nombre del asentamiento mas al norte"):
                    answer = "Longyearbyen, el cual cuenta con solo 2300 residentes";
                    break;

                case String k when k.Contains("clima"):
                    string parameters = question.Remove(0, 6);
                    answer = weatherProcessor.weatherApi(parameters).Result;
                    break;

                case String l when l.Contains("conversion"):
                    string parameterse = question.Remove(0, 11);
                    answer = exchangeProcessor.exchangeApi(parameterse).Result;
                    break;

                default:
                    answer = "Sin respuesta";
                    break;

            }
            singleCast(answer, uName, flag);

        }
       

    }//end Main class


    public class HandleClient
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            //Se inicializa la clase con los datos del cliente nuevo
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;

            //Se lanza un hilo para poder recibir asincronamente mensajes
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            //byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            //Byte[] sendBytes = null;
            //string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];

                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    if (dataFromClient.IndexOf("$") > 0)
                    {
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                        Console.WriteLine("Del cliente - " + clNo + " : " + dataFromClient);
                        rCount = Convert.ToString(requestCount);

                        doBot(dataFromClient, clNo, true);
                    }
                    else
                    {
                        clientsList.Remove(clNo);
                        Console.WriteLine(clNo + " salió de la sala de chat ");
                        return;
                    }

                }
                catch (Exception ex)
                {
                    //clientSocket.Close();
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }//end while
        }//end doChat
        private void doBot(string msg, string uName, bool flag)
        {
        
            if (msg.StartsWith("-b"))
            {
                Program.QA(msg, uName, flag);
            }
            else
            {
                Program.broadcast(msg, uName, flag);
            }   
        }

       
    } //end class handleClinet

}//end namespace

