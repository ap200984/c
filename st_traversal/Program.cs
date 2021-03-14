using System;

namespace st_traversal
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
                Console.WriteLine("usage: st_traversal [AP IP] [port number for ssh] [login] [password]");
            else
            {
                cAP AP = new cAP(args[0], Convert.ToInt32(args[1]), args[2], args[3]);
                AP.GetStations();
                AP.Print();
            }

        }
    }
}
