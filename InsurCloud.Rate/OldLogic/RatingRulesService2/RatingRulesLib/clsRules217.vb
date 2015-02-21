Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data
Imports System.Data.SqlClient

Public Class clsRules217
    Inherits clsRules2

    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim parent As New clsRules2

        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""

        Try
            If parent.CheckNEI(oPolicy) Then
                Select Case oPolicy.Program.ToUpper
                    Case "SUMMIT"
                        If Not oPolicy.PolicyInsured Is Nothing Then
                            With oPolicy.PolicyInsured
                                'CreditScore
                                If IsNumeric(.CreditScore) Then
                                    'If .CreditScore = 0 Then
                                    '    bEnoughInfoToRate = False
                                    '    sMissing += "CreditScore" & "-"
                                    'End If
                                Else
                                    bEnoughInfoToRate = False
                                    sMissing += "CreditScore" & "-"
                                End If
                            End With
                        End If
                    Case "CLASSIC", "MONTHLY"
                End Select

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

    Public Overrides Sub CheckNonOwner(ByVal oPolicy As clsPolicyPPA)
        'MyBase.CheckNonOwner(oPolicy)

        Dim bIsNonOwner As Boolean
        bIsNonOwner = False

        ' Check to see if this is a nonowner policy
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If oVehicle.VinNo.ToUpper = "NONOWNER" Then
                    bIsNonOwner = True
                    Exit For
                End If
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

        End If
    End Sub

