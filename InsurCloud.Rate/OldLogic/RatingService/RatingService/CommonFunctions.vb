Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Xml
Imports System.Xml.Schema
Imports System.Xml.Serialization
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Security.Cryptography
Imports System.Web.Security


Public Class CommonFunctions

    Private Const _aesKey As String = "$LandryCourageUltimateQuestion42"

    Public Shared Function GetRow(ByVal Table As DataTable, ByVal sFactorName As String) As System.Data.DataRow

        Dim drFactorRows() As DataRow = Nothing

        drFactorRows = Table.Select("FactorName='" & sFactorName & "'")

        If drFactorRows.Length > 0 Then
            Return drFactorRows(0)
        End If

        Return Nothing


    End Function

    Public Shared Function GetIncreasedLimitRow(ByVal Table As DataTable, ByVal sFactorName As String) As System.Data.DataRow

        Dim drFactorRows() As DataRow = Nothing
        drFactorRows = Table.Select("FactorName LIKE '" & sFactorName & "%'")

        If drFactorRows.Length > 0 Then
            Return drFactorRows(0)
        End If

        Return Nothing


    End Function

    Public Shared Function GetNumbersOnlyFromString(ByVal sSource As String) As String
        Dim sResult As String = ""

        For i As Int64 = 0 To sSource.Length - 1
            If IsNumeric(sSource.Substring(i, 1)) Then
                sResult &= sSource.Substring(i, 1)
            End If
        Next

        Return sResult
    End Function

    Public Shared Function GetTableOwner(ByVal sProduct As String, ByVal sStateCode As String) As String

        Dim sTableOwner As String = "pgm" 'pgm142

        If Val(sProduct) > 0 Then
            sTableOwner &= sProduct
        Else
            Select Case UCase$(sProduct)
                Case "HOMEOWNERS"
                    sTableOwner = sTableOwner & "1"
                Case "PERSONALAUTO"
                    sTableOwner = sTableOwner & "2"
                Case "COMMERCIALAUTO"
                    sTableOwner &= "3"
                Case "FLOOD"
                    sTableOwner &= "4"
                Case Else
                    sTableOwner = sTableOwner & "1"
            End Select
        End If

        If Val(sStateCode) > 0 Then
            sTableOwner = sTableOwner & sStateCode
        Else
            sTableOwner = sTableOwner & GetStateCode(sStateCode)
        End If

        Return sTableOwner

    End Function

