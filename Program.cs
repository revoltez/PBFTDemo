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
        public static void PrintByteArray(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write($"{array[i]:X2}");
            }
            Console.WriteLine();
        }
    }
    public static class  Mystringextensions {
    public static void Dump(this string str)=>Console.WriteLine(str);
    }
}
