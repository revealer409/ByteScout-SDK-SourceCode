import os
import requests # pip install requests

# The authentication key (API Key).
# Get your own by registering at https://app.pdf.co/documentation/api
API_KEY = "***************************************"

# Base URL for PDF.co Web API requests
BASE_URL = "https://api.pdf.co/v1"

# Source PDF file
SourceFile = ".\\sample.pdf"
# Comma-separated list of page indices (or ranges) to process. Leave empty for all pages. Example: '0,2-5,7-'.
Pages = ""
# PDF document password. Leave empty for unprotected documents.
Password = ""
# Destination Excel file name
DestinationFile = ".\\result.xlsx"


def main(args = None):
    uploadedFileUrl = uploadFile(SourceFile)
    if (uploadedFileUrl != None):
        convertPdfToExcel(uploadedFileUrl, DestinationFile)


def convertPdfToExcel(uploadedFileUrl, destinationFile):
    """Converts PDF To Excel using PDF.co Web API"""

    # Some of advanced options available through profiles:
    # (JSON can be single/double-quoted and contain comments.)
    # {
    #     "profiles": [
    #         {
    #             "profile1": {
    #                 "NumberDecimalSeparator": "", // Allows to customize decimal separator in numbers.
    #                 "NumberGroupSeparator": "", // Allows to customize thousands separator.
    #                 "AutoDetectNumbers": true, // Whether to detect numbers. Values: true / false
    #                 "RichTextFormatting": true, // Whether to keep text style and fonts. Values: true / false
    #                 "PageToWorksheet": true, // Whether to create separate worksheet for each page of PDF document. Values: true / false
    #                 "ExtractInvisibleText": true, // Invisible text extraction. Values: true / false
    #                 "ExtractShadowLikeText": true, // Shadow-like text extraction. Values: true / false
    #                 "LineGroupingMode": "None", // Values: "None", "GroupByRows", "GroupByColumns", "JoinOrphanedRows"
    #                 "ColumnDetectionMode": "ContentGroupsAndBorders", // Values: "ContentGroupsAndBorders", "ContentGroups", "Borders", "BorderedTables"
    #                 "Unwrap": false, // Unwrap grouped text in table cells. Values: true / false
    #                 "ShrinkMultipleSpaces": false, // Shrink multiple spaces in table cells that affect column detection. Values: true / false
    #                 "DetectNewColumnBySpacesRatio": 1, // Spacing ratio that affects column detection.
    #                 "CustomExtractionColumns": [ 0, 50, 150, 200, 250, 300 ], // Explicitly specify columns coordinates for table extraction.
    #                 "CheckPermissions": true, // Ignore document permissions. Values: true / false
    #             }
    #         }
    #     ]
    # }

    # Sample profile that sets advanced conversion options.
    # Advanced options are properties of XLSExtractor class from ByteScout PDF Extractor SDK used in the back-end:
    # https://cdn.bytescout.com/help/BytescoutPDFExtractorSDK/html/2712c05b-9674-5253-df76-2a31ed055afd.htm
    profiles = '{ "profiles": [ { "profile1": { "RichTextFormatting": false, "PageToWorksheet": false } } ] }'


    # Prepare URL for 'PDF To Xlsx' API request
    url = "{}/pdf/convert/to/xlsx?name={}&password={}&pages={}&url={}&profiles={}".format(
        BASE_URL,
        os.path.basename(destinationFile),
        Password,
        Pages,
        uploadedFileUrl,
        profiles
    )

    # Execute request and get response as JSON
    response = requests.get(url, headers={ "x-api-key": API_KEY, "content-type": "application/octet-stream" })
    if (response.status_code == 200):
        json = response.json()

        if json["error"] == False:
            #  Get URL of result file
            resultFileUrl = json["url"]            
            # Download result file
            r = requests.get(resultFileUrl, stream=True)
            if (r.status_code == 200):
                with open(destinationFile, 'wb') as file:
                    for chunk in r:
                        file.write(chunk)
                print(f"Result file saved as \"{destinationFile}\" file.")
            else:
                print(f"Request error: {response.status_code} {response.reason}")
        else:
            # Show service reported error
            print(json["message"])
    else:
        print(f"Request error: {response.status_code} {response.reason}")


def uploadFile(fileName):
    """Uploads file to the cloud"""
    
    # 1. RETRIEVE PRESIGNED URL TO UPLOAD FILE.

    # Prepare URL for 'Get Presigned URL' API request
    url = "{}/file/upload/get-presigned-url?contenttype=application/octet-stream&name={}".format(
        BASE_URL, os.path.basename(fileName))
    
    # Execute request and get response as JSON
    response = requests.get(url, headers={ "x-api-key": API_KEY })
    if (response.status_code == 200):
        json = response.json()
        
        if json["error"] == False:
            # URL to use for file upload
            uploadUrl = json["presignedUrl"]
            # URL for future reference
            uploadedFileUrl = json["url"]

            # 2. UPLOAD FILE TO CLOUD.
            with open(fileName, 'rb') as file:
                requests.put(uploadUrl, data=file, headers={ "x-api-key": API_KEY, "content-type": "application/octet-stream" })

            return uploadedFileUrl
        else:
            # Show service reported error
            print(json["message"])    
    else:
        print(f"Request error: {response.status_code} {response.reason}")

    return None


if __name__ == '__main__':
    main()