Module constants
    Public Const PRODUCT_NAME As String = "GameMaster NAT negotitation masterserver"
    Public Const CFG_DIR As String = "/cfg"         'main config file
    Public Const CFG_FILE As String = "/core.xml"   'config directory

    Public GS_SERVICE_NATNEG_PREFIX As Byte() = {&HFD, &HFC}        'packet prefix
    Public GS_NATNEG_HEADER As Byte() = {&H1E, &H66, &H6A, &HB2}    'packet header
    Public GS_NATNEG_FIN As Byte() = {&HFF, &HFF, &H6D, &H16, &HB5, &H7D, &HEA} 'packet fin.

    Public Const GS_NATNEG_CMD_INIT As Byte = &H0           'requests new natneg-session
    Public Const GS_NATNEG_CMD_INIT_ACK As Byte = &H1       'validates arrival of a init-packet
    Public Const GS_NATNEG_CMD_CONNECT As Byte = &H5        'tells the client to start connecting now
    Public Const GS_NATNEG_CMD_CONNECT_ACK As Byte = &H6    'validates that the connect

    Public Const GS_NATNEG_MINSEQUENCE As Byte = &H2        'No. of init-p.'s by one client before attemp. connect
    Public Const GS_NATNEG_OLDPROTOCOL As Byte = &H1        'min. protocol for falling back to old packets

    Public Const GS_NATNEG_CLIENTTYPE_GUEST As Byte = &H0   'ID for Guests
    Public Const GS_NATNEG_CLIENTTYPE_HOST As Byte = &H1    'ID for Hosts

    Public Const GS_NATNEG_CONNECTSTATE_OK As Byte = &H0    'Indicated the connection is ready
    Public Const GS_NATNEG_CONNECTSTATE_FAIL As Byte = &H1  'Indicates a problem with t.o. client

    'Public Const GS_NATNEG_PROTOCOL_VERSION As Byte = &H2
    'Public Const DEBUG_IGNORE_OTHER_PROTOCOLVERSION As Boolean = False

    Public Const DEBUGMODE_ENABLE As Boolean = False        'Enables verbose debugging

    Public Const P2P_CMD_NATNEGCONNECT As Byte = &H20       'P2P-Protocol for Packet-Proxy
End Module