'Packet for handling session requests/updates
'JW "LeKeks" 07/2014
Public Class NatnegInitPacket
    Inherits GamespyNatnegPacket

    Private cookie As Int32             'session's cookie
    Private sequence As Byte            'sequence id
    Private clienttype As Byte          'client type (guest/host)
    Private usegameport As Byte         'shall we use the game's def.-port?
    Private localIPEP As Net.IPEndPoint 'the client's local IPEP

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
        MyBase.New(server, remoteIPEP, protocolVersion)
    End Sub

    Public Overrides Sub ManageData()
        Logger.Log("Client init: " & Me.RemoteIPEP.ToString, LogLevel.Verbose)

        'The cookie consists out of 4 bytes
        Me.cookie = BitConverter.ToInt32(Me.data, Me.bytesParsed)
        Me.bytesParsed += 4

        'The next 3 bytes deliver the further information
        Me.sequence = Me.data(Me.bytesParsed)
        Me.clienttype = Me.data(Me.bytesParsed + 1)
        Me.usegameport = Me.data(Me.bytesParsed + 2)
        Me.bytesParsed += 3

        'The gamename will be attached for some games, however some will not -> just ignore it
        Me.GameName = "protocol #" & Me.ProtocolVersion.ToString()

        'The newer protocol will also send a local IPEP
        If Me.ProtocolVersion > GS_NATNEG_OLDPROTOCOL Then
            Me.localIPEP = GetIPEndPointFromByteArray(Me.data, Me.bytesParsed)
            Me.bytesParsed += 6
            'Me.GameName = Me.FetchString(Me.data)
        Else
            Me.localIPEP = New Net.IPEndPoint(0, 0)
        End If

        Logger.Log("Received Natneg Init Packet #{0}", LogLevel.Verbose, Me.sequence.ToString())

        'Create a new session
        If sequence = 0 Then
            Me.GSServer.MySQL.RegisterNatnegToken(Me.localIPEP, Me.RemoteIPEP, Me.cookie, Me.GameName, Me.sequence, Me.clienttype)

            If clienttype = GS_NATNEG_CLIENTTYPE_GUEST Then
                Logger.Log("Creating Host Session {0} for {1} / {2}", LogLevel.Verbose, Me.cookie.ToString(), Me.RemoteIPEP.ToString(), Me.localIPEP.ToString())
            Else
                Logger.Log("Creating Guest Session {0} for {1} / {2}", LogLevel.Verbose, Me.cookie.ToString(), Me.RemoteIPEP.ToString(), Me.localIPEP.ToString())
            End If
        Else 'Update an existing session
            Me.GSServer.MySQL.RegisterNatnegToken(Me.localIPEP, Me.RemoteIPEP, Me.cookie, GameName, Me.sequence, Me.clienttype, Me.RemoteIPEP.Port)
            Logger.Log("Updating Session {0} for {1} / {2}", LogLevel.Verbose, Me.cookie.ToString(), Me.RemoteIPEP.ToString(), Me.localIPEP.ToString())
        End If

        'ack the packet
        Me.Server.send(Me)

        'If both sessions are ready tell the peers to connect to each other
        If Me.GSServer.MySQL.NatnegReady(Me.cookie) Then
            Logger.Log("Init Sequence OK for '" & Me.cookie.ToString & "', '" & Me.GameName & "' at " & Me.RemoteIPEP.ToString & "/" & Me.localIPEP.ToString, LogLevel.Verbose)
            Me.PerformConnect()
        End If
    End Sub

    Private Sub ConnectPeers(ByVal peer As NatnegPeer, ByVal remotePeer As NatnegPeer)
        'Check if we can use the local socket
        If Not Me.GSServer.Config.P2PEnable Or peer.ms.id = Me.GSServer.Config.ServerID Then
            Dim CP As New NatnegConnectPacket(Me.Server, peer.comIPEP, Me.ProtocolVersion)
            CP.Destination = remotePeer.hostIPEP
            CP.cookie = Me.cookie

            If IsNothing(remotePeer.comIPEP) Then               'seems like there's no server
                CP.Failed = True                                'report error
                Logger.Log("Ready to connect {0} but no Peer could be found - sending error", LogLevel.Verbose, Me.cookie.ToString())
            ElseIf IsNothing(peer.comIPEP) Then 'own peer failed -> no point of sending a report
                Logger.Log("Ready to connect {0} but no Peer could be found", LogLevel.Verbose, Me.cookie.ToString())
                Return
            Else
                Logger.Log("Connecting {0} to peer at {1}", LogLevel.Verbose, peer.comIPEP.ToString(), remotePeer.hostIPEP.ToString())
            End If

            Me.Server.send(CP)
        Else 'Forward the packet using MS-P2P protocol
            Logger.Log("Connecting via {0} ({1}) to Peer at {2}", LogLevel.Verbose, peer.ms.msName, peer.ms.rIPEP.Port.ToString(), remotePeer.hostIPEP.ToString())
            'setup the packet and send it
            Dim cfp As New ConnectForwardPacket(Me.GSServer.MSP2PHandler, peer.ms.rIPEP)
            cfp.Cookie = Me.cookie
            cfp.FwdIPEP = peer.comIPEP
            cfp.RemotePeer = remotePeer.hostIPEP
            cfp.ProtocolVersion = Me.ProtocolVersion
            Me.GSServer.MSP2PHandler.send(cfp)
        End If
    End Sub
    Private Sub PerformConnect()
        Dim hostPeer As NatnegPeer = Me.GSServer.MySQL.FetchRemotePeer(Me.cookie, GS_NATNEG_CLIENTTYPE_HOST)
        Dim guestPeer As NatnegPeer = Me.GSServer.MySQL.FetchRemotePeer(Me.cookie, GS_NATNEG_CLIENTTYPE_GUEST)

        ConnectPeers(guestPeer, hostPeer)
        ConnectPeers(hostPeer, guestPeer)
    End Sub
    Public Overrides Function CompileResponse() As Byte()
        Dim buffer() As Byte = {}
        'Build the response-packet
        ConcatArray(GS_SERVICE_NATNEG_PREFIX, buffer)
        ConcatArray(GS_NATNEG_HEADER, buffer)
        ConcatArray({Me.ProtocolVersion, GS_NATNEG_CMD_INIT_ACK}, buffer)
        ConcatArray(BitConverter.GetBytes(Me.cookie), buffer)
        ConcatArray({Me.sequence, Me.clienttype}, buffer)
        ConcatArray(GS_NATNEG_FIN, buffer)
        Return buffer
    End Function
End Class