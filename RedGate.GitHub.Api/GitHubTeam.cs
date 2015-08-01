using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGate.GitHub.Api.GitHub
{
    public class GitHubTeam
    {
        public string Name { get; private set; }
        public string URL { get; private set; }

        public GitHubTeam(string url, string name)
        {
            Name = name;
            URL = url;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
