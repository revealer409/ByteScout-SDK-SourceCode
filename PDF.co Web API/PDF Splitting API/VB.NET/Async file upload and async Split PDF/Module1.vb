'*******************************************************************************************'
'                                                                                           '
' Download Free Evaluation Version From:     https://bytescout.com/download/web-installer   '
'                                                                                           '
' Also available as Web API! Get free API Key https://app.pdf.co/signup                     '
'                                                                                           '
' Copyright © 2017-2020 ByteScout, Inc. All rights reserved.                                '
' https://www.bytescout.com                                                                 '
' https://www.pdf.co                                                                        '
'*******************************************************************************************'


Imports System.IO
Imports System.Net
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Module1

    ' The authentication key (API Key).
    ' Get your own by registering at https://app.pdf.co
    Const API_KEY As String = "***********************************"

    ' Source PDF file to split
    Const SourceFile As String = ".\sample.pdf"
    ' Comma-separated list of page numbers (or ranges) to process. Example: '1,3-5,7-'.
    Const Pages As String = "1-2,3-"
    ' (!) Make asynchronous job
    Const Async As Boolean = True

    Sub Main()

        ' Create standard .NET web client instance
        Dim webClient As WebClient = New WebClient()

        ' Set API Key
        webClient.Headers.Add("x-api-key", API_KEY)

        ' 1. RETRIEVE THE PRESIGNED URL TO UPLOAD THE FILE.
        ' * If you already have a direct file URL, skip to the step 3.

        ' Prepare URL for `Get Presigned URL` API call
        Dim query As String = Uri.EscapeUriString(String.Format(
            "https://api.pdf.co/v1/file/upload/get-presigned-url?contenttype=application/octet-stream&name={0}",
            Path.GetFileName(SourceFile)))

        Try
            ' Execute request
            Dim response As String = webClient.DownloadString(query)

            ' Parse JSON response
            Dim json As JObject = JObject.Parse(response)

            If json("error").ToObject(Of Boolean) = False Then

                ' Get URL to use for the file upload
                Dim uploadUrl As String = json("presignedUrl").ToString()
                ' Get URL of uploaded file to use with later API calls
                Dim uploadedFileUrl As String = json("url").ToString()

                ' 2. UPLOAD THE FILE TO CLOUD.

                webClient.Headers.Add("content-type", "application/octet-stream")
                webClient.UploadFile(uploadUrl, "PUT", SourceFile) ' You can use UploadData() instead if your file is byte array or Stream

                ' Set JSON content type
                webClient.Headers.Add("Content-Type", "application/json")

                ' 3. SPLIT UPLOADED PDF

                ' Prepare URL for `Split PDF` API call
                Dim url As String = "https://api.pdf.co/v1/pdf/split"


                ' Prepare requests params as JSON
                ' See documentation: https : //apidocs.pdf.co
                Dim parameters As New Dictionary(Of String, Object)
                parameters.Add("pages", Pages)
                parameters.Add("url", uploadedFileUrl)
                parameters.Add("async", Async)

                ' Convert dictionary of params to JSON
                Dim jsonPayload As String = JsonConvert.SerializeObject(parameters)

                Try
                    ' Execute POST request with JSON payload
                    response = webClient.UploadString(url, jsonPayload)

                    ' Parse JSON response
                    json = JObject.Parse(response)

                    If json("error").ToObject(Of Boolean) = False Then

                        ' Asynchronous job ID
                        Dim jobId As String = json("jobId").ToString()
                        ' URL of generated JSON file available after the job completion; it will contain URLs of result PDF files.
                        Dim resultJsonFileUrl As String = json("url").ToString()

                        ' Check the job status in a loop. 
                        ' If you don't want to pause the main thread you can rework the code 
                        ' to use a separate thread for the status checking and completion.
                        Do
                            Dim status As String = CheckJobStatus(jobId) ' Possible statuses: "working", "failed", "aborted", "success".

                            ' Display timestamp and status (for demo purposes)
                            Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + status)

                            If status = "success" Then

                                ' Download JSON file as string
                                Dim jsonFileString As String = webClient.DownloadString(resultJsonFileUrl)

                                Dim resultFilesUrls As JArray = JArray.Parse(jsonFileString)

                                ' Download generated PDF files
                                Dim part As Integer = 1
                                For Each token As JToken In resultFilesUrls

                                    Dim resultFileUrl As String = token.ToString()
                                    Dim localFileName As String = String.Format(".\part{0}.pdf", part)

                                    webClient.DownloadFile(resultFileUrl, localFileName)

                                    Console.WriteLine("Downloaded ""{0}"".", localFileName)
                                    part = part + 1

                                Next

                                Exit Do

                            ElseIf status = "working" Then

                                ' Pause for a few seconds
                                Thread.Sleep(3000)

                            Else

                                Console.WriteLine(status)
                                Exit Do

                            End If

                        Loop

                    Else
                        Console.WriteLine(json("message").ToString())
                    End If

                Catch ex As WebException
                    Console.WriteLine(ex.ToString())
                End Try

            End If

        Catch ex As WebException
            Console.WriteLine(ex.ToString())
        End Try

        webClient.Dispose()


        Console.WriteLine()
        Console.WriteLine("Press any key...")
        Console.ReadKey()

    End Sub

    ''' <summary>
    ''' Check job status
    ''' </summary>
    Function CheckJobStatus(jobId As String) As String

        Using webClient As WebClient = New WebClient()

            ' Set API Key
            webClient.Headers.Add("x-api-key", API_KEY)

            Dim url As String = "https://api.pdf.co/v1/job/check?jobid=" + jobId

            Dim response As String = webClient.DownloadString(url)
            Dim json As JObject = JObject.Parse(response)

            Return Convert.ToString(json("status"))

        End Using

    End Function


End Module
