using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGate.GitHub.Api.GitHub
{
    public sealed class GitHubRepository
    {
        public string Name { get; private set; }

        public GitHubRepository(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
