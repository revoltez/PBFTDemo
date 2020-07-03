using System.Collections.Generic;
using System.Net;

namespace Pbft_demo
{
    public enum Phase
        { idle, Preprepare, prepare, commit }
    public enum Mode{
        Normale, View_Change
    }
    public class State
    {
        public Phase phase =Phase.Preprepare;
        bool is_primary;
        int primary_index;
        public int Primary_Index { get {
            primary_index = view_number % Configs.members.Count;
            return primary_index;
        }
        }
        public bool Is_Primary { get {
            if(Configs.members[Primary_Index].Equals(Configs.My_address)){
               is_primary=true; 
            }else
            {
                is_primary=false;
            }
            return is_primary;
        }
        }
        public Mode mode = Mode.Normale;
        public int view_number =0;
        int  seq=0;
        public int Seq { 
            // get and set the sequence number from configuration files            
            get {seq= Confirmed_messages.Count +1;
                 return seq ;} 
            // set the seq number in the configuration files
            set { seq =value ;} 
        }
        public List<string> Confirmed_messages =new List<string>();
        public List<string> log {get; set;}
    }
}
