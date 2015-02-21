Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data.SqlClient
Imports System.Data
Imports System.Collections.Generic
Imports System.Configuration

Public Class clsRules202
    Inherits clsRules2

#Region "IER Functions"
    ' this is overriden to do nothing for AZ and not throw this IER
    Public Overrides Sub CheckPermittedNotExcluded(ByVal oPolicy)
        Dim sDriverList As String = ""
        Dim bHasNamedInsured As Boolean = False

        sDriverList = ""
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.RelationToInsured = "SELF" Then
                bHasNamedInsured = True
            End If
        Next
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) must be at least 14 years of age - " & sDriverList & ".", "UnderAgeDriver", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckUnderAgeDriver(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If Not oDriver.IsMarkedForDelete Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                ' Drivers must be at least 14 years of age
                If (oDriver.Age < 14) Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) must be at least 14 years of age - " & sDriverList & ".", "UnderAgeDriver", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckDriverViolations(ByRef oPolicy As clsPolicyPPA)
        ' Please make a driver ineligible if they have  2 or more of any combination of the following violations
        ' within the previous 35 months:
        'a. Criminal negligence resulting in death, homicide or assault, and arising from the operation of a vehicle;
        'b. Leaving the scene of an accident;
        'c. Making false statements in an application for a driver’s license; or
        'd. Reckless Driving.

        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    Dim iViolCount As Integer = 0
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        ' 11110:MAJ:V Reckless
                        ' 11520:MAJ:V -- crim neg
                        ' 41240:MIN:V,41250:MIN:V, 11750:MAJ:A -- leaving scene
                        ' 36570:MAJ:V -- false s tatements
                        ' 11510 MAJ Homicide by Vehicle, 11530 MAJ Viol resulting manslaughter 
                        If oViolation.ViolCode = "11110:MAJ:V" Or oViolation.ViolCode = "11520:MAJ:V" _
                                Or oViolation.ViolCode = "41240:MIN:V" Or oViolation.ViolCode = "41250:MIN:V" _
                                Or oViolation.ViolCode = "36570:MAJ:V" Or oViolation.ViolCode = "11750:MAJ:A" _
                                Or oViolation.ViolCode = "11530:MAJ:V" Or oViolation.ViolCode = "11510:MAJ:V" Then
                            If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
                                iViolCount += 1
                            End If
                        End If
                    Next
                    If iViolCount >= 2 Then
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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have 2 or more criminal negligence violations - " & sDriverList & ".", "ChargeableCrimCount", "IER", oPolicy.Notes.Count)
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

    Public Sub CheckDWICountUnder21(ByRef oPolicy As clsPolicyPPA)
        ' No DWI,DUI,alcohol, drug, or controlled substance violations
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete And oDriver.Age < 21 Then
                    Dim iDWI As Integer = 0
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        If oViolation.ViolGroup = "DWI" Then
                            iDWI += 1
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

    Public Sub CheckDriverPoints15(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints15(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 6 points - " & sDriverList & ".", "MaxDriverPoints15", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckDriverPoints19(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints19(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 8 violation points - " & sDriverList & ".", "MaxDriverPoints19", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Overrides Sub CheckPolicyPoints(ByVal oPolicy As clsPolicyPPA)
        ' override, this doesn't apply to florida
    End Sub

    Public Sub CheckDriverPoints22(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints22(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), have more than 25 violation points - " & sDriverList & ".", "MaxDriverPoints22", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Overridable Function CheckDriverPoints15(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' 15-18 more than 6 points
        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If (oDriver.Age >= 15 And oDriver.Age <= 18) Then
                If oDriver.Points > 6 Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 6 violation points - " & sDriverList & ".", "MaxDriverPoints15", "IER", oNoteList.Count, "AOLE")
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Function CheckDriverPoints19(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If (oDriver.Age >= 19 And oDriver.Age <= 21) Then
                If oDriver.Points > 8 Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 8 violation points - " & sDriverList & ".", "MaxDriverPoints19", "IER", oNoteList.Count, "AOLE")
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Function CheckDriverPoints22(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            If oDriver.Age >= 22 Then
                If oDriver.Points > 25 Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s) have more than 25 violation points - " & sDriverList & ".", "MaxDriverPoints22", "IER", oNoteList.Count, "AOLE")
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckChargeableCount(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckChargeableCount(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), have more than 6 chargeable violations - " & sDriverList & ".", "MaxChargeableViols", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Overridable Function CheckChargeableCount(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""
        Dim iNumChargeable As Integer = 0

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
            For Each oViol As clsBaseViolation In oDriver.Violations
                If oViol.Chargeable Then
                    iNumChargeable += 1
                End If
            Next
        End If


        If iNumChargeable > 6 Then
            sDriverList = oDriver.IndexNum

            If Not oNoteList Is Nothing Then
                oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), have more than 6 chargeable violations - " & sDriverList & ".", "MaxChargeableViols", "IER", oNoteList.Count, "AOLE")
                Return ""
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckCombinedViolCount(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""
        Dim iNumPoints As Integer = 0

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                iNumPoints += oDriver.Points
            End If
        Next

        If iNumPoints > 30 Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Policy has more than 30 violation points combined for all drivers.", "MaxCombinedViols", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckPermittedDriverAge(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            Dim sDrv As String = ""
            sDrv = CheckPermittedDriverAge(oDrv, oPolicy.Program)

            If Len(sDrv) > 0 Then
                If sDriverList = String.Empty Then
                    sDriverList = sDrv
                Else
                    sDriverList &= ", " & sDrv
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Permitted drivers can not be older than 15 years old - " & sDriverList & ".", "PermitAge", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckPermittedDriverAge(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If Not oDriver.IsMarkedForDelete Then
            If oDriver.DriverStatus.ToUpper = "PERMITTED" Then
                If (oDriver.Age > 15) Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: Permitted drivers can not be older than 15 years old - " & sDriverList & ".", "UnderAgeDriver", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckMAJViolCount(ByRef oPolicy As clsPolicyPPA)
        ' No more than 2 MAJ within the previous 35 months
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    Dim iMAJ As Integer = 0
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        If oViolation.ViolGroup = "MAJ" Then
                            If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
                                iMAJ += 1
                            End If
                        End If
                    Next
                    If iMAJ > 2 Then
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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than 2 major violations - " & sDriverList & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckSymbol2010(ByRef oPolicy As clsPolicyPPA)

        Dim sVehicleList As String = ""
        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then

                Dim sVeh As String = ""
                sVeh = CheckSymbol2010(oVehicle, oPolicy.Program)

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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a symbol 25 or greater- " & sVehicleList & ".", "SymbolOver25", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckSymbol2010(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If oVehicle.VehicleYear <= 2010 Then
            If oVehicle.VehicleSymbolCode <> String.Empty Then
                Try
                    If CInt(oVehicle.VehicleSymbolCode.Trim) >= 25 And oVehicle.VinNo.ToUpper <> "NONOWNER" And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 65 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 66 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 67 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 68 Then
                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol 25 or greater - " & sVehicleList & ".", "SymbolOver25", "IER", oNoteList.Count, "AOLE"))
                            Return ""
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
        End If

        Return sVehicleList
    End Function

    Public Sub CheckSymbol2011(ByRef oPolicy As clsPolicyPPA)

        Dim sVehicleList As String = ""
        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then

                Dim sVeh As String = ""
                sVeh = CheckSymbol2011(oVehicle, oPolicy.Program)

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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a symbol 58 or greater- " & sVehicleList & ".", "SymbolOver22", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckSymbol2011(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If oVehicle.VehicleYear >= 2011 Then
            If oVehicle.VehicleSymbolCode <> String.Empty Then
                Try
                    If CInt(oVehicle.VehicleSymbolCode.Trim) >= 58 And oVehicle.VinNo.ToUpper <> "NONOWNER" And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 965 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 966 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 967 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 968 Then
                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol 58 or greater- " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                            Return ""
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
        End If

        Return sVehicleList
    End Function

    Public Sub CheckBusinessUse(ByVal oPolicy As clsPolicyPPA)
        Dim iBusinessUseCount As Integer = 0

        ' Check to see if any vehicle has the business use factor
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                For Each oFactor As clsBaseFactor In oVehicle.Factors
                    If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                        iBusinessUseCount = iBusinessUseCount + 1
                        Exit For
                    End If
                Next
            End If
        Next

        If iBusinessUseCount > 1 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Maximum of 1 vehicle is permitted for business use.", "BusinessUseCount", "IER", oPolicy.Notes.Count))
        End If
    End Sub

	Public Sub CheckGaragingZipAZ(ByVal oPolicy As clsPolicyPPA)
		Dim sVehicleList As String = ""
		sVehicleList = String.Empty

		For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
			If VehicleApplies(oVeh, oPolicy) Then
				Dim sVeh As String
				sVeh = CheckGaragingZipAZ(oVeh)

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
			oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a garaging zip outside of Arizona - " & sVehicleList & ".", "InvalidZipAZ", "IER", oPolicy.Notes.Count))
		End If
	End Sub

    Public Function CheckGaragingZipAZ(ByVal oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing)
        Dim sVehicleList As String = ""
        sVehicleList = String.Empty

        Dim dtValidZips As DataTable
        dtValidZips = GetValidZips()



        Dim bValidZip As Boolean = False
        For Each oZip As DataRow In dtValidZips.Rows
            Dim sZip = oZip("zip").ToString
            If sZip = oVehicle.Zip Then
                bValidZip = True
                Exit For
            End If
        Next

        If Not bValidZip Then
            sVehicleList = oVehicle.IndexNum

            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a garaging zip outside of Arizona - " & sVehicleList & ".", "InvalidZipAZ", "IER", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End If

        Return sVehicleList
    End Function


    ' Restricted only if they have phys damage
    Private Function GetValidZips() As DataTable

        Dim dtRatingRules As New DataTable
		Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String

        Try
            sSql = " select distinct zip from pgm202..codeterritorydefinitions"

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtRatingRules)
            End Using

            oConn.close()

        Catch ex As Exception
        End Try

        Return dtRatingRules


    End Function


    Public Sub CheckDriverLicensed(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverLicensed(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) do not have a driver's license - " & sDriverList & ".", "MaxChargeableViols", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Overridable Function CheckDriverLicensed(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""
        Dim iNumChargeable As Integer = 0

        If oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED" Then
            If oDriver.DLN.Length = 0 Then
                sDriverList = oDriver.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s) do not have a driver's license - " & sDriverList & ".", "MaxChargeableViols", "IER", oNoteList.Count, "AOLE")
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overrides Sub CheckPayPlan(ByRef oPolicy As clsPolicyPPA)
        Call MyBase.CheckPayPlan(oPolicy)


        If oPolicy.PayPlanCode = "MTA" Then
            If oPolicy.PolicyInsured.PriorLimitsCode > 0 Then
                If oPolicy.IsEFT = False Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: EFT must be selected for this payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
                End If
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Prior Coverage is required for the selected payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub


#End Region

    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim parent As New clsRules2

        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""

        Try
            If parent.CheckNEI(oPolicy) Then
				For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
					If Not oVeh.IsMarkedForDelete Then
						If ((oVeh.VehicleSymbolCode = "66" Or oVeh.VehicleSymbolCode = "67" Or oVeh.VehicleSymbolCode = "68") And oVeh.VehicleYear < 2011) Or ((oVeh.VehicleSymbolCode = "966" Or oVeh.VehicleSymbolCode = "967" Or oVeh.VehicleSymbolCode = "968") And oVeh.VehicleYear >= 2011) Then
							If oVeh.VinNo.Trim.Length = 0 Then
								bEnoughInfoToRate = False
								sMissing += "InvalidVIN:Veh " & oVeh.IndexNum & "-"
							End If
						End If
					End If
				Next

                If sMissing = "" Then
                    bEnoughInfoToRate = True
                Else
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Needs: " & sMissing, "Not Enough Information To Rate", "NEI", oPolicy.Notes.Count))
                    bEnoughInfoToRate = False
                End If
            Else
                bEnoughInfoToRate = False
            End If

            Return bEnoughInfoToRate
        Catch ex As Exception
            oPolicy.Notes = (AddNote(oPolicy.Notes, ex.Message & "Needs: " & sMissing & " - " & ex.StackTrace, "Not Enough Information To Rate", "NEI", oPolicy.Notes.Count))
            Return False
        End Try
    End Function

    Public Overrides Function PolicyHasIneligibleRisk(ByVal oPolicy As CorPolicy.clsPolicyPPA) As Boolean
        Dim bIneligibleRisk As Boolean = False
        Dim sReason As String = String.Empty

        'Drivers (New business and new driver added to an existing policy will not be rated or 
        '   written if they fall into one of the categories below.  
        '   Renewal drivers who fall into one of these categories will 
        '   receive and ineligible risk surcharge.)
        Dim iTotalPoints As Integer = 0
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            CheckViolations(oDriver, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, "B")

            If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then

                '1.     Operators not residing in the state.
                'If oDriver.DLNState <> "AZ" Then
                '    bIneligibleRisk = True
                '    sReason = "Driver not residing in the state- " & oDriver.IndexNum
                '    Exit For
                'End If

                '2.     Operators age 15-18 with more than 6 points.
                If oDriver.Age >= 15 AndAlso oDriver.Age <= 18 Then
                    If oDriver.Points > 6 Then
                        sReason = "Driver age 15-18 with more than 6 points- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If

                '3.     Operators age 19-21 with more than 8 points.
                If oDriver.Age >= 19 AndAlso oDriver.Age <= 21 Then
                    If oDriver.Points > 8 Then
                        sReason = "Driver age 19-21 with more than 8 points.- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If

                '4.     Operators age 22 and older with more than 25 points.
                If oDriver.Age >= 22 Then
                    If oDriver.Points > 25 Then
                        sReason = "Driver age 22 and older with more than 25 points.- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If

                '5.     Operators with more than 6 chargeable violations.
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

                '6.     Any risk with more than 30 driver violation points combined for all drivers. 
                '       total up for each driver, then check outside the for loop
                iTotalPoints = iTotalPoints + oDriver.Points


                '7.     Drivers with more than 2 major violations.
                Dim iMAJ As Integer = 0
                For Each oViolation As clsBaseViolation In oDriver.Violations
                    If oViolation.ViolGroup = "MAJ" Then
                        If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
                            iMAJ += 1
                        End If
                    End If
                Next
                If iMAJ > 2 Then
                    sReason = "Driver with more than 2 major violations.- " & oDriver.IndexNum
                    bIneligibleRisk = True
                    Exit For
                End If

                '8.     Operators with more than 2 alcohol, drug, or controlled substance violations within the previous 35 months.
                '           (Use the same violations as Webrater)
                Dim iDWI As Integer = 0
                For Each oViolation As clsBaseViolation In oDriver.Violations
                    If oViolation.ViolGroup = "DWI" Then
                        If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
                            iDWI += 1
                        End If
                    End If
                Next
                If iDWI > 2 Then
                    sReason = "Driver with more than 2 alcohol, drug, or controlled substance violations within the previous 35 months.- " & oDriver.IndexNum
                    bIneligibleRisk = True
                    Exit For
                End If

                '9.     Operators convicted of an alcohol, drug, or controlled substance violation prior to age 21 (Underage DWI).
                '           (Use same violations as Webrater)
                If oDriver.Age < 21 Then
                    If iDWI > 0 Then
                        sReason = "Driver convicted of an alcohol, drug, or controlled substance violation prior to age 21 (Underage DWI).- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If
            End If
        Next

        '6.     Any risk with more than 30 driver violation points combined for all drivers. 
        If iTotalPoints > 30 Then
            sReason = "More than 30 driver violation points combined for all drivers"
            bIneligibleRisk = True
        End If

        If Not bIneligibleRisk Then ' No need to look at vehicles if we already know this is an ineligible risk
            'Replacement Vehicles (The surcharge list applies to replacement vehicles only. 
            '   If a driver wants to add a new vehicle to a policy or the policy is new business, 
            '   we will not write the risk if it falls into one of the categories below.  
            '   Any replacement vehicles that fall under one of these categories will receive an ineligible risk surcharge)

            Dim iBusinessUseCount As Integer = 0
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete AndAlso oVeh.VinNo <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then

                    '1.     Vehicles with a value over $60,000.
                    Dim iMaxSymbol As Integer
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

                    If iVehSymbol > iMaxSymbol Then
                        sReason = "Vehicle with a value over $60,000.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '2.     Vehicles rated a with physical damage symbol 25 or higher for model years 2010 and older.
                    If oVeh.VehicleYear <= 2010 Then
                        If oVeh.VehicleSymbolCode <> String.Empty Then
                            Try
                                If CInt(oVeh.VehicleSymbolCode.Trim) >= 25 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
                                    sReason = "Vehicle with physical damage symbol 25 or higher.- " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            Catch ex As Exception
                            End Try
                        End If
                    End If

                    '3.     Vehicles rated a with physical damage symbol 58 or higher for model years 2011 and newer.
                    If oVeh.VehicleYear >= 2011 Then
                        If oVeh.VehicleSymbolCode <> String.Empty Then
                            Try
                                If CInt(oVeh.VehicleSymbolCode.Trim) >= 58 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
                                    sReason = "Vehicle with physical damage symbol 58 or higher.- " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            Catch Ex As Exception
                            End Try
                        End If
                    End If

                    '4.     Vehicle with special additional/custom equipment in excess of $5,000.
                    If oVeh.CustomEquipmentAmt > 5000 Then
                        sReason = "Vehicle with special additional/custom equipment in excess of $5,000.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '5.     Vehicles that have a title or registration indicating that the vehicle has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.
                    '   (These vehicles can be quoted for BI, PD, UMBI, UIMBI and MED coverages). 
                    If DeterminePhysDamageExists(oVeh) Then
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

                    '6.     More than 1 Business or Artisan use vehicle. 
                    For Each oFactor As clsBaseFactor In oVeh.Factors
                        If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                            iBusinessUseCount = iBusinessUseCount + 1
                            Exit For
                        End If
                    Next

                    '7.     Vehicles with a principal out-of-state garaging location.
                    Dim sVeh As String = String.Empty
                    sVeh = CheckGaragingZipAZ(oVeh)

                    If Len(sVeh) > 0 Then
                        sReason = "Vehicle with a principal out-of-state garaging location.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '8.     Vehicles over 15 years old are unacceptable for all physical damage coverage on new policies. 
                    If oVeh.VehicleAge > 15 AndAlso DeterminePhysDamageExists(oVeh) Then
                        sReason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '9.     Vehicles over 40 years old are unacceptable for all coverages.
                    If oVeh.VehicleAge > 40 Then
                        sReason = "Vehicles over 40 years old are unacceptable for all coverages.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If
            Next

            '6.     More than 1 Business or Artisan use vehicle. 
            If iBusinessUseCount > 1 Then
                sReason = "More than 1 Business or Artisan use vehicle."
                bIneligibleRisk = True
            End If
        End If

        If bIneligibleRisk Then
            If Not FactorOnPolicy(oPolicy, "INELIGIBLE") Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: A surcharge has been applied to policy due to: " & sReason, "IRSurcharge", "AAF", oPolicy.Notes.Count))
            End If
        End If

        Return bIneligibleRisk
    End Function

    Public Function NeedUMForm(ByVal policy As clsPolicyPPA) As Boolean

        If policy.StateCode = "35" Then
            Return True
        End If

        If policy.VehicleUnits IsNot Nothing AndAlso policy.VehicleUnits.Count > 0 Then
            Dim covUMBILimit As String = GetCoverageLimit("UMBI", policy, policy.VehicleUnits(0))
            Dim covUIMBILimit As String = GetCoverageLimit("UIMBI", policy, policy.VehicleUnits(0))
            Dim covBILimit As String = GetCoverageLimit("BI", policy, policy.VehicleUnits(0))

            If covUIMBILimit = covBILimit AndAlso covUMBILimit = covBILimit Then
                Return False
            End If
        End If

        Return True
    End Function

    Public Function GetCoverageLimit(ByVal sCovGroup As String, ByVal oPolicy As clsPolicyPPA, ByVal oVeh As clsVehicleUnit) As String
        Dim oReturnedCov As clsPACoverage = Nothing

        For Each oCov As clsPACoverage In oVeh.Coverages
            If oCov.CovGroup.ToUpper = sCovGroup.ToUpper Then
                Return oCov.CovLimit
                Exit For
            End If
        Next

        Return "0"
    End Function



    Public Overrides Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyPPA) As String

        Dim sItemsToBeFaxedIn As String = ""

        If oPolicy.CallingSystem <> "PAS" Then
            sItemsToBeFaxedIn &= "Signed and dated Application by both insured and agent" & vbNewLine

            If NeedUMForm(oPolicy) Then
                sItemsToBeFaxedIn &= "Signed and dated Uninsured and Underinsured Motorist Coverage Selection Form by the insured" & vbNewLine
            End If

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

        'proof of prior
        If oPolicy.PolicyInsured.DaysLapse > 0 AndAlso Not IsRewritePolicy(oPolicy) Then
            sItemsToBeFaxedIn &= "Proof of Prior Coverage" & vbNewLine
        End If

        If oPolicy.IsEFT Then
            sItemsToBeFaxedIn &= "EFT Authorization Form" & vbNewLine
        End If

        'military 
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete Then
                If oDrv.Military AndAlso Not IsRewritePolicy(oPolicy) Then
                    sItemsToBeFaxedIn &= "Proof of Military Discount for " & oDrv.EntityName1 & " " & oDrv.EntityName2 & vbNewLine
                End If
            End If
        Next
        Return sItemsToBeFaxedIn

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

    Public Overrides Sub AddAutoApplyFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Call MyBase.AddAutoApplyFactors(oPolicy)

            ' For each driver with a UDR violation, add a UDR factor
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

End Class
