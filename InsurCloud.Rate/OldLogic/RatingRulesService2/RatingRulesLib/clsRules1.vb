Imports System.Xml
Imports CorPolicy
Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic
Imports System.Configuration

Public Class clsRules1
    Inherits clsRules

    Public Sub New()



    End Sub


    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        oPolicy.Notes = RemoveNotes(oPolicy.Notes, "NEI")
        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""

        Try



            If Not IsDate(oPolicy.EffDate) Then
                bEnoughInfoToRate = False
                sMissing += "EffDate" & "-"
            End If
            If Not IsNumeric(oPolicy.DwellingUnits(0).YearOfConstruction) Then
                bEnoughInfoToRate = False
                sMissing += "YearOfConstruction" & "-"
            End If
            If oPolicy.DwellingUnits(0).Construction = "" Then
                bEnoughInfoToRate = False
                sMissing += "Construction" & "-"
            End If
            If oPolicy.DwellingUnits(0).ProtectionClass = "" Then
                bEnoughInfoToRate = False
                sMissing += "ProtectionClass" & "-"
            End If
            If Not IsNumeric(oPolicy.DwellingUnits(0).ProtectionClass) Then
                bEnoughInfoToRate = False
                sMissing += "ProtectionClass" & "-"
            End If
            If Not IsNumeric(oPolicy.DwellingUnits(0).Ded1) Then
                bEnoughInfoToRate = False
                sMissing += "Ded1" & "-"
            End If
            If Not IsNumeric(oPolicy.DwellingUnits(0).Ded2) Then
                bEnoughInfoToRate = False
                sMissing += "Ded2" & "-"
            End If

            If oPolicy.ProgramType.ToUpper <> "TENANT" Then
                If Not IsNumeric(oPolicy.DwellingUnits(0).DwellingAmt) Then
                    bEnoughInfoToRate = False
                    sMissing += "DwellingAmt" & "-"
                Else
                    If oPolicy.DwellingUnits(0).DwellingAmt = 0 Then
                        bEnoughInfoToRate = False
                        sMissing += "DwellingAmt" & "-"
                    End If
                End If
            End If

            If Not IsNumeric(oPolicy.DwellingUnits(0).ContentsAmt) Then
                bEnoughInfoToRate = False
                sMissing += "ContentsAmt" & "-"
            Else
                If oPolicy.DwellingUnits(0).ContentsAmt = 0 And Not oPolicy.ProgramType.Contains("DWELLING") Then
                    bEnoughInfoToRate = False
                    sMissing += "ContentsAmt" & "-"
                End If
            End If

            If oPolicy.ProgramType.ToUpper <> "TENANT" Then
                If Not IsNumeric(oPolicy.DwellingUnits(0).OtherStructureAmt) Then
                    bEnoughInfoToRate = False
                    sMissing += "OtherStructureAmt" & "-"
                Else
                    If oPolicy.DwellingUnits(0).OtherStructureAmt = 0 Then
                        bEnoughInfoToRate = False
                        sMissing += "OtherStructureAmt" & "-"
                    End If
                End If
            End If

            If oPolicy.DwellingUnits(0).BuildingTypeCode = "" Then
                bEnoughInfoToRate = False
                sMissing += "BuildingType" & "-"
            End If

            If oPolicy.DwellingUnits(0).Territory = "" Then
                bEnoughInfoToRate = False
                sMissing += "Territory" & "-"
            End If


            If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                If oPolicy.PolicyInsured.CreditStatus <> "SUCCESS" And oPolicy.PolicyInsured.CreditStatus <> "NOHIT" Then
                    bEnoughInfoToRate = False
                    sMissing += "CreditScore" & "-"
                End If
            End If

            If sMissing = "" Then
                Return True
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Needs: " & sMissing, "Not Enough Information To Rate", "NEI", oPolicy.Notes.Count))
                Return False
            End If

        Catch ex As Exception
            'log it
            oPolicy.Notes = (AddNote(oPolicy.Notes, ex.Message & "Needs: " & sMissing & " - " & ex.StackTrace, "Not Enough Information To Rate", "NEI", oPolicy.Notes.Count))
            Return False
        Finally

        End Try

    End Function

