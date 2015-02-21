<Serializable()>
Public Class ProgramSetting
    Private _program As String
    Private _settingName As String
    Private _settingDesc As String
    Private _value As String
    Private _appliesToCode As Char
    Private _effDate As DateTime
    Private _expDate As DateTime

    Public Property Program() As String
        Get
            Return _program
        End Get
        Set(value As String)
            _program = value
        End Set
    End Property

    Public Property SettingName As String
        Get
            Return _settingName
        End Get
        Set(value As String)
            _settingName = value
        End Set
    End Property

    Public Property SettingDesc As String
        Get
            Return _settingDesc
        End Get
        Set(value As String)
            _settingDesc = value
        End Set
    End Property

    Public Property ProgramValue() As String
        Get
            Return _value
        End Get
        Set(value As String)
            _value = value
        End Set
    End Property

    Public Property AppliesToCode() As Char
        Get
            Return _appliesToCode
        End Get
        Set(value As Char)
            _appliesToCode = value
        End Set
    End Property

    Public Property EffDate As DateTime
        Get
            Return _effDate
        End Get
        Set(value As DateTime)
            _effDate = value
        End Set
    End Property

    Public Property ExpDate As DateTime
        Get
            Return _expDate
        End Get
        Set(value As DateTime)
            _expDate = value
        End Set
    End Property

    Public Function IsValid() As Boolean
        Return (Not String.IsNullOrEmpty(_program) And Not String.IsNullOrEmpty(_settingName) And Not String.IsNullOrEmpty(_settingDesc) _
                And Not String.IsNullOrEmpty(_value) And Not IsNothing(_appliesToCode))
    End Function

    Public Shared Function GetNew(ByVal startDate As DateTime, ByVal program As String) As ProgramSetting

        Dim result As New ProgramSetting With {.Program = program, .SettingName = "WeatherOverrideDate", .SettingDesc = "WeatherOverride", _
                                               .ProgramValue = startDate, .AppliesToCode = CChar("B"), .EffDate = New DateTime(DateTime.Now.Year, 1, 1), _
                                               .ExpDate = New DateTime(2050, 12, 31)}
        Return result

    End Function

    Public Shared Operator =(ByVal x As ProgramSetting, ByVal y As ProgramSetting) As Boolean

        Return (x.Program = y.Program And x.SettingName = y.SettingName And x.AppliesToCode = y.AppliesToCode _
                And x.EffDate = y.EffDate And x.ExpDate = y.ExpDate)

    End Operator

    Public Shared Operator <>(ByVal x As ProgramSetting, ByVal y As ProgramSetting) As Boolean
        Return Not x = y
    End Operator

End Class
