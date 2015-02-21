Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Data.SqlClient
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Activator
Imports CorPolicy.clsCommonFunctions
Imports CorPolicy
Imports log4net
Imports log4net.Config

<WebService(Namespace:="com.insurcloud/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class RatingService
    Inherits System.Web.Services.WebService

    Private ReadOnly log4net As ILog

    Sub New()
        log4net = LogManager.GetLogger(GetType(RatingService))
        XmlConfigurator.Configure()
    End Sub

    <WebMethod(CacheDuration:=0)> _
    Public Function PPAPolicyConversion(ByRef oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsBasePolicy

        Dim oRate As New clsPgm2()

        Try

            'Pre Rating Stuff
            Select Case oPolicy.StateCode
                Case "02"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm202()
                Case "03"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm203()
                Case "09"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm209()
                Case "17"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm217()
                Case "35"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm235()
                Case "42"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm242()
                Case Else
                    oRate = New clsPgm2()
            End Select


            If Not oRate Is Nothing Then
                oPolicy = oRate.PolicyConversion(oPolicy, sRatedProgram)
                oPolicy = GetActualRateDate(oPolicy)
            End If

            Return oPolicy

        Catch ex As Exception
            'log it
            Dim errCtx As ExceptionContext = New ExceptionContext(ex)
            errCtx.AddContext("Policy", oPolicy)
            errCtx.AddContext("oRate", oRate)
            errCtx.AddContext("sRatedProgram", sRatedProgram)
            If oPolicy IsNot Nothing And oPolicy.PolicyID <> "" Then
                errCtx.ReferenceID = oPolicy.PolicyID
                errCtx.ReferenceType = "PolicyID"
            ElseIf oPolicy IsNot Nothing And oPolicy.QuoteID <> "" Then
                errCtx.ReferenceID = oPolicy.QuoteID
                errCtx.ReferenceType = "QuoteID"

            End If
            errCtx.SourceSystem = "RatingService"
            errCtx.SystemTS = Date.Now
            errCtx.LogError()

            Dim oEx As New SoapException(ex.Message & " - " & ex.StackTrace, New System.Xml.XmlQualifiedName("string", "http://services.imperialfire.com/databaseerror"))
            Throw oEx
        Finally

            If Not oRate Is Nothing Then
                oRate = Nothing
            End If

        End Try
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPARate(ByRef oPolicy As clsPolicyPPA, ByVal bLogRate As Boolean) As clsBasePolicy

        Dim oRate As New clsPgm2()

        Try

            'Pre Rating Stuff
            Select Case oPolicy.StateCode
                Case "02"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm202()
                Case "03"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm203()
                Case "09"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm209()
                Case "17"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm217()
                Case "35"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm235()
                Case "42"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm242()
                Case Else
                    oRate = New clsPgm2()
            End Select


            If Not oRate Is Nothing Then
                oRate.ApplyCapFactor(oPolicy, bLogRate)
                oRate.Rate(oPolicy, bLogRate)
            End If

            Return oPolicy

        Catch ex As Exception
            'log it
            Dim errCtx As ExceptionContext = New ExceptionContext(ex)
            errCtx.AddContext("Policy", oPolicy)
            errCtx.AddContext("oRate", oRate)
            errCtx.AddContext("bLogRate", bLogRate)
            If oPolicy IsNot Nothing Then
                errCtx.ReferenceID = oPolicy.PolicyID
                errCtx.ReferenceType = "PolicyID"
            End If
            errCtx.SourceSystem = "RatingService"
            errCtx.SystemTS = Date.Now
            errCtx.LogError()

            Dim oEx As New SoapException(ex.Message & " - " & ex.StackTrace, New System.Xml.XmlQualifiedName("string", "http://services.imperialfire.com/databaseerror"))
            Throw oEx
        Finally

            If Not oRate Is Nothing Then
                oRate = Nothing
            End If

        End Try
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPANonPremiumEndorse(ByRef oPolicy As clsPolicyPPA, ByVal bLogRate As Boolean) As clsBasePolicy

        Dim oRate As New clsPgm2()

        Try

            'Pre Rating Stuff
            Select Case oPolicy.StateCode
                Case "02"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm202()
                Case "03"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm203()
                Case "09"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm209()
                Case "17"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm217()
                Case "35"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm235()
                Case "42"
                    If Not oRate Is Nothing Then
                        oRate = Nothing
                    End If
                    oRate = New clsPgm242()
                Case Else
                    oRate = New clsPgm2()
            End Select


            If Not oRate Is Nothing Then
                oRate.ApplyCapFactor(oPolicy, bLogRate)
                'oRate.Rate(oPolicy, bLogRate)
            End If

            Return oPolicy

        Catch ex As Exception
            'log it
            Dim errCtx As ExceptionContext = New ExceptionContext(ex)
            errCtx.AddContext("Policy", oPolicy)
            errCtx.AddContext("oRate", oRate)
            errCtx.AddContext("bLogRate", bLogRate)
            If oPolicy IsNot Nothing Then
                errCtx.ReferenceID = oPolicy.PolicyID
                errCtx.ReferenceType = "PolicyID"
            End If
            errCtx.SourceSystem = "RatingService"
            errCtx.SystemTS = Date.Now
            errCtx.LogError()

            Dim oEx As New SoapException(ex.Message & " - " & ex.StackTrace, New System.Xml.XmlQualifiedName("string", "http://services.imperialfire.com/databaseerror"))
            Throw oEx
        Finally

            If Not oRate Is Nothing Then
                oRate = Nothing
            End If

        End Try
    End Function

    '<WebMethod(CacheDuration:=0)> _
    'Public Function CVRate(ByRef oPolicy As clsPolicyCV, ByVal bLogRate As Boolean) As clsBasePolicy
    '    Dim oRate As New clsPgm3()
    '    Return oPolicy
    'End Function

    '<WebMethod(CacheDuration:=0)> _
    'Public Function FloodRate(ByRef oPolicy As clsPolicyFlood, ByVal bLogRate As Boolean) As clsBasePolicy

    '    Dim oRate As New clsPgm4()

    '    Try

    '        'Pre Rating Stuff

    '        If Not oRate Is Nothing Then
    '            oRate.Rate(oPolicy, bLogRate, True)
    '        End If

    '        Return oPolicy

    '    Finally
    '        If Not oRate Is Nothing Then
    '            oRate = Nothing
    '        End If

    '    End Try

    'End Function

    '<WebMethod(CacheDuration:=0)> _
    'Public Function HomeOwnersRate(ByRef oPolicy As clsPolicyHomeOwner, ByVal bLogRate As Boolean) As clsBasePolicy

    '    Dim oRate As clsPgm1 = Nothing

    '    Select Case oPolicy.StateCode
    '        Case "42"
    '            If Not oRate Is Nothing Then
    '                oRate = Nothing
    '            End If
    '            oRate = New clsPgm142()
    '        Case "17"
    '            If Not oRate Is Nothing Then
    '                oRate = Nothing
    '            End If
    '            oRate = New clsPgm117()
    '        Case Else
    '            oRate = New clsPgm1()
    '    End Select

    '    Dim sMissing As String = ""

    '    Try
    '        Try
    '            oRate.MapObjects(oPolicy)
    '            'commented out by MP - Dim rulesService As New ImperialFire2.RatingRulesService2

    '            'commented out by MP - oPolicy = rulesService.HomeOwnersRules(oPolicy, bLogRate)
    '            'commented out by MP - oPolicy = rulesService.HomeOwnersEnoughToRate(oPolicy, bLogRate)
    '        Catch ex As Exception
    '            'log it
    '            Dim errCtx As CorFunctions.ExceptionContext = New CorFunctions.ExceptionContext(ex)
    '            errCtx.AddContext("Policy", oPolicy)
    '            errCtx.AddContext("oRate", oRate)
    '            errCtx.AddContext("bLogRate", bLogRate)
    '            errCtx.AddContext("sMissing", sMissing)
    '            If oPolicy IsNot Nothing Then
    '                If Not oPolicy.PolicyID Is Nothing Then
    '                    If Not oPolicy.PolicyID.Length < 8 Then
    '                        errCtx.ReferenceID = oPolicy.PolicyID
    '                        errCtx.ReferenceType = "PolicyID"
    '                    Else
    '                        errCtx.ReferenceID = oPolicy.QuoteID
    '                        errCtx.ReferenceType = "QuoteID"
    '                        log4net.Debug(oPolicy.QuoteID)

    '                    End If
    '                Else
    '                    errCtx.ReferenceID = oPolicy.QuoteID
    '                    errCtx.ReferenceType = "QuoteID"
    '                End If
    '            End If
    '            errCtx.SourceSystem = "RatingService"
    '            errCtx.SystemTS = Date.Now
    '            errCtx.LogError()
    '            Dim oEx As New SoapException(ex.Message & " - " & ex.StackTrace, New System.Xml.XmlQualifiedName("string", "http://services.imperialfire.com/databaseerror"))
    '            Throw oEx
    '        Finally

    '        End Try

    '        'Pre Rating Stuff
    '        For Each oNote As clsBaseNote In oPolicy.Notes
    '            Select Case oNote.SourceCode
    '                Case "NEI"
    '                    sMissing = "Not Enough Information to Rate"
    '            End Select
    '        Next
    '        If sMissing = "" Then
    '            If Not oRate Is Nothing Then
    '                oRate.Rate(oPolicy, bLogRate)
    '            End If
    '        Else
    '            'not enough information to rate
    '        End If

    '        Return oPolicy

    '    Catch ex As Exception
    '        'log it
    '        Dim errCtx As CorFunctions.ExceptionContext = New CorFunctions.ExceptionContext(ex)
    '        errCtx.AddContext("Policy", oPolicy)
    '        errCtx.AddContext("oRate", oRate)
    '        errCtx.AddContext("bLogRate", bLogRate)
    '        errCtx.AddContext("sMissing", sMissing)
    '        If oPolicy IsNot Nothing Then
    '            If Not oPolicy.PolicyID Is Nothing Then
    '                If Not oPolicy.PolicyID.Length < 8 Then
    '                    errCtx.ReferenceID = oPolicy.PolicyID
    '                    errCtx.ReferenceType = "PolicyID"
    '                Else
    '                    errCtx.ReferenceID = oPolicy.QuoteID
    '                    errCtx.ReferenceType = "QuoteID"
    '                End If
    '            Else
    '                errCtx.ReferenceID = oPolicy.QuoteID
    '                errCtx.ReferenceType = "QuoteID"
    '            End If
    '        End If
    '        errCtx.SourceSystem = "RatingService"
    '        errCtx.SystemTS = Date.Now
    '        errCtx.LogError()
    '        Dim oEx As New SoapException(ex.Message & " - " & ex.StackTrace, New System.Xml.XmlQualifiedName("string", "http://services.imperialfire.com/databaseerror"))

    '        Throw oEx

    '    Finally
    '        If Not oRate Is Nothing Then
    '            oRate = Nothing
    '        End If

    '    End Try

    'End Function

    '<WebMethod(CacheDuration:=0)> _
    'Public Function HomeOwnersNonPremiumEndorse(ByRef oPolicy As clsPolicyHomeOwner) As clsBasePolicy
    '    Dim oRate As clsPgm1 = Nothing

    '    Select Case oPolicy.StateCode
    '        Case "42"
    '            If Not oRate Is Nothing Then
    '                oRate = Nothing
    '            End If
    '            oRate = New clsPgm142()
    '        Case "17"
    '            If Not oRate Is Nothing Then
    '                oRate = Nothing
    '            End If
    '            oRate = New clsPgm117()
    '        Case Else
    '            oRate = New clsPgm1()
    '    End Select

    '    Dim sMissing As String = ""

    '    Try
    '        oRate.MapObjects(oPolicy)

    '    Catch ex As Exception
    '        'log it
    '        Dim errCtx As CorFunctions.ExceptionContext = New CorFunctions.ExceptionContext(ex)
    '        errCtx.AddContext("Policy", oPolicy)
    '        errCtx.AddContext("oRate", oRate)
    '        errCtx.AddContext("bLogRate", False)
    '        errCtx.AddContext("sMissing", sMissing)
    '        If oPolicy IsNot Nothing Then
    '            If Not oPolicy.PolicyID Is Nothing Then
    '                If Not oPolicy.PolicyID.Length < 8 Then
    '                    errCtx.ReferenceID = oPolicy.PolicyID
    '                    errCtx.ReferenceType = "PolicyID"
    '                Else
    '                    errCtx.ReferenceID = oPolicy.QuoteID
    '                    errCtx.ReferenceType = "QuoteID"
    '                    log4net.Debug(oPolicy.QuoteID)

    '                End If
    '            Else
    '                errCtx.ReferenceID = oPolicy.QuoteID
    '                errCtx.ReferenceType = "QuoteID"
    '            End If
    '        End If
    '        errCtx.SourceSystem = "RatingService"
    '        errCtx.SystemTS = Date.Now
    '        errCtx.LogError()
    '        Dim oEx As New SoapException(ex.Message & " - " & ex.StackTrace, New System.Xml.XmlQualifiedName("string", "http://services.imperialfire.com/databaseerror"))
    '        Throw oEx
    '    Finally
    '        If Not oRate Is Nothing Then
    '            oRate = Nothing
    '        End If
    '    End Try

    '    Return oPolicy

    'End Function

    <WebMethod()> _
    Public Function GetActualRateDate(ByVal oPolicy As CorPolicy.clsBasePolicy) As CorPolicy.clsBasePolicy

        ' Lookup to see if a rate date override exists
        Dim sSql As String = String.Empty
        Dim bOverridden As Boolean = False

        Dim cn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("ConnStr"))
        Try
            Using cmd As New SqlCommand(sSql, cn)

                sSql = " SELECT RateVersionDate FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateDateOverride"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode = @AppliesToCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                Dim dtDate As Date
                cn.Open()
                dtDate = cmd.ExecuteScalar()

                If oPolicy.ActualRateDate = Date.MinValue Then
                    oPolicy.ActualRateDate = oPolicy.RateDate
                End If

                If Not dtDate = DateTime.MinValue Then
                    bOverridden = True
                    oPolicy.RateDate = dtDate
                End If
            End Using
        Catch ex As Exception

        Finally
            cn.Close()
        End Try

        ' Look in common..raterevision table
        Try
            Using cmd As New SqlCommand(sSql, cn)

                sSql = "        SELECT TOP 1 RateRevisionID FROM Common..RateRevision RR with (nolock) "
                sSql = sSql & " INNER JOIN pgm" & oPolicy.Product & oPolicy.StateCode & "..CodeXref xRef with (nolock) on RR.PasProgramCode = xRef.MappingCode1 "
                sSql = sSql & " WHERE RateLoadDate <= @RateDate "
                sSql = sSql & "    AND TermEffDate <= @TermEffDate "
                sSql = sSql & "    AND AppliesToCode = @AppliesToCode "
                sSql = sSql & "    AND xRef.Source = 'PROGRAM' and xRef.CodeType = 'CODE' "
                sSql = sSql & "    AND xRef.Code = @Program "
                sSql = sSql & "    ORDER  BY RateRevisionID DESC "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                If oPolicy.ActualRateDate = Date.MinValue Then
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                Else
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.ActualRateDate
                End If
                cmd.Parameters.Add("@TermEffDate", SqlDbType.DateTime, 8).Value = oPolicy.EffDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                Dim dtDate As Date
                cn.Open()
                dtDate = cmd.ExecuteScalar()

                If oPolicy.ActualRateDate = Date.MinValue Then
                    oPolicy.ActualRateDate = oPolicy.RateDate
                End If

                If Not dtDate = DateTime.MinValue Then
                    bOverridden = True
                    oPolicy.RateDate = dtDate
                End If
            End Using
        Catch ex As Exception

        Finally
            cn.Close()
        End Try

        Return oPolicy
    End Function
End Class

