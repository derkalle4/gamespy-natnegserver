Public Class PacketBase
    Public Property PacketId As Byte
    Public Property GameName As String
    Public Property data As Byte()
    Public Property requestedKey As Byte() = {}

    Friend bytesParsed As Int32 = 8

    Friend Function FetchString(ByVal buffer() As Byte) As String
        Dim strEnd As Int32 = Array.IndexOf(buffer, CByte(0), bytesParsed)
        Dim buf(strEnd - bytesParsed - 1) As Byte
        Array.Copy(buffer, bytesParsed, buf, 0, buf.Length)
        bytesParsed = strEnd + 1
        Return System.Text.Encoding.ASCII.GetString(buf)
    End Function
    Friend Sub ConcatArray(ByVal source() As Byte, ByRef dest() As Byte, Optional ByVal separator As Byte = Byte.MaxValue)
        Dim newSize As Int32 = dest.Length + source.Length
        Dim oldSize As Int32 = dest.Length
        If separator <> Byte.MaxValue Then newSize += 1

        Array.Resize(dest, newSize)
        Array.Copy(source, 0, dest, oldSize, source.Length)

        If separator <> Byte.MaxValue Then dest(dest.Length - 1) = separator
    End Sub
    Friend Function GetBytes(ByVal str As String) As Byte()
        Return System.Text.Encoding.ASCII.GetBytes(str)
    End Function
    Friend Function GetString(ByVal bytes() As Byte) As String
        Return System.Text.Encoding.ASCII.GetString(bytes)
    End Function
    Friend Function BuildInvertedUInt16Array(ByVal value As UInt16) As Byte()
        Dim buffer() As Byte = BitConverter.GetBytes(value)
        Array.Reverse(buffer)
        Return buffer
    End Function
    Friend Function GetInvertedUInt16(ByVal buffer() As Byte, ByVal offset As Int32)
        Dim buf() As Byte = {buffer(offset + 1), buffer(offset)}
        Return BitConverter.ToUInt16(buf, 0)
    End Function
    Friend Function FetchKey(ByVal data() As Byte) As Byte()
        Dim buffer(7) As Byte
        Array.Copy(data, Me.bytesParsed, buffer, 0, buffer.Length)
        bytesParsed += 8
        Return buffer
    End Function

    'Vorlagen
    Public Overridable Function CompileResponse() As Byte()
        Return {}
    End Function
    Public Overridable Sub ManageData()
        Return
    End Sub


    Friend Function GetIPEndPointFromByteArray(ByVal buffer() As Byte, ByVal offset As Int32) As Net.IPEndPoint
        Dim queryIP(3) As Byte
        Array.Copy(buffer, offset, queryIP, 0, 4)
        Return New Net.IPEndPoint(New Net.IPAddress(queryIP), GetInvertedUInt16(buffer, offset + 4))
    End Function

End Class