'Imports MySql
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class MySQLHandler
    Public Property Hostname As String
    Public Property Port As Int32
    Public Property DbName As String
    Public Property DbUser As String
    Public Property DbPwd As String

    Private connection As MySqlConnection

    Public Function Connect() As Boolean
        connection = New MySqlConnection
        Dim connectionString As String = String.Empty
        connectionString = "server=" & _
                            Me.Hostname & ";port=" & _
                            Me.Port.ToString & ";uid = " & _
                            Me.DbUser & ";pwd=" & _
                            Me.DbPwd & ";database=" & _
                            Me.DbName & ";"

        connection.ConnectionString = connectionString

        Logger.Log("Testing MySQL Connection...", LogLevel.Verbose)

        Try
            connection.Open()
            connection.Close()
            Logger.Log("MySQL OK", LogLevel.Verbose)
            Return True
        Catch ex As Exception

            Logger.Log("Can't connect to MySQL Server!", LogLevel.Exception)
        End Try
        Return False
    End Function

    Public Function DoQuery(ByVal sql As String) As MySqlDataReader
        Dim query As MySqlCommand = Nothing
        Dim reader As MySqlDataReader = Nothing
        Dim fields As New List(Of String)
        SyncLock Me.connection
            Try
                Logger.Log("Query: " & sql, LogLevel.Verbose)
                If Not Me.connection.State = ConnectionState.Open Then
                    Me.connection.Open()
                End If
                'If Not reader Is Nothing Then
                'If Not reader.IsClosed = True Then
                'Threading.Thread.Sleep(1)
                'End If
                'End If
                query = New MySqlCommand(sql)
                query.Connection = Me.connection
                query.Prepare()

                reader = query.ExecuteReader()


                Return reader
            Catch ex As Exception
                If reader.IsClosed = False Then
                    reader.Close()
                End If
                Logger.Log("Failed to execute Query " & sql & vbCrLf & ex.ToString, LogLevel.Warning)
            End Try
            Return Nothing
        End SyncLock
      
    End Function

    Public Function NonQuery(ByVal sql As String) As Boolean
        Dim query As MySqlCommand = Nothing
        SyncLock Me.connection
            Try
                Logger.Log("Query: " & sql, LogLevel.Verbose)
                If Not Me.connection.State = ConnectionState.Open Then
                    Me.connection.Open()
                End If

                query = New MySqlCommand(sql)
                query.Connection = Me.connection
                query.Prepare()
                query.ExecuteNonQuery()
            Catch ex As Exception
                Logger.Log("Failed to execute Query " & sql & vbCrLf & ex.ToString, LogLevel.Warning)
                Return False
            End Try
        End SyncLock
        Return True

    End Function

    Public Function EscapeString(ByVal sql As String) 'Die bösen Sachen filtern 
        Return MySqlHelper.EscapeString(sql)
    End Function

    Public Sub Close()
        If Not Me.connection.State = ConnectionState.Open Then
            Me.connection.Close()
        End If
        connection = Nothing
    End Sub
   


    Private Function GetUnixTimestamp(ByVal time As DateTime) As Int64
        Return (DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
    End Function

    Private Function GetDateTime(ByVal timestamp As Int64) As DateTime
        Dim dt As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        dt.AddSeconds(timestamp).ToLocalTime()
        Return dt
    End Function


    Public Sub RegisterNatnegToken(ByVal localIPEP As Net.IPEndPoint, ByVal rIPEP As Net.IPEndPoint, ByVal gamename As String)

    End Sub

    Public Function FetchServerIPEPByToken(ByVal token As Int32) As Net.IPEndPoint
        Return New Net.IPEndPoint(Net.Dns.Resolve("clyde.janelo.net").AddressList(0), 3658)
    End Function

End Class

