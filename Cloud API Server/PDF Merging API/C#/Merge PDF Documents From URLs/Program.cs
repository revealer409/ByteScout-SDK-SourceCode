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

// Please NOTE: In this sample we're assuming Cloud Api Server is hosted at "https://localhost". 
// If it's not then please replace this with with your hosting url.
namespace ByteScoutWebApiExample
{
	class Program
	{
		// Direct URLs of PDF files to merge
		static string[] SourceFiles = {
			"https://bytescout-com.s3.amazonaws.com/files/demo-files/cloud-api/pdf-merge/sample1.pdf",
			"https://bytescout-com.s3.amazonaws.com/files/demo-files/cloud-api/pdf-merge/sample2.pdf" };
		// Destination PDF file name
		const string DestinationFile = @".\result.pdf";

		static void Main(string[] args)
		{
			// Create standard .NET web client instance
			WebClient webClient = new WebClient();

			// Prepare URL for `Merge PDF` API call
			string query = Uri.EscapeUriString(string.Format(
				"https://localhost/pdf/merge?name={0}&url={1}",
				Path.GetFileName(DestinationFile),
				string.Join(",", SourceFiles)));

			try
			{
				// Execute request
				string response = webClient.DownloadString(query);

				// Parse JSON response
				JObject json = JObject.Parse(response);

				if (json["error"].ToObject<bool>() == false)
				{
					// Get URL of generated PDF file
					string resultFileUrl = json["url"].ToString();

					// Download PDF file
					webClient.DownloadFile(resultFileUrl, DestinationFile);

					Console.WriteLine("Generated PDF file saved as \"{0}\" file.", DestinationFile);
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
	}
}
