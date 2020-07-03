using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace Pbft_demo
{
    public class Configs
    {
        byte[] private_key ;
        public byte[] PrivateKey {get {return private_key ;} }
        public byte[] PublicKey {get ;set;}
        public ECDsa Ecc=ECDsa.Create();
        string Path { get; set; } = "/App";
        public static IPAddress My_address;

        public static List<IPAddress> members ;
         
        public Configs()
        {
            // initialize the key pair
            System.Console.WriteLine();
            Initialize_key_Pair();
            members  =new List<IPAddress>(){
                IPAddress.Parse("172.17.0.2"),
                IPAddress.Parse("172.17.0.3"),
                IPAddress.Parse("172.17.0.4"),
                IPAddress.Parse("172.17.0.5"),
            };
                        
            //get my ip address
            string hostname=Dns.GetHostName();
            var ip=Dns.GetHostEntry(hostname);
            My_address=ip.AddressList[0];

            Console.WriteLine("node id is : "+Configs.My_address);
            Console.WriteLine();


        }
        public void Initialize_key_Pair(){
             
         if (File.Exists("Keys/PUBkey.txt"))
            {
                Console.WriteLine("retreiving keys");
                using (FileStream fspub=File.OpenRead("Keys/PUBkey.txt"))
                using (FileStream fsprv=File.OpenRead("Keys/PRVkey.txt"))
                {
                    PublicKey= new byte[fspub.Length];
                    private_key = new byte[fsprv.Length];
               
                        fspub.Read(PublicKey, 0,PublicKey.Length);
                        fsprv.Read(PrivateKey, 0, PrivateKey.Length);

                        //import the keys to the current ECDSA object
                        int bytes,bytes2 = 0; 
                        Span<byte> imported_prv_key=new Span<byte>(PrivateKey);
                        Span<byte> imported_pub_key=new Span<byte>(PublicKey);
                        
                        Ecc.ImportECPrivateKey(imported_prv_key,out bytes);
                        Ecc.ImportSubjectPublicKeyInfo(imported_pub_key,out bytes2);
                                                
                }
            }
            else
            {
                Console.WriteLine("creating new keys");
                
                using (FileStream fspub = File.Create("Keys/PUBkey.txt"))
                using (FileStream fsprv = File.Create("Keys/PRVkey.txt"))
                {
                    PublicKey = Ecc.ExportSubjectPublicKeyInfo();
                    private_key = Ecc.ExportECPrivateKey();
                    fspub.Write(PublicKey, 0, PublicKey.Length);
                    fsprv.Write(PrivateKey, 0, PrivateKey.Length);
                }
            }
        }


        
    }
}