#Region "Abbreviations"

    Public Shared Function GetStateCode(ByVal sStateCode As String) As String
        'use the state from the VersionInfo sheet to find the statecode
        Dim sGetStateCode As String = ""

        Try
            Select Case sStateCode
                Case "AL"
                    sGetStateCode = "01"
                Case "AZ"
                    sGetStateCode = "02"
                Case "AR"
                    sGetStateCode = "03"
                Case "CA"
                    sGetStateCode = "04"
                Case "CO"
                    sGetStateCode = "05"
                Case "CT"
                    sGetStateCode = "06"
                Case "DE"
                    sGetStateCode = "07"
                Case "DC"
                    sGetStateCode = "08"
                Case "FL"
                    sGetStateCode = "09"
                Case "GA"
                    sGetStateCode = "10"
                Case "ID"
                    sGetStateCode = "11"
                Case "IL"
                    sGetStateCode = "12"
                Case "IN"
                    sGetStateCode = "13"
                Case "IA"
                    sGetStateCode = "14"
                Case "KS"
                    sGetStateCode = "15"
                Case "KY"
                    sGetStateCode = "16"
                Case "LA"
                    sGetStateCode = "17"
                Case "ME"
                    sGetStateCode = "18"
                Case "MD"
                    sGetStateCode = "19"
                Case "MA"
                    sGetStateCode = "20"
                Case "MI"
                    sGetStateCode = "21"
                Case "MN"
                    sGetStateCode = "22"
                Case "MS"
                    sGetStateCode = "23"
                Case "MO"
                    sGetStateCode = "24"
                Case "MT"
                    sGetStateCode = "25"
                Case "NE"
                    sGetStateCode = "26"
                Case "NV"
                    sGetStateCode = "27"
                Case "NH"
                    sGetStateCode = "28"
                Case "NJ"
                    sGetStateCode = "29"
                Case "NM"
                    sGetStateCode = "30"
                Case "NY"
                    sGetStateCode = "31"
                Case "NC"
                    sGetStateCode = "32"
                Case "ND"
                    sGetStateCode = "33"
                Case "OH"
                    sGetStateCode = "34"
                Case "OK"
                    sGetStateCode = "35"
                Case "OR"
                    sGetStateCode = "36"
                Case "PA"
                    sGetStateCode = "37"
                Case "RI"
                    sGetStateCode = "38"
                Case "SC"
                    sGetStateCode = "39"
                Case "SD"
                    sGetStateCode = "40"
                Case "TN"
                    sGetStateCode = "41"
                Case "TX"
                    sGetStateCode = "42"
                Case "UT"
                    sGetStateCode = "43"
                Case "VT"
                    sGetStateCode = "44"
                Case "VA"
                    sGetStateCode = "45"
                Case "WA"
                    sGetStateCode = "46"
                Case "WV"
                    sGetStateCode = "47"
                Case "WI"
                    sGetStateCode = "48"
                Case "WY"
                    sGetStateCode = "49"
                Case "HI"
                    sGetStateCode = "52"
                Case "AK"
                    sGetStateCode = "54"
                Case Else
                    sGetStateCode = ""
            End Select

            Return sGetStateCode

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally

        End Try

    End Function

    Public Shared Function GetStateAbbr(ByVal sStateCode As String) As String

        Dim sStateAbbr As String = ""
        Dim sStateNum As String = ""

        Select Case sStateCode
            Case "54"
                sStateAbbr = "AK"
            Case "01"
                sStateAbbr = "AL"
            Case "03"
                sStateAbbr = "AR"
            Case "02"
                sStateAbbr = "AZ"
            Case "04"
                sStateAbbr = "CA"
            Case "05"
                sStateAbbr = "CO"
            Case "06"
                sStateAbbr = "CT"
            Case "08"
                sStateAbbr = "DC"
            Case "07"
                sStateAbbr = "DE"
            Case "09"
                sStateAbbr = "FL"
            Case "10"
                sStateAbbr = "GA"
            Case "52"
                sStateAbbr = "HI"
            Case "14"
                sStateAbbr = "IA"
            Case "11"
                sStateAbbr = "ID"
            Case "12"
                sStateAbbr = "IL"
            Case "13"
                sStateAbbr = "IN"
            Case "15"
                sStateAbbr = "KS"
            Case "16"
                sStateAbbr = "KY"
            Case "17"
                sStateAbbr = "LA"
            Case "20"
                sStateAbbr = "MA"
            Case "19"
                sStateAbbr = "MD"
            Case "18"
                sStateAbbr = "ME"
            Case "21"
                sStateAbbr = "MI"
            Case "22"
                sStateAbbr = "MN"
            Case "24"
                sStateAbbr = "MO"
            Case "23"
                sStateAbbr = "MS"
            Case "25"
                sStateAbbr = "MT"
            Case "32"
                sStateAbbr = "NC"
            Case "33"
                sStateAbbr = "ND"
            Case "26"
                sStateAbbr = "NE"
            Case "28"
                sStateAbbr = "NH"
            Case "29"
                sStateAbbr = "NJ"
            Case "30"
                sStateAbbr = "NM"
            Case "27"
                sStateAbbr = "NV"
            Case "31"
                sStateAbbr = "NY"
            Case "34"
                sStateAbbr = "OH"
            Case "35"
                sStateAbbr = "OK"
            Case "36"
                sStateAbbr = "OR"
            Case "37"
                sStateAbbr = "PA"
            Case "38"
                sStateAbbr = "RI"
            Case "39"
                sStateAbbr = "SC"
            Case "40"
                sStateAbbr = "SD"
            Case "41"
                sStateAbbr = "TN"
            Case "42"
                sStateAbbr = "TX"
            Case "43"
                sStateAbbr = "UT"
            Case "45"
                sStateAbbr = "VA"
            Case "44"
                sStateAbbr = "VT"
            Case "46"
                sStateAbbr = "WA"
            Case "48"
                sStateAbbr = "WI"
            Case "47"
                sStateAbbr = "WV"
            Case "49"
                sStateAbbr = "WY"
        End Select

        GetStateAbbr = sStateAbbr

    End Function

    Public Shared Function GetStateName(ByVal sStateAbbr As String) As String
        Dim sStateName As String = ""

        Select Case sStateAbbr
            Case "AL"
                sStateName = "Alabama"
            Case "AZ"
                sStateName = "Arizona"
            Case "AR"
                sStateName = "Arkansas"
            Case "CA"
                sStateName = "California"
            Case "CO"
                sStateName = "Colorado"
            Case "CT"
                sStateName = "Connecticut"
            Case "DE"
                sStateName = "Deleware"
            Case "DC"
                sStateName = "District of Columbia"
            Case "FL"
                sStateName = "Florida"
            Case "GA"
                sStateName = "Georgia"
            Case "ID"
                sStateName = "Idaho"
            Case "IL"
                sStateName = "Illinois"
            Case "IN"
                sStateName = "Indiana"
            Case "IA"
                sStateName = "Iowa"
            Case "KS"
                sStateName = "Kansas"
            Case "KY"
                sStateName = "Kentucky"
            Case "LA"
                sStateName = "Louisiana"
            Case "ME"
                sStateName = "Maine"
            Case "MD"
                sStateName = "Maryland"
            Case "MA"
                sStateName = "Massachusetts"
            Case "MI"
                sStateName = "Michigan"
            Case "MN"
                sStateName = "Minnesota"
            Case "MS"
                sStateName = "Mississippi"
            Case "MO"
                sStateName = "Missouri"
            Case "MT"
                sStateName = "Montana"
            Case "NE"
                sStateName = "Nebraska"
            Case "NV"
                sStateName = "Nevada"
            Case "NH"
                sStateName = "New Hampshire"
            Case "NJ"
                sStateName = "New Jersey"
            Case "NM"
                sStateName = "New Mexico"
            Case "NY"
                sStateName = "New York"
            Case "NC"
                sStateName = "North Carolina"
            Case "ND"
                sStateName = "North Dakota"
            Case "OH"
                sStateName = "Ohio"
            Case "OK"
                sStateName = "Oklahoma"
            Case "OR"
                sStateName = "Oregon"
            Case "PA"
                sStateName = "Pennsylvania"
            Case "RI"
                sStateName = "Rhode Island"
            Case "SC"
                sStateName = "South Carolina"
            Case "SD"
                sStateName = "South Dakota"
            Case "TN"
                sStateName = "Tennessee"
            Case "TX"
                sStateName = "Texas"
            Case "UT"
                sStateName = "Utah"
            Case "VT"
                sStateName = "Vermont"
            Case "VA"
                sStateName = "Virginia"
            Case "WA"
                sStateName = "Washington"
            Case "WV"
                sStateName = "West Virginia"
            Case "WI"
                sStateName = "Wisconson"
            Case "WY"
                sStateName = "Wyoming"
            Case "HI"
                sStateName = "Hawaii"
            Case "AK"
                sStateName = "Alaska"
            Case Else
                sStateName = ""
        End Select

        Return sStateName

    End Function

