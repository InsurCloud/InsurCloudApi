Imports System.Web
Imports System.Runtime.Serialization

<Serializable()> _
Public Class ExceptionContext
    Private _exception As SerializableException
    Private _contextobjects As SerializableDictionary(Of Object)
    Private _sourcesystem As String
    Private _referenceType As String
    Private _referenceID As String
    Private _systemTS As String

    Property ExceptionToLog() As SerializableException
        Get
            Return _exception
        End Get
        Set(ByVal value As SerializableException)
            _exception = value
        End Set
    End Property

    Property ContextObjects() As SerializableDictionary(Of Object)
        Get
            Return _contextobjects
        End Get
        Set(ByVal value As SerializableDictionary(Of Object))
            _contextobjects = value
        End Set
    End Property

    Property SourceSystem() As String
        Get
            Return _sourcesystem
        End Get
        Set(ByVal value As String)
            _sourcesystem = value
        End Set
    End Property

    Property ReferenceType() As String
        Get
            Return _referenceType
        End Get
        Set(ByVal value As String)
            _referenceType = value
        End Set
    End Property

    Property ReferenceID() As String
        Get
            Return _referenceID
        End Get
        Set(ByVal value As String)
            _referenceID = value
        End Set
    End Property

    Property SystemTS() As String
        Get
            Return _systemTS
        End Get
        Set(ByVal value As String)
            _systemTS = value
        End Set
    End Property
#Region "Functions"

    Public Sub New()
        Me.ContextObjects = New SerializableDictionary(Of Object)
        Me.SourceSystem = String.Empty
        Me.ReferenceID = String.Empty
        Me.ReferenceType = String.Empty
        Me.SystemTS = Date.Now()
    End Sub

    Public Sub New(ByVal e As Exception)
        AddException(e)
        Me.ContextObjects = New SerializableDictionary(Of Object)
        Me.SourceSystem = String.Empty
        Me.ReferenceID = String.Empty
        Me.ReferenceType = String.Empty
        Me.SystemTS = Date.Now()
    End Sub

    Public Sub AddException(ByVal e As Exception)
        _exception = New SerializableException()
        _exception.innerEx = e
    End Sub

    Public Sub AddSession(ByVal session As System.Web.SessionState.HttpSessionState)
        If Not Me.ContextObjects.ContainsKey("Session") Then
            If session.Count > 0 Then
                Dim tempSession As New SerializableDictionary(Of Object)
                For Each key As String In session.Keys
                    tempSession.Add(key, session(key))
                Next
                ContextObjects.Add("Session", tempSession)
            Else
                ContextObjects.Add("Session", "Empty")
            End If
        Else
            If TypeOf (ContextObjects("Session")) Is String And session.Count > 0 Then
                ContextObjects("Session") = session
            End If
        End If
    End Sub

    Public Sub AddContext(ByVal ObjectName As String, ByVal obj As Object)
        Dim i As Integer = 2
        ObjectName.Replace(" ", "_")
        While ContextObjects.Keys.Contains(ObjectName)
            ObjectName = ObjectName & i
            i = i + 1
        End While

        If obj IsNot Nothing Then
            ContextObjects.Add(ObjectName, obj)
        Else
            ContextObjects.Add(ObjectName, "NULL")
        End If
    End Sub

    Public Function LogError() As Integer
        Dim ErrorSvc As New ErrorLoggingService.ErrorLoggingServiceSoapClient()

        Dim errorid As Integer
        If ReferenceID = String.Empty And ReferenceType = String.Empty Then
            errorid = ErrorSvc.LogErrorXML(CommonFunctions.SerializeToXML(Me), SourceSystem, Date.Now())
        Else
            errorid = ErrorSvc.LogErrorXMLWithReference(CommonFunctions.SerializeToXML(Me), SourceSystem, ReferenceType, ReferenceID, Date.Now())
        End If
        ErrorSvc = Nothing
        Return errorid
    End Function
#End Region
End Class


