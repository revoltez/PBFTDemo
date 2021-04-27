using System.Timers;

namespace Pbft_demo
{
    class Timeout
    {
        public Timer Idle_Timeout =new Timer(20000);
        public Timer Commit_Timeout=new Timer(20000);
        public Timer View_Change_Timeout =new Timer(30000);
        Node node;
        string timeout="idle";
        public Timeout (Node n){
            node=n;
            Set_View_Change_Timeout();
        }
        public void Set_Idle_Timeout(){
            Idle_Timeout.Stop();
            Idle_Timeout=new System.Timers.Timer(15000);
            Idle_Timeout.AutoReset=false;
            timeout="idle timeout elapsed";
            Idle_Timeout.Elapsed+=(sender,e)=>node.View_Change(sender,e,timeout);
            Idle_Timeout.Start();
        }
        public void Set_View_Change_Timeout(){
            View_Change_Timeout.Stop();
            View_Change_Timeout=new System.Timers.Timer(30000);
            View_Change_Timeout.AutoReset=false;
            timeout="view change timeout elapsed";
            View_Change_Timeout.Elapsed+=(sender,e)=>node.View_Change(sender,e,timeout);;
            View_Change_Timeout.Start();
        }
        public void Set_Commit_Timeout(){

            View_Change_Timeout.Stop();
            View_Change_Timeout=new System.Timers.Timer(10000);
            View_Change_Timeout.AutoReset=false;
            timeout="commit timeout elapsed";
            View_Change_Timeout.Elapsed+=(sender,e)=>node.View_Change(sender,e,timeout);;
            View_Change_Timeout.Start();
        }
        

    }
}