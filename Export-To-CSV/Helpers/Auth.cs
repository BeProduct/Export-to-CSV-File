using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OAuth2.Helpers
{
    public class Token
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }

    public static class Auth
    {
        /// <summary>
        /// Performs authentication through default browser and beproduct auth server
        /// </summary>
        /// <param name="authEndpointUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        public static Token GetRefreshToken(string authEndpointUrl, string clientId, string clientSecret, string callbackUrl)
        {
            // Get Authorization Code and an Approval from the User
            var client = new RestClient(authEndpointUrl);
            var request = new RestRequest(
                string.Format(
                "/connect/authorize?client_id={0}" +
                "&redirect_uri={1}" +
                "&response_type=code&scope=openid+profile+email+roles+offline_access+BeProductPublicApi",
                clientId, HttpUtility.UrlEncode(callbackUrl)), Method.POST);
            bool bHasUserGrantedAccess = false;
            var url = client.BuildUri(request).ToString();

            // Set up a local HTTP server to accept authentication callback
            string auth_code = null;

            var resetEvent = new System.Threading.ManualResetEvent(false);
            using (var svr = Server.Create(callbackUrl, context =>
            {
                var qs = HttpUtility.ParseQueryString(context.Request.RawUrl);
                auth_code = qs["/?code"];
                if (!string.IsNullOrEmpty(auth_code))
                {
                    // The user has granted access
                    bHasUserGrantedAccess = true;
                }
                // Resume execution...
                resetEvent.Set();

            }))
            {
                // Launch a default browser to get the user's approval
                System.Diagnostics.Process.Start(url);
                // Wait until the user decides whether to grant access
                resetEvent.WaitOne();

            }

            if (false == bHasUserGrantedAccess)
            {
                // The user has not granded access
                return null;
            }

            string authorizationCode = auth_code;

            //  Using auth code to retrieve refresh token and access token
            request = new RestRequest("connect/token", Method.POST);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", authorizationCode);
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("redirect_uri", callbackUrl);
            IRestResponse<dynamic> response = client.Execute<dynamic>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new Token
                {
                    RefreshToken = response.Data["refresh_token"],
                    AccessToken = response.Data["access_token"],
                };

            }
            else
            {
                Environment.Exit(0);
                return null;
            }

        }

        /// <summary>
        /// Refreshing access token with refresh token
        /// </summary>
        /// <param name="authEndpointUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static string RefreshAccessToken(string authEndpointUrl, string clientId, string clientSecret, string refreshToken)
        {
            var client = new RestClient(authEndpointUrl);
            var request = new RestRequest("/connect/token", Method.POST);

            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("refresh_token", refreshToken);

            var response = client.Execute<dynamic>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Environment.Exit(0);
            }

            return response.Data["access_token"];
        }
    }
}
