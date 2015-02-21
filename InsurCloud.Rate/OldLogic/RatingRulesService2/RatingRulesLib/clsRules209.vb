Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data.SqlClient
Imports System.Data
Imports System.Collections.Generic
Imports System.Configuration


Public Class clsRules209
    Inherits clsRules2

    Public Overrides Sub CheckEffectiveDate(ByRef oPolicy As clsPolicyPPA)
        Dim bIsFLImport As Boolean = False

        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.NoteDesc.ToUpper.Trim = "IMPORT" And oNote.SourceCode.ToUpper.Trim = "FLR" Then
                bIsFLImport = True
            End If
        Next

        With oPolicy
            If .PolicyID = "" Then
                If .EffDate < Today.AddDays(1) Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Effective date must be a future date", "PastEffDate", "IER", .Notes.Count))
                ElseIf .EffDate > DateAdd(DateInterval.Day, 30, Today) And Not bIsFLImport Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have an Effective Date more than 30 days in the future", "FutureEffDate", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

#Region "IER Functions"

    Public Sub CheckStatedValue(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If oVehicle.StatedAmt > 0 Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList &= ", " & oVehicle.IndexNum
                    End If
                End If
            End If
        Next

        If sVehicleList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Stated value is not permitted for this program - " & sVehicleList & ".", "StatedVal", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Sub CheckReports(ByVal oPolicy As clsPolicyPPA)
        For Each note As clsBaseNote In oPolicy.Notes
            If note.NoteDesc.Contains("ErrorOrdering") Then
                Dim sTempNote As String = ""
                sTempNote = note.NoteText.Substring(0, note.NoteText.IndexOf("-"))
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: An error occurred during Report Ordering, and we were unable to obtain reports.  Please contact Customer Service for assistance at 888-522-8242. ( " _
                                          & sTempNote & " )", "ReportError", "IER", oPolicy.Notes.Count))
            End If
        Next
    End Sub

    Public Sub CheckClaimActivity(ByVal oPolicy As clsPolicyPPA)
        Dim bHasPriorClaims As Boolean = False
        For Each note As clsBaseNote In oPolicy.Notes
            If note.NoteDesc = "ClaimActivity" Then
                If Integer.Parse(note.NoteText.Split(" ")(0)) >= 3 Then
                    bHasPriorClaims = True
                End If
            End If
        Next
        If bHasPriorClaims Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The number of claims is over the threshold.", "ClaimActivity", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckPIPClaims(ByVal oPolicy As clsPolicyPPA)
        Dim iPIPCount As Integer = 0

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            For Each oViol As clsBaseViolation In oDrv.Violations
                If oViol.ViolCode.Trim.Contains("42110") Then
                    iPIPCount = iPIPCount + 1
                ElseIf oViol.ViolTypeCode.Trim.Contains("42110") Then
                    iPIPCount = iPIPCount + 1
                End If
            Next
        Next
        If iPIPCount > 1 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot have 2 or more PIP claims.", "PIPCount", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckPIPClaims_All(ByVal oPolicy As clsPolicyPPA)
        Dim iPIPCount As Integer = 0
        'Dim dtThreeYearsAgo As DateTime = DateTime.Now.AddYears(-3)
        Dim dtClaimLookBack As DateTime = DateTime.Now.AddYears(-5)

        'IMPROVE 4082: Allow one PIP Claim on the policy for the last 3 years
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            For Each oViol As clsBaseViolation In oDrv.Violations
                If oViol.ViolDate.CompareTo(dtClaimLookBack) > 0 Then
                    If oViol.ViolCode.Trim.Contains("42110") Then
                        iPIPCount = iPIPCount + 1
                    ElseIf oViol.ViolTypeCode.Trim.Contains("42110") Then
                        iPIPCount = iPIPCount + 1
                    End If
                End If
            Next
        Next
        If iPIPCount > 1 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Unacceptable Risk - Does not meet our underwriting criteria.", "PIPCount", "IER", oPolicy.Notes.Count))
        Else
            iPIPCount = 0
            Dim claimDate As New DateTime
            For Each oNote As clsBaseNote In oPolicy.Notes
                claimDate = DateTime.MinValue
                If oNote.NoteDesc.ToUpper.Trim = "PIPCLAIMACTIVITY" Then
                    claimDate = GetClaimDateFromPipClaimActivityNote(oNote.NoteText)
                    If claimDate > DateTime.MinValue Then
                        If claimDate.CompareTo(dtClaimLookBack) > 0 Then
                            iPIPCount = iPIPCount + 1
                        End If
                    End If
                End If
            Next

            If iPIPCount > 1 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Unacceptable Risk - Does not meet our underwriting criteria.", "PIPCount", "IER", oPolicy.Notes.Count))
            End If
        End If

    End Sub

    Private Function GetClaimDateFromPipClaimActivityNote(ByVal note As String) As DateTime
        Dim claimDate As New DateTime
        note = note.Substring(note.IndexOf("PIP LossDate: ") + 14)
        note = note.Substring(0, note.IndexOf(" "))

        DateTime.TryParse(note, claimDate)
        Return claimDate
    End Function

    'Public Sub CheckSuspendedLicense(ByVal oPolicy As clsPolicyPPA)
    '    Dim sDriverList As String = String.Empty

    '    For Each oDrv As clsEntityDriver In oPolicy.Drivers
    '        Dim sDrv As String = ""
    '        sDrv = CheckSuspendedLicense(oDrv, oPolicy.Program)

    '        If Len(sDrv) > 0 Then
    '            If sDriverList = String.Empty Then
    '                sDriverList = sDrv
    '            Else
    '                sDriverList &= ", " & sDrv
    '            End If
    '        End If
    '    Next
    '    If sDriverList <> "" Then
    '        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have a suspended license without filing an SR-22 - " & sDriverList & ".", "SuspendedLic", "IER", oPolicy.Notes.Count))
    '    End If
    'End Sub

    'Public Overridable Function CheckSuspendedLicense(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
    '    Dim sDriverList As String = ""

    '    If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
    '        If Not oDriver.LicenseStatus Is Nothing Then
    '            If oDriver.LicenseStatus.Length > 0 Then
    '                If (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
    '                    If Not oDriver.SR22 Then
    '                        sDriverList = oDriver.IndexNum

    '                        If Not oNoteList Is Nothing Then
    '                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have a suspended license without filing an SR-22 - " & sDriverList & ".", "SuspendedLic", "IER", oNoteList.Count, "AOLE"))
    '                            Return ""
    '                        End If
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If

    '    Return sDriverList
    'End Function

    Public Sub CheckSuspendedLicense(ByVal oPolicy As clsPolicyPPA)
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            Dim sDriverList As String = ""
            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                    If Not oDriver.LicenseStatus Is Nothing Then
                        If oDriver.LicenseStatus.Length > 0 Then
                            If (oDriver.LicenseStatus.ToUpper.Trim = "SUS") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPEND") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
                                If Not oDriver.SR22 Then
                                    sDriverList = oDriver.IndexNum
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have a Suspended driver’s license and do not have an SR-22. _ " & sDriverList & ".", "SuspendedLic", "IER", oPolicy.Notes.Count))

                                End If
                            End If
                        End If
                    End If
                ElseIf (oDriver.DriverStatus.ToUpper = "EXCLUDED") Then
                    If Not oDriver.LicenseStatus Is Nothing Then
                        If oDriver.LicenseStatus.Length > 0 Then
                            If (oDriver.LicenseStatus.ToUpper.Trim = "SUS") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPEND") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
                                'Do nothing
                            End If
                        End If
                    End If
                ElseIf (oDriver.DriverStatus.ToUpper = "NHH") Then
                    If Not oDriver.LicenseStatus Is Nothing Then
                        If oDriver.LicenseStatus.Length > 0 Then
                            If (oDriver.LicenseStatus.ToUpper.Trim = "SUS") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPEND") Or (oDriver.LicenseStatus.ToUpper.Trim = "SUSPENDED") Then
                                sDriverList = oDriver.IndexNum
                                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following NHH driver(s) have a suspended license  - " & sDriverList & ".", "NHHSuspendedLic", "IER", oPolicy.Notes.Count))

                            End If
                        End If
                    End If
                End If
            Next
        End If
    End Sub

    Public Sub CheckRevokedLicense(ByVal oPolicy As clsPolicyPPA)
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            Dim sDriverList As String = String.Empty

            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                Dim sDrv As String = ""
                sDrv = CheckRevokedLicense(oDrv, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            Next
            If sDriverList <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have a Revoked/Cancelled driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "RevokedLic", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckRevokedLicense(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If Not oDriver.LicenseStatus Is Nothing Then
                If oDriver.LicenseStatus.Length > 0 Then
                    If (oDriver.LicenseStatus.ToUpper.Trim = "REV") Or (oDriver.LicenseStatus.ToUpper.Trim = "REVOKED") Or (oDriver.LicenseStatus.ToUpper.Trim = "REVOKED/CANCELED") Then
                        sDriverList = oDriver.IndexNum
                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have a Revoked/Canceled driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "RevokedLic", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckExpiredLicense(ByVal oPolicy As clsPolicyPPA)
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            Dim sDriverList As String = String.Empty

            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                Dim sDrv As String = ""
                sDrv = CheckExpiredLicense(oDrv, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            Next
            If sDriverList <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have an Expired driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "ExpiredLic", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckExpiredLicense(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If Not oDriver.LicenseStatus Is Nothing Then
                If oDriver.LicenseStatus.Length > 0 Then
                    If (oDriver.LicenseStatus.ToUpper.Trim = "EX") Or (oDriver.LicenseStatus.ToUpper.Trim = "EXPIRED") Then
                        sDriverList = oDriver.IndexNum
                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have an Expired driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "ExpiredLic", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckIDOnly(ByVal oPolicy As clsPolicyPPA)
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            Dim sDriverList As String = String.Empty

            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                Dim sDrv As String = ""
                sDrv = CheckIDOnly(oDrv, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            Next
            If sDriverList <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) do not have a Valid driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "IDOnly", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckIDOnly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If Not oDriver.LicenseStatus Is Nothing Then
                If oDriver.LicenseStatus.Length > 0 Then
                    If (oDriver.LicenseStatus = "ID ONLY") Then
                        sDriverList = oDriver.IndexNum
                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) do not have a Valid driver’s license and are unacceptable in this program. -  " & sDriverList & ".", "IDOnly", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function


    Public Sub CheckDriverPoints(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""
            sDrv = CheckDriverPoints(oDrv, oPolicy.Program)

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than 7 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overrides Sub CheckPolicyPoints(ByVal oPolicy As clsPolicyPPA)
        ' override, this doesn't apply to florida
    End Sub

    Public Overridable Function CheckDriverPoints(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If (oDriver.Points > 7) Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 7 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckVehicleAge(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If oVehicle.VehicleAge > 20 Then
                    If Len(sVehicleList) = 0 Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList = sVehicleList & ", " & oVehicle.IndexNum
                    End If
                End If
            End If
        Next

        If Len(sVehicleList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Vehicle age cannot be greater than 20 years.  - " & sVehicleList & ".", "VehicleAgeGT20", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckVehiclePerformanceCode(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If DeterminePhysDamageExists(oVehicle) Then
                    Dim sPerfCode As String = oVehicle.VehiclePerformanceCode.Trim.ToUpper
                    If sPerfCode = "H" Or sPerfCode = "S" Or sPerfCode = "P" Then
                        If Len(sVehicleList) = 0 Then
                            sVehicleList = oVehicle.IndexNum
                        Else
                            sVehicleList = sVehicleList & ", " & oVehicle.IndexNum
                        End If
                    End If
                End If
            End If
        Next

        If Len(sVehicleList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Physical damage is not permitted for High performance vehicles - " & sVehicleList & ".", "VehiclePerformance", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckVehiclePerformanceMidTerm(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If DeterminePhysDamageExists(oVehicle) Then
                    Dim sPerfCode As String = oVehicle.VehiclePerformanceCode.Trim.ToUpper
                    If sPerfCode = "H" Or sPerfCode = "S" Or sPerfCode = "P" Then
                        If Not DeterminePhysDamageMaxDeductible(oVehicle) Then
                            If Len(sVehicleList) = 0 Then
                                sVehicleList = oVehicle.IndexNum
                            Else
                                sVehicleList = sVehicleList & ", " & oVehicle.IndexNum
                            End If
                        End If
                    End If
                End If
            End If
        Next

        If Len(sVehicleList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: High performance vehicles must have $1,000 Comp/Coll deductibles - " & sVehicleList & ".", "HighPerfDed", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Function CheckVehiclePerformanceMidTerm(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = String.Empty

        If DeterminePhysDamageExists(oVehicle) Then
            Dim sPerfCode As String = oVehicle.VehiclePerformanceCode.Trim.ToUpper
            If sPerfCode = "H" Or sPerfCode = "S" Or sPerfCode = "P" Then
                If Not DeterminePhysDamageMaxDeductible(oVehicle) Then
                    If Len(sVehicleList) = 0 Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList = sVehicleList & ", " & oVehicle.IndexNum
                    End If
                End If
            End If
        End If

        If Len(sVehicleList) > 0 Then
            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: High performance vehicles must have $1,000 Comp/Coll deductibles - " & sVehicleList & ".", "HighPerfDed", "IER", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End If

        Return ""
    End Function

    Public Function DeterminePhysDamageMaxDeductible(ByVal oVehicle As clsVehicleUnit) As Boolean
        Dim bMaxDeductible As Boolean = True

        ' Check if this is a Physical Damage Policy (i.e. If there are COMP/COLL coverage)
        For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
            If (oCoverage.CovCode.Contains("OTC") Or oCoverage.CovCode.Contains("COL")) And Not oCoverage.IsMarkedForDelete Then
                If Convert.ToInt32(oCoverage.CovDeductible) < 1000 Then
                    bMaxDeductible = False
                    Exit For
                End If
            End If
        Next

        Return bMaxDeductible
    End Function


    Public Sub CheckLicenseStateDate(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        Dim sDriverDOBList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                If Not oDrv.IsMarkedForDelete Then
                    If Len(oDrv.LicenseStateDate) = 0 Or oDrv.LicenseStateDate = "#12:00:00 AM#" Then
                        If Len(sDriverList) = 0 Then
                            sDriverList = oDrv.IndexNum
                        Else
                            sDriverList = sDriverList & ", " & oDrv.IndexNum
                        End If
                    Else
                        If CDate(oDrv.LicenseStateDate) < CDate(oDrv.DOB) Then
                            If Len(sDriverDOBList) = 0 Then
                                sDriverDOBList = oDrv.IndexNum
                            Else
                                sDriverDOBList = sDriverDOBList & ", " & oDrv.IndexNum
                            End If
                        End If
                    End If
                End If
            End If
        Next

        If Len(sDriverList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Date Licensed is Required for driver(s).  - " & sDriverList & ".", "DateLicensed", "IER", oPolicy.Notes.Count))
        End If

        If Len(sDriverDOBList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Date Licensed cannot be less than the driver's date of  birth.  - " & sDriverDOBList & ".", "DateLicensedDOB", "IER", oPolicy.Notes.Count))
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state licensing - " & sDriverList & ".", "MinDriverAge", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckBusinessUseDriverAge(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        Dim bHasBusinessUse As Boolean = False

        ' Check to see if any vehicle has the business use factor
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                For Each oFactor As clsBaseFactor In oVehicle.Factors
                    If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                        bHasBusinessUse = True
                        Exit For
                    End If
                Next
            End If
        Next

        ' If at least one vehicle has a business use factor
        ' check to see if any males under 25 or females under 21
        If bHasBusinessUse Then
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                Dim sDrv As String = ""

                If oDrv.DriverStatus.ToUpper.Trim = "ACTIVE" And Not oDrv.IsMarkedForDelete Then
                    If oDrv.Gender.ToUpper.StartsWith("M") Then
                        ' no males < 25
                        If oDrv.Age < 25 Then
                            sDrv = oDrv.IndexNum
                        End If
                    Else
                        ' No females < 21
                        If oDrv.Age < 21 Then
                            sDrv = oDrv.IndexNum
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
        End If

        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: No males under 25 or females under 21 are permitted when business use is selected for a vehicle - " & sDriverList & ".", "BusinessUseDrvAge", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckCarToDriverRatio(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        Dim iVehicleCount As Integer = 0
        Dim iDriverCount As Integer = 0

        iVehicleCount = oPolicy.VehicleCount(True)

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                iDriverCount = iDriverCount + 1
            End If
        Next

        If iVehicleCount > iDriverCount Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot have more cars than drivers.", "CartoDrvRation", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckDWI36(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        Dim oRatingRules As New CommonRulesFunctions

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.DriverStatus.ToUpper.Trim = "ACTIVE" And Not oDrv.IsMarkedForDelete Then
                Dim sDrv As String = ""
                For Each oViol As clsBaseViolation In oDrv.Violations
                    If oViol.ViolGroup.ToUpper.Trim = "DWI" Then
                        If oRatingRules.CalculateViolAge(oViol.ViolDate, oPolicy.EffDate) < 36 Then
                            sDrv = oDrv.IndexNum
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
            End If
        Next

        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot have a DWI Claim in the past 36 months- " & sDriverList & ".", "DWI36", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckHigherLimits(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        Dim bHasHigherLimits As Boolean = False

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVeh.IsMarkedForDelete Then
                For Each oCov As clsBaseCoverage In oVeh.Coverages
                    If oCov.CovGroup = "BI" Then
                        If oCov.CovLimit.Trim <> "10/20" Then
                            bHasHigherLimits = True
                            Exit For
                        End If
                    ElseIf oCov.CovGroup = "PD" Then
                        If oCov.CovLimit.Trim <> "10" Then
                            bHasHigherLimits = True
                            Exit For
                        End If
                    ElseIf oCov.CovGroup = "UM" Then
                        If oCov.CovLimit <> "10/20" Then
                            bHasHigherLimits = True
                            Exit For
                        End If
                    End If
                Next
            End If
        Next

        If bHasHigherLimits Then
            Dim sDrv As String = ""
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If oDrv.Age < 25 Or oDrv.Points > 3 Then
                    sDrv = oDrv.IndexNum

                    If Len(sDrv) > 0 Then
                        If sDriverList = String.Empty Then
                            sDriverList = sDrv
                        Else
                            sDriverList &= ", " & sDrv
                        End If
                    End If
                End If
            Next
        End If


        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: All drivers must be 25 or older and have no more than 3 points if higher BI/PD/UM limits are selected- " & sDriverList & ".", "HighLimits", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckVehicleMake(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim dtRestrictedMakes As DataTable
        dtRestrictedMakes = GetRestrictedVehicleModels(oPolicy, "MAKE")

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
                If bPhysDamage Then
                    For Each oMake As DataRow In dtRestrictedMakes.Rows
                        Dim sMake As String = oMake("EditValue")

                        If sMake.ToUpper.Trim = oVehicle.VehicleMakeCode.ToUpper.Trim Then
                            ' Add IER
                            Dim sVeh As String
                            sVeh = oVehicle.IndexNum & ":" & sMake

                            If Len(sVeh) > 0 Then
                                If sVehicleList = String.Empty Then
                                    sVehicleList = sVeh
                                Else
                                    sVehicleList &= ", " & sVeh
                                End If
                            End If

                        End If
                    Next
                End If
            End If
        Next

        If sVehicleList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Physical Damage is not permitted for this vehicle make - " & sVehicleList & ".", "RestrictedMake", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckVehicleModel(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim dtRestrictedModels As DataTable
        dtRestrictedModels = GetRestrictedVehicleModels(oPolicy, "MODEL")

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
                If bPhysDamage Then
                    For Each oModel As DataRow In dtRestrictedModels.Rows
                        Dim sModel As String = oModel("EditValue")

                        If sModel.ToUpper.Trim = oVehicle.VehicleModelCode.ToUpper.Trim Or oVehicle.VehicleModelCode.ToUpper.Trim.StartsWith(sModel.ToUpper.Trim) Then
                            ' Add IER
                            Dim sVeh As String
                            sVeh = oVehicle.IndexNum & ":" & sModel

                            If Len(sVeh) > 0 Then
                                If sVehicleList = String.Empty Then
                                    sVehicleList = sVeh
                                Else
                                    sVehicleList &= ", " & sVeh
                                End If
                            End If

                        End If
                    Next
                End If
            End If
        Next

        If sVehicleList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Physical Damage is not permitted for this vehicle Model - " & sVehicleList & ".", "RestrictedModel", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckAlwaysRestrictedVehicleModel(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim dtRestrictedModels As DataTable
        dtRestrictedModels = GetAlwaysRestrictedVehicleModels(oPolicy, "MODEL")

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                For Each oModel As DataRow In dtRestrictedModels.Rows
                    Dim sModel As String = oModel("EditValue")

                    If sModel.ToUpper.Trim = oVehicle.VehicleModelCode.ToUpper.Trim Then
                        ' Add IER
                        Dim sVeh As String
                        sVeh = oVehicle.IndexNum & ":" & sModel

                        If Len(sVeh) > 0 Then
                            If sVehicleList = String.Empty Then
                                sVehicleList = sVeh
                            Else
                                sVehicleList &= ", " & sVeh
                            End If
                        End If

                    End If
                Next
            End If
        Next

        If sVehicleList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: This vehicle Model is not permitted - " & sVehicleList & ".", "RestrictedModel", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    ' restricted wether or not they have physical damange
    Private Function GetAlwaysRestrictedVehicleModels(ByVal oPolicy As clsPolicyPPA, ByVal sRestrictionType As String) As DataTable

        Dim dtRatingRules As New DataTable
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String

        Try
            sSql = "  SELECT EditValue "
            sSql &= " FROM pgm2" & oPolicy.StateCode & "..EditCode with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= "    AND Category = 'ALWAYSRESTRICTED' "
            sSql &= "    AND SubCategory = 'VEHICLE' "
            sSql &= "    AND EditCode =@RestrictionType "
            sSql &= "    AND EffDate <=@RateDate "
            sSql &= "    AND ExpDate > @RateDate "

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 11).Value = oPolicy.Program
                cmd.Parameters.Add("@RestrictionType", SqlDbType.VarChar, 50).Value = sRestrictionType
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtRatingRules)
            End Using

            oConn.Close()

        Catch ex As Exception
        End Try

        Return dtRatingRules


    End Function

    ' Restricted only if they have phys damage
    Private Function GetRestrictedVehicleModels(ByVal oPolicy As clsPolicyPPA, ByVal sRestrictionType As String) As DataTable

        Dim dtRatingRules As New DataTable
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String

        Try
            sSql = "  SELECT EditValue "
            sSql &= " FROM pgm2" & oPolicy.StateCode & "..EditCode with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= "    AND Category = 'RESTRICTED' "
            sSql &= "    AND SubCategory = 'VEHICLE' "
            sSql &= "    AND EditCode =@RestrictionType "
            sSql &= "    AND EffDate <=@RateDate "
            sSql &= "    AND ExpDate > @RateDate "

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 11).Value = oPolicy.Program
                cmd.Parameters.Add("@RestrictionType", SqlDbType.VarChar, 50).Value = sRestrictionType
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtRatingRules)
            End Using

            oConn.Close()

        Catch ex As Exception
        End Try

        Return dtRatingRules


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

    Public Sub CheckPhysicalDamageRestrictionConv(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                sVehicle = CheckPhysicalDamageRestrictionConv(oVehicle)
                If sVehicleList = String.Empty Then
                    sVehicleList = sVehicle
                Else
                    sVehicleList &= ", " & sVehicle
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are convertibles- " & sVehicleList & ".", "PhysDamageConv", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Function CheckPhysicalDamageRestrictionConv(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleID As String = ""

        Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
        If bPhysDamage Then
            If oVehicle.VehicleBodyStyleCode.Trim = "CONV  2D" Or oVehicle.VehicleBodyStyleCode.Trim = "CONVRTBL" Or _
             oVehicle.VehicleBodyStyleCode.Trim = "CONVHDTP" Or oVehicle.VehicleBodyStyleCode.Trim = "CONVRTB" Or _
             oVehicle.VehicleBodyStyleCode.Trim = "CONV/CPE" Or oVehicle.VehicleBodyStyleCode.Trim = "CONVTIBL" Then
                sVehicleID = oVehicle.IndexNum
                If oNoteList Is Nothing Then
                    Return sVehicleID
                Else
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are convertibles - " & sVehicleID & ".", "PhysDamageConv", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If
        Return sVehicleID
    End Function

    Public Sub CheckPhysicalDamageRestrictionAWDPickup(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                sVehicle = CheckPhysicalDamageRestrictionAWDPickup(oVehicle)
                If sVehicleList = String.Empty Then
                    sVehicleList = sVehicle
                Else
                    sVehicleList &= ", " & sVehicle
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are 4x4 pickups  - " & sVehicleList & ".", "PhysDamageAWDPick", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Function CheckPhysicalDamageRestrictionAWDPickup(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleID As String = ""

        Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
        If bPhysDamage Then
            If oVehicle.VehicleBodyStyleCode = "PKP4X44D" Or oVehicle.VehicleBodyStyleCode = "PKP4X42D" _
             Or oVehicle.VehicleBodyStyleCode = "PKUP4X4D" Or oVehicle.VehicleBodyStyleCode = "PKUP4X4D" _
             Or oVehicle.VehicleBodyStyleCode = "PKP  4X4" Or oVehicle.VehicleBodyStyleCode = "PKUP 4X4" _
             Or oVehicle.VehicleBodyStyleCode = "PKP4X43D" Then
                sVehicleID = oVehicle.IndexNum
                If oNoteList Is Nothing Then
                    Return sVehicleID
                Else
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are 4x4 pickups - " & sVehicleID & ".", "PhysDamageAWDPick", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If
        Return sVehicleID
    End Function

    ' Physical Damage Restriction 2 (Any vehicle over 20 years old.)
    Public Overrides Sub CheckPhysicalDamageRestriction(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                sVehicle = CheckPhysicalDamageRestriction(oVehicle)
                If sVehicleList = String.Empty Then
                    sVehicleList = sVehicle
                Else
                    sVehicleList &= ", " & sVehicle
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 20 years - " & sVehicleList & ".", "PhysDamageOver15", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overrides Function CheckPhysicalDamageRestriction(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleID As String = ""

        Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
        If bPhysDamage Then
            If oVehicle.VehicleYear < Now.AddYears(-20).Year Then
                sVehicleID = oVehicle.IndexNum
                If oNoteList Is Nothing Then
                    Return sVehicleID
                Else
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 20 years - " & sVehicleID & ".", "PhysDamageOver15", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If
        Return sVehicleID
    End Function

    Public Overrides Sub CheckNonOwner(ByVal oPolicy As clsPolicyPPA)
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

        If bIsNonOwner Or iVehicleCount < 1 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Non-owners risks are not acceptible", "NonOwner", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Overridable Sub CheckPOBox(ByVal oPolicy As clsPolicyPPA)
        Dim sAddress As String = String.Empty

        If oPolicy.PolicyInsured.Address1.Length > 0 Then
            sAddress = Replace(oPolicy.PolicyInsured.Address1, ".", "")
        End If

        sAddress = sAddress.ToUpper
        'Dim sMailingAddress As String = String.Empty

        'If oPolicy.PolicyInsured.MailingAddrDiff Then
        '    If oPolicy.PolicyInsured.MailingAddress1.Length > 0 Then
        '        sMailingAddress = Replace(oPolicy.PolicyInsured.MailingAddress1, ".", "")
        '    End If
        '    sMailingAddress = sMailingAddress.ToUpper
        'End If

        If sAddress.Contains("PO") And sAddress.Contains("BOX") Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: PO Boxes are unacceptable", "POBox", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Overridable Sub CheckAddress(ByVal oPolicy As clsPolicyPPA)

        If oPolicy.PolicyInsured.MailingAddrDiff Then
            If oPolicy.PolicyInsured.MailingState.ToUpper.Trim <> "FL" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Mailing address outside of Florida is unacceptible", "MailingState", "IER", oPolicy.Notes.Count))
            End If
        End If

        Dim dtValidZips As DataTable
        dtValidZips = GetValidZips(oPolicy)

        Dim bValidZip As Boolean = False
        For Each oZip As DataRow In dtValidZips.Rows
            Dim sZip = oZip("zip").ToString
            If sZip = oPolicy.PolicyInsured.Zip Then
                bValidZip = True
                Exit For
            End If
        Next

        If Not bValidZip Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Invalid Florida zip code", "FLZip", "IER", oPolicy.Notes.Count))
        End If


    End Sub


    Public Sub CheckActualCashValue(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                sVehicle = CheckActualCashValue(oVehicle)
                If sVehicleList = String.Empty Then
                    sVehicleList = sVehicle
                Else
                    sVehicleList &= ", " & sVehicle
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an Actual Cash Value over $45,000 with physical damage coverage - " & sVehicleList & ".", "PhysDamageACV", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overrides Sub CheckMSRPRestriction(ByRef oPolicy As clsPolicyPPA)
        ' not used for florida
    End Sub

    Public Function CheckActualCashValue(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleID As String = ""

        Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
        If bPhysDamage Then
            If oVehicle.StatedAmt > 45000 Then
                sVehicleID = oVehicle.IndexNum
                If oNoteList Is Nothing Then
                    Return sVehicleID
                Else
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have an Actual Cash Value over $45,000 with physical damage coverage - " & sVehicleID & ".", "PhysDamageACV", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If
        Return sVehicleID
    End Function


    ' Restricted only if they have phys damage
    Private Function GetValidZips(ByVal oPolicy As clsPolicyPPA) As DataTable

        Dim dtRatingRules As New DataTable
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String

        Try
            sSql = " select distinct zip from pgm209..codeterritorydefinitions"

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtRatingRules)
            End Using

            oConn.Close()

        Catch ex As Exception
        End Try

        Return dtRatingRules


    End Function

    Public Overrides Sub CheckPayPlan(ByRef oPolicy As clsPolicyPPA)
        If Not ValidatePayPlan(oPolicy) Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The policy has an invalid pay plan. Please make sure a valid pay plan is selected.", "InvalidPayPlan", "IER", oPolicy.Notes.Count))
        End If

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                If oDrv.SR22CaseCode.StartsWith("2") Then
                    If Not oPolicy.PayPlanCode = "100" Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: SR-22 Case is non-cancelable and must be paid in full", "SR22PIF", "IER", oPolicy.Notes.Count))
                    End If
                End If
            End If
        Next
    End Sub


    Public Sub CheckSR22Requirements(ByRef oPolicy As clsPolicyPPA)
        ' Must have BI, no excluded Drivers
        Dim bHasSR22 As Boolean = False
        Dim bHasBI As Boolean = False
        Dim bHasExcludedDriver As Boolean = False

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                If oDrv.SR22 Then
                    bHasSR22 = True
                End If
            End If

            If oDrv.DriverStatus.ToUpper = "EXCLUDED" Then
                bHasExcludedDriver = True
            End If
        Next

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
                    If (oCoverage.CovCode.Contains("BI") And Not oCoverage.IsMarkedForDelete) Then
                        bHasBI = True
                        Exit For
                    End If
                Next
            End If
        Next

        If bHasSR22 And (bHasExcludedDriver Or Not bHasBI) Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Must have BI Coverage and no excluded drivers with SR-22 ", "SR22Reqs", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Sub CheckSymbol(ByRef oPolicy As clsPolicyPPA)

        Dim sVehicleList As String = ""
        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) And DeterminePhysDamageExists(oVehicle) Then

                Dim sVeh As String = ""
                sVeh = CheckSymbol(oVehicle, oPolicy.Program)

                If Len(sVeh) > 0 Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = sVeh
                    Else
                        sVehicleList &= ", " & sVeh
                    End If
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a symbol greater than 22/49 - " & sVehicleList & ".", "SymbolOver22", "IER", oPolicy.Notes.Count))
        End If
    End Sub


    Public Overrides Sub CheckActualRateDate(ByVal oPolicy As clsPolicyPPA)

        Dim bShow As Boolean = True

        ' Do not require/show order reports  for RAC imported policies
        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.NoteDesc.ToUpper.Trim = "IMPORT" And oNote.SourceCode.ToUpper.Trim = "FLR" Then
                bShow = False
            End If
        Next

        If bShow Then
            With oPolicy
                If oPolicy.ActualRateDate <> Date.MinValue Then
                    If oPolicy.ActualRateDate.AddDays(1) < Now() Then
                        oPolicy.ActualRateDate = Now()
                        .Notes = (AddNote(.Notes, "Information Updated: Quote is over 1 day old, prior rate is no longer valid and has been updated.", "ActualRateDate", "RPT", .Notes.Count))
                    End If
                End If
            End With
        End If


    End Sub


    Public Overridable Function CheckSymbol(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If oVehicle.VehicleSymbolCode <> String.Empty And DeterminePhysDamageExists(oVehicle) Then
            Try
                If oVehicle.VehicleYear < 2011 Then
                    If CInt(oVehicle.VehicleSymbolCode.Trim) > 22 And oVehicle.VinNo.ToUpper <> "NONOWNER" And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 65 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 66 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 67 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 68 Then
                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than 22 - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                Else
                    If CInt(oVehicle.VehicleSymbolCode.Trim) > 49 And oVehicle.VinNo.ToUpper <> "NONOWNER" And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 965 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 966 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 967 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 968 Then
                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than 49 - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            Catch Ex As Exception
                sVehicleList = oVehicle.IndexNum
                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a non-numeric symbol - " & sVehicleList & ".", "SymbolNonNumeric", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End Try
        End If

        Return sVehicleList
    End Function

    Public Overridable Sub CheckSalvagedVehicle(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            Dim bHasPhysicalDamage As Boolean = False
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVehicle.IsMarkedForDelete Then
                    If DeterminePhysDamageExists(oVehicle) Then
                        bHasPhysicalDamage = True
                        Exit For
                    End If
                End If
            Next

            If bHasPhysicalDamage Then
                For Each uw As clsUWQuestion In .UWQuestions
                    If uw.QuestionCode = "332" Then
                        If uw.AnswerCode = "001" Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Salvaged Vehicle with Physical Damage coverage is not permitted.", "SalvagePD", "IER", .Notes.Count))
                        End If
                    End If
                Next
            End If
        End With
    End Sub


    Public Overridable Sub Check12MonthTerm(ByVal oPolicy As clsPolicyPPA)

        Dim bShow As Boolean = True

        ' Do not require/show order reports  for RAC imported policies
        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.NoteDesc.ToUpper.Trim = "IMPORT" And oNote.SourceCode.ToUpper.Trim = "FLR" Then
                bShow = False
            End If
        Next

        If bShow Then
            With oPolicy
                If .Term = 12 And bShow Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: 12 month term is temporarily unavailable.", "12MTRM", "IER", .Notes.Count))
                End If
            End With
        End If
    End Sub

    Public Sub CheckPriorInsurance(ByVal oPolicy As clsPolicyPPA)

        ' If no Prior Insurance, then ineligible risk
        If oPolicy.PolicyInsured.PriorLimitsCode = "0" Or oPolicy.PriorCarrierName.Trim = "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Policy with no Prior Coverage is an ineligible risk.", "PriorInsurance", "IER", oPolicy.Notes.Count))
        End If

        If oPolicy.PriorCarrierName.ToUpper = "EQUITY" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Prior Carrier is an ineligible risk.", "PriorInsurance", "IER", oPolicy.Notes.Count))
        ElseIf oPolicy.PriorCarrierName.StartsWith("Other:") Then
            If oPolicy.PriorCarrierName.ToUpper.Contains("EQUITY") Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Prior Carrier is an ineligible risk.", "PriorInsurance", "IER", oPolicy.Notes.Count))
            End If
        End If

    End Sub

    Public Overridable Function DriverLicenseStatusIsPermit(ByVal oDriver As clsEntityDriver) As Boolean
        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If Not oDriver.LicenseStatus Is Nothing Then
                If oDriver.LicenseStatus.Length > 0 Then
                    If (oDriver.LicenseStatus.ToUpper.Trim = "PERMIT") Then
                        Return True
                    End If
                End If
            End If
        End If

        Return False
    End Function

    Public Sub CheckAPANotOrderedForAllDrivers(ByVal oPolicy As clsPolicyPPA)
        Dim cloneNotes As New List(Of clsBaseNote)(oPolicy.Notes)

        For Each note As clsBaseNote In cloneNotes
            If note.SourceCode.ToUpper.Trim = "RPT" AndAlso note.NoteDesc.ToUpper.Trim = "APANOTORDEREDMORETHAN5DRIVERS" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Please contact Customer Service at (305) 260-3600. Reports not Ordered (>5).", "APANotOrderedForAllDrivers", "IER", oPolicy.Notes.Count))
                Exit For
            End If
        Next
    End Sub

#End Region

#Region "UWW Functions"
    Public Overridable Sub CheckMedicalClaims(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            For Each uw As clsUWQuestion In .UWQuestions
                If uw.QuestionCode = "323" Then
                    If uw.AnswerCode = "001" Then
                        .Notes = (AddNote(.Notes, "Underwriting Approval Needed: PIP Claim within the last three years.", "PIPClaim", "UWW", .Notes.Count))
                    End If
                End If
            Next
        End With

        Dim iPIPCount As Integer = 0

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            For Each oViol As clsBaseViolation In oDrv.Violations
                If oViol.ViolCode.Trim.Contains("42110") Then
                    iPIPCount = iPIPCount + 1
                ElseIf oViol.ViolTypeCode.Trim.Contains("42110") Then
                    iPIPCount = iPIPCount + 1
                End If
            Next
        Next
        If iPIPCount >= 1 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: PIP claim within the last three years.", "PIPClaim", "UWW", oPolicy.Notes.Count))
        End If
    End Sub
#End Region

    Public Overrides Sub AddExclFactor(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String)
        Dim bHasExcl As Boolean = False

        If ExclFactorType = "VEHICLE" Then
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                For i As Integer = oVeh.Factors.Count - 1 To 0 Step -1
                    If oVeh.Factors.Item(i).FactorCode.ToUpper = "EXCL" Then
                        oVeh.Factors.RemoveAt(i)
                    End If
                Next
            Next
        Else
            For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = "EXCL" Then
                    oPolicy.PolicyFactors.RemoveAt(i)
                End If
            Next
        End If

        ' Determine if we need to apply the Excluded Driver Factor
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete Then
                If oDrv.IndexNum < 98 Then
                    If oDrv.DriverStatus.ToUpper = "EXCLUDED" Then
                        bHasExcl = True
                        Exit For
                    End If
                End If
            End If
        Next

        If bHasExcl Then
            If ExclFactorType = "VEHICLE" Then
                For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                    AddVehicleFactor(oPolicy, oVeh, sFactorCode)
                Next
            Else
                If Not FactorOnPolicy(oPolicy, sFactorCode) Then
                    AddPolicyFactor(oPolicy, sFactorCode)
                End If
            End If
        End If

    End Sub

    Public Overrides Sub RemoveNOVIOLIfINEXPFactor(ByVal oPolicy As clsPolicyPPA)

        Dim iNoViol As Integer = 0
        Dim iInExp As Integer = 0
        For Each oDrv As CorPolicy.clsEntityDriver In oPolicy.Drivers
            iNoViol = -1
            iInExp = -1
            For i As Integer = oDrv.Factors.Count - 1 To 0 Step -1
                If oDrv.Factors.Item(i).FactorCode.ToUpper.Contains("NO_VI") Then
                    iNoViol = i
                ElseIf oDrv.Factors.Item(i).FactorCode.ToUpper.Contains("INEXP") Then
                    iInExp = i
                End If
                If iNoViol >= 0 And iInExp >= 0 Then
                    oDrv.Factors.RemoveAt(iNoViol)
                    Exit For
                End If
            Next
        Next

        Dim bHasSR22 As Boolean = False
        For Each oDrv As CorPolicy.clsEntityDriver In oPolicy.Drivers
            If oDrv.SR22 Then
                bHasSR22 = True
                Exit For
            End If
        Next

        For Each oDrv As CorPolicy.clsEntityDriver In oPolicy.Drivers
            iNoViol = -1
            If bHasSR22 Then
                For i As Integer = oDrv.Factors.Count - 1 To 0 Step -1
                    If oDrv.Factors.Item(i).FactorCode.ToUpper.Contains("NO_VI") Then
                        iNoViol = i
                    End If

                    If iNoViol >= 0 Then
                        oDrv.Factors.RemoveAt(iNoViol)
                        Exit For
                    End If
                Next
            End If
        Next


    End Sub

    ' Require underwriting approval when Agents add BI mid-term
    Public Sub CheckBIAdded(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            If oPolicy.TransactionNum > 1 Then
                For Each oCov As clsBaseCoverage In .VehicleUnits(0).Coverages
                    If oCov.CovGroup = "BI" Then
                        If oCov.IsNew Then
                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Please contact Imperial for approval to add Bodily Injury to this policy", "AddBI", "UWW", .Notes.Count))
                        End If
                    End If
                Next
            End If
        End With
    End Sub
    Private Function HasForeignLicense(ByVal oDriver As clsEntityDriver) As Boolean

        Select Case oDriver.DLNState
            Case "FN", "IT", "VI", "AS", "FM", "GU", "MH", "MP", "PR", "PW", "ON", "AE", "AP", "AA", "JZ"
                Return True
            Case Else
                Return False
        End Select
    End Function


    Public Sub CheckInternationalOutOfStateDL(ByRef oPolicy As clsPolicyPPA)
        'Improve Item 2876
        Dim bNotFlorida As Boolean = False
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.DLNState <> "FL" Then
                    bNotFlorida = True
                End If
            End If
        Next
        If bNotFlorida Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Not all drivers have a Florida license.", "InternationalOutOfStateDL", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Overrides Sub AddAutoApplyFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Call MyBase.AddAutoApplyFactors(oPolicy)

            Dim bIsInexperiencedDriverFact As Boolean = False   '...vG
            Dim oInexperiencedViolation As clsBaseViolation = Nothing

            'if the program does not have a Foreign License factor then we need to check to see if we need to add a UDR viol
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If Not oDrv.IsMarkedForDelete Then
                    If oDrv.IndexNum < 98 Then
                        If HasForeignLicense(oDrv) Then
                            If ApplyUDR(oPolicy, oDrv) Then
                                AddViolation(oPolicy, oDrv, "55559", "Unable to obtain Motor Vehicle Record for past 36 months for any reason", "V", "MED", "M", oPolicy.EffDate)
                            End If
                        End If
                        If DriverLicenseStatusIsPermit(oDrv) And Not ApplyUDR(oPolicy, oDrv) Then
                            AddInexperiencedViolation(oPolicy, oDrv)
                        Else
                            AddInexperiencedViolationPerLicenseIssueDate(oPolicy, oDrv)
                        End If

                        If FactorOnDriver(oDrv, "INEXPERIENCED") And Not bIsInexperiencedDriverFact Then   '...vG
                            bIsInexperiencedDriverFact = True
                        End If
                        If Not ApplyUDR(oPolicy, oDrv) Then  '...vG Remove Inexp Viol if UDR Viol is applied
                            For Each oViol As clsBaseViolation In oDrv.Violations
                                If oViol.ViolCode = "99999" OrElse oViol.ViolTypeCode = "99999" Then
                                    'RemoveInexperiencedViolation(oPolicy, oDrv)
                                    oInexperiencedViolation = oViol
                                End If
                            Next
                            If oInexperiencedViolation IsNot Nothing Then
                                oDrv.Violations.Remove(oInexperiencedViolation)
                            End If
                        End If
                    End If
                End If
            Next

            If bIsInexperiencedDriverFact Then      '...vG Remove Inexp Driver Surcharge in FL
                RemoveNOVIOLIfINEXPFactor(oPolicy)
                RemoveINEXPFactor(oPolicy)
            End If

            ApplySafeDriverRules(oPolicy)


        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
        End Try

    End Sub

    Private Sub AddInexperiencedViolationPerLicenseIssueDate(ByVal policy As clsPolicyPPA, ByVal driver As clsEntityDriver)

        If driver.DriverStatus.ToUpper = "ACTIVE" Then
            If driver.LicenseIssueDate <> Date.MinValue Then
                Dim driverAddedDate As Date = policy.EffDate
                If driver.AddedDate > policy.EffDate Then
                    driverAddedDate = driver.AddedDate
                End If

                Dim monthDiff As Integer = DateDiff("m", CDate(driver.LicenseIssueDate), CDate(driverAddedDate))

                ' DateDiff fix:  i.e. if ratedate is 1/15/2009 and license date is 1/30/2008
                ' that isn't a full 12 months, so need to subtract off one month
                If CDate(driver.LicenseIssueDate).Month = CDate(driverAddedDate).Month Then
                    If CDate(driverAddedDate).Day < CDate(driver.LicenseIssueDate).Day Then
                        monthDiff = monthDiff - 1
                    End If
                End If

                If monthDiff < 36 Then
                    If Not FactorOnDriver(driver, "INEXPERIENCED") Then
                        AddDriverFactor(policy, driver, "INEXPERIENCED")
                    End If
                    AddInexperiencedViolation(policy, driver)
                End If
            End If
        End If

    End Sub


    Private Function ApplyUDR(ByVal oPolicy As clsPolicyPPA, ByVal oDrv As clsEntityDriver)

        Dim bApply As Boolean = True
        For Each oViol As clsBaseViolation In oDrv.Violations
            If oViol.ViolGroup.ToUpper = "MED" And oViol.ViolTypeCode = "55559" Then
                bApply = False
                Exit For
            End If
        Next

        Return bApply
    End Function
    'Public Overrides Function GetNoViolDiscount(ByVal oDrv As clsEntityDriver, ByVal dtEffDate As Date) As Integer
    '    Dim oRatingRules As New RatingRulesService2
    '    Dim iDiscountToAdd As Integer = -1

    '    Dim iPts36 As Integer = 0
    '    Dim iPts18 As Integer = 0
    '    Dim iPts12 As Integer = 0


    '    ' Next check to see if there were any chargeable accidents or serious infractions
    '    ' in the past x months (none allowed to qualify for discount)
    '    For Each oViol As clsBaseViolation In oDrv.Violations
    '        Dim iTempMonthsOld As Integer = 0
    '        iTempMonthsOld = oRatingRules.CalculateViolAge(oViol.ViolDate, dtEffDate)

    '        If iTempMonthsOld < 0 Then
    '            iTempMonthsOld = 0
    '        End If

    '        ' Check to see if this is a serious violation or chargeable accident
    '        ' If the violation was within the past 18 months
    '        If iTempMonthsOld < 12 Then
    '            iPts12 += oViol.Points
    '        ElseIf iTempMonthsOld < 18 Then
    '            iPts18 += oViol.Points
    '        ElseIf iTempMonthsOld < 36 Then
    '            iPts36 += oViol.Points
    '        End If
    '    Next

    '    ' For the 36 month discount, all 3 must be at 0
    '    Dim bDiscountAdded As Boolean = False
    '    If iPts36 + iPts18 + iPts12 < 2 And Not bDiscountAdded Then
    '        ' give 36 month discount
    '        iDiscountToAdd = 36
    '        bDiscountAdded = True
    '    End If

    '    If iPts18 + iPts12 < 2 And Not bDiscountAdded Then
    '        ' Give the 18 month discount
    '        iDiscountToAdd = 18
    '        bDiscountAdded = True
    '    End If

    '    If iPts12 < 2 And Not bDiscountAdded Then
    '        ' Give the 12 month discount
    '        iDiscountToAdd = 12
    '        bDiscountAdded = True
    '    End If

    '    Return iDiscountToAdd ' -1 if no discount is added
    'End Function

    Public Sub RemoveINEXPFactor(ByVal oPolicy As clsPolicyPPA)

        Dim iInExp As Integer = 0
        For Each oDrv As CorPolicy.clsEntityDriver In oPolicy.Drivers
            iInExp = -1
            For i As Integer = oDrv.Factors.Count - 1 To 0 Step -1
                If oDrv.Factors.Item(i).FactorCode.ToUpper.Contains("INEXP") Then
                    iInExp = i
                End If
                If iInExp >= 0 Then
                    oDrv.Factors.RemoveAt(iInExp)
                    Exit For
                End If
            Next
        Next

    End Sub

    Public Sub ApplySafeDriverRules(ByVal oPolicy As clsPolicyPPA)
        Try

            Dim operatorLicensedLessThan36MonthsExists As Boolean = False
            Dim suspendedViolationExists As Boolean = False
            Dim restrictedLicenseExists As Boolean = False
            Dim sr22Exists As Boolean = False
            Dim outOfStateLicenseExists As Boolean = False
            Dim removeSafeDriverDiscounts As Boolean = False

            Dim oRatingRules As New CommonRulesFunctions
            Dim iDiscountToAdd As Integer = -1

            Dim disqualifiedForSafeDriver As Boolean = False

            For Each driver As clsEntityDriver In oPolicy.Drivers
                If driver.DriverStatus.ToUpper = "ACTIVE" Then
                    If driver.SR22 Then
                        sr22Exists = True
                    End If
                    If driver.LicenseIssueDate <> Date.MinValue Then
                        If driver.LicenseIssueDate >= DateAdd(DateInterval.Month, -36, oPolicy.EffDate) Then
                            operatorLicensedLessThan36MonthsExists = True
                        End If
                    Else
                        If driver.LicenseStateDate >= DateAdd(DateInterval.Month, -36, oPolicy.EffDate) Then
                            operatorLicensedLessThan36MonthsExists = True
                        End If
                    End If
                    If driver.DLNState IsNot Nothing AndAlso driver.DLNState.ToUpper.Trim <> "FL" Then
                        outOfStateLicenseExists = True
                    End If
                    If driver.LicenseStatus IsNot Nothing AndAlso driver.LicenseStatus.ToUpper.Trim = "PERMIT" Then
                        restrictedLicenseExists = True
                    End If
                End If
            Next

            If Not operatorLicensedLessThan36MonthsExists _
                    AndAlso Not restrictedLicenseExists _
                        AndAlso Not sr22Exists _
                            AndAlso Not outOfStateLicenseExists Then

                Dim iTempDriverMonthsList As New ArrayList()

                For Each driver As clsEntityDriver In oPolicy.Drivers
                    If driver.DriverStatus.ToUpper = "ACTIVE" Then
                        Dim iTempMonthsOldlist As New ArrayList()
                        Dim iMinorViolations As Integer = 0
                        For Each violations As clsBaseViolation In driver.Violations
                            Dim iTempMonthsOld As Integer = 0
                            iTempMonthsOld = oRatingRules.CalculateViolAge(violations.ViolDate, oPolicy.EffDate)

                            If CriminalSuspendedViol(violations, oPolicy, iTempMonthsOld) Then
                                disqualifiedForSafeDriver = True
                                Exit For
                            Else
                                If SafeDriverViolRules(violations) Then

                                    If iTempMonthsOld < 0 Then
                                        iTempMonthsOld = 0
                                    End If
                                    iTempMonthsOldlist.Add(iTempMonthsOld)

                                ElseIf violations.ViolGroup = "MIN" Then

                                    ' If the violation was within the past 18 months
                                    If iTempMonthsOld <= 18 Then
                                        iMinorViolations += 1
                                        'discount
                                    End If

                                End If
                                If iMinorViolations >= 3 Then
                                    disqualifiedForSafeDriver = True
                                    Exit For
                                End If
                            End If
                        Next
                        '08-07-2012 RK
                        If disqualifiedForSafeDriver Then
                            Exit For
                        End If

                        'sort voilation list
                        iTempMonthsOldlist.Sort()
                        If iTempMonthsOldlist.Count > 0 Then
                            Dim XleastMonth As Integer = (iTempMonthsOldlist(0))
                            'driver
                            iTempDriverMonthsList.Add(XleastMonth)
                        End If
                    End If
                Next

                iTempDriverMonthsList.Sort()
                If iTempDriverMonthsList.Count > 0 Then
                    iDiscountToAdd = (iTempDriverMonthsList(0))
                    If iDiscountToAdd >= 36 Then
                        If Not DeterminePolicyFactorExists(oPolicy, "SAFE_DRV36") Then
                            AddPolicyFactor(oPolicy, "SAFE_DRV36")
                        End If
                    End If
                    If iDiscountToAdd >= 18 And iDiscountToAdd < 36 Then
                        If Not DeterminePolicyFactorExists(oPolicy, "SAFE_DRV18") Then
                            AddPolicyFactor(oPolicy, "SAFE_DRV18")
                        End If
                    End If
                    If iDiscountToAdd >= 12 And iDiscountToAdd < 18 Then
                        If Not DeterminePolicyFactorExists(oPolicy, "SAFE_DRV12") Then
                            AddPolicyFactor(oPolicy, "SAFE_DRV12")
                        End If
                    End If
                ElseIf iTempDriverMonthsList.Count <= 0 Then
                    If Not DeterminePolicyFactorExists(oPolicy, "SAFE_DRV36") Then
                        AddPolicyFactor(oPolicy, "SAFE_DRV36")
                    End If
                End If
            Else
                disqualifiedForSafeDriver = True
            End If

            If disqualifiedForSafeDriver Then
                ' Remove discount 
                RemovePolicyFactor(oPolicy, "SAFE_DRV12")
                RemovePolicyFactor(oPolicy, "SAFE_DRV18")
                RemovePolicyFactor(oPolicy, "SAFE_DRV36")
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function SafeDriverViolRules(ByVal oViolations As clsBaseViolation) As Boolean

        Select Case oViolations.ViolGroup
            Case "FEL", "DWI", "MAJ", "FAL", "LIC", "AFA", "MED"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Function CriminalSuspendedViol(ByRef oViolations As clsBaseViolation, ByRef oPolicy As clsPolicyPPA, ByRef oNum As Integer) As Boolean

        If (Not String.IsNullOrWhiteSpace(oViolations.ViolCode) AndAlso oViolations.ViolCode.ToString.Contains("30111")) _
            OrElse (Not String.IsNullOrWhiteSpace(oViolations.ViolTypeCode) AndAlso oViolations.ViolTypeCode.ToString.Contains("30111")) Then
            Dim msMVRVoilDetails As String() = oViolations.MVRViolDetail.Split(";")
            For Each mvrVoildetail In msMVRVoilDetails
                If mvrVoildetail <> "" Then
                    If GetStateInfoValue(oPolicy, oPolicy.Program, "SAFE_DRV", "SUSPENSION", mvrVoildetail) = "C" Then
                        Return True
                    End If
                End If
            Next
        End If
        Return False
    End Function

    Public Sub CheckAccidentPreventionDiscount(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""

            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.DrivingCourse Then

                    Dim numYears As Double = -1.0
                    Dim numDaysDiff As Long = DateDiff(DateInterval.DayOfYear, oDriver.DrivingCourseDate, Date.Today)

                    If numDaysDiff <> 0 Then
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The Accident Prevention Discount does not apply unless the driver is 55 or older and the accident prevention course was completed no more than 3 years prior to the policy effective date. Driver# " & sDriverList & ".", "ACC_PREV", "IER", oPolicy.Notes.Count))
        End If

    End Sub
End Class







