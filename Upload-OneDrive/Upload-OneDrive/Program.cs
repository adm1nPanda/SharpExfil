using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Upload_OneDrive
{
    class Program
    {
        
        
        static string cid = "28c06217-61b6-4838-90a4-793ef51313ac";
        static string sco = "user.read.all files.readwrite.all offline_access";
        static string ruri = "http://localhost/auth";
        static string cs = "0KAtXXanUbM/[utu-UjaNQmniNGT=906";



        static void Main()
        {

            //authuntication using code token
            Console.WriteLine($"Authenticate at URL - https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={cid}&scope={sco}&response_type=code&redirect_uri={ruri} . Once completed copy the code token from the browser address bar and input into application");   
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create($"https://login.microsoftonline.com/common/oauth2/v2.0/token");
            var code = Console.ReadLine();
            string postData = $"client_id={cid}&redirect_uri={ruri}&client_secret={cs}&code={code}&grant_type=authorization_code";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();

            Stream outStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(outStream);
            string responseFromServer = reader.ReadToEnd();
            dynamic resp = JObject.Parse(responseFromServer);
            
            response.Close();




            // Requesting URL for large file upload
            string upload = "https://graph.microsoft.com/v1.0/drive/root:/testfoldera:/createUploadSession";
            
            HttpWebRequest rq1 = (HttpWebRequest)HttpWebRequest.Create(upload);
            rq1.Method = "POST";
            rq1.Host = "graph.microsoft.com";
            rq1.Headers.Add("Authorization", "Bearer " + resp.access_token);
            rq1.Accept = "application/json, text/plain, */*";
            rq1.ContentType = "application/json";
            
            String a = "{" +
                            "\"item\": {" +
                                        "\"@odata.type\": \"microsoft.graph.driveItemUploadableProperties\"," +
                                        "\"@microsoft.graph.conflictBehavior\": \"rename\"," +
                                        "\"name\": \"largefile.dat\"" +
                            "}" +
                        "}";

            byte[] data = Encoding.ASCII.GetBytes(a);
            rq1.ContentLength = data.Length;
            using (var requestStream = rq1.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            WebResponse rs1 = rq1.GetResponse();
            outStream = rs1.GetResponseStream();
            reader = new StreamReader(outStream);
            responseFromServer = reader.ReadToEnd();
            dynamic resp1 = JObject.Parse(responseFromServer);
            Console.WriteLine("Received UploadURL: {0}",resp1.uploadUrl);
            rs1.Close();

            /*
            // Read bytes of file to upload
            String file_data = "Apples Mangoes Oranges Watermelon";
            byte[] fdata = Encoding.ASCII.GetBytes(file_data);

            var flen = fdata.Length;

            //Uploading File few bytes at a time to OneDrive
            var psize = 0;
            if (flen < 1024)
            {
                psize = 1024;
            }
            else
            {
                psize = flen;
            }
            var temp_upload_url = resp1.uploadUrl;
            var brstart = 0;
            var brend = psize;
            for (var i=0; i<flen/psize; i++) {
                HttpWebRequest rq2 = (HttpWebRequest)HttpWebRequest.Create(temp_upload_url);
                rq2.Method = "PUT";
                rq2.Host = "graph.microsoft.com";
                rq2.Headers.Add("Authorization", "Bearer " + resp.access_token);
                rq2.ContentLength = psize;
                brstart = (brend*i);
                brend = (brend*(i+1))-1;
                Console.WriteLine($"Sending bytes {brstart}-{brend}/{flen}");
                rq2.Headers.Add("ContentRange", $"bytes {brstart}-{brend}/{flen}");
                
                using (var requestStream = rq2.GetRequestStream())
                {
                    requestStream.Write(fdata, brstart, psize);
                }

                WebResponse rs2 = rq1.GetResponse();
                outStream = rs2.GetResponseStream();
                reader = new StreamReader(outStream);
                responseFromServer = reader.ReadToEnd();
                dynamic resp2 = JObject.Parse(responseFromServer);
                Console.WriteLine("Received UploadURL: {0}", resp2.nextExpectedRanges);
                rs2.Close();
                

            }*/
        }

    }
}






