Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Public Class UdpServer
    Public Property Address As ipaddress
    Public Property Port As Int32 = 27900

    Private listenThread As Thread
    Private client As UdpClient
    Private alterClient As UdpClient
    Private running As Boolean

    Public Property Server As GamespyServer

    'Public Event ClientConnected(ByVal sender As UdpServer, ByVal client As TcpClient)
    Public Event BindFailed(ByVal sender As UdpServer, ByVal ex As Exception)

    Sub New(ByVal server As GamespyServer)
        Me.Server = server
    End Sub

    Public Sub Open()
        If Not running Then
            Try
                client = New UdpClient(New Net.IPEndPoint(Me.Address, Me.Port))
                alterClient = New UdpClient()
                'dpClien
            Catch ex As Exception
                Logger.Log("Bind failed [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Exception)
                RaiseEvent BindFailed(Me, ex)
            End Try
            running = True
            Me.listenThread = New Thread(AddressOf Me.Listen)
            Me.listenThread.Start()
            Logger.Log("Server Heartbeat Listener started [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Info)
        End If
    End Sub

    Public Sub Close()
        If running Then
            running = False
            Me.client.Close()
            Me.alterClient.Close()
        End If
    End Sub

    Private Sub Listen()
        While running
            Try
                Dim rIPEP As IPEndPoint = Nothing
                Dim data() As Byte = Me.client.Receive(rIPEP)

                If data.Length > 0 Then
                    Me.OnDataInput(data, rIPEP)
                End If
            Catch ex As Exception
                Logger.Log(ex.ToString, LogLevel.Verbose)
            End Try
            Threading.Thread.Sleep(10)
        End While
    End Sub

    Friend Overridable Sub OnDataInput(ByVal data() As Byte, ByVal rIPEP As IPEndPoint)
        Logger.Log("Fetched " & data.Count & " Bytes from " & rIPEP.Address.ToString & " [UDP]", LogLevel.Verbose)
        Dim packet As GamespyUdpPacket

        If data(0) <> GS_SERVICE_NATNEG_PREFIX(0) Or data(1) <> GS_SERVICE_NATNEG_PREFIX(1) Then
            Logger.Log("Received broken Packet", LogLevel.Verbose)
            Return
        End If

        If (data(6) <> GS_NATNEG_PROTOCOL_VERSION And DEBUG_IGNORE_OTHER_PROTOCOLVERSION) Then
            Logger.Log("Packet has wrong Protocol Version!", LogLevel.Verbose)
            Return
        End If

        Select Case data(7)
            Case GS_NATNEG_CMD_INIT
                packet = New NatnegInitPacket(Me, rIPEP)
            Case Else
                Logger.Log("Unkown UDP Packet #" & data(0) & " (" & rIPEP.Address.ToString & ")", LogLevel.Verbose)
                Return
        End Select
        packet.data = data
        packet.ManageData()
    End Sub

    Public Sub send(ByVal data() As Byte, ByVal rIPEP As Net.IPEndPoint)
        Try
            Me.client.Send(data, data.Length, rIPEP)
            Logger.Log("Sending to  " & rIPEP.ToString, LogLevel.Verbose)
        Catch ex As Exception
            Logger.Log("Couldn't send UDP-Packet to " & rIPEP.Address.ToString, LogLevel.Warning)
        End Try
    End Sub

    Public Sub sendFromAlterSocket(ByVal data() As Byte, ByVal rIPEP As Net.IPEndPoint)
        Try
            alterClient.Send(data, data.Length, rIPEP)
            Logger.Log("[socket2] Sending to  " & rIPEP.ToString, LogLevel.Verbose)
        Catch ex As Exception
            Logger.Log("[socket2] Couldn't send UDP-Packet to " & rIPEP.Address.ToString, LogLevel.Warning)
        End Try
    End Sub

End Class
