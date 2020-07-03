using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using PBFT_Messages;

namespace Pbft_demo
{
    public class Node
    {
        public Configs configs;
        Server server;
        Client client;
        public State state;
        Timeout timeout;
        Dictionary<Tuple<int,int>,Temp_Message> Temporary_Messages=new Dictionary<Tuple<int,int>, Temp_Message>();
        static readonly object _locker = new object();
        Dictionary<int,Temp_view_Message> Temporary_View_Change_messages=new Dictionary<int, Temp_view_Message>();
        List<string> Transaction_Pool=new List<string>();

        public Task Create_Block_Task { get; set; }

        public Node()
        {
            // load the configs
            configs=new Configs();
            state=new State();

            timeout=new Timeout(this);
            client=new Client();
            server=new Server(Configs.members); 
            // register the node for the Message_recieved event and open the server to listen for client request 
            server.Message_Received += On_Message_Received;

            timeout.Set_Idle_Timeout();
        }

        public void Start_Consensus(){
            //configuire the state
            //start the server Task
            Console.WriteLine("Server Started");
            Task Server_task=new Task(()=>server.Open(Configs.My_address,5000));
            Server_task.Start();
            //check if primary by comparing with own ip address
            if (state.Is_Primary){
                timeout.Idle_Timeout.Close();
                Send_Message(Sign_Message(Create_Block()));
            }
            Server_task.Wait();
        }

        public void On_Message_Received(object source,string message,IPAddress ip){
            lock (_locker)
            {
            PBFT_Messages.pbft_message Message= Newtonsoft.Json.JsonConvert.DeserializeObject<PBFT_Messages.pbft_message>(message); 
            System.Console.WriteLine("received a "+Message.message_Type+" message from node :"+ip+"\n message content is : "+Message.message +"with seq number : "+Message.seq);
            // verify the validity of the message and proceed
            Process_Message(Message,ip);
            }
        }

        public void Add_To_Temp(Func<bool> predicate,pbft_message Message,Tuple<int,int> Message_Tuple){
            if(!predicate()){
                Temp_Message temp=new Temp_Message(){
                commit_counter=0,
                Prepare_counter=0,
                message=Message.message,
                id=Message.id,
                seq=Message.seq,
                view_number=Message.view_number,  
                };
            Temporary_Messages.Add(Message_Tuple,temp);
            System.Console.WriteLine("Message with sequence number"+Message.seq+" and viewnumber "+Message.view_number+" added to the temporary pool");
            }
        }

        public void Add_To_Temp_View(pbft_message Message){
                if(! Temporary_View_Change_messages.ContainsKey(Message.view_number)){
                        Temp_view_Message temp=new Temp_view_Message(){
                        message=Message.message,
                        id=Message.id,
                        seq=Message.seq,
                        view_number=Message.view_number,  
                    };
                    temp.New_Primary_Event+= New_Primary;
                    Temporary_View_Change_messages.Add(Message.view_number,temp);
                }
            //verify if this has trigered a new primary call already or not
            Temporary_View_Change_messages[Message.view_number].View_Counter ++;
        }

        public bool Process_Message(PBFT_Messages.pbft_message Message,IPAddress Sender_Ip){
         
        if(Verify_Message(Message)){
             Tuple<int,int> Message_Tuple=Tuple.Create(Message.seq,Message.view_number);

            switch (Message.message_Type)
            {
                case Message_Type.preprepare:
                    //verify if the preprepare message came from the primary
                    //send a prepare block to everyone
                    System.Console.WriteLine("primary is at index "+state.Primary_Index);
                    if(Configs.members[state.Primary_Index].Equals(Sender_Ip)){
                    //  add the received message to the temporary pool if it doesn't exist
                    Add_To_Temp(()=>Temporary_Messages.ContainsKey(Message_Tuple),Message,Message_Tuple);
                    timeout.Idle_Timeout.Close();
                    var prepare=new pbft_message()
                    {
                        message=Message.message,
                        id=configs.PublicKey,
                        message_Type=Message_Type.prepare,
                        seq=Message.seq,
                        view_number=state.view_number,   
                    };
                    state.phase=Phase.prepare;
                    Temporary_Messages[Message_Tuple].Prepare_counter ++;
                    Send_Message(Sign_Message(prepare));

                    Console.WriteLine(Message.message_Type+" message from node "+Sender_Ip+" processed succesfully ");
                    return true;
                    }
                    else
                    {
                        System.Console.WriteLine(Message.message_Type+" message rejected from node "+Sender_Ip );
                        return false;
                    }
                
                case Message_Type.prepare:
                //check if there is 2f+1 prepare messages in the pool and store the prepare message
                //check if it is the same message of that in the transaction pool
                //if true create a commit block if not increment the counter

                Add_To_Temp(()=>Temporary_Messages.ContainsKey(Message_Tuple),Message,Message_Tuple);

                //this verification is useless and must be changed
                if(Temporary_Messages[Message_Tuple].message==Message.message){

                    Temporary_Messages[Message_Tuple].Prepare_counter ++; 
                    System.Console.WriteLine("prepare counter of message with seq:"+Message.seq+" is : "+Temporary_Messages[Message_Tuple].Prepare_counter);
                
                } 
                if (Temporary_Messages[Message_Tuple].Prepare_counter == 3 )
                {
                    System.Console.WriteLine("Received 2f+1 prepare message ");
                    state.phase=Phase.commit;
                    timeout.Commit_Timeout.Start();
                    //send a commit block
                    var commit=new pbft_message()
                        {
                            message=Message.message,
                            id=configs.PublicKey,
                            message_Type=Message_Type.commit,
                            seq=Message.seq,
                            view_number=state.view_number,   
                        };
                    ;
                    Send_Message(Sign_Message(commit));
 
                }
                break;

                case Message_Type.commit:
                Add_To_Temp(()=>Temporary_Messages.ContainsKey(Message_Tuple),Message,Message_Tuple);
                
                timeout.Set_Commit_Timeout();
                if(Temporary_Messages[Message_Tuple].message==Message.message)
                {
                    Temporary_Messages[Message_Tuple].commit_counter ++;
                }
                // check if there is 2f+1 commit meessage in the pool and store 
                if(Temporary_Messages[Message_Tuple].commit_counter==3){

                    //store the Message and send a validation to the client ,for now just simulate it  
                    timeout.Commit_Timeout.Stop();
                    // this should be in a new view in the case of pbft
                    timeout.Set_Idle_Timeout();
                    state.Confirmed_messages.Add(Message.message);
                    Console.WriteLine("message added to the blockchain"+Message.message);
                    System.Console.WriteLine();
                    
                    //if primary create another block 
                    if (state.Is_Primary){  
                        state.phase=Phase.Preprepare;
                        System.Console.WriteLine("seq num : "+state.Seq);
                        Create_Block_Task = Task.Run(()=>Send_Message(Sign_Message(Create_Block())));
                        }
                Console.WriteLine(Message.message_Type+" message from node "+Sender_Ip+" processed succesfully ");
                return true;
                }

                break;

                case Message_Type.view_change :
                if(Message.view_number >= state.view_number) {Add_To_Temp_View(Message);} else {  return false;}
                
                break;

            }
            Console.WriteLine(Message.message_Type+" message from node processed succesfully ");
            return true;
            }
            System.Console.WriteLine("message rejected");
            return false;
        }

