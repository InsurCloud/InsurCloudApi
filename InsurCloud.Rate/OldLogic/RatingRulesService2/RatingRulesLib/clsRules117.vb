Imports Microsoft.VisualBasic
Imports CorPolicy
Imports CorPolicy.clsCommonFunctions
Imports System.Data.SqlClient
Imports System.Data
Imports System.Collections.Generic
Imports System.Configuration

Public Class clsRules117
    Inherits clsRules1


    Public Overridable Sub CheckHOTDisabled(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .Program = "HO30T" And Now() > CDate("3/1/2011") Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: We are not currently accepting new business with the HO-T program.", "HOTDISABLED", "IER", .Notes.Count))
            End If
        End With
    End Sub


    Public Overridable Sub CheckHomeAgeOver35(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).HomeAge > 35 And oPolicy.DwellingUnits(0).HomeAge < 300 Then
                .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Homes over 35 years of age require prior approval from the Underwriting Department.", "HOMEAGEOVER35", "UWW", .Notes.Count))
            End If
        End With
    End Sub


    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim parent As New clsRules1

        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""

        Try
            If parent.CheckNEI(oPolicy) Then

                If Not IsNumeric(oPolicy.DwellingUnits.Item(0).LiaLimit) Then
                    bEnoughInfoToRate = False
                    sMissing += "LiaLimit" & "-"
                Else
                    If Not oPolicy.Program.StartsWith("DW") Then
                        If oPolicy.DwellingUnits.Item(0).LiaLimit = 0 Then
                            bEnoughInfoToRate = False
                            sMissing += "LiaLimit" & "-"
                        End If
                    End If
                End If
                If Not IsNumeric(oPolicy.DwellingUnits.Item(0).MedPayLimit) Then
                    bEnoughInfoToRate = False
                    sMissing += "MedPayLimit" & "-"
                Else
                    If Not oPolicy.Program.StartsWith("DW") Then
                        If oPolicy.DwellingUnits.Item(0).MedPayLimit = 0 Then
                            bEnoughInfoToRate = False
                            sMissing += "MedPayLimit" & "-"
                        End If
                    End If
                End If

                ''Endorsement Info *****Need to add****
                For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
                    Select Case oEndorse.FactorCode.ToUpper

                        Case "HO206", "HO206-J", "HO206-O"
                            'make sure they have at least one value entered

                        Case "HO207", "HO207-A", "HO207-B", "HO207-C", "HO207-D"
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                Select Case oUWQuestion.QuestionCode.ToUpper
                                    Case "H02" 'length of watercraft
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO215 Length of Watercraft" & "-"
                                        End If
                                    Case "H03" 'motor type
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO215 Motor Type" & "-"
                                        End If
                                    Case "H05" 'max speed
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO215 Max Speed" & "-"
                                        End If
                                End Select
                            Next

                        Case "HO208", "HO208-O", "HO208-T"
                            'need occupied by

                        Case "DW211"
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                If oUWQuestion.QuestionCode = "202" Then
                                    If oUWQuestion.AnswerText = "" Then
                                        bEnoughInfoToRate = False
                                        sMissing += "DW211 Coverage Amount" & "-"
                                    End If
                                End If
                            Next


                    End Select

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

#Region "IER Functions"

    Public Function SelectedDedAmount(ByVal oPolicy As clsPolicyHomeOwner) As Decimal
        With oPolicy
            If .DwellingUnits.Item(0).Ded3 > 0 Then
                Return .DwellingUnits.Item(0).Ded3
            ElseIf .DwellingUnits.Item(0).Ded1 > 0 Then
                Return .DwellingUnits.Item(0).Ded1
            Else
                Return .DwellingUnits.Item(0).Ded1
            End If
        End With
    End Function

    Public Function SelectedDedName(ByVal oPolicy As clsPolicyHomeOwner) As String
        With oPolicy
            'If .DwellingUnits.Item(0).Ded3 > 0 Then
            '    Return "Named Storm"
            'ElseIf .DwellingUnits.Item(0).Ded1 > 0 Then
            '    Return "Wind/Hail"
            'Else
            '    Return "Wind/Hail or Named Storm"
            'End If

            Select Case oPolicy.Program
                Case "HO20", "HO30"
                    Return "Wind/Hail or Named Storm"
                Case Else
                    Return "Wind/Hail"
            End Select
        End With
    End Function

    Public Sub CheckWindHailORNamedStorm(ByVal oPolicy As clsPolicyHomeOwner)
        If oPolicy.DwellingUnits(0).Ded1 > 0 And oPolicy.DwellingUnits(0).Ded3 > 0 And oPolicy.DwellingUnits(0).DwellingAmt > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Wind/Hail and Named Storm deductibles cannot both be selected. Please select one or the other.", "DED1OR3", "IER", oPolicy.Notes.Count))
        End If

        If oPolicy.DwellingUnits(0).Region = "5" Then
            If Len(oPolicy.DwellingUnits(0).Ded3) > 0 Then
                If oPolicy.DwellingUnits(0).Ded3 > 0 And oPolicy.DwellingUnits(0).DwellingAmt > 0 Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Named Storm deductible cannot be purchased in region 5.", "NamedStorm5", "IER", oPolicy.Notes.Count))
                End If
            End If
        End If
    End Sub
    Public Sub CheckDed1Ded3GTEDed2(ByVal oPolicy As clsPolicyHomeOwner)
        Dim DedAmt As Double = SelectedDedAmount(oPolicy)
        Dim DedName As String = SelectedDedName(oPolicy)
        Dim isLessThanDed2 As Boolean = False

        If (DedAmt < 1 And oPolicy.DwellingUnits(0).Ded2 < 1) Or (DedAmt > 1 And oPolicy.DwellingUnits(0).Ded2 > 1) Then
            'If Both are percents or Both are Flat Rate, we compare directly
            If DedAmt < oPolicy.DwellingUnits(0).Ded2 Then
                isLessThanDed2 = True
            End If
        Else
            'Not The same type, so we find which is the percent and find out the equivalent flat rate and compare.
            If DedAmt < 1 And oPolicy.DwellingUnits(0).Ded2 > 1 Then
                If DedAmt * oPolicy.DwellingUnits(0).DwellingAmt < oPolicy.DwellingUnits(0).Ded2 Then
                    isLessThanDed2 = True
                End If
            ElseIf DedAmt > 1 And oPolicy.DwellingUnits(0).Ded2 < 1 Then
                If DedAmt < oPolicy.DwellingUnits(0).Ded2 * oPolicy.DwellingUnits(0).DwellingAmt Then
                    isLessThanDed2 = True
                End If
            End If
        End If

        If isLessThanDed2 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: " & DedName & " deductible must be equal to or greater than the All Other Peril Deductible.", "DED1OR3GTEDED2", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckDisallowedParish(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).County.ToUpper = "SAINT TAMMANY" Then
                If Not AllowStTammany(oPolicy) Then
                    'Saint Tammany no longer allowed
                    .Notes = (AddNote(.Notes, "Ineligible Risk: We are currently not accepting new business in this region of Saint Tammany parish", "SaintTammanyParish", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Function AllowStTammany(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim oMktCRMService As New MarketingCRMService.InsurCloudAMSServiceSoapClient
        Dim dsAgencyOptions As New DataSet
        Dim bAllowStTammany As Boolean = False


        'We would like to lift the LA Property St Tammany restriction in region 3 for all agents.  
        'We currently have one agent who has had the restriction lifted in region 3.  
        'Below is a list of zips codes we would like to open.  
        'All other zip codes in St Tammany not listed here are still closed to all agents for new business.

        '70420
        '70427
        '70431
        '70433
        '70434
        '70435
        '70437
        '70463
        '70464

        ' uncomment this to only allow for agents with the "allowsttammany" setting in marketingcrm
        'dsAgencyOptions = oMktCRMService.GetOptions(oPolicy.Agency.AgencyID)
        'For Each oRow As DataRow In dsAgencyOptions.Tables(0).Rows
        '	If oRow.Item("EditValue").ToString.ToUpper = "ALLOWSTTAMMANY" Then
        '		bAllowStTammany = True
        '		Exit For
        '	End If
        'Next

        'If bAllowStTammany Then
        'If oPolicy.DwellingUnits(0).Zip = "70420" Then
        '	Return True
        'End If

        ' check to see if it's a St. Tammany Zip
        Dim arrStTammanyZips(8) As String
        arrStTammanyZips(0) = "70420"
        arrStTammanyZips(1) = "70427"
        arrStTammanyZips(2) = "70431"
        arrStTammanyZips(3) = "70433"
        arrStTammanyZips(4) = "70434"
        arrStTammanyZips(5) = "70437"
        arrStTammanyZips(6) = "70437"
        arrStTammanyZips(7) = "70463"
        arrStTammanyZips(8) = "70464"

        Dim inStTammanyArray As String = Array.FindIndex(arrStTammanyZips, Function(s) s.Contains(oPolicy.DwellingUnits(0).Zip))
        If inStTammanyArray > 0 Then
            Return True
        End If

        'End If
        Return False
    End Function

    Public Overridable Sub CheckACVLessThanRC(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            'Insurance to Value
            If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                If UseActualCashValue(oPolicy) Then
                    If .DwellingUnits.Item(0).ReplacementCashAmt > 0 Then
                        If .DwellingUnits.Item(0).ActualCashAmt < (.DwellingUnits.Item(0).ReplacementCashAmt * 0.8) Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: The actual cash value can not be less than 80% of the full replacement cost of the risk", "ACVLessThanRC", "IER", .Notes.Count))
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckDwellingLessThanRC(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim bHasDwellingReplacementCost As Boolean = False
            For Each oPolFactor As clsBaseFactor In .PolicyFactors
                If oPolFactor.FactorCode.ToUpper = "REPLACE_FULL" Then
                    bHasDwellingReplacementCost = True
                End If
            Next
            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                Select Case oEndorse.Type.ToUpper
                    Case "DWELLINGREPLACEMENTCOST"
                        bHasDwellingReplacementCost = True
                End Select
            Next
            If bHasDwellingReplacementCost Then
                If .DwellingUnits.Item(0).DwellingAmt < .DwellingUnits.Item(0).ReplacementCashAmt Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Dwellings should be insured for a minimum of the replacement cost when the policy contains replacement cost coverage on the dwelling", "DwellingLessThanRC", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckDwellingLessThanACV(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            'Insurance to Value
            If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                Dim bHasDwellingReplacementCost As Boolean = False
                For Each oPolFactor As clsBaseFactor In .PolicyFactors
                    If oPolFactor.FactorCode.ToUpper = "REPLACE_FULL" Then
                        bHasDwellingReplacementCost = True
                    End If
                Next
                For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                    Select Case oEndorse.Type.ToUpper
                        Case "DWELLINGREPLACEMENTCOST"
                            bHasDwellingReplacementCost = True
                    End Select
                Next

                If Not bHasDwellingReplacementCost Then
                    If .DwellingUnits.Item(0).DwellingAmt < .DwellingUnits.Item(0).ActualCashAmt Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Dwellings should be insured for a minimum of the actual cash value of the dwelling", "DwellingLessThanACV", "IER", .Notes.Count))
                    End If
                End If
            End If

        End With
    End Sub

    Public Function CheckRegionDeductibleLimits(ByVal DedAmount As Decimal, ByVal DedNum As Integer, ByVal Region As String, ByVal DwellingAmount As Integer, ByVal MinAmount As Decimal) As String
        Dim DedDollarAmount As Decimal = 0.0
        Dim MinDollarAmount As Decimal = 1000.0
        Dim limit As String = String.Empty

        If DedAmount < 1 Then
            DedDollarAmount = DwellingAmount * DedAmount
        Else
            DedDollarAmount = DedAmount
        End If
        If MinAmount >= MinDollarAmount Then
            Select Case DedNum
                Case 1, 3
                    Select Case Region
                        Case "1"
                            MinDollarAmount = 0.01 * DwellingAmount
                            limit = "1%"
                        Case "2", "5"
                            MinDollarAmount = 0.01 * DwellingAmount
                            limit = "1%"
                        Case "3", "4", "6"
                            MinDollarAmount = MinAmount
                            limit = "$" & MinAmount & " Flat"
                    End Select
                Case 2
                    MinDollarAmount = 1000
            End Select
        End If
        If DedDollarAmount < MinDollarAmount Then
            'does not meet limit requirements
            If DedDollarAmount < MinAmount Then
                'if limit would be less than $1000, default to $1000
                limit = "$" & MinAmount & " Flat"
            End If
            Return limit
        Else
            Return String.Empty
        End If

        Return String.Empty
    End Function

    Public Overridable Sub CheckDeductibles2(ByVal oPolicy As clsPolicyHomeOwner)
        'New Region rules 10/06/09 DRS
        Dim oQGNNote As clsBaseNote = GetNote(oPolicy, "QGN", "QuoteGen")
        Dim Ded1Name As String = SelectedDedName(oPolicy)
        Dim Ded1Amount As Decimal = SelectedDedAmount(oPolicy)
        Dim Ded2Amount As Decimal = oPolicy.DwellingUnits(0).Ded2
        Dim Ded2Name As String = "All Other Peril"
        Dim Ded1Result As String = String.Empty
        Dim Ded2Result As String = String.Empty

        With oPolicy
            If .DwellingUnits(0).DwellingAmt > 0 Or .Program.ToUpper = "HO30T" Then
                Select Case .Program.ToUpper
                    Case "HO30T"
                        'Only has ded1
                        Ded1Result = CheckRegionDeductibleLimits(Ded1Amount, 1, .DwellingUnits(0).Region, .DwellingUnits(0).ContentsAmt, 250)
                        Ded1Name = ""
                    Case "DW10"
                        'Ded2 required
                        'if has EC then check ded1 against Region Rules.

                        Dim HasEC As Boolean = False
                        For Each oCov As clsHomeOwnerCoverage In .DwellingUnits.Item(0).Coverages
                            If oCov.CovGroup.ToUpper = "EC" Then
                                HasEC = True
                                Exit For
                            End If
                        Next

                        If HasEC Then
                            If CommonRulesFunctions.AllowCode("CheckDed1Ded3GTEDed2") And .DwellingUnits(0).DwellingAmt > 0 Then
                                CheckDed1Ded3GTEDed2(oPolicy)
                            End If
                            Ded1Result = CheckRegionDeductibleLimits(Ded1Amount, 1, .DwellingUnits(0).Region, .DwellingUnits(0).DwellingAmt, 1000)
                        End If
                        Ded2Result = CheckRegionDeductibleLimits(Ded2Amount, 2, .DwellingUnits(0).Region, .DwellingUnits(0).DwellingAmt, 1000)

                    Case Else
                        'HO20, HO30, DW20, DW30
                        If .DwellingUnits(0).Ded1 >= 0 Or .DwellingUnits(0).Ded3 >= 0 Then
                            If CommonRulesFunctions.AllowCode("CheckWindHailORNamedStorm") Then
                                CheckWindHailORNamedStorm(oPolicy)
                            End If
                            If CommonRulesFunctions.AllowCode("CheckDed1Ded3GTEDed2") And .DwellingUnits(0).DwellingAmt > 0 Then
                                CheckDed1Ded3GTEDed2(oPolicy)
                            End If
                        End If

                        If Ded1Amount > -1 Then
                            Ded1Result = CheckRegionDeductibleLimits(Ded1Amount, 1, .DwellingUnits(0).Region, .DwellingUnits(0).DwellingAmt, 1000)
                        End If
                        If Ded2Amount > -1 Then
                            Ded2Result = CheckRegionDeductibleLimits(Ded2Amount, 2, .DwellingUnits(0).Region, .DwellingUnits(0).DwellingAmt, 1000)
                        End If

                End Select
            End If

            If Ded1Result <> String.Empty Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose at least a " & Ded1Result & " " & Ded1Name & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
            End If
            If Ded2Result <> String.Empty Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose at least a " & Ded2Result & " " & Ded2Name & " Deductible in this territory.", "Ded2", "IER", .Notes.Count))
            End If
        End With

    End Sub
    Public Overridable Sub CheckDeductibles(ByVal oPolicy As clsPolicyHomeOwner)
        'REPLACED BY NEW REGION RULES (DRS - 10/06/09)
        Dim oQGNNote As clsBaseNote = GetNote(oPolicy, "QGN", "QuoteGen")
        With oPolicy
            If Not oQGNNote Is Nothing Then
                Dim dtStartDate As Date = oQGNNote.NoteText

                If CDate(dtStartDate) > New Date(2008, 11, 11) Then
                    Select Case .DwellingUnits.Item(0).Zip
                        Case 70445, 70447, 70448, 70458, 70459, 70460, 70461, 70471
                            'ded1 is windHail
                            If SelectedDedAmount(oPolicy) < 0.02 Then 'ded1 not flat and ded < 2%
                                'not allowed
                                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose at least a 2% " & SelectedDedName(oPolicy) & " Deductible in this Zip.", "Ded1", "IER", .Notes.Count))
                            ElseIf (.DwellingUnits.Item(0).DwellingAmt * 0.02) <= SelectedDedAmount(oPolicy) Then 'else flat calc is > 2%
                                'allowed
                            ElseIf SelectedDedAmount(oPolicy) >= 0.02 And SelectedDedAmount(oPolicy) < 1 Then
                                'allowed
                            Else
                                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose at least a 2% " & SelectedDedName(oPolicy) & " Deductible in this Zip.", "Ded1", "IER", .Notes.Count))
                            End If
                    End Select
                End If
            End If


            If .ProgramType = "HOMEOWNERS" Or .ProgramType.Contains("DWELLING") Then
                Dim bDoDed1Check As Boolean = True
                If .ProgramType.ToUpper = "DWELLING1" Then
                    Dim bHasEC As Boolean = False
                    Dim bHasVMM As Boolean = False
                    For Each oCov As clsHomeOwnerCoverage In .DwellingUnits.Item(0).Coverages
                        Select Case oCov.CovGroup.ToUpper
                            Case "EC"
                                bHasEC = True
                            Case "VMM"
                                bHasVMM = True
                        End Select
                    Next
                    If Not bHasEC And Not bHasVMM Then
                        bDoDed1Check = False
                    End If
                End If
                If bDoDed1Check Then
                    Select Case .DwellingUnits.Item(0).Region
                        Case "3", "4", "5"
                            If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                                'ded1 is windHail
                                If SelectedDedAmount(oPolicy) > 1 And SelectedDedAmount(oPolicy) < 1000 Then 'ded1 is flat and ded1 < 1000
                                    'not allowed
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher " & SelectedDedName(oPolicy) & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
                                ElseIf (SelectedDedAmount(oPolicy) * (.DwellingUnits.Item(0).DwellingAmt)) >= 1000 Then 'ded1 is % and >= 1000
                                    'allowed
                                Else
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher " & SelectedDedName(oPolicy) & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
                                End If
                            End If
                        Case "1", "2"
                            'ded1 is windHail
                            If SelectedDedAmount(oPolicy) < 0.02 Then 'ded1 not flat and ded1 < 2%
                                'not allowed
                                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher " & SelectedDedName(oPolicy) & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
                            ElseIf (.DwellingUnits.Item(0).DwellingAmt * 0.02) <= SelectedDedAmount(oPolicy) Then 'else flat calc is > 2%
                                'allowed
                            ElseIf SelectedDedAmount(oPolicy) >= 0.02 And SelectedDedAmount(oPolicy) < 1 Then
                                'allowed
                            Else
                                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher " & SelectedDedName(oPolicy) & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
                            End If
                        Case Else
                            'no restrictions
                    End Select

                End If

                'ded2 is all other
                If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                    If .DwellingUnits.Item(0).Ded2 < 1000 And .DwellingUnits.Item(0).Ded2 > 1 Then 'ded2 is flat and ded2 < 1000
                        'not allowed
                        .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher All Other Peril Deductible in this territory.", "Ded2", "IER", .Notes.Count))
                    ElseIf (.DwellingUnits.Item(0).Ded2 * (.DwellingUnits.Item(0).DwellingAmt)) >= 1000 Then 'ded2 is % and >= 1000
                        'allowed
                    Else
                        .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher All Other Peril Deductible in this territory.", "Ded2", "IER", .Notes.Count))
                    End If
                    If .DwellingUnits.Item(0).Ded2 = 0.01 Then
                        Dim min As Double = .DwellingUnits.Item(0).DwellingAmt * 0.01
                        If min < 500 Then min = 500 'the minimum ded2 is the greater of 500 or 1%

                        If .DwellingUnits.Item(0).Ded2 < min And .DwellingUnits.Item(0).Ded2 > 1 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher All Other Deductible in this territory.", "Ded2", "IER", .Notes.Count))
                        ElseIf .DwellingUnits.Item(0).DwellingAmt <> 0 And (.DwellingUnits.Item(0).Ded2 * .DwellingUnits.Item(0).DwellingAmt) < min Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher All Other Deductible in this territory.", "Ded2", "IER", .Notes.Count))
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckDed13Limit(ByVal oPolicy As clsPolicyHomeOwner, ByVal dedName As String, ByVal dedAmt As Double)
        With oPolicy

            'ded1 is windHail
            If SelectedDedAmount(oPolicy) > 1 And SelectedDedAmount(oPolicy) < 1000 Then 'ded1 is flat and ded1 < 1000
                'not allowed
                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher " & SelectedDedName(oPolicy) & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
            ElseIf (SelectedDedAmount(oPolicy) * (.DwellingUnits.Item(0).DwellingAmt)) >= 1000 Then 'ded1 is % and >= 1000
                'allowed
            Else
                .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher " & SelectedDedName(oPolicy) & " Deductible in this territory.", "Ded1", "IER", .Notes.Count))
            End If

        End With
    End Sub

    Public Overridable Sub CheckConstructionType(ByVal oPolicy As clsPolicyHomeOwner)
        'With oPolicy
        '	Select Case .DwellingUnits.Item(0).Region
        '		Case "1" 'Case "1", "2"
        '			If Not .DwellingUnits.Item(0).Construction.Contains("Masonry") Then
        '				.Notes = (AddNote(.Notes, "Ineligible Risk: Only masonry or brick veneer construction is allowed in this territory", "Construction", "IER", .Notes.Count))
        '			End If
        '                  'Case Else
        '                  'no restrictions
        '          End Select
        'End With
    End Sub

    Public Overridable Sub CheckConstructionYear(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType <> "TENANT" Then
                Select Case .DwellingUnits.Item(0).Region
                    Case "1", "2"
                        If .DwellingUnits.Item(0).YearOfConstruction < 1970 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Only homes built in 1970 or later are allowed in this territory", "ConstructionYear", "IER", .Notes.Count))
                        End If
                    Case "3", "4", "5", "6"
                        If .DwellingUnits.Item(0).YearOfConstruction < 1950 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Only homes built in 1950 or later are allowed in this territory", "ConstructionYear", "IER", .Notes.Count))
                        End If
                    Case Else
                        'no restrictions
                End Select
            End If
        End With
    End Sub

    Public Overridable Sub CheckDwellingAmounts2(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType = "TENANT" Then
                Select Case .Program.ToUpper
                    Case "HO20", "HO30"
                        Select Case .DwellingUnits.Item(0).Region
                            Case "1"
                                If .DwellingUnits.Item(0).DwellingAmt < 150000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $150,000 or more", "DwellingAmount", "IER", .Notes.Count))
                                End If
                            Case Else
                                If .DwellingUnits.Item(0).DwellingAmt < 125000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $125,000 or more", "DwellingAmount", "IER", .Notes.Count))
                                End If
                        End Select
                        If .DwellingUnits.Item(0).DwellingAmt > 750000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $750,000 or less", "DwellingAmount", "IER", .Notes.Count))
                        ElseIf .DwellingUnits.Item(0).DwellingAmt > 500000 Then
                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Homes valued over $500,000 but less than or equal to $750,000 require underwriting approval.", "DwellingAmount", "UWW", .Notes.Count))
                        End If
                    Case "DW10", "DW20", "DW30"
                        Select Case .DwellingUnits.Item(0).Region
                            Case "1"
                                If .DwellingUnits.Item(0).DwellingAmt > 750000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $750,000 or less", "DwellingAmount", "IER", .Notes.Count))
                                ElseIf .DwellingUnits.Item(0).DwellingAmt > 500000 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Homes valued over $500,000 but less than or equal to $750,000 require underwriting approval.", "DwellingAmount", "UWW", .Notes.Count))
                                End If

                                If .DwellingUnits.Item(0).DwellingAmt < 150000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $150,000 or more", "DwellingAmount", "IER", .Notes.Count))
                                End If
                            Case "2"
                                If .DwellingUnits.Item(0).DwellingAmt > 750000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $750,000 or less", "DwellingAmount", "IER", .Notes.Count))
                                ElseIf .DwellingUnits.Item(0).DwellingAmt > 500000 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Homes valued over $500,000 but less than or equal to $750,000 require underwriting approval.", "DwellingAmount", "UWW", .Notes.Count))
                                End If

                                If .DwellingUnits.Item(0).DwellingAmt < 125000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $125,000 or more", "DwellingAmount", "IER", .Notes.Count))
                                End If
                            Case "3", "4", "5", "6"
                                If .DwellingUnits.Item(0).DwellingAmt > 750000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $750,000 or less", "DwellingAmount", "IER", .Notes.Count))
                                ElseIf .DwellingUnits.Item(0).DwellingAmt > 500000 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Homes valued over $500,000 but less than or equal to $750,000 require underwriting approval.", "DwellingAmount", "UWW", .Notes.Count))
                                End If

                                If .DwellingUnits.Item(0).DwellingAmt < 100000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $100,000 or more", "DwellingAmount", "IER", .Notes.Count))
                                End If
                        End Select

                End Select

            End If
        End With
    End Sub

    Public Overridable Sub CheckDwellingAmounts(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType = "TENANT" Then
                Select Case .DwellingUnits.Item(0).Region
                    Case "1" 'Case "1", "2"
                        If CommonRulesFunctions.AllowCode("OldDeductibleZipCodeRules") Then
                            If .DwellingUnits.Item(0).DwellingAmt < 200000 Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $200,000 or more", "DwellingAmount", "IER", .Notes.Count))
                            End If
                        Else
                            If .DwellingUnits.Item(0).DwellingAmt < 150000 Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $150,000 or more", "DwellingAmount", "IER", .Notes.Count))
                            End If
                        End If
                    Case "2"
                        If .DwellingUnits.Item(0).DwellingAmt < 125000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $125,000 or more", "DwellingAmount", "IER", .Notes.Count))
                        End If
                    Case "3", "4", "5", "6"
                        If .DwellingUnits.Item(0).DwellingAmt < 50000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $50,000 or more", "DwellingAmount", "IER", .Notes.Count))
                        End If
                End Select

                Select Case .Program.ToUpper
                    Case "HO20", "HO30", "DW30"
                        If .DwellingUnits.Item(0).DwellingAmt > 350000 And .DwellingUnits.Item(0).DwellingAmt <= 500000 And .DwellingUnits.Item(0).Region <> "1" Then
                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Approval is required on all homes greater than $350,000", "DwellingAmount", "IER", .Notes.Count))
                        End If

                        If .DwellingUnits.Item(0).DwellingAmt > 500000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $500,000 or less", "DwellingAmount", "IER", .Notes.Count))
                        End If
                    Case "DW10", "DW20"
                        If .DwellingUnits.Item(0).DwellingAmt > 500000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Homes in this territory must be valued at $500,000 or less", "DwellingAmount", "IER", .Notes.Count))
                        End If
                End Select

            End If
        End With
    End Sub

    Public Overridable Sub CheckContentAmounts(ByVal oPolicy As clsPolicyHomeOwner)
        'Homeowners and Dwelling removed 10/19/09 due to contents now being drop down instead free entry.
        With oPolicy
            Select Case .ProgramType
                'Case "HOMEOWNERS"
                '	If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                '		Dim contentsPercent As Double = .DwellingUnits.Item(0).ContentsAmt / (.DwellingUnits.Item(0).DwellingAmt)
                '		If contentsPercent < 0.4 Then
                '			.Notes = (AddNote(.Notes, "Ineligible Risk: Contents amount must be at least 40% of the dwelling amount.", "ContentsAmount", "IER", .Notes.Count))
                '		End If
                '		If contentsPercent > 0.7 Then
                '			.Notes = (AddNote(.Notes, "Ineligible Risk: Contents amount must be less than 70% of the dwelling amount.", "ContentsAmount", "IER", .Notes.Count))
                '		End If
                '	End If
                'Case "DWELLING", "DWELLING1"
                '	If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                '		Dim contentsPercent As Double = .DwellingUnits.Item(0).ContentsAmt / (.DwellingUnits.Item(0).DwellingAmt)
                '		If Not contentsPercent <= 0.6 Then
                '			.Notes = (AddNote(.Notes, "Ineligible Risk: Contents amount must be less than 60% of the dwelling amount.", "ContentsAmount", "IER", .Notes.Count))
                '		End If
                '	End If
                Case "TENANT"
                    If .DwellingUnits.Item(0).ContentsAmt < 20000 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Contents amount must be at least $20,000", "ContentsAmount", "IER", .Notes.Count))
                    End If
                    If .DwellingUnits.Item(0).ContentsAmt > 50000 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Contents amount must be less than or equal to $50,000", "ContentsAmount", "IER", .Notes.Count))
                    End If
            End Select
        End With
    End Sub

    Public Overridable Sub CheckScheduledPersonalProperty(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim dJewelryTotalAmt As Decimal = 0
            Dim dFirearmsTotalAmt As Decimal = 0
            Dim dScheduledTotalAmt As Decimal = 0

            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                If oEndorse.Type.ToUpper.Contains("SCHEDULEDPROPERTY") Then ' Left(oEndorse.FactorCode, 5) = "HO160" Then
                    dScheduledTotalAmt += oEndorse.Limit

                    For Each oSchProp As clsHomeScheduledProperty In CType(oPolicy, clsPolicyHomeOwner).DwellingUnits(0).HomeScheduledProperty
                        Select Case oSchProp.PropertyCategoryDesc.ToUpper
                            Case "JEWELRY"
                                If oSchProp.PropertyAmt > 15000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Individual scheduled jewelry items must be less than $15,000. (HO206)", "ScheduledProperty", "IER", .Notes.Count))
                                End If
                                dJewelryTotalAmt += oSchProp.PropertyAmt
                            Case "FIREARMS"
                                If oSchProp.PropertyAmt > 2500 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Individual scheduled firearm items must be less than $2,500. (HO206)", "ScheduledPropertyFirearms", "IER", .Notes.Count))
                                End If
                                dFirearmsTotalAmt += oSchProp.PropertyAmt
                        End Select
                    Next
                ElseIf oEndorse.Type.ToUpper.Contains("ADDITIONALPREMISES") Then
                    'can't have 0 and 0
                    Dim bZeroOwner As Boolean = False
                    Dim bZeroTenant As Boolean = False
                    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                        If oUWQuestion.QuestionCode = "306" Then 'Tenant
                            If CInt(oUWQuestion.AnswerText) = 0 Then
                                bZeroTenant = True
                            End If
                        ElseIf oUWQuestion.QuestionCode = "307" Then 'Owner
                            If CInt(oUWQuestion.AnswerText) = 0 Then
                                bZeroOwner = True
                            End If
                        End If
                    Next

                    If oPolicy.CallingSystem.ToUpper <> "WEBRATER" Then
                        ' check to see if at least one address is filled in
                        Dim bHasAddress As Boolean = False
                        For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                            If oUWQuestion.QuestionCode = "H11" Then 'Tenant
                                If oUWQuestion.AnswerText.Length > 0 Then
                                    bHasAddress = True
                                End If
                            End If
                        Next

                        If Not bHasAddress Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Must select at least 1 Additional Premise (HO208)", "AdditionalPremises", "IER", .Notes.Count))
                        End If

                    End If
                    If bZeroOwner And bZeroTenant Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Must select at least 1 Additional Premise (HO208)", "AdditionalPremises", "IER", .Notes.Count))
                    End If

                    If oPolicy.CallingSystem.ToUpper = "WEBRATER" And (IsNumeric(oPolicy.Status) AndAlso oPolicy.Status > 2) Then

                        For i As Integer = 1 To oEndorse.NumberOfEndorsements

                            Dim bHasState As Boolean = False
                            Dim bHasCity As Boolean = False
                            Dim bHasAddress1 As Boolean = False
                            Dim bHasZip As Boolean = False
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                If oUWQuestion.QuestionCode = "H11" And oUWQuestion.IndexNum = i Then 'address1
                                    If oUWQuestion.AnswerText.Length > 0 Then
                                        bHasAddress1 = True
                                    End If
                                ElseIf oUWQuestion.QuestionCode = "309" And oUWQuestion.IndexNum = i Then 'city
                                    If oUWQuestion.AnswerText.Length > 0 Then
                                        bHasCity = True
                                    End If
                                ElseIf oUWQuestion.QuestionCode = "310" And oUWQuestion.IndexNum = i Then 'state
                                    If oUWQuestion.AnswerText.Length > 0 Then
                                        bHasState = True
                                    End If
                                ElseIf oUWQuestion.QuestionCode = "311" And oUWQuestion.IndexNum = i Then 'zip
                                    If oUWQuestion.AnswerText.Length > 0 Then
                                        bHasZip = True
                                    End If
                                End If
                            Next

                            If Not bHasAddress1 Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Missing Additional Premise " & i & " Address.", "AdditionalPremisesAdd" & i.ToString, "IER", .Notes.Count))
                            End If

                            If Not bHasCity Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Missing Additional Premise " & i & " City.", "AdditionalPremisesCity" & i.ToString, "IER", .Notes.Count))
                            End If

                            If Not bHasState Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Missing Additional Premise " & i & " State.", "AdditionalPremisesState" & i.ToString, "IER", .Notes.Count))
                            End If

                            If Not bHasZip Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Missing Additional Premise " & i & " Zip.", "AdditionalPremisesZip" & i.ToString, "IER", .Notes.Count))
                            End If
                        Next
                    End If
                    'can't have more than 4 total
                    If oEndorse.NumberOfEndorsements > 4 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have more than 4 Additional Premises (HO208)", "AdditionalPremises", "IER", .Notes.Count))
                    End If
                ElseIf oEndorse.Type.ToUpper.Contains("FAIRRENTAL") Then
                    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                        If oUWQuestion.QuestionCode = "202" Then 'Coverage Amount
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    If CInt(oUWQuestion.AnswerText) > (.DwellingUnits.Item(0).DwellingAmt * 0.2) Then
                                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have Fair Rental coverage greater than 20% of the dwelling amount (DW211)", "FairRentalAmt", "IER", .Notes.Count))
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If
                If .ProgramType.Contains("DWELLING") Then
                    If .DwellingUnits.Item(0).OwnerOccupiedFlag = 1 Then
                        'can not have fair rental endorsement
                        If oEndorse.Type.ToUpper.Contains("FAIRRENTAL") Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have Fair Rental on an owner occupied dwelling (DW211)", "FairRental", "IER", .Notes.Count))
                        End If
                    Else
                        'can not have contents replacement endorsement
                        If oEndorse.Type.ToUpper.Contains("PERSONALPROPERTYREPLACEMENTCOST") Then 'PersonalPropertyReplacementCost
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have Contents Replacement on a tenant occupied dwelling (DW201)", "ContentsReplacement", "IER", .Notes.Count))
                        End If
                    End If
                End If
            Next
            If dJewelryTotalAmt > 20000 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Total of scheduled jewelry must be less than $20,000. (HO206)", "ScheduledPropertyTotal", "IER", .Notes.Count))
            End If
            If dFirearmsTotalAmt > 7500 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Total of scheduled firearms must be less than $7,500. (HO206)", "ScheduledPropertyFirearmsTotal", "IER", .Notes.Count))
            End If
            If dScheduledTotalAmt > 30000 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Total of scheduled property must be less than $30,000. (HO206)", "ScheduledTotal", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckSeasonalProperty(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '7. Answers "Yes" to secondary or seasonal pool : IER
            If .DwellingUnits.Item(0).SecSeasonalDwelling = True Then
                Select Case .Program
                    Case "HO20", "HO30"
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Secondary or seasonal only allowed for dwelling policies", "Seasonal", "IER", .Notes.Count))
                    Case "HO30T", "DW20", "DW30", "DW10"
                        If .DwellingUnits.Item(0).PrimaryPolicyNumber = "" Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Primary policy number must be provided", "PolicyNum", "IER", .Notes.Count))
                        End If
                End Select
            End If
        End With
    End Sub

    Public Overridable Sub CheckRoofType(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '16. Tile, slate, tar & gravel or wood roof entered
            Select Case .DwellingUnits.Item(0).RoofTypeCode
                Case "Slate Roof", "Tar & Gravel Roof", "Tile Roof", "Wood Roof", "Concrete Tile Roof"
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have this roof", "RoofType", "IER", .Notes.Count))
                Case Else
            End Select
        End With
    End Sub

    Public Overridable Sub CheckMetal2Roof(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Select Case .DwellingUnits.Item(0).RoofTypeCode
                Case "Metal Roof"
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes with metal roof are not eligible for coverage", "RoofType", "IER", .Notes.Count))
                Case Else
            End Select
        End With
    End Sub

    Public Overridable Sub CheckTinRoof(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Select Case .DwellingUnits.Item(0).RoofTypeCode
                Case "Tin Roof"
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes with tin roofs are not eligible for coverage.", "TinRoof", "IER", .Notes.Count))
                Case Else
            End Select
        End With
    End Sub

    Public Overridable Sub CheckPlumbingQuestions(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .Status.Trim = "4" Then
                If .DwellingUnits.Item(0).HomeAge > 30 And .ProgramType.ToUpper <> "TENANT" Then
                    If .DwellingUnits.Item(0).PlumbingDesc = "" Or .DwellingUnits.Item(0).PlumbingYear = "" Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Must answer plumbing questions on Application screen.", "PlumbingQuestions", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckWiringQuestions(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .Status.Trim = "4" Then
                If .DwellingUnits.Item(0).HomeAge > 30 And .ProgramType.ToUpper <> "TENANT" Then
                    If .DwellingUnits.Item(0).RenovationWiringDesc = "" Or .DwellingUnits.Item(0).RenovationWiringYear = "" Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Must answer wiring questions on Application screen.", "WiringQuestions", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckPriorCarrier(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).Region = "1" Then
                If .PriorCarrierName <> "" Then
                    If .PriorCarrierName.ToUpper = "CITIZENS" Then
                        'they are good
                    Else
                        If CommonRulesFunctions.StateInfoContains("ALLOW", "NOCITIZENS", .DwellingUnits(0).Zip, .Product & .StateCode, .AppliesToCode) Then
                            'Allowed
                        Else
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Citizens must be the prior carrier.", "CitizensRegion", "IER", .Notes.Count))
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckLADeductible2(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            ' Require Ded1 Or Ded3 AND Ded2
            If oPolicy.Program = "HO20" Or oPolicy.Program = "HO30" Then
                If oPolicy.DwellingUnits(0).Ded2 > 0 Then
                    If Not (oPolicy.DwellingUnits(0).Ded1 > 0 Or oPolicy.DwellingUnits(0).Ded3 > 0) Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Wind/Hail Or Named Storm deductible is required.", "LADED2", "IER", .Notes.Count))
                    End If
                Else
                    .Notes = (AddNote(.Notes, "Ineligible Risk: All Other Peril Deductible is required.", "LADED2", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckLAReplacementCost(ByVal oPolicy As clsPolicyHomeOwner)

        With oPolicy
            If .Program = "HO30" Then
                'HO30s the endorsement HO201 is required
                Dim bEndorsementFound As Boolean = False

                For Each oEnd As clsEndorsementFactor In .EndorsementFactors
                    If oEnd.FactorCode.ToUpper.Trim = "HO201" And Not oEnd.IsMarkedForDelete Then
                        bEndorsementFound = True
                        Exit For
                    End If
                Next
                If Not bEndorsementFound Then
                    oPolicy.Notes = (AddNote(.Notes, "Ineligible Risk: The HO201 - Replacement Cost for Personal Property Endorsement is required on HO30 policies.", "InvalidReplacementCost", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overrides Function CheckAccreditedBuilderHomeAge(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim bHasAccreditedBuilder As Boolean = False

        With oPolicy
            If HasDiscount(oPolicy, "A_BUILDER") Then
                bHasAccreditedBuilder = True
            End If

            If bHasAccreditedBuilder And Not (.DwellingUnits(0).YearOfConstruction >= Year(DateAdd("yyyy", -5, oPolicy.OrigTermEffDate))) Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: The Accredited Builder discount is only applicable to homes that are no more than 5 years old", "AccBuilder", "IER", .Notes.Count))
            End If

        End With
    End Function

    Public Overridable Sub CheckMultiFamily(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType.Contains("DWELLING") Then
                'No multi-family allowed
                If .DwellingUnits(0).BuildingTypeCode = "BLD2" Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Multi-Family building types are only accepted for Dwelling forms.", "BuildingType", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

#End Region

#Region "UWW Functions"
    Public Overridable Sub CheckNonWeatherClaims(ByVal oPolicy As clsPolicyHomeOwner)
        ' Removed 9/21/2010
        ''Removed b/c this is a HOM rule and is not state specific (DRS 10/6/09)
        'With oPolicy
        '	Dim iNumNonWeatherLoss As Integer = 0
        '	For Each oClaim As clsBaseClaim In .DwellingUnits(0).Claims
        '		If oClaim.Chargeable Then
        '			If oClaim.ClaimTypeIndicator.Trim <> "W" Then
        '				iNumNonWeatherLoss += 1
        '			End If
        '		End If
        '	Next

        '	If iNumNonWeatherLoss > 1 Then
        '		.Notes = (AddNote(.Notes, "Ineligible Risk: UW approval is required for multiple non-weather claims.", "NonW", "IER", .Notes.Count))
        '	End If
        'End With
    End Sub

    Public Overridable Sub CheckEndorsements(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim dJewelryTotalAmt As Decimal = 0
            Dim dFurTotalAmt As Decimal = 0
            Dim dCamerasTotalAmt As Decimal = 0
            Dim dMusicTotalAmt As Decimal = 0
            Dim dSilverwareTotalAmt As Decimal = 0
            Dim dGolfTotalAmt As Decimal = 0
            Dim dArtTotalAmt As Decimal = 0
            Dim dStampTotalAmt As Decimal = 0
            Dim dCoinTotalAmt As Decimal = 0
            Dim dFirearmTotalAmt As Decimal = 0
            Dim dTotalEndorsementAmt As Decimal = 0

            Dim bHasFur As Boolean = False
            Dim bHasCameras As Boolean = False
            Dim bHasMusic As Boolean = False
            Dim bHasSilverware As Boolean = False
            Dim bHasGolf As Boolean = False
            Dim bHasArt As Boolean = False
            Dim bHasStamp As Boolean = False
            Dim bHasCoin As Boolean = False
            Dim bHasFirearm As Boolean = False

            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                If oEndorse.Type.ToUpper.Contains("SCHEDULEDPROPERTY") Then
                    For Each oSchProp As clsHomeScheduledProperty In CType(oPolicy, clsPolicyHomeOwner).DwellingUnits(0).HomeScheduledProperty
                        Select Case oSchProp.PropertyCategoryDesc.ToUpper
                            Case "JEWELRY"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dJewelryTotalAmt += oSchProp.PropertyAmt

                            Case "FINE ARTS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dArtTotalAmt += oSchProp.PropertyAmt

                                bHasArt = True
                            Case "STAMPS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dStampTotalAmt += oSchProp.PropertyAmt
                                bHasStamp = True
                            Case "COINS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dCoinTotalAmt += oSchProp.PropertyAmt
                                bHasCoin = True
                            Case "FIREARMS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dFirearmTotalAmt += oSchProp.PropertyAmt
                                bHasFirearm = True
                            Case "FURS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dFurTotalAmt += oSchProp.PropertyAmt
                                bHasFur = True
                            Case "CAMERAS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dCamerasTotalAmt += oSchProp.PropertyAmt
                                bHasCameras = True
                            Case "MUSICAL INSTRUMENTS"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dMusicTotalAmt += oSchProp.PropertyAmt
                                bHasMusic = True
                            Case "SILVERWARE"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dSilverwareTotalAmt += oSchProp.PropertyAmt
                                bHasSilverware = True
                            Case "GOLF EQUIPMENT", "GOLFER'S EQUIPMENT"
                                If oSchProp.PropertyAmt > 1500 Then
                                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "ScheduledProperty", "UWW", .Notes.Count))
                                End If
                                dGolfTotalAmt += oSchProp.PropertyAmt
                                bHasGolf = True
                        End Select
                    Next

                    'For i As Integer = 1 To oEndorse.NumberOfEndorsements
                    '    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                    '        If oUWQuestion.IndexNum = i Then
                    '            If oUWQuestion.QuestionCode.ToUpper = "300" Or oUWQuestion.QuestionCode.ToUpper = "301" Then
                    '                Select Case oUWQuestion.AnswerText.ToUpper
                    '                    Case "JEWELRY"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "1ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dJewelryTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                    Case "FINE ARTS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "2ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dArtTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasArt = True
                    '                    Case "STAMPS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "3ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dStampTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasStamp = True
                    '                    Case "COINS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "4ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dCoinTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasCoin = True
                    '                    Case "FIREARMS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "5ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dFirearmTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasFirearm = True
                    '                    Case "FURS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "6ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dFurTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasFur = True
                    '                    Case "CAMERAS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "7ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dCamerasTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasCameras = True
                    '                    Case "MUSICAL INSTRUMENTS"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "8ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dMusicTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasMusic = True
                    '                    Case "SILVERWARE"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "9ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dSilverwareTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasSilverware = True
                    '                    Case "GOLF EQUIPMENT"
                    '                        'loop through the questions again and find the amt for the index
                    '                        For Each oUWQ As clsUWQuestion In oEndorse.UWQuestions
                    '                            If oUWQ.IndexNum = i Then
                    '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                    '                                    If oUWQ.AnswerText <> "" Then
                    '                                        If CDec(oUWQ.AnswerText) > 1500 Then
                    '                                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Appraisals must be submitted on scheduled property valued over $1,500 (HO206)", "10ScheduledProperty", "UWW", .Notes.Count))
                    '                                        End If
                    '                                        dGolfTotalAmt += CDec(oUWQ.AnswerText)
                    '                                    End If
                    '                                End If
                    '                            End If
                    '                        Next
                    '                        bHasGolf = True
                    '                End Select
                    '            End If
                    '        End If
                    '    Next
                    'Next
                End If

                If bHasFur Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Fur before binding (HO206)", "ScheduledProperty1", "UWW", .Notes.Count))
                End If
                If bHasCameras Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Camera Equipment before binding (HO206)", "ScheduledProperty2", "UWW", .Notes.Count))
                End If
                If bHasMusic Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Musical Instruments before binding (HO206)", "ScheduledProperty3", "UWW", .Notes.Count))
                End If
                If bHasSilverware Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Silverware before binding (HO206)", "ScheduledProperty4", "UWW", .Notes.Count))
                End If
                If bHasGolf Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Golf Equipment before binding (HO206)", "ScheduledProperty5", "UWW", .Notes.Count))
                End If
                If bHasArt Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Fine Art before binding (HO206)", "ScheduledProperty6", "UWW", .Notes.Count))
                End If
                If bHasStamp Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Stamps before binding (HO206)", "ScheduledProperty7", "UWW", .Notes.Count))
                End If
                If bHasCoin Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Coins before binding (HO206)", "ScheduledProperty8", "UWW", .Notes.Count))
                End If
                If bHasFirearm Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Prior Approval Required for Firearms before binding (HO206)", "ScheduledProperty9", "UWW", .Notes.Count))
                End If
            Next
        End With
    End Sub

    Public Overrides Sub CheckHomeAge(ByVal oPolicy As clsPolicyHomeOwner)
        MyBase.CheckHomeAge(oPolicy)
        With oPolicy
            If oPolicy.DwellingUnits.Item(0).YearOfConstruction > 0 And .Program.ToUpper <> "HO30T" Then
                If oPolicy.DwellingUnits.Item(0).HomeAge > 30 Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Any home older than 30 years requires acceptable proof that the plumbing and wiring have been updated.", "OldHomeUpdate", "UWW", .Notes.Count))
                End If
            End If
        End With
    End Sub
#End Region

#Region "WRN Functions"
    Public Overridable Sub CheckWRNDeductibles(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType = "HOMEOWNERS" Or .ProgramType.Contains("DWELLING") Then
                Select Case .DwellingUnits.Item(0).Region
                    Case "1"
                        'ded1 is windHail
                        If .DwellingUnits.Item(0).Ded1 < 0.02 Then
                            If .DwellingUnits.Item(0).Ded1 = 0.01 Or .DwellingUnits.Item(0).Ded1 = 0.015 Then
                                .Notes = (AddNote(.Notes, "Warning: A 1% or 1.5% deductible will be allowed only if proof of acceptable storm shutters is provided (Acceptable storm shutters are certified to provide protection against wind speeds of 120 miles per hour or greater)", "ShutterProof", "WRN", .Notes.Count))
                            End If
                        End If
                End Select
            End If
        End With
    End Sub


    Public Overridable Sub CheckFloodCoverage(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType.ToUpper <> "TENANT" Then
                Select Case .DwellingUnits.Item(0).Region
                    Case "1"
                        '.Notes = (AddNote(.Notes, "Warning: All policies in this region are required to provide proof of flood coverage", "FloodProof", "WRN", .Notes.Count))
                    Case Else
                        .Notes = (AddNote(.Notes, "Warning: Proof of flood coverage is required if in FEMA zones A or V", "FloodProof", "WRN", .Notes.Count))
                End Select
            End If
        End With
    End Sub


#End Region

#Region "Helper Functions"

    Public Overrides Sub SetIncreasedLimitFactors(ByVal oPolicy As clsPolicyHomeOwner)

        Dim oNewFactor As clsBaseFactor = Nothing
        Dim iLiaNum As Integer = 0
        Dim iMedNum As Integer = 0

        Try

            'remove the increased limits that are already on the policy
            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                If oFactor.FactorCode.Length > 8 Then
                    If oFactor.FactorCode.Substring(0, 8).ToUpper = "INCR_LIA" Then
                        iLiaNum = oFactor.FactorNum
                        oPolicy.PolicyFactors.Remove(oFactor)
                        Exit For
                    End If
                End If
            Next
            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                If oFactor.FactorCode.Length > 8 Then
                    If oFactor.FactorCode.Substring(0, 8).ToUpper = "INCR_MED" Then
                        iMedNum = oFactor.FactorNum
                        oPolicy.PolicyFactors.Remove(oFactor)
                        Exit For
                    End If
                End If
            Next
            If oPolicy.Program.Substring(0, 2) <> "DW" Then
                If iLiaNum = 0 Then
                    iLiaNum = oPolicy.PolicyFactors.Count + 1
                End If
                oNewFactor = New clsBaseFactor
                Select Case oPolicy.DwellingUnits.Item(0).LiaLimit
                    Case 25000
                        oNewFactor.FactorCode = "INCR_LIA_25"
                        oNewFactor.FactorDesc = "Increased Liability Limit 25"
                        oNewFactor.FactorName = "Increased Liability Limit 25"
                        oNewFactor.FactorNum = iLiaNum
                        oNewFactor.IndexNum = iLiaNum
                        oNewFactor.CovType = "N"
                    Case 50000
                        oNewFactor.FactorCode = "INCR_LIA_50"
                        oNewFactor.FactorDesc = "Increased Liability Limit 50"
                        oNewFactor.FactorName = "Increased Liability Limit 50"
                        oNewFactor.FactorNum = iLiaNum
                        oNewFactor.IndexNum = iLiaNum
                        oNewFactor.CovType = "N"
                    Case 100000
                        oNewFactor.FactorCode = "INCR_LIA_100"
                        oNewFactor.FactorDesc = "Increased Liability Limit 100"
                        oNewFactor.FactorName = "Increased Liability Limit 100"
                        oNewFactor.FactorNum = iLiaNum
                        oNewFactor.IndexNum = iLiaNum
                        oNewFactor.CovType = "N"
                    Case 200000
                        oNewFactor.FactorCode = "INCR_LIA_200"
                        oNewFactor.FactorDesc = "Increased Liability Limit 200"
                        oNewFactor.FactorName = "Increased Liability Limit 200"
                        oNewFactor.FactorNum = iLiaNum
                        oNewFactor.IndexNum = iLiaNum
                        oNewFactor.CovType = "N"
                    Case 300000
                        oNewFactor.FactorCode = "INCR_LIA_300"
                        oNewFactor.FactorDesc = "Increased Liability Limit 300"
                        oNewFactor.FactorName = "Increased Liability Limit 300"
                        oNewFactor.FactorNum = iLiaNum
                        oNewFactor.IndexNum = iLiaNum
                        oNewFactor.CovType = "N"
                    Case 500000
                        oNewFactor.FactorCode = "INCR_LIA_500"
                        oNewFactor.FactorDesc = "Increased Liability Limit 500"
                        oNewFactor.FactorName = "Increased Liability Limit 500"
                        oNewFactor.FactorNum = iLiaNum
                        oNewFactor.IndexNum = iLiaNum
                        oNewFactor.CovType = "N"

                End Select

                If Not oNewFactor Is Nothing Then
                    oPolicy.PolicyFactors.Add(oNewFactor)
                    oNewFactor = Nothing
                End If

                If iMedNum = 0 Then
                    iMedNum = oPolicy.PolicyFactors.Count + 1
                End If
                oNewFactor = New clsBaseFactor
                Select Case oPolicy.DwellingUnits.Item(0).MedPayLimit
                    Case 500
                        oNewFactor.FactorCode = "INCR_MED_500"
                        oNewFactor.FactorDesc = "Increased Medical Limit 500"
                        oNewFactor.FactorName = "Increased Medical Limit 500"
                        oNewFactor.FactorNum = iMedNum
                        oNewFactor.IndexNum = iMedNum
                        oNewFactor.CovType = "N"
                    Case 1000
                        oNewFactor.FactorCode = "INCR_MED_1000"
                        oNewFactor.FactorDesc = "Increased Medical Limit 1000"
                        oNewFactor.FactorName = "Increased Medical Limit 1000"
                        oNewFactor.FactorNum = iMedNum
                        oNewFactor.IndexNum = iMedNum
                        oNewFactor.CovType = "N"
                    Case 2000
                        oNewFactor.FactorCode = "INCR_MED_2000"
                        oNewFactor.FactorDesc = "Increased Medical Limit 2000"
                        oNewFactor.FactorName = "Increased Medical Limit 2000"
                        oNewFactor.FactorNum = iMedNum
                        oNewFactor.IndexNum = iMedNum
                        oNewFactor.CovType = "N"
                    Case 3000
                        oNewFactor.FactorCode = "INCR_MED_3000"
                        oNewFactor.FactorDesc = "Increased Medical Limit 3000"
                        oNewFactor.FactorName = "Increased Medical Limit 3000"
                        oNewFactor.FactorNum = iMedNum
                        oNewFactor.IndexNum = iMedNum
                        oNewFactor.CovType = "N"
                    Case 4000
                        oNewFactor.FactorCode = "INCR_MED_4000"
                        oNewFactor.FactorDesc = "Increased Medical Limit 4000"
                        oNewFactor.FactorName = "Increased Medical Limit 4000"
                        oNewFactor.FactorNum = iMedNum
                        oNewFactor.IndexNum = iMedNum
                        oNewFactor.CovType = "N"
                    Case 5000
                        oNewFactor.FactorCode = "INCR_MED_5000"
                        oNewFactor.FactorDesc = "Increased Medical Limit 5000"
                        oNewFactor.FactorName = "Increased Medical Limit 5000"
                        oNewFactor.FactorNum = iMedNum
                        oNewFactor.IndexNum = iMedNum
                        oNewFactor.CovType = "N"
                End Select

                If Not oNewFactor Is Nothing Then
                    oPolicy.PolicyFactors.Add(oNewFactor)
                    oNewFactor = Nothing
                End If
            End If

        Catch ex As Exception
            Throw New ArgumentException(ex.Message)
        Finally
            If Not oNewFactor Is Nothing Then
                oNewFactor = Nothing
            End If
        End Try

    End Sub

    Public Overrides Function AddPolicyFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        'LA

        'Set Loss Of Use Coverage Amount
        Select Case oPolicy.Program
            Case "HO30T"
                oPolicy.DwellingUnits.Item(0).LossOfUseAmt = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 0.2)
            Case "HO30"
                oPolicy.DwellingUnits.Item(0).LossOfUseAmt = (oPolicy.DwellingUnits.Item(0).DwellingAmt * 0.2)
            Case "HO20", "DW20", "DW30", "DW10"
                oPolicy.DwellingUnits.Item(0).LossOfUseAmt = (oPolicy.DwellingUnits.Item(0).DwellingAmt * 0.1)
        End Select

        Dim oRatedFactors() As clsBaseFactor
        ReDim oRatedFactors(-1)
        ' Copy off factors with ratedfactor
        ' then add it back after the clear
        Dim iCapFactorCount As Integer = 0
        For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
            If Len(oFactor.RatedFactor) > 0 Then
                ' Copy this off and restore it after the clear
                ReDim oRatedFactors(oRatedFactors.Length)
                oRatedFactors(oRatedFactors.Length - 1) = oFactor
                iCapFactorCount += 1
            End If
        Next

        'If iCapFactorCount > 1 Then
        '    Throw New Exception("Cannot have more than one cap factor on a policy")
        'End If

        oPolicy.PolicyFactors.Clear()
        For Each oFactor As clsBaseFactor In oRatedFactors
            If Not oFactor Is Nothing Then
                oPolicy.PolicyFactors.Add(oFactor)
            End If
        Next

        SetIncreasedLimitFactors(oPolicy)
        'SetLossLevel(oPolicy)
        Dim iNumClaimsLess5YRS As Integer = 0
        'for WebRater if the policy has any loss with a claim amt within the past 5 years then it does not get the NOCLAIM discount
        For Each oClaim As clsBaseClaim In oPolicy.DwellingUnits.Item(0).Claims
            If oClaim.ClaimAmt > 0 Then
                If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) >= oPolicy.EffDate Then
                    iNumClaimsLess5YRS += 1
                End If
            End If
        Next
        If iNumClaimsLess5YRS = 0 Then
            AddPolicyFactor(oPolicy, "NOCLAIM")
        End If

        Dim oNote As clsBaseNote = Nothing

        'FORM
        AddPolicyFactor(oPolicy, "FORM")

        'REPLACE_FULL (Only for HO30 and DW30) (HO20 added 10/22/09 DRS)

        If oPolicy.Program = "HO30" Or oPolicy.Program = "DW30" Or oPolicy.Program = "HO20" Then
            AddPolicyFactor(oPolicy, "REPLACE_FULL")
        End If


        'OCCUPANCY
        Dim bOccupancyFlagAdded As Boolean = False

        ' todo: need to get webrater off of the note system
        If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
            oNote = GetNote(oPolicy, "OCC1")
            If Not oNote Is Nothing Then
                oPolicy.DwellingUnits.Item(0).OwnerOccupiedFlag = 1
                AddPolicyFactor(oPolicy, "OCC1")
                bOccupancyFlagAdded = True
            End If

            If Not oNote Is Nothing Then
                oNote = Nothing
            End If
            oNote = GetNote(oPolicy, "OCC2")
            If Not oNote Is Nothing Then
                oPolicy.DwellingUnits.Item(0).OwnerOccupiedFlag = 0
                AddPolicyFactor(oPolicy, "OCC2")
                bOccupancyFlagAdded = True
            End If
        End If

        If Not bOccupancyFlagAdded Then
            If oPolicy.DwellingUnits(0).OwnerOccupiedFlag = 1 Then
                AddPolicyFactor(oPolicy, "OCC1")
            Else
                AddPolicyFactor(oPolicy, "OCC2")
            End If
        End If


        'HIP ROOF
        ApplyHipRoof(oPolicy)


        'only for HO20 and HO30 - this should be handled with dynamic build of drop downs
        'F_ALARM
        If HasDiscount(oPolicy, "F_ALARM") Then
            AddPolicyFactor(oPolicy, "F_ALARM")
            oPolicy.DwellingUnits.Item(0).FireAlarmCreditID = "F_ALARM"
        Else
            oPolicy.DwellingUnits.Item(0).FireAlarmCreditID = ""
        End If

        'P_ALARM
        If HasDiscount(oPolicy, "P_ALARM") Then
            AddPolicyFactor(oPolicy, "P_ALARM")
            oPolicy.DwellingUnits.Item(0).PoliceAlarmCreditID = "P_ALARM"
        Else
            oPolicy.DwellingUnits.Item(0).PoliceAlarmCreditID = ""
        End If

        'Accredited  Builder
        Dim bAccreditedBuilder As Boolean = False
        If HasDiscount(oPolicy, "A_BUILDER") Then
            bAccreditedBuilder = True
            AddPolicyFactor(oPolicy, "A_BUILDER")
        End If

        ' New Purchase Discount
        'Per the 8/2013 revision, Accredited Builder and New Purchase are no longer mutually exclusive
        Dim dtNewRevisionDate As Date = "08/23/2013"
        Dim dtRenRevisionDate As Date = "10/06/2013"
        Dim bNewPurchaseAdded As Boolean = False
        Dim bEligibleForNewPurchase As Boolean = False

        If Not bAccreditedBuilder _
            Or (oPolicy.Type.ToUpper = "RENEWAL" And oPolicy.EffDate >= dtRenRevisionDate) _
            Or (oPolicy.Type.ToUpper = "NEW" And oPolicy.EffDate >= dtNewRevisionDate) Then

            bEligibleForNewPurchase = True
            oNote = GetNote(oPolicy, "HOMEPURCHASEDATE")
            If Not oNote Is Nothing Then
                Dim dtOriginalNewPurDate As Date
                Try
                    dtOriginalNewPurDate = CDate(oNote.NoteText)

                    If DateDiff(DateInterval.Year, dtOriginalNewPurDate, oPolicy.EffDate) < 1 Then
                        AddPolicyFactor(oPolicy, "NEW_PUR1")

                        AddDiscount(oPolicy, "NEW_PUR1")
                        RemoveDiscount(oPolicy, "NEW_PUR2")
                        RemoveDiscount(oPolicy, "NEW_PUR3")
                        bNewPurchaseAdded = True
                    ElseIf DateDiff(DateInterval.Year, dtOriginalNewPurDate, oPolicy.EffDate) < 2 Then
                        AddPolicyFactor(oPolicy, "NEW_PUR2")

                        RemoveDiscount(oPolicy, "NEW_PUR1")
                        AddDiscount(oPolicy, "NEW_PUR2")
                        RemoveDiscount(oPolicy, "NEW_PUR3")
                        bNewPurchaseAdded = True
                    ElseIf DateDiff(DateInterval.Year, dtOriginalNewPurDate, oPolicy.EffDate) < 3 Then
                        AddPolicyFactor(oPolicy, "NEW_PUR3")

                        RemoveDiscount(oPolicy, "NEW_PUR1")
                        RemoveDiscount(oPolicy, "NEW_PUR2")
                        AddDiscount(oPolicy, "NEW_PUR3")
                        bNewPurchaseAdded = True
                    End If

                Catch ex As Exception
                    ' might not be a valid date, just need to eat the error
                End Try
            End If
        End If

        If Not bEligibleForNewPurchase Or Not bNewPurchaseAdded Then
            RemoveDiscount(oPolicy, "NEW_PUR1")
            RemoveDiscount(oPolicy, "NEW_PUR2")
            RemoveDiscount(oPolicy, "NEW_PUR3")
        End If

        'HOME
        'only for HOA and HOB
        'No Note Here, Always Check for This
        ' With May 2012 revision, this now applies to all programs except HO30T
        'If oPolicy.Program = "HO20" Or oPolicy.Program = "HO30" Then
        If oPolicy.Program <> "HO30T" Then
            Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            Dim DataRows() As DataRow
            Dim oStateInfoTable As DataTable = Nothing

            oStateInfoTable = oStateInfoDataSet.Tables(0)
            DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='HOMEAGEGREATERTHAN9'")

            Dim sHomeAgeGreaterThan9 As String = Now()
            Dim sHomeAgeGreaterThan16 As String = Now()
            For Each oRow As DataRow In DataRows
                sHomeAgeGreaterThan9 = oRow("ItemValue")
            Next

            DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='HOMEAGEGREATERTHAN16'")
            For Each oRow As DataRow In DataRows
                sHomeAgeGreaterThan16 = oRow("ItemValue")
            Next

            If oPolicy.RateDate < CDate(sHomeAgeGreaterThan9) Then
                Select Case oPolicy.DwellingUnits.Item(0).HomeAge
                    Case 0
                        AddPolicyFactor(oPolicy, "HOME0")
                    Case 1
                        AddPolicyFactor(oPolicy, "HOME1")
                    Case 2
                        AddPolicyFactor(oPolicy, "HOME2")
                    Case 3
                        AddPolicyFactor(oPolicy, "HOME3")
                    Case 4
                        AddPolicyFactor(oPolicy, "HOME4")
                    Case 5
                        AddPolicyFactor(oPolicy, "HOME5")
                    Case 6
                        AddPolicyFactor(oPolicy, "HOME6")
                    Case 7
                        AddPolicyFactor(oPolicy, "HOME7")
                    Case 8
                        AddPolicyFactor(oPolicy, "HOME8")
                    Case Is >= 9
                        AddPolicyFactor(oPolicy, "HOME9")
                    Case Else
                End Select
            ElseIf oPolicy.RateDate < CDate(sHomeAgeGreaterThan16) Then
                Select Case oPolicy.DwellingUnits.Item(0).HomeAge
                    Case 0
                        AddPolicyFactor(oPolicy, "HOME0")
                    Case 1
                        AddPolicyFactor(oPolicy, "HOME1")
                    Case 2
                        AddPolicyFactor(oPolicy, "HOME2")
                    Case 3
                        AddPolicyFactor(oPolicy, "HOME3")
                    Case 4
                        AddPolicyFactor(oPolicy, "HOME4")
                    Case 5
                        AddPolicyFactor(oPolicy, "HOME5")
                    Case 6
                        AddPolicyFactor(oPolicy, "HOME6")
                    Case 7
                        AddPolicyFactor(oPolicy, "HOME7")
                    Case 8
                        AddPolicyFactor(oPolicy, "HOME8")
                    Case 9
                        AddPolicyFactor(oPolicy, "HOME9")
                    Case 10 To 19
                        AddPolicyFactor(oPolicy, "HOME10")
                    Case 20 To 29
                        AddPolicyFactor(oPolicy, "HOME11")
                    Case 30 To 39
                        AddPolicyFactor(oPolicy, "HOME12")
                    Case 40 To 49
                        AddPolicyFactor(oPolicy, "HOME13")
                    Case 50 To 59
                        AddPolicyFactor(oPolicy, "HOME14")
                    Case 60 To 70
                        AddPolicyFactor(oPolicy, "HOME15")
                    Case Is > 70
                        AddPolicyFactor(oPolicy, "HOME16")
                    Case Else
                End Select
            Else
                Select Case oPolicy.DwellingUnits.Item(0).HomeAge
                    Case 0
                        AddPolicyFactor(oPolicy, "HOME0")
                    Case 1
                        AddPolicyFactor(oPolicy, "HOME1")
                    Case 2
                        AddPolicyFactor(oPolicy, "HOME2")
                    Case 3
                        AddPolicyFactor(oPolicy, "HOME3")
                    Case 4
                        AddPolicyFactor(oPolicy, "HOME4")
                    Case 5
                        AddPolicyFactor(oPolicy, "HOME5")
                    Case 6
                        AddPolicyFactor(oPolicy, "HOME6")
                    Case 7
                        AddPolicyFactor(oPolicy, "HOME7")
                    Case 8
                        AddPolicyFactor(oPolicy, "HOME8")
                    Case 9
                        AddPolicyFactor(oPolicy, "HOME9")
                    Case 10
                        AddPolicyFactor(oPolicy, "HOME10")
                    Case 11
                        AddPolicyFactor(oPolicy, "HOME11")
                    Case 12
                        AddPolicyFactor(oPolicy, "HOME12")
                    Case 13
                        AddPolicyFactor(oPolicy, "HOME13")
                    Case 14
                        AddPolicyFactor(oPolicy, "HOME14")
                    Case 15
                        AddPolicyFactor(oPolicy, "HOME15")
                    Case 16
                        AddPolicyFactor(oPolicy, "HOME16")
                    Case 17
                        AddPolicyFactor(oPolicy, "HOME17")
                    Case 18
                        AddPolicyFactor(oPolicy, "HOME18")
                    Case 19 To 25
                        AddPolicyFactor(oPolicy, "HOME19")
                    Case 26 To 35
                        AddPolicyFactor(oPolicy, "HOME20")
                    Case Is > 35
                        AddPolicyFactor(oPolicy, "HOME21")
                    Case Else
                End Select
            End If
        End If

        'BLD Factor
        If oPolicy.DwellingUnits.Item(0).BuildingTypeCode <> "" Then
            Dim sBuildingTypeCode As String = oPolicy.DwellingUnits(0).BuildingTypeCode
            If oPolicy.DwellingUnits(0).BuildingTypeCode = "APT" Then
                sBuildingTypeCode = "BLD3"
            End If

            If oPolicy.DwellingUnits(0).BuildingTypeCode = "MF" Then
                sBuildingTypeCode = "BLD2"
            End If

            If oPolicy.DwellingUnits(0).BuildingTypeCode = "SF" Then
                sBuildingTypeCode = "BLD1"
            End If
            AddPolicyFactor(oPolicy, sBuildingTypeCode)
        End If


        ' RESET MULTILINE5 DISCOUNT (should get re-applied below based on MULTILINE2 and 4)
        RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE5")
        RemovePolicyFactor(oPolicy, "MULTILINE5")

        For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
            If oDiscount.FactorCode = "MULTILINE5" Then
                oPolicy.Discounts.Remove(oDiscount)
                Exit For
            End If
        Next

        'MULTILINE
        Dim bHasAuto As Boolean = False
        Dim bHasAgencyAuto As Boolean = False
        Dim bHasFlood As Boolean = False
        Dim bHasBoth As Boolean = False

        If HasDiscount(oPolicy, "MULTILINE1") Then
            bHasAuto = True
            AddPolicyFactor(oPolicy, "MULTILINE1")
            Dim oDiscount As clsHomeOwnerDiscount = GetDiscount(oPolicy, "MULTILINE1")
            If Not oDiscount Is Nothing Then
                oPolicy.CompanionPAPolicyID = oDiscount.Param2
            End If
            If oPolicy.CompanionPAPolicyID = "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Confirm Auto Companion Policy ID #" & oPolicy.CompanionPAPolicyID, "MULTILINE1", "UWW", oPolicy.Notes.Count))
            End If

        Else
            bHasAuto = False
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE1")
        End If

        If HasDiscount(oPolicy, "MULTILINE2") Then
            bHasFlood = True
            AddPolicyFactor(oPolicy, "MULTILINE2")
            Dim oDiscount As clsHomeOwnerDiscount = GetDiscount(oPolicy, "MULTILINE2")
            If Not oDiscount Is Nothing Then
                oPolicy.CompanionFloodPolicyID = oDiscount.Param2
            End If
            If oPolicy.CompanionFloodPolicyID = "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Confirm Flood Companion Policy ID #" & oPolicy.CompanionFloodPolicyID, "MULTILINE2", "UWW", oPolicy.Notes.Count))
            End If
        Else
            bHasFlood = False
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE2")
        End If

        If Not oNote Is Nothing Then
            oNote = Nothing
        End If
        RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE3")
        'flood and auto
        If bHasAuto And bHasFlood Then
            bHasBoth = True
            AddPolicyFactor(oPolicy, "MULTILINE3")
            RemovePolicyFactor(oPolicy, "MULTILINE1")
            RemovePolicyFactor(oPolicy, "MULTILINE2")

            ' todo: remove once policy discounts is complete implemented
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE1")
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE2")
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Discount:MULTILINE3", "MULTILINE3", "DIS", oPolicy.Notes.Count))
            ' end todo

            If Not HasDiscount(oPolicy, "MULTILINE3") Then
                Dim oNewdiscount As New clsHomeOwnerDiscount
                oNewdiscount.FactorCategory = "MULTILINE3"
                oNewdiscount.FactorCode = "MULTILINE3"
                oNewdiscount.IsNew = True
                oNewdiscount.FactorType = "POLICY"
                oNewdiscount.IsNew = True
                oNewdiscount.UnitNumber = 1
                oNewdiscount.Param1 = ""
                oNewdiscount.Param2 = ""
                oNewdiscount.Param3 = ""
                oNewdiscount.Param4 = ""

                oPolicy.Discounts.Add(oNewdiscount)
            End If
        Else
            If HasDiscount(oPolicy, "MULTILINE3") And oPolicy.CallingSystem.ToUpper <> "WEBRATER" Then
                bHasBoth = True
                AddPolicyFactor(oPolicy, "MULTILINE3")
            Else
                bHasBoth = False
                RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE3")

                For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
                    If oDiscount.FactorCode = "MULTILINE3" Then
                        oPolicy.Discounts.Remove(oDiscount)
                        Exit For
                    End If
                Next
            End If
        End If

        If bHasBoth Then
            'don't clear out policy id fields
        Else
            If Not bHasAuto Then
                RemoveNotes(oPolicy.Notes, "MLN", "MULTILINE1")
                oPolicy.CompanionPAPolicyID = ""
            ElseIf Not bHasFlood Then
                RemoveNotes(oPolicy.Notes, "MLN", "MULTILINE2")
                oPolicy.CompanionFloodPolicyID = ""
            End If
        End If

        If HasDiscount(oPolicy, "MULTILINE4") Then
            bHasAgencyAuto = True
            AddPolicyFactor(oPolicy, "MULTILINE4")
            Dim oDiscount As clsHomeOwnerDiscount = GetDiscount(oPolicy, "MULTILINE4")
            If Not oDiscount Is Nothing Then
                oPolicy.AgencyCompanionPAPolicyID = oDiscount.Param2
                oPolicy.AgencyCompanionPACarrierName = oDiscount.Param3
            End If
            If oPolicy.AgencyCompanionPAPolicyID = "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Confirm Agency Auto Companion Policy ID #" & oPolicy.CompanionPAPolicyID, "MULTILINE4", "UWW", oPolicy.Notes.Count))
            End If

            If bHasAuto Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot have both agency companion policy and Imperial companion policy.", "AGENIMPCOMP", "UWW", oPolicy.Notes.Count))
            End If
        Else
            bHasAgencyAuto = False
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE4")
        End If

        ' flood and agency auto
        If bHasAgencyAuto And bHasFlood Then
            bHasBoth = True
            AddPolicyFactor(oPolicy, "MULTILINE5")
            RemovePolicyFactor(oPolicy, "MULTILINE4")
            RemovePolicyFactor(oPolicy, "MULTILINE2")

            ' todo: remove once policy discounts is complete implemented
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE4")
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE2")
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Discount:MULTILINE5", "MULTILINE5", "DIS", oPolicy.Notes.Count))
            ' end todo

            If Not HasDiscount(oPolicy, "MULTILINE5") Then
                Dim oNewdiscount As New clsHomeOwnerDiscount
                oNewdiscount.FactorCategory = "MULTILINE5"
                oNewdiscount.FactorCode = "MULTILINE5"
                oNewdiscount.FactorType = "POLICY"
                oNewdiscount.IsNew = True
                oNewdiscount.UnitNumber = 1
                oNewdiscount.Param1 = ""
                oNewdiscount.Param2 = ""
                oNewdiscount.Param3 = ""
                oNewdiscount.Param4 = ""
                oPolicy.Discounts.Add(oNewdiscount)
            End If
        Else
            If HasDiscount(oPolicy, "MULTILINE5") Then
                bHasBoth = True
                RemovePolicyFactor(oPolicy, "MULTILINE4")
                RemovePolicyFactor(oPolicy, "MULTILINE2")
                AddPolicyFactor(oPolicy, "MULTILINE5")
            Else
                bHasBoth = False
                RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE5")

                For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
                    If oDiscount.FactorCode = "MULTILINE5" Then
                        oPolicy.Discounts.Remove(oDiscount)
                        Exit For
                    End If
                Next
            End If
        End If

        '' Auto apply endorsements
        ''117HOE214 (1109) – Should apply to all owner occupied HO20 and HO30 policies that have a Deductible 3 – Named Storm
        ''117HOE213 (1109) – Should apply to all owner occupied HO20 and HO30 policies that have a Deductible 1 – Wind/Hail
        ''117DWE214 (1109) - Should apply to all owner occupied DW10, DW20 and DW30 policies that have a Deductible 1 – Wind/Hail
        Dim oEnd As clsEndorsementFactor
        Dim oEndRemove As clsEndorsementFactor
        Dim i As Integer
        Dim j As Integer

        If oPolicy.CallingSystem.Contains("OLE") Or oPolicy.CallingSystem.ToUpper.Contains("UWC") Or oPolicy.CallingSystem.Contains("PAS") Then
            ' Clear out the auto apply endorsements (if they exist)
            oEndRemove = GetEndorsement(oPolicy, "HOE213")
            If Not oEndRemove Is Nothing Then
                For i = 0 To oPolicy.EndorsementFactors.Count - 1
                    If oPolicy.EndorsementFactors(i).IndexNum = oEndRemove.IndexNum Then
                        oPolicy.EndorsementFactors(i).IsMarkedForDelete = True
                    End If
                Next
            End If

            oEndRemove = GetEndorsement(oPolicy, "HOE214")
            If Not oEndRemove Is Nothing Then
                For i = 0 To oPolicy.EndorsementFactors.Count - 1
                    If oPolicy.EndorsementFactors(i).IndexNum = oEndRemove.IndexNum Then
                        oPolicy.EndorsementFactors(i).IsMarkedForDelete = True
                    End If
                Next
            End If


            oEndRemove = GetEndorsement(oPolicy, "DWE214")
            If Not oEndRemove Is Nothing Then
                For i = 0 To oPolicy.EndorsementFactors.Count - 1
                    If oPolicy.EndorsementFactors(i).IndexNum = oEndRemove.IndexNum Then
                        oPolicy.EndorsementFactors(i).IsMarkedForDelete = True
                    End If
                Next
            End If
        Else
            ' Clear out the auto apply endorsements (if they exist)
            oEndRemove = GetEndorsement(oPolicy, "HOE213")
            If Not oEndRemove Is Nothing Then
                oPolicy.EndorsementFactors.Remove(oEndRemove)
            End If
            oEndRemove = GetEndorsement(oPolicy, "HOE214")
            If Not oEndRemove Is Nothing Then
                oPolicy.EndorsementFactors.Remove(oEndRemove)
            End If
            oEndRemove = GetEndorsement(oPolicy, "DWE214")
            If Not oEndRemove Is Nothing Then
                oPolicy.EndorsementFactors.Remove(oEndRemove)
            End If
        End If

        If oPolicy.DwellingUnits(0).OwnerOccupiedFlag Then
            Dim bAddFactor As Boolean = False

            If oPolicy.Program = "HO20" Or oPolicy.Program = "HO30" Then
                If oPolicy.DwellingUnits(0).Ded1 > 0 Then
                    If DetermineEndorsementFactorExists(oPolicy, "HOE213") Then
                        oEndRemove = GetEndorsement(oPolicy, "HOE213")
                        If Not oEndRemove Is Nothing Then
                            If oEndRemove.IsMarkedForDelete Then
                                For i = 0 To oPolicy.EndorsementFactors.Count - 1
                                    If oPolicy.EndorsementFactors(i).IndexNum = oEndRemove.IndexNum Then
                                        oPolicy.EndorsementFactors(i).IsMarkedForDelete = False
                                    End If
                                Next
                                bAddFactor = False
                            Else
                                bAddFactor = True
                            End If
                        Else
                            bAddFactor = True
                        End If
                    Else
                        bAddFactor = True
                    End If

                    If bAddFactor Then
                        oEnd = New clsEndorsementFactor
                        oEnd.FactorCode = "HOE213"
                        oEnd.FactorDesc = "Calendar Year Deductible 1 - Wind Hail"
                        oEnd.FactorType = "HOCALENDARYEARDED1"
                        oEnd.Type = "HOCALENDARYEARDED1"
                        If oPolicy.EndorsementFactors.Count = 0 Then
                            oEnd.IndexNum = GetMaxCoverageIndex(oPolicy) + 1
                        Else
                            oEnd.IndexNum = GetMaxEndorsementIndex(oPolicy) + 1
                        End If
                        oEnd.UWQuestions = New List(Of clsUWQuestion)
                        If CommonRulesFunctions.StateInfoContains("AUTOADD", "HOE213", "", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode) Then
                            oPolicy.EndorsementFactors.Add(oEnd)
                        End If
                    End If
                End If

                If oPolicy.DwellingUnits(0).Ded3 > 0 Then
                    If DetermineEndorsementFactorExists(oPolicy, "HOE214") Then
                        oEndRemove = GetEndorsement(oPolicy, "HOE214")
                        If Not oEndRemove Is Nothing Then
                            If oEndRemove.IsMarkedForDelete Then
                                For i = 0 To oPolicy.EndorsementFactors.Count - 1
                                    If oPolicy.EndorsementFactors(i).IndexNum = oEndRemove.IndexNum Then
                                        oPolicy.EndorsementFactors(i).IsMarkedForDelete = False
                                    End If
                                Next
                                bAddFactor = False
                            Else
                                bAddFactor = True
                            End If
                        Else
                            bAddFactor = True
                        End If
                    Else
                        bAddFactor = True
                    End If

                    If bAddFactor Then
                        oEnd = New clsEndorsementFactor
                        oEnd.FactorCode = "HOE214"
                        oEnd.FactorDesc = "Calendar Year Deductible 3 – Named Storm"
                        oEnd.FactorType = "HOCALENDARYEARDED3"
                        oEnd.Type = "HOCALENDARYEARDED3"
                        If oPolicy.EndorsementFactors.Count = 0 Then
                            oEnd.IndexNum = GetMaxCoverageIndex(oPolicy) + 1
                        Else
                            oEnd.IndexNum = GetMaxEndorsementIndex(oPolicy) + 1
                        End If
                        oEnd.UWQuestions = New List(Of clsUWQuestion)
                        If CommonRulesFunctions.StateInfoContains("AUTOADD", "HOE214", "", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode) Then
                            oPolicy.EndorsementFactors.Add(oEnd)
                        End If
                    End If
                End If

                oEndRemove = GetEndorsement(oPolicy, "HOE214")
                'remove HOE213 auto apply endorsements when override ier -RK
                If oPolicy.DwellingUnits(0).Ded1 < 0 And oPolicy.DwellingUnits(0).Ded3 > 0 Then
                    If Not oEndRemove Is Nothing Then
                        For j = 0 To oPolicy.EndorsementFactors.Count - 1
                            If oPolicy.EndorsementFactors(j).FactorCode = "HOE213" And oPolicy.EndorsementFactors(j).FactorDesc = "Calendar Year Deductible 1 - Wind Hail" And oPolicy.EndorsementFactors(j).IsMarkedForDelete = False Then
                                oPolicy.EndorsementFactors(j).IsMarkedForDelete = True
                            End If
                        Next
                    End If
                End If
            End If

            If oPolicy.Program = "DW10" Or oPolicy.Program = "DW20" Or oPolicy.Program = "DW30" Then
                If oPolicy.DwellingUnits(0).Ded1 > 0 Then
                    If DetermineEndorsementFactorExists(oPolicy, "DWE214") Then
                        oEndRemove = GetEndorsement(oPolicy, "DWE214")
                        If Not oEndRemove Is Nothing Then
                            If oEndRemove.IsMarkedForDelete Then
                                For i = 0 To oPolicy.EndorsementFactors.Count - 1
                                    If oPolicy.EndorsementFactors(i).IndexNum = oEndRemove.IndexNum Then
                                        oPolicy.EndorsementFactors(i).IsMarkedForDelete = False
                                    End If
                                Next
                                bAddFactor = False
                            Else
                                bAddFactor = True
                            End If
                        Else
                            bAddFactor = True
                        End If
                    Else
                        bAddFactor = True
                    End If

                    If bAddFactor Then
                        oEnd = New clsEndorsementFactor
                        oEnd.FactorCode = "DWE214"
                        oEnd.FactorDesc = "Calendar Year Deductible 1 – Wind/Hail"
                        oEnd.FactorType = "DPCALENDARYEARDED1"
                        oEnd.Type = "DPCALENDARYEARDED1"
                        If oPolicy.EndorsementFactors.Count = 0 Then
                            oEnd.IndexNum = GetMaxCoverageIndex(oPolicy) + 1
                        Else
                            oEnd.IndexNum = GetMaxEndorsementIndex(oPolicy) + 1
                        End If
                        oEnd.UWQuestions = New List(Of clsUWQuestion)
                        If CommonRulesFunctions.StateInfoContains("AUTOADD", "DWE214", "", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode) Then
                            oPolicy.EndorsementFactors.Add(oEnd)
                        End If
                    End If
                End If
            End If
        End If

        Dim iNumAdditionalInsureds As Integer = 0
        For Each oAddInsured As clsEntityAddlInsured In oPolicy.AddlInsureds
            If Not oAddInsured.IsMarkedForDelete Then
                iNumAdditionalInsureds += 1
            End If
        Next

        If iNumAdditionalInsureds > 0 Then
            If oPolicy.Program = "HO20" Or oPolicy.Program = "HO30" Then
                ' HO209
                Dim oEndorse As clsEndorsementFactor = Nothing
                oEndorse = GetEndorsement(oPolicy, "HO209")

                If oEndorse Is Nothing Then
                    AddEndorsementFactor(oPolicy, "HO209")
                    oEndorse = GetEndorsement(oPolicy, "HO209")
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                Else
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                    oEndorse.IsMarkedForDelete = False
                End If
            Else
                ' DW209
                Dim oEndorse As clsEndorsementFactor = Nothing
                oEndorse = GetEndorsement(oPolicy, "DW209")

                If oEndorse Is Nothing Then
                    AddEndorsementFactor(oPolicy, "DW209")
                    oEndorse = GetEndorsement(oPolicy, "DW209")
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                Else
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                    oEndorse.IsMarkedForDelete = False
                End If
            End If
        Else
            If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                RemoveEndorsementFactor(oPolicy, "HO209")
                RemoveEndorsementFactor(oPolicy, "DW209")
            Else
                RemoveEndorsementFactorPAS(oPolicy, "HO209")
                RemoveEndorsementFactorPAS(oPolicy, "DW209")
            End If
        End If

    End Function

    Public Sub ApplyHipRoof(ByVal oPolicy As clsPolicyHomeOwner)
        Dim sHipRoofFactor As String = ""

        With oPolicy
            'check program for HO20 or HO30
            If .Program = "HO20" Or .Program = "HO30" Then
                'check Roof Shape, must be Hip
                If .DwellingUnits.Item(0).RoofShapeCode.ToUpper = "HIP" Then
                    'what Region?
                    sHipRoofFactor = "HIP_ROOF_" & .DwellingUnits.Item(0).Region

                    'apply the HIP_ROOF_# factor                    
                    AddPolicyFactor(oPolicy, sHipRoofFactor)

                    AddDiscount(oPolicy, sHipRoofFactor)
                End If
            End If
        End With

    End Sub


#Region "Endorsement Functions"

    Public Sub AddEndorsementFactor(ByVal oPolicy As clsBasePolicy, ByVal sEndorsementCode As String)
        Dim oEF As New CorPolicy.clsEndorsementFactor
        oEF.FactorCode = sEndorsementCode
        oEF.IndexNum = GetMaxEndorsementIndex(oPolicy) + 1
        oEF.FactorNum = oPolicy.EndorsementFactors.Count + 1
        oEF.FactorAmt = 0
        oEF.FactorDesc = GetEndorseDesc(oPolicy, sEndorsementCode)
        oEF.Type = GetEndorseType(oPolicy, sEndorsementCode)
        oEF.UWQuestions = New List(Of clsUWQuestion)
        If Not oEF.IsModified Then
            oEF.IsNew = True
        End If

        oPolicy.EndorsementFactors.Add(oEF)
    End Sub

    Private Function GetMaxCoverageIndex(ByVal oPolicy As clsPolicyHomeOwner) As Integer
        Dim iMax As Integer = -1

        For i As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
            If oPolicy.DwellingUnits(0).Coverages(i).IndexNum > iMax Then
                iMax = oPolicy.DwellingUnits(0).Coverages(i).IndexNum
            End If
        Next

        For i As Integer = 0 To oPolicy.EndorsementFactors.Count - 1
            If oPolicy.EndorsementFactors(i).IndexNum > iMax Then
                iMax = oPolicy.EndorsementFactors(i).IndexNum
            End If
        Next

        Return iMax
    End Function

    Private Function GetMaxEndorsementIndex(ByVal oPolicy As clsPolicyHomeOwner) As Integer
        Dim iMax As Integer = -1

        For i As Integer = 0 To oPolicy.EndorsementFactors.Count - 1
            If oPolicy.EndorsementFactors(i).IndexNum > iMax Then
                iMax = oPolicy.EndorsementFactors(i).IndexNum
            End If
        Next

        For i As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
            If oPolicy.DwellingUnits(0).Coverages(i).IndexNum > iMax Then
                iMax = oPolicy.DwellingUnits(0).Coverages(i).IndexNum
            End If
        Next

        Return iMax
    End Function

    Public Shared Sub RemoveEndorsementFactor(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String)
        For i As Integer = oPolicy.EndorsementFactors.Count - 1 To 0 Step -1
            If oPolicy.EndorsementFactors.Item(i).FactorCode.ToUpper = sFactorCode.ToUpper Then
                'remove it
                oPolicy.EndorsementFactors.RemoveAt(i)
            End If
        Next
    End Sub

    Public Shared Sub RemoveEndorsementFactorPAS(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String)
        For i As Integer = oPolicy.EndorsementFactors.Count - 1 To 0 Step -1
            If oPolicy.EndorsementFactors.Item(i).FactorCode.ToUpper = sFactorCode.ToUpper Then
                'remove it
                oPolicy.EndorsementFactors.Item(i).IsMarkedForDelete = True
            End If
        Next
    End Sub

    Public Shared Function GetEndorsement(ByVal oPolicy As clsBasePolicy, ByVal sEndorsementCode As String) As clsEndorsementFactor

        For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
            If oEndorse.HasSubCode Then
                If Left(oEndorse.FactorCode.ToString.ToUpper, 5) = sEndorsementCode.ToString.ToUpper Then
                    Return oEndorse
                    Exit For
                End If
            Else
                If oEndorse.FactorCode.ToString.ToUpper = sEndorsementCode.ToString.ToUpper Then
                    Return oEndorse
                    Exit For
                End If
            End If
        Next

        Return Nothing

    End Function

    Private Function GetEndorseDesc(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String) As String

        Dim sEndorseDesc As String = ""
        Dim DataRows() As DataRow
        Dim oFactorEndorsementTable As DataTable = Nothing

        Dim oFactorEndorsementDataSet As DataSet = LoadFactorEndorsementTable(oPolicy)

        oFactorEndorsementTable = oFactorEndorsementDataSet.Tables(0)

        DataRows = oFactorEndorsementTable.Select("Program='" & oPolicy.Program & "'" & " AND ItemSubCode='" & sFactorCode & "'")

        For Each oRow As DataRow In DataRows
            sEndorseDesc = oRow("ItemValue").ToString
        Next

        Return sEndorseDesc

    End Function

    Private Function GetEndorseType(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String) As String

        Dim sEndorseType As String = ""
        Dim DataRows() As DataRow
        Dim oFactorEndorsementTable As DataTable = Nothing

        Dim oFactorEndorsementDataSet As DataSet = LoadFactorEndorsementTable(oPolicy)

        oFactorEndorsementTable = oFactorEndorsementDataSet.Tables(0)

        DataRows = oFactorEndorsementTable.Select("Program='" & oPolicy.Program & "'" & " AND ItemSubCode='" & sFactorCode & "'")

        For Each oRow As DataRow In DataRows
            sEndorseType = oRow("ItemCode").ToString
        Next

        Return sEndorseType

    End Function

    Public Shared Function LoadFactorEndorsementTable(ByVal oPolicy As clsPolicyHomeOwner) As DataSet
        Dim sSql As String = ""

        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Dim oDS As New DataSet

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue "
                sSql = sSql & " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..StateInfo with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND ItemGroup = 'ENDORSEMENT' "
                sSql = sSql & " ORDER BY Program, ItemSubCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "FactorEndorsement")

                Return oDS

            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Shared Function AllowAEC(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Dim bAllowAEC As Boolean = True
        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        Dim oRatingRules As New CommonRulesFunctions
        Dim oStateInfoDataSet As DataSet = oRatingRules.LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        oStateInfoTable = oStateInfoDataSet.Tables(0)

        DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='ALLOW' AND ItemCode='AEC' ")

        For Each oRow As DataRow In DataRows
            bAllowAEC = CBool(oRow("ItemValue"))
        Next

        If Not bAllowAEC Then
            'see if it has the 2 endorsements that will give it AEC
            Dim bHasAECPlus As Boolean = False
            Dim bHasWaterBackUp As Boolean = False
            For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
                If oEndorse.Type = "AEC_Plus" Or oEndorse.FactorCode = "HO170P" Then
                    bHasAECPlus = True
                ElseIf oEndorse.Type = "WaterBackUp" Or oEndorse.FactorCode = "HO170W" Then
                    bHasWaterBackUp = True
                End If
            Next
            If bHasAECPlus And bHasWaterBackUp Then
                bAllowAEC = True
            End If
        End If

        Return bAllowAEC

    End Function
#End Region


    'Public Overrides Function AddPASPolicyFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

    '	MyBase.AddPASPolicyFactors(oPolicy)

    '	Dim oNote As clsBaseNote = Nothing
    '	'Accredited  Builder
    '	Dim bAccreditedBuilder As Boolean = False
    '	If HasDiscount(oPolicy, "A_BUILDER") Then
    '		bAccreditedBuilder = True
    '		AddPolicyFactor(oPolicy, "A_BUILDER")
    '	End If

    '	' New Purchase Discount
    '       ' Cannot have both accredit builder and new purchase discount
    '       Dim bAccreditedBuilderAdded As Boolean = False
    '	If Not bAccreditedBuilder Then
    '		oNote = GetNote(oPolicy, "HOMEPURCHASEDATE")
    '		If Not oNote Is Nothing Then
    '			Dim dtOriginalNewPurDate As Date
    '			Try
    '				dtOriginalNewPurDate = CDate(oNote.NoteText)

    '				If DateDiff(DateInterval.Year, dtOriginalNewPurDate, oPolicy.EffDate) < 1 Then
    '                       AddPolicyFactor(oPolicy, "NEW_PUR1")


    '                       AddDiscount(oPolicy, "NEW_PUR1")
    '                       RemoveDiscount(oPolicy, "NEW_PUR2")
    '                       RemoveDiscount(oPolicy, "NEW_PUR3")
    '                       bAccreditedBuilderAdded = True
    '				ElseIf DateDiff(DateInterval.Year, dtOriginalNewPurDate, oPolicy.EffDate) < 2 Then
    '                       AddPolicyFactor(oPolicy, "NEW_PUR2")


    '                       RemoveDiscount(oPolicy, "NEW_PUR1")
    '                       AddDiscount(oPolicy, "NEW_PUR2")
    '                       RemoveDiscount(oPolicy, "NEW_PUR3")
    '                       bAccreditedBuilderAdded = True
    '				ElseIf DateDiff(DateInterval.Year, dtOriginalNewPurDate, oPolicy.EffDate) < 3 Then
    '                       AddPolicyFactor(oPolicy, "NEW_PUR3")


    '                       RemoveDiscount(oPolicy, "NEW_PUR1")
    '                       RemoveDiscount(oPolicy, "NEW_PUR2")
    '                       AddDiscount(oPolicy, "NEW_PUR3")
    '                       bAccreditedBuilderAdded = True
    '				End If

    '			Catch ex As Exception
    '				' might not be a valid date, just need to eat the error
    '			End Try

    '		End If
    '       End If

    '       If Not bAccreditedBuilderAdded Then
    '           RemoveDiscount(oPolicy, "NEW_PUR1")
    '           RemoveDiscount(oPolicy, "NEW_PUR2")
    '           RemoveDiscount(oPolicy, "NEW_PUR3")
    '       End If

    '	Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
    '	Dim DataRows() As DataRow
    '	Dim oStateInfoTable As DataTable = Nothing

    '	oStateInfoTable = oStateInfoDataSet.Tables(0)
    '	DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='HOMEAGEGREATERTHAN9'")

    '	Dim sHomeAgeGreaterThan9 As String = Now()
    '	Dim sHomeAgeGreaterThan16 As String = Now()
    '	For Each oRow As DataRow In DataRows
    '		sHomeAgeGreaterThan9 = oRow("ItemValue")
    '	Next


    '	DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='HOMEAGEGREATERTHAN16'")
    '	For Each oRow As DataRow In DataRows
    '		sHomeAgeGreaterThan16 = oRow("ItemValue")
    '	Next


    '	For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
    '		If oFactor.FactorCode.Length > 4 Then
    '			If oFactor.FactorCode.Substring(0, 4).ToUpper = "HOME" Then
    '				oPolicy.PolicyFactors.Remove(oFactor)
    '				Exit For
    '			End If
    '		End If
    '	Next

    '	'HOME
    '	'only for HOA and HOB
    '	'No Note Here, Always Check for This
    '	If oPolicy.Program = "HO20" Or oPolicy.Program = "HO30" Then



    '		' Only start doing this as of the HomeAgeGreaterThan16 Date, otherwise there will be discrepancies.
    '		If oPolicy.RateDate < CDate(sHomeAgeGreaterThan9) Then
    '			Select Case oPolicy.DwellingUnits.Item(0).HomeAge
    '				Case 0
    '					AddPolicyFactor(oPolicy, "HOME0")
    '				Case 1
    '					AddPolicyFactor(oPolicy, "HOME1")
    '				Case 2
    '					AddPolicyFactor(oPolicy, "HOME2")
    '				Case 3
    '					AddPolicyFactor(oPolicy, "HOME3")
    '				Case 4
    '					AddPolicyFactor(oPolicy, "HOME4")
    '				Case 5
    '					AddPolicyFactor(oPolicy, "HOME5")
    '				Case 6
    '					AddPolicyFactor(oPolicy, "HOME6")
    '				Case 7
    '					AddPolicyFactor(oPolicy, "HOME7")
    '				Case 8
    '					AddPolicyFactor(oPolicy, "HOME8")
    '				Case Is >= 9
    '					AddPolicyFactor(oPolicy, "HOME9")
    '				Case Else
    '			End Select
    '		ElseIf oPolicy.RateDate < CDate(sHomeAgeGreaterThan16) Then
    '			Select Case oPolicy.DwellingUnits.Item(0).HomeAge
    '				Case 0
    '					AddPolicyFactor(oPolicy, "HOME0")
    '				Case 1
    '					AddPolicyFactor(oPolicy, "HOME1")
    '				Case 2
    '					AddPolicyFactor(oPolicy, "HOME2")
    '				Case 3
    '					AddPolicyFactor(oPolicy, "HOME3")
    '				Case 4
    '					AddPolicyFactor(oPolicy, "HOME4")
    '				Case 5
    '					AddPolicyFactor(oPolicy, "HOME5")
    '				Case 6
    '					AddPolicyFactor(oPolicy, "HOME6")
    '				Case 7
    '					AddPolicyFactor(oPolicy, "HOME7")
    '				Case 8
    '					AddPolicyFactor(oPolicy, "HOME8")
    '				Case 9
    '					AddPolicyFactor(oPolicy, "HOME9")
    '				Case 10 To 19
    '					AddPolicyFactor(oPolicy, "HOME10")
    '				Case 20 To 29
    '					AddPolicyFactor(oPolicy, "HOME11")
    '				Case 30 To 39
    '					AddPolicyFactor(oPolicy, "HOME12")
    '				Case 40 To 49
    '					AddPolicyFactor(oPolicy, "HOME13")
    '				Case 50 To 59
    '					AddPolicyFactor(oPolicy, "HOME14")
    '				Case 60 To 70
    '					AddPolicyFactor(oPolicy, "HOME15")
    '				Case Is > 70
    '					AddPolicyFactor(oPolicy, "HOME16")
    '				Case Else
    '			End Select
    '		Else
    '			Select Case oPolicy.DwellingUnits.Item(0).HomeAge
    '				Case 0
    '					AddPolicyFactor(oPolicy, "HOME0")
    '				Case 1
    '					AddPolicyFactor(oPolicy, "HOME1")
    '				Case 2
    '					AddPolicyFactor(oPolicy, "HOME2")
    '				Case 3
    '					AddPolicyFactor(oPolicy, "HOME3")
    '				Case 4
    '					AddPolicyFactor(oPolicy, "HOME4")
    '				Case 5
    '					AddPolicyFactor(oPolicy, "HOME5")
    '				Case 6
    '					AddPolicyFactor(oPolicy, "HOME6")
    '				Case 7
    '					AddPolicyFactor(oPolicy, "HOME7")
    '				Case 8
    '					AddPolicyFactor(oPolicy, "HOME8")
    '				Case 9
    '					AddPolicyFactor(oPolicy, "HOME9")
    '				Case 10
    '					AddPolicyFactor(oPolicy, "HOME10")
    '				Case 11
    '					AddPolicyFactor(oPolicy, "HOME11")
    '				Case 12
    '					AddPolicyFactor(oPolicy, "HOME12")
    '				Case 13
    '					AddPolicyFactor(oPolicy, "HOME13")
    '				Case 14
    '					AddPolicyFactor(oPolicy, "HOME14")
    '				Case 15
    '					AddPolicyFactor(oPolicy, "HOME15")
    '				Case 16
    '					AddPolicyFactor(oPolicy, "HOME16")
    '				Case 17
    '					AddPolicyFactor(oPolicy, "HOME17")
    '				Case 18
    '					AddPolicyFactor(oPolicy, "HOME18")
    '				Case 19 To 25
    '					AddPolicyFactor(oPolicy, "HOME19")
    '				Case 26 To 35
    '					AddPolicyFactor(oPolicy, "HOME20")
    '				Case Is > 35
    '					AddPolicyFactor(oPolicy, "HOME21")
    '				Case Else
    '			End Select
    '		End If
    '	End If
    'End Function




    Public Overrides Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sItemsToBeFaxedIn As String = ""

        sItemsToBeFaxedIn &= MyBase.ItemsToBeFaxedIn(oPolicy)

        Select Case oPolicy.DwellingUnits.Item(0).Region
            Case "1", "2"
                sItemsToBeFaxedIn &= "Copy of Flood Policy" & vbNewLine
        End Select

        If FactorOnPolicy(oPolicy, "P_ALARM") Then
            sItemsToBeFaxedIn &= "Copy of Police Alarm Certificate" & vbNewLine
        End If
        If FactorOnPolicy(oPolicy, "F_ALARM") Then
            sItemsToBeFaxedIn &= "Copy of Fire Alarm Certificate" & vbNewLine
        End If

        Dim bHasScheduledProperty As Boolean = False
        For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
            If oEndorse.Type.ToUpper.Contains("SCHEDULEDPROP") Then
                bHasScheduledProperty = True
                Exit For
            End If
        Next

        If bHasScheduledProperty Then
            sItemsToBeFaxedIn &= "Copy of Personal Property appraisals" & vbNewLine
        End If

        If oPolicy.PriorCarrierName.Trim <> "" Then
            sItemsToBeFaxedIn &= "Copy of Prior Homeowner Insurance Policy" & vbNewLine
        End If

        If oPolicy.DwellingUnits.Item(0).HomeAge > 30 And oPolicy.ProgramType.ToUpper <> "TENANT" Then
            sItemsToBeFaxedIn &= "Acceptable proof that the plumbing and wiring have been updated since the home is more than 30 years old." & vbNewLine
        End If

        With oPolicy
            If .ProgramType = "HOMEOWNERS" Or .ProgramType.Contains("DWELLING") Then
                Select Case .DwellingUnits.Item(0).Region
                    Case "1"
                        If .DwellingUnits.Item(0).DwellingAmt > 0 Then
                            'ded1 is windHail
                            If .DwellingUnits.Item(0).Ded1 < 1 And .DwellingUnits.Item(0).Ded1 >= 0.01 And .DwellingUnits.Item(0).Ded1 < 0.02 Then 'ded1 not flat and ded1 >= 1% and ded1 <2%
                                'need storm shutters to be allowed
                                sItemsToBeFaxedIn &= "Proof of acceptable storm shutters is required (Acceptable storm shutters are certified to provide protection against wind speeds of 120 miles per hour or greater)" & vbNewLine
                            ElseIf (.DwellingUnits.Item(0).Ded1 / (.DwellingUnits.Item(0).DwellingAmt)) >= 0.01 And (.DwellingUnits.Item(0).Ded1 / (.DwellingUnits.Item(0).DwellingAmt)) < 0.02 Then 'else flat calc if >= 1% and <2%
                                'need storm shutters to be allowed
                                sItemsToBeFaxedIn &= "Proof of acceptable storm shutters is required (Acceptable storm shutters are certified to provide protection against wind speeds of 120 miles per hour or greater)" & vbNewLine
                            End If
                        End If
                    Case Else
                        'no restrictions
                End Select
            End If
        End With

        Return sItemsToBeFaxedIn

    End Function

    Public Overrides Sub SetLossLevel(ByVal oPolicy As clsPolicyHomeOwner)

        Dim iNumClaimsLess5YRS As Integer = 0
        Dim iNumNonWeatherLoss As Integer = 0
        Dim iNumNonWeatherLossLess1000 As Integer = 0
        Dim iNumNonWeatherLossGreat1000 As Integer = 0
        Dim iNumWeatherLoss As Integer = 0
        Dim iNumChargeLoss As Integer = 0
        Dim bHasOpenClaim As Boolean = False
        Dim parent As New clsRules1


        If UseOldSetLossLevel(oPolicy) Then

            'RemovePolicyFactor(oPolicy, "NOCLAIM")

            'N-Water
            'O-NonWeather
            'W-Weather

            With oPolicy.DwellingUnits.Item(0)
                For Each oClaim As clsBaseClaim In .Claims
                    ' Start with claim being chargeable, remove if it meets any of the criteria below
                    If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                        oClaim.Chargeable = True
                    End If

                    If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) >= oPolicy.EffDate Then
                        If oClaim.ClaimAmt > 0 Then
                            If oPolicy.CallingSystem.ToUpper <> "WEBRATER" Then
                                If oClaim.ClaimTypeIndicator <> "W" Then
                                    iNumClaimsLess5YRS += 1
                                Else
                                    'it is a weather claim, now we have to see if it belongs to Imperial
                                    'If oClaim.ClaimSource.ToUpper = "INPUT" Then
                                    If oClaim.ClaimDate > oPolicy.OrigTermEffDate Then
                                        'don't count towards NOCLAIM discount
                                    Else
                                        iNumClaimsLess5YRS += 1
                                    End If
                                End If
                            End If
                        Else
                            If oClaim.ClaimStatus.Trim.ToUpper = "OPEN" Then
                                'look at this guy too
                                If oPolicy.CallingSystem.ToUpper <> "WEBRATER" Then
                                    If oClaim.ClaimTypeIndicator <> "W" Then
                                        iNumClaimsLess5YRS += 1
                                    Else
                                        'it is a weather claim, now we have to see if it belongs to Imperial
                                        'If oClaim.ClaimSource.ToUpper = "INPUT" Then
                                        If oClaim.ClaimDate > oPolicy.OrigTermEffDate Then
                                            'don't count towards NOCLAIM discount
                                        Else
                                            iNumClaimsLess5YRS += 1
                                        End If
                                    End If
                                End If
                            Else
                                oClaim.Chargeable = False
                            End If
                        End If
                        If oPolicy.CallingSystem <> "PAS" Then
                            If DateAdd(DateInterval.Month, 36, oClaim.ClaimDate) < oPolicy.EffDate Then
                                'nonchargeable
                                oClaim.Chargeable = False
                            Else
                                'oClaim.Chargeable = True
                            End If
                        End If
                    Else
                        If oPolicy.CallingSystem <> "PAS" Then
                            oClaim.Chargeable = False
                        End If
                    End If
                    If oClaim.ClaimStatus.Trim.ToUpper = "OPEN" Then
                        oClaim.Chargeable = True
                    End If
                    If oClaim.Chargeable Then
                        If oClaim.ClaimTypeIndicator.Trim <> "W" Then 'If oClaim.ClaimTypeIndicator = "O" Then 
                            If oClaim.ClaimAmt <= 1000 Then
                                iNumNonWeatherLossLess1000 += 1
                            Else
                                iNumNonWeatherLossGreat1000 += 1
                            End If
                            iNumNonWeatherLoss += 1
                            iNumChargeLoss += 1
                        Else
                            'If oClaim.ClaimSource.ToUpper = "INPUT" Then
                            If oClaim.ClaimDate > oPolicy.OrigTermEffDate Then
                                If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                                    iNumChargeLoss += 1
                                End If
                            Else
                                iNumChargeLoss += 1
                            End If
                            iNumWeatherLoss += 1
                        End If
                    End If
                    'If oClaim.Status.ToUpper = "OPEN" Then
                    '    bHasOpenClaim = True
                    'End If
                Next

                For Each oClaim As clsBaseClaim In oPolicy.DwellingUnits.Item(0).Claims
                    If oClaim.ClaimStatus.Trim.ToUpper = "OPEN" Then
                        bHasOpenClaim = True
                        Exit For
                    End If
                Next

                If iNumChargeLoss = 0 Then
                    .LossLevel = 1
                    If iNumClaimsLess5YRS = 0 Then
                        If oPolicy.CallingSystem.Contains("PAS") Or oPolicy.CallingSystem.Contains("CITIZENS") Then
                            AddPolicyFactor(oPolicy, "NOCLAIM")
                        End If
                    End If
                ElseIf iNumChargeLoss > 2 Then
                    .LossLevel = 4
                ElseIf iNumChargeLoss = 2 Then
                    '.LossLevel = 3
                    If iNumNonWeatherLoss = 1 Then
                        .LossLevel = 3
                    Else
                        .LossLevel = 4
                    End If
                ElseIf iNumChargeLoss = 1 Then
                    'If iNumNonWeatherLoss = 1 Then
                    If iNumNonWeatherLossLess1000 <= 1 And iNumNonWeatherLossGreat1000 = 0 Then
                        .LossLevel = 2
                    Else
                        If iNumWeatherLoss = 1 Then
                            .LossLevel = 2
                        Else
                            .LossLevel = 3
                        End If
                    End If
                    'Else
                    '    .LossLevel = 3
                    'End If
                ElseIf bHasOpenClaim Then
                    .LossLevel = 4
                End If

                If bHasOpenClaim Then
                    .LossLevel = 4
                End If
            End With
        Else
            parent.SetLossLevel(oPolicy)
        End If
    End Sub

    ' LA Homeowners drops the FactorTierMatrix table as of 10/1/2009
    ' Need to use the previous setlosslevel function for those with a rate date before then
    ' for all others use the new method
    Private Function UseOldSetLossLevel(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim bUseOldSetLossLevel As Boolean = False

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim sFormType As String = ""

        Dim oConn As SqlConnection
        oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())


        Dim sCreditTier As String = dbGetCreditTier(oPolicy)
        Dim sUWTier As String = dbGetUWTier(oPolicy)

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorTierMatrix with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND CreditTier = @CreditTier "
                sSql = sSql & " AND UWTier = @UWTier "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                If sCreditTier = "" Then sCreditTier = "0"
                cmd.Parameters.Add("@CreditTier", SqlDbType.Int, 22).Value = CInt(sCreditTier)
                cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 1).Value = sUWTier

                oConn.Open()
                oReader = cmd.ExecuteReader
                If oReader.HasRows Then
                    bUseOldSetLossLevel = True
                End If
                oConn.Close()

            End Using
        Catch ex As Exception
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Return bUseOldSetLossLevel
    End Function

    Public Overrides Function AddRenewalFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        'RENEWAL FACTORS
        'If oPolicy.Type.ToUpper = "RENEWAL" Then
        '    If FactorOnPolicy(oPolicy, "NOCLAIM") Then
        '        AddPolicyFactor(oPolicy, "RNW")
        '    Else
        '        AddPolicyFactor(oPolicy, "RNW-C")
        '    End If
        'End If

    End Function

    Public Function DetermineEndorsementFactorExists(ByVal oPolicy As clsPolicyHomeOwner, ByVal sFactorCode As String) As Boolean
        Dim bFactorExists As Boolean = False

        For Each oFactor In oPolicy.EndorsementFactors
            If Not oFactor Is Nothing Then
                If oFactor.FactorCode = sFactorCode Then
                    bFactorExists = True
                    Exit For
                End If
            End If
        Next

        Return bFactorExists
    End Function



#End Region

End Class
