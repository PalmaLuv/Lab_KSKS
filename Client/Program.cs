using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientUDP
{
    class UPDClient
    {
        string standartIp = "127.0.0.";
        string serverIP = "127.0.0.1";
        static int[] Ports = new int[2]; // 0 - localPort | 1 - remotePort.
        static Socket listenSocket;

        public void Client(string ip = null)
        {
            var rand = new Random();
            ip = ip ?? standartIp + rand.Next(0,256);      // If the passed ip value is null , then the standard value is written.

            Console.WriteLine("Client IP : {0}", ip);
            Console.Write("Write down the port for receiving data : ");
            while (!int.TryParse(Console.ReadLine(), out Ports[0]))
                Console.Write("You have entered the wrong value, make sure it is correct:");
            Console.Write("Write down the port for sending data : ");
            while (!int.TryParse(Console.ReadLine(), out Ports[1]))
                Console.Write("You have entered the wrong value, make sure it is correct:");

            try
            {
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Task.Run(() => conectToServer(ip));

                while (true)
                {
                    var message = Console.ReadLine();
                    if (message.ToLower() == "exit") break;
                    else
                    {
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        var remotePoint = new IPEndPoint(IPAddress.Parse(serverIP), Ports[1]);
                        listenSocket.SendTo(data, remotePoint);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error {e.Message}");
            }
            finally
            {
                if (listenSocket is not null)
                {
                    listenSocket.Shutdown(SocketShutdown.Both);
                    listenSocket.Close();
                    listenSocket = null;
                }
            }
        }

        private static void conectToServer(string ip)
        {
            try
            {
                var localIP = new IPEndPoint(IPAddress.Parse(ip), Ports[0]);
                listenSocket.Bind(localIP);

                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int NumberOfBytes = 0;

                    byte[] data = new byte[1024];
                    EndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        NumberOfBytes = listenSocket.ReceiveFrom(data, ref remoteIP);
                        builder.Append(Encoding.Unicode.GetString(data, 0, NumberOfBytes));
                    } while (listenSocket.Available > 0);

                    IPEndPoint FullIP_remote = remoteIP as IPEndPoint;
                    Console.WriteLine($"{FullIP_remote.Address}:{FullIP_remote.Port} - {builder}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (listenSocket is not null)
                {
                    listenSocket.Shutdown(SocketShutdown.Both);
                    listenSocket.Close();
                    listenSocket = null;
                }
            }
        }
    }

    class Program
    {
        static void Main()
        {
            UPDClient clientClass = new UPDClient();
            clientClass.Client(); 
        }
    }
}