//*******************************************************************************************//
//                                                                                           //
// Download Free Evaluation Version From: https://bytescout.com/download/web-installer       //
//                                                                                           //
// Also available as Web API! Get Your Free API Key: https://app.pdf.co/signup               //
//                                                                                           //
// Copyright © 2017-2019 ByteScout, Inc. All rights reserved.                                //
// https://www.bytescout.com                                                                 //
// https://pdf.co                                                                            //
//                                                                                           //
//*******************************************************************************************//


using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading;

// Please NOTE: In this sample we're assuming Cloud Api Server is hosted at "https://localhost". 
// If it's not then please replace this with with your hosting url.

// Cloud API asynchronous "PDF To JSON" job example.
// Allows to avoid timeout errors when processing huge or scanned PDF documents.

namespace ByteScoutWebApiExample
{
	class Program
	{
        // Source PDF file
        const string SourceFile = @".\sample.pdf";
        // Comma-separated list of page indices (or ranges) to process. Leave empty for all pages. Example: '0,2-5,7-'.
        const string Pages = "";
		// PDF document password. Leave empty for unprotected documents.
		const string Password = "";
		// Destination JSON file name
		const string DestinationFile = @".\result.json";
		// (!) Make asynchronous job
		const bool Async = true;

		static void Main(string[] args)
		{
			// Create standard .NET web client instance
			WebClient webClient = new WebClient();

            // Upload file
            string uploadedFileUrl = UploadFile(webClient, SourceFile);

            // Prepare URL for `PDF To JSON` API call
            string query = Uri.EscapeUriString(string.Format(
				"https://localhost/pdf/convert/to/json?name={0}&password={1}&pages={2}&url={3}&async={4}", 
				Path.GetFileName(DestinationFile),
				Password,
				Pages,
                uploadedFileUrl,
				Async));

			try
			{
				// Execute request
				string response = webClient.DownloadString(query);

				// Parse JSON response
				JObject json = JObject.Parse(response);

				if (json["error"].ToObject<bool>() == false)
				{
					// Asynchronous job ID
					string jobId = json["jobId"].ToString();
					// URL of generated JSON file that will available after the job completion
					string resultFileUrl = json["url"].ToString();

					// Check the job status in a loop. 
					// If you don't want to pause the main thread you can rework the code 
					// to use a separate thread for the status checking and completion.
					do
					{
						string status = CheckJobStatus(webClient, jobId); // Possible statuses: "working", "failed", "aborted", "success".

						// Display timestamp and status (for demo purposes)
						Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + status);

						if (status == "success")
						{
							// Download JSON file
							webClient.DownloadFile(resultFileUrl, DestinationFile);

							Console.WriteLine("Generated JSON file saved as \"{0}\" file.", DestinationFile);
							break;
						}
						else if (status == "working")
						{
							// Pause for a few seconds
							Thread.Sleep(3000);
						}
						else
						{
							Console.WriteLine(status);
							break;
						}
					}
					while (true);
				}
				else
				{
					Console.WriteLine(json["message"].ToString());
				}
			}
			catch (WebException e)
			{
				Console.WriteLine(e.ToString());
			}

			webClient.Dispose();

			Console.WriteLine();
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

        static string UploadFile(WebClient webClient, string file)
        {
            // RETRIEVE THE PRESIGNED URL TO UPLOAD THE FILE:

            // Prepare URL for `Get Presigned URL` API call
            string query = Uri.EscapeUriString(string.Format(
                "https://localhost/file/upload/get-presigned-url?contenttype=application/octet-stream&name={0}",
                Path.GetFileName(SourceFile)));

            // Execute request
            string response = webClient.DownloadString(query);

            // Parse JSON response
            JObject json = JObject.Parse(response);

            string uploadUrl = json["presignedUrl"].ToString();
            string uploadedFileUrl = json["url"].ToString();

            // UPLOAD FILE:

            webClient.Headers.Add("content-type", "application/octet-stream");
            webClient.UploadFile(uploadUrl, "PUT", SourceFile); // You can use UploadData() instead if your file is byte[] or Stream
            webClient.Headers.Remove("content-type");

            return uploadedFileUrl;
        }

        static string CheckJobStatus(WebClient webClient, string jobId)
		{
			string url = "https://localhost/job/check?jobid=" + jobId;

			string response = webClient.DownloadString(url);
			JObject json = JObject.Parse(response);

		    return Convert.ToString(json["status"]);
		}
	}
}
