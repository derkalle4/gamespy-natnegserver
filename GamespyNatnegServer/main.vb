'GameMaster Nat-negotitation-server v1.2
'JW "LeKeks" 07/2014
Module Main
    Private server As GamespyServer

    '<System.STAThread> _
    Sub Main()
        Console.WriteLine(PRODUCT_NAME)
        server = New GamespyServer()
        server.Run()
    End Sub

End Module