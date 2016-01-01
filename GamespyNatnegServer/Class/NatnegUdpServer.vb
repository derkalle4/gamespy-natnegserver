'UdpServer-Wrapper for Handling Natneg-packets
'JW "LeKeks" 07/2014
Public Class NatnegUdpServer
    Inherits UdpServer

    Sub New(ByVal server As GamespyServer)
        MyBase.New(server)
    End Sub

    Friend Overrides Sub OnDataInput(data() As Byte, rIPEP As Net.IPEndPoint)

        'check the packet's itegrity
        If data(0) <> GS_SERVICE_NATNEG_PREFIX(0) Or data(1) <> GS_SERVICE_NATNEG_PREFIX(1) Or data.Length < 7 Then
            Logger.Log("Received broken Packet", LogLevel.Verbose)
            Return
        End If

        'get the packet's protocol version
        Dim protocol As Byte = data(6)

        'Handle the packet
        Dim packet As GamespyNatnegPacket
        Select Case data(7)
            Case GS_NATNEG_CMD_INIT
                packet = New NatnegInitPacket(Me, rIPEP, protocol)
            Case GS_NATNEG_CMD_CONNECT_ACK
                packet = New NatnegConnectAckdPacket(Me, rIPEP, protocol)
            Case Else
                Logger.Log("Unkown UDP Packet #" & data(0) & " (" & rIPEP.Address.ToString & ")", LogLevel.Verbose)
                Return
        End Select
        packet.data = data
        packet.ManageData()
        MyBase.OnDataInput(data, rIPEP)
    End Sub
End Class
