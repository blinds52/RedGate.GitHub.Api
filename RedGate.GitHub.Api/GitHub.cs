using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace RedGate.GitHub.Api.GitHub
{
    public class GitHub : IGitHub
    {
        public string Username { get; set; }
        public string APIToken { get; set; }
        public string Organisation { get; set; }

        private static readonly Uri c_ApiBaseURI = new Uri("https://api.github.com/");
        private ILog m_Log = LogManager.GetLogger(typeof(GitHub));
        
        public IEnumerable<GitHubRepository> GetRepositories()
        {
            JArray repositories = GetJsonCollectionAuthenticated(string.Format("/orgs/{0}/repos", Uri.EscapeDataString(Organisation)), true);
            foreach (var repo in repositories)
                yield return new GitHubRepository(repo["full_name"].Value<string>());
        }

        public IEnumerable<GitHubUser> GetAllUsers()
        {
            JArray members = GetJsonCollectionAuthenticated(string.Format("/orgs/{0}/members", Uri.EscapeDataString(Organisation)), false);
            foreach (var member in members)
                yield return new GitHubUser(member["login"].Value<string>());
        }

        public IEnumerable<GitHubUser> GetAllUsersWithout2FA()
        {
            JArray members = GetJsonCollectionAuthenticated(string.Format("/orgs/{0}/members?filter=2fa_disabled", Uri.EscapeDataString(Organisation)), false);
            foreach (var member in members)
                yield return new GitHubUser(member["login"].Value<string>());
        }

        public IEnumerable<GitHubTeam> GetTeams()
        {
            JArray teams = GetJsonCollectionAuthenticated(string.Format("/orgs/{0}/teams", Uri.EscapeDataString(Organisation)), false);
            foreach (var team in teams)
                yield return new GitHubTeam(team["url"].Value<string>(), team["name"].Value<string>());
        }

        public void CreateTeam(string name)
        {
            throw new NotImplementedException();
        }

        public void CreateRepository(string name)
        {
            var createRepoModel = new CreateRepoModel { Name = name, Private = true, AutoInit = true, GitIgnoreTemplate = "CSharp" };
            var result = PushAuthenticatedJson("POST", Path.Combine("orgs", Organisation, "repos"), createRepoModel);
        }

        private class CreateRepoModel
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }
            [JsonProperty(PropertyName = "private")]
            public bool Private { get; set; }
            [JsonProperty(PropertyName="gitignore_template")]
            public string GitIgnoreTemplate { get; set; }
            [JsonProperty(PropertyName = "auto_init")]
            public bool AutoInit { get; set; }
        }

        public IEnumerable<GitHubTeam> GetTeams(GitHubRepository repo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GitHubUser> GetTeamMembers(GitHubTeam team)
        {
            JArray members = GetJsonCollectionAuthenticated(team.URL + "/members", false);
            foreach (var member in members)
                yield return new GitHubUser(member["login"].Value<string>());
        }

        public void AddTeamMember(GitHubTeam team, string username)
        {
            PushAuthenticatedJson("PUT", Path.Combine(team.URL, "members", username), null);
        }

        public void RemoveTeamMember(GitHubTeam team, string username)
        {
            PushAuthenticatedJson("DELETE", Path.Combine(team.URL, "members", username), null);
        }

        public IEnumerable<GitHubRepository> GetTeamRepositories(GitHubTeam team)
        {
            JArray repos = GetJsonCollectionAuthenticated(team.URL + "/repos", false);
            foreach (var repo in repos)
                yield return new GitHubRepository(repo["full_name"].Value<string>());
        }

        public void AddTeamToRepository(GitHubRepository repo, GitHubTeam team)
        {
            PushAuthenticatedJson("PUT", Path.Combine(team.URL, "repos", Organisation, repo.Name), null);
        }

        private JArray GetJsonCollectionAuthenticated(string url, bool paged)
        {
            try
            {
                // Ensure the URL is absolute - add the base URI if it's currently relative
                Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                    uri = new Uri(c_ApiBaseURI, uri.OriginalString);

                // Put the auth token on if necessary
                uri = HttpRequestHelper.AddParameter(uri, "access_token", APIToken);

                JArray allTheData = new JArray();

                // Start at page 1...
                int page = 1;
                while (true)
                {
                    HttpStatusCode statusCode;
                    int retries = 5;
                    string rawJson;
                    NameValueCollection headers = new NameValueCollection();
                    do
                    {
                        rawJson = HttpRequestHelper.PerformHttpGet(HttpRequestHelper.AddParameter(uri, "page", page.ToString()).AbsoluteUri, out statusCode);
                    } while (statusCode == HttpStatusCode.BadGateway && retries-- > 0);

                    if( statusCode > HttpStatusCode.BadRequest )
                        throw new Exception(string.Format("HttpStatusCode: {0}({1})", statusCode, (int)statusCode));
                    
                    JArray data = JsonSerializer.Create().Deserialize<JArray>(new JsonTextReader(new StringReader(rawJson)));

                    foreach (var obj in data)
                        allTheData.Add(obj);

                    if (data.Count == 0 || !paged)
                        break;
                    page++;
                }
                return allTheData;
            }
            catch (Exception ex)
            {
                m_Log.ErrorFormat("Error getting {0} : {1}", url, ex);
                throw;
            }
        }

        private JObject PushAuthenticatedJson(string method, string url, object postBodyObject)
        {
            // Ensure the URL is absolute - add the base URI if it's currently relative
            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = new Uri(c_ApiBaseURI, uri.OriginalString);

            // Put the auth token on if necessary
            uri = HttpRequestHelper.AddParameter(uri, "access_token", APIToken);

            HttpStatusCode status;
            JsonSerializer serializer = JsonSerializer.Create();
            string bodyContent;
            using (StringWriter stringWriter = new StringWriter())
            {
                serializer.Serialize(new JsonTextWriter(stringWriter), postBodyObject);
                bodyContent = stringWriter.ToString();
            }

            string result = HttpRequestHelper.PerformHttpRequest(uri.AbsoluteUri, null, out status, request =>
            {
                request.Method = method;
                using (StreamWriter bodyWriter = new StreamWriter(request.GetRequestStream()))
                {
                    bodyWriter.Write(bodyContent);
                }
            });
            return serializer.Deserialize<JObject>(new JsonTextReader(new StringReader(result)));
        }
    }
}
