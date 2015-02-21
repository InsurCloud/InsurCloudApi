Imports Microsoft.VisualBasic
Imports CorPolicy.clsCommonFunctions
Imports CorPolicy
Public Class clsPgm4
    Inherits clsPgm

    Public Sub New()

    End Sub

    Public Overloads Sub FinishLogging(ByVal bLogRate As Boolean)
        MyBase.FinishLogging(bLogRate)
        If bLogRate Then
            ErrorLogging("", "")
        End If
    End Sub

    Public Overloads Sub ErrorLogging(ByVal sMethodName As String, ByVal sMessage As String)
        Dim oLogSvc As New ImperialFire.Logging
        oLogSvc.WriteHomeownersLog(moLogging, sMethodName, sMessage)
    End Sub

    Public Overridable Sub UpdateLog(ByVal oPolicy As CorPolicy.clsPolicyFlood, ByVal oFactorTable As System.Data.DataTable)
        CType(moLogging, ImperialFire.clsLogging4).Policy = oPolicy
        moLogging.DataTable.Add(oFactorTable)
    End Sub

    Public Overloads Function Rate(ByVal oPolicy As clsPolicyFlood, ByVal bLogRate As Boolean, Optional ByVal bLookupLatLong As Boolean = False) As Boolean


  ' ''      Try
  ' ''          Call BeginLogging(moLogging, Nothing, Nothing)

  ' ''          Call InitializeConnection()


  ' ''          If bLookupLatLong Then
  ' ''              Dim oES As New ImperialFire.ExternalRptService
  ' ''              oPolicy.DwellingUnits(0) = oES.LookupElevation(oPolicy.DwellingUnits(0))
  ' ''              UpdateLog(oPolicy, Nothing)
  ' ''          End If

		' ''	Dim oTFS As New torrentflood.RatingService

		' ''	Dim oFlood As New torrentflood.RatingRequest
		' ''	oFlood.Factors = New torrentflood.RatingFactors
		' ''	oFlood.Factors.ElevationData = New torrentflood.ElevationData
		' ''	oFlood.Requestor = New torrentflood.Credentials

  ' ''          AddLog("Begin Flood Object mapping", "Torrent")
  ' ''          With oPolicy.DwellingUnits(0)
  ' ''              oFlood.Factors.BasementType_ID = .BasementTypeID
  ' ''              oFlood.Factors.BFE_Diff = .BFEDiff
  ' ''              oFlood.Factors.BuildingCoverage = .DwellingAmt
  ' ''              If .Ded1 < 1 Then
  ' ''                  oFlood.Factors.BuildingDeductible = .DwellingAmt * .Ded1
  ' ''              Else
  ' ''                  oFlood.Factors.BuildingDeductible = .Ded1
  ' ''              End If
  ' ''              oFlood.Factors.Community_Panel_ID = .CommunityPanelID 'Not sure how we get this yet
  ' ''              oFlood.Factors.CondominiumType = .CondoType
  ' ''              oFlood.Factors.ConstructionDate = "01/01/" & .YearOfConstruction
  ' ''              oFlood.Factors.ContentsCoverage = .ContentsAmt
  ' ''              If .Ded2 < 1 Then
  ' ''                  oFlood.Factors.ContentsDeductible = .DwellingAmt * .Ded2
  ' ''              Else
  ' ''                  oFlood.Factors.ContentsDeductible = .Ded2
  ' ''              End If

  ' ''              oFlood.Factors.ContentsLocation_ID = .ContentsLocation
  ' ''              oFlood.Factors.CRSCreditPercentage = .CRSCreditPercentage
  ' ''              oFlood.Factors.Elevation_Cert_ID = .ElevationCertId
  ' ''              oFlood.Factors.FloodRiskZone = .FloodRiskZone
  ' ''              oFlood.Factors.FloorsBuildingType_ID = .NumOfFloors
  ' ''              oFlood.Factors.IsElevated = .IsElevated
  ' ''              oFlood.Factors.IsPostFirmConstruction = .PostFirmConstruction
  ' ''              oFlood.Factors.IsObstruction = .IsObstruction
  ' ''              oFlood.Factors.NumFloorsCondo = .NumCondoFloors
  ' ''              oFlood.Factors.NumUnitsCondo = .NumCondoUnits
  ' ''              oFlood.Factors.OccupancyType_ID = 1
  ' ''              oFlood.Factors.State = .State

  ' ''              AddLog("Set Elevation Data on Flood Object", "Torrent")
  ' ''              oFlood.Factors.ElevationData.BaseFloodElevation = .BaseFloorElevation
  ' ''              oFlood.Factors.ElevationData.BottomOfLowestHorizontalStructuralMember = .BottomOfLowestHorizStructMember
  ' ''              oFlood.Factors.ElevationData.ElevationOfMandE = .ElevationOfMandE
  ' ''              oFlood.Factors.ElevationData.GarageFloorElevation = .GarageFloorElevation
  ' ''              oFlood.Factors.ElevationData.HighestAdjacentGrade = .HighestAdjacentGrade
  ' ''              oFlood.Factors.ElevationData.LowestAdjacentGrade = .LowestAdjacentGrade
  ' ''              oFlood.Factors.ElevationData.LowestFloorElevation = .LowestFloorElevation
  ' ''              oFlood.Factors.ElevationData.TopOfNextHighestFlood = .TopOfNextHighestFlood

  ' ''          End With
  ' ''          AddLog("Set Policy Level attributes", "Torrent")
  ' ''          With oPolicy
  ' ''              If .Program = "STD" Then
  ' ''                  oFlood.Factors.RiskRatingMethod = 1
  ' ''              Else
  ' ''                  oFlood.Factors.RiskRatingMethod = 2
  ' ''              End If
  ' ''              oFlood.Factors.IsProbation = .IsProbation

  ' ''              oFlood.Factors.IsEmergencyProgram = .IsEmergencyProgram
  ' ''              oFlood.Factors.InsuranceToValueIndicator = .InsuranceToValueInd
  ' ''              oFlood.Factors.Effective_Date = .EffDate
  ' ''              oFlood.Factors.Customer_Tracking = .QuoteID
  ' ''          End With

  ' ''          With oPolicy.Agency
  ' ''              oFlood.Requestor.Account_Number = "IMPERIAL"
  ' ''              oFlood.Requestor.User_ID = .AgencyID
  ' ''              oFlood.Requestor.Password = "H40P7"
  ' ''              oFlood.Requestor.Customer_Tracking = ""
  ' ''          End With

  ' ''          AddLog("Begin Rate call to Torrent", "Torrent")
  ' ''          Dim oRR As torrentflood.RatingResponse
  ' ''          oRR = oTFS.GetRate(oFlood)

  ' ''          Dim bReturn As Boolean = False
  ' ''          If oRR.Result.IsRated Then
  ' ''              AddLog("Rate has been returned", "Torrent")
  ' ''              oPolicy.FullTermPremium = oRR.Result.Premium_ICC
  ' ''              oPolicy.TotalFees = oRR.Result.FederalPolicyFee
  ' ''              'oRR.Result.Rate_End_Date
  ' ''              'oRR.result.rate_start_date
  ' ''              'oRR.Result.Rating_Table
  ' ''              'oRR.Result.Torrent_Tracking
  ' ''              bReturn = True
  ' ''          Else
  ' ''              oPolicy.Status = oRR.Status.Message
  ' ''              AddLog("No Rate - " & oRR.Status.Message & -oRR.Status.Code, "Torrent")
  ' ''              bReturn = False
  ' ''          End If

  ' ''          Call FinishLogging(bLogRate)
  ' ''          Return bReturn
  ' ''      Catch ex As Exception
  ' ''          Call ErrorLogging("Flood", ex.Message & ex.StackTrace)
		' ''	Return False
		' ''Finally
		' ''	If moConn IsNot Nothing Then
		' ''		moConn.Close()
		' ''		moConn.Dispose()
		' ''	End If
		' ''End Try

    End Function
End Class
