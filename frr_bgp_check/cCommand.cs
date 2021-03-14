using System;
using System.IO;
using System.Diagnostics;

namespace frr_bgp_check
{
    public class cCommand
    {
        public string cmd;

        public cCommand(string _cmd)
        {
            cmd = _cmd;
        }

        public string Execute()
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "/usr/bin/sudo";
                process.StartInfo.Arguments = cmd;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                process.WaitForExit();
                process.Close();

                return output;
            }
            catch
            {
                return null;
            }
        }

    }
}