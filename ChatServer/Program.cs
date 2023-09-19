using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Program
    {


        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            string ip;
            Console.WriteLine("Please enter a ip adress to start server");
            ip = Console.ReadLine();
            Console.WriteLine($"Server started at {ip}");

            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse(ip), 7891);
            _listener.Start();

            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

                /* Broadcast the connection to everyone on the server */
                BroadcastConnection();


            }
        }

        static void BroadcastConnection()
        {
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();

                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());

                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }

        public static void BoradcastMessage(string message)
        {
            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }

        public static void BoradcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }

            BoradcastMessage($"[{disconnectedUser.Username}] Disconnected!");
        }

    }
}
