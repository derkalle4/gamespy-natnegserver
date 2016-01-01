'Packet to tell the natneg-peer that the session is ready
'or to report any error which occured while processing the request
'JW "LeKeks" 7/2014
Public Class NatnegConnectPacket
    Inherits GamespyNatnegPacket

    Public Property Failed As Boolean = False       'true if the request failed
    Public Property Destination As Net.IPEndPoint   'the remote-peer's host-IPEP
    Public Property cookie As Int32                 'the session cookie

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
        MyBase.New(server, remoteIPEP, protocolVersion)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buffer() As Byte = {}

        'Set the requeststate-byte
        Dim requestState As Byte = GS_NATNEG_CONNECTSTATE_OK
        If Me.Failed Then
            requestState = GS_NATNEG_CONNECTSTATE_FAIL
            Me.Destination = New Net.IPEndPoint(New Net.IPAddress(0), 0)
        End If

        'Temp
        Dim t As Byte = &H42

        'Build the packet
        ConcatArray(GS_SERVICE_NATNEG_PREFIX, buffer)
        ConcatArray(GS_NATNEG_HEADER, buffer)
        ConcatArray({Me.ProtocolVersion, GS_NATNEG_CMD_CONNECT}, buffer)
        ConcatArray(BitConverter.GetBytes(Me.cookie), buffer)
        ConcatArray(Me.Destination.Address.GetAddressBytes, buffer)
        ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(Destination.Port), buffer)
        ConcatArray({t}, buffer)
        ConcatArray({requestState}, buffer)

        Return buffer
    End Function
End Class