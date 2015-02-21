Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data
Imports System.Data.SqlClient

Public Class clsRules235
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

                    Case "CLASSIC", "DIRECT", "MONTHLY"
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

#Region "IER Functions"

    Public Overrides Sub CheckPhysicalDamageRestriction(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

        If oPolicy.Program.ToUpper <> "SUMMIT" Then
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
        End If
    End Sub



    Public Overrides Function CheckPhysicalDamageRestriction(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleID As String = ""

        'If sProgram.ToUpper <> "SUMMIT" Then
        Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
        If bPhysDamage Then
            If oVehicle.VehicleYear < Now.AddYears(-15).Year Then
                sVehicleID = oVehicle.IndexNum
                If oNoteList Is Nothing Then
                    Return sVehicleID
                Else
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 15 years - " & sVehicleID & ".", "PhysDamageOver15", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If
        'End If
        Return sVehicleID
    End Function

    Public Sub CheckRentToOwn(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""

                sVeh = CheckRentToOwn(oVehicle, oPolicy.Program)
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Rent To Own is not available for the program selected.  The following vehicle(s) are listed as Rent To Own - " & sVehicleList & ".", "RentToOwn", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckRentToOwn(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        If sProgram.ToUpper = "SUMMIT" Or sProgram.ToUpper = "MONTHLY" Then
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
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a symbol greater than 22/57 - " & sVehicleList & ".", "SymbolOver22", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckSymbol(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
            If oVehicle.VehicleSymbolCode <> String.Empty Then
                Try
                    If oVehicle.VehicleYear < 2011 Then
                        If CInt(oVehicle.VehicleSymbolCode.Trim) > 24 And oVehicle.VinNo.ToUpper <> "NONOWNER" And CInt(oVehicle.VehicleSymbolCode.Trim) <> 999 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 65 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 66 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 67 And CInt(oVehicle.VehicleSymbolCode.Trim) <> 68 Then
                            sVehicleList = oVehicle.IndexNum

                            If Not oNoteList Is Nothing Then
                                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than 24 - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                                Return ""
                            End If
                        End If
                    Else
                        '2011 and above uses new symbols
                        If CInt(oVehicle.CollSymbolCode.Trim) > 57 And oVehicle.VinNo.ToUpper <> "NONOWNER" And oVehicle.VehicleSymbolCode.Trim <> "999" And oVehicle.VehicleSymbolCode.Trim <> "965" And oVehicle.VehicleSymbolCode.Trim <> "966" And oVehicle.VehicleSymbolCode.Trim <> "967" And oVehicle.VehicleSymbolCode.Trim <> "968" Then
                            sVehicleList = oVehicle.IndexNum
                            If Not oNoteList Is Nothing Then
                                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have a symbol greater than 57 - " & sVehicleList & ".", "SymbolOver22", "IER", oNoteList.Count, "AOLE"))
                                Return ""
                            End If
                        End If
                    End If
                Catch Ex As Exception
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
            If ((oVehicle.VehicleSymbolCode = "66" And oVehicle.VehicleYear < 2011) Or (oVehicle.VehicleSymbolCode = "966" And oVehicle.VehicleYear >= 2011)) And oVehicle.VehicleModelCode.ToUpper <> "LIGHT TRUCK" And CInt(oVehicle.VehicleYear) > 1993 Then
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

    Public Sub CheckUnLicensedNamedInsuredSummit(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "SUMMIT" Then
            ' Rule 4.A (named insureds that have never been licensed, unless the named insured is excluded from coverage)
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

        If oPolicy.Program.ToUpper = "SUMMIT" Then
            ' Rule 4.B (drivers under the minimum age for state licensing)
            sDriverList = ""
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

    Public Sub CheckDriverViolationsSummit(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""


        ' Rule 4.C (drivers with greater than 12 violations or greater than 30 points in the Chargeable Period)
        sDriverList = ""

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sDriver As String = ""
                sDriver = CheckDriverViolationsSummit(oDrv, oPolicy.Program)

                If Len(sDriver) > 0 Then
                    If sDriverList = "" Then
                        sDriverList = oDrv.IndexNum
                    Else
                        sDriverList &= ", " & oDrv.IndexNum
                    End If
                End If
            End If
        Next
        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than 12 violations - " & sDriverList & ".", "MaxDriverViols", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Function CheckDriverViolationsSummit(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""
        Dim iNumOfViols As Integer = 0

        ' Rule 4.C (drivers with greater than 12 violations or greater than 30 points in the Chargeable Period)
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
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than 12 violations - " & sDriverList & ".", "MaxDriverViols", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Sub CheckOutOfStateGaragingSummit(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        ' Rule 4.E (vehicles with a principal out of state garaging location)
        If oPolicy.Program.ToUpper = "SUMMIT" Then
            sVehicleList = ""
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete Then
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


    Public Sub CheckArtisanUseSummit(ByRef oPolicy As clsPolicyPPA)
        Dim iCounter As Integer = 0

        ' Rule 4.L (risks with 2 or more artisan use vehicles)
        iCounter = 0
        If oPolicy.Program.ToUpper = "SUMMIT" Then
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete Then
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
    Public Sub CheckPayPlanClassic(ByRef oPolicy As clsPolicyPPA)

        ' Pay Plans with < 25% Down require prior coverage
        If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Then
            If oPolicy.UWTier = 91 Then
                If oPolicy.PayPlanCode <> "100" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: 6 Months Prior Coverage is required for the selected payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
                End If
            End If
        End If

    End Sub


    Public Overridable Function CheckDriverPoints15ClassicMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.c (Age 15-18 with more than 3 points; or age 19-21 with more than 5 points.)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If (oDriver.Age >= 15 And oDriver.Age <= 18) Then
                    If oDriver.Points > 3 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 3 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
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
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If (oDriver.Age >= 19 And oDriver.Age <= 21) Then
                    If oDriver.Points > 3 Then
                        sDriverList = oDriver.IndexNum

                        If Not oNoteList Is Nothing Then
                            oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE")
                            Return ""
                        End If
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Function CheckDriverPoints30ClassicMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""
        Dim iMaxPoints As Integer = 30

        Try
            iMaxPoints = Integer.Parse(GetProgramSetting("MaxDriverPoints"))
        Catch ex As Exception
        End Try

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                If oDriver.Points > iMaxPoints Then
                    sDriverList = oDriver.IndexNum

                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following driver(s) have greater than " & iMaxPoints.ToString() & " points - " & sDriverList & ".", "MaxDriverPoints", "IER", oNoteList.Count, "AOLE"))
                        Return ""
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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 15 to 18 years old, have more than 3 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
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
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s), aged 19 to 21 years old, have more than 5 points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count)
        End If

        sDriverList = ""
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sDrv As String = ""
                sDrv = CheckDriverPoints30ClassicMonthly(oDrv, oPolicy.Program)

                If Len(sDrv) > 0 Then
                    If sDriverList = String.Empty Then
                        sDriverList = sDrv
                    Else
                        sDriverList &= ", " & sDrv
                    End If
                End If
            End If
        Next

        Dim iMaxPoints As Integer = 30

        Try
            iMaxPoints = Integer.Parse(GetProgramSetting("MaxDriverPoints"))
        Catch ex As Exception
        End Try

        If sDriverList <> "" Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have greater than " & iMaxPoints.ToString() & " points - " & sDriverList & ".", "MaxDriverPoints", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Sub CheckChargeableClassicMonthly(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than two (2) chargeable alcohol/drug/narcotic related violation (of any kind).)
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
        If sDriverList <> String.Empty Then
            oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: The following driver(s) have more than 2 chargeable alcohol/drug/narcotic related violation - " & sDriverList & ".", "ChargeableDWICount", "IER", oPolicy.Notes.Count)
        End If

    End Sub

    Public Overridable Function CheckChargeableClassicMonthly(ByRef oDriver As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sDriverList As String = ""

        ' Rule 1.d (Having more than two (2) chargeable alcohol/drug/narcotic related violation (of any kind).)
        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                Dim iChargeableDWI As Integer = 0
                For Each oViolation As clsBaseViolation In oDriver.Violations
                    If oViolation.ViolGroup = "DWI" And oViolation.Chargeable Then
                        iChargeableDWI += 1
                    End If
                Next
                If iChargeableDWI > 2 Then
                    sDriverList = oDriver.IndexNum
                    If Not oNoteList Is Nothing Then
                        oNoteList = AddNote(oNoteList, "Ineligible Risk: The following driver(s) have more than 2 chargeable alcohol/drug/narcotic related violation - " & sDriverList & ".", "ChargeableDWICount", "IER", oNoteList.Count, "AOLE")
                    End If
                End If
            End If
        End If

        Return sDriverList
    End Function

    Public Overridable Sub CheckSummitDisabled(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            If .Program.ToUpper = "SUMMIT" And Now() > CDate("1/1/2011") Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: We are not currently accepting new business with the Summit program.", "SUMMITDISABLED", "IER", .Notes.Count))
            End If
        End With
    End Sub


    Public Overridable Function CheckPhysicalDamageOnlyClassicMonthly(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
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

    Public Overridable Function CheckPhysicalDamageOldClassicMonthly(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        If sProgram.ToUpper = "CLASSIC" Or sProgram.ToUpper = "DIRECT" Or sProgram.ToUpper = "MONTHLY" Then
            Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
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

        Return sVehicleList
    End Function

    Public Sub CheckPhysicalDamageClassicMonthly(ByRef oPolicy As clsPolicyPPA)
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
        If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Or oPolicy.Program.ToUpper = "MONTHLY" Then
            If oPolicy.UWQuestions.Count > 0 Then
                For Each oUWQ As clsUWQuestion In oPolicy.UWQuestions
                    Select Case oUWQ.QuestionCode
                        Case "304"
                            If Left(oUWQ.AnswerText.ToUpper, 3) = "YES" Then
                                If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Then
                                    oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Vehicles cannot be used for business or commercial purposes.", "BusinessUse", "IER", oPolicy.Notes.Count)
                                End If
                            End If
                            'Case "307"
                            'If Left(oUWQ.AnswerText.ToUpper, 3) = "YES" Then
                            '    sVehicleList = String.Empty
                            '    For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                            '        If Not oVehicle.IsMarkedForDelete Then
                            '            Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
                            '            If bPhysDamage Then
                            '                If sVehicleList = String.Empty Then
                            '                    sVehicleList = oVehicle.IndexNum
                            '                Else
                            '                    sVehicleList &= ", " & oVehicle.IndexNum
                            '                End If
                            '            End If
                            '        End If
                            '    Next
                            '    If sVehicleList <> String.Empty Then
                            '        oPolicy.Notes = AddNote(oPolicy.Notes, "Ineligible Risk: Vehicle(s) with Physical Damage cannot have been re-built, salvaged or water damaged - " & sVehicleList & ".", "ReBuiltSalvagedVeh", "IER", oPolicy.Notes.Count)
                            '    End If
                            'End If
                    End Select
                Next
            End If
        End If
    End Sub

    Public Sub CheckSR22Monthly(ByRef oPolicy As clsPolicyPPA)
        Dim bIsNonOwner As Boolean = False
        Dim bHasSR22 As Boolean = False
        If oPolicy.Program.ToUpper = "MONTHLY" Then
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

    Public Sub CheckVIN(ByVal oPolicy As clsPolicyPPA)
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
            If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Then
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

        Return sItemsToBeFaxedIn
    End Function


    Public Overrides Sub CheckPolicyPoints(ByVal oPolicy As clsPolicyPPA)
        ' 6/20/2011 2 point restriction removed for OK
    End Sub

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
                End Select
            Next

            ' Add a dummy multicar factor so that it appears on the dec page
            ' actual multicar discount for OK is handled by FactorPolicyDiscountMatrix
            If oPolicy.VehicleCount(True) > 1 Then
                If Not FactorOnPolicy(oPolicy, "MULTICAR") Then
                    AddPolicyFactor(oPolicy, "MULTICAR")
                End If
            End If

            'if the program does not have a Foreign License factor then we need to check to see if we need to add a UDR viol
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If Not oDrv.IsMarkedForDelete Then
                    If oDrv.IndexNum < 98 Then
                        If oDrv.DLNState = "FN" Or oDrv.DLNState = "IT" Then
                            If ApplyUDR(oPolicy, oDrv) Then
                                AddViolation(oPolicy, oDrv, "55559", "UNVERIFIABLE DRIVING RECORD", "V", "UDR", "M", oPolicy.EffDate)
                            End If
                        End If

                        If HasViolation(oDrv, "55559") And Not (oDrv.DLNState = "FN" Or oDrv.DLNState = "IT") Then
                            If Not FactorOnDriver(oDrv, "UDR") Then
                                AddDriverFactor(oPolicy, oDrv, "UDR")
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

    Public Overrides Function PolicyHasIneligibleRisk(ByVal oPolicy As CorPolicy.clsPolicyPPA) As Boolean
        Dim bIneligibleRisk As Boolean = False
        Dim sReason As String = String.Empty

        'Drivers (New business and new driver added to an existing policy will not be rated or 
        '   written if they fall into one of the categories below.  
        '   Renewal drivers who fall into one of these categories will 
        '   receive and ineligible risk surcharge.)
        Dim iTotalPoints As Integer = 0
        Dim iMaxSymbol As Integer
        Dim iBusinessUseCount As Integer
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            CheckViolations(oDriver, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, "B")

            If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then

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

                If oDriver.Points > 12 Then
                    sReason = "Driver with more than 12 points.- " & oDriver.IndexNum
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

            iBusinessUseCount = 0
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
                                'If CInt(oVeh.VehicleSymbolCode.Trim) >= 25 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
                                If CInt(oVeh.VehicleSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
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
                                'If CInt(oVeh.VehicleSymbolCode.Trim) >= 58 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
                                If CInt(oVeh.CollSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
                                    sReason = "Vehicle with physical damage symbol " & iMaxSymbol.ToString & " - " & oVeh.IndexNum
                                    bIneligibleRisk = True
                                    Exit For
                                End If
                            Catch Ex As Exception
                            End Try
                        End If
                    End If


                    '4.     Vehicles over 15 years old are unacceptable for all physical damage coverage on new policies. 
                    'If oVeh.VehicleAge > 15 AndAlso DeterminePhysDamageExists(oVeh) Then
                    If oVeh.VehicleYear < Now.AddYears(-15).Year AndAlso DeterminePhysDamageExists(oVeh) Then
                        sReason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '5.     Vehicles over 40 years old are unacceptable for all coverages.
                    If oVeh.VehicleAge > 40 Then
                        sReason = "Vehicles over 40 years old are unacceptable for all coverages.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '6.
                    If Not ValidateVehicleZipCode(oVeh.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                        sReason = "Vehicles is garaged out of state.- " & oVeh.IndexNum
                        bIneligibleRisk = True
                        Exit For
                    End If

                    '7.     Vehicles that have a title or registration indicating that the vehicle has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.
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


    Private Function ApplyUDR(ByVal oPolicy As clsPolicyPPA, ByVal oDrv As clsEntityDriver)

        Dim bApply As Boolean = True
        For Each oViol As clsBaseViolation In oDrv.Violations
            If oViol.ViolGroup.ToUpper = "UDR" And oViol.ViolTypeCode = "55559" Then
                bApply = False
                Exit For
            End If
        Next

        ' Check rate date to ensure this was rated on or after 9/18/2009 when the 
        ' UDR was added for OK
        Dim sOKUDRViolStartDate As String
        sOKUDRViolStartDate = GetStateInfoValue(oPolicy, oPolicy.Program, "UDR", "VIOLATION", "DATE")

        If bApply Then
            If CDate(oPolicy.RateDate) < CDate(sOKUDRViolStartDate) Then
                ' Don't add the UDR if it was rated prior to the OK UDR Viol Start Date
                bApply = False
            End If
        End If

        Return bApply
    End Function

    Public Overrides Sub AddExclFactor(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String)

        Dim bHasExcl As Boolean = False

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
            If Not FactorOnPolicy(oPolicy, sFactorCode) Then
                AddPolicyFactor(oPolicy, sFactorCode)
            End If
        End If

    End Sub

#End Region

End Class
