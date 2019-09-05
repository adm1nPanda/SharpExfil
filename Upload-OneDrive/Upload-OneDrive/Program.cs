using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Security;

namespace Upload_OneDrive
{
    class Program
    {
        
        
        static string cid = "<Clientid-Token>";
        static string sco = "user.read.all%20files.readwrite.all%20offline_access";
        static string ruri = "<Redirect-URL>";
        static string cs = "<Client-Secret>";
        static string access_token = null;
        static string rtoken = null;

        static string option = "large";
        static string code = null;
        static string file_path = null;
        static string file_name = null;


        static void refresh_token()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create($"https://login.microsoftonline.com/common/oauth2/v2.0/token");

            string postData = $"client_id={cid}&redirect_uri={ruri}&client_secret={cs}&refresh_token={rtoken}&grant_type = refresh_token";
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

            access_token = responseFromServer.Substring(responseFromServer.IndexOf("\"access_token\":\"") + ("\"access_token\":\"").Length, 1184);
            rtoken = responseFromServer.Substring(responseFromServer.IndexOf("\"refresh_token\":\"") + ("\"refresh_token\":\"").Length, 389);
            
            response.Close();
        }

        static void UploadFile()
        {
            
            
            if (option == "small")
            {
                //Reading Small File
                try
                {
                    byte[] small_fdata = System.IO.File.ReadAllBytes(file_path + "\\" + file_name);

                    string upload_small = $"https://graph.microsoft.com/v1.0/drive/root:/testfoldera/{file_name}:/content";
                    HttpWebRequest rq_small = (HttpWebRequest)HttpWebRequest.Create(upload_small);
                    rq_small.Method = "PUT";

                    rq_small.Headers.Add("Authorization", "Bearer " + access_token);
                    rq_small.ContentLength = small_fdata.Length;
                    var requestStream = rq_small.GetRequestStream();
                    requestStream.Write(small_fdata, 0, small_fdata.Length);
                    requestStream.Close();

                    WebResponse rs_small = rq_small.GetResponse();
                    Stream outStream = rs_small.GetResponseStream();
                    StreamReader reader = new StreamReader(outStream);
                    string responseFromServer = reader.ReadToEnd();
                    var name = responseFromServer.Substring(responseFromServer.IndexOf("\"name\":\"") + ("\"name\":\"").Length, file_name.Length);
                    Console.WriteLine("Uploaded File: {0}", name);
                    rs_small.Close();
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Console.WriteLine("[-] Invalid File Path");
                }
                catch (System.IO.FileNotFoundException)
                {
                    Console.WriteLine("[-] File Doesn't Exist");
                }
            }
            else if (option == "large")
            {
                try
                {
                    // Requesting URL for large file upload
                    string upload_large = $"https://graph.microsoft.com/v1.0/drive/root:/testfoldera/{file_name}:/createUploadSession";

                    HttpWebRequest rq1 = (HttpWebRequest)HttpWebRequest.Create(upload_large);
                    rq1.Method = "POST";
                    rq1.Headers.Add("Authorization", "Bearer " + access_token);
                    rq1.ContentType = "application/json";


                    String a = "{\"item\": {\"@microsoft.graph.conflictBehavior\": \"rename\",\"name\": \"" + file_name + "\"}}";
                    byte[] data = Encoding.UTF8.GetBytes(a);

                    rq1.ContentLength = data.Length;
                    var requestStream = rq1.GetRequestStream();
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                    WebResponse rs1 = rq1.GetResponse();
                    Stream outStream = rs1.GetResponseStream();
                    StreamReader reader = new StreamReader(outStream);
                    string responseFromServer = reader.ReadToEnd();
                    
                    var temp_upload_url = responseFromServer.Substring(responseFromServer.IndexOf("\"uploadUrl\":\"") + ("\"uploadUrl\":\"").Length, (responseFromServer.IndexOf("\"}", responseFromServer.IndexOf("\"uploadUrl\":\"") + ("\"uploadUrl\":\"").Length)) - (responseFromServer.IndexOf("\"uploadUrl\":\"") + ("\"uploadUrl\":\"").Length));

                    rs1.Close();


                    // Read no of bytes in upload file
                    long flen = new System.IO.FileInfo(file_path + "\\" + file_name).Length;
                    Console.WriteLine((int)flen);

                    //Uploading File few bytes at a time to OneDrive

                    var i = 0;
                    var psize = 0;
                    FileStream fs = new FileStream(file_path + "\\" + file_name, FileMode.Open, FileAccess.Read);
                    do
                    {
                        HttpWebRequest rq2 = (HttpWebRequest)HttpWebRequest.Create(temp_upload_url);
                        //rq2.Proxy = new WebProxy("192.168.97.1", 8044);
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                        rq2.Method = "PUT";
                        if ((flen - i) > 100000)
                        {
                            psize = 100000;

                        }
                        else
                        {
                            psize = (int)flen - i;
                        }


                        byte[] fdata = new byte[psize];
                        fs.Read(fdata, 0, psize);

                        rq2.ContentLength = psize;

                        Console.WriteLine($"Sending bytes {i}-{i + psize - 1}/{flen}");

                        rq2.Headers.Add("Content-Range", $"bytes {i}-{i + psize - 1}/{flen}");

                        using (var uploadstream = rq2.GetRequestStream())
                        {
                            uploadstream.Write(fdata, 0, psize);
                        }
                        //Console.WriteLine(rq2.Headers.ToString());
                        //Console.WriteLine(rq2.ContentLength);
                        try
                        {
                            WebResponse rs2 = rq2.GetResponse();
                            outStream = rs2.GetResponseStream();
                            reader = new StreamReader(outStream);
                            responseFromServer = reader.ReadToEnd();
                            Console.WriteLine(responseFromServer);
                            rs2.Close();
                        }
                        catch (WebException e)
                        {
                            if (e.Status == WebExceptionStatus.ProtocolError)
                            {
                                Console.WriteLine("ProtocolError: {0}", e);
                                var err = new System.IO.StreamReader(e.Response.GetResponseStream());
                                Console.WriteLine(err.ReadToEnd());
                            }
                            else
                            {
                                Console.WriteLine(e);
                            }
                        }

                        i = i + psize;

                    } while (i < flen);
                    fs.Close();
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Console.WriteLine("[-] Invalid File Path");
                }
                catch (System.IO.FileNotFoundException)
                {
                    Console.WriteLine("[-] File Doesn't Exist");
                }
            }
        
        }