#Region "IER Functions"

    Public Overridable Sub CheckMinimumDeductibles(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Select Case .Program.ToUpper
                Case "DW10", "DW20", "DW30"
                    If .DwellingUnits(0).Ded1 < 1 Or .DwellingUnits(0).Ded2 < 1 Then
                        If (.DwellingUnits(0).Ded1 * (.DwellingUnits(0).DwellingAmt)) < 1000 Or (.DwellingUnits(0).Ded2 * (.DwellingUnits(0).DwellingAmt)) < 1000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: When choosing a percentage, the calculated deductible amount must be equal to or greater than $1,000 or a $1,000 flat deductible should be selected.", "DedMin", "IER", .Notes.Count))
                        End If
                    End If
                Case "HOA", "HOB"
                    If .DwellingUnits(0).Ded1 < 1 Or .DwellingUnits(0).Ded2 < 1 Then
                        If (.DwellingUnits(0).Ded1 * (.DwellingUnits(0).DwellingAmt)) < 1000 Or (.DwellingUnits(0).Ded2 * (.DwellingUnits(0).DwellingAmt)) < 250 Then

                            Dim bHasWindHailExclusionEndorsement As Boolean = False
                            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                                If (oEndorse.Type.ToUpper = "WINDHAILEXCLUSION" And Not oEndorse.IsMarkedForDelete) Or (oEndorse.FactorCode = "HO140" And Not oEndorse.IsMarkedForDelete) Then
                                    bHasWindHailExclusionEndorsement = True
                                    Exit For
                                End If
                            Next

                            If bHasWindHailExclusionEndorsement = False Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: When choosing a percentage, the calculated deductible amount must be equal to or greater than $1,000 or a $1,000 flat deductible should be selected.", "DedMin", "IER", .Notes.Count))
                            End If
                        End If
                    End If
                Case "HO20", "HO30"
                    Dim dDed1 As Decimal = .DwellingUnits(0).Ded1
                    Dim dDed2 As Decimal = .DwellingUnits(0).Ded2
                    Dim dDed3 As Decimal = .DwellingUnits(0).Ded3

                    ' Ded will be -1 if N/A is selected, only 2 deductibles are required for these programs
                    If dDed1 < 0 Then
                        dDed1 = 1
                    End If

                    If dDed2 < 0 Then
                        dDed2 = 1
                    End If

                    If dDed3 < 0 Then
                        dDed3 = 1
                    End If

                    If dDed1 < 1 Or dDed2 < 1 Or dDed3 < 1 Then
                        If (dDed1 * (.DwellingUnits(0).DwellingAmt)) < 1000 Or (dDed2 * (.DwellingUnits(0).DwellingAmt)) < 1000 Or (dDed3 * (.DwellingUnits(0).DwellingAmt)) < 1000 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: When choosing a percentage, the calculated deductible amount must be equal to or greater than $1,000 or a $1,000 flat deductible should be selected.", "DedMin", "IER", .Notes.Count))
                        End If
                    End If
                Case "TDP1"
                    If .DwellingUnits(0).Ded1 < 1 Then
                        If (.DwellingUnits(0).Ded1 * (.DwellingUnits(0).DwellingAmt)) < 1000 Then

                            Dim bHasWindHailExclusionEndorsement As Boolean = False
                            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                                If (oEndorse.Type.ToUpper = "WINDHAILEXCLUSION" And Not oEndorse.IsMarkedForDelete) Or (oEndorse.FactorCode = "HO140" And Not oEndorse.IsMarkedForDelete) Then
                                    bHasWindHailExclusionEndorsement = True
                                    Exit For
                                End If
                            Next

                            If bHasWindHailExclusionEndorsement = False Then
                                .Notes = (AddNote(.Notes, "Ineligible Risk: When choosing a percentage, the calculated deductible amount must be equal to or greater than $1,000 or a $1,000 flat deductible should be selected.", "DedMin", "IER", .Notes.Count))
                            End If
                        End If
                    End If
                Case "HO30T"
                    If .DwellingUnits(0).Ded3 < 1 Then
                        If (.DwellingUnits(0).Ded3 * (.DwellingUnits(0).ContentsAmt)) < 250 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: When choosing a percentage, the calculated deductible amount must be equal to or greater than $250 or a $250 flat deductible should be selected.", "DedMin", "IER", .Notes.Count))
                        End If
                    End If
            End Select
        End With
    End Sub


    Public Overridable Sub CheckIndividualMortgagee(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .IndividualMortgagee Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Home with individual as mortgagee.", "IMORT", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckBillingType(ByVal oPolicy As clsPolicyHomeOwner)
        Try
            Dim bInvalid As Boolean = False
            If oPolicy.BillMortgageeInd = "M1" Then
                bInvalid = True
                For Each oLH As clsEntityLienHolder In oPolicy.DwellingUnits(0).LienHolders
                    If oLH.IndexNum = "1" Then
                        If Not oLH.IsMarkedForDelete Then
                            bInvalid = False
                        End If
                    End If
                Next
            End If

            If oPolicy.BillMortgageeInd = "M2" Then
                bInvalid = True
                For Each oLH As clsEntityLienHolder In oPolicy.DwellingUnits(0).LienHolders
                    If oLH.IndexNum = "2" Then
                        If Not oLH.IsMarkedForDelete Then
                            bInvalid = False
                        End If
                    End If
                Next
            End If


            If bInvalid Then
                ' check to see if at least one active mortgagee
                For Each oLH As clsEntityLienHolder In oPolicy.DwellingUnits(0).LienHolders
                    If Not oLH.IsMarkedForDelete Then
                        bInvalid = False
                    End If
                Next

            End If
            If bInvalid Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Must change bill to party when mortgagee is removed.", "DelMortgagee", "IER", oPolicy.Notes.Count))
            End If
        Catch
        End Try
    End Sub

    Public Overridable Sub CheckBurglarBars(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).BurglarBars Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Burglar bars", "BurglarBars", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckTropicalStorm(ByVal oPolicy As clsPolicyHomeOwner)
        Try
            Dim weather As Weather = New Weather(oPolicy.DwellingUnits(0).Zip)
            If (Not weather.checkWeather()) Or (WeatherOverride(oPolicy)) Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot bind policies during a tropical storm warning", "TropicalStorm", "IER", oPolicy.Notes.Count))
            End If

            If oPolicy.DwellingUnits(0).Latitude = 0 Then
                oPolicy.DwellingUnits(0).Latitude = weather.Latitude
                oPolicy.DwellingUnits(0).Longitude = weather.Longitude
            End If
        Catch
        End Try
    End Sub
    Private Function WeatherOverride(ByVal opolicy As clsPolicyHomeOwner) As Boolean

        Dim bHasOverride As Boolean = False
        Dim sOverrideDate As String = GetProgramSetting("WeatherOverrideDate")

        If sOverrideDate = String.Empty Then
            bHasOverride = False
        Else
            If CDate(sOverrideDate) < Now() Then
                Return True
            End If
        End If

        Return bHasOverride
    End Function

    Public Overridable Sub CheckCitizensDisabledZip(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).Zip <> "" Then
                Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
                Dim disabled As Boolean


                Dim sSql As String = ""
                Using cmd As New SqlCommand(sSql, oConn)

                    sSql = " SELECT TOP 1 Disabled"
                    sSql = sSql & " FROM pgm" & .Product & .StateCode & "..CodeTerritoryDefinitions with(nolock)"
                    sSql = sSql & " WHERE Zip = @Zip "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND EffDate <= @RateDate "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = .RateDate
                    cmd.Parameters.Add("@Zip", SqlDbType.VarChar).Value = .DwellingUnits(0).Zip
                    oConn.Open()
                    disabled = cmd.ExecuteScalar
                    oConn.Close()
                End Using

                If disabled Then
                    If oPolicy.PriorCarrierName.ToUpper = "CITIZENS" Then
                        'they are good
                    Else
                        If Not oPolicy.PriorCarrierName = "" Then
                            'do the check to see if the agent has an override
                            sSql = ""
                            Using cmd As New SqlCommand(sSql, oConn)

                                sSql = " SELECT AllowedAgentRecordID"
                                sSql = sSql & " FROM pgm" & .Product & .StateCode & "..AgentZipOverride with(nolock)"
                                sSql = sSql & " WHERE Zip = @Zip "

                                'Execute the query
                                cmd.CommandText = sSql
                                cmd.Parameters.Add("@Zip", SqlDbType.VarChar).Value = .DwellingUnits(0).Zip
                                oConn.Open()
                                Dim reader As System.Data.SqlClient.SqlDataReader = cmd.ExecuteReader()

                                Dim AgentOverride As Boolean = False
                                Do While (reader.Read)
                                    If .Agency.AgencyID = reader.Item(0).ToString Then
                                        AgentOverride = True
                                        Exit Do
                                    End If
                                Loop

                                oConn.Close()

                                If Not AgentOverride Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: New business in this zip code is currently only available to prior Citizens customers.", "DisabledZip", "IER", .Notes.Count))
                                End If

                            End Using
                        End If
                    End If
                End If
            End If
        End With

    End Sub

    Public Overridable Sub CheckEffectiveDate(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .PolicyID = "" Then
                If .EffDate < Today Then
                    If Not CBool(ConfigurationManager.AppSettings("IsTest")) Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have an Effective Date in the past", "PastEffDate", "IER", .Notes.Count))
                    End If
                ElseIf .EffDate > DateAdd(DateInterval.Day, 60, Today) Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have an Effective Date more than 60 days in the future", "FutureEffDate", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckDogBreed(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType.Contains("DWELLING") Or .ProgramType.ToUpper = "DWELLING3" Then
                '1. owns Pit Bull, Doberman, Chow, RottWeiller, German Shepard : IER
                Select Case .DwellingUnits.Item(0).BreedCode
                    Case "Chow", "Doberman Pinscher", "German Shepard", "Pit Bull", "Rottweiler", "American Staffordshire Terrier"
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Dangerous pet breed", "Breed", "IER", .Notes.Count))
                    Case "Other"
                        If .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("DOBERMAN") Or .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("PINSCHER") Or .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("SHEPARD") Or
                                .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("PIT") Or .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("ROTTWEILER") Or .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("STAFFORDSHIRE") Or
                                .DwellingUnits.Item(0).OtherBreed.ToUpper.Contains("CHOW") Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Dangerous pet breed", "Breed", "IER", .Notes.Count))
                        End If
                End Select
            End If
        End With
    End Sub

    Public Overridable Sub CheckDogBiting(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType.Contains("DWELLING") Or .ProgramType.ToUpper = "DWELLING3" Then
                '2. Answers "yes" to biting history : IER
                If .DwellingUnits.Item(0).PetBitingHistoryDesc <> "" Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Pet with a Biting History", "BitingHist", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckPoolFenced(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).SwimmingPool Then
                '4. Answers "no" to fenced pool : IER
                If .DwellingUnits.Item(0).PoolFenced = False Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Pool must be fenced", "FencedPool", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckPoolSlide(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).SwimmingPool Then
                '5. Answers "yes" to pool slide: IER
                If .DwellingUnits.Item(0).PoolSlide = True Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Pool cannot have a slide", "PoolSlide", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckPoolDivingBoard(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).PoolDivingBoard Then
                '5. Answers "yes" to pool diving board: IER
                If .DwellingUnits.Item(0).PoolDivingBoard = True Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Swimming Pool with a Diving Board", "PoolDivingBoard", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckTrampolineFence(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType.Contains("DWELLING") Or .ProgramType.ToUpper = "DWELLING3" Then
                If .DwellingUnits.Item(0).Trampoline Then
                    'Answers "no" to fenced tramp : IER
                    If .DwellingUnits.Item(0).TrampolineFenced = False Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Trampoline must be fenced", "FencedTramp", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckTrampolineSafteyRing(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If Not .ProgramType.Contains("DWELLING") Or .ProgramType.ToUpper = "DWELLING3" Then
                If .DwellingUnits.Item(0).Trampoline Then
                    'Answers "no" to tramp ring: IER
                    If .DwellingUnits.Item(0).TrampolineSafety = False Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Trampoline must have safety ring", "TrampRing", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    '2 claims in the last five years (includes both weather and non-weather)  
    Public Overridable Sub CheckClaimsUWW(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim iNumLoss As Integer = 0

            With oPolicy.DwellingUnits(0)
                For Each oClaim As clsBaseClaim In .Claims
                    oClaim.Chargeable = True
                    If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) < oPolicy.EffDate Then
                        oClaim.Chargeable = False
                    End If

                    If oClaim.ClaimAmt = 0 Then
                        oClaim.Chargeable = False
                    End If

                    If oClaim.Chargeable Then
                        iNumLoss += 1
                    End If
                Next
            End With

            If iNumLoss = 2 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Multiple claims require Underwriting Approval.", "UWClaims", "IER", .Notes.Count))
            End If
        End With
    End Sub

    '3 or more claims in the last five years (includes both weather and non-weather)  – “Ineligible Risk – Due to the number of claims this risk is unacceptable.”
    Public Overridable Sub CheckClaimsCount(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim iNumLoss As Integer = 0

            With oPolicy.DwellingUnits(0)
                For Each oClaim As clsBaseClaim In .Claims
                    oClaim.Chargeable = True
                    If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) < oPolicy.EffDate Then
                        oClaim.Chargeable = False
                    End If

                    If oClaim.ClaimAmt = 0 Then
                        oClaim.Chargeable = False
                    End If

                    If oClaim.Chargeable Then
                        iNumLoss += 1
                    End If
                Next
            End With

            If iNumLoss > 2 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Due to the number of claims this risk is unacceptable.", "ClaimsCnt", "IER", .Notes.Count))
            End If
        End With
    End Sub

    'Non-Weather Claims above $20,000 in the last five years (includes claims of the insured or property) – “Ineligible Risk – Non-Weather claims greater than $20,000 require Underwriting Approval.”
    Public Overridable Sub CheckClaimsAmt(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim iAmount As Decimal = 0.0

            With oPolicy.DwellingUnits(0)
                For Each oClaim As clsBaseClaim In .Claims
                    oClaim.Chargeable = True
                    If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) < oPolicy.EffDate Then
                        oClaim.Chargeable = False
                    End If
                    If oClaim.ClaimAmt = 0 Then
                        oClaim.Chargeable = False
                    End If
                    If oClaim.ClaimTypeIndicator.Trim <> "W" Then
                        If oClaim.Chargeable Then
                            iAmount += oClaim.ClaimAmt
                        End If
                    End If
                Next
            End With

            If iAmount > 20000 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Non-Weather claims greater than $20,000 require Underwriting Approval.", "ClaimsAmt", "IER", .Notes.Count))
            End If
        End With
    End Sub



    Public Overridable Sub CheckOpenClaims(ByVal oPolicy As clsPolicyHomeOwner)
        Dim bHasOpenClaim As Boolean = False
        For Each oClaim As clsBaseClaim In oPolicy.DwellingUnits.Item(0).Claims
            If oClaim.ClaimStatus.Trim.ToUpper = "OPEN" Then
                bHasOpenClaim = True
                Exit For
            End If
        Next
        If bHasOpenClaim Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Open claims are unacceptable.", "OpenClaim", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckVacantHome(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '6. Answers "yes" to vacant dwelling : IER
            If .DwellingUnits.Item(0).VacantDwelling = True Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Vacant home", "VacantHome", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckBankruptcy(ByVal oPolicy As clsPolicyHomeOwner)
        'REMOVED - Cannot restrict policies based on this (DRS - 10/06/09)
        With oPolicy
            '10. Answers yes to bankruptcy filed in past 3 years
            If .PolicyInsured.FiledBankruptcy Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Filed for bankruptcy in last 3 years", "Bankruptcy", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckRoofLayers(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '12. Answers with '2' or higher to number of roof layers
            If .DwellingUnits.Item(0).RoofLayerCode <> "" Then
                If .DwellingUnits.Item(0).RoofLayerCode > 1 Then
                    If .DwellingUnits.Item(0).RoofTypeCode = "Metal Roof" Then
                        'we don't care about the multiple roof layers on a metal roof because they are generally laid on top of the 
                        'preexisting shingled roof so agents are answering yes to multiple roof layers
                    Else
                        .Notes = (AddNote(.Notes, "Ineligible Risk: More than one roof layer", "RoofLayer", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckAluminumSiding(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '15. Aluminum siding or Asbestos Construction is chosen
            If .DwellingUnits.Item(0).SidingCode = "Aluminum" Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have aluminum siding", "AluminumSiding", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckAsbestos(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).Construction = "Asbestos" Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have asbestos construction", "AsbestosConstruction", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckMetalBuilding(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If oPolicy.DwellingUnits(0).Construction = "Frame" And oPolicy.DwellingUnits(0).SidingCode = "Steel" Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Buildings of all metal construction are not acceptable.", "MetalConstr", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckMaxDwellingAmount(ByVal oPolicy As clsPolicyHomeOwner)
        'Max Amounts
        Dim lMaxAmount As Long = 0
        ' * Dwelling
        lMaxAmount = GetMaxAmount(oPolicy.StateCode, oPolicy.Product, oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, "D")
        With oPolicy
            If .DwellingUnits.Item(0).DwellingAmt > lMaxAmount Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: The Maximum Amount for Dwelling is " & lMaxAmount, "MaxDwellingAmt", "IER", .Notes.Count))
            End If
        End With

    End Sub

    Public Overridable Sub CheckMaxContentAmount(ByVal oPolicy As clsPolicyHomeOwner)
        'Max Amounts
        Dim lMaxAmount As Long = 0

        ' * Contents
        lMaxAmount = GetMaxAmount(oPolicy.StateCode, oPolicy.Product, oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, "C")
        With oPolicy
            If .DwellingUnits.Item(0).ContentsAmt > lMaxAmount Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: The Maximum Amount for Contents is " & lMaxAmount, "MaxContentsAmt", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckMaxStructureAmount(ByVal oPolicy As clsPolicyHomeOwner)
        Dim lMaxAmount As Long = 0

        'Max Other Structure Amt
        lMaxAmount = GetMaxOtherStructureAmt(oPolicy.StateCode, oPolicy.Product, oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode)

        With oPolicy
            If .DwellingUnits.Item(0).OtherStructureAmt > lMaxAmount Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: The Maximum Amount for Other Structures is " & lMaxAmount, "MaxOtherStructureAmt", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckTenantDwellingOnly(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType <> "" Then
                If Not .ProgramType.ToUpper.Contains("DWELLING") Then
                    If .DwellingUnits.Item(0).OwnerOccupiedFlag <> 1 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Tenant occupied dwellings are acceptable only on Dwelling policies", "TenantDwellingOnly", "IER", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckTenantMortgageeBill(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType <> "" Then
                If .ProgramType.ToUpper = "TENANT" Then
                    If Not .BillMortgageeInd Is Nothing Then
                        If .BillMortgageeInd.ToUpper = "Y" Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Unable to bill mortgagee on this type of policy", "TenantMortgageeBill", "IER", .Notes.Count))
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckReplacementCost(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType <> "" Then
                If .ProgramType.ToUpper <> "TENANT" Then
                    If .Status = "2" Or .Status = "3" Or .Status = "4" Then
                        If .DwellingUnits.Item(0).ReplacementCashAmt = 0 Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Replacement Cost must be greater than $0", "InvalidReplacementCost", "IER", .Notes.Count))
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckMaxAddlOtherStructure(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).OtherStructureAddnAmt > 50000 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have more than $50,000 in additional other structures coverage", "MaxAddlOtherStructure", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckExtendedCoverage(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .ProgramType.ToUpper = "DWELLING1" Then
                'if we have VMM, then we need to have EC
                Dim bHasVMM As Boolean = False
                Dim bHasEC As Boolean = False
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                    If oCov.CovGroup.ToString.ToUpper = "VMM" And Not oCov.IsMarkedForDelete Then
                        bHasVMM = True
                    ElseIf oCov.CovGroup.ToString.ToUpper = "EC" And Not oCov.IsMarkedForDelete Then
                        bHasEC = True
                    End If
                Next
                If bHasVMM And Not bHasEC Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Can not purchase Vandalism and Malicious Mischief without Extended Coverage Perils.", "NeedsEC", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckNonOwnerDwellingLiability(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            ' Check to see if Dwelling Liability Coverage is added to a non-owner occupied policy
            If .DwellingUnits.Item(0).OwnerOccupiedFlag <> 1 Then
                For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                    If oEndorse.Type.ToUpper.Contains("DWELLING") And oEndorse.Type.ToUpper.Contains("LIABILITY") And Not oEndorse.IsMarkedForDelete Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Can not purchase Dwelling Liability Endorsement as it is not applicable to tenant occupied dwellings.", "NONOWNERDWLLIA", "IER", .Notes.Count))
                    End If
                Next
            End If
        End With
    End Sub

    Public Overridable Sub CheckNonWeatherLosses(ByVal oPolicy As clsPolicyHomeOwner)

        With oPolicy
            '13. Loss report shows more than 2 non-weather related losses in past 3 years
            Dim iNumNonWeatherLoss As Integer = 0
            For Each oClaim As clsBaseClaim In .DwellingUnits.Item(0).Claims
                If oClaim.ClaimTypeIndicator = "O" Then 'Non-Weather
                    If DateAdd(DateInterval.Month, 36, oClaim.ClaimDate) >= .RateDate Then
                        iNumNonWeatherLoss += 1
                    End If
                End If
            Next
            If iNumNonWeatherLoss > 2 Then
                'add UWW note OLD NOTE: Underwriting Approval Needed: Loss report shows more than 2 non-weather related losses in past 3 years
                .Notes = (AddNote(.Notes, "Ineligible Risk: Multiple non-weather claims requires Underwriting Approval", "NonWeatherLoss", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckRoutingNumbers(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            For Each oAccount As clsBaseAccount In .Accounts
                If Len(oAccount.RoutingNum) > 0 Then
                    If Len(oAccount.RoutingNum) <> 9 Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Routing Number must be 9 digits", "InvalidRouting", "IER", .Notes.Count))
                    End If
                End If
            Next
        End With
    End Sub
#End Region
    Public Overridable Sub CheckInsuredAddress(ByVal oPolicy As clsPolicyHomeOwner)
        Dim isValid As Boolean = True
        With oPolicy
            If Not .PolicyInsured.Address1.Trim.Length > 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Address1 cannot be blank", "InsuredAddress1", "IER", .Notes.Count))
                isValid = False
            End If
            If Not .PolicyInsured.City.Trim.Length > 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Insured City cannot be blank", "InsuredCity", "IER", .Notes.Count))
                isValid = False
            End If
            If Not .PolicyInsured.State.Trim.Length > 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Insured State cannot be blank", "InsuredState", "IER", .Notes.Count))
                isValid = False
            End If
            If .PolicyInsured.MailingAddrDiff Then
                If Not .PolicyInsured.MailingAddress1.Trim.Length > 1 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Mailing Address1 cannot be blank", "InsuredMailingAddress1", "IER", .Notes.Count))
                    isValid = False
                End If
                If Not .PolicyInsured.MailingCity.Trim.Length > 1 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Mailing City cannot be blank", "InsuredMailingCity", "IER", .Notes.Count))
                    isValid = False
                End If
                If Not .PolicyInsured.MailingState.Trim.Length > 1 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Mailing State cannot be blank", "InsuredMailingState", "IER", .Notes.Count))
                    isValid = False
                End If
            End If
        End With
    End Sub

    Public Overridable Function CheckAccreditedBuilderHomeAge(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim bHasAccreditedBuilder As Boolean = False

        With oPolicy
            If HasDiscount(oPolicy, "A_BUILDER") Then
                bHasAccreditedBuilder = True
            End If

            If bHasAccreditedBuilder And Not (.DwellingUnits(0).YearOfConstruction = Year(oPolicy.OrigTermEffDate)) Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: The Accredited Builder discount is only applicable to homes that are 0 years old", "AccBuilder", "IER", .Notes.Count))
            End If


        End With
    End Function


    Public Overridable Sub CheckDwellingAddress(ByVal oPolicy As clsPolicyHomeOwner)
        Dim isValid As Boolean = True
        With oPolicy
            If Not .DwellingUnits(0).Address1.Trim.Length > 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Dwelling Address1 cannot be blank", "DwellingAddress1", "IER", .Notes.Count))
                isValid = False
            End If
            If Not .DwellingUnits(0).City.Trim.Length > 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Dwelling City cannot be blank", "DwellingCity", "IER", .Notes.Count))
                isValid = False
            End If
            If Not .DwellingUnits(0).State.Trim.Length > 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Dwelling State cannot be blank", "DwellingState", "IER", .Notes.Count))
                isValid = False
            End If
        End With
    End Sub

    Public Sub CheckIncreasedReplacementCostCoverage(ByVal oPolicy As clsPolicyHomeOwner)

        With oPolicy
            For Each oEndorse As clsEndorsementFactor In .EndorsementFactors
                'does the HO903 endorsement exist?
                If oEndorse.FactorCode.ToUpper = "HO903" And Not oEndorse.IsMarkedForDelete Then

                    'if so, get the Increased Replacement Cost Coverage Amount from the stateInfo table
                    Dim sMaxCoverageAmt As String = CorFunctions.CommonFunctions.GetStateInfoValue(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.Program, "INCR_REPLACE", "MAX_DWELLING", String.Empty)

                    'compare amounts of Increased Replacement Cost Coverage amount and Dwelling amount from the policy
                    'if Dwelling amt is greater, add the IER to .Notes
                    If .DwellingUnits.Item(0).DwellingAmt > Convert.ToInt64(sMaxCoverageAmt) Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have HOE903 Increased Replacement Cost Coverage with a dwelling coverage over $" & String.Format("{0:0,0}", sMaxCoverageAmt) & ".", "MaxIncReplaceAmt", "IER", .Notes.Count))
                        Exit For
                    End If

                End If
            Next
        End With

    End Sub

    Public Sub CheckFlatRoof(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            'is it a Flat Roof?
            If .DwellingUnits.Item(0).RoofShapeCode = "Flat" Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Flat Roof.", "RoofShape", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckPayPlan(ByRef oPolicy As clsPolicyHomeOwner)

        If oPolicy.PayPlanCode = "210" Or oPolicy.PayPlanCode = "201" Then
            If oPolicy.IsEFT = False Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: EFT must be selected for this payplan.", "IneligiblePayPlan", "IER", oPolicy.Notes.Count))
            End If
        End If

    End Sub

    Public Sub CheckCreditScores(ByVal oPolicy As clsPolicyHomeOwner)

        Dim bIneligibleRisk As Boolean = False

        If (Not String.IsNullOrWhiteSpace(oPolicy.PolicyInsured.CreditScore) AndAlso (oPolicy.PolicyInsured.CreditScore <= 550 Or oPolicy.PolicyInsured.CreditScore >= 998)) OrElse oPolicy.PolicyInsured.CreditStatus = "NOHIT" Then
            bIneligibleRisk = True
        End If

        If bIneligibleRisk Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Photos of the interior and exterior of this risk must be submitted to the Underwriting Department prior to approval.", "CREDITSCORES", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Sub CheckForUnrepairedDamageClaim(ByVal oPolicy As clsPolicyHomeOwner)

        If oPolicy.DwellingUnits.Item(0).InsuranceClaim And Not oPolicy.DwellingUnits.Item(0).ClaimDamageRepaired Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Unrepaired damage related to a previous insurance claim.", "UnrepairedDamageClaim", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Sub CheckForFelonyConviction(ByVal oPolicy As clsPolicyHomeOwner)

        If oPolicy.DwellingUnits.Item(0).FelonyConviction Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Applicant has been convicted of a felony in the past 5 years, or applicant has been convicted of arson or fraud.", "FelonyArsonFraudConviction", "IER", oPolicy.Notes.Count))
        End If

    End Sub


#Region "UWW Functions"
    Public Overridable Sub CheckWoodBurningStove(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '3. Answers "yes" to wood burning stove : UWW
            If .DwellingUnits.Item(0).WoodBurningStoveInstalledBy <> "" Then
                .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Wood burning stove.", "WoodStove", "UWW", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckBusinessOnPremises(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '8. Answers yes to business on premises
            If .DwellingUnits.Item(0).BusinessOnPremisesDesc <> "" Then
                .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Business on Premises", "BusinessOnPremises", "UWW", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckCancelled(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '11. Answers yes to cancelled in last 3 years
            If .CnxLastThreeYrsDesc <> "" Then
                .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Cancelled in last 3 years", "Cancelled", "UWW", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckClaims(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            '3. Answers "yes" to wood burning stove : UWW
            If .DwellingUnits.Item(0).WoodBurningStoveInstalledBy <> "" Then
                .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Wood burning stove.", "WoodStove", "UWW", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CleanTheftClaimsByMatchType(ByVal oPolicy As clsPolicyHomeOwner)
        'IMPROVE 2069
        'Only use theft claims that match the insured, not the property address.
        Dim claimsToRemove As List(Of clsBaseClaim) = New List(Of clsBaseClaim)

        With oPolicy
            For Each oClaim As clsBaseClaim In .DwellingUnits.Item(0).Claims
                If oClaim.ClaimType = "22" OrElse oClaim.ClaimType = "23" OrElse oClaim.ClaimDesc.Contains("THEFT") Then    'THEFT
                    If oClaim.ClaimComment.StartsWith("INSURED") OrElse oClaim.MatchType.StartsWith("INSURED") Then
                        'keep
                    Else
                        'remove
                        claimsToRemove.Add(oClaim)
                    End If
                End If
            Next

            For Each claimToDel In claimsToRemove
                .DwellingUnits.Item(0).Claims.Remove(claimToDel)
            Next

        End With

    End Sub

    Public Overridable Sub CheckCitizensOverride(ByVal oPolicy As clsPolicyHomeOwner)
        'REMOVED because this is a LA specific requirement (DRS 10/6/09)
        With oPolicy
            If .DwellingUnits.Item(0).Zip <> "" Then
                Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
                Dim disabled As Boolean

                Dim sSql As String = ""
                Using cmd As New SqlCommand(sSql, oConn)

                    sSql = " SELECT TOP 1 Disabled"
                    sSql = sSql & " FROM pgm" & .Product & .StateCode & "..CodeTerritoryDefinitions with(nolock)"
                    sSql = sSql & " WHERE Zip = @Zip "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND EffDate <= @RateDate "

                    'Execute the query
                    cmd.CommandText = sSql
                    cmd.Parameters.Add("@Zip", SqlDbType.VarChar).Value = .DwellingUnits(0).Zip
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = .RateDate

                    oConn.Open()
                    disabled = cmd.ExecuteScalar
                    oConn.Close()

                End Using
                If disabled Then
                    If Not oPolicy.PriorCarrierName.ToUpper = "CITIZENS" Then
                        .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Sorry, we are not currently accepting new business in Zip: " & .DwellingUnits(0).Zip, "CitizensOverride", "UWW", .Notes.Count))
                    End If
                End If
            End If
        End With
    End Sub
    Public Overridable Sub CheckProtectionClass(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If .DwellingUnits.Item(0).ProtectionClass = "9" Or .DwellingUnits.Item(0).ProtectionClass = "10" Then
                .Notes = (AddNote(.Notes, "Underwriting Approval Needed: Home is in protection class 9 or 10.", "ProtectionClass", "UWW", .Notes.Count))
            End If
        End With
    End Sub

#End Region

#Region "WRN Functions"
    Public Overridable Sub CheckHomeAge(ByVal oPolicy As clsPolicyHomeOwner)
        ' Removed 10/8/2010 per Julia all homes in TX now have inspection reports ordered
        '' ''With oPolicy
        '' ''    '20. Inspections automatically ordered on homes over 4 years old
        '' ''    If .ProgramType.ToUpper <> "TENANT" Then
        '' ''        If .DwellingUnits.Item(0).YearOfConstruction > 1 Then
        '' ''            If .DwellingUnits.Item(0).HomeAge > 4 Then
        '' ''                .Notes = (AddNote(.Notes, "Warning: Home inspections are ordered on homes older than 4 years", "HomeAgeInspection", "WRN", .Notes.Count))
        '' ''            End If
        '' ''        End If
        '' ''    End If
        '' ''End With
    End Sub

    Public Overridable Sub CheckExistingRenewal(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy

            ' if within 45 days of expiration, ask if this should be on the new term

            If DateAdd(DateInterval.Day, -45, .ExpDate) < Now() Then
                .Notes = (AddNote(.Notes, "Warning: Policy is within 45 days of expiration. Change should also be made on the new term.", "EXSTRENEWAL", "WRN", .Notes.Count))
            End If


        End With
    End Sub

    Public Overridable Sub CheckWaterDamage(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            Dim iNumWaterLoss As Integer = 0
            For Each oClaim As clsBaseClaim In .DwellingUnits.Item(0).Claims
                If oClaim.ClaimTypeIndicator = "N" Then 'Water
                    iNumWaterLoss += 1
                End If
            Next
            If iNumWaterLoss > 0 Then
                .Notes = (AddNote(.Notes, "Warning: Any home that has sustained prior flood and/or other water damage is required to provide proof that the damage has been properly repaired and remediated", "PriorWaterLoss", "WRN", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckActualRateDate(ByVal oPolicy As clsPolicyHomeOwner)
        With oPolicy
            If oPolicy.ActualRateDate <> Date.MinValue Then
                If oPolicy.ActualRateDate.AddDays(60) < Now() Then
                    oPolicy.ActualRateDate = Now()
                    .Notes = (AddNote(.Notes, "Information Updated: Quote is over 60 days old, prior rate is no longer valid and has been updated.", "ActualRateDate", "RPT", .Notes.Count))
                End If
            End If
        End With


    End Sub

#End Region

#Region "Helper Functions"
    Public Overridable Sub AddDiscount(ByVal oPolicy As clsPolicyHomeOwner, ByVal sFactorCode As String)
        If Not HasDiscount(oPolicy, sFactorCode) Then
            Dim oNewDiscount As New clsHomeOwnerDiscount
            oNewDiscount.FactorCategory = sFactorCode
            oNewDiscount.FactorCode = sFactorCode
            oNewDiscount.Param1 = "TRUE"
            oNewDiscount.Param2 = ""
            oNewDiscount.Param3 = ""
            oNewDiscount.IsNew = True
            oNewDiscount.IsMarkedForDelete = False
            oNewDiscount.FactorType = "POLICY"
            oNewDiscount.UnitNumber = 1

            oPolicy.Discounts.Add(oNewDiscount)
        End If
    End Sub

    Public Overrides Sub ExpireWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal programs As List(Of ProgramSetting))

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) And programs.Any()) Then

            Dim allProgramSetting = GetAllProgramSetting(productCode, stateCode, "WeatherOverrideDate")
            Dim activeProgramSetting = allProgramSetting.Where(Function(x) x.ExpDate > DateTime.Now)

            'Expire All
            For Each program As ProgramSetting In activeProgramSetting
                Dim expireDate = program.EffDate
                program.ExpDate = expireDate
                UpdateProgramSetting(productCode, stateCode, program)
            Next
        End If

    End Sub

    Public Overrides Sub SetWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal startDate As DateTime, _
                                            ByVal programs As List(Of Integer))

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) And programs.Any()) Then

            Dim CRM As New MarketingCRMService.InsurCloudAMSServiceSoapClient
            Dim allPrograms = CRM.GetActivePrograms(productCode, stateCode).ToList()
            Dim allProgramSetting = GetAllProgramSetting(productCode, stateCode, "WeatherOverrideDate")
            Dim activeProgramSetting = allProgramSetting.Where(Function(x) x.ExpDate > DateTime.Now)

            Dim activeProgramIDs = (From p In allPrograms
                                    Join p1 In activeProgramSetting On
                                    p.ProgramCode Equals p1.Program
                                    Select p.ProgramID)

            Dim netPrograms = programs.Union(activeProgramIDs)

            'Set Individual, Expire All Program, Set Individual Selected w/ Restriction StartTime
            If (allPrograms.Select(Function(x) x.ProgramID).Except(netPrograms).Any()) Then

                'Expire Active HOM
                If (allProgramSetting.Any(Function(x) x.Program = "HOM")) Then
                    Dim expireProgramSetting = ProgramSetting.GetNew(startDate, "HOM")
                    expireProgramSetting.ExpDate = expireProgramSetting.EffDate
                    UpdateProgramSetting(productCode, stateCode, expireProgramSetting)
                End If

                For Each programID As Integer In programs
                    Dim existingProgram = allPrograms.SingleOrDefault(Function(x) x.ProgramID = programID)
                    If (allProgramSetting.Any(Function(x) x.Program = existingProgram.ProgramCode)) Then
                        UpdateProgramSetting(productCode, stateCode, ProgramSetting.GetNew(startDate, existingProgram.ProgramCode))
                    Else
                        InsertProgramSetting(productCode, stateCode, ProgramSetting.GetNew(startDate, existingProgram.ProgramCode))
                    End If
                Next
            Else
                'Set All Override - Expire All Existing, Set Override w/ Restricion StartTime w/ HOM Program
                'Expire Active
                For Each activeProgram As ProgramSetting In activeProgramSetting.Where(Function(x) x.Program <> "HOM")
                    activeProgram.ExpDate = activeProgram.EffDate
                    UpdateProgramSetting(productCode, stateCode, activeProgram)
                Next

                If (allProgramSetting.Any(Function(x) x.Program = "HOM")) Then
                    UpdateProgramSetting(productCode, stateCode, ProgramSetting.GetNew(startDate, "HOM"))
                Else
                    InsertProgramSetting(productCode, stateCode, ProgramSetting.GetNew(startDate, "HOM"))
                End If
            End If
        End If
    End Sub


    Public Overridable Sub RemoveDiscount(ByVal oPolicy As clsPolicyHomeOwner, ByVal sFactorCode As String)
        For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
            If oDiscount.FactorCode.ToUpper = sFactorCode.ToUpper Then
                oDiscount.IsMarkedForDelete = True
                oDiscount.IsNew = False
                oDiscount.IsModified = False
            End If
        Next
    End Sub


    Public Overridable Function HasDiscount(ByVal oPolicy As clsPolicyHomeOwner, ByVal sFactorCode As String) As Boolean

        ' new method, check policy discounts
        For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
            If Not oDiscount.IsMarkedForDelete Then
                If oDiscount.FactorCode.ToUpper = sFactorCode.ToUpper Then
                    Return True
                End If
            End If
        Next


        ' 4/12/2011 Cannot look at notes anymore, because deleting a discount in OLE will not delete the note
        ' this only applies to homeowner's right now.
        '' old method, check policy notes
        'Dim oNote As clsBaseNote
        'oNote = GetNote(oPolicy, sFactorCode.ToUpper)
        'If Not oNote Is Nothing Then
        '	Return True
        'End If

        Return False
    End Function


    Public Overridable Function GetDiscount(ByVal oPolicy As clsPolicyHomeOwner, ByVal sFactorCode As String) As clsHomeOwnerDiscount
        ' new method, check policy discounts
        For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
            If Not oDiscount.IsMarkedForDelete Then
                If oDiscount.FactorCode.ToUpper = sFactorCode.ToUpper Then
                    Return oDiscount
                End If
            End If
        Next
        Return Nothing
    End Function


    'Public Overridable Function AddPASPolicyFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

    '	'MULTILINE
    '	Dim bHasAuto As Boolean = False
    '	Dim bHasFlood As Boolean = False
    '	Dim bHasBoth As Boolean = False

    '	If HasDiscount(oPolicy, "MULTILINE1") Then
    '		bHasAuto = True
    '		AddPolicyFactor(oPolicy, "MULTILINE1")
    '		Dim oDiscount As clsHomeOwnerDiscount = GetDiscount(oPolicy, "MULTILINE1")
    '		If Not oDiscount Is Nothing Then
    '			oPolicy.CompanionPAPolicyID = oDiscount.Param2
    '		End If
    '		If oPolicy.CompanionPAPolicyID = "" Then
    '			oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Confirm Auto Companion Policy ID #" & oPolicy.CompanionPAPolicyID, "MULTILINE1", "UWW", oPolicy.Notes.Count))
    '		End If

    '	Else
    '		bHasAuto = False
    '		RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE1")
    '	End If

    '	If HasDiscount(oPolicy, "MULTILINE2") Then
    '		bHasFlood = True
    '		AddPolicyFactor(oPolicy, "MULTILINE2")
    '		Dim oDiscount As clsHomeOwnerDiscount = GetDiscount(oPolicy, "MULTILINE2")
    '		If Not oDiscount Is Nothing Then
    '			oPolicy.CompanionFloodPolicyID = oDiscount.Param2
    '		End If
    '		If oPolicy.CompanionFloodPolicyID = "" Then
    '			oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Confirm Flood Companion Policy ID #" & oPolicy.CompanionFloodPolicyID, "MULTILINE2", "UWW", oPolicy.Notes.Count))
    '		End If
    '	Else
    '		bHasFlood = False
    '		RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE2")
    '	End If

    '	'flood and auto
    '	If bHasAuto And bHasFlood Then
    '		bHasBoth = True
    '		AddPolicyFactor(oPolicy, "MULTILINE3")
    '		RemovePolicyFactor(oPolicy, "MULTILINE1")
    '		RemovePolicyFactor(oPolicy, "MULTILINE2")

    '		' todo: remove once policy discounts is complete implemented
    '		RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE1")
    '		RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE2")
    '		oPolicy.Notes = (AddNote(oPolicy.Notes, "Discount:MULTILINE3", "MULTILINE3", "DIS", oPolicy.Notes.Count))
    '		' end todo

    '		If Not HasDiscount(oPolicy, "MULTILINE3") Then
    '			Dim oNewdiscount As New clsHomeOwnerDiscount
    '			oNewdiscount.FactorCategory = "MULTILINE3"
    '			oNewdiscount.FactorCode = "MULTILINE3"
    '               oNewdiscount.IsNew = True
    '               oNewdiscount.FactorType = "POLICY"
    '               oNewdiscount.IsNew = True
    '               oNewdiscount.UnitNumber = 1
    '               oNewdiscount.Param1 = ""
    '               oNewdiscount.Param2 = ""
    '               oNewdiscount.Param3 = ""
    '               oNewdiscount.Param4 = ""
    '               oPolicy.Discounts.Add(oNewdiscount)
    '		End If
    '	Else
    '           If HasDiscount(oPolicy, "MULTILINE3") And oPolicy.CallingSystem.ToUpper <> "WEBRATER" Then
    '               bHasBoth = True
    '               AddPolicyFactor(oPolicy, "MULTILINE3")
    '           Else
    '               bHasBoth = False
    '               RemoveNotes(oPolicy.Notes, "DIS", "MULTILINE3")

    '               For Each oDiscount As clsHomeOwnerDiscount In oPolicy.Discounts
    '                   If oDiscount.FactorCode = "MULTILINE3" Then
    '                       oPolicy.Discounts.Remove(oDiscount)
    '                       Exit For
    '                   End If
    '               Next
    '           End If
    '	End If

    '	If bHasBoth Then
    '		'don't clear out policy id fields
    '	Else
    '		If Not bHasAuto Then
    '			RemoveNotes(oPolicy.Notes, "MLN", "MULTILINE1")
    '			oPolicy.CompanionPAPolicyID = ""
    '		ElseIf Not bHasFlood Then
    '			RemoveNotes(oPolicy.Notes, "MLN", "MULTILINE2")
    '			oPolicy.CompanionFloodPolicyID = ""
    '		End If
    '	End If

    'End Function

    Public Overridable Function AddPolicyFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

    End Function

    Public Overridable Sub SetIncreasedLimitFactors(ByVal oPolicy As clsPolicyHomeOwner)

    End Sub

    Public Overridable Sub SetLossLevel(ByVal oPolicy As clsPolicyHomeOwner)

        Dim iNumClaimsLess5YRS As Integer = 0
        Dim iNumNonWeatherLoss As Integer = 0
        Dim iNumWeatherLoss As Integer = 0
        Dim iNumChargeLoss As Integer = 0
        Dim iLossLevel As Integer = 0

        'RemovePolicyFactor(oPolicy, "NOCLAIM")

        'N-Water
        'O-NonWeather
        'W-Weather

        ' We need to use EffDate for lookup because this change is going forward since we currently do not have MatchType information on 
        ' But renewals will still also have to go through the old method since Match Type data is not set
        If CorFunctions.CommonFunctions.GetStateInfoValue(oPolicy.Product, oPolicy.StateCode, oPolicy.EffDate, oPolicy.Program, "LOSSLEVEL", "USE_MATCHTYPE", String.Empty) = "1" Then
            iLossLevel = SetLossLevelWithMatchType(oPolicy)
        End If

        If iLossLevel <> 0 Then
            oPolicy.DwellingUnits.Item(0).LossLevel = iLossLevel
        ElseIf iLossLevel = 0 Then
            With oPolicy.DwellingUnits.Item(0)
                For Each oClaim As clsBaseClaim In .Claims
                    ' Start with claim being chargeable, remove if it meets any of the criteria below
                    If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                        oClaim.Chargeable = True
                    End If

                    If DateAdd(DateInterval.Month, 60, oClaim.ClaimDate) >= oPolicy.EffDate Then
                        If oClaim.ClaimAmt > 0 Then
                            iNumClaimsLess5YRS += 1
                        End If

                        If oPolicy.CallingSystem <> "PAS" Then
                            If DateAdd(DateInterval.Month, 36, oClaim.ClaimDate) < oPolicy.EffDate Then
                                'nonchargeable
                                oClaim.Chargeable = False
                            End If
                        End If
                    Else
                        If oPolicy.CallingSystem <> "PAS" Then
                            oClaim.Chargeable = False
                        End If
                    End If
                    If oClaim.ClaimAmt = 0 Then
                        If oPolicy.CallingSystem <> "PAS" Then
                            oClaim.Chargeable = False
                        End If
                    End If
                    If oClaim.Chargeable And oClaim.ClaimAmt > 0 Then
                        If oClaim.ClaimTypeIndicator.Trim <> "W" Then
                            iNumNonWeatherLoss += 1
                        Else
                            iNumWeatherLoss += 1
                        End If
                        iNumChargeLoss += 1
                    End If
                Next

                '...added by 
                Dim bNewProcess As Boolean = False
                Dim sLossLevelEffDate As String = CorFunctions.CommonFunctions.GetStateInfoValue(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.Program, "LOSSLEVEL", "USENEW", String.Empty)
                If String.IsNullOrWhiteSpace(sLossLevelEffDate) Then
                    sLossLevelEffDate = Now()
                End If

                If oPolicy.RateDate < CDate(sLossLevelEffDate) Then
                    bNewProcess = False
                Else
                    bNewProcess = True
                End If

                .LossLevel = GetLossLevel(oPolicy, iNumChargeLoss, iNumNonWeatherLoss, iNumClaimsLess5YRS, bNewProcess)
            End With
        End If

    End Sub

    Public Overridable Function SetLossLevelWithMatchType(ByVal policy As clsPolicyHomeOwner) As Integer

        Dim numClaimsLess5YRS As Integer = 0
        Dim numNonWeatherLoss As Integer = 0
        Dim numWeatherLoss As Integer = 0
        Dim numChargeLoss As Integer = 0
        Dim lossLevel As Integer = 0
        Dim useOldMethod As Boolean = False

        'RemovePolicyFactor(oPolicy, "NOCLAIM")

        'N-Water
        'O-NonWeather
        'W-Weather

        With policy.DwellingUnits.Item(0)
            For Each claim As clsBaseClaim In .Claims
                ' First we need to see if Match Type is set, if not we need to use the old method
                If String.IsNullOrWhiteSpace(claim.MatchType) Then
                    useOldMethod = True
                    Exit For
                End If
                ' Start with claim being chargeable, remove if it meets any of the criteria below
                If policy.CallingSystem.ToUpper = "WEBRATER" Then
                    claim.Chargeable = True
                End If

                If DateAdd(DateInterval.Month, 60, claim.ClaimDate) >= policy.EffDate Then
                    If claim.ClaimAmt > 0 Then
                        numClaimsLess5YRS += 1
                    End If

                    If policy.CallingSystem <> "PAS" Then
                        If DateAdd(DateInterval.Month, 36, claim.ClaimDate) < policy.EffDate Then
                            'nonchargeable
                            claim.Chargeable = False
                        End If
                    End If
                Else
                    If policy.CallingSystem <> "PAS" Then
                        claim.Chargeable = False
                    End If
                End If
                If claim.ClaimAmt = 0 Then
                    If policy.CallingSystem <> "PAS" Then
                        claim.Chargeable = False
                    End If
                End If
                If claim.Chargeable And claim.ClaimAmt > 0 Then
                    If claim.ChargeableAgainst.ToUpper.Trim = "BOTH" _
                        OrElse claim.ChargeableAgainst.ToUpper.Trim = claim.MatchType.ToUpper.Trim Then
                        If claim.ClaimTypeIndicator.Trim <> "W" Then
                            numNonWeatherLoss += 1
                        Else
                            numWeatherLoss += 1
                        End If
                        numChargeLoss += 1
                    Else
                        claim.Chargeable = False
                    End If

                End If
            Next

            If Not useOldMethod Then
                Dim bNewProcess As Boolean = False
                Dim sLossLevelEffDate As String = CorFunctions.CommonFunctions.GetStateInfoValue(policy.Product, policy.StateCode, policy.RateDate, policy.Program, "LOSSLEVEL", "USENEW", String.Empty)
                If String.IsNullOrWhiteSpace(sLossLevelEffDate) Then
                    sLossLevelEffDate = Now()
                End If

                If policy.RateDate < CDate(sLossLevelEffDate) Then
                    bNewProcess = False
                Else
                    bNewProcess = True
                End If

                lossLevel = GetLossLevel(policy, numChargeLoss, numNonWeatherLoss, numClaimsLess5YRS, bNewProcess)
            End If
        End With

        Return lossLevel

    End Function


    Public Overridable Sub SetUnderwriterTier(ByVal oPolicy As clsPolicyHomeOwner)

        dbGetUWTier(oPolicy)

        'This is here for PAS, so don't remove it 
        'UW TIER
        Select Case oPolicy.UWTier
            Case "A"
                AddPolicyFactor(oPolicy, "UW_A")
            Case "B"
                AddPolicyFactor(oPolicy, "UW_B")
            Case "C"
                AddPolicyFactor(oPolicy, "UW_C")
            Case "D"
                AddPolicyFactor(oPolicy, "UW_D")
        End Select
    End Sub

    Public Overridable Sub SetCreditTier(ByVal oPolicy As clsPolicyHomeOwner)

        dbGetCreditTier(oPolicy)

        ' Remove existing credit tier factors
        For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
            If oPolicy.PolicyFactors.Item(i).FactorCode.Length > 7 Then
                If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper.Substring(0, 7) = "CREDIT_" Then
                    'remove it
                    oPolicy.PolicyFactors.RemoveAt(i)
                End If
            End If
        Next

        'CREDIT
        Select Case oPolicy.PolicyInsured.CreditTier
            Case "1"
                AddPolicyFactor(oPolicy, "CREDIT_1")
            Case "2"
                AddPolicyFactor(oPolicy, "CREDIT_2")
            Case "3"
                AddPolicyFactor(oPolicy, "CREDIT_3")
            Case "4"
                AddPolicyFactor(oPolicy, "CREDIT_4")
            Case "5"
                AddPolicyFactor(oPolicy, "CREDIT_5")
            Case "6"
                AddPolicyFactor(oPolicy, "CREDIT_6")
            Case "7"
                AddPolicyFactor(oPolicy, "CREDIT_7")
            Case "8"
                AddPolicyFactor(oPolicy, "CREDIT_8")
            Case "9"
                AddPolicyFactor(oPolicy, "CREDIT_9")
            Case "10"
                AddPolicyFactor(oPolicy, "CREDIT_10")
            Case "11"
                AddPolicyFactor(oPolicy, "CREDIT_11")
            Case "12"
                AddPolicyFactor(oPolicy, "CREDIT_12")
            Case "13"
                AddPolicyFactor(oPolicy, "CREDIT_13")
            Case "14"
                AddPolicyFactor(oPolicy, "CREDIT_14")
            Case "15"
                AddPolicyFactor(oPolicy, "CREDIT_15")
            Case "16"
                AddPolicyFactor(oPolicy, "CREDIT_16")
            Case "17"
                AddPolicyFactor(oPolicy, "CREDIT_17")
            Case "18"
                AddPolicyFactor(oPolicy, "CREDIT_18")
            Case "19"
                AddPolicyFactor(oPolicy, "CREDIT_19")
            Case "20"
                AddPolicyFactor(oPolicy, "CREDIT_20")
            Case "21"
                AddPolicyFactor(oPolicy, "CREDIT_21")
            Case "22"
                AddPolicyFactor(oPolicy, "CREDIT_22")
            Case "23"
                AddPolicyFactor(oPolicy, "CREDIT_23")
            Case "24"
                AddPolicyFactor(oPolicy, "CREDIT_24")
        End Select
    End Sub

    Public Overridable Function GetLossLevel(ByVal oPolicy As clsPolicyHomeOwner, ByVal iNumChargeLoss As Integer, ByVal iNumNonWeatherLoss As Integer, _
                                          ByVal iNumClaimsLess5YRS As Integer, ByVal bNewProcess As Boolean) As String
        Dim iTempLossLevel As Integer

        If bNewProcess Then
            If iNumChargeLoss = 0 Then
                iTempLossLevel = 1
                If iNumClaimsLess5YRS = 0 Then
                    If oPolicy.CallingSystem.Contains("PAS") Or oPolicy.CallingSystem.Contains("CITIZENS") Then
                        AddPolicyFactor(oPolicy, "NOCLAIM")
                    End If
                End If
            ElseIf iNumChargeLoss <= 3 Then
                If iNumNonWeatherLoss = 0 Then
                    iTempLossLevel = 1
                ElseIf iNumNonWeatherLoss = 1 Then
                    iTempLossLevel = 2
                ElseIf iNumNonWeatherLoss = 2 Then
                    iTempLossLevel = 3
                Else
                    iTempLossLevel = 4
                End If
            Else
                iTempLossLevel = 4
            End If
        Else
            If iNumChargeLoss = 0 Then
                iTempLossLevel = 1
                If iNumClaimsLess5YRS = 0 Then
                    If oPolicy.CallingSystem.Contains("PAS") Or oPolicy.CallingSystem.Contains("CITIZENS") Then
                        AddPolicyFactor(oPolicy, "NOCLAIM")
                    End If
                End If
            ElseIf iNumChargeLoss = 1 Then
                If iNumNonWeatherLoss > 0 Then
                    iTempLossLevel = 2
                Else
                    iTempLossLevel = 1
                End If
            ElseIf iNumChargeLoss = 2 Then
                If iNumNonWeatherLoss > 1 Then
                    iTempLossLevel = 3
                Else
                    iTempLossLevel = 2
                End If
            ElseIf iNumChargeLoss = 3 Then
                If iNumNonWeatherLoss > 2 Then
                    iTempLossLevel = 4
                Else
                    iTempLossLevel = 3
                End If
            ElseIf iNumChargeLoss > 3 Then
                iTempLossLevel = 4
            End If
        End If

        Return iTempLossLevel

    End Function

    Public Overridable Function dbGetUWTier(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""


        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()
        Try

            If oPolicy.DwellingUnits.Item(0).HomeAge >= 999 Then
                oPolicy.DwellingUnits.Item(0).HomeAge = 998
            End If

            If oPolicy.DwellingUnits.Item(0).HomeAge < 999 And oPolicy.DwellingUnits.Item(0).LossLevel > 0 And oPolicy.DwellingUnits.Item(0).ProtectionClass <> "" Then

                Using cmd As New SqlCommand(sSql, oConn)

                    sSql = " SELECT Tier FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeUWTiers with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND HomeAgeStart <= @HomeAge "
                    sSql = sSql & " AND HomeAgeEnd > @HomeAge "
                    sSql = sSql & " AND LossLevel = @LossLevel "
                    'sSql = sSql & " AND LossLevel <= @LossLevel "
                    sSql = sSql & " AND OwnerOccupiedFlag IN ( @OwnerOccupiedFlag , 99) "
                    sSql = sSql & " AND MaxProtectionClass >= @MaxProtectionClass "
                    sSql = sSql & " AND DwellingCoverage <= @DwellingCoverageAmt "
                    sSql = sSql & " ORDER BY Tier Asc "

                    'Execute the query
                    cmd.CommandText = sSql

                    If oPolicy.DwellingUnits(0).ProtectionClass.ToUpper = "NOT LOADED" Then
                        oPolicy.DwellingUnits(0).ProtectionClass = 99
                    End If

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@HomeAge", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).HomeAge
                    cmd.Parameters.Add("@LossLevel", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).LossLevel
                    cmd.Parameters.Add("@OwnerOccupiedFlag", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).OwnerOccupiedFlag
                    cmd.Parameters.Add("@MaxProtectionClass", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits(0).ProtectionClass
                    cmd.Parameters.Add("@DwellingCoverageAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits(0).DwellingAmt
                    oReader = cmd.ExecuteReader

                    Do While oReader.Read()
                        sTier = oReader.Item("Tier")
                        'just get the first one since there could be multiple tiers returned
                        oPolicy.UWTier = sTier
                        Exit Do
                    Loop

                End Using
            Else
                sTier = "A"
            End If

            Return sTier

        Catch ex As Exception
            Throw New Exception("dbGetUWTier Failed: ", ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            oConn.Close()
            oConn.Dispose()
        End Try

    End Function

    Public Overridable Function dbGetCreditTier(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""

        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
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

    <WebMethod(CacheDuration:=3600)> _
    Public Shared Function GetMaxAmount(ByVal iState As Integer, ByVal iProduct As Integer, ByVal sProgram As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String, ByVal sType As String) As Long
        Dim sSql As String = ""
        Dim lMaxAmt As Long = 0
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Dim oDS As New DataSet

        Try



            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Max(Amount) "
                sSql = sSql & " FROM pgm" & iProduct & iState & "..FactorAmtOfInsurance with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Type = @Type "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode
                cmd.Parameters.Add("@Type", SqlDbType.VarChar, 1).Value = sType

                oConn.Open()
                lMaxAmt = cmd.ExecuteScalar
                oConn.Close()

                Return lMaxAmt

            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

    End Function

    <WebMethod(CacheDuration:=3600)> _
    Public Function GetMaxOtherStructureAmt(ByVal iState As Integer, ByVal iProduct As Integer, ByVal sProgram As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As Long
        Dim sSql As String = ""
        Dim lMaxAmt As Long = 0
        Dim oReturn As Object
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Dim oDS As New DataSet

        Try



            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Max(Amount) "
                sSql = sSql & " FROM pgm" & iProduct & iState & "..FactorAPS with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                oConn.Open()
                oReturn = cmd.ExecuteScalar
                oConn.Close()

                If Not TypeOf (oReturn) Is System.DBNull Then
                    lMaxAmt = Double.Parse(oReturn)
                Else
                    lMaxAmt = 999999
                End If


                Return lMaxAmt


            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

    End Function

    Public Overloads Function RemoveNotes(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sSourceCode As String, ByVal sNoteDescription As String) As System.Collections.Generic.List(Of clsBaseNote)

        For i As Integer = oNoteList.Count - 1 To 0 Step -1
            If oNoteList.Item(i).SourceCode.ToUpper = sSourceCode.ToUpper Then
                If oNoteList.Item(i).NoteDesc.ToUpper = sNoteDescription.ToUpper Then
                    oNoteList.RemoveAt(i)
                End If
            End If
        Next

        Return oNoteList

    End Function

    Public Overloads Function GetNote(ByVal oPolicy As clsBasePolicy, ByVal sSourceCode As String, ByVal sNoteDesc As String) As clsBaseNote

        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.SourceCode.ToUpper = sSourceCode.ToUpper Then
                If oNote.NoteDesc.ToString.ToUpper = sNoteDesc.ToString.ToUpper Then
                    Return oNote
                    Exit For
                End If
            End If
        Next

        Return Nothing

    End Function

    Public Overloads Sub UpdateNote(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sExistingNoteDescription As String, ByVal sExistingNoteSourceCode As String, ByVal sNewNoteText As String)

        For Each oNote As clsBaseNote In oNoteList
            If oNote.SourceCode.ToUpper = sExistingNoteSourceCode.ToUpper Then
                If oNote.NoteDesc.ToUpper = sExistingNoteDescription.ToUpper Then
                    oNote.NoteText = sNewNoteText
                End If
            End If
        Next
    End Sub

    Public Overridable Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sItemsToBeFaxedIn As String = ""


        sItemsToBeFaxedIn &= "Signed and dated Application by both insured and agent" & vbNewLine
        sItemsToBeFaxedIn &= "Signed Flood Waiver (if applicable)" & vbNewLine
        If oPolicy.IsEFT Then
            sItemsToBeFaxedIn &= "EFT Authorization Form" & vbNewLine
        End If

        Return sItemsToBeFaxedIn

    End Function

    Public Overridable Function AddRenewalFactors(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        'RENEWAL FACTORS

    End Function

    Public Function UseActualCashValue(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim sSQL As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Try
            'Open the connection


            sSQL = "Select ItemValue From pgm" & oPolicy.Product & oPolicy.StateCode & "..StateInfo with(nolock) "
            sSQL &= " WHERE EffDate <= @RateDate "
            sSQL &= " AND ExpDate > @RateDate "
            sSQL &= " AND AppliesToCode IN ('B', @AppliesToCode) "
            sSQL &= " AND Program = @Program "
            sSQL &= " AND ItemGroup = 'DISPLAY' "
            sSQL &= " AND ItemCode = 'ACV' "
            sSQL &= " AND ItemSubCode = 'False' "

            Dim cmd As SqlCommand = New SqlCommand(sSQL, oConn)

            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program

            oConn.Open()
            Dim oRr As Data.SqlClient.SqlDataReader
            oRr = cmd.ExecuteReader
            If Not oRr Is Nothing Then
                If oRr.Read Then
                    Return False
                Else
                    Return True
                End If
            Else
                Return True
            End If
            oConn.Close()

        Catch ex As SoapException
            Return False
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

    End Function

    Public Overridable Sub ResetFireDepartmentNum(ByVal oPolicy As clsPolicyHomeOwner)
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim iFireDeptNum As Integer = 0
        If Len(oPolicy.DwellingUnits(0).PlaceCode) > 0 Then
            Dim oReader As SqlDataReader
            Try

                Dim sSql As String = ""
                Using cmd As New SqlCommand(sSql, oConn)

                    sSql = " SELECT FireDepartmentNum "
                    sSql = sSql & " FROM PasCarrier..HOMFireDepartment with(nolock)"
                    sSql = sSql & " where Territory = @Territory"
                    sSql = sSql & " and PlaceCode = @PlaceCode"
                    sSql = sSql & " and FireDepartmentDesc = @FireDeptDesc"

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Territory", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).Territory.Trim
                    cmd.Parameters.Add("@FireDeptDesc", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).FireDept.Trim
                    cmd.Parameters.Add("@PlaceCode", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).PlaceCode.Trim

                    oConn.Open()
                    oReader = cmd.ExecuteReader
                    While oReader.Read()
                        iFireDeptNum = oReader("FireDepartmentNum")
                    End While
                    oConn.Close()

                End Using
            Catch ex As Exception

            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
            If iFireDeptNum <> 0 Then
                oPolicy.DwellingUnits(0).FireDeptNum = iFireDeptNum
            End If
        End If
    End Sub
    Public Overridable Sub ResetTerritory(ByVal oPolicy As clsPolicyHomeOwner)

        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sTerritory As String = String.Empty
        Dim sFireDeptDesc As String = String.Empty
        Dim sOldFireDeptDesc As String = String.Empty
        Dim sRegion As String = String.Empty

        If Len(oPolicy.DwellingUnits(0).PlaceCode) > 0 Then
            Dim oReader As SqlDataReader
            Try

                Dim sSql As String = ""
                Using cmd As New SqlCommand(sSql, oConn)

                    sSql = " SELECT Territory,Region, FireDept"
                    sSql = sSql & " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..CodeTerritoryDefinitions with(nolock)"
                    sSql = sSql & " WHERE Zip = @Zip "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND PlaceCode = @PlaceCode "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@Zip", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).Zip.Trim

                    If oPolicy.DwellingUnits(0).PlaceCode.Trim <> "999" Then
                        cmd.Parameters.Add("@PlaceCode", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).PlaceCode.Trim
                    Else
                        cmd.Parameters.Add("@PlaceCode", SqlDbType.VarChar).Value = ""
                    End If
                    sOldFireDeptDesc = oPolicy.DwellingUnits(0).FireDept.Trim

                    oConn.Open()
                    oReader = cmd.ExecuteReader
                    While oReader.Read()
                        sTerritory = oReader("Territory")
                        sRegion = oReader("Region")
                        sFireDeptDesc = oReader("FireDept")
                    End While

                    ' todo: test this
                    ' todo: test this
                    ' todo: test this
                    ' todo: test this
                    ' todo: test this
                    ' todo: test this
                    If Not oReader.HasRows() Then
                        Dim sNoteText As String
                        sNoteText = "Zip code " & oPolicy.DwellingUnits(0).Zip.Trim & " is not valid with place code " & oPolicy.DwellingUnits(0).PlaceCode.Trim
                        AddPolicyNote(oPolicy, sNoteText, "InvalidTerritory", "1")

                    End If
                    oConn.Close()

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

                If sOldFireDeptDesc <> sFireDeptDesc Then
                    oPolicy.DwellingUnits(0).FireDept = sFireDeptDesc
                    ' need to verify that the FireDepartmentNum matches 
                    ' using the Territory, PlaceCode, and FireDepartmentDesc
                    ResetFireDepartmentNum(oPolicy)
                End If
            End If
        End If
    End Sub

    Public Sub AddPolicyNote(ByVal oPolicy As clsPolicyHomeOwner, ByVal sDesc As String, ByVal sCode As String, ByVal sRedFlag As String)
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String

        Try
            sSql = "  INSERT INTO PasCarrier..PolicyNote (CompanyCode,ProgramCode,PolicyTransactionNum,MonolineProductCode,PolicyNo,NoteTypeCode,NoteDesc,RedFlag,NoteText,TermEffDate,TermExpDate,AddedDateT,AddedUserCode,LastUpdatedDateT,LastUpdatedUserCode)"
            sSql &= " Values(@CompanyCode,@ProgramCode,@TransNum,'PA',@PolicyNo,'PRE',@NoteDesc,@RedFlag,@NoteText,@EffDate,@ExpDate,getdate(),'RATINGRULES',null,null)"

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)
                cmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = "IF"
                cmd.Parameters.Add("@ProgramCode", SqlDbType.VarChar).Value = oPolicy.ProgramCode
                cmd.Parameters.Add("@TransNum", SqlDbType.Int).Value = oPolicy.TransactionNum
                cmd.Parameters.Add("@PolicyNo", SqlDbType.VarChar).Value = oPolicy.PolicyID
                cmd.Parameters.Add("@NoteDesc", SqlDbType.VarChar).Value = sCode
                cmd.Parameters.Add("@RedFlag", SqlDbType.VarChar).Value = sRedFlag
                cmd.Parameters.Add("@NoteText", SqlDbType.VarChar).Value = sDesc
                cmd.Parameters.Add("@EffDate", SqlDbType.DateTime).Value = oPolicy.EffDate
                cmd.Parameters.Add("@ExpDate", SqlDbType.DateTime).Value = oPolicy.ExpDate

                cmd.CommandText = sSql
                cmd.ExecuteNonQuery()
            End Using

            oConn.Close()
        Catch ex As Exception
            Throw New Exception("Error in Function AddPolicyNote" & ex.Message)
        End Try


    End Sub

    Public Function CheckForRestrictedCounty(ByVal oPolicy As clsPolicyHomeOwner, Optional ByVal sRestrictionType As String = "") As Boolean

        If oPolicy.DwellingUnits(0).County <> "" Then
            If sRestrictionType = "" Then
                sRestrictionType = "RESTRICTION"
            End If

            If CorFunctions.CommonFunctions.GetStateInfoValue(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.Program, sRestrictionType, "COUNTY", oPolicy.DwellingUnits(0).County).ToUpper = "TRUE" Then
                If sRestrictionType = "RESTRICTION" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county.", sRestrictionType & " Restr", "IER", oPolicy.Notes.Count))
                End If
                Return True
            End If
        End If

        Return False

    End Function

    Public Function CheckForRestrictedCountyByMaxAge(ByVal oPolicy As clsPolicyHomeOwner, ByVal iMaxHomeAge As Integer, Optional ByVal sRestrictionType As String = "") As Boolean

        If oPolicy.DwellingUnits(0).County <> "" Then
            If iMaxHomeAge = 10 Then
                sRestrictionType = "10YEARRESTRICTION"
            ElseIf iMaxHomeAge = 30 Then
                sRestrictionType = "30YEARRESTRICTION"
            End If

            If CorFunctions.CommonFunctions.GetStateInfoValue(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.Program, sRestrictionType, "COUNTY", oPolicy.DwellingUnits(0).County).ToUpper = "TRUE" Then
                'oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: We are not currently accepting new business in " & StrConv(oPolicy.DwellingUnits(0).County, VbStrConv.ProperCase) & " county.", sRestrictionType & " Restr", "IER", oPolicy.Notes.Count))
                Return True
            End If
        End If

        Return False

    End Function

#End Region

End Class
