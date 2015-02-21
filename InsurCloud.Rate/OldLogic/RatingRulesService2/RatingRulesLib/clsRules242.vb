'Imports System.Web.UI.WebControls.Expressions
Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class clsRules242
    Inherits clsRules2

    Public Overrides Function ApplyTemporaryRulesOverride(ByVal ratingRules As DataTable, ByVal policy As clsBasePolicy) As DataTable

        If policy.CallingSystem <> "RENEWAL_SVC" AndAlso (ratingRules.Rows.OfType(Of DataRow)().Any(Function(a) a("FunctionName") = "CheckNonOwnerNotAllowed")) Then
            Dim rows As List(Of DataRow) = ratingRules.Rows.OfType(Of DataRow)().Where(Function(a) a("FunctionName") = "CheckNonOwner").ToList()
            Dim index As Integer = 0
            While index < rows.Count
                ratingRules.Rows.Remove(rows(index))
                index = index + 1
            End While
        End If

        Return ratingRules
    End Function

    Public Overrides Sub CheckNonOwner(ByVal oPolicy As clsPolicyPPA)
        Dim bIsNonOwner As Boolean

        bIsNonOwner = False

        ' Check to see if this is a nonowner policy
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If oVehicle.VinNo.ToUpper = "NONOWNER" And Not oVehicle.IsMarkedForDelete Then
                bIsNonOwner = True
                Exit For
            End If
        Next

        If bIsNonOwner Then
            ' If this is a non-owner policy remove all extra vehicles
            For i As Integer = oPolicy.VehicleUnits.Count - 1 To 0 Step -1
                If Not oPolicy.VehicleUnits(i).IsMarkedForDelete Then
                    If oPolicy.VehicleUnits(i).VinNo.ToUpper <> "NONOWNER" Then
                        oPolicy.VehicleUnits.Remove(oPolicy.VehicleUnits(i))
                        'Else
                        '	oPolicy.VehicleUnits(i).IndexNum = 1
                        ' removed 11/12/2010 causing issues with endorsements
                    End If
                End If
            Next

            ' If this is a non-owner policy, restrict to rated driver and spouse only
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If Not oDrv.IsMarkedForDelete Then
                    If oDrv.RelationToInsured.ToUpper <> "SELF" Then
                        If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                            If oDrv.RelationToInsured.ToUpper = "SPOUSE" And oDrv.MaritalStatus.ToUpper = "MARRIED" Then
                                If Not SpouseAllowed(oPolicy) Then
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Only one named insured is allowed on a Non-owner policy", "NonOwner", "IER", oPolicy.Notes.Count))
                                    Exit For
                                End If
                            Else
                                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Only one named insured and spouse (if applicable) are allowed on a Non-owner policy", "NonOwner", "IER", oPolicy.Notes.Count))
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next

            ' Per Stacey, this rule does not apply to TX
            '' if this is a non owner, must be SR22
            'For Each oDrv As clsEntityDriver In oPolicy.Drivers
            '	If Not oDrv.IsMarkedForDelete Then
            '		If oDrv.RelationToInsured.ToUpper = "SELF" Then
            '			If Not oDrv.SR22 Then
            '				oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-Ownership liability coverage is available only when an SR22 is required.", "NonOwnrS22", "IER", oPolicy.Notes.Count))
            '			End If
            '			Exit For
            '		End If
            '	End If
            'Next

            ' if this is a non-owner, only allow minmum limits
            Dim bNonMinimumCoverage As Boolean = False
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete Then
                    For Each oCov As clsBaseCoverage In oVeh.Coverages
                        If Not oCov.IsMarkedForDelete Then
                            If oCov.CovGroup <> "BI" And oCov.CovGroup <> "PD" Then
                                bNonMinimumCoverage = True
                            End If

                            If oCov.CovGroup = "BI" Then
                                Dim sMinimumBICovCode As String = ""
                                sMinimumBICovCode = LookupMinimumCoverageLimit(oPolicy, "BI")
                                If oCov.CovCode <> sMinimumBICovCode Then
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-Ownership policies are only permitted state minimum BI limits.", "NonOwnrBI", "IER", oPolicy.Notes.Count))
                                End If
                            End If

                            If oCov.CovGroup = "PD" Then
                                Dim sMinimumPDCovCode As String = ""
                                sMinimumPDCovCode = LookupMinimumCoverageLimit(oPolicy, "PD")
                                If oCov.CovCode <> sMinimumPDCovCode Then
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-Ownership policies are only permitted state minimum PD limits.", "NonOwnrPD", "IER", oPolicy.Notes.Count))
                                End If
                            End If
                        End If
                    Next
                End If
            Next

            ' Per Stacey, this rule does not apply to TX
            'If bNonMinimumCoverage Then
            '    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-Ownership policies are only permitted to have liability coverage.", "NonOwnrPD", "IER", oPolicy.Notes.Count))
            'End If

        End If
    End Sub

    Public Sub CheckNonOwnerNotAllowed(ByVal oPolicy As clsPolicyPPA)

        
        Dim bIsNonOwner As Boolean
        Dim iVehicleCount As Integer = 0
        bIsNonOwner = False

        ' Check to see if this is a nonowner policy
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If oVehicle.VinNo.ToUpper = "NONOWNER" And Not oVehicle.IsMarkedForDelete Then
                bIsNonOwner = True
                Exit For
            End If

            If Not oVehicle.IsMarkedForDelete Then
                iVehicleCount += 1
            End If
        Next

        If bIsNonOwner Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-owners risks are not acceptible", "NonOwner", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Sub CheckMinimumAge(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        Dim iMinDriverAge As Integer = GetStateInfoValue(oPolicy, oPolicy.Program, "MINIMUM", "AGE", "")
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                    If oDrv.Age < iMinDriverAge Then
                        If sDriverList = "" Then
                            sDriverList = oDrv.IndexNum
                        Else
                            sDriverList &= ", " & oDrv.IndexNum
                        End If
                    End If
                End If
            End If
        Next
        If sDriverList <> "" Then
            If oPolicy.Program.ToUpper = "DIRECT" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state licensing and must be Excluded from coverage or listed with a Learners Permit. - " & sDriverList & ".", "MinDriverAge", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state licensing - " & sDriverList & ".", "MinDriverAge", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub
    Public Sub CheckMinimumPermitAge(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        Dim iMinDriverPermitAge As Integer = GetStateInfoValue(oPolicy, oPolicy.Program, "MINIMUM", "PERMITAGE", "")
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.DriverStatus.ToUpper = "PERMITTED" Or oDrv.DriverStatus.ToUpper = "EXCLUDED" Then
                    If oDrv.Age < iMinDriverPermitAge Then
                        If sDriverList = "" Then
                            sDriverList = oDrv.IndexNum
                        Else
                            sDriverList &= ", " & oDrv.IndexNum
                        End If
                    End If
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state permit - " & sDriverList & ".", "MinDriverPermitAge", "IER", oPolicy.Notes.Count))
        End If
    End Sub
    Public Sub CheckDWICountUnder21(ByRef oPolicy As clsPolicyPPA)
        ' No DWI,DUI,alcohol, drug, or controlled substance violations
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    Dim age21DOB As Date
                    ' Find the date the driver turned/turns 21
                    age21DOB = DateAdd(DateInterval.Year, 21, oDriver.DOB)

                    Dim iDWI As Integer = 0
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        If oViolation.ViolGroup = "DWI" Then
                            If oViolation.ViolDate < age21DOB Then
                                iDWI += 1
                            End If
                        End If
                    Next
                    If iDWI > 0 Then
                        sDrv = oDriver.IndexNum
                    End If
                End If

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            End If
        Next
        If sDriverList <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have drug or alcohol violations prior to the age of 21 - " & sDriverList & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckDWICount(ByRef oPolicy As clsPolicyPPA)
        ' No more than 2 DWI,DUI,alcohol, drug, or controlled substance violations within the previous 35 months
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    Dim iDWI As Integer = 0
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        If oViolation.ViolGroup = "DWI" Then
                            If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
                                iDWI += 1
                            End If
                        End If
                    Next
                    If iDWI > 2 Then
                        sDrv = oDriver.IndexNum
                    End If
                End If

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            End If
        Next
        If sDriverList <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than 2 drug or alcohol violations - " & sDriverList & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckArtistanUse(ByRef oPolicy As clsPolicyPPA)
        Dim iCounter As Integer = 0
        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVeh, oPolicy) Then
                If oVeh.TypeOfUseCode.ToUpper = "ARTISAN" Or oVeh.TypeOfUseCode.ToUpper = "BUSINESS" Or oVeh.TypeOfUseCode.ToUpper = "ART" Then
                    iCounter += 1
                End If
            End If
        Next
        If oPolicy.Program.ToUpper = "DIRECT" Then
            If iCounter > 0 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with Business Use require company approval.", "ArtisanLimit", "IER", oPolicy.Notes.Count))
            End If
        Else
            If iCounter > 1 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Policies may have no more than one (1) business use vehicle.", "ArtisanLimit", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub


    Public Overrides Sub CheckPolicyPoints(ByVal oPolicy As clsPolicyPPA)
        ' 6/13/2011 2 point restriction removed for TX
    End Sub

    Public Overridable Sub CheckDriverPointsClassic(ByRef oPolicy As clsPolicyPPA)
        Dim iMaxPoints As Integer = GetProgramSetting("MaxDriverPoints")

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sResult As String = ""
                sResult = CheckDriverPointsClassic(oDrv)
                If Len(sResult) > 0 Then
                    If oPolicy.Program.ToUpper = "DIRECT" Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Driver " & sResult & " has more than " & iMaxPoints & " violation points", "DriverPoints", "IER", oPolicy.Notes.Count))
                    Else
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Driver " & sResult & " has more than " & iMaxPoints & " violation points", "DriverPoints", "UWW", oPolicy.Notes.Count))
                    End If
                End If
            End If
        Next
    End Sub

    Public Overridable Function CheckDriverPointsClassic(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String

        Dim iMaxPoints As Integer = GetProgramSetting("MaxDriverPoints")

        If oDrv.Points > iMaxPoints Then
            If oNoteList Is Nothing Then
                Return oDrv.IndexNum
            Else
                oNoteList = (AddNote(oNoteList, "Underwriting Approval Needed: Driver " & oDrv.IndexNum & " has more than " & iMaxPoints & " violation points", "DriverPoints", "UWW", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End If

        Return ""
    End Function


    Public Overridable Sub CheckDriverViolationsClassic(ByRef oPolicy As clsPolicyPPA)
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sResult As String = ""
                sResult = CheckDriverViolationsClassic(oDrv)
                If Len(sResult) > 0 Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Driver " & sResult & " has more than 6 violations", "DriverViolCount", "UWW", oPolicy.Notes.Count))
                End If
            End If
        Next
    End Sub

    Public Overridable Function CheckDriverViolationsClassic(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim iViolCount As Integer = 0

        For Each oViol As clsBaseViolation In oDrv.Violations
            If oViol.Chargeable Then
                iViolCount = iViolCount + 1
            End If
        Next

        If iViolCount > 6 Then
            If oNoteList Is Nothing Then
                Return oDrv.IndexNum
            Else
                oNoteList = (AddNote(oNoteList, "Underwriting Approval Needed: Driver " & oDrv.IndexNum & " has more than 6 violations", "DriverViolCount", "UWW", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End If

        Return ""
    End Function


    Public Overridable Sub CheckTotalPoints(ByRef oPolicy As clsPolicyPPA)
        Dim iTotalPoints As Integer = 0
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                iTotalPoints = iTotalPoints + oDrv.Points
            End If
        Next

        If iTotalPoints > 17 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Maxmium of 18 violation points is allowed for all drivers", "DriverPtTot", "IER", oPolicy.Notes.Count))
        End If
    End Sub


    Private Function LookupMinimumCoverageLimit(ByVal oPolicy As clsPolicyPPA, ByVal sCovGroup As String) As String
        Dim sCovLimit As String = String.Empty

        Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        Dim DataRows() As DataRow
        DataRows = oStateInfoDataSet.Tables(0).Select("ItemGroup = 'MINIMUM' AND ItemCode = 'LIMIT' AND ItemSubCode = '" & sCovGroup & "'")

        For Each oRow As DataRow In DataRows
            sCovLimit = oRow.Item("ItemValue").ToString
        Next

        Return sCovLimit
    End Function

    Public Overrides Function dbGetUWTier(ByVal oPolicy As clsPolicyPPA) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""
        Dim grandfatheredRenewalDate As Date = CDate(GetStateInfoValue(oPolicy, oPolicy.Program, "UWTIER", "TIER_94_REVISION", "GRANDFATHERED_RENEWAL_DATE"))
        Dim uwTierRevisionDate As Date = CDate(GetStateInfoValue(oPolicy, oPolicy.Program, "UWTIER", "TIER_94_REVISION", "REVISION_DATE"))
        Dim origTermUWTierValues As OriginalTermUWTierValues = Nothing
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        If oPolicy.PolicyInsured.PriorLimitsCode = "0" And oPolicy.PriorCarrierName.ToUpper.Contains("IMPERIAL") Then
            oPolicy.PolicyInsured.PriorLimitsCode = "1"

            ' if this is a renewal of an imperial policy, make sure the priorexpdate isn't a null value
            If oPolicy.Type.ToUpper = "RENEWAL" Then
                If oPolicy.PolicyInsured.PriorExpDate = Date.MinValue Then
                    oPolicy.PolicyInsured.PriorExpDate = oPolicy.EffDate
                End If
            End If
        End If

        ' Renewal of Policy uploaded after May 2014 revision and if program is not Summit
        ' Load the values form the Original Term, to keep the same tier it was uploaded with
        If oPolicy.Type.ToUpper = "RENEWAL" And DateDiff(DateInterval.Day, grandfatheredRenewalDate, oPolicy.OrigTermEffDate) >= 0 And oPolicy.Program.ToUpper.Trim <> "SUMMIT" Then

            origTermUWTierValues = GetOriginalTermUWTierValues(oPolicy.PolicyID, oPolicy.OrigTermEffDate, oPolicy.Product, oPolicy.StateCode, oPolicy.ProgramCode)
            If origTermUWTierValues IsNot Nothing Then
                oPolicy.PolicyInsured.PriorLimitsCode = origTermUWTierValues.PriorLimitsCode
            End If

        End If

        Try
            If oPolicy.PolicyInsured.PriorLimitsCode = "0" Then
                oPolicy.PolicyInsured.DaysLapse = 0
            Else
                If oPolicy.Program.ToUpper.Trim = "SUMMIT" And oPolicy.Type.ToUpper = "RENEWAL" Then
                    Select Case oPolicy.EffDate.Subtract(oPolicy.PolicyInsured.PriorExpDate).Days
                        Case Is <= 7
                            oPolicy.PolicyInsured.DaysLapse = 2
                        Case 8 To 30
                            oPolicy.PolicyInsured.DaysLapse = 1
                        Case Else
                            oPolicy.PolicyInsured.DaysLapse = 0
                            oPolicy.PolicyInsured.PriorLimitsCode = "0"
                    End Select
                Else
                    If oPolicy.Type.ToUpper = "RENEWAL" And DateDiff(DateInterval.Day, grandfatheredRenewalDate, oPolicy.OrigTermEffDate) >= 0 Then
                        ' Renewal of policy created after May 2014 revision
                        ' Use Original Term Days Lapse to keep the same tier it was uploaded with
                        If origTermUWTierValues IsNot Nothing Then
                            oPolicy.PolicyInsured.DaysLapse = GetDaysLapseCode(origTermUWTierValues.DaysLapse)
                        End If
                    Else
                        oPolicy.PolicyInsured.DaysLapse = GetDaysLapseCode(oPolicy.EffDate.Subtract(oPolicy.PolicyInsured.PriorExpDate).Days)
                        If oPolicy.PolicyInsured.DaysLapse = 0 Then
                            oPolicy.PolicyInsured.PriorLimitsCode = "0"
                        End If
                    End If
                End If
            End If

            ' Monthly does not qualify for the prior coverage discount
            If oPolicy.PolicyInsured.DaysLapse > 0 Then
                If oPolicy.PriorCarrierName.ToUpper = "IMPERIAL MONTHLY" Then
                    oPolicy.PolicyInsured.PriorLimitsCode = 0
                End If
            End If

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Tier FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeUWTiers with(nolock)"
                sSql &= " WHERE Program = @Program "
                sSql &= " AND EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " AND PriorInsurance = @PriorInsurance "
                sSql &= " AND PriorLimits = @PriorLimits "
                sSql &= " AND ContCov IN ( @ContCov , 99 ) "

                sSql &= " ORDER BY Tier Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@PriorInsurance", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.DaysLapse
                cmd.Parameters.Add("@PriorLimits", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.PriorLimitsCode

                '' If it is a renewal, set monthspriorcontcov = 6 since they had to have prior coverage
                'If Not oPolicy.PolicyTermTypeInd Is Nothing Then
                '    If oPolicy.PolicyTermTypeInd.ToUpper.Trim = "R" Then
                '        If oPolicy.PolicyInsured.MonthsPriorContCov < 1 Then
                '            oPolicy.PolicyInsured.MonthsPriorContCov = 6
                '        End If
                '    End If
                'End If

                'cmd.Parameters.Add("@ContCov", SqlDbType.Int, 22).Value = IIf(oPolicy.PolicyInsured.MonthsPriorContCov >= 6, 1, 0)

                ' Set the Continuous Coverage value
                Dim continuousCoverage As Integer = 0


                If oPolicy.Type.ToUpper = "RENEWAL" Then
                    ' If this is Classic or Direct, and after the January 2014 revision, use the new logic
                    If ((oPolicy.Program.ToUpper.Trim = "CLASSIC" Or oPolicy.Program.ToUpper.Trim = "DIRECT") And (DateDiff(DateInterval.Day, oPolicy.RateDate, uwTierRevisionDate) <= 0)) Then
                        ' Renewal after May 2014 revision. Is it a grandfathered renewal?
                        If DateDiff(DateInterval.Day, grandfatheredRenewalDate, oPolicy.OrigTermEffDate) < 0 Then
                            ' Grandfathered Renewal
                            continuousCoverage = 2
                        Else
                            If origTermUWTierValues IsNot Nothing Then
                                If origTermUWTierValues.MonthsPriorContCov >= 6 Then
                                    continuousCoverage = 1
                                End If
                            End If
                        End If
                    Else
                        ' Renewal prior to May 2014 revision. Use existing renewal logic
                        If oPolicy.PolicyInsured.MonthsPriorContCov < 1 Or oPolicy.PolicyInsured.MonthsPriorContCov >= 6 Then
                            oPolicy.PolicyInsured.MonthsPriorContCov = 6
                            continuousCoverage = 1
                        End If
                    End If
                Else
                    ' New Business
                    If oPolicy.PolicyInsured.MonthsPriorContCov >= 6 Then
                        continuousCoverage = 1
                    End If
                End If

                'cmd.Parameters.Add("@ContCov", SqlDbType.Int, 22).Value = IIf(oPolicy.PolicyInsured.MonthsPriorContCov >= 6, 1, 0)
                cmd.Parameters.Add("@ContCov", SqlDbType.Int, 22).Value = continuousCoverage


                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    sTier = oReader.Item("Tier")
                    'just get the first one since there could be multiple tiers returned
                    oPolicy.PolicyInsured.UWTier = sTier
                    oPolicy.UWTier = sTier
                    Exit Do
                Loop
            End Using

            ' -- added to treat TXI renewal policies as new policies
            ' emulating logic from PasRating..[SetUWTier]
            If Left(oPolicy.PolicyID, 3) = "TXI" Then
                sTier = 7
                oPolicy.PolicyInsured.UWTier = 7
                oPolicy.UWTier = 7
            End If

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

    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim parent As New clsRules2

        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""

        Try
            If parent.CheckNEI(oPolicy) Then

            Else
                bEnoughInfoToRate = False
            End If

            Return bEnoughInfoToRate
        Catch ex As Exception
            oPolicy.Notes = (AddNote(oPolicy.Notes, ex.Message & "Needs: " & sMissing & " - " & ex.StackTrace, "Not Enough Information To Rate", "NEI", oPolicy.Notes.Count))
            Return False
        End Try
    End Function

    Public Overrides Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyPPA) As String

        Dim sItemsToBeFaxedIn As String = ""
        sItemsToBeFaxedIn &= MyBase.ItemsToBeFaxedIn(oPolicy)


        'companion
        If oPolicy.CompanionHOMCarrierName.Trim <> "" And (Not IsRewritePolicy(oPolicy)) Then
            sItemsToBeFaxedIn &= "Copy of Companion Homeowner Insurance Policy" & vbNewLine
        End If
        If Not IsRewritePolicy(oPolicy) Then
            'proof of prior
            If oPolicy.PolicyInsured.DaysLapse > 0 Then
                sItemsToBeFaxedIn &= "Proof of Prior Coverage" & vbNewLine
            End If

            'homeowners
            If oPolicy.Program.ToUpper = "CLASSIC" Then
                If oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER" Or oPolicy.PolicyInsured.OccupancyType.ToUpper = "MOBILEHOMEOWNER" Then
                    sItemsToBeFaxedIn &= "Proof of Home/Mobile Home Ownership" & vbNewLine
                End If
            ElseIf oPolicy.Program.ToUpper = "SUMMIT" Then
                If oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER" Then
                    sItemsToBeFaxedIn &= "Proof of Home Ownership" & vbNewLine
                End If
            End If
        End If

        Return sItemsToBeFaxedIn

    End Function

    Public Overrides Sub AddAutoApplyFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Call MyBase.AddAutoApplyFactors(oPolicy)

            ' summit doesn't get a discount for mobilehomeowner
            If oPolicy.Program.ToUpper = "SUMMIT" And oPolicy.PolicyInsured.OccupancyType.ToUpper = "MOBILEHOMEOWNER" Then
                For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                    If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = "HOMEOWNER" Then
                        oPolicy.PolicyFactors.RemoveAt(i)
                        Exit For
                    End If
                Next
            End If

            Dim FactorPolicyDataRows() As DataRow
            Dim oFactorPolicyTable As DataTable = Nothing
            Dim oFactorPolicyDataSet As DataSet = LoadFactorPolicyTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            oFactorPolicyTable = oFactorPolicyDataSet.Tables(0)

            Dim iNumOfActiveDrivers As Integer = 0
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If Not oDrv.IsMarkedForDelete Then
                    If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                        iNumOfActiveDrivers += 1
                    End If
                End If
            Next

            'add policy auto apply factors
            FactorPolicyDataRows = oFactorPolicyTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND AutoApply = 1 ")
            For Each oRow As DataRow In FactorPolicyDataRows 'AutoApply factors on Factor Policy table
                'all auto policy auto apply factors (6_TERM, ADV_QUOTE, EFT_DISC, NO_VIOL, etc)
                Select Case oRow.Item("FactorCode").ToString.ToUpper

                    Case "DVR_1"
                        If oPolicy.DriverCount(True) > 0 And oPolicy.VehicleCount(True) > 0 Then
                            Dim dRatio As Decimal = iNumOfActiveDrivers / oPolicy.VehicleCount(True)
                            If dRatio < 0.5 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "DVR_2"
                        If oPolicy.DriverCount(True) > 0 And oPolicy.VehicleCount(True) > 0 Then
                            Dim dRatio As Decimal = iNumOfActiveDrivers / oPolicy.VehicleCount(True)
                            If dRatio >= 0.5 And dRatio < 1 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "DVR_3"
                        If oPolicy.DriverCount(True) > 0 And oPolicy.VehicleCount(True) > 0 Then
                            Dim dRatio As Decimal = iNumOfActiveDrivers / oPolicy.VehicleCount(True)
                            If dRatio = 1 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "DVR_4"
                        If oPolicy.DriverCount(True) > 0 And oPolicy.VehicleCount(True) > 0 Then
                            Dim dRatio As Decimal = iNumOfActiveDrivers / oPolicy.VehicleCount(True)
                            If dRatio > 1 And dRatio <= 2 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "DVR_5"
                        If oPolicy.DriverCount(True) > 0 And oPolicy.VehicleCount(True) > 0 Then
                            Dim dRatio As Decimal = iNumOfActiveDrivers / oPolicy.VehicleCount(True)
                            If dRatio > 2 And dRatio <= 3 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "DVR_6"
                        If oPolicy.DriverCount(True) > 0 And oPolicy.VehicleCount(True) > 0 Then
                            Dim dRatio As Decimal = iNumOfActiveDrivers / oPolicy.VehicleCount(True)
                            If dRatio > 3 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "MILITARY"
                        Dim bAddMiltFactor As Boolean = False
                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            If Not oDrv.IsMarkedForDelete Then
                                If oDrv.Military Then
                                    bAddMiltFactor = True
                                    Exit For
                                End If
                            End If
                        Next
                        If bAddMiltFactor Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "SR22"
                        Dim bAddSR22Factor As Boolean = False
                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            If Not oDrv.IsMarkedForDelete Then
                                If oDrv.SR22 And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                                    bAddSR22Factor = True
                                    Exit For
                                End If
                            End If
                        Next
                        If bAddSR22Factor Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "LIMITED"
                        If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                            AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                        End If
                    Case "6RDC91", "12RDC91", "18RDC91", "24RDC91", "30RDC91", "36RDC91", "6RDC92", "12RDC92", "18RDC92", "24RDC92", "30RDC92", "36RDC92",
                         "6RDC93", "12RDC93", "18RDC93", "24RDC93", "30RDC93", "36RDC93", "6RDC94", "12RDC94", "18RDC94", "24RDC94", "30RDC94", "36RDC94"
                        If oPolicy.Type.ToUpper = "RENEWAL" Then
                            If AddRenewalDiscount(oPolicy, oRow.Item("FactorCode").ToString) Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If

                End Select
            Next

            ' For each driver with a UDR violation, add a UDR factor
            If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Then
                For Each oDrv As clsEntityDriver In oPolicy.Drivers
                    If Not oDrv.IsMarkedForDelete Then
                        If oDrv.IndexNum < 98 Then
                            If HasViolation(oDrv, "55559") And Not (oDrv.DLNState = "FN" Or oDrv.DLNState = "IT") Then
                                If oDrv.Age > 18 Then
                                    If Not FactorOnDriver(oDrv, "UDR") Then
                                        AddDriverFactor(oPolicy, oDrv, "UDR")
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            End If

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally

        End Try
    End Sub

    Private Function HasViolation(ByVal oDriver As clsEntityDriver, ByVal sViolCode As String) As Boolean
        Dim bHasViolation As Boolean = False

        For Each oViol As clsBaseViolation In oDriver.Violations
            If oViol.ViolTypeCode = sViolCode Then
                bHasViolation = True
                Exit For
            End If
        Next

        Return bHasViolation
    End Function

    Public Overridable Sub CheckPhysicalDamageSymbols2010AndOlder(ByVal policy As clsPolicyPPA)

        Dim ineligibleSymbolExists As Boolean = False
        Dim vehicleList As String = String.Empty

        For Each veh In policy.VehicleUnits
            If Not String.IsNullOrEmpty(veh.VehicleYear) AndAlso CInt(veh.VehicleYear) <= 2010 Then
                If veh.VehicleSymbolCode <> String.Empty Then
                    If CInt(veh.VehicleSymbolCode.Trim) >= 25 _
                        And veh.VinNo.ToUpper <> "NONOWNER" _
                        And CInt(veh.VehicleSymbolCode.Trim) <> 999 _
                        And CInt(veh.VehicleSymbolCode.Trim) <> 65 _
                        And CInt(veh.VehicleSymbolCode.Trim) <> 66 _
                        And CInt(veh.VehicleSymbolCode.Trim) <> 67 _
                        And CInt(veh.VehicleSymbolCode.Trim) <> 68 Then

                        ineligibleSymbolExists = True
                        vehicleList += veh.IndexNum() & ", "
                    End If
                End If
            End If
        Next

        If ineligibleSymbolExists Then
            vehicleList = vehicleList.Remove(vehicleList.LastIndexOf(", "), 2)
            policy.Notes = (AddNote(policy.Notes, "Ineligible Risk: Vehicle(s) " & vehicleList & " are unacceptable due to vehicle's value (Code: Symb)", "CheckPhysicalDamageSymbols2010AndOlder", "IER", policy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckPhysicalDamageSymbols2011AndNewer(ByVal policy As clsPolicyPPA)

        Dim ineligibleSymbolExists As Boolean = False
        Dim vehicleList As String = String.Empty

        For Each veh In policy.VehicleUnits
            If Not String.IsNullOrEmpty(veh.VehicleYear) AndAlso CInt(veh.VehicleYear) >= 2011 Then
                If veh.CollSymbolCode <> String.Empty Then
                    If CInt(veh.CollSymbolCode.Trim) >= 58 _
                        And veh.VinNo.ToUpper <> "NONOWNER" _
                        And CInt(veh.CollSymbolCode.Trim) <> 999 _
                        And CInt(veh.CollSymbolCode.Trim) <> 965 _
                        And CInt(veh.CollSymbolCode.Trim) <> 966 _
                        And CInt(veh.CollSymbolCode.Trim) <> 967 _
                        And CInt(veh.CollSymbolCode.Trim) <> 968 Then

                        ineligibleSymbolExists = True
                        vehicleList += veh.IndexNum() & ", "
                    End If
                End If
            End If
        Next

        If ineligibleSymbolExists Then
            vehicleList = vehicleList.Remove(vehicleList.LastIndexOf(", "), 2)
            policy.Notes = (AddNote(policy.Notes, "Ineligible Risk: Vehicle(s) " & vehicleList & " are unacceptable due to vehicle's value (Code: Symb).", "CheckPhysicalDamageSymbols2011AndNewer", "IER", policy.Notes.Count))
        End If
    End Sub

    Public Sub CheckSymbol2(ByRef oPolicy As clsPolicyPPA)

        Dim sVehicleList As String = ""
        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            'If VehicleApplies(oVehicle, oPolicy) Then

            Dim sVeh As String = ""
            sVeh = CheckSymbol2(oVehicle, oPolicy, oPolicy.Program)

            If Len(sVeh) > 0 Then
                If sVehicleList = String.Empty Then
                    sVehicleList = sVeh
                Else
                    sVehicleList &= ", " & sVeh
                End If
            End If
            'End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) are unacceptable due to vehicle value (Code: Symb) - " & sVehicleList & ".", "SymbolOver22", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckSymbol2(ByRef oVehicle As clsVehicleUnit, ByRef oPolicy As clsBasePolicy, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        Dim iMaxSymbol As Integer = 0

        Try
            iMaxSymbol = GetMaxMSRPSymbol(oVehicle.VehicleYear, oPolicy)

            If oVehicle.VehicleYear < 2011 Then
                If oVehicle.VehicleSymbolCode.Trim <> String.Empty Then
                    If CInt(oVehicle.VehicleSymbolCode.Trim) > iMaxSymbol _
                        And oVehicle.VinNo.ToUpper <> "NONOWNER" _
                        And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 65 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 66 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 67 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 68 Then

                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than " & iMaxSymbol & " - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            Else
                If CInt(oVehicle.VehicleSymbolCode.Trim) > iMaxSymbol _
                    And oVehicle.VinNo.ToUpper <> "NONOWNER" _
                    And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 965 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 966 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 967 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 968 Then

                    sVehicleList = oVehicle.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than " & iMaxSymbol & " - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If

                Try
                    If CInt(oVehicle.CompSymbolCode.Trim) > iMaxSymbol _
                        And oVehicle.VinNo.ToUpper <> "NONOWNER" _
                        And CInt(oVehicle.CompSymbolCode.Trim) <> 999 And CInt(oVehicle.CompSymbolCode.Trim) <> 965 And CInt(oVehicle.CompSymbolCode.Trim) <> 966 And CInt(oVehicle.CompSymbolCode.Trim) <> 967 And CInt(oVehicle.CompSymbolCode.Trim) <> 968 Then

                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than " & iMaxSymbol & " - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If


                    If CInt(oVehicle.CollSymbolCode.Trim) > iMaxSymbol _
                        And oVehicle.VinNo.ToUpper <> "NONOWNER" _
                        And CInt(oVehicle.CollSymbolCode.Trim) <> 999 And CInt(oVehicle.CollSymbolCode.Trim) <> 965 And CInt(oVehicle.CollSymbolCode.Trim) <> 966 And CInt(oVehicle.CollSymbolCode.Trim) <> 967 And CInt(oVehicle.CollSymbolCode.Trim) <> 968 Then

                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than " & iMaxSymbol & " - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                Catch ex As Exception
                    ' do nothing
                End Try
            End If
        Catch Ex As Exception
            sVehicleList = oVehicle.IndexNum
            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a non-numeric symbol - " & sVehicleList & ".", "SymbolNonNumeric", "IER", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End Try

        Return sVehicleList
    End Function

    Public Overrides Sub CheckVehicleBusinessUse(ByRef oPolicy As clsPolicyPPA)

        Dim iNumOfVehsWithBusUse As Integer = 0
        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            'If VehicleApplies(oVeh, oPolicy) Then
            If HasBusinessUse(oVeh) Or oVeh.TypeOfUseCode.ToUpper.Contains("ART") Then
                iNumOfVehsWithBusUse += 1
            End If
            'End If
        Next

        If iNumOfVehsWithBusUse > 1 And oPolicy.Program.ToUpper <> "DIRECT" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Only one vehicle on a policy may have Business Use.", "OnlyOneBusinessUse", "IER", oPolicy.Notes.Count))
        End If

        If oPolicy.Program.ToUpper = "DIRECT" Then
            If iNumOfVehsWithBusUse > 0 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with Business Use require company approval.", "CheckVehicleBusinessUse", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Sub CheckVehicleAge(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If oVehicle.VehicleAge > 40 And oVehicle.VinNo.ToUpper.Trim <> "NONOWNER" Then
                    If Len(sVehicleList) = 0 Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList = sVehicleList & ", " & oVehicle.IndexNum
                    End If
                End If
            End If
        Next

        If Len(sVehicleList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Vehicle age cannot be greater than 40 years.  - " & sVehicleList & ".", "VehicleAgeGT40", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckSalvagedPhysicalDamage(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        If oPolicy.UWQuestions.Count > 0 Then
            For Each oUWQ As clsUWQuestion In oPolicy.UWQuestions
                Select Case oUWQ.QuestionCode
                    Case "307"
                        If Left(oUWQ.AnswerText.ToUpper, 3) = "YES" Then
                            sVehicleList = String.Empty
                            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                                If Not oVehicle.IsMarkedForDelete Then
                                    Dim bPhysDamage As Boolean = DeterminePhysDamageExistsUW(oVehicle)
                                    If bPhysDamage Then
                                        If sVehicleList = String.Empty Then
                                            sVehicleList = oVehicle.IndexNum
                                        Else
                                            sVehicleList &= ", " & oVehicle.IndexNum
                                        End If
                                    End If
                                End If
                            Next
                            If sVehicleList <> String.Empty Then
                                oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Vehicle(s) with Physical Damage cannot have been re-built, salvaged or water damaged - " & sVehicleList & ".", "ReBuiltSalvagedVeh", "IER", oPolicy.Notes.Count)
                            End If
                        End If
                End Select
            Next
        End If
    End Sub

    Public Overridable Sub CheckVINLength(ByVal policy As clsPolicyPPA)

        Dim vehicleList As String = String.Empty

        For Each veh In policy.VehicleUnits
            If veh.VehicleYear >= 1980 Then
                If veh.VinNo.Length < 17 Then
                    vehicleList += veh.IndexNum() & ", "
                End If
            End If
        Next

        If Not String.IsNullOrWhiteSpace(vehicleList) Then
            vehicleList = vehicleList.Remove(vehicleList.LastIndexOf(", "), 2)
            policy.Notes = (AddNote(policy.Notes, "Ineligible Risk: The following vehicle(s) have a VIN under 17 characters long - " & vehicleList, "CheckVINLength", "IER", policy.Notes.Count))
        End If
    End Sub

    Public Overrides Sub CheckPermittedNotExcluded(ByVal oPolicy)
        Dim sDriverList As String = ""
        Dim bHasNamedInsured As Boolean = False

        sDriverList = ""
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.RelationToInsured.ToUpper.Trim() = "SELF" Then
                bHasNamedInsured = True
            End If

            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.DriverStatus.ToUpper = "PERMITTED" Then
                    If oDrv.MaritalStatus.ToUpper = "SINGLE" And oDrv.Age <= 18 Then
                        'the driver is allowed as Permitted
                    Else
                        If sDriverList = "" Then
                            sDriverList = oDrv.IndexNum
                        Else
                            sDriverList &= ", " & oDrv.IndexNum
                        End If
                    End If
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) must be rated as Active or Excluded - " & sDriverList & ".", "PermittedNotPermitted", "IER", oPolicy.Notes.Count))
        End If


        If Not bHasNamedInsured Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: At least one driver must have relation to insured as 'Insured'.", "NoNamedInsured", "IER", oPolicy.Notes.Count))
        End If
    End Sub
    Public Overridable Sub CheckUnlistedAdditionalDrivers(ByVal policy As clsPolicyPPA)

        With policy
            If .UWQuestions.Count > 0 Then
                For Each oUWQ As clsUWQuestion In .UWQuestions
                    Select Case oUWQ.QuestionCode
                        Case "302"
                            If Left(oUWQ.AnswerText.ToUpper.Trim, 3) = "YES" Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: All drivers of your vehicle(s) must be listed on the application.  Please review Question #3 on the Additional Information page.", "UnlistedAdditionalDrivers", "IER", .Notes.Count))
                            End If
                    End Select
                Next
            End If
        End With

    End Sub

    Public Overrides Function PolicyHasIneligibleRisk(ByVal oPolicy As CorPolicy.clsPolicyPPA) As Boolean
        Dim bIneligibleRisk As Boolean = False
        Dim sReason As String = String.Empty

        ' *** TX DOES NOT CURRENTLY HAVE INELIGIBLE RISK FACTOR, SO LEAVE AS EMPTY IMPLEMENTATION FOR NOW ***

        ''Drivers (New business and new driver added to an existing policy will not be rated or 
        ''   written if they fall into one of the categories below.  
        ''   Renewal drivers who fall into one of these categories will 
        ''   receive and ineligible risk surcharge.)
        'Dim iTotalPoints As Integer = 0
        'For Each oDriver As clsEntityDriver In oPolicy.Drivers
        '    CheckViolations(oDriver, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, "B")

        '    If (oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED") And Not oDriver.IsMarkedForDelete Then

        '        '1.     Operators with more than 6 chargeable violations.
        '        Dim iNumChargeable As Integer = 0
        '        For Each oViol As clsBaseViolation In oDriver.Violations
        '            If oViol.Chargeable Then
        '                iNumChargeable += 1
        '            End If
        '        Next

        '        If iNumChargeable > 6 Then
        '            If oPolicy.Program.ToUpper() = "DIRECT" Then
        '                sReason = "Driver " & oDriver.IndexNum & " has more than 6 violations."
        '            Else
        '                sReason = "Driver with more than 6 chargeable violations.- " & oDriver.IndexNum
        '            End If
        '            bIneligibleRisk = True
        '            Exit For
        '        End If


        '        '2.     Operators with more than 2 alcohol, drug, or controlled substance violations within the previous 35 months.
        '        '           (Use the same violations as Webrater)
        '        Dim iDWI As Integer = 0
        '        For Each oViolation As clsBaseViolation In oDriver.Violations
        '            If oViolation.ViolGroup = "DWI" Then
        '                If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
        '                    iDWI += 1
        '                End If
        '            End If
        '        Next
        '        If iDWI > 2 Then
        '            If oPolicy.Program.ToUpper() = "DIRECT" Then
        '                sReason = "The following driver(s) have more than 2 drug or alcohol violations.- " & oDriver.IndexNum
        '            Else
        '                sReason = "Driver with more than 2 alcohol, drug, or controlled substance violations within the previous 35 months.- " & oDriver.IndexNum
        '            End If
        '            bIneligibleRisk = True
        '            Exit For
        '        End If

        '        '3.     Operators convicted of an alcohol, drug, or controlled substance violation prior to age 21 (Underage DWI).
        '        '           (Use same violations as Webrater)
        '        If oDriver.Age < 21 Then
        '            If iDWI > 0 Then
        '                sReason = "Driver convicted of an alcohol, drug, or controlled substance violation prior to age 21 (Underage DWI).- " & oDriver.IndexNum
        '                bIneligibleRisk = True
        '                Exit For
        '            End If
        '        End If

        '        '4.     Any risk with more than 18 driver violation points combined for all drivers. 
        '        '       total up for each driver, then check outside the for loop
        '        iTotalPoints = iTotalPoints + oDriver.Points


        '    End If
        'Next

        ''4.     Any risk with more than 18 driver violation points combined for all drivers. 
        'If iTotalPoints > 18 Then
        '    If oPolicy.Program.ToUpper = "DIRECT" Then
        '        sReason = "Maximum of 18 violation points is allowed for all drivers"
        '    Else
        '        sReason = "More than 18 driver violation points combined for all drivers"
        '    End If
        '    bIneligibleRisk = True
        'End If

        'If Not bIneligibleRisk Then ' No need to look at vehicles if we already know this is an ineligible risk
        '    'Replacement Vehicles (The surcharge list applies to replacement vehicles only. 
        '    '   If a driver wants to add a new vehicle to a policy or the policy is new business, 
        '    '   we will not write the risk if it falls into one of the categories below.  
        '    '   Any replacement vehicles that fall under one of these categories will receive an ineligible risk surcharge)

        '    Dim iBusinessUseCount As Integer = 0
        '    For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
        '        If Not oVeh.IsMarkedForDelete AndAlso oVeh.VinNo <> "NONOWNER" Then

        '            '1.     Vehicles with a value over $60,000.
        '            Dim iMaxSymbol As Integer
        '            iMaxSymbol = GetMaxMSRPSymbol(oVeh.VehicleYear, oPolicy)

        '            Dim iVehSymbol As Integer
        '            Try
        '                If oVeh.PriceNewSymbolCode.Trim = String.Empty Then
        '                    iVehSymbol = 0
        '                Else
        '                    iVehSymbol = CInt(oVeh.PriceNewSymbolCode.Trim)
        '                End If
        '            Catch ex As Exception
        '                ' if the vehicle was added before we added pricenewsymbolcode it might be an empty string
        '                ' if this is the case use vehiclesymbolcode
        '                iVehSymbol = 0
        '            End Try

        'If CInt(oVeh.VehicleSymbolCode.Trim) = 999 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 965 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 966 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 967 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 968 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 65 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 66 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 67 Or
        '                   CInt(oVeh.VehicleSymbolCode.Trim) = 68 Then
        '    If CInt(oVeh.StatedAmt) > 60000 Then
        '        sReason = "Vehicle with a value over $60,000.- " & oVeh.IndexNum
        '        bIneligibleRisk = True
        '        Exit For
        '    End If
        'ElseIf iVehSymbol > iMaxSymbol Then
        '    sReason = "Vehicle with a value over $60,000.- " & oVeh.IndexNum
        '    bIneligibleRisk = True
        '    Exit For
        'End If

        '            '2.     Vehicles rated a with physical damage symbol 25 or higher for model years 2010 and older.
        '            If oVeh.VehicleYear <= 2010 Then
        '                If oVeh.VehicleSymbolCode <> String.Empty Then
        '                    Try
        '                        If CInt(oVeh.VehicleSymbolCode.Trim) >= 25 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
        '                            If oPolicy.Program.ToUpper = "DIRECT" Then
        '                                sReason = "Vehicle " & oVeh.IndexNum & " has an Original Cost New over $60,000 (rating symbol above 24)"
        '                            Else
        '                                sReason = "Vehicle with physical damage symbol 25 or higher.- " & oVeh.IndexNum
        '                            End If

        '                            bIneligibleRisk = True
        '                            Exit For
        '                        End If
        '                    Catch ex As Exception
        '                    End Try
        '                End If
        '            End If

        '            '3.     Vehicles rated a with physical damage symbol 58 or higher for model years 2011 and newer.
        '            If oVeh.VehicleYear >= 2011 Then
        '                If oVeh.VehicleSymbolCode <> String.Empty Then
        '                    Try
        '                        If CInt(oVeh.VehicleSymbolCode.Trim) >= 58 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
        '                            If oPolicy.Program.ToUpper = "DIRECT" Then
        '                                sReason = "Vehicle " & oVeh.IndexNum & " has an Original Cost New over $60,000 (rating symbol above 57)"
        '                            Else
        '                                sReason = "Vehicle with physical damage symbol 58 or higher.- " & oVeh.IndexNum
        '                            End If
        '                            bIneligibleRisk = True
        '                            Exit For
        '                        End If
        '                    Catch Ex As Exception
        '                    End Try
        '                End If
        '            End If

        '            '4.     Vehicle with special additional/custom equipment in excess of $5,000.
        '            If oVeh.CustomEquipmentAmt > 5000 Then
        '                sReason = "Vehicle with special additional/custom equipment in excess of $5,000.- " & oVeh.IndexNum
        '                bIneligibleRisk = True
        '                Exit For
        '            End If

        '            '5.     Vehicles that have a title or registration indicating that the vehicle has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.
        '            '   (These vehicles can be quoted for BI, PD, UMBI, UIMBI and MED coverages). 
        '            If DeterminePhysDamageExists(oVeh) Then
        '                For Each uw As clsUWQuestion In oPolicy.UWQuestions
        '                    If uw.QuestionCode = "307" Then
        '                        If Left(uw.AnswerText.ToUpper, 3) = "YES" Then
        '                            sReason = "Vehicle that has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.- " & oVeh.IndexNum
        '                            bIneligibleRisk = True
        '                            Exit For
        '                        End If
        '                    End If
        '                Next
        '            End If

        '            '6.     More than 1 Business or Artisan use vehicle. 
        '            For Each oFactor As clsBaseFactor In oVeh.Factors
        '                If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
        '                    iBusinessUseCount = iBusinessUseCount + 1
        '                    Exit For
        '                End If
        '            Next

        '            '7.     Vehicles with an out-of-state garaging location.
        '            If Not ValidateVehicleZipCode(oVeh.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
        '                sReason = "Vehicles is garaged out of state.- " & oVeh.IndexNum
        '                bIneligibleRisk = True
        '                Exit For
        '            End If


        '            '8.     Vehicles over 15 years old are unacceptable for all physical damage coverage on new policies. 
        '            If oVeh.VehicleAge > 15 AndAlso DeterminePhysDamageExists(oVeh) Then
        '                sReason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " & oVeh.IndexNum
        '                bIneligibleRisk = True
        '                Exit For
        '            End If

        '            '9.     Vehicles over 40 years old are unacceptable for all coverages.
        '            If oVeh.VehicleAge > 40 Then
        '                sReason = "Vehicles over 40 years old are unacceptable for all coverages.- " & oVeh.IndexNum
        '                bIneligibleRisk = True
        '                Exit For
        '            End If
        '        End If
        '    Next

        '    '6.     More than 1 Business or Artisan use vehicle. 
        '    If iBusinessUseCount > 1 Then
        '        sReason = "More than 1 Business or Artisan use vehicle."
        '        bIneligibleRisk = True
        '    End If
        'End If

        'If bIneligibleRisk Then
        '    If Not FactorOnPolicy(oPolicy, "INELIGIBLE") Then
        '        oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: A surcharge has been applied to policy due to: " & sReason, "IRSurcharge", "AAF", oPolicy.Notes.Count))
        '    End If
        'End If

        Return bIneligibleRisk
    End Function
    'Public Sub CheckSuspendedLicense(ByVal oPolicy As clsPolicyPPA)
    '    Dim sDriverList As String = ""
    '    For Each oDriver As clsEntityDriver In oPolicy.Drivers
    '        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
    '            If Not oDriver.LicenseStatus Is Nothing Then
    '                If oDriver.LicenseStatus.Length > 0 Then
    '                    If (oDriver.LicenseStatus.ToUpper.Trim = "SUS") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPEND") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
    '                        If Not oDriver.SR22 Then
    '                            sDriverList = oDriver.IndexNum
    '                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have a suspended license without filing an SR-22 - " & sDriverList & ".", "SuspendedLic", "IER", oPolicy.Notes.Count))
    '                        End If
    '                    End If
    '                End If
    '            End If
    '        ElseIf (oDriver.DriverStatus.ToUpper = "EXCLUDED") Then
    '            If Not oDriver.LicenseStatus Is Nothing Then
    '                If oDriver.LicenseStatus.Length > 0 Then
    '                    If (oDriver.LicenseStatus.ToUpper.Trim = "SUS") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPEND") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
    '                        'Do nothing
    '                    End If
    '                End If
    '            End If
    '        ElseIf (oDriver.DriverStatus.ToUpper = "NHH") Then
    '            If Not oDriver.LicenseStatus Is Nothing Then
    '                If oDriver.LicenseStatus.Length > 0 Then
    '                    If (oDriver.LicenseStatus.ToUpper.Trim = "SUS") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPEND") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
    '                        sDriverList = oDriver.IndexNum
    '                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following NHH driver(s) have a suspended license  - " & sDriverList & ".", "NHHSuspendedLic", "IER", oPolicy.Notes.Count))
    '                    End If
    '                End If
    '            End If
    '        End If

    '    Next
    'End Sub
    'Public Sub CheckRevokedLicense(ByVal oPolicy As clsPolicyPPA)
    '    Dim sDriverList As String = String.Empty

    '    For Each oDrv As clsEntityDriver In oPolicy.Drivers
    '        Dim sDrv As String = ""
    '        sDrv = CheckRevokedLicense(oDrv, oPolicy.Program)

    '        If Len(sDrv) > 0 Then
    '            If sDriverList = String.Empty Then
    '                sDriverList = sDrv
    '            Else
    '                sDriverList &= ", " & sDrv
    '            End If
    '        End If
    '    Next
    '    If sDriverList <> "" Then
    '        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have a Revoked/Cancelled driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "RevokedLic", "IER", oPolicy.Notes.Count))
    '    End If
    'End Sub

    'Public Overridable Function CheckRevokedLicense(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
    '    Dim sDriverList As String = ""

    '    If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
    '        If Not oDriver.LicenseStatus Is Nothing Then
    '            If oDriver.LicenseStatus.Length > 0 Then
    '                If (oDriver.LicenseStatus.ToUpper.Trim = "REV") Or (oDriver.LicenseStatus.ToUpper.Trim = "REVOKED") Or (oDriver.LicenseStatus.ToUpper.Trim = "REVOKED/CANCELED") Then
    '                    sDriverList = oDriver.IndexNum
    '                    If Not oNoteList Is Nothing Then
    '                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have a Revoked/Cancelled driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "RevokedLic", "IER", oNoteList.Count, "AOLE"))
    '                        Return ""
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If

    '    Return sDriverList
    'End Function
    'Public Sub CheckExpiredLicense(ByVal oPolicy As clsPolicyPPA)
    '    Dim sDriverList As String = String.Empty

    '    For Each oDrv As clsEntityDriver In oPolicy.Drivers
    '        Dim sDrv As String = ""
    '        sDrv = CheckExpiredLicense(oDrv, oPolicy.Program)

    '        If Len(sDrv) > 0 Then
    '            If sDriverList = String.Empty Then
    '                sDriverList = sDrv
    '            Else
    '                sDriverList &= ", " & sDrv
    '            End If
    '        End If
    '    Next
    '    If sDriverList <> "" Then
    '        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have an Expired driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "ExpiredLic", "IER", oPolicy.Notes.Count))
    '    End If
    'End Sub

    'Public Overridable Function CheckExpiredLicense(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
    '    Dim sDriverList As String = ""

    '    If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
    '        If Not oDriver.LicenseStatus Is Nothing Then
    '            If oDriver.LicenseStatus.Length > 0 Then
    '                If (oDriver.LicenseStatus.ToUpper.Trim = "EX") Or (oDriver.LicenseStatus.ToUpper.Trim = "EXPIRED") Then
    '                    sDriverList = oDriver.IndexNum
    '                    If Not oNoteList Is Nothing Then
    '                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have an Expired driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "ExpiredLic", "IER", oNoteList.Count, "AOLE"))
    '                        Return ""
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If

    '    Return sDriverList
    'End Function
    'Public Sub CheckIDOnly(ByVal oPolicy As clsPolicyPPA)
    '    Dim sDriverList As String = String.Empty

    '    For Each oDrv As clsEntityDriver In oPolicy.Drivers
    '        Dim sDrv As String = ""
    '        sDrv = CheckIDOnly(oDrv, oPolicy.Program)

    '        If Len(sDrv) > 0 Then
    '            If sDriverList = String.Empty Then
    '                sDriverList = sDrv
    '            Else
    '                sDriverList &= ", " & sDrv
    '            End If
    '        End If
    '    Next
    '    If sDriverList <> "" Then
    '        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) do not have a Valid driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "IDOnly", "IER", oPolicy.Notes.Count))
    '    End If
    'End Sub

    'Public Overridable Function CheckIDOnly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
    '    Dim sDriverList As String = ""

    '    If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
    '        If Not oDriver.LicenseStatus Is Nothing Then
    '            If oDriver.LicenseStatus.Length > 0 Then
    '                If (oDriver.LicenseStatus = "ID Only") Then
    '                    sDriverList = oDriver.IndexNum
    '                    If Not oNoteList Is Nothing Then
    '                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) do not have a Valid driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "IDOnly", "IER", oNoteList.Count, "AOLE"))
    '                        Return ""
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If

    '    Return sDriverList
    'End Function

    Private Function AddRenewalDiscount(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String) As Boolean

        Dim addDiscount As Boolean = False
        Dim policyAge As Integer = 0
        Dim bContinue As Boolean = False


        ' First, age the renewal
        policyAge = DateDiff(DateInterval.Month, oPolicy.OrigTermEffDate, oPolicy.EffDate)

        Select Case sFactorCode
            Case "6RDC91", "6RDC92", "6RDC93", "6RDC94"
                ' >= 6 month and <12 month
                If policyAge >= 6 And policyAge < 12 Then
                    bContinue = True
                End If

            Case "12RDC91", "12RDC92", "12RDC93", "12RDC94"
                ' >= 12 month and < 18 month
                If policyAge >= 12 And policyAge < 18 Then
                    bContinue = True
                End If

            Case "18RDC91", "18RDC92", "18RDC93", "18RDC94"
                ' >= 18 month and < 24 month
                If policyAge >= 18 And policyAge < 24 Then
                    bContinue = True
                End If
            Case "24RDC91", "24RDC92", "24RDC93", "24RDC94"
                ' >= 24 month and < 30 month
                If policyAge >= 24 And policyAge < 30 Then
                    bContinue = True
                End If
            Case "30RDC91", "30RDC92", "30RDC93", "30RDC94"
                ' >= 30 month and < 36 month
                If policyAge >= 30 And policyAge < 36 Then
                    bContinue = True
                End If
            Case "36RDC91", "36RDC92", "36RDC93", "36RDC94"
                ' >= 36 month
                If policyAge >= 36 Then
                    bContinue = True
                End If
        End Select

        If bContinue Then
            ' Now check the UWTier
            ' If this is a grandfathered renewal, then it should be in tier 94
            Dim grandfatheredRenewalDate As Date = CDate(GetStateInfoValue(oPolicy, oPolicy.Program, "UWTIER", "TIER_94_REVISION", "GRANDFATHERED_RENEWAL_DATE"))
            If DateDiff(DateInterval.Day, oPolicy.OrigTermEffDate, grandfatheredRenewalDate) > 0 Then
                Select Case sFactorCode
                    Case "6RDC94", "12RDC94", "18RDC94", "24RDC94", "30RDC94", "36RDC94"
                        addDiscount = True
                End Select
            Else

                Select Case sFactorCode
                    Case "6RDC91", "12RDC91", "18RDC91", "24RDC91", "30RDC91", "36RDC91"
                        If oPolicy.UWTier = "91" Then
                            addDiscount = True
                        End If
                    Case "6RDC92", "12RDC92", "18RDC92", "24RDC92", "30RDC92", "36RDC92"
                        If oPolicy.UWTier = "92" Then
                            addDiscount = True
                        End If
                    Case "6RDC93", "12RDC93", "18RDC93", "24RDC93", "30RDC93", "36RDC93"
                        If oPolicy.UWTier = "93" Then
                            addDiscount = True
                        End If
                End Select

            End If
        End If

        Return addDiscount
    End Function

    Private Function GetDaysLapseCode(ByVal daysLapse As Integer) As Integer
        Dim daysLapseCode As Integer = 0

        Select Case daysLapse
            Case Is <= 7
                daysLapseCode = 2
            Case 8 To 30
                daysLapseCode = 1
            Case Else
                daysLapseCode = 0
        End Select

        Return daysLapseCode
    End Function

    Public Overridable Sub CheckForRestrictedAgentByGaragingZipCode(ByVal policy As clsPolicyPPA)

        Dim garagingZip As String
        Dim garagingCounty As String

        For Each oVeh As clsVehicleUnit In policy.VehicleUnits
            If oVeh.Zip <> "" Then
                garagingZip = oVeh.Zip
                garagingCounty = oVeh.County

                If (IsAgentRestrictedByGaragingZipCode(policy, garagingZip, garagingCounty)) Then
                    policy.Notes = (AddNote(policy.Notes, "Ineligible Risk: Agent is currently restricted from binding coverage in this zip code", "AgentRestrictedByZip", "IER", policy.Notes.Count))
                    Exit For
                End If

            End If
        Next

    End Sub

    Public Overridable Function IsAgentRestrictedByGaragingZipCode(ByVal policy As clsPolicyPPA, ByVal gZip As String, ByVal gCounty As String) As Boolean

        Dim productCode As String = policy.Product
        Dim stateCode As String = policy.StateCode
        Dim agentCode As String = policy.Agency.AgencyID
        Dim garagingZipCode As String = gZip
        Dim garagingCounty As String = gCounty


        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT * "
                sSql &= " FROM pgm" & productCode & stateCode & "..AgentRestrictionByGaragingZipCode with (NOLOCK) "
                sSql &= " WHERE AgentCode = @agentCode "
                sSql &= " AND ZipCode = @zipCode "
                sSql &= " AND County = @county "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@agentCode", SqlDbType.VarChar).Value = agentCode
                cmd.Parameters.Add("@zipCode", SqlDbType.VarChar).Value = garagingZipCode
                cmd.Parameters.Add("@county", SqlDbType.VarChar).Value = garagingCounty

                oReader = cmd.ExecuteReader

                Return oReader.HasRows()

            End Using

        Catch ex As Exception
            'Let the caller handle it
            Throw ex
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            oConn.Close()
            oConn.Dispose()
        End Try


        Return False

    End Function

End Class