        static void Main(string[] args)
        {

            if (args.Length < 1 || (args[0].ToLower() == "-h" || args[0].ToLower() == "--help"))
            {
                Console.WriteLine($"Authenticate at URL below -\n\nhttps://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={cid}&scope={sco}&response_type=code&redirect_uri={ruri} \n\nOnce completed copy the code token from the browser address bar and input into application");
                Console.WriteLine("\n\nExample Command - \n     Upload-OneDrive.exe -uri    [Will Print the URL to generate code tokens]\n     Upload-OneDrive.exe -size=large -code=<code_token>  -file=<path_to_file>  [Use input code token to upload file to OneDrive]\n\n\t\t\t -size=<large/small> [If Upload file requires to chunked. Default - large]\n\t\t\t -code=<code_token> [Requested from the URL]\n\t\t\t -file=<path_to_file> [Location of the file to upload]");
                Console.WriteLine("\n\n***ARGUMENTS NEED TO BE PASSED IN THE SAME ORDER AS ABOVE***");
                Environment.Exit(0);
            }

            if (args[0].ToLower() == "-uri")
            {
                Console.WriteLine($"Authenticate at URL below -\n\nhttps://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={cid}&scope={sco}&response_type=code&redirect_uri={ruri} \n\nOnce completed copy the code token from the browser address bar and input into application");
                Environment.Exit(0);
            }
            try
            {
                if (args[0].ToLower().Contains("-size"))
                {
                    option = args[0].Substring(6, 5);

                    if (args[1].ToLower().Contains("-code"))
                    {
                        code = args[1].Substring(6, args[1].Length - 6);

                        //authuntication using code token

                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create($"https://login.microsoftonline.com/common/oauth2/v2.0/token");

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

                        access_token = responseFromServer.Substring(responseFromServer.IndexOf("\"access_token\":\"") + ("\"access_token\":\"").Length, 1184);
                        rtoken = responseFromServer.Substring(responseFromServer.IndexOf("\"refresh_token\":\"") + ("\"refresh_token\":\"").Length, 389);
                        
                        response.Close();

                        if (args[2].ToLower().Contains("-file"))
                        {
                            if (File.Exists(args[2].Substring(6, args[2].Length - 6)))
                            {
                                var f = new FileInfo(args[2].Substring(6, args[2].Length - 6));
                                file_path = @f.DirectoryName;
                                file_name = @f.Name;
                                UploadFile();
                            }
                            else if (Directory.Exists(args[2].Substring(6, args[2].Length - 6)))
                            {
                                string[] files = Directory.GetFiles(args[2].Substring(6, args[2].Length - 6));
                                
                                foreach (string file in files)
                                {
                                    file_name = null;
                                    file_path = null;
                                    
                                    var f = new FileInfo(file);
                                    file_path = @f.DirectoryName;
                                    file_name = @f.Name;
                                    
                                    UploadFile();
                                    refresh_token(); 
                                }
                            }
                            else
                            {
                                Console.WriteLine("File or Directory doesn't exist");
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            Console.WriteLine("You need to pass a file as an argument");
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Console.WriteLine("You need to pass a code token as an argument");
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("You are missing an argument {0}", e.ToString());
                Environment.Exit(0);
            }

        }

    }
}
