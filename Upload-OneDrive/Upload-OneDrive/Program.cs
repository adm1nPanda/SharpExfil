using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Net;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Upload_OneDrive
{
    class Program
    {
        private static string clientId = "48dfa3bc-fa28-4c40-8933-cd0bd70a3c25";
        private static string AzureCloudInstance = "AzurePublic";
        private static string Tenant = "organizations";
        private static string MicrosoftGraphBaseEndpoint = "https://graph.microsoft.com";


        

        private static async Task<AuthenticationResult> GetATokenForGraph()
        {

            string authority = "https://login.microsoftonline.com/8d5ac6ca-308b-420d-a91f-05f879c4f8df/";
            string[] scopes = new string[] { "Files.ReadWrite.All" };
            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
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
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            
        }


        private static async Task RunAsync()
        {
            AuthenticationResult aa = await GetATokenForGraph();
            Console.WriteLine("Access Token : {0}",aa.AccessToken);
        }


    }
}






