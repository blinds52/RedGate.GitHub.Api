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
    internal static class HttpRequestHelper
    {
        private static ILog s_Log = LogManager.GetLogger(typeof(HttpRequestHelper));

        internal static Uri AddParameter(Uri uri, string param, string val)
        {
            if (string.IsNullOrEmpty(uri.Query))
            {
                return new Uri(string.Format("{0}?{1}={2}", uri.AbsoluteUri, Uri.EscapeDataString(param), Uri.EscapeDataString(val)));
            }
            else if (!uri.Query.Contains(Uri.EscapeDataString(param) + "="))
            {
                return new Uri(string.Format("{0}&{1}={2}", uri.AbsoluteUri, param, val));
            }
            else
            {
                // Should we be able to replace a param in place?
                throw new InvalidOperationException("URI already contains " + param);
            }
        }

        internal static string PerformHttpGet(string url, out HttpStatusCode status)
        {            
            return PerformHttpGet(url, null, out status);
        }

        internal static string PerformHttpGet(string url, NameValueCollection headers, out HttpStatusCode status)
        {
            return PerformHttpRequest(url, headers, out status, null);
        }

        internal static string PerformHttpRequest(string url, NameValueCollection headers, out HttpStatusCode status, Action<HttpWebRequest> requestAction)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.UserAgent = "RedGateAdmin";
            if (headers != null)
            {
                foreach (string key in headers.AllKeys)
                {
                    switch (key)
                    {
                        case "Content-Type":
                            request.ContentType = headers[key];
                            break;
                        case "Accept":
                            request.Accept = headers[key];
                            break;
                        default:
                            request.Headers[key] = headers[key];
                            break;
                    }
                }
            }


            if (requestAction != null)
                requestAction(request);

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                status = response.StatusCode;
                if ((int)response.StatusCode >= 300)
                {
                    return null;
                }
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                try
                {
                    if (ex.Response == null)
                    {
                        throw;
                    }
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    status = response != null ? response.StatusCode : HttpStatusCode.Unused;
                    s_Log.DebugFormat("Response code to {0} was {1} : {2}", url, status, ex.Response.Headers);

                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        status = ((HttpWebResponse)ex.Response).StatusCode;
                        return reader.ReadToEnd();
                    }
                }
                catch
                {
                    // fall through to rethrow original
                }
                throw;
            }
        }
    }
}
