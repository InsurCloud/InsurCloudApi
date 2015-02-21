'Imports System.Web.UI.WebControls.Expressions
Imports Microsoft.VisualBasic
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Configuration

Public Class clsRules142
    Inherits clsRules1

    Public Overridable Sub CheckHomeAgeOver40(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType.ToUpper <> "TENANT" Then
                If .DwellingUnits.Item(0).HomeAge > 40 And oPolicy.DwellingUnits(0).HomeAge < 300 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes over 40 years old must be submitted to Underwriting for a quote", "HOMEAGEOVER40", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckHOTDisabled(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .Program = "HOT" And Now() > CDate("5/19/2010") Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: We are not currently accepting new business with the HO-T program.", "HOTDISABLED", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim parent As New clsRules1

        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""
        Dim bAnimalCollisionIncluded As Boolean = False
        Dim bPayrollIncluded As Boolean = False
        Try
            If parent.CheckNEI(oPolicy) Then

                If Not IsNumeric(oPolicy.DwellingUnits(0).LiaLimit) Then
                    bEnoughInfoToRate = False
                    sMissing += "LiaLimit" & "-"
                Else
                    If Not oPolicy.ProgramType.Contains("DWELLING") Then
                        If oPolicy.DwellingUnits.Item(0).LiaLimit = 0 Then
                            bEnoughInfoToRate = False
                            sMissing += "LiaLimit" & "-"
                        End If
                    End If
                End If
                If Not IsNumeric(oPolicy.DwellingUnits(0).MedPayLimit) Then
                    bEnoughInfoToRate = False
                    sMissing += "MedPayLimit" & "-"
                Else
                    If Not oPolicy.ProgramType.Contains("DWELLING") Then
                        If oPolicy.DwellingUnits.Item(0).MedPayLimit = 0 Then
                            bEnoughInfoToRate = False
                            sMissing += "MedPayLimit" & "-"
                        End If
                    End If
                End If

                'Endorsement Info
                Dim bQuestionChecked As Boolean = False
                For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
                    'We need to check factorcode because if the Rating Service is being
                    ' called from the SP, then only the SystemCode is populated which 
                    ' will be mapped later to retrieve the factorcode.
                    Select Case oEndorse.FactorCode.ToUpper
                        Case "HO120"
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                If oUWQuestion.QuestionCode = "166" Then
                                    bQuestionChecked = True
                                    If oUWQuestion.AnswerText = "" Then
                                        bEnoughInfoToRate = False
                                        sMissing += "HO120 Value of Equipment" & "-"
                                    End If
                                End If
                            Next
                            If Not bQuestionChecked Then
                                bEnoughInfoToRate = False
                                sMissing += "HO120 Not All Questions Checked" & "-"
                            End If
                            bQuestionChecked = False
                        Case "HO210"
                            bQuestionChecked = False
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                Select Case oUWQuestion.QuestionCode.ToUpper
                                    Case "H46" 'Total acreage
                                        bQuestionChecked = True
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO210 Total Acreage" & "-"
                                        End If
                                    Case "H47" 'Animal Collision Included?
                                        bQuestionChecked = True
                                        If oUWQuestion.AnswerText.ToUpper = "YES" Then
                                            bAnimalCollisionIncluded = True
                                        End If
                                    Case "H48" '# of animals covered by animal collision
                                        bQuestionChecked = True
                                        If bAnimalCollisionIncluded Then
                                            If oUWQuestion.AnswerText = "" Then
                                                bEnoughInfoToRate = False
                                                sMissing += "HO210 # of animals covered by animal collision" & "-"
                                            End If
                                        End If
                                    Case "H49" 'Payroll Included?
                                        bQuestionChecked = True
                                        If oUWQuestion.AnswerText.ToUpper = "YES" Then
                                            bPayrollIncluded = True
                                        End If
                                    Case "H50" 'total payroll
                                        bQuestionChecked = True
                                        If bPayrollIncluded Then
                                            If oUWQuestion.AnswerText = "" Then
                                                bEnoughInfoToRate = False
                                                sMissing += "HO210 total payroll" & "-"
                                            End If
                                        End If
                                End Select
                            Next
                            If Not bQuestionChecked Then
                                bEnoughInfoToRate = False
                                sMissing += "HO210 Not All Questions Checked" & "-"
                            End If
                            bQuestionChecked = False
                        Case "HO215"
                            bQuestionChecked = False
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                Select Case oUWQuestion.QuestionCode.ToUpper
                                    Case "H02" 'length of watercraft
                                        bQuestionChecked = True
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO215 Length of Watercraft" & "-"
                                        End If
                                    Case "H03" 'motor type
                                        bQuestionChecked = True
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO215 Motor Type" & "-"
                                        End If
                                    Case "H05" 'max speed
                                        bQuestionChecked = True
                                        If oUWQuestion.AnswerText = "" Then
                                            bEnoughInfoToRate = False
                                            sMissing += "HO215 Max Speed" & "-"
                                        End If
                                End Select
                            Next
                            If Not bQuestionChecked Then
                                bEnoughInfoToRate = False
                                sMissing += "HO215 Not All Questions Checked" & "-"
                            End If
                            bQuestionChecked = False
                        Case "HO160", "HO160-J", "HO160-O"
                            'make sure they have a least one value entered

                        Case "HO225", "HO225-O", "HO225-T"
                            'need occupied by
                        Case "TDP017"
                            bQuestionChecked = False
                            For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                                If oUWQuestion.QuestionCode = "202" Then
                                    bQuestionChecked = True
                                    If oUWQuestion.AnswerText = "" Then
                                        bEnoughInfoToRate = False
                                        sMissing += "TDP017 Amount Rented For Per Month" & "-"
                                    End If
                                ElseIf oUWQuestion.QuestionCode = "221" Then
                                    bQuestionChecked = True
                                    If oUWQuestion.AnswerText = "" Then
                                        bEnoughInfoToRate = False
                                        sMissing += "TDP017 Term" & "-"
                                    End If
                                End If
                            Next
                            If Not bQuestionChecked Then
                                bEnoughInfoToRate = False
                                sMissing += "TDP017 Not All Questions Checked" & "-"
                            End If
                            bQuestionChecked = False
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
    Public Overridable Sub CheckACVLessThanRC(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
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
                If bHasDwellingReplacementCost Then
                    If .DwellingUnits.Item(0).DwellingAmt < .DwellingUnits.Item(0).ReplacementCashAmt Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Dwellings should be insured for a minimum of the replacement cost when the policy contains replacement cost coverage on the dwelling", "DwellingLessThanRC", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckDwellingLessThanACV(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
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

    Public Overridable Sub CheckRestrictedWindZone(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim distance As Double = New LocationDistance().DistanceFromWaterway(.DwellingUnits(0).Latitude, .DwellingUnits(0).Longitude)

            If oPolicy.DwellingUnits(0).County.ToUpper() = "HARRIS" _
                Or oPolicy.DwellingUnits(0).County.ToUpper() = "FORT BEND" _
                Or oPolicy.DwellingUnits(0).County.ToUpper() = "MONTGOMERY" _
                Or (distance < 50 And .DwellingUnits(0).Region = "2") Then 'if the distance from the waterway is less than 50 miles and in texas
                .Notes = (AddNote(.Notes, "Ineligible Risk: Risk is located in restricted wind zone. ", "RestrictedZone", "IER", .Notes.Count))
            End If
        End With

        ' Override the restricted zone ier for several zips
        ' Weather service is returning the wrong lat/longitude for some zips
        RestrictedZoneOverride(oPolicy)
    End Sub

    Public Overridable Sub CheckMinMaxCoverages(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            'min\max coverages
            Select Case .ProgramType.ToUpper
                Case "HOMEOWNERS", "DWELLING1"
                    If .DwellingUnits(0).DwellingAmt < 50000 And .DwellingUnits(0).DwellingAmt <> 0 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: You must have at least $50,000 of dwelling coverage.", "MinCoverage", "IER", .Notes.Count))
                    Else
                        If .DwellingUnits(0).DwellingAmt > 750000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Homes must be valued at $750,000 or less.", "MaxCoverage", "IER", .Notes.Count))
                        ElseIf .DwellingUnits(0).DwellingAmt > 500000 Then
                            .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Homes valued over $500,000 but less than or equal to $750,000 require underwriting approval.", "MaxCoverage", "UWW", .Notes.Count))
                        End If
                    End If
                Case "TENANT"
                    If .DwellingUnits(0).ContentsAmt < 15000 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: You must have at least $15,000 of coverage.", "MinCoverage", "IER", .Notes.Count))
                    ElseIf .DwellingUnits(0).ContentsAmt > 75000 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Maximum amount of coverage is $75,000.", "MaxCoverage", "IER", .Notes.Count))
                    End If
            End Select

            If oPolicy.DwellingUnits(0).OwnerOccupiedFlag = False Then
                If oPolicy.DwellingUnits(0).ContentsAmt > 0 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: You cannot have contents coverage when dwelling is tenant occupied.", "TenantCon", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckDeductibles(ByVal oPolicy As clsPolicyHomeOwner)
        Dim bHasEC As Boolean = False
        Dim bHasVMM As Boolean = False
        Dim bDoDed1Check As Boolean = True

        With oPolicy
            For Each oCov As clsHomeOwnerCoverage In .DwellingUnits.Item(0).Coverages
                Select Case oCov.CovGroup.ToUpper
                    Case "EC"
                        bHasEC = True
                    Case "VMM"
                        bHasVMM = True
                End Select
            Next

            If .ProgramType.ToUpper = "DWELLING1" Then
                If Not bHasEC And Not bHasVMM Then
                    bDoDed1Check = False
                End If
            End If

            If CommonRulesFunctions.AllowCode("CheckDed1Ded3GTEDed2") And Not .ProgramType.ToUpper = "DWELLING1" And .DwellingUnits(0).DwellingAmt > 0 And .DwellingUnits(0).Region <> "1" Then
                CheckDed1Ded3GTEDed2(oPolicy)
            End If

            If .ProgramType = "HOMEOWNERS" Or .ProgramType.Contains("DWELLING") Then
                Dim bHarrisCountyTier2 As Boolean = False
                Dim bMontgomeryCountyTier1 As Boolean = False
                If oPolicy.DwellingUnits(0).Region = "2" Then
                    If oPolicy.DwellingUnits(0).County.ToUpper() = "HARRIS" Or oPolicy.DwellingUnits(0).County.ToUpper() = "FORT BEND" Then
                        bHarrisCountyTier2 = True
                    End If
                End If

                If oPolicy.DwellingUnits(0).County.ToUpper() = "MONTGOMERY" Then
                    bMontgomeryCountyTier1 = True
                End If

                If bDoDed1Check Then
                    If bHarrisCountyTier2 Then
                        If .DwellingUnits(0).DwellingAmt > 0 Then
                            If .DwellingUnits(0).Ded1 < 0.02 Then ' Ded 1 must be 2% for Harris County
                                .Notes = (AddNote(.Notes, "Ineligible Risk: 2% Wind/Hail deductible is required in this County.", "Ded1", "IER", .Notes.Count))
                            ElseIf .DwellingUnits(0).Ded1 > 1 And .DwellingUnits.Item(0).Ded1 < (.DwellingUnits(0).DwellingAmt * 0.02) Then 'else flat calc is > 2%
                                .Notes = (AddNote(.Notes, "Ineligible Risk: 2% Wind/Hail deductible is required in this territory.", "Ded1", "IER", .Notes.Count))
                            End If
                        End If
                    ElseIf bMontgomeryCountyTier1 Then
                        If .DwellingUnits(0).DwellingAmt > 0 Then
                            If .DwellingUnits(0).Ded1 < 0.01 Then ' Ded 1 must be 1% for Montgomery County
                                .Notes = (AddNote(.Notes, "Ineligible Risk: 1% Wind/Hail deductible is required in this County.", "Ded1", "IER", .Notes.Count))
                            ElseIf .DwellingUnits(0).Ded1 > 1 And .DwellingUnits.Item(0).Ded1 < (.DwellingUnits(0).DwellingAmt * 0.02) Then 'else flat calc is > 2%
                                .Notes = (AddNote(.Notes, "Ineligible Risk: 1% Wind/Hail deductible is required in this territory.", "Ded1", "IER", .Notes.Count))
                            End If
                        End If
                    Else
                        If .ProgramType.ToUpper = "DWELLING1" Then
                            '1000 is min
                            If .DwellingUnits(0).DwellingAmt > 0 Then
                                If .DwellingUnits(0).Ded1 > 1 And .DwellingUnits(0).Ded1 < 1000 Then 'ded1 is flat and ded1 < 1000
                                    'not allowed
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher Deductible in this territory.", "Ded1", "IER", .Notes.Count))
                                ElseIf (.DwellingUnits(0).Ded1 * (.DwellingUnits(0).DwellingAmt)) >= 1000 Then 'ded1 is % and >= 1000
                                    'allowed
                                Else
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher Deductible in this territory.", "Ded1", "IER", .Notes.Count))
                                End If
                            End If
                        Else
                            '1% minimum Deductible
                            If .DwellingUnits(0).DwellingAmt > 0 Then
                                'ded1 is windHail
                                If .DwellingUnits(0).Ded1 < 0.01 Then 'ded1 not flat and ded1 < 1%
                                    'not allowed
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher Wind/Hail Deductible.", "Ded1", "IER", .Notes.Count))
                                ElseIf (.DwellingUnits(0).DwellingAmt * 0.01) < .DwellingUnits.Item(0).Ded1 Then 'else flat calc is > 1%
                                    'allowed
                                ElseIf .DwellingUnits.Item(0).Ded1 >= 0.01 And .DwellingUnits.Item(0).Ded1 < 1 Then
                                    'allowed
                                ElseIf .DwellingUnits.Item(0).Ded1 >= 1000 Then
                                    'allowed
                                Else
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher Wind/Hail Deductible.", "Ded1", "IER", .Notes.Count))
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckApartment(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType = "HOMEOWNERS" Or .ProgramType.Contains("DWELLING") Then
                'No apartments allowed
                If .DwellingUnits(0).BuildingTypeCode = "APT" Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Apartment buildings are only eligible on the HOT form.", "BuildingType", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

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

    Public Overridable Sub CheckFireOnlyEndorsements(ByVal oPolicy As clsPolicyHomeOwner)
        Dim bHasEC As Boolean = False
        Dim bHasVMM As Boolean = False

        With oPolicy
            For Each oCov As clsHomeOwnerCoverage In .DwellingUnits.Item(0).Coverages
                Select Case oCov.CovGroup.ToUpper
                    Case "EC"
                        bHasEC = True
                    Case "VMM"
                        bHasVMM = True
                End Select
            Next

            If .ProgramType.ToUpper = "DWELLING1" Then
                If Not bHasEC And Not bHasVMM Then
                    'not all endorsements are applicable to a FIRE only policy
                    For Each oEnd As clsEndorsementFactor In oPolicy.EndorsementFactors
                        If Not oEnd.IsMarkedForDelete Then
                            Select Case oEnd.Type
                                Case "WindHailExclusion"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Windstorm, Hurricane and Hail Exclusion Agreement does not apply to Fire only policies.", "NAEndorsementWindHailExclusion", "IER", .Notes.Count))
                                Case "CosmeticRoofExclusion"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Exclusion of Cosmetic Damage to Roof Caused by Hail does not apply to Fire only policies.", "NAEndorsementCosmeticRoofExclusion", "IER", .Notes.Count))
                                Case "AEC_Plus"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Additional Extended Coverage does not apply to Fire only policies.", "NAEndorsementAEC_Plus", "IER", .Notes.Count))
                                Case "WaterBackUp"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Water Back Up and Sump Discharge Overflow does not apply to Fire only policies.", "NAEndorsementWaterBackUp", "IER", .Notes.Count))
                                Case "FairRental"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Fair Rental does not apply to Fire only policies.", "NAEndorsementFairRental", "IER", .Notes.Count))
                                Case "Mold4_25"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Mold, Fungi or Other Microbes Coverage - 25% of Dwelling does not apply to Fire only policies.", "NAEndorsementMold4_25", "IER", .Notes.Count))
                                Case "Mold4_50"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Mold, Fungi or Other Microbes Coverage - 50% of Dwelling does not apply to Fire only policies.", "NAEndorsementMold4_50", "IER", .Notes.Count))
                                Case "Mold4_100"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Mold, Fungi or Other Microbes Coverage - 100% of Dwelling does not apply to Fire only policies.", "NAEndorsementMold4_100", "IER", .Notes.Count))
                                Case "ResidenceGlass"
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: The Unscheduled Glass does not apply to Fire only policies.", "NAEndorsementResidenceGlass", "IER", .Notes.Count))
                            End Select
                        End If
                    Next
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckRoofCondition(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits(0).CondOfRoof <> "Good" And .DwellingUnits(0).CondOfRoof <> "" Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Roof must be in good condition.", "RoofCondition", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckRoofType(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '16. Tile, slate, tar & gravel or wood roof entered
            Select Case .DwellingUnits.Item(0).RoofTypeCode
                Case "Slate Roof", "Tar & Gravel Roof", "Wood Roof", "Tile Roof", "Concrete Tile Roof"
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have this roof", "RoofType", "IER", .Notes.Count))
                Case Else
            End Select
        End With
    End Sub

    Public Overridable Sub CheckMetal2Roof(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Select Case .DwellingUnits.Item(0).RoofTypeCode
                Case "Metal Roof"
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have this roof", "RoofType", "IER", .Notes.Count))
                Case Else
            End Select
        End With
    End Sub

    Public Overridable Sub CheckMetalRoof(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '18. Metal roof is chosen
            If .DwellingUnits.Item(0).RoofTypeCode = "Metal Roof" Then
                Select Case .DwellingUnits(0).Territory
                    Case "2", "3", "4", "7", "15C", "15N", "16C", "16D", "16N", "18", "19C", "19N", "20"
                        Dim bHasCosmeticExclusion As Boolean = False
                        For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                            If oEndorse.Type = "CosmeticRoofExclusion" Then
                                bHasCosmeticExclusion = True
                                Exit For
                            End If
                        Next
                        If bHasCosmeticExclusion Then
                            .Notes = (AddNote(.Notes, "Warning: Must obtain impact resistant certificate and signed cosmetic damage exclusion", "MetalRoof", "WRN", .Notes.Count))
                        Else
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Must obtain impact resistant certificate and signed cosmetic damage exclusion", "MetalRoof", "IER", .Notes.Count))
                        End If
                End Select
            End If
        End With
    End Sub

    Public Overridable Sub CheckWindHailEndorsement(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy

            Dim sEndName As String = ""
            If .ProgramType.ToUpper = "DWELLING1" Then
                sEndName = "TDP001"
            Else
                sEndName = "HO140"
            End If

            Dim bHasWindHailExclusionEndorsement As Boolean = False
            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                If (oEndorse.Type.ToUpper = "WINDHAILEXCLUSION" And Not oEndorse.IsMarkedForDelete) Or (oEndorse.FactorCode = sEndName And Not oEndorse.IsMarkedForDelete) Then
                    bHasWindHailExclusionEndorsement = True
                    Exit For
                End If
            Next

            If .DwellingUnits.Item(0).Region = "1" Then
                'they have to have the WindHailExclusion endorsement
                If Not bHasWindHailExclusionEndorsement Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: This risk requires the " & sEndName & " - Wind/Hail Exclusion Endorsement", "WindHailExclusionEndorsement", "IER", .Notes.Count))
                End If
            Else
                'they can't have the WindHailExclusion endorsement
                If bHasWindHailExclusionEndorsement Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: The " & sEndName & " - Wind/Hail Exclusion Endorsement is only applicable to Tier 1, please remove the endorsement", "WindHailExclusionEndorsement", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckSecondarySeasonalDwelling(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '7. Answers "Yes" to secondary or seasonal pool : IER for HOA, HOA+, and HOB otherwise must have primary policy num
            If .DwellingUnits.Item(0).SecSeasonalDwelling = True Then
                Select Case .Program
                    Case "HOA", "HOB"
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Secondary or seasonal only allowed for dwelling policies", "Seasonal", "IER", .Notes.Count))
                    Case "HOT", "TDP1"
                        If .DwellingUnits.Item(0).PrimaryPolicyNumber = "" Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Primary policy number must be provided", "PolicyNum", "IER", .Notes.Count))
                        End If
                End Select
            End If
        End With
    End Sub

    Public Overridable Sub CheckTenantDed3(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType.ToUpper = "TENANT" Then
                If (.DwellingUnits(0).Ded3 * (.DwellingUnits(0).ContentsAmt)) >= 250 Then 'ded3 is > 250
                    'allowed
                Else
                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must choose a higher All Other Peril Deductible.", "Ded3", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckMinContents(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType.ToUpper = "DWELLING1" Then
                ' min contents amount is 8000
                If .DwellingUnits.Item(0).ContentsAmt < 8000 And .DwellingUnits.Item(0).ContentsAmt > 0 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: You must select a higher contents amount.", "MinContentsAmt", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckEndorsementFactors(ByVal oPolicy As clsPolicyHomeOwner)
        Dim dJewelryTotalAmt As Decimal = 0
        Dim dFirearmsTotalAmt As Decimal = 0
        Dim dScheduledTotalAmt As Decimal = 0
        Dim bHasWaterBackUp As Boolean = False
        Dim bHasAEC_Plus As Boolean = False
        Dim bHasAPlusOver41 As Boolean = False

        With oPolicy
            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors

                If oPolicy.DwellingUnits(0).HomeAge >= 41 Then
                    If (oPolicy.TransactionNum = 1 Or (oEndorse.IsNew Or oEndorse.IsModified)) And (oEndorse.FactorCode = "HO170P" Or oEndorse.FactorCode = "HO170W " Or oEndorse.FactorCode = "HO214") Then
                        bHasAPlusOver41 = True
                    End If
                End If
                If oEndorse.Type.ToUpper.Contains("SCHEDULEDPROPERTY") Then ' Left(oEndorse.FactorCode, 5) = "HO160" Then
                    dScheduledTotalAmt += oEndorse.Limit

                    For Each oSchProp As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                        Select Case oSchProp.PropertyCategoryDesc.ToUpper
                            Case "JEWELRY"
                                If oSchProp.PropertyAmt > 15000 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Individual scheduled jewelry items must be less than $15,000.", "ScheduledProperty", "IER", .Notes.Count))
                                End If
                                dJewelryTotalAmt += oSchProp.PropertyAmt
                            Case "FIREARMS"
                                If oSchProp.PropertyAmt > 2500 Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Individual scheduled firearm items must be less than $2,500.", "ScheduledPropertyFirearms", "IER", .Notes.Count))
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
                        ElseIf oUWQuestion.QuestionCode = "312" Then 'Business Pursuits
                            If oUWQuestion.AnswerText.ToUpper = "YES" Then
                                If .Status.Trim = "3" Or .Status.Trim = "4" Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Liability cannot be extended to properties with business pursuits (HO225)", "AddlPremBusPursuits", "IER", .Notes.Count))
                                End If
                            End If
                        End If
                    Next

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
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Must select at least 1 Additional Premise (HO225)", "AdditionalPremises", "IER", .Notes.Count))
                    End If
                    'can't have more than 4 total
                    If oEndorse.NumberOfEndorsements > 4 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have more than 4 Additional Premises (HO225)", "AdditionalPremises", "IER", .Notes.Count))
                    End If
                ElseIf oEndorse.Type.ToUpper = "ANTENNASSATELLITE" Then
                    oEndorse.Limit = 0
                    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                        If Not oUWQuestion.QuestionCode = "" Then
                            If oUWQuestion.QuestionCode.ToUpper = "166" Then
                                If Not oUWQuestion.AnswerText = "" Then
                                    If IsNumeric(oUWQuestion.AnswerText) Then
                                        oEndorse.Limit = CInt(oUWQuestion.AnswerText)
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If oEndorse.Limit < 100 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: The minimum limit for Antennas & Satellite Dish Coverage is $100 (HO120)", "AntennasSatelliteMin", "IER", .Notes.Count))
                    ElseIf oEndorse.Limit > 3000 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: The maximum limit for Antennas & Satellite Dish Coverage is $3000 (HO120)", "AntennasSatelliteMax", "IER", .Notes.Count))
                    End If
                ElseIf oEndorse.Type.ToUpper = "INCREASEDLIMITS" Then
                    oEndorse.Limit = 0
                    'add $500 to the limit amount selected since it is an increased limit from $500
                    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                        If Not oUWQuestion.QuestionCode = "" Then
                            If oUWQuestion.QuestionCode.ToUpper = "173" Then
                                If Not oUWQuestion.AnswerText = "" Then
                                    If IsNumeric(oUWQuestion.AnswerText) Then
                                        oEndorse.Limit = CInt(oUWQuestion.AnswerText) + 500
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If oEndorse.Limit > 5000 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: The maximum limit for Jewelry, Watches and Furs Increased Limits is $5000 (HO110)", "IncreasedLimitsMax", "IER", .Notes.Count))
                    End If
                ElseIf oEndorse.Type.ToUpper = "WATERBACKUP" And Not oEndorse.IsMarkedForDelete Then 'HO170W
                    bHasWaterBackUp = True
                ElseIf oEndorse.Type.ToUpper = "AEC_PLUS" And Not oEndorse.IsMarkedForDelete Then 'HO170P
                    bHasAEC_Plus = True
                ElseIf oEndorse.Type.ToUpper = "FAIRRENTAL" Then
                    Dim iTotalRentalAmt As Integer = 0
                    Dim iRentalAmt As Integer = 0
                    Dim iTerm As Integer = 0
                    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                        If oUWQuestion.QuestionCode = "202" Then 'Coverage Amount
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    iRentalAmt = CInt(oUWQuestion.AnswerText)
                                End If
                            End If
                        ElseIf oUWQuestion.QuestionCode = "221" Then 'Term
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    iTerm = CInt(oUWQuestion.AnswerText)
                                End If
                            End If
                        End If
                    Next
                    If CInt(iRentalAmt * iTerm) > (.DwellingUnits.Item(0).DwellingAmt * 0.1) Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have Fair Rental coverage greater than 10% of the dwelling amount (TDP017)", "FairRentalAmt", "IER", .Notes.Count))
                    End If
                ElseIf oEndorse.Type.ToUpper = "CONTENTSREPLACEMENTCOST" Then
                    If .ProgramType.ToUpper = "DWELLING1" Then
                        If .DwellingUnits.Item(0).ContentsAmt = 0 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have Contents Replacement Cost with no contents coverage (TDP902P)", "ContentsReplacementCostIER", "IER", .Notes.Count))
                        End If
                    End If
                End If
            Next

            If dJewelryTotalAmt > 20000 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Total of scheduled jewelry must be less than $20,000.", "ScheduledPropertyTotal", "IER", .Notes.Count))
            End If
            If dFirearmsTotalAmt > 7500 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Total of scheduled firearms must be less than $7,500.", "ScheduledPropertyFirearmsTotal", "IER", .Notes.Count))
            End If
            If .ProgramType = "TENANT" Then
                If .DwellingUnits(0).ContentsAmt < dScheduledTotalAmt Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Scheduled property cannot be greater than the Coverage B limit.", "ScheduledTotal", "IER", .Notes.Count))
                End If
            End If
            If dScheduledTotalAmt > 30000 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Total of scheduled property must be less than $30,000.", "ScheduledTotal", "IER", .Notes.Count))
            End If

            If .ProgramType.ToUpper = "DWELLING1" Then
                If bHasWaterBackUp And Not bHasAEC_Plus Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: If TDP170W is selected, then you must select TDP170P as well.", "MissingTDP170P", "IER", .Notes.Count))
                ElseIf bHasAEC_Plus And Not bHasWaterBackUp Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: If TDP170P is selected, then you must select TDP170W as well.", "MissingTDP170W", "IER", .Notes.Count))
                End If
            Else
                If bHasWaterBackUp And Not bHasAEC_Plus Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: If HO170W is selected, then you must select HO170P as well.", "MissingHO170P", "IER", .Notes.Count))
                ElseIf bHasAEC_Plus And Not bHasWaterBackUp Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: If HO170P is selected, then you must select HO170W as well.", "MissingHO170W", "IER", .Notes.Count))
                End If
            End If

            If bHasAPlusOver41 Then
                If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Homes which are 41 years or older are not eligible for A+ program endorsements (HO170P, HO170W and HO214).", "APlusOver41", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckPlumbing(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits(0).YearOfConstruction > 0 Then
                If (Date.Today.Year - .DwellingUnits(0).YearOfConstruction) > 40 And .ProgramType.ToUpper <> "TENANT" Then
                    If .DwellingUnits(0).PlumbingDesc = "" Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Dwellings over 40 years old must have renovated plumbing", "OldPlumbing", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckWiring(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits(0).YearOfConstruction > 0 Then
                If (Date.Today.Year - .DwellingUnits(0).YearOfConstruction) > 40 And .ProgramType.ToUpper <> "TENANT" Then
                    If .DwellingUnits(0).RenovationWiringDesc = "" Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Dwellings over 40 years old must have renovated wiring", "OldWiring", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckAdditionalInsuredComplete(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            For Each oEnd As clsEndorsementFactor In .EndorsementFactors
                If oEnd.Type.ToUpper = "ADDITIONALINSURED" Then
                    Dim bAddNote As Boolean = False
                    'make sure they have answered all of the questions
                    For Each oUWQuestion As clsUWQuestion In oEnd.UWQuestions
                        If oUWQuestion.QuestionCode = "H35" Then 'Name
                            If oUWQuestion.AnswerText = "" Then
                                bAddNote = True
                            End If
                        ElseIf oUWQuestion.QuestionCode = "204" Then 'Address1
                            If oUWQuestion.AnswerText = "" Then
                                bAddNote = True
                            End If
                        ElseIf oUWQuestion.QuestionCode = "351" Then 'City
                            If oUWQuestion.AnswerText = "" Then
                                bAddNote = True
                            End If
                        ElseIf oUWQuestion.QuestionCode = "352" Then 'State
                            If oUWQuestion.AnswerText = "" Then
                                bAddNote = True
                            End If
                        ElseIf oUWQuestion.QuestionCode = "353" Then 'Zip
                            If oUWQuestion.AnswerText = "" Then
                                bAddNote = True
                            End If
                        End If
                    Next
                    If bAddNote Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: You must complete all of the information for the Additional Insureds", "MissingAddlInsuredInfo", "IER", .Notes.Count))
                    End If
                End If
            Next
        End With
    End Sub

    Public Overridable Sub CheckFireDepartmentSelected(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits(0).FireDept = "" Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: There is no fire department selected please go to the Quote screen and select one", "MissingFireDept", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Function CheckHW146(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        If CommonRulesFunctions.AllowCode("CheckHW146") Then
            If oPolicy.DwellingUnits(0).Latitude = 0 And oPolicy.DwellingUnits(0).Longitude = 0 Then
                If String.IsNullOrEmpty(oPolicy.DwellingUnits(0).Region) Then
                    oPolicy.DwellingUnits(0).Region = "2"
                End If
            Else
                If oPolicy.DwellingUnits(0).County.ToUpper() = "HARRIS" Then
                    Select Case LocationDistance.EastWestOfHW146(oPolicy.DwellingUnits(0).Latitude, oPolicy.DwellingUnits(0).Longitude)
                        Case "WEST"
                            oPolicy.DwellingUnits(0).Region = "2"
                        Case "EAST"
                            oPolicy.DwellingUnits(0).Region = "1"
                    End Select
                End If
            End If
        End If
    End Function

    Public Function CheckHarrisCounty(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        ' Following Code has been flattened on database. 
        'If oPolicy.DwellingUnits(0).County.ToUpper() = "HARRIS" Then
        '    If oPolicy.RateDate > "5/18/2010" Or Now() > CDate("6/1/2010") Then
        '        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in Harris County.", "HarrisRestr", "IER", oPolicy.Notes.Count))
        '    End If
        'End If

        ' Re-using this function to implement the new IER
        If oPolicy.DwellingUnits(0).County.ToUpper() = "HARRIS" _
            Or oPolicy.DwellingUnits(0).County.ToUpper() = "FORT BEND" _
            Or oPolicy.DwellingUnits(0).County.ToUpper() = "MONTGOMERY" Then
            If oPolicy.Program <> "HOB" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The HO-B form is the only eligible policy form for this county.", "HarrisRestr", "IER", oPolicy.Notes.Count))
            End If
        End If

    End Function

    Public Function CheckWildfireCounty(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Dim counties() As String = ConfigurationManager.AppSettings("WildfireCounties").Split(",")

        If oPolicy.DwellingUnits(0).County.Trim() <> "" Then
            For c As Integer = 0 To counties.Length - 1
                If oPolicy.DwellingUnits(0).County.ToUpper() = counties(c).Trim Then
                    If oPolicy.RateDate > "5/18/2010" Or Now() > CDate("9/7/2011") Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county.", "WildfireRestr", "IER", oPolicy.Notes.Count))
                        Exit For
                    End If
                End If
            Next
        End If

    End Function

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

    Private Sub RestrictedZoneOverride(ByRef oPolicy As clsPolicyHomeOwner)
        Dim bRemoveRestriction As Boolean = False
        Dim bAddRestriction As Boolean = False
        Dim DataRows() As DataRow

        ' X = Exclude Restriction
        ' I = Include Restriction
        ' R = Exclude Restriction if criteria are met (Added 4/11/12)
        Dim oOverrideDataset As DataSet = LoadWindRestrictionOverrideTable(oPolicy)

        ' Check to see if this zone should be excluded from the restricted zones 
        DataRows = oOverrideDataset.Tables(0).Select("Type='X'")
        If oPolicy.DwellingUnits(0).Zip.Length > 1 Then
            For Each oRow As DataRow In DataRows
                If oRow("Zip") = oPolicy.DwellingUnits(0).Zip.ToString() Then
                    bRemoveRestriction = True
                    Exit For
                End If
            Next
        End If

        ' Check to see if this zone should be exlcuded if certain criteria are met.
        DataRows = oOverrideDataset.Tables(0).Select("Type='R'")
        If oPolicy.DwellingUnits(0).Zip.Length > 1 Then
            For Each oRow As DataRow In DataRows
                If oRow("Zip") = oPolicy.DwellingUnits(0).Zip.ToString() Then
                    ' Found a matching Zip, now make sure the construction is Brick Veneer and the age is less than database driven age.
                    If oPolicy.DwellingUnits(0).Construction.Contains("Masonry") And (Date.Today.Year - oPolicy.DwellingUnits(0).YearOfConstruction <= LoadHomeAgeRestriction(oPolicy)) Then
                        If OverrideWindRestrictionByAgentOption(oPolicy) Then
                            bRemoveRestriction = True
                            Exit For
                        End If
                    End If
                End If
            Next
        End If

        If bRemoveRestriction Then
            For i As Integer = 0 To oPolicy.Notes.Count - 1
                If oPolicy.Notes(i).NoteDesc = "RestrictedZone" Then
                    oPolicy.Notes.Remove(oPolicy.Notes(i))
                    Exit For
                End If
            Next
        End If

        ' Check to see if this zip should be included in the restricted zones
        DataRows = oOverrideDataset.Tables(0).Select("Type='I'")
        If oPolicy.DwellingUnits(0).Zip.Length > 1 Then
            For Each oRow As DataRow In DataRows
                If oRow("Zip") = oPolicy.DwellingUnits(0).Zip.ToString() Then
                    bAddRestriction = True
                    Exit For
                End If
            Next
        End If

        If bAddRestriction Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot bind policies during a tropical storm warning. ", "RestrictedZone", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Function LoadWindRestrictionOverrideTable(ByVal oPolicy As clsPolicyHomeOwner) As DataSet
        Dim sSql As String = ""

        Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Dim oDS As New DataSet
        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Zip,Type "
                sSql &= " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..WindRestrictOverride with(nolock)"

                'Execute the query
                cmd.CommandText = sSql

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)
                adapter.Fill(oDS, "WindRestrictions")
                Return oDS
            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Function LoadHomeAgeRestriction(ByVal oPolicy As clsPolicyHomeOwner, Optional ByVal bNTRestriction As Boolean = False, Optional ByVal iMaxAgeRestriction As Integer = 0) As Integer
        Dim sSql As String = ""
        Dim bNorthTexasHomeAgeRestriction As Boolean = bNTRestriction
        Dim iYearAgeRestriction As Integer = iMaxAgeRestriction
        Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Dim oDS As New DataSet
        Dim oReader As SqlDataReader = Nothing
        Dim iHomeAgeRestriction As Integer = 0

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program,ItemGroup,ItemValue,EffDate,ExpDate "
                sSql &= " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..StateInfo with(nolock)"
                If bNorthTexasHomeAgeRestriction Then
                    sSql &= " WHERE ItemGroup = 'NORTHTEXASHOMEAGERESTRICTION'"
                ElseIf iYearAgeRestriction = 10 Then
                    sSql &= " WHERE ItemGroup = '10YEARHOMEAGERESTRICTION'"
                ElseIf iYearAgeRestriction = 30 Then
                    sSql &= " WHERE ItemGroup = '30YEARHOMEAGERESTRICTION'"
                Else
                    sSql &= " WHERE ItemGroup = 'HOMEAGERESTRICTION'"
                End If
                sSql &= " AND Program = 'HOM'"
                sSql &= " AND EffDate <= @RateDate"
                sSql &= " AND ExpDate > @RateDate"

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    iHomeAgeRestriction = CInt(oReader.Item("Itemvalue"))
                Loop

                Return iHomeAgeRestriction

            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Private Function OverrideWindRestrictionByAgentOption(ByRef oPolicy As clsPolicyHomeOwner) As Boolean

        Dim oMktCRMService As New MarketingCRMService.InsurCloudAMSServiceSoapClient
        Dim dsAgencyOptions As New DataSet
        Dim bOverrideWindRestriction As Boolean = False

        dsAgencyOptions = oMktCRMService.GetOptions(oPolicy.Agency.AgencyID)
        For Each oRow As DataRow In dsAgencyOptions.Tables(0).Rows
            If oRow.Item("EditValue").ToString.ToUpper = "ALLOWHARRISCOUNTY" Then
                bOverrideWindRestriction = True
                Exit For
            End If
        Next

        Return bOverrideWindRestriction

    End Function

    Private Function OverrideRestrictionByAgentOption(ByRef oPolicy As clsPolicyHomeOwner, ByVal sRestrictionType As String) As Boolean

        Dim oMktCRMService As New MarketingCRMService.InsurCloudAMSServiceSoapClient
        Dim dsAgencyOptions As New DataSet
        Dim bOverrideRestriction As Boolean = False

        dsAgencyOptions = oMktCRMService.GetOptions(oPolicy.Agency.AgencyID)
        For Each oRow As DataRow In dsAgencyOptions.Tables(0).Rows
            If oRow.Item("EditValue").ToString.ToUpper = sRestrictionType Then
                bOverrideRestriction = True
                Exit For
            End If
        Next

        Return bOverrideRestriction

    End Function

    Public Overridable Sub CheckHO214Endorsement(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim bHasHO214 As Boolean = False
            Dim bHasHO170P As Boolean = False
            Dim bHasHO170W As Boolean = False

            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                If oEndorse.FactorCode.ToUpper = "HO214" And Not oEndorse.IsMarkedForDelete Then
                    bHasHO214 = True
                End If

                If oEndorse.FactorCode.ToUpper = "HO170P" And Not oEndorse.IsMarkedForDelete Then
                    bHasHO170P = True
                End If

                If oEndorse.FactorCode.ToUpper = "HO170W" And Not oEndorse.IsMarkedForDelete Then
                    bHasHO170W = True
                End If
            Next

            If bHasHO214 And Not (bHasHO170P And bHasHO170W) Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: The HO214 can only be purchased in combination with the HO170P and HO170W", "WindHailExclusionEndorsement", "IER", .Notes.Count))
            End If

        End With
    End Sub

    Public Overridable Sub CheckBuildingType(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .Program = "HOA" Or .Program = "HOB" Or .Program = "TDP1" Or .Program = "TDP3" Then
                If .DwellingUnits(0).BuildingTypeCode = "BLD3" Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Apartments are not eligible for this program.", "WindHailExclusionEndorsement", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckTXReplacementCost(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .Program = "HOB" Then
                Dim bHasHOBCost As Boolean = False

                For Each oEnd As clsEndorsementFactor In .EndorsementFactors
                    If oEnd.FactorCode.ToUpper.Trim = "HO101" And Not oEnd.IsMarkedForDelete Then
                        bHasHOBCost = True
                        Exit For
                    End If
                Next
                If Not bHasHOBCost Then
                    oPolicy.Notes = (AddNote(.Notes, "Ineligible Risk: The HO101 - Replacement Cost for Personal Property Endorsement is required on HOB policies.", "InvalidReplacementCost", "IER", .Notes.Count))
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
#End Region

#Region "UWW Functions"
    Public Overridable Sub CheckFarmersPersonalLiability(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '9. Chooses farmers personal liability endorsement (HO210)
            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                If oEndorse.FactorCode.ToUpper.StartsWith("HO210") Then
                    .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Farmers Personal Liability Endorsement", "HO210", "UWW", .Notes.Count))
                    Exit For
                End If
            Next
        End With
    End Sub

    Public Overridable Sub CheckNonWeatherClaims(ByVal oPolicy As clsPolicyHomeOwner)
        ' Removed 9/21/2010
        ''Removed Because this is a HOM restriction not a state specific restriction
        'With oPolicy
        '	Dim iNumNonWeatherLoss As Integer = 0
        '	'N-Water, O-NonWeather,W-Weather

        '	With oPolicy.DwellingUnits.Item(0)
        '		For Each oClaim As clsBaseClaim In .Claims
        '			If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) >= oPolicy.EffDate Then
        '				If oPolicy.CallingSystem <> "PAS" Then
        '					If DateAdd(DateInterval.Month, 36, oClaim.ClaimDate) < oPolicy.EffDate Then
        '						'nonchargeable
        '						oClaim.Chargeable = False
        '					End If
        '				End If
        '			Else
        '				If oPolicy.CallingSystem <> "PAS" Then
        '					oClaim.Chargeable = False
        '				End If
        '			End If
        '			If oClaim.ClaimAmt = 0 Then
        '				If oPolicy.CallingSystem <> "PAS" Then
        '					oClaim.Chargeable = False
        '				End If
        '			End If
        '			If oClaim.Chargeable Then
        '				If oClaim.ClaimTypeIndicator.Trim <> "W" Then
        '					iNumNonWeatherLoss += 1
        '				End If
        '			End If
        '		Next
        '	End With

        '	If iNumNonWeatherLoss > 1 Then
        '		.Notes = (AddNote(.Notes, "Ineligible Risk: UW approval is required for multiple non-weather claims.", "NonW", "IER", .Notes.Count))
        '	End If
        'End With
    End Sub

#End Region

#Region "WRN Functions"
    Public Overridable Sub CheckWRNRoofType(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '18. Metal roof is chosen in select select territories
            If .DwellingUnits.Item(0).RoofTypeCode = "Metal Roof" Then
                If oPolicy.Program = "TDP1" Then
                    .Notes = (AddNote(.Notes, "Warning: Due to the selection of a metal roof the TDP022 has been applied", "TDP022Added", "WRN", .Notes.Count))
                Else
                    .Notes = (AddNote(.Notes, "Warning: Due to the selection of a metal roof the HO145 has been applied", "HO145Added", "WRN", .Notes.Count))
                End If
                Select Case .DwellingUnits.Item(0).Territory
                    Case "1", "5", "6", "8", "9", "10", "11", "11A", "12", "13", "13A", "14", "14A", "16D", "16S", "17", "17A"
                        'these territories are ok
                    Case Else
                        'anything else is bad
                        .Notes = (AddNote(.Notes, "Warning: Must obtain impact resistant certificate and signed cosmetic damage exclusion", "MetalRoof", "WRN", .Notes.Count))
                End Select
            End If
        End With
    End Sub

    Public Overridable Sub CheckWRNHomeAge(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).HomeAge >= 41 And oPolicy.Program = "HOB" Then
                .Notes = (AddNote(.Notes, "Warning: Due to the age of the risk the HOE216 – Limited Water Damage Coverage endorsement has been applied", "HO216Added", "WRN", .Notes.Count))
            End If
        End With
    End Sub
#End Region


    Public Overrides Sub SetIncreasedLimitFactors(ByVal oPolicy As clsPolicyHomeOwner)

        Dim oNewFactor As clsBaseFactor = Nothing
        Dim iLiaNum As Integer = 0
        Dim iMedNum As Integer = 0
        Dim iContentNum As Integer = 0
        Dim iOtherStructureNum As Integer = 0

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

            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                If oFactor.FactorCode.Length > 9 Then
                    If oFactor.FactorCode.Substring(0, 9).ToUpper = "INCR_CONT" Then
                        iContentNum = oFactor.FactorNum
                        oPolicy.PolicyFactors.Remove(oFactor)
                        Exit For
                    End If
                End If
            Next

            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                If oFactor.FactorCode.Length > 9 Then
                    If oFactor.FactorCode.Substring(0, 9).ToUpper = "INCR_OTST" Then
                        iOtherStructureNum = oFactor.FactorNum
                        oPolicy.PolicyFactors.Remove(oFactor)
                        Exit For
                    End If
                End If
            Next

            If Not oPolicy.ProgramType.Contains("DWELLING") Then
                If iLiaNum = 0 Then
                    iLiaNum = oPolicy.PolicyFactors.Count + 1
                End If
                If oPolicy.DwellingUnits(0).LiaLimit = 0 Then oPolicy.DwellingUnits(0).LiaLimit = 25000 'default to min
                oNewFactor = New clsBaseFactor
                Select Case oPolicy.DwellingUnits(0).LiaLimit
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
                    Case Else
                        Throw New Exception("Liability Limit is currently: " & oPolicy.DwellingUnits(0).LiaLimit & ".  Only 25k,50k,100k,200k,300k,and 500k are supported")
                End Select

                If Not oNewFactor Is Nothing Then
                    oPolicy.PolicyFactors.Add(oNewFactor)
                    oNewFactor = Nothing
                End If

                If iMedNum = 0 Then
                    iMedNum = oPolicy.PolicyFactors.Count + 1
                End If
                If oPolicy.DwellingUnits(0).MedPayLimit = 0 Then oPolicy.DwellingUnits(0).MedPayLimit = 500 'default to min
                oNewFactor = New clsBaseFactor
                Select Case oPolicy.DwellingUnits(0).MedPayLimit
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

            '' contents
            If iContentNum = 0 Then
                iContentNum = oPolicy.PolicyFactors.Count + 1
            End If
            'If oPolicy.DwellingUnits(0).ContentsAmtPct = 0 Then oPolicy.DwellingUnits(0).ContentsAmtPct = 500 'default to min
            Dim dContentAmtPct As Decimal

            If oPolicy.DwellingUnits(0).DwellingAmt > 0 Then
                dContentAmtPct = oPolicy.DwellingUnits(0).ContentsAmt / oPolicy.DwellingUnits(0).DwellingAmt
            End If

            If oPolicy.DwellingUnits(0).DwellingAmt > 0 Then
                If oPolicy.DwellingUnits(0).ContentsAmtPct = 0 Then
                    oPolicy.DwellingUnits(0).ContentsAmtPct = Math.Round(oPolicy.DwellingUnits(0).ContentsAmt / oPolicy.DwellingUnits(0).DwellingAmt, 1, MidpointRounding.AwayFromZero)
                End If
            End If

            oNewFactor = New clsBaseFactor
            Select Case oPolicy.DwellingUnits(0).ContentsAmtPct
                Case 0.1
                    oNewFactor.FactorCode = "INCR_CONT_10"
                    oNewFactor.FactorDesc = "Increased Contents Limit 10%"
                    oNewFactor.FactorName = "Increased Contents Limit 10%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case 0.2
                    oNewFactor.FactorCode = "INCR_CONT_20"
                    oNewFactor.FactorDesc = "Increased Contents Limit 20%"
                    oNewFactor.FactorName = "Increased Contents Limit 20%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case 0.3
                    oNewFactor.FactorCode = "INCR_CONT_30"
                    oNewFactor.FactorDesc = "Increased Contents Limit 30%"
                    oNewFactor.FactorName = "Increased Contents Limit 30%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case 0.4
                    oNewFactor.FactorCode = "INCR_CONT_40"
                    oNewFactor.FactorDesc = "Increased Contents Limit 40%"
                    oNewFactor.FactorName = "Increased Contents Limit 40%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case 0.5
                    oNewFactor.FactorCode = "INCR_CONT_50"
                    oNewFactor.FactorDesc = "Increased Contents Limit 50%"
                    oNewFactor.FactorName = "Increased Contents Limit 50%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case 0.6
                    oNewFactor.FactorCode = "INCR_CONT_60"
                    oNewFactor.FactorDesc = "Increased Contents Limit 60%"
                    oNewFactor.FactorName = "Increased Contents Limit 60%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case 0.7
                    oNewFactor.FactorCode = "INCR_CONT_70"
                    oNewFactor.FactorDesc = "Increased Contents Limit 70%"
                    oNewFactor.FactorName = "Increased Contents Limit 70%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
                Case Else '0.0
                    oNewFactor.FactorCode = "INCR_CONT_0"
                    oNewFactor.FactorDesc = "Increased Contents Limit 0%"
                    oNewFactor.FactorName = "Increased Contents Limit 0%"
                    oNewFactor.FactorNum = iContentNum
                    oNewFactor.IndexNum = iContentNum
                    'oNewFactor.CovType = "N"
            End Select

            If Not oNewFactor Is Nothing Then
                oPolicy.PolicyFactors.Add(oNewFactor)
                oNewFactor = Nothing
            End If

            '' other structures
            If iOtherStructureNum = 0 Then
                iOtherStructureNum = oPolicy.PolicyFactors.Count + 1
            End If

            oNewFactor = New clsBaseFactor
            Dim dOtherStructuresAmt As Decimal
            dOtherStructuresAmt = oPolicy.DwellingUnits(0).OtherStructureAmt + oPolicy.DwellingUnits(0).OtherStructureAddnAmt

            Dim dOtherStructurePct As Decimal
            If oPolicy.DwellingUnits(0).DwellingAmt > 0 Then
                dOtherStructurePct = dOtherStructuresAmt / oPolicy.DwellingUnits(0).DwellingAmt
                Select Case dOtherStructurePct
                    Case Is > 0.15
                        oNewFactor.FactorCode = "INCR_OTST_20"
                        oNewFactor.FactorDesc = "Increased Other Structures Lim"
                        oNewFactor.FactorName = "Increased Other Structures Lim"
                        oNewFactor.FactorNum = iOtherStructureNum
                        oNewFactor.IndexNum = iOtherStructureNum
                        'oNewFactor.CovType = "N"
                    Case Is > 0.1
                        oNewFactor.FactorCode = "INCR_OTST_15"
                        oNewFactor.FactorDesc = "Increased Other Structures Lim"
                        oNewFactor.FactorName = "Increased Other Structures Lim"
                        oNewFactor.FactorNum = iOtherStructureNum
                        oNewFactor.IndexNum = iOtherStructureNum
                        'oNewFactor.CovType = "N"
                    Case Else
                        oNewFactor.FactorCode = "INCR_OTST_10"
                        oNewFactor.FactorDesc = "Increased Other Structures Lim"
                        oNewFactor.FactorName = "Increased Other Structures Lim"
                        oNewFactor.FactorNum = iOtherStructureNum
                        oNewFactor.IndexNum = iOtherStructureNum
                        'oNewFactor.CovType = "N"
                End Select
            End If

            If Not oNewFactor Is Nothing Then
                oPolicy.PolicyFactors.Add(oNewFactor)
                oNewFactor = Nothing
            End If


        Catch ex As Exception
            Throw New ArgumentException(ex.Message)
        Finally
            If Not oNewFactor Is Nothing Then
                oNewFactor = Nothing
            End If
        End Try

    End Sub
    'Public Overrides Function AddPASPolicyFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

    '	MyBase.AddPASPolicyFactors(oPolicy)

    '	Dim oNote As clsBaseNote = Nothing
    '	'Smoker Surcharge
    '	If HasDiscount(oPolicy, "SMOKER") Then
    '		AddPolicyFactor(oPolicy, "SMOKER")
    '	End If

    '	'WOOD_STOVE Surcharge
    '	If HasDiscount(oPolicy, "WOOD_STOVE") Then
    '		AddPolicyFactor(oPolicy, "WOOD_STOVE")
    '	End If

    '	' Local Police Alarm
    '	If HasDiscount(oPolicy, "LP_ALARM") Then
    '		AddPolicyFactor(oPolicy, "LP_ALARM")
    '	End If

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
    '	End If


    '       If Not bAccreditedBuilderAdded Then
    '           RemoveDiscount(oPolicy, "NEW_PUR1")
    '           RemoveDiscount(oPolicy, "NEW_PUR2")
    '           RemoveDiscount(oPolicy, "NEW_PUR3")
    '       End If

    '	' Senior Discount
    '       ' Age 60 or Age 55 and retired
    '       Dim bHasSenior As Boolean = False
    '	If oPolicy.PolicyInsured.DOB <> CDate("1/1/1901") And oPolicy.PolicyInsured.DOB <> CDate("1/1/1900") Then
    '		If oPolicy.PolicyInsured.Age >= 60 Then
    '               AddPolicyFactor(oPolicy, "SENIOR")
    '               AddDiscount(oPolicy, "SENIOR")
    '               bHasSenior = True
    '		ElseIf oPolicy.PolicyInsured.Age >= 55 And oPolicy.PolicyInsured.Occupation.ToUpper.Trim = "RETIRED" Then
    '               AddPolicyFactor(oPolicy, "SENIOR")
    '               AddDiscount(oPolicy, "SENIOR")
    '               bHasSenior = True
    '		End If
    '       End If

    '       If Not bHasSenior Then
    '           RemoveDiscount(oPolicy, "SENIOR")
    '       End If

    '	'Non-Smoker Discount
    '	If HasDiscount(oPolicy, "NON_SMOKER") Then
    '		AddPolicyFactor(oPolicy, "NON_SMOKER")
    '	End If



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
    '	If oPolicy.ProgramType.ToUpper = "HOMEOWNERS" Or oPolicy.ProgramType.ToUpper = "DWELLING1" Then
    '		Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
    '		Dim DataRows() As DataRow
    '		Dim oStateInfoTable As DataTable = Nothing

    '		oStateInfoTable = oStateInfoDataSet.Tables(0)
    '		DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='HOMEAGEGREATERTHAN16'")

    '		Dim sHomeAgeGreaterThan16 As String = Now()

    '		For Each oRow As DataRow In DataRows
    '			sHomeAgeGreaterThan16 = oRow("ItemValue")
    '		Next

    '		If oPolicy.RateDate < CDate(sHomeAgeGreaterThan16) Then
    '			Select Case oPolicy.DwellingUnits(0).HomeAge
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

    '			End Select
    '		Else
    '			Select Case oPolicy.DwellingUnits(0).HomeAge
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
    '				Case 26 To 40
    '					AddPolicyFactor(oPolicy, "HOME20")
    '				Case Is > 40
    '					AddPolicyFactor(oPolicy, "HOME21")
    '			End Select
    '		End If
    '	End If

    'End Function


    Public Overrides Function AddPolicyFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        'TX

        'Set Loss Of Use Coverage Amount
        Select Case oPolicy.Program
            Case "HOA"
                oPolicy.DwellingUnits(0).LossOfUseAmt = (oPolicy.DwellingUnits.Item(0).DwellingAmt * 0.1)
            Case "HOB"
                oPolicy.DwellingUnits(0).LossOfUseAmt = (oPolicy.DwellingUnits.Item(0).DwellingAmt * 0.2)
            Case "HOT"
                oPolicy.DwellingUnits(0).LossOfUseAmt = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 0.2)
            Case "TDP1"
                Dim oEndorse As clsEndorsementFactor = GetEndorsement(oPolicy, "TDP017")
                If oEndorse Is Nothing Then
                    oPolicy.DwellingUnits(0).LossOfUseAmt = 0
                Else
                    Dim iTotalRentalAmt As Integer = 0
                    Dim iRentalAmt As Integer = 0
                    Dim iTerm As Integer = 0
                    For Each oUWQuestion As clsUWQuestion In oEndorse.UWQuestions
                        If oUWQuestion.QuestionCode = "202" Then 'Coverage Amount
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    iRentalAmt = CInt(oUWQuestion.AnswerText)
                                End If
                            End If
                        ElseIf oUWQuestion.QuestionCode = "221" Then 'Term
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    iTerm = CInt(oUWQuestion.AnswerText)
                                End If
                            End If
                        End If
                    Next
                    oPolicy.DwellingUnits(0).LossOfUseAmt = iRentalAmt * iTerm
                End If

            Case Else
                oPolicy.DwellingUnits(0).LossOfUseAmt = 0
        End Select

        Dim oRatedFactors() As clsBaseFactor
        ReDim oRatedFactors(-1)
        ' Copy off factors with ratedfactor
        ' then add it back after the clear
        Dim iCapFactorCount As Integer = 0
        For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
            If Len(oFactor.RatedFactor) > 0 Then
                ' Copy this off and restore it after the clear
                ReDim Preserve oRatedFactors(oRatedFactors.Length)
                oRatedFactors(oRatedFactors.Length - 1) = oFactor
                iCapFactorCount += 1
            End If
        Next

        'If iCapFactorCount > 1 Then
        '    Throw New Exception("Cannot have more than one cap factor on a policy")
        'End If

        oPolicy.PolicyFactors.Clear()
        For Each oFactor As clsBaseFactor In oRatedFactors
            oPolicy.PolicyFactors.Add(oFactor)
        Next

        SetIncreasedLimitFactors(oPolicy)
        'SetLossLevel(oPolicy)

        Dim oNote As clsBaseNote = Nothing

        'FORM
        AddPolicyFactor(oPolicy, "FORM")

        'REPLACE_FULL (Only for HOB)
        If oPolicy.Program = "HOB" Then
            AddPolicyFactor(oPolicy, "REPLACE_FULL")
        End If

        'Smoker Surcharge
        If HasDiscount(oPolicy, "SMOKER") Then
            AddPolicyFactor(oPolicy, "SMOKER")
        End If

        'WOOD_STOVE Surcharge
        If HasDiscount(oPolicy, "WOOD_STOVE") Then
            AddPolicyFactor(oPolicy, "WOOD_STOVE")
        End If

        'INELIGIBLE Risk Surcharge
        If HasDiscount(oPolicy, "INELIGIBLE") Then
            AddPolicyFactor(oPolicy, "INELIGIBLE")
        End If

        ' Local Police Alarm
        If HasDiscount(oPolicy, "LP_ALARM") Then
            AddPolicyFactor(oPolicy, "LP_ALARM")
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

        'only for HOA and HOB - this should be handled with dynamic build of drop downs
        'F_ALARM
        Dim bContainsFireAlarmDiscount As Boolean = False
        If HasDiscount(oPolicy, "F_ALARM") Then
            AddPolicyFactor(oPolicy, "F_ALARM")
            bContainsFireAlarmDiscount = True
        Else
            bContainsFireAlarmDiscount = False
        End If

        'P_ALARM
        Dim bContainsPoliceAlarmDiscount As Boolean = False
        If HasDiscount(oPolicy, "P_ALARM") Then
            If oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                'the police alarm discount should only be applicabale if VMM coverage is purchased
                Dim bHasVMM As Boolean = False
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                    If oCov.CovGroup.ToString.ToUpper = "VMM" Then
                        bHasVMM = True
                        Exit For
                    End If
                Next
                If bHasVMM Then
                    AddPolicyFactor(oPolicy, "P_ALARM")
                    bContainsPoliceAlarmDiscount = True
                Else
                    bContainsPoliceAlarmDiscount = False
                End If
            Else
                AddPolicyFactor(oPolicy, "P_ALARM")
                bContainsPoliceAlarmDiscount = True
            End If
        Else
            bContainsPoliceAlarmDiscount = False
        End If

        'PREMRED
        If Not oNote Is Nothing Then
            oNote = Nothing
        End If
        oNote = GetNote(oPolicy, "PREMRED1")
        If Not oNote Is Nothing Then
            AddPolicyFactor(oPolicy, "PREMRED1")
        End If
        If Not oNote Is Nothing Then
            oNote = Nothing
        End If
        oNote = GetNote(oPolicy, "PREMRED2")
        If Not oNote Is Nothing Then
            AddPolicyFactor(oPolicy, "PREMRED2")
        End If
        If Not oNote Is Nothing Then
            oNote = Nothing
        End If
        oNote = GetNote(oPolicy, "PREMRED1_2")
        If Not oNote Is Nothing Then
            AddPolicyFactor(oPolicy, "PREMRED1_2")
        End If

        'ROOF
        If HasDiscount(oPolicy, "ROOF_1") Then
            If oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                'the roof discount should only be applicabale if EC coverage is purchased
                Dim bHasEC As Boolean = False
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                    If oCov.CovGroup.ToString.ToUpper = "EC" Then
                        bHasEC = True
                        Exit For
                    End If
                Next
                If bHasEC Then
                    AddPolicyFactor(oPolicy, "ROOF_1")
                End If
            Else
                AddPolicyFactor(oPolicy, "ROOF_1")
            End If
        End If

        If HasDiscount(oPolicy, "ROOF_2") Then
            If oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                'the roof discount should only be applicabale if EC coverage is purchased
                Dim bHasEC As Boolean = False
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                    If oCov.CovGroup.ToString.ToUpper = "EC" Then
                        bHasEC = True
                        Exit For
                    End If
                Next
                If bHasEC Then
                    AddPolicyFactor(oPolicy, "ROOF_2")
                End If
            Else
                AddPolicyFactor(oPolicy, "ROOF_2")
            End If
        End If

        If HasDiscount(oPolicy, "ROOF_3") Then
            If oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                'the roof discount should only be applicabale if EC coverage is purchased
                Dim bHasEC As Boolean = False
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                    If oCov.CovGroup.ToString.ToUpper = "EC" Then
                        bHasEC = True
                        Exit For
                    End If
                Next
                If bHasEC Then
                    AddPolicyFactor(oPolicy, "ROOF_3")
                End If
            Else
                AddPolicyFactor(oPolicy, "ROOF_3")
            End If
        End If

        If HasDiscount(oPolicy, "ROOF_4") Then
            If oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                'the roof discount should only be applicabale if EC coverage is purchased
                Dim bHasEC As Boolean = False
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                    If oCov.CovGroup.ToString.ToUpper = "EC" Then
                        bHasEC = True
                        Exit For
                    End If
                Next
                If bHasEC Then
                    AddPolicyFactor(oPolicy, "ROOF_4")
                End If
            Else
                AddPolicyFactor(oPolicy, "ROOF_4")
            End If
        End If

        'HOME
        'only for HOA and HOB
        'No Note Here, Always Check for This
        If oPolicy.ProgramType.ToUpper = "HOMEOWNERS" Or oPolicy.ProgramType.ToUpper = "DWELLING1" Then
            Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            Dim DataRows() As DataRow
            Dim oStateInfoTable As DataTable = Nothing

            oStateInfoTable = oStateInfoDataSet.Tables(0)
            DataRows = oStateInfoTable.Select("Program IN ('" & oPolicy.Program & "', 'HOM') AND ItemGroup='HOMEAGEGREATERTHAN16'")

            Dim sHomeAgeGreaterThan16 As String = Now()

            For Each oRow As DataRow In DataRows
                sHomeAgeGreaterThan16 = oRow("ItemValue")
            Next

            If oPolicy.RateDate < CDate(sHomeAgeGreaterThan16) Then
                Select Case oPolicy.DwellingUnits(0).HomeAge
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

                End Select
            Else
                Select Case oPolicy.DwellingUnits(0).HomeAge
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
                    Case 26 To 40
                        AddPolicyFactor(oPolicy, "HOME20")
                    Case Is > 40
                        AddPolicyFactor(oPolicy, "HOME21")
                End Select
            End If
        End If

        'BLD Factor
        If oPolicy.DwellingUnits(0).BuildingTypeCode <> "" Then
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

        'WOOD ROOF
        If oPolicy.DwellingUnits.Item(0).RoofTypeCode.ToUpper = "WOOD ROOF" Then
            AddPolicyFactor(oPolicy, "WOOD_ROOF")
        End If

        'WIND HAIL EXCLUSION
        For Each oEnd As clsEndorsementFactor In oPolicy.EndorsementFactors
            If (oEnd.Type.ToUpper = "WINDHAILEXCLUSION" And Not oEnd.IsMarkedForDelete) Or ((oEnd.FactorCode = "HO140" Or oEnd.FactorCode = "TDP001") And Not oEnd.IsMarkedForDelete) Then
                AddPolicyFactor(oPolicy, "X_WIND_HAIL")
                Exit For
            End If
        Next

        ' RESET MULTILINE3 AND MULTILINE5 DISCOUNTS (These should get re-applied below based on MULTILINE1, 2, and 4)
        RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE3")
        RemovePolicyFactor(oPolicy, "MULTILINE3")
        RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE5")
        RemovePolicyFactor(oPolicy, "MULTILINE5")

        For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
            If oDiscount.FactorCode = "MULTILINE3" OrElse oDiscount.FactorCode = "MULTILINE5" Then
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
            If oPolicy.CompanionPAPolicyID = "" And oPolicy.CallingSystem.ToUpper <> "WEBRATER" Then
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
            'Rolling back the change put in for Improve 3804.
            'ElseIf oPolicy.DwellingUnits(0).Region = "1" Then
            '    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: This risk requires a Flood Companion Policy. Flood Policies must be written through Imperial.", "IMPCOMPFLOODREQUIRED", "IER", oPolicy.Notes.Count))
        Else
            bHasFlood = False
            RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE2")
        End If

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




        'RENEWAL FACTORS
        ' 5/26/2011 Removed since this doesn't seem to be used anywhere
        ' throwing off icebox since it doesn't know what this code is
        'If oPolicy.Type.ToUpper = "RENEWAL" Then
        '    If FactorOnPolicy(oPolicy, "NOCLAIM") Then
        '        AddPolicyFactor(oPolicy, "RNW")
        '    Else
        '        AddPolicyFactor(oPolicy, "RNW-C")
        '    End If
        'End If

        'add needed endorsements
        Dim oEndorsement As clsEndorsementFactor = Nothing
        Dim bContainsRoofDiscount As Boolean = False

        For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
            If FactorOnPolicy(oPolicy, "ROOF_1") Then
                bContainsRoofDiscount = True
                oPolicy.DwellingUnits.Item(0).HailResistiveRoofDisc = "ROOF_1"
                Exit For
            ElseIf FactorOnPolicy(oPolicy, "ROOF_2") Then
                bContainsRoofDiscount = True
                oPolicy.DwellingUnits.Item(0).HailResistiveRoofDisc = "ROOF_2"
                Exit For
            ElseIf FactorOnPolicy(oPolicy, "ROOF_3") Then
                bContainsRoofDiscount = True
                oPolicy.DwellingUnits.Item(0).HailResistiveRoofDisc = "ROOF_3"
                Exit For
            ElseIf FactorOnPolicy(oPolicy, "ROOF_4") Then
                bContainsRoofDiscount = True
                oPolicy.DwellingUnits.Item(0).HailResistiveRoofDisc = "ROOF_4"
                Exit For
            End If
        Next

        If bContainsRoofDiscount Then
            'only for HOA and HOB
            If oPolicy.Program = "HOA" Or oPolicy.Program = "HOB" Then
                If Not oEndorsement Is Nothing Then
                    oEndorsement = Nothing
                End If
                oEndorsement = GetEndorsement(oPolicy, "HO145") 'TDP022
                If oEndorsement Is Nothing Then
                    AddEndorsementFactor(oPolicy, "HO145") 'TDP022
                End If
                If Not oEndorsement Is Nothing Then
                    oEndorsement = Nothing
                End If
            ElseIf oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                If Not oEndorsement Is Nothing Then
                    oEndorsement = Nothing
                End If
                oEndorsement = GetEndorsement(oPolicy, "TDP022") 'TDP022
                If oEndorsement Is Nothing Then
                    AddEndorsementFactor(oPolicy, "TDP022") 'TDP022
                End If
                If Not oEndorsement Is Nothing Then
                    oEndorsement = Nothing
                End If
            End If
        Else

            oPolicy.DwellingUnits.Item(0).HailResistiveRoofDisc = ""
            If oPolicy.Program = "HOA" Or oPolicy.Program = "HOB" Then
                If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                    RemoveEndorsementFactor(oPolicy, "HO145")
                Else
                    RemoveEndorsementFactorPAS(oPolicy, "HO145")
                End If
            ElseIf oPolicy.ProgramType.ToUpper = "DWELLING1" Then
                If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                    RemoveEndorsementFactor(oPolicy, "TDP022")
                Else
                    RemoveEndorsementFactorPAS(oPolicy, "TDP022")
                End If
            End If
        End If

        'if AllowAEC is true and the AEC covs are not on the policy then add them
        ' don't run this code for OLE
        If Not oPolicy.CallingSystem.Contains("OLE") And Not oPolicy.CallingSystem.ToUpper.Contains("UWC") And Not oPolicy.CallingSystem.Contains("PAS") Then
            If AllowAEC(oPolicy) Then
                AddCoverage(oPolicy, "AEC")
            Else
                RemoveCoverage(oPolicy, "AEC")
            End If
        End If

        'if AllowAEC is true and the AEC covs are not on the policy then add them
        If oPolicy.CallingSystem.Contains("OLE") Or oPolicy.CallingSystem.ToUpper.Contains("UWC") Or oPolicy.CallingSystem.Contains("PAS") Then
            If AllowAEC(oPolicy) Then
                AddCoverageOLE(oPolicy, "AEC")
            Else
                RemoveCoverageOLE(oPolicy, "AEC")
            End If
        End If

        'add mandatory endorsements
        If oPolicy.FormType = "HOA+" Then
            'add HO170-P and HO170-W
            oEndorsement = GetEndorsement(oPolicy, "HO170-P")
            If oEndorsement Is Nothing Then
                AddEndorsementFactor(oPolicy, "HO170-P")
            End If
            If Not oEndorsement Is Nothing Then
                oEndorsement = Nothing
            End If
            oEndorsement = GetEndorsement(oPolicy, "HO170-W")
            If oEndorsement Is Nothing Then
                AddEndorsementFactor(oPolicy, "HO170-W")
            End If
            If Not oEndorsement Is Nothing Then
                oEndorsement = Nothing
            End If
        ElseIf oPolicy.FormType = "TDP1+" Then
            'add TDP170-P and TDP170-W
            oEndorsement = GetEndorsement(oPolicy, "TDP170-P")
            If oEndorsement Is Nothing Then
                AddEndorsementFactor(oPolicy, "TDP170-P")
            End If
            If Not oEndorsement Is Nothing Then
                oEndorsement = Nothing
            End If
            oEndorsement = GetEndorsement(oPolicy, "TDP170-W")
            If oEndorsement Is Nothing Then
                AddEndorsementFactor(oPolicy, "TDP170-W")
            End If
            If Not oEndorsement Is Nothing Then
                oEndorsement = Nothing
            End If
        End If

        'Set Discount Flags
        For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
            If FactorOnPolicy(oPolicy, "PREMRED1") Then
                oPolicy.DwellingUnits.Item(0).PremRedCertificationCode = "PREMRED1"
                Exit For
            ElseIf FactorOnPolicy(oPolicy, "PREMRED2") Then
                oPolicy.DwellingUnits.Item(0).PremRedCertificationCode = "PREMRED2"
                Exit For
            ElseIf FactorOnPolicy(oPolicy, "PREMRED1_2") Then
                oPolicy.DwellingUnits.Item(0).PremRedCertificationCode = "PREMRED1_2"
                Exit For
            End If
        Next

        If bContainsFireAlarmDiscount Then
            oPolicy.DwellingUnits.Item(0).FireAlarmCreditID = "F_ALARM"
        Else
            oPolicy.DwellingUnits.Item(0).FireAlarmCreditID = ""
        End If

        If bContainsPoliceAlarmDiscount Then
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
        ' Cannot have both accredit builder and new purchase discount
        Dim dtNewRevisionDate As Date = CDate(GetPropertyStateInfoValue(oPolicy, oPolicy.ProgramCode, "ALLOW", "NEWPURCH_ABUILDER", "NEW"))
        Dim dtRenRevisionDate As Date = CDate(GetPropertyStateInfoValue(oPolicy, oPolicy.ProgramCode, "ALLOW", "NEWPURCH_ABUILDER", "RENEWAL"))
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

        ' Senior Discount
        ' Age 60 or Age 55 and retired
        Dim bHasSenior As Boolean = False
        If oPolicy.PolicyInsured.DOB <> CDate("1/1/1901") And oPolicy.PolicyInsured.DOB <> CDate("1/1/1900") Then
            If oPolicy.PolicyInsured.Age >= 60 Then
                AddPolicyFactor(oPolicy, "SENIOR")
                AddDiscount(oPolicy, "SENIOR")
                bHasSenior = True
            ElseIf oPolicy.PolicyInsured.Age >= 55 And oPolicy.PolicyInsured.Occupation.ToUpper.Trim = "RETIRED" Then
                AddPolicyFactor(oPolicy, "SENIOR")
                AddDiscount(oPolicy, "SENIOR")
                bHasSenior = True
            End If
        End If

        If Not bHasSenior Then
            RemoveDiscount(oPolicy, "SENIOR")
        End If

        'Non-Smoker Discount
        If HasDiscount(oPolicy, "NON_SMOKER") Then
            AddPolicyFactor(oPolicy, "NON_SMOKER")
        End If


        If Not bContainsRoofDiscount Then
            ' Auto Add HO145 Endorsement if roof type is metal
            If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                RemoveEndorsementFactor(oPolicy, "HO145")
            Else
                RemoveEndorsementFactorPAS(oPolicy, "HO145")
            End If
        End If

        If (oPolicy.DwellingUnits(0).RoofTypeCode = "Metal Roof" Or oPolicy.DwellingUnits(0).RoofTypeCode = "MTL") And oPolicy.Program <> "TDP1" Then
            AddEndorsementFactor(oPolicy, "HO145")
        End If


        ' Auto Add HO145 Endorsement if roof type is metal
        If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
            RemoveEndorsementFactor(oPolicy, "TDP022")
        Else
            RemoveEndorsementFactorPAS(oPolicy, "TDP022")
        End If
        If oPolicy.DwellingUnits(0).RoofTypeCode = "Metal Roof" And oPolicy.Program = "TDP1" Then
            AddEndorsementFactor(oPolicy, "TDP022")
        End If

        ' Removed 10/7/2010
        '' Auto Add HO145 Endorsement if roof type is metal
        'If CommonRulesFunctions.StateInfoContains("AUTOADD", "HO216", "", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode) Then
        '    RemoveEndorsementFactor(oPolicy, "HO216")
        '    If oPolicy.DwellingUnits(0).HomeAge >= 41 Then
        '        AddEndorsementFactor(oPolicy, "HO216")
        '    End If
        'End If
        ' for TDP3 Program
        If oPolicy.Program = "TDP3" Then
            oEndorsement = GetEndorsement(oPolicy, "TDP703")
            If oEndorsement Is Nothing Then
                AddEndorsementFactor(oPolicy, "TDP703")
            End If

            If Not oEndorsement Is Nothing Then
                oEndorsement = Nothing
            End If

            oEndorsement = GetEndorsement(oPolicy, "TDP005A")
            If oEndorsement Is Nothing Then
                AddEndorsementFactor(oPolicy, "TDP005A")
            End If
            If Not oEndorsement Is Nothing Then
                oEndorsement = Nothing
            End If
        End If


        Dim iNumAdditionalInsureds As Integer = 0
        For Each oAddInsured As clsEntityAddlInsured In oPolicy.AddlInsureds
            If Not oAddInsured.IsMarkedForDelete Then
                iNumAdditionalInsureds += 1
            End If
        Next

        If iNumAdditionalInsureds > 0 Then
            If oPolicy.Program = "TDP1" Or oPolicy.Program = "TDP3" Then
                ' TDP007
                Dim oEndorse As clsEndorsementFactor = Nothing
                oEndorse = GetEndorsement(oPolicy, "TDP007")

                If oEndorse Is Nothing Then
                    AddEndorsementFactor(oPolicy, "TDP007")
                    oEndorse = GetEndorsement(oPolicy, "TDP007")
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                Else
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                    oEndorse.IsMarkedForDelete = False
                End If
            Else
                ' HO301
                Dim oEndorse As clsEndorsementFactor = Nothing
                oEndorse = GetEndorsement(oPolicy, "HO301")

                If oEndorse Is Nothing Then
                    AddEndorsementFactor(oPolicy, "HO301")
                    oEndorse = GetEndorsement(oPolicy, "HO301")
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                Else
                    oEndorse.NumberOfEndorsements = iNumAdditionalInsureds
                    oEndorse.IsMarkedForDelete = False
                End If
            End If
        Else
            If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                RemoveEndorsementFactor(oPolicy, "TDP007")
                RemoveEndorsementFactor(oPolicy, "HO301")

            Else
                RemoveEndorsementFactorPAS(oPolicy, "TDP007")
                RemoveEndorsementFactorPAS(oPolicy, "HO301")

            End If
        End If

        ' HO216 only permitted on homes over 40 years old
        If oPolicy.DwellingUnits(0).HomeAge < 41 Then
            RemoveEndorsementFactor(oPolicy, "HO216")
        End If
    End Function

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
                If Not oEndorse.IsMarkedForDelete Then
                    If oEndorse.Type = "AEC_Plus" Or oEndorse.FactorCode = "HO170P" Then
                        bHasAECPlus = True
                    ElseIf oEndorse.Type = "WaterBackUp" Or oEndorse.FactorCode = "HO170W" Then
                        bHasWaterBackUp = True
                    End If
                End If
            Next
            If bHasAECPlus And bHasWaterBackUp Then
                bAllowAEC = True
            End If
        End If

        Return bAllowAEC

    End Function
#End Region

#Region "Coverage Functions"

    Public Sub AddCoverage(ByRef oPolicy As clsPolicyHomeOwner, ByVal sCovGroup As String)

        'load the coverages for the Program selected
        Dim oCoverage As clsHomeOwnerCoverage
        Dim oFactorBaseRateDataSet As DataSet = LoadFactorBaseRateTable(oPolicy)

        Dim DataRows() As DataRow
        DataRows = oFactorBaseRateDataSet.Tables(0).Select("Program='" & oPolicy.Program & "' AND Coverage ='" & sCovGroup & "'")

        For Each oRow As DataRow In DataRows
            If Not PolicyContainsCov(oPolicy, oRow("Coverage").ToString, oRow("Type").ToString) Then
                oCoverage = New clsHomeOwnerCoverage
                oCoverage.CovGroup = oRow("Coverage").ToString
                oCoverage.CovDesc = oRow("Description").ToString
                oCoverage.Type = oRow("Type").ToString
                oCoverage.UnitNum = 1
                oCoverage.IndexNum = GetMaxCoverageIndex(oPolicy) + 1
                oCoverage.IsNew = True
                oCoverage.IsMarkedForDelete = False
                oCoverage.IsModified = False
                oPolicy.DwellingUnits(0).Coverages.Add(oCoverage)
                If Not oCoverage Is Nothing Then
                    oCoverage = Nothing
                End If
            End If
        Next

    End Sub


    Private Function GetMaxCoverageIndex(ByVal oPolicy As clsPolicyHomeOwner) As Integer
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

    Public Sub RemoveCoverage(ByRef oPolicy As clsPolicyHomeOwner, ByVal sCovGroup As String)

        Dim oCov As clsHomeOwnerCoverage = Nothing

        For i As Integer = oPolicy.DwellingUnits.Item(0).Coverages.Count - 1 To 0 Step -1
            oCov = oPolicy.DwellingUnits.Item(0).Coverages.Item(i)
            If oCov.CovGroup.ToUpper = sCovGroup.ToUpper Then
                oPolicy.DwellingUnits(0).Coverages.RemoveAt(i)
            End If
        Next

    End Sub


    Public Sub AddCoverageOLE(ByRef oPolicy As clsPolicyHomeOwner, ByVal sCovGroup As String)

        'load the coverages for the Program selected
        Dim oCoverage As clsHomeOwnerCoverage
        Dim oFactorBaseRateDataSet As DataSet = LoadFactorBaseRateTable(oPolicy)

        Dim DataRows() As DataRow
        DataRows = oFactorBaseRateDataSet.Tables(0).Select("Program='" & oPolicy.Program & "' AND Coverage ='" & sCovGroup & "'")

        For Each oRow As DataRow In DataRows
            ' see if coverage is on policy, if it is and it is marked for delete, set ismarkedfordelete = false
            If Not PolicyContainsCovOLE(oPolicy, oRow("Coverage").ToString, oRow("Type").ToString) Then
                oCoverage = New clsHomeOwnerCoverage
                oCoverage.CovGroup = oRow("Coverage").ToString
                oCoverage.CovDesc = oRow("Description").ToString
                oCoverage.Type = oRow("Type").ToString
                oCoverage.UnitNum = 1
                oCoverage.IndexNum = GetMaxCoverageIndex(oPolicy) + 1
                oCoverage.IsNew = True
                oCoverage.IsMarkedForDelete = False
                oCoverage.IsModified = False
                oPolicy.DwellingUnits(0).Coverages.Add(oCoverage)
                If Not oCoverage Is Nothing Then
                    oCoverage = Nothing
                End If
            End If
        Next

    End Sub

    Public Sub RemoveCoverageOLE(ByRef oPolicy As clsPolicyHomeOwner, ByVal sCovGroup As String)

        Dim oCov As clsHomeOwnerCoverage = Nothing

        For i As Integer = oPolicy.DwellingUnits.Item(0).Coverages.Count - 1 To 0 Step -1
            oCov = oPolicy.DwellingUnits.Item(0).Coverages.Item(i)
            If oCov.CovGroup.ToUpper = sCovGroup.ToUpper Then
                oCov.IsMarkedForDelete = True
                oCov.IsNew = False
                oCov.IsModified = False
                'Exit For
            End If
        Next

    End Sub

    Public Function LoadFactorBaseRateTable(ByVal oPolicy As clsPolicyHomeOwner) As DataSet
        Dim sSql As String = ""

        Dim oConn As SqlConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Dim oDS As New DataSet

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, Coverage, Description, Type "
                sSql &= " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..FactorBaseRate with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, Coverage "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "FactorBaseRate")

                Return oDS

            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Function PolicyContainsCovOLE(ByVal oPolicy As clsPolicyHomeOwner, ByVal sCov As String, ByVal sCovType As String) As Boolean

        Dim bPolicyContainsCov As Boolean = False

        If sCovType = "N" Then
            'assume this coverage is ok because it is either LIA or MED, which are always required, or an endorsement
            bPolicyContainsCov = True
        Else
            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                If oCov.CovGroup.ToUpper = sCov.ToUpper Then
                    If oCov.Type.ToUpper = sCovType.ToUpper Then
                        If oCov.IsMarkedForDelete Then
                            oCov.IsMarkedForDelete = False
                        End If
                        bPolicyContainsCov = True
                        Exit For
                    End If
                End If
            Next
        End If

        Return bPolicyContainsCov

    End Function

    Public Function PolicyContainsCov(ByVal oPolicy As clsPolicyHomeOwner, ByVal sCov As String, ByVal sCovType As String) As Boolean

        Dim bPolicyContainsCov As Boolean = False

        If sCovType = "N" Then
            'assume this coverage is ok because it is either LIA or MED, which are always required, or an endorsement
            bPolicyContainsCov = True
        Else
            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                If Not oCov.IsMarkedForDelete Then
                    If oCov.CovGroup.ToUpper = sCov.ToUpper Then
                        If oCov.Type.ToUpper = sCovType.ToUpper Then
                            bPolicyContainsCov = True
                            Exit For
                        End If
                    End If
                End If
            Next
        End If

        Return bPolicyContainsCov

    End Function
#End Region

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
            If .DwellingUnits.Item(0).Ded3 > 0 Then
                Return "All Peril"
            ElseIf .DwellingUnits.Item(0).Ded1 > 0 Then
                Return "Wind/Hail"
            Else
                Return "Wind/Hail or All Peril"
            End If
        End With
    End Function

    Public Overrides Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyHomeOwner) As String
        Return String.Empty
    End Function

    Public Overrides Sub ResetTerritory(ByVal oPolicy As clsPolicyHomeOwner)

        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sTerritory As String = String.Empty
        Dim sRegion As String = String.Empty

        Dim oReader As SqlDataReader
        Try
            oConn.Open()
            Dim sSql As String = ""
            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Territory,Region "
                sSql = sSql & " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..CodeTerritoryDefinitions with(nolock)"
                sSql = sSql & " WHERE Zip = @Zip "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND County = @County "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@Zip", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).Zip.Trim
                cmd.Parameters.Add("@County", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).County.Trim

                oReader = cmd.ExecuteReader
                While oReader.Read()
                    sTerritory = oReader("Territory")
                    sRegion = oReader("Region")
                End While

            End Using
        Catch ex As Exception

        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        ' Do not reset the territory on endorsements
        If oPolicy.TransactionNum < 2 Then
            If Len(sTerritory) > 0 And Len(sRegion) > 0 Then
                oPolicy.DwellingUnits(0).Territory = sTerritory
                oPolicy.DwellingUnits(0).Region = sRegion
            End If
        End If
    End Sub

    Public Function CheckForNorthTexasRestrictedCounty(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Dim sRestrictionType As String = "NORTHTEXASRESTRICTION"
        Dim iHomeAge As Integer = LoadHomeAgeRestriction(oPolicy, True)

        Try
            If oPolicy.DwellingUnits(0).County <> "" Then
                If CheckForRestrictedCounty(oPolicy, sRestrictionType) Then
                    If oPolicy.Program <> "HOB" Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The HO-B form is the only eligible policy form for this county.", "NorthTexasRestr", "IER", oPolicy.Notes.Count))
                        Return True
                    Else
                        If oPolicy.DwellingUnits.Item(0).HomeAge > iHomeAge Then
                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county for homes > " & iHomeAge & " years old.", "NorthTexasRestr", "IER", oPolicy.Notes.Count))
                            Return True
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        End Try

        Return False

    End Function

    Public Function CheckFor10YearRestrictedCounty(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Const sRestrictionType As String = "10YEARHOMEAGERESTRICTION"
        Dim iHomeAge As Integer = LoadHomeAgeRestriction(oPolicy, False, 10)

        Try
            If oPolicy.DwellingUnits(0).County <> "" Then
                If CheckForRestrictedCountyByMaxAge(oPolicy, 10, sRestrictionType) Then
                    If oPolicy.DwellingUnits.Item(0).HomeAge > iHomeAge Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county for homes older than " & iHomeAge & " years old.", "10YearAgeRestr", "IER", oPolicy.Notes.Count))
                        Return True
                    End If
                End If
            End If

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        End Try

        Return False

    End Function

    Public Function CheckFor30YearRestrictedCounty(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Const sRestrictionType As String = "30YEARHOMEAGERESTRICTION"
        Dim iHomeAge As Integer = LoadHomeAgeRestriction(oPolicy, False, 30)

        Try
            If oPolicy.DwellingUnits(0).County <> "" Then
                If CheckForRestrictedCountyByMaxAge(oPolicy, 30, sRestrictionType) Then
                    If oPolicy.DwellingUnits.Item(0).HomeAge > iHomeAge Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county for homes older than " & iHomeAge & " years old.", "30YearAgeRestr", "IER", oPolicy.Notes.Count))
                        Return True
                    End If
                End If
            End If

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        End Try

        Return False

    End Function

    Public Function CheckForUWApprovalCounty(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Try
            If oPolicy.DwellingUnits(0).County <> "" Then
                If GetPropertyStateInfoValue(oPolicy, oPolicy.ProgramCode, "UW_APPROVAL", "COUNTY", oPolicy.DwellingUnits(0).County.Trim.ToUpper()).ToUpper = "TRUE" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Policies written in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county require Underwriter Approval.", "County", "UWW", oPolicy.Notes.Count))
                End If
            End If


        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        End Try

        Return False

    End Function
End Class
