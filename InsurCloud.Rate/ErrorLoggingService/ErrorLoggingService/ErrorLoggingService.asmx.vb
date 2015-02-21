Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.IO
Imports System.Xml
Imports System.Xml.Schema
Imports System.Xml.Serialization
Imports System.Data
Imports System.Data.SqlClient

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="com.insurcloud")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class ErrorLoggingService
    Inherits System.Web.Services.WebService

    <WebMethod()> _
    Public Function TestLoggingWithReference(ByVal SourceSystem As String, ByVal ReferenceType As String, ByVal ReferenceID As String, ByVal SystemTS As Date) As Integer
        Return LogErrorXMLWithReference(New XmlDocument(), SourceSystem, ReferenceType, ReferenceID, SystemTS)
    End Function

    <WebMethod()> _
    Public Function TestLogging(ByVal SourceSystem As String, ByVal SystemTS As Date) As Integer
        Return LogErrorXML(New XmlDocument(), SourceSystem, SystemTS)
    End Function

    <WebMethod()> _
    Public Function LogErrorXML(ByVal sXML As XmlDocument, ByVal SourceSystem As String, ByVal SystemTS As Date) As Integer
        Return SaveErrorToDB(sXML, SourceSystem, SystemTS, "", "")
    End Function

    <WebMethod()> _
    Public Function LogErrorXMLWithReference(ByVal sXML As XmlDocument, ByVal SourceSystem As String, ByVal ReferenceType As String, ByVal ReferenceID As String, ByVal SystemTS As Date) As Integer
        Return SaveErrorToDB(sXML, SourceSystem, SystemTS, ReferenceType, ReferenceID)
    End Function

    Private Function SaveErrorToDB(ByVal sXML As XmlDocument, ByVal SourceSystem As String, ByVal SystemTS As Date, ByVal ReferenceType As String, ByVal ReferenceID As String) As Integer
        Dim iIdentity As Integer
        ' Then save this as a new endorsement
        Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSQL As String = ""

        'Insert the String into the database
        sSQL = "INSERT INTO Common..ErrorXML"
        sSQL &= " Values("
        sSQL &= " @ErrorXml, "
        sSQL &= " @SourceSystem, "
        sSQL &= " @ReferenceType,"
        sSQL &= " @ReferenceID,"
        sSQL &= " @SystemTS);"
        sSQL &= " Select @@Identity"

        'Open the connection
        cn.Open()

        Using cmd As SqlCommand = New SqlCommand(sSQL, cn)
            'Set the parameters
            cmd.Parameters.Add("@SystemTS", SqlDbType.DateTime, 8).Value = Date.Parse(SystemTS)
            cmd.Parameters.Add("@SourceSystem", SqlDbType.VarChar, 50).Value = SourceSystem
            cmd.Parameters.Add("@ReferenceType", SqlDbType.VarChar, 50).Value = ReferenceType
            cmd.Parameters.Add("@ReferenceID", SqlDbType.VarChar, 50).Value = ReferenceID
            cmd.Parameters.Add("@ErrorXml", SqlDbType.Xml).Value = sXML.OuterXml.ToString()

            'Execute the SQL
            iIdentity = cmd.ExecuteScalar()
        End Using

        'Close the connection
        cn.Close()
        cn.Dispose()

        Return iIdentity
    End Function
End Class
