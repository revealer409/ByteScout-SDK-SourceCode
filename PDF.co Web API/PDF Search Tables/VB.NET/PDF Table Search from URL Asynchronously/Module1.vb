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

    ' Direct URL of source PDF file.
    ' You can also upload your own file into PDF.co and use it as url. Check "Upload File" samples for code snippets: https://github.com/bytescout/pdf-co-api-samples/tree/master/File%20Upload/   
    Const SourceFileUrl As String = "https://bytescout-com.s3.amazonaws.com/files/demo-files/cloud-api/pdf-to-text/sample.pdf"

    ' Comma-separated list of page indices (or ranges) to process. Leave empty for all pages. Example: '0,2-5,7-'.
    Const Pages As String = ""

    ' PDF document password. Leave empty for unprotected documents.
    Const Password As String = ""

    '(!) Make asynchronous job
    Const Async As Boolean = True

    Sub Main()

        ' Create standard .NET web client instance
        Dim webClient As WebClient = New WebClient()

        ' Set API Key
        webClient.Headers.Add("x-api-key", API_KEY)

        ' Set JSON content type
        webClient.Headers.Add("Content-Type", "application/json")

        ' Prepare URL for PDF Table Search API call.
        ' See documentation: https : //app.pdf.co/documentation/api/1.0/pdf/find/table.html
		Dim url As String = "https://api.pdf.co/v1/pdf/find/table"

        ' Prepare requests params as JSON
        ' See documentation: https : //apidocs.pdf.co
        Dim parameters As New Dictionary(Of String, Object)
		parameters.Add("password", Password)
		parameters.Add("pages", Pages)
		parameters.Add("url", SourceFileUrl)
		parameters.Add("async", Async)

        ' Convert dictionary of params to JSON
        Dim jsonPayload As String = JsonConvert.SerializeObject(parameters)

        Try
            ' Execute POST request with JSON payload
            Dim response As String = webClient.UploadString(url, jsonPayload)

            ' Parse JSON response
            Dim json As JObject = JObject.Parse(response)

            If json("error").ToObject(Of Boolean) = False Then

                ' Asynchronous job ID
                Dim jobId As String = json("jobId").ToString()

                ' URL of generated json file that will available after the job completion
                Dim resultFileUrl As String = json("url").ToString()

                ' Check the job status in a loop. 
                ' If you don't want to pause the main thread you can rework the code 
                ' to use a separate thread for the status checking And completion.
                Do
                    Dim status = CheckJobStatus(jobId) ' Possible statuses: "working", "failed", "aborted", "success".

                    ' Display timestamp and status (for demo purpose)
                    Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + status)

                    If (status = "success") Then
                        ' Execute request
                        Dim respFileJson As String = webClient.DownloadString(resultFileUrl)

                        ' Print JSON response
                        Console.WriteLine(respFileJson)

                        Exit Do
                    ElseIf (status = "working") Then
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

        webClient.Dispose()

        Console.WriteLine()
        Console.WriteLine("Press any key...")
        Console.ReadKey()

    End Sub

    Function CheckJobStatus(ByVal jobId As String)

        Using webClient As New WebClient

            ' Set API Key
            webClient.Headers.Add("x-api-key", API_KEY)

            Dim url As String = "https://api.pdf.co/v1/job/check?jobid=" & jobId

            Dim response As String = webClient.DownloadString(url)
            Dim json As JObject = JObject.Parse(response)

            Return Convert.ToString(json("status"))

        End Using

    End Function

End Module
