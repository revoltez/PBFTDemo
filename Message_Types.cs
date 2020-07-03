namespace Message_Types
{
    public enum Message_Type  {preprepare, prepare, commit, view_change,
        new_view
    }
    public class pbft_message
    {
        public Message_Type message_Type;
        public byte[] id  ;
        public int view_number;
        public int seq;
        public byte[] digest;
        public string message;

        public override string ToString(){
            return ""+message_Type+id+view_number+seq+message;
        }
    }

    public class view_change :pbft_message{
        public int Primary_Index;
    }

    public class Temp_Message :pbft_message{
        public int Prepare_counter;
        public int commit_counter;
    }

    public class Temp_view_Message :pbft_message
    {
        public delegate void new_primary_delegate();
        public event new_primary_delegate New_Primary_Event;
        int view_counter=0;
        public int View_Counter{get {return view_counter;} 
         set{
            view_counter=value;
            if(view_counter == 3){
                //call new_primary method 
                New_Primary_Event();
                
            }
            ;} 
        }
    }

}