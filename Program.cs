using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace Pbft_demo
{
    class Program
    {
       static System.Timers.Timer timer =new System.Timers.Timer(1000);
        static void Main(string[] args)
        {
            Node  node=new Node();
            node.Start_Consensus();

        }
    }
}
