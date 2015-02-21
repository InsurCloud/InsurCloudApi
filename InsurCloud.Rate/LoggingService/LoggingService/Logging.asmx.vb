Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.SqlTypes


<WebService(Namespace:="com.insurcloud")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class Logging
    Inherits System.Web.Services.WebService

    <WebMethod()> _
    Public Sub WriteHomeownersLog(ByVal oLog As clsLogging1, ByVal sMethodName As String, ByVal sErrorMsg As String)
        Try
            Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.AppSettings("ConnStr"))
            oConn.Open()
            If sErrorMsg <> "" Then
                oLog.WriteErrorLogToXML(oConn, sMethodName, sErrorMsg)
            Else
                oLog.WriteLogToXML(oConn)
            End If

            oConn.Close()
        Catch ex As Exception
            'Event Log Entry
        End Try
    End Sub
    <WebMethod()> _
    Public Sub WriteFloodLog(ByVal oLog As clsLogging4, ByVal sMethodName As String, ByVal sErrorMsg As String)
        Try
            Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.AppSettings("ConnStr"))
            oConn.Open()
            If sErrorMsg <> "" Then
                oLog.WriteErrorLogToXML(oConn, sMethodName, sErrorMsg)
            Else
                oLog.WriteLogToXML(oConn)
            End If
            oConn.Close()
        Catch ex As Exception
            'Event Log Entry
        End Try
    End Sub
    <WebMethod()> _
    Public Sub WriteAutoLog(ByVal oLog As clsLogging2, ByVal sMethodName As String, ByVal sErrorMsg As String)
        Try
            Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.AppSettings("ConnStr"))
            oConn.Open()
            If sErrorMsg <> "" Then
                oLog.WriteErrorLogToXML(oConn, sMethodName, sErrorMsg)
            Else
                oLog.WriteLogToXML(oConn)
            End If
            oConn.Close()
        Catch ex As Exception
            'Event Log Entry
        End Try
    End Sub

End Class
