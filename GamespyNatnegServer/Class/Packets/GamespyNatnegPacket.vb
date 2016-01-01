'Packetbase for Nat-Negotitationpackets
'JW "LeKeks" 07/2014
Public Class GamespyNatnegPacket
    Inherits GamespyUdpPacket
    Public Property ProtocolVersion As Byte = &H2

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
        MyBase.New(server, remoteIPEP)
        Me.ProtocolVersion = protocolVersion    'Fetch the protocol version
        Me.bytesParsed = 8                      'Already parsed 8 Bytes: 2(prefix) + 5(header) + 1(version)
    End Sub
End Class