Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Collections.Generic
Imports System.Data.SqlClient
Imports CorPolicy
Imports RatingRulesLib

<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class RatingRulesService2
    Inherits System.Web.Services.WebService

    <WebMethod(CacheDuration:=0)> _
    Public Function HomeOwnersEnoughToRate(ByRef oPolicy As clsPolicyHomeOwner, ByVal bLogRate As Boolean) As clsPolicyHomeOwner
        Dim oRules
        Select Case oPolicy.StateCode
            Case "42"
                oRules = New clsRules142
            Case "17"
                oRules = New clsRules117
            Case Else
                oRules = New clsRules1
        End Select

        oRules.CheckNEI(oPolicy)
        Return oPolicy
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function HomeOwnersRules(ByRef oPolicy As clsPolicyHomeOwner, ByVal bLogRate As Boolean) As clsPolicyHomeOwner
        Dim oRules

        Select Case oPolicy.StateCode
            Case "42"
                oRules = New clsRules142
            Case "17"
                oRules = New clsRules117
            Case Else
                oRules = New clsRules1
        End Select


        If oPolicy.DwellingUnits.Count < 1 Then
            Throw New Exception("There are no dwelling units on this policy, rating rules will not continue")
        End If

        'If (oPolicy.CallingSystem <> "PAS" And oPolicy.CallingSystem <> "BATCH_RENEWAL") Then
        '    oRules.ResetTerritory(oPolicy)
        '    oRules.CheckIER(oPolicy)
        '    oRules.CheckUWW(oPolicy)
        '    oRules.CheckWRN(oPolicy)
        '    oRules.CheckRES(oPolicy)
        '    oRules.AddPolicyFactors(oPolicy)
        '    oRules.SetLossLevel(oPolicy)
        'Else
        '    oRules.AddPASPolicyFactors(oPolicy)
        '    oRules.SetLossLevel(oPolicy)
        '    oRules.SetIncreasedLimitFactors(oPolicy)
        '    oRules.ResetTerritory(oPolicy)
        'End If

        oRules.ResetTerritory(oPolicy)
        oRules.CheckIER(oPolicy)
        oRules.CheckUWW(oPolicy)
        oRules.CheckWRN(oPolicy)
        oRules.CheckRES(oPolicy)
        oRules.AddPolicyFactors(oPolicy)
        oRules.SetLossLevel(oPolicy)
        oRules.SetUnderwriterTier(oPolicy)
        oRules.SetCreditTier(oPolicy)
        oRules.SetIncreasedLimitFactors(oPolicy)
        oRules.AddRenewalFactors(oPolicy)

        Return oPolicy

    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function HomeOwnersSetItemsToBeFaxedIn(ByRef oPolicy As clsPolicyHomeOwner) As String
        Dim oRules
        Dim sItemsToBeFaxedIn As String = ""

        Select Case oPolicy.StateCode
            Case "42"
                oRules = New clsRules142
            Case "17"
                oRules = New clsRules117
            Case Else
                oRules = New clsRules1
        End Select


        If RequirePaper(oPolicy) Then
            sItemsToBeFaxedIn = oRules.ItemsToBeFaxedIn(oPolicy)
        End If

        Return sItemsToBeFaxedIn
    End Function

    'If the agent is Tier A, no audit flags would appear for any business they write.
    'If the agent is Tier B, audit flags would appear for any property business they write, but not auto.
    'Tier C would have audit flags for auto, but not property.
    'Tier D would have audit flags for auto and property.
    Public Function RequirePaper(ByVal oPolicy As clsBasePolicy) As Boolean
        Dim oMktCRMService As New MarketingCRMService.MarketingCRMService
        Dim dsAgencyOptions As New DataSet
        Dim bTierA As Boolean = False
        Dim bTierB As Boolean = False
        Dim bTierC As Boolean = False
        Dim bTierD As Boolean = False
        Dim bRequirePaper As Boolean = True

        dsAgencyOptions = oMktCRMService.GetOptions(oPolicy.Agency.AgencyID)
        For Each oRow As DataRow In dsAgencyOptions.Tables(0).Rows
            If oRow.Item("EditValue").ToString.ToUpper = "TIERA" Then
                bTierA = True
            End If

            If oRow.Item("EditValue").ToString.ToUpper = "TIERB" Then
                bTierB = True
            End If

            If oRow.Item("EditValue").ToString.ToUpper = "TIERC" Then
                bTierC = True
            End If

            If oRow.Item("EditValue").ToString.ToUpper = "TIERD" Then
                bTierD = True
            End If
        Next

        If bTierD Then
            ' TierD = Require All Paper
            Return True
        ElseIf bTierA Then
            ' TierA = No Paper
            Return False
        Else
            If oPolicy.Product = "2" Then
                If bTierB Then
                    Return False
                Else
                    Return True
                End If
            Else ' Program is homeowners
                If bTierC Then
                    Return False
                Else
                    Return True
                End If
            End If
        End If

        Return True
    End Function


    <WebMethod(CacheDuration:=0)> _
    Public Function PPASetItemsToBeFaxedIn(ByRef oPolicy As clsPolicyPPA) As String
        Dim oRules
        Dim sItemsToBeFaxedIn As String = ""

        Select Case oPolicy.StateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case "42"
                oRules = New clsRules242
            Case Else
                oRules = New clsRules2
        End Select

        If RequirePaper(oPolicy) Then
            sItemsToBeFaxedIn = oRules.ItemsToBeFaxedIn(oPolicy)
        End If

        Return sItemsToBeFaxedIn
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPAEnoughToRate(ByRef oPolicy As clsPolicyPPA, ByVal bLogRate As Boolean) As clsPolicyPPA
        Dim oRules
        Select Case oPolicy.StateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select

        oRules.CheckNEI(oPolicy)
        Return oPolicy
    End Function

    Public Overridable Function RemoveNotes(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sSourceCode As String) As System.Collections.Generic.List(Of clsBaseNote)

        For i As Integer = oNoteList.Count - 1 To 0 Step -1
            If oNoteList.Item(i).SourceCode.ToUpper.Trim = sSourceCode.ToUpper.Trim Then
                oNoteList.RemoveAt(i)
            End If
        Next

        Return oNoteList

    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPARules(ByRef oPolicy As clsPolicyPPA, ByVal bLogRate As Boolean) As clsPolicyPPA
        Dim oRules As clsRules2

        Select Case oPolicy.StateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select

        dbGetCreditTier(oPolicy)
        oRules.dbGetUWTier(oPolicy)

        'add SR22 viols if needed
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.SR22 Then
                'add the viol
                AddSR22Violation(oPolicy, oDrv)
            Else
                'remove the viol
                RemoveSR22Violation(oPolicy, oDrv)
            End If
        Next

        'calculate vehicle age
        oRules.CalculateVehicleAge(oPolicy, True)

        'Monthly Stuff
        If oPolicy.Program.ToUpper = "MONTHLY" Then
            'verify viol mappings
            MapMonthlyViols(oPolicy)
        End If

        dbGetPoints(oPolicy)

        RemoveNotes(oPolicy.Notes, "AAF")
        ' Notes added in Auto apply factors should use AAF note type
        oRules.AddAutoApplyFactors(oPolicy)

        ' Need to recalc points incase Foreign license violation was added
        dbGetPoints(oPolicy)

        ' Do not restrict DL for OLE, but Webrater and the renewal process needs this
        ' -- 7/17/2013: Removed OLE from the execption list
        If Not oPolicy.CallingSystem.ToUpper.Contains("UWC") And Not oPolicy.CallingSystem.Contains("PAS") Then
            oRules.CheckDLRestrictionTable(oPolicy)
        End If

        If (oPolicy.CallingSystem <> "PAS") Then
            oRules.CheckIER(oPolicy)
            oRules.CheckUWW(oPolicy)
            oRules.CheckWRN(oPolicy)
            oRules.CheckRES(oPolicy)
        Else
        End If

        'calculate vehicle age
        oRules.CalculateVehicleAge(oPolicy, False)

        Return oPolicy
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPAPolicyPoints(ByRef oPolicy As clsPolicyPPA) As clsPolicyPPA
        Dim doPointRecalc As Boolean = StateInfoContainsRateDate("ALLOW", "RECALC", "POINTS", oPolicy.Product & oPolicy.StateCode, "B", oPolicy.RateDate)
        Dim oRules As clsRules2

        Select Case oPolicy.StateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select


        If doPointRecalc Then
            'add SR22 viols if needed
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If oDrv.SR22 Then
                    'add the viol
                    AddSR22Violation(oPolicy, oDrv)
                Else
                    'remove the viol
                    RemoveSR22Violation(oPolicy, oDrv)
                End If
            Next

            dbGetPoints(oPolicy)
        End If

        oRules.AddAutoApplyFactors(oPolicy)

        If doPointRecalc Then
            ' Need to recalc points incase Foreign license violation was added
            dbGetPoints(oPolicy)
        End If

        Return oPolicy
    End Function
    <WebMethod(CacheDuration:=0)> _
    Public Function PPADriverRules(ByRef oDriver As clsEntityDriver, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal sRateDate As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote)) As System.Collections.Generic.List(Of clsBaseNote)
        Dim oRules

        Select Case sStateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select

        If (sCallingSystem.ToUpper <> "PAS") Then
            oRules.CheckIER(oDriver, sCallingSystem, sProgram, sStateCode, sRateDate, oNoteList)
        End If

        Return oNoteList
    End Function
    <WebMethod(CacheDuration:=0)> _
    Public Function PPADriverPoints(ByRef oDriver As clsEntityDriver, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal sRateDate As String) As clsEntityDriver
        Dim oRules

        Select Case sStateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select

        If (sCallingSystem.ToUpper <> "PAS") Then
            oRules.CheckViolations(oDriver, sCallingSystem, sProgram, sStateCode, sRateDate, Date.Now(), "B")
        End If

        Return oDriver
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPAVehicleRules(ByRef oVehicle As clsVehicleUnit, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal sRateDate As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote)) As System.Collections.Generic.List(Of clsBaseNote)
        Dim oRules

        Select Case sStateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select


        If (sCallingSystem.ToUpper <> "PAS") Then
            oRules.CheckIER(oVehicle, sCallingSystem, sProgram, sStateCode, sRateDate, oNoteList)
        End If

        Return oNoteList
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function PPACoverageRules(ByVal sCovCodes As String, ByVal sCovGroups As String, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal sRateDate As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote)) As System.Collections.Generic.List(Of clsBaseNote)
        Dim oRules

        Select Case sStateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select

        oRules.CheckCoverages(sCovCodes, sCovGroups, sProgram, oNoteList, sRateDate, sStateCode)
        Return oNoteList
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function AddAutoApplyFactors(ByRef oPolicy As clsPolicyPPA) As clsPolicyPPA
        Dim oRules

        Select Case oPolicy.StateCode
            Case "02"
                oRules = New clsRules202
            Case "03"
                oRules = New clsRules203
            Case "09"
                oRules = New clsRules209
            Case "42"
                oRules = New clsRules242
            Case "17"
                oRules = New clsRules217
            Case "35"
                oRules = New clsRules235
            Case Else
                oRules = New clsRules2
        End Select

        oRules.AddAutoApplyFactors(oPolicy)
        Return oPolicy
    End Function


    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadStateInfoTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue "
                sSql &= " FROM pgm" & sProduct & sStateCode & "..StateInfo with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, ItemGroup, ItemCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)
                adapter.Fill(oDS, "StateInfo")
                Return oDS
            End Using
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function
    <WebMethod(CacheDuration:=216000)> _
    Public Function CoastalDistance(ByVal zip As String) As String
        Try
            Dim weather As Weather = New Weather(zip)
            Return New LocationDistance().DistanceFromWaterway(weather.Latitude, weather.Longitude)
        Catch
            Return "Error"
        End Try
    End Function


    Public Overridable Function dbGetCreditTier(ByVal oPolicy As clsPolicyPPA) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT CreditTier FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeCreditTiers with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND MinScore <= @CreditScore "
                sSql = sSql & " AND MaxScore >= @CreditScore "
                sSql = sSql & " AND AgeStart <= @Age "
                sSql = sSql & " AND AgeEnd > @Age "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@CreditScore", SqlDbType.Int, 22).Value = oPolicy.PolicyInsured.CreditScore
                cmd.Parameters.Add("@Age", SqlDbType.Int, 22).Value = oPolicy.PolicyInsured.Age

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    sTier = oReader.Item("CreditTier")
                    oPolicy.PolicyInsured.CreditTier = sTier
                Loop
            End Using
            Return sTier
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function GetRatingRules(ByVal productCode As Integer, ByVal stateCode As String) As List(Of String)

        Return clsRules.GetRatingRules(productCode, stateCode)

    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function GetActiveRestrictions(ByVal productCode As Integer, ByVal stateCode As String) As List(Of ProgramSetting)

        Return clsRules.GetActiveRestrictions(productCode, stateCode)

    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Function GetActiveZipCountyRestrictions(ByVal productCode As Integer, ByVal stateCode As String) As List(Of StateInfo)

        Return clsRules.GetActiveZipCountyRestrictions(productCode, stateCode)

    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Sub ExpireWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal programs As List(Of ProgramSetting))

        Dim oRules As clsRules

        Select Case productCode
            Case 1
                oRules = New clsRules1
            Case 2
                oRules = New clsRules2
            Case Else
                Throw New NotSupportedException("Only Proprty and Auto Products are Supported")
        End Select

        oRules.ExpireWeatherOverride(productCode, stateCode, programs)

    End Sub

    <WebMethod(CacheDuration:=0)> _
    Public Sub ExpireActiveWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String)

        If (productCode = 2) Then
            Dim oRules As New clsRules2

            oRules.ExpireActiveWeatherOverride(productCode, stateCode)
        Else
            Throw New NotSupportedException("Only Personal Auto Products are Supported")
        End If
    End Sub

    <WebMethod(CacheDuration:=0)> _
    Public Sub ExpireWeatherOverrideByCounty(ByVal productCode As Integer, ByVal stateCode As String, ByVal stateinfo As List(Of StateInfo))

        If (productCode = 2) Then
            Dim oRules As New clsRules2

            oRules.ExpireWeatherOverrideByCounty(productCode, stateCode, stateinfo)
        Else
            Throw New NotSupportedException("Only Personal Auto Products are Supported")
        End If
    End Sub

    <WebMethod(CacheDuration:=0)> _
    Public Sub SetWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal startDate As DateTime, _
                                  ByVal programs As List(Of Integer))
        Dim oRules As clsRules

        Select Case productCode
            Case 1
                oRules = New clsRules1
            Case 2
                oRules = New clsRules2
            Case Else
                Throw New NotSupportedException("Only Proprty and Auto Products are Supported")
        End Select

        oRules.SetWeatherOverride(productCode, stateCode, startDate, programs)
    End Sub

    <WebMethod(CacheDuration:=300)> _
    Public Function GetCounties(ByVal productCode As Integer, ByVal stateCode As String) As List(Of String)
        Return clsRules.GetZipCountyMapping(productCode, stateCode).Select(Function(x) x.County).Distinct().OrderBy(Function(x) x).ToList()
    End Function

    <WebMethod(CacheDuration:=300)> _
    Public Function GetZipCodes(ByVal productCode As Integer, ByVal stateCode As String) As List(Of String)
        Return clsRules.GetZipCountyMapping(productCode, stateCode).Select(Function(x) x.ZipCode).Distinct().OrderBy(Function(x) x).ToList()
    End Function

    <WebMethod(CacheDuration:=0)> _
    Public Sub SetWeatherOverrideByCounty(ByVal productCode As Integer, ByVal stateCode As String, ByVal userID As String, _
                                  ByVal programs As List(Of Integer), ByVal counties As List(Of String))
        If (productCode = 2) Then
            Dim oRules As New clsRules2

            oRules.SetWeatherOverrideByCounty(productCode, stateCode, userID, programs, counties)
        Else
            Throw New NotSupportedException("Only Personal Auto Products are Supported")
        End If
    End Sub

    <WebMethod(CacheDuration:=0)> _
    Public Sub SetWeatherOverrideByZip(ByVal productCode As Integer, ByVal stateCode As String, ByVal userID As String, _
                                  ByVal programs As List(Of Integer), ByVal zipCodes As List(Of String))
        If (productCode = 2) Then
            Dim oRules As New clsRules2

            oRules.SetWeatherOverrideByZipCode(productCode, stateCode, userID, programs, zipCodes)
        Else
            Throw New NotSupportedException("Only Personal Auto Products are Supported")
        End If
    End Sub

    Public Function dbGetPoints(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim Rules2 As New clsRules2

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Rules2.CheckViolations(oDrv, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, oPolicy.AppliesToCode)
        Next
    End Function

    Public Function GetOccurrence(ByVal Violations As List(Of clsBaseViolation), ByVal sViolGroup As String, ByVal iViolNum As Integer, ByVal iMinAgeViol As Integer, ByVal iMaxAgeViol As Integer, ByVal iMonthsOld As Integer, ByVal dtEffDate As String, ByVal bUseViolationAgeLogic As Boolean) As Integer

        Dim iOccurrence As Integer = 0
        For i As Integer = 0 To iViolNum
            If Violations.Item(i).ViolGroup.ToUpper = sViolGroup.ToUpper Then
                Dim iViolAge = CalculateViolAge(Violations.Item(i).ViolDate, dtEffDate)

                ' OLE Lets you run mvr for a new driver
                ' those could be today's date, yet calcviolage would return a negative
                ' since we are basing off of the policy eff date
                If iViolAge < 0 Then
                    iViolAge = 0
                End If
                If (iMinAgeViol <= iMonthsOld And iMaxAgeViol > iMonthsOld) And (Not bUseViolationAgeLogic Or (iMinAgeViol <= iViolAge And iMaxAgeViol > iViolAge)) Then
                    iOccurrence += 1
                End If
            End If
        Next
        Return iOccurrence
    End Function

    Public Sub MapMonthlyViols(ByVal oPolicy As clsPolicyPPA)
        Dim bConvertMonthly As Boolean = True
        If StateInfoContains("NOCONVERT", "MONTHLY", "VIOLATION", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
            bConvertMonthly = False
        End If

        If bConvertMonthly Then
            Dim DataRows() As DataRow
            Dim oCodeViolCodesTable As DataTable = Nothing
            Dim oCodeXRefTable As DataTable = Nothing
            Dim oCodeXRefDataSet As DataSet = LoadCodeXRefTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

            oCodeXRefTable = oCodeXRefDataSet.Tables(0)

            'get viol code mappings
            DataRows = oCodeXRefTable.Select("Source='WEBRATER' AND CodeType='VIOLMAP'")

            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                For Each oViol As clsBaseViolation In oDrv.Violations
                    For Each oRow As DataRow In DataRows
                        If oViol.ViolTypeCode = oRow("Code").ToString.Trim Then
                            'we found it now reset the viol
                            oViol.ViolGroup = oRow("MappingCode3").ToString.Trim
                            oViol.ViolTypeCode = oRow("MappingCode2").ToString.Trim
                            Exit For
                        End If
                    Next
                Next
            Next
        End If
    End Sub

    Public Shared Function SelectDistinct(ByVal SourceTable As DataTable, ByVal FieldName As String) As DataTable
        Dim lastValue As New Object
        Dim newTable As DataTable

        If FieldName Is Nothing OrElse FieldName.Length = 0 Then
            Throw New ArgumentNullException("FieldNames")
        End If

        newTable = New DataTable
        newTable.Columns.Add(FieldName, SourceTable.Columns(FieldName).DataType)

        For Each Row As DataRow In SourceTable.Select("", FieldName)
            If Not lastValue.Equals(Row(FieldName)) Then

                newTable.Rows.Add(Row(FieldName))

                lastValue = Row(FieldName)
            End If
        Next
        Return newTable
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadCodeViolGroupsTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()
        Dim oDS As New DataSet

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Program, ViolGroup, FirstOccurrence, SecondOccurrence, AddlOccurrence, MinAgeViol, MaxAgeViol"
                sSql &= " FROM pgm" & sProduct & sStateCode & "..CodeViolGroups with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, ViolGroup "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "CodeViolGroups")
                Return oDS
            End Using
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Shared Function LoadViolCodeGroup(ByVal ViolCode As String, ByVal Program As String, ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As String
        Dim sSql As String = ""
        Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT ViolGroup "
                sSql &= " FROM pgm" & sProduct & sStateCode & "..CodeViolCodes with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " AND ViolCode = @ViolCode"
                sSql &= " AND Program = @Program"

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.AddWithValue("@ViolCode", ViolCode)
                cmd.Parameters.AddWithValue("@Program", Program)
                cmd.Parameters.AddWithValue("@RateDate", dtRateDate)
                cmd.Parameters.AddWithValue("@AppliesToCode", sAppliesToCode)

                oConn.Open()
                Dim group As String = cmd.ExecuteScalar()
                oConn.Close()

                Return group
            End Using
        Catch ex As Exception
            Throw New Exception("Loading Viol Codes/Groups Failed:", ex)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function


    Public Overloads Sub AddSR22Violation(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver)

        Dim oViolation As clsBaseViolation = GetViol("S22", "59998", oDriver)

        ' Monthly converts to a different code/group, need to lookup for LA Monthly
        If oViolation Is Nothing And (oPolicy.StateCode = "17" And oPolicy.Program.ToUpper = "MONTHLY") Then
            oViolation = GetViol("XX2", "59998", oDriver)   ' Violation 26:XX2 did not exist, change to 59998.
        End If

        If oViolation Is Nothing Then
            oViolation = New clsBaseViolation
            oViolation.ViolTypeCode = "59998"
            oViolation.ViolDesc = "SR-22 FILING"
            oViolation.ViolTypeIndicator = "V"
            oViolation.ViolGroup = "S22"
            oViolation.ViolSourceCode = "M"
            oViolation.AtFault = False
            oViolation.ViolDate = oDriver.SR22Date
            oViolation.ConvictionDate = oDriver.SR22Date
            oViolation.Chargeable = True
            oViolation.IndexNum = oDriver.Violations.Count + 1
            oViolation.AddToXML = True

            If oPolicy.StateCode <> "09" Then
                oDriver.Violations.Add(oViolation)
            End If
        End If
    End Sub

    Public Overloads Sub RemoveSR22Violation(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver)

        For Each oViol As clsBaseViolation In oDriver.Violations
            If oViol.ViolGroup = "S22" Then
                oDriver.Violations.Remove(oViol)
                Exit For
            End If
        Next
    End Sub

    Public Function GetViol(ByVal sViolGroup As String, ByVal sViolTypeCode As String, ByVal oDriver As clsEntityDriver) As clsBaseViolation

        Dim oReturnedViol As clsBaseViolation = Nothing
        For Each oViol As clsBaseViolation In oDriver.Violations
            If oViol.ViolGroup.ToUpper = sViolGroup.ToUpper And oViol.ViolTypeCode.ToUpper = sViolTypeCode.ToUpper Then
                oReturnedViol = oViol
                Exit For
            End If
        Next
        Return oReturnedViol
    End Function

    Public Function CalculateViolAge(ByVal dtViolDate As Date, ByVal dtEffDate As Date) As Integer

        Dim dViolAge As Double = 0
        dViolAge = (DateDiff("m", dtViolDate, dtEffDate))
        If (DatePart("d", dtViolDate) > DatePart("d", dtEffDate)) Then
            dViolAge = dViolAge - 1
        End If
        If dViolAge < 0 Then
            dViolAge = dViolAge + 1
        End If

        Return CInt(dViolAge)
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadCodeXRefTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""
        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Source, CodeType, Code, MappingCode1, MappingCode2, MappingCode3 FROM pgm" & sProduct & sStateCode & ".." & "CodeXRef with(nolock)"
                sSql &= " ORDER BY Source, CodeType, Code "

                'Execute the query
                cmd.CommandText = sSql

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)
                adapter.Fill(oDS, "CodeXRef")
                Return oDS
            End Using
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Shared Function HasForeignLicense(ByVal oDriver As clsEntityDriver) As Boolean

        Select Case oDriver.DLNState
            Case "FN", "IT", "VI", "AS", "FM", "GU", "MH", "MP", "PR", "PW", "ON", "AE", "AP", "AA", "JZ"
                Return True
            Case Else
                Return False
        End Select
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function testHW146(ByVal Lat As Double, ByVal Lon As Double) As String

        'Return LocationDistance.EastWestOfHW146(Lat, Lon)
        Dim rules As New clsRules142()
        Dim dummyPolicy As New clsPolicyHomeOwner
        dummyPolicy.StateCode = "42"
        dummyPolicy.Product = "1"
        dummyPolicy.AppliesToCode = "B"
        dummyPolicy.RateDate = Now()

        dummyPolicy.DwellingUnits.Add(New clsDwellingUnit)
        dummyPolicy.DwellingUnits(0).Latitude = Lat
        dummyPolicy.DwellingUnits(0).Longitude = Lon
        If AllowCode("CheckHW146") Then
            rules.CheckHW146(dummyPolicy)
            Return dummyPolicy.DwellingUnits(0).Region
        Else
            Return ""
        End If
    End Function

    Public Shared Function AllowCode(ByVal FunctionOrCodeName As String) As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet
        Dim allow As Boolean = False

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT ItemValue "
                sSql = sSql & " FROM Common..StateInfo with(nolock)"
                sSql = sSql & " WHERE EffDate <= @Date "
                sSql = sSql & " AND ExpDate > @Date "
                sSql = sSql & " AND AppliesToCode IN ('B') "
                sSql = sSql & " AND ItemGroup = 'ALLOW' "
                sSql = sSql & " AND ItemCode = 'CODE' "
                sSql = sSql & " AND ItemSubCode = @FunctionName "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = Now()
                cmd.Parameters.Add("@FunctionName", SqlDbType.VarChar, 50).Value = FunctionOrCodeName

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "StateInfo")

                If oDS.Tables(0).Rows.Count > 0 Then
                    allow = True
                End If
            End Using

        Catch ex As Exception
            Throw New Exception("AllowCode(" & FunctionOrCodeName & ") failed: " & ex.Message, ex)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
        Return allow
    End Function

    Public Shared Function StateInfoContainsRateDate(ByVal group As String, ByVal code As String, ByVal subcode As String, ByVal program As String, ByVal AppliesToCode As String, ByVal dtRateDate As Date, Optional ByVal sProgram As String = "") As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet
        Dim allow As Boolean = False

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT ItemValue "
                sSql = sSql & " FROM pgm" & program & "..StateInfo with(nolock)"
                sSql = sSql & " WHERE EffDate <= @Date "
                sSql = sSql & " AND ExpDate > @Date "
                sSql = sSql & " AND AppliesToCode IN ('B',@AppliesToCode) "
                sSql = sSql & " AND ItemGroup = @Group "
                sSql = sSql & " AND ItemCode = @Code "
                sSql = sSql & " AND ItemSubCode = @SubCode"
                If Len(sProgram) > 0 Then
                    sSql = sSql & " AND Program IN ('ALL', @Program )"
                End If

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@Group", SqlDbType.VarChar, 50).Value = group
                cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code
                cmd.Parameters.Add("@SubCode", SqlDbType.VarChar, 50).Value = subcode
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = AppliesToCode
                If Len(sProgram) > 0 Then
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = sProgram
                End If

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "StateInfo")

                If oDS.Tables(0).Rows.Count > 0 Then
                    allow = True
                End If
            End Using

        Catch ex As Exception
            Throw New Exception("StateInfoContains(" & group & ", " & code & ", " & subcode & ", " & program & ", " & AppliesToCode & ") failed: " & ex.Message, ex)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
        Return allow
    End Function


    Public Shared Function StateInfoContains(ByVal group As String, ByVal code As String, ByVal subcode As String, ByVal program As String, ByVal AppliesToCode As String, Optional ByVal sProgram As String = "") As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet
        Dim allow As Boolean = False

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT ItemValue "
                sSql = sSql & " FROM pgm" & program & "..StateInfo with(nolock)"
                sSql = sSql & " WHERE EffDate <= @Date "
                sSql = sSql & " AND ExpDate > @Date "
                sSql = sSql & " AND AppliesToCode IN ('B',@AppliesToCode) "
                sSql = sSql & " AND ItemGroup = @Group "
                sSql = sSql & " AND ItemCode = @Code "
                sSql = sSql & " AND ItemSubCode = @SubCode"
                If Len(sProgram) > 0 Then
                    sSql = sSql & " AND Program IN ('ALL', @Program )"
                End If

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = Now()
                cmd.Parameters.Add("@Group", SqlDbType.VarChar, 50).Value = group
                cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code
                cmd.Parameters.Add("@SubCode", SqlDbType.VarChar, 50).Value = subcode
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = AppliesToCode
                If Len(sProgram) > 0 Then
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = sProgram
                End If

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "StateInfo")

                If oDS.Tables(0).Rows.Count > 0 Then
                    allow = True
                End If
            End Using

        Catch ex As Exception
            Throw New Exception("StateInfoContains(" & group & ", " & code & ", " & subcode & ", " & program & ", " & AppliesToCode & ") failed: " & ex.Message, ex)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
        Return allow
    End Function
End Class