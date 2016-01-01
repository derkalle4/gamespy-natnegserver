'Class for sheduling cleanup-queries on the MySQL-Server
'JW "LeKeks" 07/2014
Imports System.Threading
Public Class DatabaseCleaner
    Public Property CleanupInterval As Int32    'Delay (in s) between the queries
    Public Property CleanupTimeout As Int32     'Time (in s) before a natneg-session gets dropped
    Public Property MySQL As NatnegMySQLHandler 'Reference to MySQLHandler-Object

    Private workThread As Thread        'Thread for sheduling the Queries
    Private running As Boolean = False  'Status variable

    Public Sub init()
        'Start the workthread if it's not running already
        If Not Me.running Then
            Me.running = True
            Me.workThread = New Thread(AddressOf Me.Cleanup)
            Me.workThread.Start()
        End If
    End Sub

    Public Sub terminate()
        'Will cause the thread to exit
        Me.running = False
    End Sub

    Private Sub Cleanup()
        While Me.running
            Logger.Log("Cleaning up Database...", LogLevel.Verbose)
            'Dropping all natneg-sessions which are older then NOW() - CleanupTimeout
            '-> ensure we drop failed sessions after a while so we won't get duplicate cookies
            Me.MySQL.NonQuery("delete from `natneg` where `natneg_lastupdate` < (UNIX_TIMESTAMP() - " & Me.CleanupTimeout & ")")
            Thread.Sleep(Me.CleanupInterval * 1000)
        End While
    End Sub
End Class
