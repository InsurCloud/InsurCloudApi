Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data.SqlClient
Imports System.Data
Imports System.Collections.Generic
Imports System.Configuration

Public Class clsRules203
    Inherits clsRules2

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

        ' If Renewal of Policy uploaded after Feb 2014 revision and if program is not Summit
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
                        Case 0
                            oPolicy.PolicyInsured.DaysLapse = 2
                        Case 1 To 30
                            oPolicy.PolicyInsured.DaysLapse = 1
                        Case Else
                            oPolicy.PolicyInsured.DaysLapse = 0
                            oPolicy.PolicyInsured.PriorLimitsCode = "0"
                    End Select
                Else
                    If oPolicy.Type.ToUpper = "RENEWAL" And DateDiff(DateInterval.Day, grandfatheredRenewalDate, oPolicy.OrigTermEffDate) >= 0 Then
                        ' Renewal of policy created after Feb 2014 revision
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

                ' If it is a renewal, set monthspriorcontcov = 6 since they had to have prior coverage
                'If Not oPolicy.PolicyTermTypeInd Is Nothing Then
                '    If oPolicy.PolicyTermTypeInd.ToUpper.Trim = "R" Then
                '        If oPolicy.PolicyInsured.MonthsPriorContCov < 1 Then
                '            oPolicy.PolicyInsured.MonthsPriorContCov = 6
                '        End If
                '    End If
                'End If

                ' Set the Continuous Coverage value
                Dim continuousCoverage As Integer = 0

                If oPolicy.Type.ToUpper = "RENEWAL" Then
                    ' If this is Classic or Direct, and after the January 2014 revision, use the new logic
                    If ((oPolicy.Program.ToUpper.Trim = "CLASSIC" Or oPolicy.Program.ToUpper.Trim = "DIRECT") And (DateDiff(DateInterval.Day, oPolicy.RateDate, uwTierRevisionDate) <= 0)) Then
                        ' Renewal after Feb 2014 revision. Is it a grandfathered renewal?
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
                        ' Renewal prior to January 2014 revision. Use existing renewal logic
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

            ' For Rate Comparison, if this is a Renewal and the Rate Date is >= '12/16/2013', use Tier 94
            'If oPolicy.Type.ToUpper = "RENEWAL" Then
            '    If DateDiff(DateInterval.Day, oPolicy.ActualRateDate, #12/16/2013#) < 1 Then
            '        sTier = "94"
            '        oPolicy.UWTier = "94"
            '        oPolicy.PolicyInsured.UWTier = "94"
            '    End If
            'End If

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

    Public Overridable Sub CheckSR22(ByRef oPolicy As clsPolicyPPA)
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.SR22 Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: SR-22 is not available for Arkansas.", "ARSR22", "IER", oPolicy.Notes.Count))
                    Exit For
                End If
            End If
        Next
    End Sub

    Public Overridable Function CheckInsuranceFraud(ByRef oPolicy As clsPolicyPPA)
        Dim isConvictedOfFraud As Boolean = False

        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED" Then
                Dim iViolcount As Integer = 0
                For Each oViolation As clsBaseViolation In oDriver.Violations
                    If oViolation.ViolDesc = "CONVICTION OF INSURANCE FRAUD" Then
                        isConvictedOfFraud = True
                    End If
                Next
            End If
        Next


        If isConvictedOfFraud Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Drivers convicted of insurance fraud are unacceptable risks.", "DriverInsFraud", "IER", oPolicy.Notes.Count))
        End If

        Return ""
    End Function
    Public Sub CheckDriverPointsClassic(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints15Classic(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 3 driver violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
        End If


        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints19Classic(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 driver violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
        End If

        sDriverList = ""
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPointsClassic(oDrv, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            End If
        Next

        Dim iMaxPoints As Integer = GetProgramSetting("MaxPoints")
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than " & iMaxPoints & " driver violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If

    End Sub
    Public Overridable Function CheckDriverPointsClassic(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED" Then
            If (oDriver.Points > 15) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 15 driver violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function
    Public Overridable Function CheckDriverPoints15Classic(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED" Then
                If (oDriver.Age >= 15 And oDriver.Age <= 18) Then
                    If oDriver.Points > 3 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 3 driver violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function
    Public Overridable Function CheckDriverPoints19Classic(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED" Then
                If (oDriver.Age >= 19 And oDriver.Age <= 21) Then
                    If oDriver.Points > 5 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 driver violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function
    Public Sub CheckMaxViolationsClassic(ByRef oPolicy As clsPolicyPPA)
        ' Rule 4.C (drivers with greater than 6 violations in the Chargeable Period)
        Dim sDriverList As String = ""

        Dim iMaxViolations As Integer = GetProgramSetting("MaxViolations")
        Dim iNumOfViols As Integer = 0
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                    Dim sDrv As String = ""
                    sDrv = CheckMaxViolationsClassic(oDrv, oPolicy.Program)
                    If Len(sDrv) > 0 Then
                        If sDriverList = "" Then
                            sDriverList = sDrv
                        Else
                            sDriverList &= ", " & sDrv
                        End If
                    End If
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than " & iMaxViolations & " accidents or violations - " & sDriverList & ".", "MaxDriverViols", "IER", oPolicy.Notes.Count))
        End If
    End Sub
    Public Overridable Function CheckMaxViolationsClassic(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        ' Rule 4.C (drivers with greater than 6 violations in the Chargeable Period)
        Dim sDriverList As String = ""
        Dim iNumOfViols As Integer = 0

        Dim iMaxViolations As Integer = GetProgramSetting("MaxViolations")
        If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
            iNumOfViols = 0
            For Each oViol As clsBaseViolation In oDrv.Violations
                If oViol.Chargeable Then
                    iNumOfViols += 1
                End If

            Next
            If iNumOfViols > iMaxViolations Then
                sDriverList = oDrv.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than " & iMaxViolations & " accidents or violations - " & sDriverList & ".", "MaxDriverViols", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function
    Public Overrides Sub CheckPolicyPoints(ByVal oPolicy As clsPolicyPPA)
        Dim iTotalPoints As Integer = 0
        Dim iMaxViolations As Integer = GetProgramSetting("MaxPolicyPoints")
        With oPolicy
            For Each oDrv As clsEntityDriver In .Drivers
                If oDrv.DriverStatus.ToUpper = "ACTIVE" Or oDrv.DriverStatus.ToUpper = "PERMITTED" And oDrv.DriverStatus.ToUpper = "EXCLUDED" And Not oDrv.IsMarkedForDelete Then
                    iTotalPoints += oDrv.Points
                End If
            Next

            If iTotalPoints > iMaxViolations Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Policy is ineligible based on the number of driver violation points", "PolPointsoverMax", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Sub CheckDriverPointsSummit(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""
            sDrv = CheckDriverPointsSummit(oDrv, oPolicy.Program)

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than 30 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckDriverPointsSummit(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If (oDriver.Points > 30) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 30 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function


    Public Sub CheckDriverViolationsSummit(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""
            sDrv = CheckDriverViolationsSummit(oDrv, oPolicy.Program)

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than 12 violations - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckDriverViolationsSummit(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            Dim iViolcount As Integer = 0
            For Each oViolation As clsBaseViolation In oDriver.Violations
                If oViolation.Chargeable Then
                    iViolcount = iViolcount + 1
                End If
            Next

            If (iViolcount > 12) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 12 violations - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function


    Public Sub CheckDriverViolationsClassic(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""
            sDrv = CheckDriverViolationsClassic(oDrv, oPolicy.Program)

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" And oPolicy.Program.ToUpper <> "DIRECT" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than 5 accidents or violations - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckDriverViolationsClassic(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            Dim iViolcount As Integer = 0
            For Each oViolation As clsBaseViolation In oDriver.Violations
                If oViolation.Chargeable Then
                    iViolcount = iViolcount + 1
                End If
            Next

            If (iViolcount > 5) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 5 accidents or violations - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function



    Public Overrides Sub CheckCustomEquipmentLimits(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckCustomEquipmentLimits(oVehicle)

                If Len(sVeh) > 0 Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList &= ", " & oVehicle.IndexNum
                    End If
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The Custom Equipment limit chosen does not match the amount of Custom Equipment entered for the following vehicle(s) -  " & sVehicleList & ".", "CustomEquipMismatch", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overrides Function CheckCustomEquipmentLimits(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        Dim dLowerLimit As Decimal
        Dim dUpperLimit As Decimal
        For Each oCov As clsPACoverage In oVehicle.Coverages
            If oCov.CovGroup = "SPE" And Not oCov.IsMarkedForDelete Then
                Dim iPos As Integer = InStr(oCov.CovLimit, "-")
                dLowerLimit = CDec(Mid(oCov.CovLimit, 1, iPos - 1))
                dUpperLimit = CDec(Mid(oCov.CovLimit, iPos + 1, oCov.CovLimit.Trim.Length))
                If oVehicle.CustomEquipmentAmt < dLowerLimit Or _
                 oVehicle.CustomEquipmentAmt > dUpperLimit Then
                    sVehicleList = oVehicle.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The Custom Equipment limit chosen does not match the amount of Custom Equipment entered for the following vehicle(s) -  " & sVehicleList & ".", "CustomEquipMismatch", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
                Exit For
            End If
        Next

        If oVehicle.CustomEquipmentAmt > 5000 Then
            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The Custom Equipment limit is over $5,000 following vehicle(s) -  " & sVehicleList & ".", "CustomEquipLimit", "IER", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End If

        Return sVehicleList
    End Function

    Public Sub CheckArtistanUse(ByRef oPolicy As clsPolicyPPA)
        Dim iCounter As Integer = 0

        Dim bIsNonOwner As Boolean = False

        ' Check to see if this is a nonowner policy
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If oVehicle.VinNo.ToUpper = "NONOWNER" And Not oVehicle.IsMarkedForDelete Then
                bIsNonOwner = True
                Exit For
            End If
        Next

        If bIsNonOwner Then
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If VehicleApplies(oVeh, oPolicy) Then
                    If oVeh.TypeOfUseCode.ToUpper = "ARTISAN" Or oVeh.TypeOfUseCode.ToUpper = "BUSINESS" Or oVeh.TypeOfUseCode.ToUpper = "ART" Then
                        iCounter += 1
                    End If
                End If
            Next
            If iCounter > 0 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Artisan/Business use vehicles are not permitted on non-owner policies.", "ArtisanNonOwner", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Sub CheckClassicNonOwner(ByVal oPolicy As clsPolicyPPA)
        Dim bIsNonOwner As Boolean = False

        If oPolicy.Program.ToUpper() = "CLASSIC" Then
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If oVehicle.VinNo.ToUpper = "NONOWNER" And Not oVehicle.IsMarkedForDelete Then
                    bIsNonOwner = True
                    Exit For
                End If
            Next
        End If

        If bIsNonOwner Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-Owner risks are not currently available for the Classic program.", "ClassicNonOwner", "IER", oPolicy.Notes.Count))
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
                                    If bPhysDamage Or oPolicy.Program.ToUpper = "DIRECT" Then
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

    Public Sub CheckMatureDriverDiscount(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""

            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.MatureDriver Then
                    Dim numYears As Double = -1.0
                    Dim numDaysDiff As Long = DateDiff(DateInterval.DayOfYear, oDriver.MatureDriverCourseDate, Date.Today)

                    If numDaysDiff = 0 Then 'course date is today
                        numYears = 0
                    ElseIf numDaysDiff <> 0 Then
                        numYears = numDaysDiff / 365.0
                    End If


                    If oDriver.Age < 55 Or numYears < 0 Or numYears > 3 Then
                        sDrv = oDriver.IndexNum
                    End If
                End If
            End If

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The Mature Driver Discount does not apply unless the driver is 55 or older and the accident prevention course was completed no more than 3 years prior to the policy effective date. - " & sDriverList & ".", "MatureDis", "IER", oPolicy.Notes.Count))
        End If
    End Sub


    Public Sub CheckScholasticDiscount(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""

            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.ScholasticHonor Then
                    If oDriver.Age > 24 Or oDriver.MaritalStatus.ToUpper <> "SINGLE" Then
                        sDrv = oDriver.IndexNum
                    End If
                End If
            End If

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The Scholastic Honor Discount does not apply unless the driver is single and 24 or younger. - " & sDriverList & ".", "SchDis", "IER", oPolicy.Notes.Count))
        End If
    End Sub


    Public Overrides Function VehicleHasIneligibleRisk(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim bIneligible As Boolean = False


        ' if not allowed, validate that zip is in the territorydefinitions table
        Dim sVehicleList As String = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not ValidateVehicleZipCode(oVehicle.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                If Len(sVehicleList) = 0 Then
                    sVehicleList = oVehicle.IndexNum
                Else
                    sVehicleList = sVehicleList & "," & oVehicle.IndexNum
                End If
            End If
        Next

        If Len(sVehicleList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: A surcharge has been applied to the added vehicle due to vehicle being garaged out of state- " & sVehicleList & ".", "IRSurcharge", "AAF", oPolicy.Notes.Count))
            Return True
        End If



        Return bIneligible
    End Function

    Public Overrides Sub CalculateVehicleAge(ByVal oPolicy As clsPolicyPPA, ByVal bShowTrueAge As Boolean)

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            Dim iVehAge As Integer = 0
            Dim iEffYear As Integer = Year(oPolicy.EffDate)
            Dim iEffMonth As Integer = Month(oPolicy.EffDate)
            Dim iVehYear As Integer = oVeh.VehicleYear

            If iEffMonth >= 10 Then
                iVehAge = iEffYear - iVehYear + 2
            Else
                iVehAge = iEffYear - iVehYear + 1
            End If

            If iVehAge < 1 Then iVehAge = 1

            If Not bShowTrueAge Then
                If iVehAge > 41 Then iVehAge = 41
            End If
            oVeh.VehicleAge = iVehAge
        Next
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

    Public Overrides Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyPPA) As String

        Dim sItemsToBeFaxedIn As String = ""

        sItemsToBeFaxedIn &= MyBase.ItemsToBeFaxedIn(oPolicy)

        'companion
        If oPolicy.CompanionHOMCarrierName.Trim <> "" And (Not IsRewritePolicy(oPolicy)) Then
            sItemsToBeFaxedIn &= "Copy of Companion Homeowner Insurance Policy" & vbNewLine
        End If

        'etch
        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVeh.IsMarkedForDelete Then
                For Each oFactor As clsBaseFactor In oVeh.Factors
                    If oFactor.FactorCode.ToUpper = "ETCH" Then
                        sItemsToBeFaxedIn &= "Copy of Proof of Window Etch Discount" & vbNewLine
                        Exit For
                    End If
                Next
            End If
        Next

        If Not IsRewritePolicy(oPolicy) Then
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

            'proof of prior
            If oPolicy.PolicyInsured.DaysLapse > 0 Then
                sItemsToBeFaxedIn &= "Proof of Prior Coverage" & vbNewLine
            End If
        End If

        'premium finance
        For Each oLienHolder As clsEntityLienHolder In oPolicy.LienHolders
            If oLienHolder.EntityType.ToUpper = "PFC" Then
                sItemsToBeFaxedIn &= "Premium Finance Contract" & vbNewLine
                Exit For
            End If
        Next

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.ScholasticHonor And (Not IsRewritePolicy(oPolicy) Or oDrv.IsModified) Then
                sItemsToBeFaxedIn &= "Proof of Scholastic Honor" & vbNewLine
                Exit For
            End If
        Next

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.MatureDriver And (Not IsRewritePolicy(oPolicy) Or oDrv.IsModified) Then
                sItemsToBeFaxedIn &= "Proof of Driving Course" & vbNewLine
                Exit For
            End If
        Next

        If oPolicy.CallingSystem <> "PAS" Then
            With oPolicy
                If .UWQuestions.Count > 0 Then
                    For Each oUWQ As clsUWQuestion In .UWQuestions
                        Select Case oUWQ.QuestionCode
                            Case "301"
                                If Left(oUWQ.AnswerText.ToUpper.Trim, 3) = "YES" And Not IsRewritePolicy(oPolicy) Then
                                    sItemsToBeFaxedIn &= "Signed and dated Physician's Statement by the insured and examining physician" & vbNewLine
                                End If
                        End Select
                    Next
                End If
            End With
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


            If (oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT") And (oPolicy.PolicyInsured.UWTier.Trim <> "92" And oPolicy.PolicyInsured.UWTier.Trim <> "93") Then
                For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                    If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = "TRANSFER" Then
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
                    Case "COMPANION_POLICY"
                        Dim oNote = GetNote(oPolicy, "DIS", "Discount:Companion Flood")
                        If Not oNote Is Nothing Then
                            oPolicy.CompanionFloodCarrierName = "IMPERIAL"
                        End If

                        If oPolicy.CompanionFloodCarrierName <> "" Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
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

    Public Overrides Sub CheckPhysicalDamageRestriction(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

        'If oPolicy.Program.ToUpper <> "SUMMIT" Then
        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            'If VehicleApplies(oVehicle, oPolicy) Then
            sVehicle = CheckPhysicalDamageRestriction(oVehicle)
            If sVehicleList = String.Empty Then
                sVehicleList = sVehicle
            Else
                sVehicleList &= ", " & sVehicle
            End If
            'End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 15 years - " & sVehicleList & ".", "PhysDamageOver15", "IER", oPolicy.Notes.Count))
        End If
        'End If
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
                    If oDrv.MaritalStatus.ToUpper = "SINGLE" And oDrv.Age <= 18 And oDrv.RelationToInsured.ToUpper = "CHILD" Then
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

    Public Sub CheckUnderAgeDriver(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""
            sDrv = CheckUnderAgeDriver(oDrv, oPolicy.Program)

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            If oPolicy.Program.ToUpper = "DIRECT" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state licensing and must be Excluded from coverage or listed with a Learners Permit. - " & sDriverList & ".", "CheckUnderAgeDriver", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state licensing and must be Excluded from coverage or listed with a Learners Permit. - " & sDriverList & ".", "CheckUnderAgeDriver", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckUnderAgeDriver(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            ' Drivers must be at least 16 years of age
            If (oDriver.Age < 16) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) must be at least 16 years of age - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        If oDriver.DriverStatus.ToUpper = "PERMITTED" Then
            ' Drivers must be at least 14 years of age
            If (oDriver.Age < 14) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) must be at least 14 years of age - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function

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

    Public Overrides Function PolicyHasIneligibleRisk(ByVal oPolicy As CorPolicy.clsPolicyPPA) As Boolean

        Dim bIneligibleRisk As Boolean = False
        Dim sReason As String = String.Empty
        Dim iTotalPoints As Integer = 0
        Dim iMaxSymbol As Integer

        'Drivers (New business and new driver added to an existing policy will not be rated or 
        '   written if they fall into one of the categories below.  
        '   Renewal drivers who fall into one of these categories will 
        '   receive and ineligible risk surcharge.)
        If oPolicy.Program.ToUpper() = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Then
            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                CheckViolations(oDriver, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, "B")

                If (oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED") And Not oDriver.IsMarkedForDelete Then

                    '1.     Operators age 15-18 with more than 3 points.
                    If oDriver.Age >= 15 AndAlso oDriver.Age <= 18 Then
                        If oDriver.Points > 3 Then
                            sReason = "Driver age 15-18 with more than 3 points- " & oDriver.IndexNum
                            bIneligibleRisk = True
                            Exit For
                        End If
                    End If

                    '2.     Operators age 19-21 with more than 5 points.
                    If oDriver.Age >= 19 AndAlso oDriver.Age <= 21 Then
                        If oDriver.Points > 5 Then
                            sReason = "Driver age 19-21 with more than 5 points.- " & oDriver.IndexNum
                            bIneligibleRisk = True
                            Exit For
                        End If
                    End If

                    '3.     Operators age 22 and older with more than 15 points.
                    If oDriver.Age >= 22 Then
                        If oDriver.Points > 15 Then
                            sReason = "Driver age 22 and older with more than 15 points.- " & oDriver.IndexNum
                            bIneligibleRisk = True
                            Exit For
                        End If
                    End If

                    '4.     Operators with more than 6 chargeable violations.
                    Dim iNumChargeable As Integer = 0
                    For Each oViol As clsBaseViolation In oDriver.Violations
                        If oViol.Chargeable Then
                            iNumChargeable += 1
                        End If
                    Next

                    If iNumChargeable > 6 Then
                        sReason = "Driver with more than 6 chargeable violations.- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '5.     Any risk with more than 18 driver violation points combined for all drivers. 
                    '       total up for each driver, then check outside the for loop
                    iTotalPoints = iTotalPoints + oDriver.Points

                End If
            Next

            '5.     Any risk with more than 18 driver violation points combined for all drivers. 
            If iTotalPoints > 18 Then
                sReason = "More than 18 driver violation points combined for all drivers"
                bIneligibleRisk = True
            End If
        End If

        If Not bIneligibleRisk Then ' No need to look at vehicles if we already know this is an ineligible risk
            'Replacement Vehicles (The surcharge list applies to replacement vehicles only. 
            '   If a driver wants to add a new vehicle to a policy or the policy is new business, 
            '   we will not write the risk if it falls into one of the categories below.  
            '   Any replacement vehicles that fall under one of these categories will receive an ineligible risk surcharge)

            Dim iBusinessUseCount As Integer = 0
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete AndAlso oVeh.VinNo <> "NONOWNER" Then

                    '1.     Vehicles over 15 years old are unacceptable for all physical damage coverage on new policies. 
                    If oPolicy.Program.ToUpper() <> "SUMMIT" Then
                        'If oVeh.VehicleAge > 15 AndAlso DeterminePhysDamageExists(oVeh) Then
                        If oVeh.VehicleYear < Now.AddYears(-15).Year AndAlso DeterminePhysDamageExists(oVeh) Then
                            sReason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " & oVeh.IndexNum
                            bIneligibleRisk = True
                            Exit For
                        End If
                    End If

                    '2.     Vehicles over 40 years old are unacceptable for all coverages.
                    If oPolicy.Program.ToUpper() <> "SUMMIT" Then
                        If oVeh.VehicleAge > 40 Then
                            sReason = "Vehicles over 40 years old are unacceptable for all coverages.- " & oVeh.IndexNum
                            bIneligibleRisk = True
                            Exit For
                        End If
                    End If

                    '3.     Vehicles with a value over $60,000.
                    iMaxSymbol = GetMaxMSRPSymbol(oVeh.VehicleYear, oPolicy)

                    Dim iVehSymbol As Integer
                    Try
                        If oVeh.PriceNewSymbolCode.Trim = String.Empty Then
                            iVehSymbol = 0
                        Else
                            iVehSymbol = CInt(oVeh.PriceNewSymbolCode.Trim)
                        End If
                    Catch ex As Exception
                        ' if the vehicle was added before we added pricenewsymbolcode it might be an empty string
                        ' if this is the case use vehiclesymbolcode
                        iVehSymbol = 0
                    End Try

                    If CInt(oVeh.VehicleSymbolCode.Trim) = 999 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 965 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 966 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 967 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 968 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 65 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 66 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 67 Or
                       CInt(oVeh.VehicleSymbolCode.Trim) = 68 Then
                        If CInt(oVeh.StatedAmt) > 60000 Then
                            sReason = "Vehicle with a value over $60,000.- " & oVeh.IndexNum
                            bIneligibleRisk = True
                            Exit For
                        End If
                    ElseIf iVehSymbol > iMaxSymbol Then
                        sReason = "Vehicle with a value over $60,000.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '4.     Vehicles rated with physical damage symbol 25 or higher for model years 2010 and older.
                    If oVeh.VehicleYear <= 2010 Then
                        If oVeh.VehicleSymbolCode <> String.Empty Then
                            Try
                                'If CInt(oVeh.VehicleSymbolCode.Trim) >= 25 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
                                If CInt(oVeh.VehicleSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
                                    sReason = "Vehicle with physical damage symbol greater than " & iMaxSymbol.ToString & " - " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            Catch ex As Exception
                            End Try
                        End If
                    End If

                    '5.     Vehicles rated with physical damage symbol 58 or higher for model years 2011 and newer.
                    If oVeh.VehicleYear >= 2011 Then
                        If oVeh.CollSymbolCode <> String.Empty Then
                            Try
                                If CInt(oVeh.CollSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
                                    sReason = "Vehicle with physical damage symbol greater than " & iMaxSymbol.ToString & " - " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            Catch Ex As Exception
                            End Try
                        End If
                    End If

                    '6.     Vehicles garaged out of state.
                    If Not ValidateVehicleZipCode(oVeh.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                        sReason = "Vehicles is garaged out of state.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '7.     More than 1 Business or Artisan use vehicle. 
                    For Each oFactor As clsBaseFactor In oVeh.Factors
                        If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                            iBusinessUseCount = iBusinessUseCount + 1
                            Exit For
                        End If
                    Next

                    '8.     Vehicles that have a title or registration indicating that the vehicle has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.
                    '   (These vehicles can be quoted for BI, PD, UMBI, UIMBI and MED coverages). 
                    If DeterminePhysDamageExists(oVeh) Or oPolicy.Program.ToUpper = "DIRECT" Then
                        For Each uw As clsUWQuestion In oPolicy.UWQuestions
                            If uw.QuestionCode = "307" Then
                                If Left(uw.AnswerText.ToUpper, 3) = "YES" Then
                                    sReason = "Vehicle that has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.- " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End If
            Next

            If Not bIneligibleRisk Then
                If (iBusinessUseCount > 1) Then
                    sReason = "More than 1 Business or Artisan use vehicle."
                    bIneligibleRisk = True
                End If
            End If

        End If

        If bIneligibleRisk Then
            If Not FactorOnPolicy(oPolicy, "INELIGIBLE") Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: A surcharge has been applied to policy due to: " & sReason, "IRSurcharge", "AAF", oPolicy.Notes.Count))
            End If
        End If

        Return bIneligibleRisk

    End Function

    Public Function CheckRestrictedCounty(ByVal oPolicy As CorPolicy.clsPolicyPPA) As String

        Dim bIneligibleRisk As Boolean = False
        Dim sReason As String = String.Empty

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If GetStateInfoValue(oPolicy, oPolicy.Program, "RESTRICTED", "COUNTY", oVeh.County.ToUpper.Trim) = "1" Then

                ' Driver Class = Marital Status, Gender, Age (ex SM45)
                ' 1) Build Driver Class from oDrv 

                For Each oDrv As clsEntityDriver In oPolicy.Drivers

                    If oDrv.DriverStatus.ToUpper = "ACTIVE" Or oDrv.DriverStatus.ToUpper = "PERMITTED" Then
                        Dim sDriverClass As String = String.Empty

                        ' Marital Status || M Married || S Single || W Widowed
                        Select Case oDrv.MaritalStatus.ToUpper.Trim
                            Case "M", "MARRIED"
                                sDriverClass = "M"
                            Case "S", "SINGLE", "W", "WIDOWED"
                                sDriverClass = "S"
                            Case Else
                                sDriverClass = "S"
                        End Select

                        ' Gender
                        Select Case oDrv.Gender.ToUpper.Trim
                            Case "M", "MALE"
                                sDriverClass += "M"
                            Case "F", "FEMALE"
                                sDriverClass += "F"
                            Case Else
                                sDriverClass += "M"
                        End Select

                        'Age(ex SM45)
                        sDriverClass += oDrv.Age.ToString()

                        Dim sItemSubCode = oVeh.County.ToUpper.Trim & "_" & sDriverClass.ToUpper.Trim
                        If GetStateInfoValue(oPolicy, oPolicy.Program, "RESTRICTED", "COUNTY_DRIVERCLASS", sItemSubCode) = "1" Then

                            bIneligibleRisk = True
                            Exit For

                        End If
                    End If
                Next
            End If
        Next
        If bIneligibleRisk Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Please contact Underwriting for this risk. (Code: 112012) ", "RESTRICTEDCOUNTY", "IER", oPolicy.Notes.Count))
        End If

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
End Class
