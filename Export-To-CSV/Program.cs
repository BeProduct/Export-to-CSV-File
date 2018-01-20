using Jitbit.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAuth2.Helpers;
using RestSharp;
using System;
using System.IO;

namespace Export_To_CSV
{
    class Program
    {
        static readonly string apiUrl = "http://developers.beproduct.com/";
        static readonly string authUrl = " https://id.winks.io/ids";
        static readonly string callbackUrl = "http://localhost:8888/";
        static string accessToken = "";
        static string refresh_token = "";

        static readonly string clientId = "[ENTER CLIENT ID]";
        static readonly string clientSecret = "[ENTER CLIENT SECRET]";
        static readonly string companyName = "[ENTER YOUR COMPANY NAME]";
        static string styleFolderId = "[ENTER STYLE MASTER FOLDER ID]";
        static string exportFilePath = @"c:\temp";
        static string exportFileName = @"result.csv";

        static void Main(string[] args)
        {
            GetAccessToken();
            ExportToCSV();

            Console.WriteLine("Yay! CSV file exported... ");
            Console.WriteLine(Path.Combine(exportFilePath, exportFileName));
            Console.WriteLine("Press any key...");
            Console.ReadKey();

        }

        /// <summary>
        /// Request Style Headers API.
        /// To add and remove fields, go to admin > master folder > style folder > select style type > search
        /// Drag and drop to add or remove attribute fields.
        /// </summary>
        /// <returns></returns>
        private static dynamic RequestStyleHeaders()
        {
            Console.WriteLine("Calling Style Header API ...");
            var client = new RestClient(apiUrl);
            var request = new RestRequest($"/api/{companyName}/Style/Headers?folderId={styleFolderId}&pageSize=200&pageNumber=0", Method.POST);
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { filters = new object[] { } });
            var response = client.Execute<dynamic>(request);

            return JsonConvert.DeserializeObject<dynamic>(response.Content);
        }

        /// <summary>
        /// Export style attributes to CSV file.
        /// </summary>
        private static void ExportToCSV()
        {
            var result = RequestStyleHeaders();
            var resultData = ((JProperty)((JContainer)result).First).Value;
            int resultCount = ((JContainer)resultData).Count;

            Console.WriteLine($"Exporting {resultCount} styles to CSV file ...");

            var csvExport = new CsvExport();
            for (int i = 0; i < resultCount; i++)
            {
                var fields = resultData[i]["headerData"]["fields"];
                csvExport.AddRow();
                foreach (var item in fields)
                {
                    var name = item["name"];
                    var value = item["value"];
                    csvExport[$"{name}"] = value;
                }
            }
            csvExport.ExportToFile(Path.Combine(exportFilePath, exportFileName));
        }

        /// <summary>
        /// Get Access Token
        /// The access token is a credential that can be used by a client to access BeProduct API.
        /// The access token should be used as a Bearer credential and transmitted in an HTTP Authorization header to the API.
        /// </summary>
        private static void GetAccessToken()
        {
            Console.WriteLine("Getting access token ...");
            refresh_token = Auth.GetRefreshToken(authUrl, clientId, clientSecret, callbackUrl)?.RefreshToken;
            accessToken = Auth.RefreshAccessToken(authUrl, clientId, clientSecret, refresh_token);
        }
    }
}
