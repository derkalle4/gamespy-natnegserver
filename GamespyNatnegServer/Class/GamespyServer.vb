'GameMaster Natneg Server Emulator main class
'JW "LeKeks" 06/2014
Public Class GamespyServer
    Public Property GSUdpServer As NatnegUdpServer
    Public Property MySQL As NatnegMySQLHandler

    Public Property MSP2PHandler As P2PServerHandler
    Public Property Config As CoreConfig

    Private ConfigMan As ConfigSerializer
    Private DBCleaner As DatabaseCleaner

#Region "Programm"
    Public Sub Run()
        Me.PreInit()
        Me.Execution()
        Me.PostInit()
    End Sub
    Private Sub PreInit()
        Me.ConfigMan = New ConfigSerializer(GetType(CoreConfig))
        Me.Config = Me.ConfigMan.LoadFromFile(CFG_FILE, CurDir() & CFG_DIR)

        Me.GSUdpServer = New NatnegUdpServer(Me)
        Me.GSUdpServer.Address = Net.IPAddress.Parse(Me.Config.UDPHeartbeatAddress)
        Me.GSUdpServer.Port = Me.Config.UDPHeartbeatPort

        Me.MySQL = New NatnegMySQLHandler()
        Me.MySQL.Hostname = Me.Config.MySQLHostname
        Me.MySQL.Port = Me.Config.MySQLPort
        Me.MySQL.DbName = Me.Config.MySQLDatabase
        Me.MySQL.DbUser = Me.Config.MySQLUsername
        Me.MySQL.DbPwd = Me.Config.MySQLPwd
        Me.MySQL.MasterServerID = Me.Config.ServerID

        Me.DBCleaner = New DatabaseCleaner()
        Me.DBCleaner.CleanupInterval = Me.Config.CleanupInterval
        Me.DBCleaner.CleanupTimeout = Me.Config.CleanupTimeout
        Me.DBCleaner.MySQL = Me.MySQL

        If Me.Config.P2PEnable Then
            Me.MSP2PHandler = New P2PServerHandler(Me)
            Me.MSP2PHandler.Address = Net.IPAddress.Parse(Me.Config.P2PAddress)
            Me.MSP2PHandler.Port = Me.Config.P2PPort
            Me.MSP2PHandler.EncKey = System.Text.Encoding.ASCII.GetBytes(Me.Config.P2PKey)
        End If

        Logger.MinLogLevel = Me.Config.Loglevel
        Logger.LogToFile = Me.Config.LogToFile
        Logger.LogFileName = Me.Config.LogFileName
    End Sub

    Private Sub Execution()
        Me.MySQL.Connect()
        Me.DBCleaner.init()
        Me.GSUdpServer.Open()
        If Me.Config.P2PEnable Then Me.MSP2PHandler.Open()
        Logger.Log("Launch OK. Server is up.", LogLevel.Info)
        Logger.Log("Press [Return] to exit", LogLevel.Info)


        'Debug-code for injecting test packets
        'Dim cfp As New ConnectForwardPacket(Me.MSP2PHandler, New Net.IPEndPoint(Net.IPAddress.Parse("127.0.0.1"), 1234))
        'cfp.Cookie = 121212
        'cfp.FwdIPEP = New Net.IPEndPoint(Net.IPAddress.Parse("1.2.3.4"), 1234)
        'cfp.RemotePeer = New Net.IPEndPoint(Net.IPAddress.Parse("2.2.3.4"), 1234)
        'cfp.ProtocolVersion = 2
        'cfp.data = cfp.CompileResponse
        'cfp.ManageData()
        'protocol1 debug:
        ' Do
        ' Console.ReadLine()
        ' Me.GSUdpServer.InjectPacket({&HFD, &HFC, &H1E, &H66, &H6A, &HB2, &H1, &H0, &H93, &H89, &H37, &H86, &H1, &H0, &H1}, New Net.IPEndPoint(Net.IPAddress.Parse("192.168.178.45"), 1234))
        ' Loop

        Console.ReadLine()
        Logger.Log("Shutting down...", LogLevel.Info)
    End Sub

    Private Sub PostInit()
        Me.GSUdpServer.Close()
        If Me.Config.P2PEnable Then Me.MSP2PHandler.Close()
        Me.DBCleaner.terminate()
        Me.MySQL.Close()

        Me.MSP2PHandler = Nothing
        Me.DBCleaner = Nothing
        Me.ConfigMan = Nothing
        Me.Config = Nothing
        GC.Collect()
        Logger.Log("Server stopped.", LogLevel.Info)
    End Sub
#End Region

End Class
