using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGate.GitHub.Api.GitHub
{
    public sealed class GitHubUser
    {
        public string Username { get; private set; }

        internal GitHubUser(string username)
        {
            Username = username;
        }

        public override string ToString()
        {
            return Username;
        }

    }
}
