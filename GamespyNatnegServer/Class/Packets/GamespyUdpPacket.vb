Public Class GamespyUdpPacket
    Inherits PacketBase

    Public Property Server As UdpServer
    Public Property RemoteIPEP As Net.IPEndPoint


    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        Me.Server = server
        Me.RemoteIPEP = remoteIPEP
    End Sub
End Class
