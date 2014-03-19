using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGate.GitHub.Api.Tests
{
    [TestFixture]
    public class ApiSanityChecks
    {
        private const string c_ApiKey = "";
        private const string c_Organisation = "red-gate";

        private GitHub.GitHub m_GitHub;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_GitHub = new GitHub.GitHub();
            m_GitHub.Organisation = c_Organisation;
            m_GitHub.APIToken = c_ApiKey;
        }

        [Test]
        public void GetRedGateTeamMembers()
        {
            var teams = m_GitHub.GetTeams().ToList();
            var readAccessTeam = teams.Where(t => t.Name == "RG read access").First();
            var members = m_GitHub.GetTeamMembers(readAccessTeam).ToList();
            Assert.That(members.Count() > 30, "Expected more than one page of members");
        }

        [Test]
        public void GetAllMembers()
        {
            var members = m_GitHub.GetAllUsers().ToList();
            Assert.That(members.Count() > 30, "Expected more than one page of members");
        }
    }
}
