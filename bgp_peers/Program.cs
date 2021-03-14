using System;
using System.IO;

namespace bgp_peers
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            if (args.Length==0)
                Console.WriteLine("For discovery use parameters:\nE.g.\n bgp_peers discovery");
            else if (args[0]=="discover")
            {

                StreamReader sr = new StreamReader("AllPeers.txt");
                string CommandResult = sr.ReadToEnd();
                sr.Close();

                cBGPPeers Peers = new cBGPPeers(CommandResult);
                string AllElements = Peers.GetJson();
                Console.WriteLine(AllElements);






                
                

            }



        }
    }
}
