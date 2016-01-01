'Packet for handling acknowledgement by the natneg-peers
'JW "LeKeks" 07/2014
Public Class NatnegConnectAckdPacket
    Inherits GamespyNatnegPacket

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
        MyBase.New(server, remoteIPEP, protocolVersion)
    End Sub

    Private cookie As Int32
    Private clientType As Byte

    Public Overrides Sub ManageData()
        Logger.Log(Me.RemoteIPEP.ToString & " ack'd Connect", LogLevel.Verbose)
        Me.cookie = BitConverter.ToInt32(Me.data, Me.bytesParsed)
        Me.bytesParsed += 5 'skip first byte
        Me.clientType = Me.data(Me.bytesParsed)

        'Drop the session if the Guest ack'd connect (it'll do that after the host)
        If Me.clientType = GS_NATNEG_CLIENTTYPE_GUEST Then
            Logger.Log("Dropping session " & Me.cookie.ToString, LogLevel.Verbose)
            Me.GSServer.MySQL.DropSession(cookie)
        End If
    End Sub
End Class