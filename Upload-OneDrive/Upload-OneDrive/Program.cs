using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Upload_OneDrive
{
    class Program
    {
        private static string clientId = "28c06217-61b6-4838-90a4-793ef51313ac";
        private static string tenantId = "8d5ac6ca-308b-420d-a91f-05f879c4f8df";

       
        private static async Task<AuthenticationResult> GetATokenForGraph()
        {

            string authority = $"https://login.microsoftonline.com/{tenantId}/";
            string[] scopes = new string[] { "User.Read", "User.Read.All", "Files.ReadWrite.All" };
            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/token")
                .Build();


            AuthenticationResult result = null;
            var accounts = await app.GetAccountsAsync();

            // All AcquireToken* methods store the tokens in the cache, so check the cache first
            try
            {
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
                return result;
            }
            catch (MsalUiRequiredException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
            }

            try
            {
                result = await app.AcquireTokenWithDeviceCode(scopes,
                      deviceCodeCallback =>
                      {

                          Console.WriteLine(deviceCodeCallback.Message);
                          return Task.FromResult(0);
                      }).ExecuteAsync();

                Console.WriteLine("Received token from {0}", result.Account.Username);
                return result;
            }
            catch (MsalServiceException ex)
            {
                Console.WriteLine("Error : {0}", ex.Message);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation Cancelled");
            }
            catch (MsalClientException)
            {
                Console.WriteLine("Verification Code Expired");
            }

            return null;
        }

        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                
                Console.ResetColor();
            }

        }


        private static async Task RunAsync()
        {
            AuthenticationResult aa = await GetATokenForGraph();
            
            

            string[] uris = { "https://graph.microsoft.com/v1.0/me", "https://graph.microsoft.com/v1.0/me/events", "https://graph.microsoft.com/v1.0/users", "https://graph.microsoft.com/v1.0/drive/root", "https://graph.microsoft.com/v1.0/me/drive/root/children" };
            foreach (string uri in uris)
            {
                var HttpClient = new HttpClient();

                var defaultRequestHeaders = HttpClient.DefaultRequestHeaders;
                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", aa.AccessToken);

                Console.WriteLine("Response from query - {0}",uri);
                HttpResponseMessage response = await HttpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject result = JsonConvert.DeserializeObject(json) as JObject;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(result.ToString());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    string content = await response.Content.ReadAsStringAsync();

                    // It's ok to not have a manager
                    if (!content.Contains("Resource 'manager' does not exist"))
                    {
                        Console.WriteLine($"Failed to call the Web Api: {response.StatusCode}");
                        Console.WriteLine($"Content: {content}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("No manager");
                    }
                }
                Console.ResetColor();


            }



        }
    }
}






