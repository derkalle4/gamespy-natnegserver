'Packet for forwarding connects to another masterserver which handles to 
'UDP-session to the NAT-gadget
'JW "LeKeks" 08/2014
Public Class ConnectForwardPacket
    Inherits P2PUdpPacket

    Public Property Cookie As Int32
    Public Property FwdIPEP As Net.IPEndPoint
    Public Property RemotePeer As Net.IPEndPoint
    Public Property ProtocolVersion As Byte

    Sub New(ByVal server As P2PServerHandler, ByVal rIPEP As Net.IPEndPoint)
        MyBase.New(server, rIPEP)
    End Sub

    Public Overrides Sub ManageData()
        Me.bytesParsed += 1
        Me.ProtocolVersion = Me.data(bytesParsed)
        Me.bytesParsed += 1
        Me.FwdIPEP = ArrayFunctions.GetIPEndPointFromByteArray(Me.data, Me.bytesParsed)
        Me.bytesParsed += 6
        Me.Cookie = BitConverter.ToInt32(Me.data, Me.bytesParsed)
        Me.bytesParsed += 4
        Me.RemotePeer = ArrayFunctions.GetIPEndPointFromByteArray(Me.data, Me.bytesParsed)

        Dim ncp As New NatnegConnectPacket(Me.Server, Me.FwdIPEP, Me.ProtocolVersion)
        ncp.cookie = Cookie
        ncp.Destination = Me.RemotePeer
        Me.GSServer.GSUdpServer.send(ncp)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buf() As Byte = {P2P_CMD_NATNEGCONNECT, Me.ProtocolVersion}
        ConcatArray(Me.FwdIPEP.Address.GetAddressBytes, buf)
        ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(FwdIPEP.Port), buf)
        ConcatArray(BitConverter.GetBytes(Me.Cookie), buf)
        ConcatArray(Me.RemotePeer.Address.GetAddressBytes, buf)
        ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(RemotePeer.Port), buf)
        Return buf
    End Function
End Class
