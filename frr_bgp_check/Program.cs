using System;

namespace frr_bgp_check
{
    class Program
    {
        static void Main(string[] args)
        {
            cBGP_Neighbors N = new cBGP_Neighbors();

            if (args.Length < 1)
            {
                Console.WriteLine("To discpver BGP neighbors use \"discover\" option");
                Console.WriteLine("To get info about neighbor use [IP] option");
                return;
            }

            if (args[0] == "discover")
            {
                //string res = ssh.GetCommandResult("sudo vtysh -c \"sh ip bgp sum\"");
                cCommand CMD = new cCommand("sudo vtysh -c \"show ip bgp summary\"");
                string res = CMD.Execute();
                Console.WriteLine(N.Discover_Neighbors(res));
            }
            else
            {
                //string res = ssh.GetCommandResult("sudo vtysh -c \"sh ip bgp nei\" | grep \"BGP neighbor is\\|Description\\|state\\|Last read\"");
                cCommand CMD = new cCommand("sudo vtysh -c \"show ip bgp neighbor\"");
                string res = CMD.Execute();
                Console.WriteLine(N.GetNeighborINFO(res, args[0]));
            }
        }
    }
}
