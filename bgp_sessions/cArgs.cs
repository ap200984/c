using System.Collections.Generic;

namespace BGP_sessions2
{
    public class cArgs
    {
        private List<string> AllArgs;

        public cArgs(string[] _args)
        {
            AllArgs = new List<string>();
            foreach (string s in _args)
                AllArgs.Add(s);
        }

        public bool Contains(string s)
        {
            return AllArgs.Contains(s);
        }

        public string GetNextArgAfter(string s)
        {
            for (int i=0; i<AllArgs.Count; i++)
            {
                string a = AllArgs[i];
                if (a==s)
                    if (AllArgs.Count>i+1)
                        return AllArgs[i+1];
            }
            return "no args after";

        }
    }
}