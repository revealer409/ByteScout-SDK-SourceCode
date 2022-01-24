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


var https = require("https");
var path = require("path");
var fs = require("fs");


// The authentication key (API Key).
// Get your own by registering at https://app.pdf.co
const API_KEY = "*****************************"

/* 
Please follow below steps to create your own HTML Template and get "templateId". 
1. Add new html template in app.pdf.co/templates/html
2. Copy paste your html template code into this new template. Sample HTML templates can be found at "https://github.com/bytescout/pdf-co-api-samples/tree/master/PDF%20from%20HTML%20template/TEMPLATES-SAMPLES"
3. Save this new template
4. Copy it’s ID to clipboard
5. Now set ID of the template into “templateId” parameter
*/

// HTML template using built-in template
// see https://app.pdf.co/templates/html/3/edit
const template_id = 3;

// Data to fill the template
const templateData = "./invoice_data.json";
// Destination PDF file name
const DestinationFile = "./result.pdf";

const fileReadTemplateData = fs.readFileSync(templateData, "utf8");

// Prepare request to `HTML To PDF` API endpoint
var queryPath = `/v1/pdf/convert/from/html?name=${path.basename(DestinationFile)}`;
var reqOptions = {
    host: "api.pdf.co",
    path: encodeURI(queryPath),
    method: "POST",
    headers: {
        "x-api-key": API_KEY,
        "Content-Type": "application/json"
    }
};
var requestBody = JSON.stringify({
    "templateId": template_id,
    "templateData": fileReadTemplateData   
});
// Send request
var postRequest = https.request(reqOptions, (response) => {
    response.on("data", (d) => {
        // Parse JSON response
        var data = JSON.parse(d);
        if (data.error == false) {
            // Download PDF file
            var file = fs.createWriteStream(DestinationFile);
            https.get(data.url, (response2) => {
                response2.pipe(file)
                    .on("close", () => {
                        console.log(`Generated PDF file saved as "${DestinationFile}" file.`);
                    });
            });
        }
        else {
            // Service reported error
            console.log(data.message);
        }
    });
}).on("error", (e) => {
    // Request error
    console.log(e);
});
// Write request data
postRequest.write(requestBody);
postRequest.end();
