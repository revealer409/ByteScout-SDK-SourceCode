# The authentication key (API Key).
# Get your own by registering at https://app.pdf.co/documentation/api
$API_KEY = "********************************"

# Source PDF file
$SourceFile = ".\sample-rotated.pdf"
# Comma-separated list of page indices (or ranges) to process. Leave empty for all pages. Example: '0,2-5,7-'.
$Pages = ""
# PDF document password. Leave empty for unprotected documents.
$Password = ""
# Destination XLS file name
$DestinationFile = ".\result.xls"

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

# Sample profile that sets advanced conversion options
# Advanced options are properties of XLSExtractor class from ByteScout PDF Extractor SDK used in the back-end:
# https://cdn.bytescout.com/help/BytescoutPDFExtractorSDK/html/87ce5fa6-3143-167d-abbd-bc7b5e160fe5.htm

# Valid RotationAngle values:
# 0 - no rotation
# 1 - 90 degrees
# 2 - 180 degrees
# 3 - 270 degrees
$Profiles = '{ "profiles": [{ "profile1": { "RotationAngle": 1 } } ] }'


# 1. RETRIEVE THE PRESIGNED URL TO UPLOAD THE FILE.
# * If you already have a direct file URL, skip to the step 3.

# Prepare URL for `Get Presigned URL` API call
$query = "https://api.pdf.co/v1/file/upload/get-presigned-url?contenttype=application/octet-stream&name=" + `
    [System.IO.Path]::GetFileName($SourceFile)
$query = [System.Uri]::EscapeUriString($query)

try {
    # Execute request
    $jsonResponse = Invoke-RestMethod -Method Get -Headers @{ "x-api-key" = $API_KEY } -Uri $query
    
    if ($jsonResponse.error -eq $false) {
        # Get URL to use for the file upload
        $uploadUrl = $jsonResponse.presignedUrl
        # Get URL of uploaded file to use with later API calls
        $uploadedFileUrl = $jsonResponse.url

        # 2. UPLOAD THE FILE TO CLOUD.

        $r = Invoke-WebRequest -Method Put -Headers @{ "x-api-key" = $API_KEY; "content-type" = "application/octet-stream" } -InFile $SourceFile -Uri $uploadUrl
        
        if ($r.StatusCode -eq 200) {
            
            # 3. CONVERT UPLOADED PDF FILE TO XLS

            # Prepare URL for `PDF To XLS` API call
            $query = "https://api.pdf.co/v1/pdf/convert/to/xls?name={0}&password={1}&pages={2}&url={3}&profiles={4}" -f `
                $(Split-Path $DestinationFile -Leaf), $Password, $Pages, $uploadedFileUrl, $Profiles
            $query = [System.Uri]::EscapeUriString($query)

            # Execute request
            $jsonResponse = Invoke-RestMethod -Method Get -Headers @{ "x-api-key" = $API_KEY } -Uri $query

            if ($jsonResponse.error -eq $false) {
                # Get URL of generated XLS file
                $resultFileUrl = $jsonResponse.url;
                
                # Download XLS file
                Invoke-WebRequest -Headers @{ "x-api-key" = $API_KEY } -OutFile $DestinationFile -Uri $resultFileUrl

                Write-Host "Generated XLS file saved as `"$($DestinationFile)`" file."
            }
            else {
                # Display service reported error
                Write-Host $jsonResponse.message
            }
        }
        else {
            # Display request error status
            Write-Host $r.StatusCode + " " + $r.StatusDescription
        }
    }
    else {
        # Display service reported error
        Write-Host $jsonResponse.message
    }
}
catch {
    # Display request error
    Write-Host $_.Exception
}
