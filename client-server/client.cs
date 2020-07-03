using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Pbft_demo
{
    class Client
    {
        public void connect(IPAddress address,int port,PBFT_Messages.pbft_message message){
            TcpClient client =new TcpClient();
            try
            {
                client.Connect(address,port);
                StreamWriter writer=new StreamWriter(client.GetStream());
                string Message=Newtonsoft.Json.JsonConvert.SerializeObject(message);
                writer.WriteLine(Message);
                writer.Flush();

            }

            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            
            finally
            {
                client.Close();
            }
        }
    }
}