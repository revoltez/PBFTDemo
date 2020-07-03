using System;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pbft_demo
{
    class Server
    {
        List<IPAddress> Access_List;
        public delegate void Message_Received_Event_Handler(object source,string  s,IPAddress ip);
        public event Message_Received_Event_Handler Message_Received;

        public Server(List<IPAddress> ACL){
            Access_List=ACL;
        }

        public void Open(IPAddress addr,int port){
           
            // configuire the server to accept requests only from the members list
            TcpListener listener =new TcpListener(addr,port);
            listener.Start();
            while (true)
            {
                TcpClient client =listener.AcceptTcpClient();
                // filter whom to accept
                IPEndPoint ip=(IPEndPoint)client.Client.RemoteEndPoint;
                //verify if the client exist in the access list
                if(Access_List.Contains(ip.Address)){
                Task.Run(()=>Handle_Client(client,ip));
                }
            }
        }

        private async void Handle_Client(TcpClient client,IPEndPoint ip)
        {
            NetworkStream stream=client.GetStream();
            StreamReader reader=new StreamReader(stream);

                String message=await reader.ReadLineAsync();    
                if(Message_Received!=null){
                    Message_Received(this,message,ip.Address);
                }
        }
    }
}