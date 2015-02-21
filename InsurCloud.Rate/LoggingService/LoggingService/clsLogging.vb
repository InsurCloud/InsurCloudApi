Imports Microsoft.VisualBasic
Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Xml
Imports System.Xml.Serialization
Imports System.IO
Imports System.Text
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions

'< _
'System.Xml.Serialization.XmlElement("DataTable", typeof(DataTable)) , _
'System.Xml.Serialization.XmlElement("Policy", typeof(clsPolicyHomeOwner)), _
'System.Xml.Serialization.XmlElement("StartTimeStamp", typeof(DateTime)), _
'System.Xml.Serialization.XmlElement("EndTimeStamp", typeof(DateTime)), _
'System.Xml.Serialization.XmlElement("LogItems", typeof(List(of clsLogItem))) _
'> _
<Serializable()> _
<System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42"), _
     System.Web.Services.WebServiceBindingAttribute(Name:="LoggingSoap", [Namespace]:="http://www.imperialfire.com/"), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsBaseCoverage)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsBaseUnit)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsBaseEntity)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsBasePolicy)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsPolicyHomeOwner)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsPolicyFlood)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsPolicyPPA)), _
     System.Xml.Serialization.XmlIncludeAttribute(GetType(clsPolicyCV))> _
Public MustInherit Class clsLogging

    Protected mdtDataTable As New List(Of DataTable)
    Protected mdtDriverDataTable As New List(Of DataTable)
    Protected mdtStartTimeStamp As DateTime
    Protected mdtEndTimeStamp As DateTime
    Protected moLogItems As New List(Of clsLogItem)
    Protected mbWriteLogXML As Boolean = True

#Region "Properties"

    Public Property StartTimeStamp() As DateTime
        Get
            StartTimeStamp = mdtStartTimeStamp
        End Get
        Set(ByVal value As DateTime)
            mdtStartTimeStamp = value
        End Set
    End Property

    Public Property EndTimeStamp() As DateTime
        Get
            EndTimeStamp = mdtEndTimeStamp
        End Get
        Set(ByVal value As DateTime)
            mdtEndTimeStamp = value
        End Set
    End Property

    Public Property DataTable() As List(Of DataTable)
        Get
            Return mdtDataTable
        End Get
        Set(ByVal value As List(Of DataTable))
            mdtDataTable = value
        End Set
    End Property

    Public Property DriverDataTable() As List(Of DataTable)
        Get
            Return mdtDriverDataTable
        End Get
        Set(ByVal value As List(Of DataTable))
            mdtDriverDataTable = value
        End Set
    End Property

    Public Property LogItems() As List(Of clsLogItem)
        Get
            Return moLogItems
        End Get
        Set(ByVal value As List(Of clsLogItem))
            moLogItems = value
        End Set
    End Property

    Public Property WriteLogXML() As Boolean
        Get
            Return mbWriteLogXML
        End Get
        Set(ByVal value As Boolean)
            mbWriteLogXML = value
        End Set
    End Property
#End Region

#Region "Functions"
    Public MustOverride Function GetInsertSQL() As String
    Public MustOverride Sub GetInsertParms(ByVal sXML As System.Xml.XmlDocument, ByRef cmd As SqlCommand)
    Public MustOverride Function GetInsertErrorSQL() As String
    Public MustOverride Sub GetInsertErrorParms(ByVal sXML As System.Xml.XmlDocument, ByRef cmd As SqlCommand, ByVal sMethodName As String, ByVal sErrorMsg As String)

    Public Function WriteLogToXML(ByVal oConn As SqlConnection, Optional ByVal sFilePath As String = "") As Boolean

        'write moLogging to xml file
        Dim Serializer As New XmlSerializer(Me.GetType)
        Dim XmlWriter As New StringWriter
        Dim sSql As String = ""
        Dim sXML As New XmlDocument

        Try

            If mbWriteLogXML Then
                Serializer.Serialize(XmlWriter, Me)
                sXML.LoadXml(XmlWriter.ToString())
                XmlWriter.Close()
            End If


            Me.EndTimeStamp = Now

            Using cmd As New SqlCommand(sSql, oConn)
                sSql = Me.GetInsertSQL()
                cmd.Parameters.Clear()

                Me.GetInsertParms(sXML, cmd)

                cmd.CommandText = sSql
                Console.WriteLine(sSql)
                cmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        End Try

    End Function

    Public Function WriteErrorLogToXML(ByVal oConn As SqlConnection, ByVal sMethodName As String, ByVal sErrorMsg As String) As Boolean

        'write moLogging to xml file
        Dim Serializer As New XmlSerializer(Me.GetType)
        Dim XmlWriter As New StringWriter
        Dim sXML As New XmlDocument
        Dim sSql As String = ""

        Try

            Me.EndTimeStamp = Now

            Serializer.Serialize(XmlWriter, Me)
            sXML.LoadXml(XmlWriter.ToString())
            XmlWriter.Close()

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = Me.GetInsertErrorSQL()
                
                cmd.Parameters.Clear()

                Me.GetInsertErrorParms(sXML, cmd, sMethodName, sErrorMsg)
               
                cmd.CommandText = sSql
                Console.WriteLine(sSql)
                cmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        End Try

    End Function
#End Region

    Public Sub New()
        Me.StartTimeStamp = Now
    End Sub
End Class