        private pbft_message Create_Block()
        {
                /*the client should send a message from the network and the On_Message_Received will be excuted this must be changed
                THE primary should contact the client and request for a message and then fom that message create a new block*/
                
                Console.WriteLine("waiting for the clinet to send A message");
                System.Console.WriteLine();    
                string Block = Console.ReadLine();
                /* create a new block and this should be in a function with */

                PBFT_Messages.pbft_message new_block=new PBFT_Messages.pbft_message(){
                    message=Block,
                    id=configs.PublicKey,
                    message_Type=Message_Type.preprepare,
                    seq=state.Confirmed_messages.Count+1,
                    view_number=state.view_number,
                };
                Temp_Message temp=new Temp_Message(){
                        commit_counter=0,
                        Prepare_counter=0,
                        message=new_block.message,
                        id=new_block.id,
                        seq=new_block.seq,
                        view_number=new_block.view_number,  
                    };
                Temporary_Messages.Add(Tuple.Create(new_block.seq,new_block.view_number),temp);
                return new_block;
        }

        private bool Verify_Message(pbft_message Message)
        {
            bool Signature_Match=false;
            var data=Encoding.UTF8.GetBytes(Message.ToString());
            using (ECDsa Ecc= ECDsa.Create())
            {
                int bytes2;
                Ecc.ImportSubjectPublicKeyInfo(Message.id,out bytes2);
                Signature_Match = Ecc.VerifyData(data,Message.digest,HashAlgorithmName.SHA256);
            }
            return Signature_Match;
        }

        public pbft_message Sign_Message(PBFT_Messages.pbft_message message){
            byte[] data=Encoding.UTF8.GetBytes(message.ToString());
            byte[] digest=configs.Ecc.SignData(data,0,data.Length,HashAlgorithmName.SHA256);
            message.digest=digest;
            return message;
        }

        private void Send_Message(pbft_message Message)
        {
            //send the message for all connected client 
            foreach (IPAddress item in Configs.members)
            {
                if(!item.Equals(Configs.My_address)){
                client.connect(item,5000,Message);
                }
            }
        }

        public void View_Change(object sender, ElapsedEventArgs e,string timeout)
        {
            System.Console.WriteLine(timeout);
            string View_Message="view change message";
            //create and send a view change message to all other nodes
            state.mode=Mode.View_Change;
            view_change msg=new view_change{
            id=configs.PublicKey,
            message=View_Message,
            seq=state.Seq,
            view_number=state.view_number,
            message_Type=Message_Type.view_change,
            Primary_Index=state.Primary_Index,
            };
            Temp_view_Message Message=new Temp_view_Message{
                id=configs.PublicKey,
                message=View_Message,
                seq=state.Seq,
                view_number=state.view_number,
                message_Type=Message_Type.view_change,
                
            };
            Message.New_Primary_Event+= New_Primary;
            Add_To_Temp_View(Message);
            Send_Message(Sign_Message(msg));
        }
        public void New_Primary(){

                //all this is in the case of new view message
                timeout.Idle_Timeout.Close();
                timeout.Commit_Timeout.Close();
                timeout.Set_Idle_Timeout();
                state.view_number++;
                state.phase=Phase.Preprepare;
                state.mode=Mode.Normale;
                System.Console.WriteLine("switching to another primary");
                System.Console.WriteLine("new primary is : "+Configs.members[state.Primary_Index]);
                System.Console.WriteLine();
                if (state.Is_Primary)
                {
                            //should send a new view message    
                            Create_Block_Task= Task.Run(()=>Send_Message(Sign_Message(Create_Block())));
                }
        }
    
    }
}