#Region "IER Functions"

    '' Physical Damage Restriction 2 (Any vehicle over 15 years old.)
    'Public Overrides Sub CheckPhysicalDamageRestriction(ByRef oPolicy As clsPolicyPPA)
    '    ' LA Has its own function, don't need to call this
    'End Sub

    Public Sub CheckRentToOwnAllowed(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""

                sVeh = CheckRentToOwnAllowed(oVehicle, oPolicy.Program)
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
            If oPolicy.Program.ToUpper = "DIRECT" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Rent to Own vehicles are unacceptable in this program.", "RentToOwn", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Rent To Own is not available for the program selected.  The following vehicle(s) are listed as Rent To Own - " & sVehicleList & ".", "RentToOwn", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub


    Public Overrides Function PolicyHasIneligibleRisk(ByVal oPolicy As CorPolicy.clsPolicyPPA) As Boolean
        Dim bIneligibleRisk As Boolean = False
        Dim sReason As String = String.Empty
        Dim iMaxSymbol As Integer
        'Drivers (New business and new driver added to an existing policy will not be rated or 
        '   written if they fall into one of the categories below.  
        '   Renewal drivers who fall into one of these categories will 
        '   receive and ineligible risk surcharge.)
        Dim iTotalPoints As Integer = 0
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            CheckViolations(oDriver, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, "B")

            If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then

                '2.     Operators age 15-18 with more than 3 points.
                If oDriver.Age >= 15 AndAlso oDriver.Age <= 18 Then
                    If oDriver.Points > 3 Then
                        sReason = "Driver age 15-18 with more than 3 points- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If

                '3.     Operators age 19-21 with more than 5 points.
                If oDriver.Age >= 19 AndAlso oDriver.Age <= 21 Then
                    If oDriver.Points > 5 Then
                        sReason = "Driver age 19-21 with more than 5 points.- " & oDriver.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If
                End If

                '4.     Operators age 22 and older with more than 15 points.
                If oDriver.Age >= 22 Then
                    If oDriver.Points > 15 Then
                        sReason = "Driver age 22 and older with more than 15 points.- " & oDriver.IndexNum
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

        '6.     Any risk with more than 18 driver violation points combined for all drivers. 
        If iTotalPoints > 18 Then
            sReason = "More than 18 driver violation points combined for all drivers"
            bIneligibleRisk = True
        End If

        If Not bIneligibleRisk Then ' No need to look at vehicles if we already know this is an ineligible risk
            'Replacement Vehicles (The surcharge list applies to replacement vehicles only. 
            '   If a driver wants to add a new vehicle to a policy or the policy is new business, 
            '   we will not write the risk if it falls into one of the categories below.  
            '   Any replacement vehicles that fall under one of these categories will receive an ineligible risk surcharge)

            Dim iBusinessUseCount As Integer = 0
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete AndAlso oVeh.VinNo <> "NONOWNER" Then

                    '1.     Vehicles with a value over $60,000.
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

                    '2.     Vehicles rated with physical damage symbol 25 or higher for model years 2010 and older.
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

                    '3.     Vehicles rated with physical damage symbol 58 or higher for model years 2011 and newer.
                    If oVeh.VehicleYear >= 2011 Then
                        If oVeh.CollSymbolCode <> String.Empty Then
                            Try
                                If CInt(oVeh.CollSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
                                    sReason = "Vehicle with physical damage symbol greater than " & iMaxSymbol.ToString & " - " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            Catch Ex As Exception
                            End Try
                        End If
                    End If


                    '4.     Vehicles that have a title or registration indicating that the vehicle has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.
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

                    '5.     Vehicles over 15 years old are unacceptable for all physical damage coverage on new policies. 
                    'If oVeh.VehicleAge > 15 AndAlso DeterminePhysDamageExists(oVeh) Then
                    If oVeh.VehicleYear < Now.AddYears(-15).Year AndAlso DeterminePhysDamageExists(oVeh) Then
                        sReason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '6.     Vehicles over 40 years old are unacceptable for all coverages.
                    If oVeh.VehicleAge > 40 Then
                        sReason = "Vehicles over 40 years old are unacceptable for all coverages.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '7.     Vehicles garaged out of state.
                    If Not ValidateVehicleZipCode(oVeh.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                        sReason = "Vehicles is garaged out of state.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '8.     More than 1 Business or Artisan use vehicle. 
                    For Each oFactor As clsBaseFactor In oVeh.Factors
                        If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                            iBusinessUseCount = iBusinessUseCount + 1
                            Exit For
                        End If
                    Next

                End If
            Next

            If Not bIneligibleRisk Then
                If iBusinessUseCount > 1 Then
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

    Public Overridable Function CheckRentToOwnAllowed(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        If sProgram.ToUpper = "SUMMIT" Or sProgram.ToUpper = "MONTHLY" Or sProgram.ToUpper = "DIRECT" Then
            If DetermineVehicleFactorExists(oVehicle, "RENT_TO_OWN") Then
                sVehicleList = oVehicle.IndexNum
                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: Rent To Own is not available for the program selected.  The following vehicle(s) are listed as Rent To Own - " & sVehicleList & ".", "RentToOwn", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sVehicleList
    End Function

    Public Sub CheckDriverPointsSummit(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDriver As String = ""
                sDriver = CheckDriverPointsSummit(oDriver, oPolicy.Program)
                If Len(sDriver) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDriver
                    Else
                        sDriverList &= ", " & sDriver
                    End If
                End If
            End If
        Next

        If sDriverList <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), have more than 30 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Overridable Function CheckDriverPointsSummit(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' No drivers with over 30 points on summit
        If sProgram.ToUpper = "SUMMIT" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.Points > 30 Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), have more than 30 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckSymbol(ByRef oPolicy As clsPolicyPPA)

        Dim sVehicleList As String = ""
        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then

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

    Public Overridable Function CheckSymbol(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "MONTHLY" Then
            If oVehicle.VehicleSymbolCode <> String.Empty And (oVehicle.CollSymbolCode.Trim = "" And oVehicle.CompSymbolCode.Trim = "") Then
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
        End If

        Return sVehicleList
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

    Public Sub CheckStatedValueClassic(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""


        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckStatedValueClassic(oVehicle, oPolicy.Program)
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) do not have a valid stated value amount (It must be between $500 and $60,000) -  " & sVehicleList & ".", "InvalidStatedValue", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckStatedValueClassic(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Then
            If oVehicle.VehicleYear < 2011 Then
                If oVehicle.VehicleSymbolCode.Trim = "66" Or oVehicle.VehicleSymbolCode.Trim = "67" Or oVehicle.VehicleSymbolCode.Trim = "68" Then
                    If oVehicle.StatedAmt < 500 Or oVehicle.StatedAmt > 60000 Then
                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) do not have a valid stated value amount (It must be between $500 and $60,000) -  " & sVehicleList & ".", "InvalidStatedValue", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            Else
                If oVehicle.VehicleSymbolCode.Trim = "966" Or oVehicle.VehicleSymbolCode.Trim = "967" Or oVehicle.VehicleSymbolCode.Trim = "968" Then
                    If oVehicle.StatedAmt < 500 Or oVehicle.StatedAmt > 60000 Then
                        sVehicleList = oVehicle.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) do not have a valid stated value amount (It must be between $500 and $60,000) -  " & sVehicleList & ".", "InvalidStatedValue", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sVehicleList
    End Function

    Public Sub CheckPremiumFinanceMonthly(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "MONTHLY" Then
            For Each oLienHolder As clsEntityLienHolder In oPolicy.LienHolders
                If oLienHolder.EntityType = "PFC" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Premium Finance Company is not available for the program selected.", "PFCMonthly", "IER", oPolicy.Notes.Count))
                End If
            Next
        End If
    End Sub

    Public Sub CheckStatedValueMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckStatedValueMonthly(oVehicle, oPolicy.Program)

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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Stated value vehicles are not available for the program selected.  The following vehicle(s) are listed as stated value - " & sVehicleList & ".", "StatedValue", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Overridable Function CheckStatedValueMonthly(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "MONTHLY" Then
            If ((oVehicle.VehicleYear < 2011 And oVehicle.VehicleSymbolCode = "66") Or (oVehicle.VehicleYear >= 2011 And oVehicle.VehicleSymbolCode = "966")) And oVehicle.VehicleModelCode.ToUpper <> "LIGHT TRUCK" And CInt(oVehicle.VehicleYear) > 1993 Then
                If sVehicleList = String.Empty Then
                    sVehicleList = oVehicle.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: Stated value vehicles are not available for the program selected.  The following vehicle(s) are listed as stated value - " & sVehicleList & ".", "StatedValue", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sVehicleList
    End Function

    ' Rule 4.A (named insureds that have never been licensed, unless the named insured is excluded from coverage)
    Public Sub CheckUnlicensedNamedInsuredSummit(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "SUMMIT" Then
            If oPolicy.PolicyInsured.EntityName1 <> String.Empty Or oPolicy.PolicyInsured.EntityName2 <> String.Empty Then
                If oPolicy.PolicyInsured.DLN = String.Empty Then
                    If oPolicy.PolicyInsured.DOB > #1/1/1900# Then
                        If oPolicy.PolicyInsured.DriverStatus.ToUpper = "EXCLUDED" Then
                            'they are ok
                        Else
                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Unlicensed named insured.", "InsuredLicense", "IER", oPolicy.Notes.Count))
                        End If
                    End If
                End If
            End If
        End If
    End Sub

    Public Sub CheckMinimumAgeSummit(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""
        ' Rule 4.B (drivers under the minimum age for state licensing)
        If oPolicy.Program.ToUpper = "SUMMIT" Then
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
        End If
    End Sub

    Public Sub CheckMinimumAge(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        If oPolicy.Program.ToUpper = "DIRECT" Then
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
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) are under the minimum age for state licensing and must be Excluded from coverage or listed with a Learners Permit. - " & sDriverList & ".", "MinDriverAge", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

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

    Public Overrides Sub CheckVehicleBusinessUse(ByRef oPolicy As clsPolicyPPA)

        If oPolicy.Program.ToUpper = "SUMMIT" Or oPolicy.Program.ToUpper = "CLASSIC" Then
            Dim iNumOfVehsWithBusUse As Integer = 0
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                'If VehicleApplies(oVeh, oPolicy) Then
                If HasBusinessUse(oVeh) Then
                    iNumOfVehsWithBusUse += 1
                End If
                'End If
            Next
            If iNumOfVehsWithBusUse > 1 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Only one vehicle on a policy may have Business Use.", "OnlyOneBusinessUse", "IER", oPolicy.Notes.Count))
            End If
        End If

        If oPolicy.Program.ToUpper = "DIRECT" Then
            Dim iNumOfVehsWithBusUse As Integer = 0
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                'If VehicleApplies(oVeh, oPolicy) Then
                If HasBusinessUse(oVeh) Then
                    iNumOfVehsWithBusUse += 1
                End If
                'End If
            Next

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

    Public Sub CheckMaxViolationsClassic(ByRef oPolicy As clsPolicyPPA)
        ' Rule 4.C (drivers with greater than 12 violations or greater than 30 points in the Chargeable Period)
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than " & iMaxViolations & " accidents or violations - " & sDriverList & ".", "MaxDriverViols", "IER", oPolicy.Notes.Count))
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
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have more than " & iMaxViolations & " accidents or violations - " & sDriverList & ".", "MaxDriverViols", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckMaxViolationsSummit(ByRef oPolicy As clsPolicyPPA)
        ' Rule 4.C (drivers with greater than 12 violations or greater than 30 points in the Chargeable Period)
        Dim sDriverList As String = ""


        Dim iNumOfViols As Integer = 0
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                    Dim sDrv As String = ""
                    sDrv = CheckMaxViolationsSummit(oDrv, oPolicy.Program)
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than 12 accidents or violations - " & sDriverList & ".", "MaxDriverViols", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckMaxViolationsSummit(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        ' Rule 4.C (drivers with greater than 12 violations or greater than 30 points in the Chargeable Period)
        Dim sDriverList As String = ""
        Dim iNumOfViols As Integer = 0

        If sProgram.ToUpper = "SUMMIT" Then
            If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                iNumOfViols = 0
                For Each oViol As clsBaseViolation In oDrv.Violations
                    If oViol.ViolGroup.ToUpper <> "NAF" Then
                        iNumOfViols += 1
                    End If
                Next
                If iNumOfViols > 12 Then
                    sDriverList = oDrv.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 12 accidents or violations - " & sDriverList & ".", "MaxDriverViols", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overrides Sub CheckOutOfStateZip(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "DIRECT" Then
            Dim bHasOutOfStateVehicle As Boolean = False
            Dim sVehicleList As String = String.Empty

            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVehicle.IsMarkedForDelete Then
                    If Not ValidateVehicleZipCode(oVehicle.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                        If sVehicleList = "" Then
                            sVehicleList = oVehicle.IndexNum
                        Else
                            sVehicleList &= ", " & oVehicle.IndexNum
                        End If
                    End If
                End If
            Next

            If sVehicleList <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an out of state garaging zip - " & sVehicleList & ".", "OutOfStateZip", "IER", oPolicy.Notes.Count, "AOLE"))
            End If
        Else
            Dim bHasInStateVehicle As Boolean = False

            ' if not allowed, validate that zip is in the territorydefinitions table
            Dim sVehicleList As String = String.Empty
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVehicle.IsMarkedForDelete Then
                    If ValidateVehicleZipCode(oVehicle.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                        bHasInStateVehicle = True
                        Exit For
                    End If
                End If
            Next

            If Not bHasInStateVehicle Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: At least one vehicle on the policy must have a Louisiana zip.", "OutOfStateZip", "IER", oPolicy.Notes.Count, "AOLE"))
            End If
        End If
    End Sub


    Public Sub CheckOutOfStateGaraging(ByRef oPolicy As clsPolicyPPA)

        ' Rule 4.E (vehicles with a principal out of state garaging location)
        Dim sVehicleList As String = ""

        If oPolicy.Program.ToUpper = "SUMMIT" Then
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If VehicleApplies(oVeh, oPolicy) Then
                    If oVeh.Territory.Trim = "99" Then
                        If sVehicleList = "" Then
                            sVehicleList = oVeh.IndexNum
                        Else
                            sVehicleList &= ", " & oVeh.IndexNum
                        End If
                    End If
                End If
            Next
            If sVehicleList <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a principal out of state garaging location - " & sVehicleList & ".", "OutOfSateGaraging", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Sub CheckArtistanUseSummit(ByRef oPolicy As clsPolicyPPA)
        Dim iCounter As Integer = 0

        If oPolicy.Program.ToUpper = "SUMMIT" Then
            ' Rule 4.L (risks with 2 or more artisan use vehicles)
            iCounter = 0
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If VehicleApplies(oVeh, oPolicy) Then
                    If oVeh.TypeOfUseCode.ToUpper = "ARTISAN" Then
                        iCounter += 1
                    End If
                End If
            Next
            If iCounter >= 2 Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: This policy has two or more Artisan use vehicles.", "TwoOrMoreArtisan", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Sub CheckPayPlanSummit(ByRef oPolicy As clsPolicyPPA)

        If oPolicy.Program.ToUpper = "SUMMIT" Then
            If oPolicy.PayPlanCode = "MTA" Then
                If oPolicy.PolicyInsured.PriorLimitsCode > 0 Then
                    If oPolicy.IsEFT = False Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: EFT must be selected for this payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
                    End If
                Else
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Prior Coverage is required for the selected payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
                End If
            End If
        End If
    End Sub

    Public Sub CheckSummitEffectiveDate(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "SUMMIT" Then
            If oPolicy.EffDate >= #1/1/2010# Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Sorry, We are not accepting new Summit business effective January 1, 2010.", "SummitEffDate", "IER", oPolicy.Notes.Count))
            End If

        End If
    End Sub

    Public Overridable Function CheckDriverPoints15ClassicMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "MONTHLY" Or sProgram.ToUpper = "DIRECT" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If (oDriver.Age >= 15 And oDriver.Age <= 18) Then
                    If oDriver.Points > 3 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 3 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Function CheckDriverPoints19ClassicMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "MONTHLY" Or sProgram.ToUpper = "DIRECT" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If (oDriver.Age >= 19 And oDriver.Age <= 21) Then
                    If oDriver.Points > 5 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                            Return ""
                        End If
                    End If
                Else
                    If (oDriver.Age >= 15 And oDriver.Age <= 18) Then
                        If oDriver.Points > 3 Then
                            sDriverList = oDriver.IndexNum

                            If Not oNoteList Is Nothing Then
                                oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                                Return ""
                            End If
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Function CheckDriverPoints30Classic(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""
        Dim iMaxPoints As Integer = GetProgramSetting("MaxPoints")

        If sProgram.ToUpper = "CLASSIC" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.Points > iMaxPoints Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than " & iMaxPoints & " points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function


    Public Overridable Function CheckDriverPoints22Monthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        If sProgram.ToUpper = "MONTHLY" Or sProgram.ToUpper = "DIRECT" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If (oDriver.Age >= 22) Then
                    If oDriver.Points > 15 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 15 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckDriverPointsClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints15ClassicMonthly(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 3 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
        End If


        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints19ClassicMonthly(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
        End If

        sDriverList = ""
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints30Classic(oDrv, oPolicy.Program)

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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than " & iMaxPoints & " violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If

        sDriverList = ""
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints22Monthly(oDrv, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than 15 violation points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckChargeableClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than one (1) chargeable alcohol/drug/narcotic related violation (of any kind).)
        ' 3/18/2011 KB Changed to 2 for the next rate revision
        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckChargeableClassicMonthly(oDriver, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            End If
        Next

        Dim iMaxDWICount As Integer = CInt(GetProgramSetting("MaxDWICount"))
        If sDriverList <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than " & iMaxDWICount & " drug or alcohol violations - " & sDriverList & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckChargeableCountMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than one (1) chargeable alcohol/drug/narcotic related violation (of any kind).)
        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckChargeableCountMonthly(oDriver, oPolicy.Program)

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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than 6 accidents or violations - " & sDriverList & ".", "ChargeableViolationCount", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Sub CheckCombinedChargeableCountMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than one (1) chargeable alcohol/drug/narcotic related violation (of any kind).)
        sDriverList = String.Empty
        Dim iPoints As Integer = 0
        If oPolicy.Program.ToUpper.ToUpper = "MONTHLY" Then
            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If Not oDriver.IsMarkedForDelete Then
                    If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                        For Each oViolation As clsBaseViolation In oDriver.Violations
                            iPoints += oViolation.Points
                        Next
                    End If
                End If
            Next
            If iPoints > 18 Then
                oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Total violation points for all drivers exceeds 18 points.", "CombinedPointsCount", "IER", oPolicy.Notes.Count)
            End If
        End If

    End Sub


    Public Overridable Function CheckChargeableCountMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than one (1) chargeable alcohol/drug/narcotic related violation (of any kind).)
        If sProgram.ToUpper = "MONTHLY" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                Dim iChargeable As Integer = 0
                For Each oViolation As clsBaseViolation In oDriver.Violations
                    If oViolation.Chargeable And oViolation.Points > 0 Then
                        iChargeable += 1
                    End If
                Next
                If iChargeable > 6 Then
                    sDriverList = oDriver.IndexNum
                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s) have more than 6 accidents or violations - " & sDriverList & ".", "ChargeableViolationCount", "IER", oNoteList.Count, "AOLE")
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Function CheckChargeableClassicMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than one (1) chargeable alcohol/drug/narcotic related violation (of any kind).)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "MONTHLY" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                Dim iChargeableDWI As Integer = 0
                For Each oViolation As clsBaseViolation In oDriver.Violations
                    If oViolation.ViolGroup = "DWI" And oViolation.Chargeable Then
                        iChargeableDWI += 1
                    End If
                Next

                Dim iMaxDWICount As Integer = CInt(GetProgramSetting("MaxDWICount"))
                If iChargeableDWI > iMaxDWICount Then
                    sDriverList = oDriver.IndexNum
                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s) have more than " & iMaxDWICount & " drug or alcohol violations - " & sDriverList & ".", "ChargeableDWICount", "IER", oNoteList.Count, "AOLE")
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckStatedValue(ByVal oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty

        If oPolicy.Program.ToUpper.Trim = "MONTHLY" Then
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
        End If

    End Sub

    Public Sub CheckNamedInsuredMinorClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "MONTHLY" Then
            ' Rule 3 (Minors (anyone under 18) as the named insured.)
            If oPolicy.PolicyInsured.Age < 18 Then
                oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The Policyholder must be at least 18 years of age.", "NIUnderage", "IER", oPolicy.Notes.Count)
            End If
        End If
    End Sub

    Public Overridable Function CheckPhysicalDamageOnlyClassicMonthly(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "MONTHLY" Then
            Dim bLiabilityExists As Boolean = False
            Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
            If bPhysDamage Then
                For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
                    If oCoverage.CovCode.Contains("BI") Or oCoverage.CovCode.Contains("PD") Then
                        bLiabilityExists = True
                        Exit For
                    End If
                Next
                If bLiabilityExists = False Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage only - " & sVehicleList & ".", "PhysicalDamageOnly", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If

        End If

        Return sVehicleList
    End Function

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

    Public Overrides Function DeterminePhysDamageExists(ByVal oVehicle As clsVehicleUnit) As Boolean
        Dim bPhysDamage As Boolean = False

        ' Check if this is a Physical Damage Policy (i.e. If there are COMP/COLL coverage)
        For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
            If Not oCoverage.IsMarkedForDelete Then
                If (oCoverage.CovCode.Contains("OTC") Or oCoverage.CovCode.Contains("COL") Or oCoverage.CovCode.Contains("UMPD")) And Not oCoverage.IsMarkedForDelete Then
                    bPhysDamage = True
                    Exit For
                End If
            End If
        Next

        Return bPhysDamage
    End Function

    Public Overrides Function DeterminePhysDamageWithNoUMPDExists(ByVal oVehicle As clsVehicleUnit) As Boolean
        Dim bPhysDamage As Boolean = False

        ' Check if this is a Physical Damage Policy (i.e. If there are COMP/COLL coverage)
        For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
            If Not oCoverage.IsMarkedForDelete Then
                If (oCoverage.CovCode.Contains("OTC") Or oCoverage.CovCode.Contains("COL")) And Not oCoverage.IsMarkedForDelete Then
                    bPhysDamage = True
                    Exit For
                End If
            End If
        Next

        Return bPhysDamage
    End Function

    Private Function DeterminePhysDamageAddedOnEndorsement(ByVal oVehicle As clsVehicleUnit) As Boolean
        Dim bPhysDamage As Boolean = False

        ' Check if this is a Physical Damage Policy (i.e. If there are COMP/COLL coverage)
        If oVehicle.IsNew Then
            For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
                If (oCoverage.CovCode.Contains("OTC") Or oCoverage.CovCode.Contains("COL") Or oCoverage.CovCode.Contains("UMPD")) And Not oCoverage.IsMarkedForDelete Then
                    bPhysDamage = True
                    Exit For
                End If
            Next
        End If

        If Not bPhysDamage Then
            For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
                If oCoverage.IsModified OrElse oCoverage.IsNew Then
                    If (oCoverage.CovCode.Contains("OTC") Or oCoverage.CovCode.Contains("COL") Or oCoverage.CovCode.Contains("UMPD")) And Not oCoverage.IsMarkedForDelete Then
                        bPhysDamage = True
                        Exit For
                    End If
                End If
            Next
        End If

        Return bPhysDamage
    End Function

    Public Overridable Function CheckPhysicalDamageOldClassicMonthly(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        Dim bPhysDamage As Boolean = False

        If sProgram.ToUpper = "DIRECT" Then
            bPhysDamage = DeterminePhysDamageWithNoUMPDExists(oVehicle)
        Else
            bPhysDamage = DeterminePhysDamageExists(oVehicle)
        End If

        If sProgram.ToUpper = "CLASSIC" Or (sProgram.ToUpper = "DIRECT" And Not oVehicle.VinNo = "NONOWNER") Then
            If bPhysDamage Then
                If oVehicle.VehicleYear < Now.AddYears(-15).Year Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 15 years - " & sVehicleList & ".", "PhysDamageOver15", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        If sProgram.ToUpper = "MONTHLY" Then

            If bPhysDamage Then
                If oVehicle.VehicleAge > 16 Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                        If Not oNoteList Is Nothing Then
                            oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 15 years - " & sVehicleList & ".", "PhysDamageOver15", "IER", oNoteList.Count, "AOLE"))
                            Return ""
                        End If
                    End If
                End If
            End If

        End If

        Return sVehicleList
    End Function

    Public Sub CheckPhysicalDamageOnlyClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        ' Physical Damage Restriction 1 (Policies written for Physical Damage only.)
        Dim sVehicleList As String = ""

        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits

            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckPhysicalDamageOnlyClassicMonthly(oVehicle, oPolicy.Program)
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage only - " & sVehicleList & ".", "PhysicalDamageOnly", "IER", oPolicy.Notes.Count))
        End If


        ' Physical Damage Restriction 2 (Any vehicle over 15 years old.)
        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckPhysicalDamageOldClassicMonthly(oVehicle, oPolicy.Program)
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 15 years - " & sVehicleList & ".", "PhysDamageOver15", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckUWQuestionsClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        If oPolicy.UWQuestions.Count > 0 Then
            For Each oUWQ As clsUWQuestion In oPolicy.UWQuestions
                Select Case oUWQ.QuestionCode
                    Case "304"
                        If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "MONTHLY" Then
                            If Left(oUWQ.AnswerText.ToUpper, 3) = "YES" Then
                                oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Vehicles cannot be used for business or commercial purposes.", "BusinessUse", "IER", oPolicy.Notes.Count)
                            End If
                        End If
                        'Case "307"
                        'This is being checked elsewhere
                        '    If Left(oUWQ.AnswerText.ToUpper, 3) = "YES" Then
                        '        sVehicleList = String.Empty
                        '        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                        '            If Not oVehicle.IsMarkedForDelete Then
                        '                Dim bPhysDamage As Boolean = DeterminePhysDamageExistsUW(oVehicle)
                        '                If bPhysDamage Then
                        '                    If sVehicleList = String.Empty Then
                        '                        sVehicleList = oVehicle.IndexNum
                        '                    Else
                        '                        sVehicleList &= ", " & oVehicle.IndexNum
                        '                    End If
                        '                End If
                        '            End If
                        '        Next
                        '        If sVehicleList <> String.Empty Then
                        '            If oPolicy.Program.ToUpper = "DIRECT" Then
                        '                oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Please call 866-874-2741 to speak with an Imperial Representative to complete your application. Vehicles that are re-built, salvaged or water damaged require company approval. - " & sVehicleList & ".", "ReBuiltSalvagedVeh", "IER", oPolicy.Notes.Count)
                        '            Else
                        '                oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Vehicle(s) with Physical Damage cannot have been re-built, salvaged or water damaged - " & sVehicleList & ".", "ReBuiltSalvagedVeh", "IER", oPolicy.Notes.Count)
                        '            End If

                        '        End If
                        '    End If
                End Select
            Next
        End If
    End Sub

    Public Sub CheckPayPlanClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "MONTHLY" Then
            If oPolicy.PayPlanCode = "MTA" Then
                If oPolicy.PolicyInsured.PriorLimitsCode > 0 Then
                    If oPolicy.IsEFT = False Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: EFT must be selected for this payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
                    End If
                Else
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Prior Coverage is required for the selected payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
                End If
            End If
        End If
    End Sub

    Public Sub CheckSR22Monthly(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "MONTHLY" Then
            Dim bIsNonOwner As Boolean = False
            Dim bHasSR22 As Boolean = False
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVehicle.IsMarkedForDelete Then
                    If oVehicle.VinNo.ToUpper = "NONOWNER" Then
                        bIsNonOwner = True
                        Exit For
                    End If
                End If
            Next
            If bIsNonOwner Then
                For Each oDriver As clsEntityDriver In oPolicy.Drivers
                    If Not oDriver.IsMarkedForDelete Then
                        If oDriver.SR22 Then
                            bHasSR22 = True
                            Exit For
                        End If
                    End If
                Next
                If Not bHasSR22 Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Insured Driver does not have a SR22.", "NonOwnerWithoutSR22", "IER", oPolicy.Notes.Count))
                End If
            End If
        End If
    End Sub

    Public Sub CheckVehicleAgeMonthly(ByVal oPolicy As clsPolicyPPA)
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

    Public Sub CheckVehicleAgeClassic(ByVal oPolicy As clsPolicyPPA)
        CalculateVehicleAge(oPolicy, True)

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

    Public Overrides Sub CalculateVehicleAge(ByVal oPolicy As clsPolicyPPA, ByVal bShowTrueAge As Boolean)

        'If oPolicy.Program.ToUpper = "MONTHLY" Or oPolicy.Program.ToUpper = "CLASSIC" Then
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
        'End If
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

    Public Sub CheckDWICount(ByRef oPolicy As clsPolicyPPA)
        ' No more than 2 DWI,DUI,alcohol, drug, or controlled substance violations within the previous 35 months
        ' No one under 21 with DWI
        Dim sDriverListWithMoreThan2DWIViols As String = String.Empty
        Dim sDriverListUnder21WithDWI As String = String.Empty

        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                Dim sDrvUnder21 As String = ""

                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    Dim iDWI As Integer = 0
                    Dim oldestDWIDate As Date = DateTime.Now()
                    Dim isMinorDWI As Boolean = False
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        If oViolation.ViolGroup = "DWI" Then
                            If DateAdd(DateInterval.Month, 35, oViolation.ViolDate) > oPolicy.EffDate Then
                                iDWI += 1
                                If oldestDWIDate > oViolation.ViolDate Then
                                    oldestDWIDate = oViolation.ViolDate
                                End If

                                If oViolation.ViolTypeCode = "11321" Then
                                    isMinorDWI = True
                                End If
                            End If
                        End If
                    Next

                    If iDWI > 2 Then
                        sDrv = oDriver.IndexNum
                    End If

                    If isMinorDWI Or (iDWI > 0 AndAlso DateDiff(DateInterval.Year, oDriver.DOB, oldestDWIDate) < 21) Then
                        sDrvUnder21 = oDriver.IndexNum
                    End If
                End If

                If Len(sDrv) > 0 Then
                    If sDriverListWithMoreThan2DWIViols = String.Empty Then
                        sDriverListWithMoreThan2DWIViols = sDrv
                    Else
                        sDriverListWithMoreThan2DWIViols &= ", " & sDrv
                    End If
                End If

                If Len(sDrvUnder21) > 0 Then
                    If sDriverListUnder21WithDWI = String.Empty Then
                        sDriverListUnder21WithDWI = sDrvUnder21
                    Else
                        sDriverListUnder21WithDWI &= ", " & sDrvUnder21
                    End If
                End If
            End If
        Next

        If sDriverListWithMoreThan2DWIViols <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than 2 drug or alcohol violations - " & sDriverListWithMoreThan2DWIViols & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If

        If sDriverListUnder21WithDWI <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have drug or alcohol violations prior to the age of 21 - " & sDriverListUnder21WithDWI & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If
    End Sub

    Public Sub CheckDWICountMonthly(ByRef oPolicy As clsPolicyPPA)
        ' No more than 2 DWI,DUI,alcohol, drug, or controlled substance violations within the previous 35 months
        Dim sDriverList As String = ""

        sDriverList = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                Dim sDrv As String = ""
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    Dim iDWI As Integer = 0
                    For Each oViolation As clsBaseViolation In oDriver.Violations
                        Dim sViolCode As String = String.Empty
                        If oViolation.ViolCode.Length > 0 AndAlso oViolation.ViolCode.Contains(":") Then
                            sViolCode = oViolation.ViolCode.Substring(0, oViolation.ViolCode.IndexOf(":"))
                        End If
                        If sViolCode = "1" Or sViolCode = "2" Or sViolCode = "16" Or sViolCode = "23" Or sViolCode = "50" Then
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

    Public Sub CheckVINALL(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If oVehicle.VehicleSymbolCode <> String.Empty Then
                    If oVehicle.VehicleYear < 2011 Then
                        If oVehicle.VinNo.ToUpper <> "NONOWNER" And oVehicle.VehicleSymbolCode.Trim <> "999" And oVehicle.VehicleSymbolCode.Trim <> "65" And oVehicle.VehicleSymbolCode.Trim <> "66" And oVehicle.VehicleSymbolCode.Trim <> "67" And oVehicle.VehicleSymbolCode.Trim <> "68" Then
                            If Not oVehicle.ValidVIN Then
                                If sVehicleList = String.Empty Then
                                    sVehicleList = oVehicle.IndexNum
                                Else
                                    sVehicleList &= ", " & oVehicle.IndexNum
                                End If
                            End If
                        End If
                    Else
                        If oVehicle.VinNo.ToUpper <> "NONOWNER" And oVehicle.VehicleSymbolCode.Trim <> "999" And oVehicle.VehicleSymbolCode.Trim <> "965" And oVehicle.VehicleSymbolCode.Trim <> "966" And oVehicle.VehicleSymbolCode.Trim <> "967" And oVehicle.VehicleSymbolCode.Trim <> "968" Then
                            If Not oVehicle.ValidVIN Then
                                If sVehicleList = String.Empty Then
                                    sVehicleList = oVehicle.IndexNum
                                Else
                                    sVehicleList &= ", " & oVehicle.IndexNum
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            Dim bAddNote As Boolean
            bAddNote = True
            For Each oNoteEntry As clsBaseNote In oPolicy.Notes
                If oNoteEntry.NoteDesc.ToUpper = "MISSINGVIN" And oNoteEntry.SourceCode.ToUpper = "IER" Then
                    bAddNote = False
                    Exit For
                End If
            Next

            If bAddNote Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an invalid VIN - " & sVehicleList & ".", "InvalidVIN", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Sub CheckProgramRestrictions(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "MONTHLY" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Effective 9/12/12, Imperial is no longer accepting LA Monthly policies.", "IneligibleProgram", "IER", oPolicy.Notes.Count))
        End If
    End Sub

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
                    If (oDriver.LicenseStatus = "ID Only") Then
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
#End Region

#Region "Warning Functions"
    Public Sub CheckPayPlanWarning(ByRef oPolicy As clsPolicyPPA)

        With oPolicy
            If .PayPlanCode = "16C" Then
                .Notes = (AddNote(.Notes, "Warning: The first Installment for the selected Pay Plan is due in 17 days.", "PAYPLAN", "WRN", .Notes.Count))
            End If
        End With
    End Sub

    Public Sub CheckPhysicalDamageWarning(ByRef oPolicy As clsPolicyPPA)

        Dim sVehicleList As String = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits.FindAll(Function(value As clsVehicleUnit)
                                                                                Return value.VinNo <> "NONOWNER"
                                                                            End Function)
            Dim sVeh As String = ""
            If oPolicy.CallingSystem.ToUpper.Contains("WEBRATER") And (Not IsRewritePolicy(oPolicy) Or oVehicle.IsModified) Then
                If DeterminePhysDamageExists(oVehicle) Then
                    sVeh = oVehicle.IndexNum
                End If
            ElseIf oPolicy.CallingSystem.ToUpper.Contains("OLE") Then
                If DeterminePhysDamageAddedOnEndorsement(oVehicle) Then
                    sVeh = oVehicle.IndexNum
                End If
            End If
            If Len(sVeh) > 0 Then
                If sVehicleList = String.Empty Then
                    sVehicleList = sVeh
                Else
                    sVehicleList &= ", " & sVeh
                End If
            End If
        Next

        If sVehicleList <> String.Empty Then
            If oPolicy.CallingSystem.ToUpper.Contains("WEBRATER") Then
                If oPolicy.Program.ToUpper <> "DIRECT" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: The following vehicle(s) with Physical Damage coverage require an Inspection Report - " & sVehicleList & ".", "PhysicalDamageWarning", "WRN", oPolicy.Notes.Count))
                End If
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: The following vehicle(s) with Physical Damage added require an Inspection Report - " & sVehicleList & ".", "PhysicalDamageAddedWarning", "WRN", oPolicy.Notes.Count))
            End If
        End If

    End Sub

#End Region

#Region "Helper Functions"

    Public Overrides Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyPPA) As String

        Dim sItemsToBeFaxedIn As String = ""

        sItemsToBeFaxedIn &= MyBase.ItemsToBeFaxedIn(oPolicy)

        'companion
        If oPolicy.CompanionHOMCarrierName.Trim <> "" And (Not IsRewritePolicy(oPolicy)) Then
            sItemsToBeFaxedIn &= "Copy of Companion Homeowner Insurance Policy" & vbNewLine
        End If

        'etch
        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVeh.IsMarkedForDelete And (Not IsRewritePolicy(oPolicy) Or oVeh.IsModified) Then
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


        'vehicle inspection form
        Dim mbAddVehicleInspection As Boolean = False
        Dim msVehInspArr() As String = Nothing

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVeh.IsMarkedForDelete Then
                If oPolicy.TransactionNum > 1 Then      'for endorsements
                    For Each oNotes As clsBaseNote In oPolicy.Notes
                        If oNotes.NoteDesc = "PhysicalDamageAddedWarning" And oNotes.PolicyTransactionNum = oPolicy.TransactionNum Then
                            msVehInspArr = oNotes.NoteText.Substring(oNotes.NoteText.IndexOf("-") + 2, Len(oNotes.NoteText) - (oNotes.NoteText.IndexOf("-") + 2)).Split(",")
                            '...Parse msVehInspList 
                            Dim x As Integer
                            For x = 0 To msVehInspArr.Length - 1
                                Select Case x
                                    Case msVehInspArr.Length - 1
                                        msVehInspArr(x) = msVehInspArr(x).Replace(".", "").Trim(" ")
                                    Case Else
                                        msVehInspArr(x) = msVehInspArr(x).Trim(" ")
                                End Select
                            Next
                            Dim msVIList As New List(Of String)(msVehInspArr)
                            If msVIList.Contains(oVeh.IndexNum) Then
                                sItemsToBeFaxedIn &= "Vehicle Inspection Form for " & oVeh.VehicleYear & " " & oVeh.VehicleMakeCode & " " & oVeh.VehicleModelCode & vbNewLine
                            End If
                            mbAddVehicleInspection = True
                            Exit For
                        End If
                    Next
                Else
                    If Not IsRewritePolicy(oPolicy) Or oVeh.IsModified Then
                        For Each oCov As clsBaseCoverage In oVeh.Coverages
                            If oCov.CovGroup = "OTC" Or oCov.CovGroup = "COL" Or oCov.CovGroup = "UUMPD" Then
                                sItemsToBeFaxedIn &= "Vehicle Inspection Form for " & oVeh.VehicleYear & " " & oVeh.VehicleMakeCode & " " & oVeh.VehicleModelCode & vbNewLine
                                mbAddVehicleInspection = True
                                Exit For
                            End If
                        Next
                    End If
                End If
            End If
        Next

        'UM Form
        If oPolicy.Program.ToUpper = "DIRECT" Then
            Dim lBILimit As Long = 0
            Dim lUMBILimit As Long = 0
            Dim bHasUMBI As Boolean = False
            Dim msSplitLimits() As String

            For Each oVeh2 As clsVehicleUnit In oPolicy.VehicleUnits
                For Each oCov2 As clsBaseCoverage In oVeh2.Coverages
                    Select Case oCov2.CovGroup
                        Case "BI"
                            msSplitLimits = oCov2.CovLimit.Split("/")
                            lBILimit = CLng(msSplitLimits(0)) * 1000
                        Case "UUMBI", "UMBI"
                            msSplitLimits = oCov2.CovLimit.Split("/")
                            lUMBILimit = CLng(msSplitLimits(0)) * 1000
                            bHasUMBI = True
                    End Select
                Next
            Next

            If lUMBILimit < lBILimit Or Not bHasUMBI Then
                sItemsToBeFaxedIn &= "Need Signed UM Form " & vbNewLine
            End If

        End If

        Return sItemsToBeFaxedIn

    End Function

    Public Overrides Sub AddAutoApplyFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Call MyBase.AddAutoApplyFactors(oPolicy)

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
                                If oDrv.SR22 Then
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
                End Select
            Next
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally

        End Try
    End Sub
#End Region

End Class
