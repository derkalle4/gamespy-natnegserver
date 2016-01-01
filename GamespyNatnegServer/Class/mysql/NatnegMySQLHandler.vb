'MySQL-Wrapper for NAT negititation functionality
'JW "LeKeks" 07/2014
Imports MySql.Data.MySqlClient
Public Class NatnegMySQLHandler
    Inherits MySQLHandler

    'The masterserver's own internal ID
    Public Property MasterServerID As Int32

    'Starts a new session or updates an existing one
    Public Sub RegisterNatnegToken(ByVal localIPEP As Net.IPEndPoint,
                                   ByVal rIPEP As Net.IPEndPoint,
                                   ByVal cookie As Int32,
                                   ByVal gamename As String,
                                   ByVal sequence As Byte,
                                   ByVal clienttype As Byte,
                                   Optional ByVal comport As Int32 = -1)

        Dim sql As String = String.Empty

        If (ClientExists(cookie, clienttype)) Then
            If comport <> -1 Then
                'was sent using the client's natneg-comport
                sql =
                "update `natneg` set " & _
                "`natneg_sequence` = (`natneg_sequence` + 1), " & _
                "`natneg_localport` = " & localIPEP.Port.ToString & ", " & _
                "`natneg_comport` = " & comport.ToString & _
                " where " & _
                "`natneg_clienttype` = '" & clienttype.ToString & "' and " & _
                "`natneg_cookie` = " & cookie.ToString
            Else
                'was sent using the client's host-port
                sql =
                "update `natneg` set " & _
                "`natneg_sequence` = (`natneg_sequence` + 1), " & _
                "`natneg_remoteip` = '" & rIPEP.Address.ToString & "', " & _
                "`natneg_remoteport` = " & rIPEP.Port.ToString & _
                " where " & _
                "`natneg_clienttype` = '" & clienttype.ToString & "' and " & _
                "`natneg_cookie` = " & cookie.ToString
            End If
        Else
            'No session for that client -> create a new one
            sql =
             "insert into `natneg` set " & _
             "`natneg_cookie` = " & cookie.ToString & ", " & _
             "`natneg_gamename` = '" & EscapeString(gamename) & "', " & _
             "`natneg_sequence` = 0, " & _
             "`natneg_clienttype` = " & clienttype.ToString & ", " & _
             "`natneg_remoteip` = '" & rIPEP.Address.ToString & "', " & _
             "`natneg_remoteport` = " & rIPEP.Port.ToString & ", " & _
             "`natneg_localip` = '" & localIPEP.Address.ToString & "', " & _
             "`natneg_localport` = " & localIPEP.Port.ToString & ", " & _
             "`natneg_masterserver` = " & MasterServerID.ToString & ", " & _
             "`natneg_lastupdate` = UNIX_TIMESTAMP()"
        End If

        'Run the Query
        Me.NonQuery(sql)
    End Sub

    'Checks if both sessions are ready for operation
    Public Function NatnegReady(ByVal cookie As Int32) As Boolean
        SyncLock Me.connection

            'Sum the sequence-IDs to get the total amount of init-packets
            Dim sql As String =
                "select SUM(`natneg_sequence`) as sequence from `natneg` where " & _
                "`natneg_cookie` = " & cookie.ToString

            'Run the Query and fetch the sum
            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()
                Dim c As Int32 = res("sequence")
                res.Close()
                'Check if it's >= then the minimum sequence required
                Return (c >= 2 * GS_NATNEG_MINSEQUENCE)
            End Using
        End SyncLock
    End Function

    'Checks if there's a session for a client
    Public Function ClientExists(ByVal cookie As Int32, ByVal clientType As Byte) As Boolean
        SyncLock Me.connection
            Dim sql As String =
                "select `id` from `natneg` where " & _
                "`natneg_cookie` = " & cookie.ToString & " and " & _
                "`natneg_clienttype` = '" & clientType.ToString & "'"

            Return Me.CheckForRows(Me.DoQuery(sql))
        End SyncLock
    End Function

    'Gets a client's remote peer
    Public Function FetchRemotePeer(ByVal cookie As Int32, ByVal ownClientType As Byte) As NatnegPeer
        SyncLock Me.connection
            'select the client, it must be another clienttype and has to share the same cookie
            'we also need the masterserver to check if we have to do a server-relay or can send
            'directly so we select that as well
            Dim sql As String =
              "select * from `natneg` " & _
              "left join `masterserver` on `masterserver`.`id` = `natneg`.`natneg_masterserver` where " & _
              "`natneg_cookie` = " & cookie.ToString & " and " & _
              "`natneg_clienttype` != " & ownClientType.ToString


            'Get the Data
            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()

                'If there are now rows exit the function
                If Not res.HasRows Then
                    res.Close()
                    Return Nothing
                End If

                'Create a var for the peer
                Dim peer As NatnegPeer = New NatnegPeer()
                Try
                    'Get the required values
                    Dim rAddr As Net.IPAddress = Net.IPAddress.Parse(res("natneg_remoteip"))
                    Dim rPort As UInt16 = UInt16.Parse(res("natneg_remoteport"))
                    Dim cPort As UInt16 = UInt16.Parse(res("natneg_comport"))

                    'Generate the IPEP-objects
                    peer.hostIPEP = New Net.IPEndPoint(rAddr, rPort)
                    peer.comIPEP = New Net.IPEndPoint(rAddr, cPort)

                    'Check if there is a masterserver-setting (might be disabled by config)
                    If Not DBNull.Value.Equals(res("server_name")) Then
                        'Build the Masterserver-set
                        Dim ms As New MasterServer
                        ms.id = res("natneg_masterserver")
                        ms.msName = res("server_name")
                        ms.rIPEP = New Net.IPEndPoint(Net.IPAddress.Parse(res("server_natnegaddress")), UInt16.Parse(res("server_natnegport")))
                        peer.ms = ms
                    End If
                    res.Close()
                Catch ex As Exception
                    'Just in case
                    res.Close()
                    Logger.Log("Failed to fetch Server " & cookie.ToString, LogLevel.Verbose)
                End Try
                Return peer
            End Using
        End SyncLock
    End Function

    'Removes both sessions
    Public Sub DropSession(ByVal cookie As Int32)
        'Execute a Query to drop the rows
        Me.NonQuery("delete from `natneg` where `natneg_cookie` = " & cookie.ToString)
    End Sub

    'Fetches details about a masterserver
    Public Function FetchMasterserver(ByVal rIPEP As Net.IPEndPoint) As MasterServer
        Dim sql As String = _
            "select `id`, `server_name` from `masterserver` " & _
            "where `server_address` = '" & rIPEP.Address.ToString & "' and " & _
            "`server_port` = " & rIPEP.Port.ToString
        SyncLock Me.connection

            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()
                If res.HasRows Then
                    Dim ms As New MasterServer
                    ms.id = res("id")
                    ms.msName = res("server_name")
                    ms.rIPEP = rIPEP
                    res.Close()
                    Return ms
                Else
                    res.Close()
                    Return Nothing
                End If
            End Using
        End SyncLock
    End Function

    'Gets a full list of masterservers
    Public Function GetMasterServers() As List(Of MasterServer)
        Dim sql As String = _
            "select * from `masterserver`"
        Dim servers As New List(Of MasterServer)
        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)
                If Not res Is Nothing Then
                    While res.Read
                        Dim ms As New MasterServer
                        ms.id = res("id")
                        ms.msName = res("server_name")
                        ms.rIPEP = New Net.IPEndPoint(Net.IPAddress.Parse(res("server_nataddress")), UInt16.Parse(res("server_natport")))
                        servers.Add(ms)
                    End While
                    res.Close()
                End If
            End Using
        End SyncLock
        Return servers
    End Function

    'Gets a masterserver by it's id
    Public Function FetchMasterserverById(ByVal id As Int32) As MasterServer
        Dim sql As String = _
            "select * from `masterserver` " & _
            "where `id` = " & id.ToString

        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()
                If res.HasRows Then
                    Dim ms As New MasterServer
                    ms.id = id
                    ms.msName = res("server_name")
                    ms.rIPEP = New Net.IPEndPoint(Net.IPAddress.Parse(res("server_nataddress")), UInt16.Parse(res("server_natport")))
                    res.Close()
                    Return ms
                Else
                    res.Close()
                    Return Nothing
                End If
            End Using
        End SyncLock
    End Function

End Class