#End Region

    Public Shared Function RoundStandard(ByVal dNumber As Decimal, ByVal iDecimalPrecision As Integer) As Decimal

        Dim dFactor As Decimal = Convert.ToDecimal(Math.Pow(10, iDecimalPrecision))
        Dim iSign As Integer = Math.Sign(dNumber)

        Return Decimal.Truncate(dNumber * dFactor + 0.5 * iSign) / dFactor
    End Function

    Public Shared Function GetStateInfoValue(ByVal iProduct As Integer, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sProgram As String, ByVal sItemGroup As String, ByVal sItemCode As String, ByVal sItemSubCode As String) As String
        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        Dim oStateInfoDataSet As DataSet = Nothing 'LoadStateInfoTable(iProduct, sStateCode, dtRateDate)
        Dim sItemValue As String = ""

        Dim sSql As String = ""

        Dim oConn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("ConnectionString").ToString)
        'Dim oConn As New SqlConnection(CorPolicy.My.Settings("ConnStr"))

        Dim oDS As New DataSet


        Using cmd As New SqlCommand(sSql, oConn)

            sSql = " SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue "
            sSql = sSql & " FROM pgm" & iProduct & sStateCode & "..StateInfo"
            sSql = sSql & " WHERE EffDate <= @RateDate "
            sSql = sSql & " AND ExpDate > @RateDate "
            sSql = sSql & " ORDER BY Program, ItemGroup, ItemCode "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate

            Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

            adapter.Fill(oDS, "StateInfo")

        End Using

        oStateInfoDataSet = oDS

        oConn.Close()
        oConn.Dispose()

        oStateInfoTable = oStateInfoDataSet.Tables(0)

        DataRows = oStateInfoTable.Select("Program IN ('" & sProgram & "', 'HOM', 'PPA') AND ItemGroup='" & sItemGroup & "' AND ItemCode='" & sItemCode & "' AND ItemSubCode='" & sItemSubCode & "'")

        For Each oRow As DataRow In DataRows
            sItemValue = oRow("ItemValue").ToString
        Next

        Return sItemValue

    End Function

    Public Shared Function SerializeToXML(Of T)(ByVal ObjectToSerialize As T) As System.Xml.Linq.XElement
        Dim oStrW As New StringWriter()
        Dim sXML As New System.Xml.Linq.XDocument
        Dim MySerializer As XmlSerializer
        Dim sString As String = ""

        'Set the objects
        MySerializer = New XmlSerializer(ObjectToSerialize.GetType)

        'Serialize object into an XML String

        Dim oXMLW As XmlWriter = sXML.CreateWriter()
        MySerializer.Serialize(oXMLW, ObjectToSerialize)

        oXMLW.Close()

        Return sXML.Root
    End Function

    Public Shared Function DeserializeFromXML(Of T)(ByVal ObjectOfTypeToDeserializeTo As T, ByVal XMLDoc As XmlDocument) As T
        Dim MySerializer As XmlSerializer
        Dim stringReader As StringReader
        Dim xmlReader As XmlTextReader
        Dim returnObject As T

        MySerializer = New XmlSerializer(ObjectOfTypeToDeserializeTo.GetType)
        stringReader = New StringReader(XMLDoc.OuterXml())
        xmlReader = New XmlTextReader(stringReader)

        Dim reader As XmlNodeReader = New XmlNodeReader(XMLDoc.DocumentElement)

        returnObject = MySerializer.Deserialize(reader)
        xmlReader.Close()
        stringReader.Close()
        stringReader.Dispose()

        Return returnObject
    End Function

    Public Shared Function DeserializeFromXML(Of T)(ByVal ObjectOfTypeToDeserializeTo As T, ByVal sXML As String) As T
        Dim MySerializer As XmlSerializer
        Dim stringReader As StringReader
        Dim xmlReader As XmlTextReader
        Dim returnObject As T

        MySerializer = New XmlSerializer(ObjectOfTypeToDeserializeTo.GetType)
        stringReader = New StringReader(sXML)
        xmlReader = New XmlTextReader(stringReader)


        returnObject = MySerializer.Deserialize(xmlReader)
        xmlReader.Close()
        stringReader.Close()

        Return returnObject
    End Function

    Public Shared Function Clone(Of T)(ByVal ObjectToClone As T) As T
        'copies original object to stream then 
        'deserializes that stream and returns the output
        'to create clone (copy) of object

        Dim objMemStream As New MemoryStream(5000)
        Dim objBinaryFormatter As New BinaryFormatter(Nothing, New StreamingContext(StreamingContextStates.Clone))

        objBinaryFormatter.Serialize(objMemStream, ObjectToClone)

        objMemStream.Seek(0, SeekOrigin.Begin)

        Clone = objBinaryFormatter.Deserialize(objMemStream)

        objMemStream.Close()

        objMemStream.Dispose()

    End Function

    Public Shared Function SterilizeValueForSQL(ByVal valueToSterilize As String, Optional ByVal maxLength As Integer = -1) As String
        Dim result As String = String.Empty

        If String.IsNullOrEmpty(valueToSterilize) Then
            result = valueToSterilize
        Else
            valueToSterilize = valueToSterilize.Replace("'", "''")
            'valueToSterilize = valueToSterilize.Replace("]", "]]'")

            If maxLength > 0 AndAlso valueToSterilize.Length > maxLength Then
                Throw New ArgumentException("String Value is longer than allowed maximum length.")
            End If


        End If


        Return result
    End Function

    Public Shared Function GetDateTimeWithTimeZone(ByVal dateToConvert As Date) As String


        Dim dateWithTimeZone As String = dateToConvert.ToString

        Dim timeZoneStr As String
        If TimeZone.CurrentTimeZone.IsDaylightSavingTime(dateToConvert) Then
            timeZoneStr = TimeZone.CurrentTimeZone.DaylightName
        Else
            timeZoneStr = TimeZone.CurrentTimeZone.StandardName
        End If

        dateWithTimeZone += " "
        For Each value As String In timeZoneStr.Split(" ")

            If Not String.IsNullOrEmpty(value) Then
                dateWithTimeZone += Char.ToUpper(value(0))
            End If

        Next

        Return dateWithTimeZone

    End Function

    Public Shared Function GetTimeWithTimeZone(ByVal dateToConvert As Date) As String


        Dim timeWithTimeZone As String = dateToConvert.ToShortTimeString

        Dim timeZoneStr As String
        If TimeZone.CurrentTimeZone.IsDaylightSavingTime(dateToConvert) Then
            timeZoneStr = TimeZone.CurrentTimeZone.DaylightName
        Else
            timeZoneStr = TimeZone.CurrentTimeZone.StandardName
        End If

        timeWithTimeZone += " "
        For Each value As String In timeZoneStr.Split(" ")

            If Not String.IsNullOrEmpty(value) Then
                timeWithTimeZone += Char.ToUpper(value(0))
            End If

        Next

        Return timeWithTimeZone

    End Function

End Class
