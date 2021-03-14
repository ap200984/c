namespace get_commits
{ 
    public class cSTR
    {            
        public string spaces(int n, string s)
        {
            if (s==null)
                s="";            
            string sp="";
            for (int i=0; i<n-s.Length; i++)
                sp+=" ";
            return sp;
        }
    }
}