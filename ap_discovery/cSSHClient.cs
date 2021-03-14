using Renci.SshNet;

namespace ap_discovery
{
    class cSSHClient
    {
        protected SshClient SSH;

        public cSSHClient(string name, int port, string login, string password)
        {
            SSH = new SshClient(name, port, login, password);
        }

        public int Connect()
        {
            try
            {
                SSH.Connect();
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        public string GetCommandResult(string command)
        {
            return SSH.CreateCommand(command).Execute();
        }

        public void Disconnect()
        {
            SSH.Disconnect();
        }
    }


}