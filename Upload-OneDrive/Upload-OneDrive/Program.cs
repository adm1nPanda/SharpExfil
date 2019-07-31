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
        private static string clientId = "28c06217-61b6-4838-90a4-793ef51313ac";
        private static string tenantId = "8d5ac6ca-308b-420d-a91f-05f879c4f8df";



        private static async Task<AuthenticationResult> GetATokenForGraph()
        {

            string authority = $"https://login.microsoftonline.com/{tenantId}/";
            string[] scopes = new string[] { "User.Read", "Files.ReadWrite.All" };
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

            String uri = "https://graph.microsoft.com/v1.0/drive/root:/testfoldera/testa.exe:/content";
            var bytes = Encoding.ASCII.GetBytes("AAAAAAAAAAAA");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "PUT";
           
            request.Headers["Authorization"] =  aa.CreateAuthorizationHeader();
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
                Console.WriteLine("Update completed");
            else
                Console.WriteLine(response.GetResponseStream().ToString());
        
        }
    }
}






