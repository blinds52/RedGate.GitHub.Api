using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGate.GitHub.Api.GitHub
{
    public interface IGitHub
    {
        IEnumerable<GitHubRepository> GetRepositories();
        IEnumerable<GitHubUser> GetAllUsers();
        IEnumerable<GitHubUser> GetAllUsersWithout2FA();
        
        void CreateTeam(string name);
        void CreateRepository(string name);

        IEnumerable<GitHubTeam> GetTeams(GitHubRepository repo);
        IEnumerable<GitHubTeam> GetTeams();

        IEnumerable<GitHubUser> GetTeamMembers(GitHubTeam team);
        void AddTeamMember(GitHubTeam team, string username);
        void RemoveTeamMember(GitHubTeam team, string username);
        IEnumerable<GitHubRepository> GetTeamRepositories(GitHubTeam team);
        void AddTeamToRepository(GitHubRepository repo, GitHubTeam team);
    }
}
