using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2.Helpers
{
    public class Connect
    {
        public static string Token(string authUrl, string clientId, string clientSecret, string callbackUrl)
        {
            Token token = Auth.GetRefreshToken(authUrl, clientId, clientSecret, callbackUrl);
            if (string.IsNullOrEmpty(token?.RefreshToken))
                throw new Exception("Can not get refresh token");

            Console.WriteLine(String.Format("Your Refresh Token:{0}", token?.RefreshToken));
            Console.WriteLine(String.Format("Your Access Token:{0}", token?.AccessToken));
            Console.WriteLine(String.Format("Save your Refresh token and use to update expired access tokens"));
            Console.WriteLine(String.Format("Refreshing access token..."));
            // Refreshing access token
            string accessToken = Auth.RefreshAccessToken(authUrl, clientId, clientSecret, token?.RefreshToken);
            if (string.IsNullOrEmpty(token?.RefreshToken))
                throw new Exception("Can not get access token");


            return accessToken;


            //client = new RestClient("https://developers.beproduct.com/");

        }

    }
}
