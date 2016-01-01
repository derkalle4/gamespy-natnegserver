
Public Class NatnegAckPacket
    Inherits GamespyUdpPacket

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        MyBase.New(server, remoteIPEP)
    End Sub

    Public Property Sequence As Byte()
    Public Property Cookie As Int32

    Public Overrides Function CompileResponse() As Byte()
        Dim buffer() As Byte = GS_SERVICE_NATNEG_PREFIX
        ConcatArray(GS_NATNEG_HEADER, buffer)
        ConcatArray({GS_NATNEG_PROTOCOL_VERSION}, buffer)
        ConcatArray({GS_NATNEG_CMD_ACK}, buffer)
        ConcatArray(BitConverter.GetBytes(Me.Cookie), buffer)
        ConcatArray(Sequence, buffer)
        ConcatArray(GS_NATNEG_FIN, buffer)

        Return buffer
    End Function
End Class


