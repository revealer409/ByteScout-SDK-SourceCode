//*******************************************************************************************//
//                                                                                           //
// Download Free Evaluation Version From: https://bytescout.com/download/web-installer       //
//                                                                                           //
// Also available as Web API! Get Your Free API Key: https://app.pdf.co/signup               //
//                                                                                           //
// Copyright © 2017-2020 ByteScout, Inc. All rights reserved.                                //
// https://www.bytescout.com                                                                 //
// https://pdf.co                                                                            //
//                                                                                           //
//*******************************************************************************************//


using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ByteScoutWebApiExample
{
	class Program
	{
		// The authentication key (API Key).
		// Get your own by registering at https://app.pdf.co/documentation/api
		const String API_KEY = "**************************************";

        // Direct URL of source PDF file.
        const string SourceFileUrl = "https://bytescout-com.s3.amazonaws.com/files/demo-files/cloud-api/pdf-to-csv/sample.pdf";
		// Comma-separated list of page indices (or ranges) to process. Leave empty for all pages. Example: '0,2-5,7-'.
		const string Pages = "";
		// PDF document password. Leave empty for unprotected documents.
		const string Password = "";
		// Destination CSV file name
		const string DestinationFile = @".\result.csv";

		/*
		Some of advanced options available through profiles:
		(JSON can be single/double-quoted and contain comments.)
		{
			"profiles": [
				{
					"profile1": {
						"CSVSeparatorSymbol": ",", // Separator symbol.
						"CSVQuotaionSymbol": "\"", // Quotation symbol.
						"ExtractInvisibleText": true, // Invisible text extraction. Values: true / false
						"ExtractShadowLikeText": true, // Shadow-like text extraction. Values: true / false
						"LineGroupingMode": "None", // Values: "None", "GroupByRows", "GroupByColumns", "JoinOrphanedRows"
						"ColumnDetectionMode": "ContentGroupsAndBorders", // Values: "ContentGroupsAndBorders", "ContentGroups", "Borders", "BorderedTables"
						"Unwrap": false, // Unwrap grouped text in table cells. Values: true / false
						"ShrinkMultipleSpaces": false, // Shrink multiple spaces in table cells that affect column detection. Values: true / false
						"DetectNewColumnBySpacesRatio": 1, // Spacing ratio that affects column detection.
						"CustomExtractionColumns": [ 0, 50, 150, 200, 250, 300 ], // Explicitly specify columns coordinates for table extraction.
						"CheckPermissions": true, // Ignore document permissions. Values: true / false
					}
				}
			]
		}
		*/
        // Sample profile that sets advanced conversion options
        // Advanced options are properties of CSVExtractor class from ByteScout PDF Extractor SDK used in the back-end:
        // https://cdn.bytescout.com/help/BytescoutPDFExtractorSDK/html/87ce5fa6-3143-167d-abbd-bc7b5e160fe5.htm

        static string Profiles = File.ReadAllText("profile.json");

		static void Main(string[] args)
		{
			// Create standard .NET web client instance
			WebClient webClient = new WebClient();

			// Set API Key
			webClient.Headers.Add("x-api-key", API_KEY);

			// Prepare URL for `PDF To CSV` API call
			string query = Uri.EscapeUriString(string.Format(
                "https://api.pdf.co/v1/pdf/convert/to/csv?name={0}&password={1}&pages={2}&url={3}&profiles={4}", 
				Path.GetFileName(DestinationFile),
				Password,
				Pages,
				SourceFileUrl,
                Profiles));

			try
			{
				// Execute request
				string response = webClient.DownloadString(query);

				// Parse JSON response
				JObject json = JObject.Parse(response);

                if (json["status"].ToString() != "error")
                {
					// Get URL of generated CSV file
					string resultFileUrl = json["url"].ToString();

					// Download CSV file
					webClient.DownloadFile(resultFileUrl, DestinationFile);

					Console.WriteLine("Generated CSV file saved as \"{0}\" file.", DestinationFile);
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
