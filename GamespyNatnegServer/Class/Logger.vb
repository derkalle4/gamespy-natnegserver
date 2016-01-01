Public Enum LogLevel
    Verbose = 1
    Warning = 3
    Exception = 4
    Info = 2
End Enum
Public Class Logger

    Public Shared Property LogToFile As Boolean = True
    Public Shared Property LogFileName As String = "\log.txt"
    Public Shared Property MinLogLevel As Byte = 2

    Public Shared Sub Log(ByVal message As String, ByVal level As Byte)
        If Not level >= MinLogLevel Then
            Return
        End If

        Select Case level
            Case LogLevel.Verbose
                message = "DEBUG | " & message
            Case LogLevel.Warning
                message = "WARN  | " & message
            Case LogLevel.Exception
                message = "EX    | " & message
            Case LogLevel.Info
                message = "INFO  | " & message
        End Select

        Console.WriteLine(message)

        If LogToFile = True Then
            My.Computer.FileSystem.WriteAllText(CurDir() & LogFileName, message & vbCrLf, True)
        End If
        If level = LogLevel.Exception Then
            Console.ReadLine()
            End
        End If
    End Sub

End Class
