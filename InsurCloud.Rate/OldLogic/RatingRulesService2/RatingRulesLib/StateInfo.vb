<Serializable()>
Public Class StateInfo
    Private _program As String
    Private _itemGroup As String
    Private _itemCode As String
    Private _itemValue As String
    Private _itemSubCode As String
    Private _appliesToCode As Char
    Private _effDate As DateTime
    Private _expDate As DateTime
    Private _userID As String
    Private _systemTS As DateTime

    Public Property Program() As String
        Get
            Return _program
        End Get
        Set(ByVal value As String)
            _program = value
        End Set
    End Property

    Public Property ItemGroup() As String
        Get
            Return _itemGroup
        End Get
        Set(ByVal value As String)
            _itemGroup = value
        End Set
    End Property

    Public Property ItemCode() As String
        Get
            Return _itemCode
        End Get
        Set(ByVal value As String)
            _itemCode = value
        End Set
    End Property

    Public Property ItemSubCode() As String
        Get
            Return _itemSubCode
        End Get
        Set(ByVal value As String)
            _itemSubCode = value
        End Set
    End Property

    Public Property ItemValue() As String
        Get
            Return _itemValue
        End Get
        Set(ByVal value As String)
            _itemValue = value
        End Set
    End Property

    Public Property AppliesToCode() As Char
        Get
            Return _appliesToCode
        End Get
        Set(ByVal value As Char)
            _appliesToCode = value
        End Set
    End Property

    Public Property EffDate() As DateTime
        Get
            Return _effDate
        End Get
        Set(ByVal value As DateTime)
            _effDate = value
        End Set
    End Property
    
    Public Property ExpDate() As DateTime
        Get
            Return _expDate
        End Get
        Set(ByVal value As DateTime)
            _expDate = value
        End Set
    End Property
    
    Public Property UserID() As String
        Get
            Return _userID
        End Get
        Set(ByVal value As String)
            _userID = value
        End Set
    End Property

    Public Property SystemTS() As DateTime
        Get
            Return _systemTS
        End Get
        Set(ByVal value As DateTime)
            _systemTS = value
        End Set
    End Property

    Public Shared Function GetNew(ByVal itemSubCode As String, Optional ByVal userID As String = "", Optional ByVal program As String = "PPA") As StateInfo

        Return New StateInfo With {.Program = program, .ItemGroup = "WEATHEROVERRIDE", .ItemCode = "COUNTY", .ItemSubCode = itemSubCode, _
                                   .ItemValue = "TRUE", .AppliesToCode = CChar("B"), .EffDate = New DateTime(DateTime.Now.Year, 1, 1), _
                                    .ExpDate = New DateTime(2050, 12, 31), .UserID = userID}
    End Function

End Class
