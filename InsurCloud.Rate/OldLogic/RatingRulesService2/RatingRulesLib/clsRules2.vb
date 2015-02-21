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

Public Class clsRules2
    Inherits clsRules

    Protected ExclFactorType As String

    Public Sub CheckValidVIN(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = ""

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                If oVehicle.VehicleSymbolCode <> String.Empty Then
                    If Integer.Parse(oVehicle.VehicleYear) < 2011 Then
                        If oVehicle.VinNo.ToUpper <> "NONOWNER" And oVehicle.PriceNewSymbolCode.Trim <> "999" And oVehicle.PriceNewSymbolCode.Trim <> "65" And oVehicle.PriceNewSymbolCode.Trim <> "66" And oVehicle.PriceNewSymbolCode.Trim <> "67" And oVehicle.PriceNewSymbolCode.Trim <> "68" Then
                            If Not IsValidVIN(oVehicle.VinNo, oPolicy) Then
                                If sVehicleList = String.Empty Then
                                    sVehicleList = oVehicle.IndexNum.ToString()
                                Else
                                    sVehicleList &= ", " & oVehicle.IndexNum
                                End If
                            End If
                        End If
                    Else
                        If oVehicle.VinNo.ToUpper <> "NONOWNER" And oVehicle.PriceNewSymbolCode.Trim <> "999" And oVehicle.PriceNewSymbolCode.Trim <> "965" And oVehicle.PriceNewSymbolCode.Trim <> "966" And oVehicle.PriceNewSymbolCode.Trim <> "967" And oVehicle.PriceNewSymbolCode.Trim <> "968" Then
                            If Not IsValidVIN(oVehicle.VinNo, oPolicy) Then
                                If sVehicleList = String.Empty Then
                                    sVehicleList = oVehicle.IndexNum.ToString()
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
            If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an invalid VIN.  Please verify the VIN number has been entered correctly.  Call 866-874-2741 if you need to speak to an Imperial Agent. - " & sVehicleList & ".", "InvalidVIN2", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an invalid VIN - " & sVehicleList & ".", "InvalidVIN2", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Private Function IsValidVIN(ByVal vin As String, ByRef oPolicy As clsPolicyPPA) As Boolean

        Try
            Dim VINSvc As New VinService.VinServiceClient
            Dim checkDigitResult = VINSvc.VerifyCheckDigit(vin)

            If vin = "NONOWNER" Or checkDigitResult = "" Then
                Dim values As New List(Of String)
                Dim ds As New DataSet

                'TODO: IMPLEMENT PAS VIN LOOKUP IN NEW VINSERVICE
                'ds = VINSvc.LookUpPASVIN(vin, oPolicy.RateDate)

                If ds.Tables(0).Rows.Count > 0 Then
                    Return True
                End If
            Else
                'Throw New Exception("Verify Check Digit Failed")
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try

        Return False
    End Function


    Public Overridable Function dbGetUWTier(ByVal oPolicy As clsPolicyPPA) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""
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

        Try
            If oPolicy.PolicyInsured.PriorLimitsCode = "0" Then
                oPolicy.PolicyInsured.DaysLapse = 0
            Else
                Select Case oPolicy.EffDate.Subtract(oPolicy.PolicyInsured.PriorExpDate).Days
                    Case Is <= 7
                        oPolicy.PolicyInsured.DaysLapse = 2
                    Case 8 To 30
                        oPolicy.PolicyInsured.DaysLapse = 1
                    Case Else
                        oPolicy.PolicyInsured.DaysLapse = 0
                        oPolicy.PolicyInsured.PriorLimitsCode = "0"
                End Select
            End If

            '' Monthly does not qualify for the prior coverage discount
            'If oPolicy.PolicyInsured.DaysLapse > 0 Then
            '    If oPolicy.PriorCarrierName.ToUpper = "IMPERIAL MONTHLY" Then
            '        oPolicy.PolicyInsured.PriorLimitsCode = 0
            '    End If
            'End If

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
                If Not oPolicy.PolicyTermTypeInd Is Nothing Then
                    If oPolicy.PolicyTermTypeInd.ToUpper.Trim = "R" Then
                        If oPolicy.PolicyInsured.MonthsPriorContCov < 1 Then
                            oPolicy.PolicyInsured.MonthsPriorContCov = 6
                        End If
                    End If
                End If

                cmd.Parameters.Add("@ContCov", SqlDbType.Int, 22).Value = IIf(oPolicy.PolicyInsured.MonthsPriorContCov >= 6, 1, 0)

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    sTier = oReader.Item("Tier")
                    'just get the first one since there could be multiple tiers returned
                    oPolicy.PolicyInsured.UWTier = sTier
                    oPolicy.UWTier = sTier
                    Exit Do
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

    Public Sub New()
    End Sub

    Public Overloads Function CheckNEI(ByVal oPolicy As clsPolicyPPA) As Boolean

        oPolicy.Notes = RemoveNotes(oPolicy.Notes, "NEI")
        Dim bEnoughInfoToRate As Boolean = True
        Dim sMissing As String = ""

        Try

            'EffDate
            If Not IsDate(oPolicy.EffDate) Then
                bEnoughInfoToRate = False
                sMissing += "EffDate" & "-"
            End If

            'Drivers
            If oPolicy.DriverCount(True) < 1 Then
                bEnoughInfoToRate = False
                sMissing += "Drivers" & "-"
            End If

            'Vehicles
            If GetVehicleCount(oPolicy) < 1 Then
                bEnoughInfoToRate = False
                sMissing += "Vehicles" & "-"
            End If

            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete Then
                    'Garaging Zip
                    If oVeh.Zip = "" And oVeh.VinNo <> "NONOWNER" Then
                        bEnoughInfoToRate = False
                        sMissing += "Zip:Veh " & oVeh.IndexNum & "-"
                    End If

                    'Model Year
                    If oVeh.VehicleYear = "" Then
                        bEnoughInfoToRate = False
                        sMissing += "VehicleYear:Veh " & oVeh.IndexNum & "-"
                    End If

                    'LiabilitySymbolCode
                    If oVeh.LiabilitySymbolCode = "" Then
                        bEnoughInfoToRate = False
                        sMissing += "LiabilitySymbolCode:Veh " & oVeh.IndexNum & "-"
                    End If

                    'PIPMedLiabilityCode
                    If oVeh.PIPMedLiabilityCode = "" Then
                        bEnoughInfoToRate = False
                        sMissing += "PIPMedLiabilityCode:Veh " & oVeh.IndexNum & "-"
                    End If

                    'VehicleSymbolCode
                    If oVeh.VehicleSymbolCode = "" And (oVeh.CollSymbolCode = "" And oVeh.CompSymbolCode = "") Then
                        bEnoughInfoToRate = False
                        sMissing += "VehicleSymbolCode:Veh " & oVeh.IndexNum & "-"
                    End If

                    'Stated Value
                    If ((oVeh.VehicleSymbolCode = "66" Or oVeh.VehicleSymbolCode = "67" Or oVeh.VehicleSymbolCode = "68") And oVeh.VehicleYear < 2011) Or ((oVeh.VehicleSymbolCode = "966" Or oVeh.VehicleSymbolCode = "967" Or oVeh.VehicleSymbolCode = "968") And oVeh.VehicleYear >= 2011) Then
                        If oVeh.VehicleSymbolCode = "67" And oVeh.VehicleYear < 2011 And oPolicy.Program.ToUpper = "SUMMIT" And (oPolicy.StateCode = "03" Or oPolicy.StateCode = "42") Then
                            ' do nothing, AR Summit used 67 as the invalid  vin symbol, does not allow stated value
                        Else

                            If oVeh.StatedAmt < 500 Or oVeh.StatedAmt > 100000 Then
                                bEnoughInfoToRate = False
                                sMissing += "InvalidStatedValueAmount:Veh " & oVeh.IndexNum & "-"
                            End If
                        End If
                    End If

                    'Coverages
                    If oVeh.Coverages.Count < 1 Then
                        bEnoughInfoToRate = False
                        sMissing += "Coverages:Veh " & oVeh.IndexNum & "-"
                    End If
                End If
            Next

            'Policy Insured
            If Not oPolicy.PolicyInsured Is Nothing Then
                With oPolicy.PolicyInsured
                    'MaritalStatus
                    If .MaritalStatus = "" Then
                        bEnoughInfoToRate = False
                        sMissing += "MaritalStatus" & "-"
                    End If

                    'Age
                    If IsNumeric(.Age) Then
                        If .Age < 10 Then
                            bEnoughInfoToRate = False
                            sMissing += "Age" & "-"
                        End If
                    Else
                        bEnoughInfoToRate = False
                        sMissing += "Age" & "-"
                    End If

                    If .PriorLimitsCode = "" Then
                        bEnoughInfoToRate = False
                        sMissing += "PriorLimitsCode" & "-"
                    End If

                End With
            Else
                bEnoughInfoToRate = False
                sMissing += "PolicyInsured" & "-"
            End If

            'PayPlan
            If oPolicy.PayPlanCode = "" Then
                bEnoughInfoToRate = False
                sMissing += "PayPlanCode" & "-"
            End If

            If oPolicy.CallingSystem <> "PAS" And oPolicy.CallingSystem <> "AOLE" And oPolicy.CallingSystem <> "UWOLE" Then
                If oPolicy.LienHolders.Count > 0 Then
                    For Each oLienHolder As clsEntityLienHolder In oPolicy.LienHolders
                        If oLienHolder.EntityType = "PFC" Then
                            If oPolicy.PayPlanCode <> "100" Then
                                bEnoughInfoToRate = False
                                sMissing += "PremiumFinanceCompany" & "-"
                            End If
                            Exit For
                        End If
                    Next
                End If
            End If


            Dim bHasActiveDrivers As Boolean = False
            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If oDriver.DriverStatus.ToUpper.Trim = "ACTIVE" Then
                    bHasActiveDrivers = True
                    Exit For
                End If
            Next

            If Not bHasActiveDrivers Then
                sMissing += "NoActiveDrivers" & "-"
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
        End Try
    End Function

#Region "IER Functions"

    Public Overridable Sub CheckActualRateDate(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            If oPolicy.ActualRateDate <> Date.MinValue Then
                If oPolicy.ActualRateDate.AddDays(15) < Now() Then
                    oPolicy.ActualRateDate = Now()
                    .Notes = (AddNote(.Notes, "Information Updated: Quote is over 15 days old, prior rate is no longer valid and has been updated.", "ActualRateDate", "RPT", .Notes.Count))
                End If
            End If
        End With


    End Sub

    Public Overridable Sub CheckPolicyPoints(ByVal oPolicy As clsPolicyPPA)
        Dim iTotalPoints As Integer = 0
        With oPolicy
            For Each oDrv As clsEntityDriver In .Drivers
                If oDrv.DriverStatus.ToUpper = "ACTIVE" Or oDrv.DriverStatus.ToUpper = "PERMITTED" And oDrv.DriverStatus.ToUpper = "EXCLUDED" And Not oDrv.IsMarkedForDelete Then
                    iTotalPoints += oDrv.Points
                End If
            Next

            If iTotalPoints > 2 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Policy is ineligible based on the number of driver violation points", "PolPointsover2", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckRoutingNumbers(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            For Each oAccount As clsBaseAccount In .Accounts
                If Len(oAccount.RoutingNum) > 0 Then
                    If Len(oAccount.RoutingNum) <> 9 Then

                    End If
                End If
            Next
        End With
    End Sub

    Public Sub CheckDLDupes(ByVal oPolicy As clsPolicyPPA)
        Dim listDLN As New List(Of String)
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If oDriver.DLN.Length > 0 Then
                'see if the DLN is already in the collection
                If listDLN.Contains(oDriver.DLN.ToUpper) Then
                    'throw error
                    oPolicy.Notes = (AddNote(oPolicy.Notes, String.Format("Ineligible Risk: Driver's License Number is duplicated on two or more drivers."), "DLNDuplicate", "IER", oPolicy.Notes.Count))
                    Exit For
                Else
                    ' Do not add if DLN = "UNKNOWN" as this is the default DLN set by ExternalRptService
                    If oDriver.DLN.ToUpper.Trim <> "UNKNOWN" Then
                        'add the DLN to the collection for further checking
                        listDLN.Add(oDriver.DLN.ToUpper)
                    End If
                End If
            End If
        Next
    End Sub

    Public Sub CheckDLRestrictionTable(ByVal oPolicy As clsPolicyPPA)
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If oDriver.DriverStatus.ToUpper = "ACTIVE" Or oDriver.DriverStatus.ToUpper = "PERMITTED" And Not oDriver.IsMarkedForDelete Then
                If IsRestricted(oDriver.DLN, oDriver.DLNState) Then
                    oPolicy.Notes = (AddNote(oPolicy.TransactionNum, oPolicy.Notes, "Ineligible Risk: Driver is an unacceptable risk.  Driver- " & oDriver.IndexNum & ".", "RestrictedDLN", "IER", oPolicy.Notes.Count))

                    If Len(oPolicy.PolicyID) > 0 Then
                        AddPolicyNote(oPolicy, "Ineligible Risk: Driver is an unacceptable risk.  Driver- " & oDriver.IndexNum & ".", "RestrictedDLN")
                    End If
                End If
            End If
        Next
    End Sub

    Public Sub CheckDLPattern(ByVal oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        Dim DLNSvc As New DLNService.DLNService
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If oDriver.DriverStatus.ToUpper = "ACTIVE" And Not oDriver.IsMarkedForDelete Then
                    If oDriver.DLN.Length > 0 And oDriver.DLNState.Length > 0 Then
                        Dim sResult As String = String.Empty

                        sResult = DLNSvc.ValidateDLNFormat(oDriver.DLN.ToUpper(), oDriver.DLNState)
                        If sResult.Contains("Invalid") Then
                            oPolicy.Notes = (AddNote(oPolicy.Notes, String.Format("Ineligible Risk: Invalid Driver's License Number Format for {0} {1}: {2}.", oDriver.EntityName1, oDriver.EntityName2, sResult), "DLNFormat", "IER", oPolicy.Notes.Count))
                        Else
                            ' If it passed the validation, check to see that it isn't all the same number or in a sequence
                            ' I.E. 111111111 or 123456789
                            Dim bIsValid As Boolean = False
                            bIsValid = CheckInvalidDLN(oDriver.DLN.ToUpper())

                            If bIsValid Then
                                oDriver.DLN = sResult
                            Else
                                oPolicy.Notes = (AddNote(oPolicy.Notes, String.Format("Ineligible Risk: Invalid Driver's License Number (failed validation) for {0} {1}: {2}.", oDriver.EntityName1, oDriver.EntityName2, sResult), "DLNInvalid", "IER", oPolicy.Notes.Count))
                            End If
                        End If
                    Else
                        If oPolicy.Status > 3 Then
                            'oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Missing DLN on Driver-", oDriver.IndexNum, "MissingDLN", "IER", oPolicy.Notes.Count))
                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Missing Driver's License Number on Driver-" + oDriver.IndexNum.ToString, "MissingDLN", "IER", oPolicy.Notes.Count))
                        End If
                    End If
                End If
            Next
        End If
    End Sub

    Private Function CheckInvalidDLN(ByVal sDLN As String) As Boolean
        Dim bIsValid As Boolean = True


        Dim bIsSequence As Boolean = False

        Dim sPreviousChar As String = String.Empty
        Dim iSameCount As Integer = 0
        Dim iSequenceCount As Integer = 0
        For i As Integer = 0 To sDLN.Length - 1
            If sPreviousChar = String.Empty Then
                sPreviousChar = sDLN(i)
            Else
                If sPreviousChar = sDLN(i) Then
                    iSameCount = iSameCount + 1
                Else
                    iSameCount = 0
                    If IsNumeric(sPreviousChar) AndAlso IsNumeric(sDLN(i)) AndAlso CInt(sDLN(i).ToString) = CInt(sPreviousChar) + 1 Then
                        iSequenceCount = iSequenceCount + 1
                    ElseIf Not (iSequenceCount >= sDLN.Length - 2 Or iSequenceCount > 5) Then
                        iSequenceCount = 0
                    End If
                    sPreviousChar = sDLN(i)
                End If
            End If
        Next


        If Not (iSequenceCount >= sDLN.Length - 2 Or iSequenceCount > 5) Then
            iSequenceCount = 0
            sPreviousChar = String.Empty

            For i As Integer = sDLN.Length - 1 To 1 Step -1
                If sPreviousChar = String.Empty Then
                    sPreviousChar = sDLN(i)
                Else
                    If IsNumeric(sPreviousChar) AndAlso IsNumeric(sDLN(i)) AndAlso CInt(sDLN(i).ToString) = CInt(sPreviousChar) + 1 Then
                        iSequenceCount = iSequenceCount + 1
                    ElseIf Not (iSequenceCount >= sDLN.Length - 2 Or iSequenceCount > 5) Then
                        iSequenceCount = 0
                    End If
                    sPreviousChar = sDLN(i)
                End If
            Next
        End If

        If iSameCount >= sDLN.Length - 2 Or iSameCount > 5 Then
            bIsValid = False
        End If

        If iSequenceCount >= sDLN.Length - 2 Or iSequenceCount > 5 Then
            bIsValid = False
        End If

        Return bIsValid
    End Function

    Public Overridable Sub CheckOutOfStateZip(ByRef oPolicy As clsPolicyPPA)
        ' Check to see if out of state is allowed
        Dim bAllowOutOfState As Boolean = True
        Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        Dim DataRows() As DataRow
        DataRows = oStateInfoDataSet.Tables(0).Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup = 'VEHICLE' AND ItemCode = 'TERRITORY' AND ItemSubCode='ALLOWOUTOFSTATE' ")

        For Each oRow As DataRow In DataRows
            If oRow.Item("ItemValue").ToString.ToUpper = "FALSE" Then
                bAllowOutOfState = False
                Exit For
            Else
                bAllowOutOfState = True
                Exit For
            End If
        Next

        ' if not allowed, validate that zip is in the territorydefinitions table
        Dim sVehicleList As String = String.Empty
        If Not bAllowOutOfState Then
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVehicle.IsMarkedForDelete Then
                    If Not ValidateVehicleZipCode(oVehicle.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                        If Len(sVehicleList) = 0 Then
                            sVehicleList = oVehicle.IndexNum
                        Else
                            sVehicleList = sVehicleList & "," & oVehicle.IndexNum
                        End If
                    End If
                End If
            Next
        End If

        If Len(sVehicleList) > 0 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an out of state garaging zip - " & sVehicleList & ".", "OutOfStateZip", "IER", oPolicy.Notes.Count, "AOLE"))
        End If
    End Sub

    Public Overridable Function ValidateVehicleZipCode(ByVal sZip As String, ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As Boolean
        Dim bIsValid As Boolean = False

        Dim sSql As String = ""
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oReader As SqlDataReader

        oConn.Open()
        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT * "
                sSql = sSql & " FROM pgm" & sProduct & sStateCode & "..CodeTerritoryDefinitions with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Zip = @Zip "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode
                cmd.Parameters.Add("@Zip", SqlDbType.VarChar, 5).Value = sZip.Trim

                oReader = cmd.ExecuteReader()
                Return oReader.HasRows()

            End Using
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Return bIsValid
    End Function

    Public Overridable Sub CheckVehicleCount(ByRef oPolicy As clsPolicyPPA)
        With oPolicy
            If .VehicleCount(True) < 1 Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: Must have at least one active vehicle.", "VehicleCount", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Sub CheckPhysicalDamageWeather(ByRef oPolicy As clsPolicyPPA)

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVeh, oPolicy) Then
                If DeterminePhysDamageExists(oVeh) Then
                    Dim bContinue As Boolean = True
                    Dim iZipCode As Integer

                    Try
                        iZipCode = Int32.Parse(oPolicy.PolicyInsured.Zip)

                        If iZipCode > 99999 Then
                            bContinue = False
                            With oPolicy
                                .Notes = (AddNote(.Notes, "Ineligible Risk: Invalid Policy Insured Zip Code.", "InvalidZip", "IER", .Notes.Count))
                            End With
                        End If
                    Catch ex As Exception
                        bContinue = False
                        With oPolicy
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Invalid Policy Insured Zip Code.", "InvalidZip", "IER", .Notes.Count))
                        End With
                    End Try

                    If bContinue Then
                        Try
                            Dim oWeather As New Weather(oPolicy.PolicyInsured.Zip)
                            If (Not oWeather.checkWeather) Or WeatherOverride(oPolicy) Or OverrideWeatherByCounty(oPolicy) Or OverrideWeatherByZIP(oPolicy) Then
                                If Not CBool(ConfigurationManager.AppSettings("IsTest")) Then
                                    With oPolicy
                                        .Notes = (AddNote(.Notes, "Ineligible Risk: Imperial is unable to bind policies with Physical Damage coverage during a severe weather event.  This includes, but is not limited to, tropical storm warning, hurricane warning, and ice storms.", "TropicalStorm", "IER", .Notes.Count))
                                    End With
                                End If
                            End If
                        Catch ex As Exception
                            If ex.Message.Contains("Error with one or more zip codes") Then
                                ' do nothing
                            Else
                                Throw New Exception("Error in CheckPhysicalDamageWeather", ex)
                            End If
                        End Try

                    End If

                    'If oWeather.HasWinterStormWarning() Then
                    '    With oPolicy
                    '        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot bind Physical Damage policies during a winter storm warning.", "WinterStorm", "IER", .Notes.Count))
                    '    End With
                    'End If

                    Exit For
                End If
            End If
        Next
    End Sub

    Private Function WeatherOverride(ByVal opolicy As clsPolicyPPA) As Boolean

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
    Private Function OverrideWeatherByCounty(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bHasOverride As Boolean = False

        Dim sValue As String = GetStateInfoValue(oPolicy, oPolicy.Program, "WEATHEROVERRIDE", "COUNTY", oPolicy.PolicyInsured.County.Trim.ToUpper)
        If sValue = "TRUE" Then
            bHasOverride = True
        End If

        Return bHasOverride

    End Function
    Private Function OverrideWeatherByZIP(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bHasOverride As Boolean = False

        Dim sValue As String = GetStateInfoValue(oPolicy, oPolicy.Program, "WEATHEROVERRIDE", "ZIP", oPolicy.PolicyInsured.Zip.Trim)
        If sValue = "TRUE" Then
            bHasOverride = True
        End If

        Return bHasOverride

    End Function
    ' Physical Damage Restriction 2 (Any vehicle over 15 years old.)
    Public Overridable Sub CheckPhysicalDamageRestriction(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sVehicle As String

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
    End Sub

    Public Overridable Function CheckPhysicalDamageRestriction(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleID As String = ""

        Dim bPhysDamage As Boolean = DeterminePhysDamageExists(oVehicle)
        If bPhysDamage Then
            If Not oVehicle.IsMarkedForDelete Then
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
        End If
        Return sVehicleID
    End Function

    Public Overridable Sub CheckNamedInsuredActive(ByRef oPolicy As clsPolicyPPA)
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sResult As String = ""
                sResult = CheckNamedInsuredActive(oDrv)

                If Len(sResult) > 0 Then
                    If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The Policyholder must be either Active or Excluded.", "InsuredDriverStatus", "IER", oPolicy.Notes.Count))
                    Else
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The named insured must be either Active or Excluded.", "InsuredDriverStatus", "IER", oPolicy.Notes.Count))
                    End If
                End If
            End If
        Next
    End Sub

    Public Overridable Function CheckNamedInsuredActive(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        If oDrv.RelationToInsured.ToUpper = "SELF" Then
            'must be Active or Excluded
            If oDrv.DriverStatus <> "" Then
                If oDrv.DriverStatus.ToUpper = "PERMITTED" Or oDrv.DriverStatus.ToUpper = "NHH" Then
                    'not allowed
                    If oNoteList Is Nothing Then
                        Return "True"
                    Else
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The named insured must be either Active or Excluded.", "InsuredDriverStatus", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return ""
    End Function

    Public Overridable Sub CheckDriverNamesEntered(ByRef oPolicy As clsPolicyPPA)
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                Dim sResult As String = ""
                sResult = CheckDriverNameEntered(oDrv)
                If Len(sResult) > 0 Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Driver " & sResult & " must have both first and last names entered.", "DriverNamesProvided", "IER", oPolicy.Notes.Count))
                End If
            End If
        Next
    End Sub

    Public Overridable Function CheckDriverNameEntered(ByRef oDrv As clsEntityDriver, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        If (oDrv.DriverStatus.ToUpper <> "NHH") Then
            If oDrv.EntityName1.Contains(" ") Then
                If oDrv.EntityName2.Length > 1 Then
                    oDrv.EntityName2 = oDrv.EntityName1.Substring(oDrv.EntityName1.IndexOf(" ") + 1) & " " & oDrv.EntityName2
                Else
                    oDrv.EntityName2 = oDrv.EntityName1.Substring(oDrv.EntityName1.IndexOf(" ") + 1)
                End If
                oDrv.EntityName1 = oDrv.EntityName1.Split(" ")(0)
            End If
            If oDrv.EntityName1 = "" Or oDrv.EntityName2 = "" Or oDrv.EntityName1 = " " Or oDrv.EntityName2 = " " Or oDrv.EntityName1.Contains("undefined") Or oDrv.EntityName2.Contains("undefined") Then
                If oNoteList Is Nothing Then
                    Return oDrv.IndexNum
                Else
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: Driver " & oDrv.IndexNum & " must have both first and last names entered.", "DriverNamesProvided", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return ""
    End Function

    Public Overridable Sub CheckPermittedNotExcluded(ByVal oPolicy)
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

    Public Overridable Sub CheckVehicleStatedValue(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckVehicleStatedValue(oVehicle)

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

    Public Overridable Function CheckVehicleStatedValue(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        Dim i As Integer = -1
        Int32.TryParse(oVehicle.VehicleSymbolCode, i)

        If i > 0 Then
            If (oVehicle.VehicleYear < 2011 And (oVehicle.VehicleSymbolCode = 66 Or oVehicle.VehicleSymbolCode = 67 Or oVehicle.VehicleSymbolCode = 68)) Or (oVehicle.VehicleYear >= 2011 And (oVehicle.VehicleSymbolCode = 966 Or oVehicle.VehicleSymbolCode = 967 Or oVehicle.VehicleSymbolCode = 968)) Then
                If oVehicle.StatedAmt < 500 Or oVehicle.StatedAmt > 60000 Then
                    sVehicleList = oVehicle.IndexNum
                    If Not oNoteList Is Nothing Then
                        oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) do not have a valid stated value amount (It must be between $500 and $60,000) -  " & sVehicleList & ".", "InvalidStatedValue", "IER", oNoteList.Count, "AOLE"))
                        Return ""
                    End If
                End If
            End If
        End If

        Return sVehicleList
    End Function

    Public Overridable Sub CheckVehicleComplete(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""
                sVeh = CheckVehicleComplete(oVehicle)

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
            If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an Invalid VIN. (Code 092008) -  " & sVehicleList & ".", "IncompleteVehicle", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) are incomplete vehicles -  " & sVehicleList & ".", "IncompleteVehicle", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckVehicleComplete(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        If oVehicle.IncompleteVehicle Then
            sVehicleList = oVehicle.IndexNum

            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) are incomplete vehicles -  " & sVehicleList & ".", "IncompleteVehicle", "IER", oNoteList.Count, "AOLE"))
                Return ""
            End If
        End If

        Return sVehicleList
    End Function

    Public Overridable Sub CheckLienholderType(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim bLienTypeRequired As Boolean = False
                Dim sLienList As String = String.Empty
                For Each oLienHolder As clsEntityLienHolder In oVehicle.LienHolders
                    If oLienHolder.EntityType = "AN" Then
                        oLienHolder.EntityType = "AI"
                    End If
                    If oLienHolder.EntityType = String.Empty Then
                        bLienTypeRequired = True
                        If sLienList = String.Empty Then
                            sLienList = oLienHolder.EntityName1
                        Else
                            sLienList &= ", " & oLienHolder.EntityName1
                        End If
                    End If
                Next
                If bLienTypeRequired Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = "(" & oVehicle.IndexNum & ") " & sLienList
                    Else
                        sVehicleList &= "; (" & oVehicle.IndexNum & ") " & sLienList
                    End If
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following lienholders(s) must have a lienholder type selected - " & sVehicleList & ".", "LienTypeRequired", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckActiveDriverDOB(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = ""

        sDriverList = ""
        Dim bHasActiveDrv As Boolean = False
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then
                If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                    bHasActiveDrv = True
                    If oDrv.DOB = #12:00:00 AM# Or oDrv.DOB = #1/1/1900# Then
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
            If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Driver(s)  " & sDriverList & " do not have a valid Date of Birth.", "InvalidDOB", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Driver(s)  " & sDriverList & " are listed as Active but do not have a valid Date of Birth.", "InvalidDOB", "IER", oPolicy.Notes.Count))
            End If
        End If
        If Not bHasActiveDrv Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The policy must contain at least one Active driver.", "NoActiveDrv", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckPayPlan(ByRef oPolicy As clsPolicyPPA)
        If Not ValidatePayPlan(oPolicy) Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The policy has an invalid pay plan. Please make sure a valid pay plan is selected.", "InvalidPayPlan", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckDriverDisclosure(ByRef oPolicy As clsPolicyPPA)
        With oPolicy
            If .UWQuestions.Count > 0 Then
                For Each oUWQ As clsUWQuestion In .UWQuestions
                    Select Case oUWQ.QuestionCode
                        Case "300"
                            If Left(oUWQ.AnswerText.ToUpper.Trim, 2) = "NO" Then
                                If oPolicy.Program.ToUpper = "DIRECT" Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: All household residents and vehicle operators age 14 or older MUST BE listed on the application.  Please review Question #1 on the Additional Information page.", "DriverDisclosure", "IER", .Notes.Count))
                                Else
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: All household residents and vehicle operators age 14 or older MUST BE listed on the application as either an active or an excluded driver.", "DriverDisclosure", "IER", .Notes.Count))
                                End If
                            End If
                    End Select
                Next
            End If
        End With
    End Sub

    Public Overridable Sub CheckPhysicalDamageWithLienholder(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty

        sVehicleList = String.Empty
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                Dim sVeh As String = ""

                If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.StateCode = "17" Then
                    sVeh = CheckPhysicalDamageWithNoUMPDWithLienholder(oVehicle)
                Else
                    sVeh = CheckPhysicalDamageWithLienholder(oVehicle)
                End If

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
            If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a Lien holder/Leasing company listed without physical damage coverage.  Physical Damage coverage is required if a vehicle is Leased or Financed. - " & sVehicleList & ".", "LienWOpd", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an Additional Insured or a Loss Payee listed without physical damage coverage - " & sVehicleList & ".", "LienWOpd", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckPhysicalDamageWithNoUMPDWithLienholder(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        Dim bHasPhysicalDamage As Boolean = False
        Dim bLienExists As Boolean = False

        If DeterminePhysDamageWithNoUMPDExists(oVehicle) Then
            bHasPhysicalDamage = True
        End If

        For Each oLienHolder As clsEntityLienHolder In oVehicle.LienHolders
            If oLienHolder.EntityType = "AI" Or oLienHolder.EntityType = "LP" Then
                bLienExists = True
                Exit For
            End If
        Next

        If bLienExists And Not bHasPhysicalDamage Then
            sVehicleList = oVehicle.IndexNum
            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have an Additional Insured or a Loss Payee listed without physical damage coverage - " & sVehicleList & ".", "LienWOpd", "IER", oNoteList.Count, "AOLE"))
            End If
        End If
        Return sVehicleList
    End Function

    Public Overridable Function CheckPhysicalDamageWithLienholder(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        Dim bHasPhysicalDamage As Boolean = False
        Dim bLienExists As Boolean = False

        If DeterminePhysDamageExists(oVehicle) Then
            bHasPhysicalDamage = True
        End If


        For Each oLienHolder As clsEntityLienHolder In oVehicle.LienHolders
            If oLienHolder.EntityType = "AI" Or oLienHolder.EntityType = "LP" Then
                bLienExists = True
                Exit For
            End If
        Next

        If bLienExists And Not bHasPhysicalDamage Then
            sVehicleList = oVehicle.IndexNum
            If Not oNoteList Is Nothing Then
                oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) have an Additional Insured or a Loss Payee listed without physical damage coverage - " & sVehicleList & ".", "LienWOpd", "IER", oNoteList.Count, "AOLE"))
            End If
        End If
        Return sVehicleList
    End Function

    Public Overridable Sub CheckLeasedVehHasLienholder(ByRef oPolicy As clsPolicyPPA)
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            Dim sVehicleList As String = String.Empty

            sVehicleList = String.Empty
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If VehicleApplies(oVehicle, oPolicy) Then
                    Dim sVeh As String = ""
                    sVeh = CheckLeasedVehHasLienholder(oVehicle)
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
                If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) are required to have a Lien Holder/Leasing Company - " & sVehicleList & ".", "LeasedVehWOAddlInsured", "IER", oPolicy.Notes.Count))
                Else
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) do not have an Additional Insured or a Loss Payee listed - " & sVehicleList & ".", "LeasedVehWOAddlInsured", "IER", oPolicy.Notes.Count))
                End If
            End If
        End If
    End Sub

    Public Overridable Function CheckLeasedVehHasLienholder(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""
        Dim bLeasedVehicle As Boolean = False
        Dim bLienExists As Boolean = False

        For Each oCov As clsBaseCoverage In oVehicle.Coverages
            If oCov.CovCode.Contains("LLS") And Not oCov.IsMarkedForDelete Then
                bLeasedVehicle = True
                Exit For
            End If
        Next
        If bLeasedVehicle Then
            For Each oLienHolder As clsEntityLienHolder In oVehicle.LienHolders
                If oLienHolder.EntityType = "AI" Or oLienHolder.EntityType = "LP" Then
                    bLienExists = True
                    Exit For
                End If
            Next
            If Not bLienExists Then
                sVehicleList = oVehicle.IndexNum
                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) do not have an Additional Insured or a Loss Payee listed - " & sVehicleList & ".", "LeasedVehWOAddlInsured", "IER", oNoteList.Count, "AOLE"))
                End If
            End If
        End If
        Return sVehicleList
    End Function

    Public Sub CheckMissingVIN(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty
        Dim sMaxLengthVehicleList As String = String.Empty
        Dim sMinLengthVehicleList As String = String.Empty
        Dim sIncompleteVehicleList As String = String.Empty

        'Dim drIncompleteVeh As DataRow()
        Dim oSvc As New CommonRulesFunctions
        Dim CodeXRefTable As DataTable
        Dim oVINSvc As New ImperialFire.VINService

        Dim oCodeXRefDataSet As DataSet = oSvc.LoadCodeXRefTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
        CodeXRefTable = oCodeXRefDataSet.Tables(0)


        sVehicleList = String.Empty

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVehicle, oPolicy) Then
                oVehicle.VinNo = oVehicle.VinNo.Trim

                If oVehicle.VinNo = String.Empty Or oVehicle.VinNo Is Nothing Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList &= ", " & oVehicle.IndexNum
                    End If
                ElseIf oVehicle.VinNo.Length > 17 Then
                    If sMaxLengthVehicleList = String.Empty Then
                        sMaxLengthVehicleList = oVehicle.IndexNum
                    Else
                        sMaxLengthVehicleList &= ", " & oVehicle.IndexNum
                    End If
                ElseIf oVehicle.VinNo.Length < 17 Then
                    If Not oVehicle.StatedAmt > 0 And oVehicle.VinNo.ToUpper <> "NONOWNER" Then
                        If sMinLengthVehicleList = String.Empty Then
                            sMinLengthVehicleList = oVehicle.IndexNum
                        Else
                            sMinLengthVehicleList &= ", " & oVehicle.IndexNum
                        End If
                    End If
                Else
                    'drIncompleteVeh = CodeXRefTable.Select("Source='VINSERVICE' AND CodeType = 'VEHICLE' AND Code = 'INCOMPLETE' " & _
                    '   " AND MappingCode1 = '" & oVehicle.VinNo.Substring(0, 3) & "' AND MappingCode2 = '" & oVehicle.VehicleMakeCode & "'")

                    oVehicle.IncompleteVehicle = oVINSvc.CheckIfIncompleteVehicle(oVehicle.VinNo, oVehicle.VehicleMakeCode)

                    If oVehicle.IncompleteVehicle Then
                        If sIncompleteVehicleList = String.Empty Then
                            sIncompleteVehicleList = oVehicle.IndexNum
                        Else
                            sIncompleteVehicleList &= ", " & oVehicle.IndexNum
                        End If
                    End If
                End If
            End If
        Next

        If sIncompleteVehicleList <> String.Empty Then
            If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an Invalid VIN. (Code 092008) -  " & sIncompleteVehicleList & ".", "MaxLenVIN", "IER", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a VIN which signifies an incomplete vehicle -  " & sIncompleteVehicleList & ".", "MaxLenVIN", "IER", oPolicy.Notes.Count))
            End If
        End If

        If sMaxLengthVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a VIN over 17 characters long -  " & sMaxLengthVehicleList & ".", "MaxLenVIN", "IER", oPolicy.Notes.Count))
        End If

        If sMinLengthVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have a VIN under 17 characters long -  " & sMinLengthVehicleList & ".", "MinLenVIN", "IER", oPolicy.Notes.Count))
        End If

        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) do not have a VIN -  " & sVehicleList & ".", "MissingVIN", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Overridable Sub CheckGaragingZip(ByRef oPolicy As clsPolicyPPA)
        Dim bDiffGaragingZip As Boolean = False
        Dim bUWQAnswered As Boolean = False
        Dim sVehicleList As String = String.Empty

        If Not oPolicy.CallingSystem = "BRIDGE" Then
            sVehicleList = String.Empty
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If VehicleApplies(oVehicle, oPolicy) Then
                    If oVehicle.Zip <> oPolicy.PolicyInsured.Zip And oVehicle.VinNo <> "NONOWNER" Then
                        bDiffGaragingZip = True
                        If sVehicleList = String.Empty Then
                            sVehicleList = oVehicle.IndexNum
                        Else
                            sVehicleList &= ", " & oVehicle.IndexNum
                        End If
                    End If
                    If bDiffGaragingZip Then
                        For Each oUWQuestion As clsUWQuestion In oPolicy.UWQuestions
                            If oUWQuestion.QuestionCode = "306" Then
                                If oUWQuestion.AnswerText.ToUpper.Contains("NO;") And oUWQuestion.AnswerText.Length > 4 Then
                                    bUWQAnswered = True
                                End If
                            End If
                        Next
                    End If
                End If
            Next

            If sVehicleList <> String.Empty Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: The garaging Zip Code(s) for the following vehicle(s) do not match the Policy Address.  Please correct the Zip Code or contact Imperial for approval -  " & sVehicleList & ".", "InvalidGaragingZip", "UWW", oPolicy.Notes.Count))
            End If

            If sVehicleList <> String.Empty And Not bUWQAnswered Then
                If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) are not garaged at the policy address. The address must be entered for the vehicle(s) under question #7 on the Purchase Screen -  " & sVehicleList & ".", "InvalidGaragingZip", "IER", oPolicy.Notes.Count))
                Else
                    If oPolicy.StateCode = "09" Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) are not garaged at the policy address. The address must be entered for the vehicle(s) under #18 in the Additional Information section -  " & sVehicleList & ".", "InvalidGaragingZip", "IER", oPolicy.Notes.Count))
                    Else
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) are not garaged at the policy address. The address must be entered for the vehicle(s) under #7 in the Additional Information section -  " & sVehicleList & ".", "InvalidGaragingZip", "IER", oPolicy.Notes.Count))
                    End If
                End If
            End If
        End If
    End Sub

    Public Overridable Sub CheckMarried(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.VehicleUnits.Count > 0 Then
            If Not oPolicy.VehicleUnits(0).IsMarkedForDelete Then
                If oPolicy.VehicleUnits(0).VinNo.ToUpper <> "NONOWNER" Then
                    Dim bMarredNIWithSpouse As Boolean = False
                    For Each oDriver As clsEntityDriver In oPolicy.Drivers
                        If Not oDriver.IsMarkedForDelete Then
                            If oDriver.RelationToInsured.ToUpper = "SELF" Then
                                If oDriver.MaritalStatus.ToUpper = "MARRIED" Then
                                    For Each oDrv As clsEntityDriver In oPolicy.Drivers
                                        If Not oDrv.IsMarkedForDelete Then
                                            If oDrv.RelationToInsured.ToUpper = "SPOUSE" Then
                                                bMarredNIWithSpouse = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                    If Not bMarredNIWithSpouse Then
                                        If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Policyholder is listed as married.  Spouse must be listed on the application as Active or Excluded.", "MarriedWithoutSpouse", "IER", oPolicy.Notes.Count))
                                        Else
                                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Named Insured is listed as married.  Spouse must be listed on the application as active or excluded.", "MarriedWithoutSpouse", "IER", oPolicy.Notes.Count))
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next

                    If GetNote(oPolicy, "IER", "MarriedWithoutSpouse") Is Nothing Then
                        Dim iMarriedDrivers As Integer = 0
                        Dim iNewMarriedDrivers As Integer = 0
                        Dim iMaleMarried As Integer = 0
                        Dim iFemaleMarried As Integer = 0
                        Dim isOneDriveActive As Boolean = False
                        For Each oDriver As clsEntityDriver In oPolicy.Drivers
                            If Not oDriver.IsMarkedForDelete Then
                                If oDriver.MaritalStatus.ToUpper = "MARRIED" Then
                                    iMarriedDrivers += 1
                                    If oDriver.IsNew Then
                                        iNewMarriedDrivers += 1
                                    End If
                                    If oDriver.Gender.ToUpper.StartsWith("M") Then
                                        iMaleMarried += 1
                                    Else
                                        iFemaleMarried += 1
                                    End If
                                    If oDriver.DriverStatus.ToUpper.Trim = "ACTIVE" Then
                                        isOneDriveActive = True
                                    End If
                                End If
                            End If
                        Next
                        If (oPolicy.CallingSystem.Contains("OLE") Or oPolicy.CallingSystem.ToUpper.Contains("UWC")) Then
                            If iMarriedDrivers Mod 2 <> 0 And iNewMarriedDrivers <> 0 Then
                                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: There is an uneven number of married drivers on the policy.", "UnevenMarriedDrivers", "IER", oPolicy.Notes.Count))
                            End If
                        Else
                            If iMarriedDrivers Mod 2 <> 0 Or (iMaleMarried <> iFemaleMarried And isOneDriveActive) Then
                                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: There is an uneven number of married drivers on the policy.", "UnevenMarriedDrivers", "IER", oPolicy.Notes.Count))
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Sub

    Public Overridable Sub CheckSR22Date(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        sDriverList = String.Empty
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then

                If oDrv.SR22 And (oDrv.SR22Date < CDate("1/1/1950")) Then
                    If sDriverList = String.Empty Then
                        sDriverList = oDrv.IndexNum
                    Else
                        sDriverList &= ", " & oDrv.IndexNum
                    End If
                End If

            End If
        Next
        If sDriverList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following drivers(s) do not have a valid SR22 date - " & sDriverList & ".", "SR22Date", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckSR22CaseCode(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        sDriverList = String.Empty
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then

                If oDrv.SR22 And (String.IsNullOrEmpty(oDrv.SR22CaseCode) OrElse oDrv.SR22CaseCode = "EnterCaseCodeHere") Then
                    If sDriverList = String.Empty Then
                        sDriverList = oDrv.IndexNum
                    Else
                        sDriverList &= ", " & oDrv.IndexNum
                    End If
                End If

            End If
        Next
        If sDriverList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following drivers(s) do not have a valid SR-22 Case Code - " & sDriverList & ".", "SR22Case", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckSR22Excluded(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty

        sDriverList = String.Empty
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDrv, oPolicy) Then

                If oDrv.SR22 And oDrv.DriverStatus.ToUpper() = "EXCLUDED" Then
                    If sDriverList = String.Empty Then
                        sDriverList = oDrv.IndexNum
                    Else
                        sDriverList &= ", " & oDrv.IndexNum
                    End If
                End If

            End If
        Next
        If sDriverList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Drivers requesting an SR22 filing must be Active.", "SR22Excluded", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckRentToOwnVehHasLienholder(ByRef oPolicy As clsPolicyPPA)
        If Not oPolicy.CallingSystem = "BRIDGE" Then
            Dim sVehicleList As String = String.Empty

            sVehicleList = String.Empty
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If VehicleApplies(oVehicle, oPolicy) Then
                    Dim sVeh As String = ""
                    sVeh = CheckRentToOwnVehHasLienholder(oVehicle)
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
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) do not have an Additional Insured or a Loss Payee listed - " & sVehicleList & ".", "RentToOwnWOAddlInsured", "IER", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Function CheckRentToOwnVehHasLienholder(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
        Dim sVehicleList As String = ""

        Dim bRentToOwnVeh As Boolean = False
        Dim bLienExists As Boolean = False
        If DetermineVehicleFactorExists(oVehicle, "RENT_TO_OWN") Then
            bRentToOwnVeh = True
        End If
        If bRentToOwnVeh Then
            For Each oLienHolder As clsEntityLienHolder In oVehicle.LienHolders
                If oLienHolder.EntityType = "AI" Or oLienHolder.EntityType = "LP" Then
                    bLienExists = True
                    Exit For
                End If
            Next
            If Not bLienExists Then
                sVehicleList = oVehicle.IndexNum

                If Not oNoteList Is Nothing Then
                    oNoteList = (AddNote(oNoteList, "Ineligible Risk: The following vehicle(s) do not have an Additional Insured or a Loss Payee listed - " & sVehicleList & ".", "RentToOwnWOAddlInsured", "IER", oNoteList.Count, "AOLE"))
                    Return ""
                End If
            End If
        End If

        Return sVehicleList
    End Function

    Public Overridable Sub CheckCustomEquipmentLimits(ByRef oPolicy As clsPolicyPPA)
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

    Public Overridable Function CheckCustomEquipmentLimits(ByRef oVehicle As clsVehicleUnit, Optional ByVal sProgram As String = "", Optional ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote) = Nothing) As String
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

        Return sVehicleList
    End Function

    Public Overridable Sub CheckSR22Term(ByRef oPolicy As clsPolicyPPA)
        If oPolicy.Term = "1" Then
            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                If DriverApplies(oDrv, oPolicy) Then
                    If oDrv.SR22 Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Policy Term must be greater than 1 month to have an SR22.", "OneTermWithSR22", "IER", oPolicy.Notes.Count))
                    End If
                    Exit For
                End If
            Next
        End If
    End Sub

    Public Overridable Sub CheckVehicleBusinessUse(ByRef oPolicy As clsPolicyPPA)

        If oPolicy.Program.ToUpper = "SUMMIT" Then
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
    End Sub

    Public Overridable Sub CheckInsuredAddress(ByVal oPolicy As clsPolicyPPA)
        Dim isValid As Boolean = True
        With oPolicy
            If Not .PolicyInsured.Address1.Trim.Length > 1 Then
                If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Address cannot be blank", "InsuredAddress1", "IER", .Notes.Count))
                Else
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Address1 cannot be blank", "InsuredAddress1", "IER", .Notes.Count))
                End If
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
                    If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Mailing Address cannot be blank", "InsuredMailingAddress1", "IER", .Notes.Count))
                    Else
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Insured Mailing Address1 cannot be blank", "InsuredMailingAddress1", "IER", .Notes.Count))
                    End If
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

    Public Overridable Sub CheckLienholderState(ByVal oPolicy As clsPolicyPPA)
        Dim isValid As Boolean = True
        With oPolicy
            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVehicle.IsMarkedForDelete Then
                    For Each oLH As clsEntityLienHolder In oVehicle.LienHolders
                        If Not IsValidState(oLH.State) Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Invalid Lienholder State for Vehicle- " & oVehicle.IndexNum, "LienholderState", "IER", .Notes.Count))
                        End If
                    Next
                End If
            Next
        End With
    End Sub

    Public Function IsValidState(ByVal sStateCode) As Boolean
        Dim bIsValid As Boolean = False
        Select Case sStateCode
            Case "AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY"
                bIsValid = True
            Case Else
                bIsValid = False
        End Select

        Return bIsValid
    End Function

    Public Overridable Sub CheckMSRPRestriction(ByRef oPolicy As clsPolicyPPA)
        Dim sVehicleList As String = String.Empty

        Dim iMaxSymbol As Integer
        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            'If VehicleApplies(oVehicle, oPolicy) Then
            If oVehicle.VinNo <> "NONOWNER" And Not oVehicle.StatedAmt > 0 And Not HasDefaultSymbols(oVehicle) Then
                iMaxSymbol = GetMaxMSRPSymbol(oVehicle.VehicleYear, oPolicy)

                Dim iVehSymbol As Integer
                Try
                    If oVehicle.PriceNewSymbolCode.Trim = String.Empty Then
                        iVehSymbol = 0
                    Else
                        iVehSymbol = CInt(oVehicle.PriceNewSymbolCode.Trim)
                    End If
                Catch ex As Exception
                    ' if the vehicle was added before we added pricenewsymbolcode it might be an empty string
                    ' if this is the case use vehiclesymbolcode
                    iVehSymbol = 0
                End Try

                If iVehSymbol > iMaxSymbol Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVehicle.IndexNum
                    Else
                        sVehicleList &= ", " & oVehicle.IndexNum
                    End If
                End If
            End If
            'End If
        Next


        Dim sPrice As String = "$45,000"
        If iMaxSymbol = 57 Or iMaxSymbol = 24 Then
            sPrice = "$60,000"
        End If

        If sVehicleList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The following vehicle(s) have an Original Cost New above " & sPrice & " - " & sVehicleList & ".", "MSRPOver45k", "IER", oPolicy.Notes.Count))
        End If
    End Sub

    Public Function GetMaxMSRPSymbol(ByVal iVehicleYear As Integer, ByVal oPolicy As clsPolicyPPA) As Integer
        Dim sSQL As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim iMaxMSRP As Integer = 99

        Try
            'Open the connection
            oConn.Open()

            sSQL = "Select ItemValue From pgm" & oPolicy.Product & oPolicy.StateCode & "..StateInfo with(nolock) "
            sSQL &= " WHERE EffDate <= @RateDate "
            sSQL &= " AND ExpDate > @RateDate "
            sSQL &= " AND AppliesToCode IN ('B', @AppliesToCode) "
            sSQL &= " AND Program = @Program "
            sSQL &= " AND ItemGroup = 'MAXMSRP' "
            sSQL &= " AND ItemCode <= @VehicleYear "
            sSQL &= " AND ItemSubCode >= @VehicleYear "

            Dim cmd As SqlCommand = New SqlCommand(sSQL, oConn)

            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@VehicleYear", SqlDbType.Int).Value = iVehicleYear

            Dim oRr As Data.SqlClient.SqlDataReader
            oRr = cmd.ExecuteReader
            While oRr.Read()
                iMaxMSRP = oRr.Item("ItemValue")
            End While

        Catch ex As SoapException
            Return 99
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Return iMaxMSRP
    End Function

    Public Overridable Sub CheckMVRDriverDOBMismatch(ByVal policy As clsPolicyPPA)

        Dim cloneNotes As New List(Of clsBaseNote)(policy.Notes)

        For Each note As clsBaseNote In cloneNotes
            If note.SourceCode.ToUpper.Trim = "RPT" _
                    AndAlso note.NoteDesc.ToUpper.Trim = "MVRDRIVERDOBMISMATCH" Then
                Dim driverName As String = String.Empty
                Dim mismatchExists As Boolean = False

                driverName = Mid(note.NoteText, note.NoteText.IndexOf("(") + 1)
                driverName = driverName.Replace("(", "").Replace(")", "").Replace(".", "").Trim

                For Each dobInfoNote As clsBaseNote In cloneNotes
                    If dobInfoNote.SourceCode.ToUpper.Trim = "RPT" _
                            AndAlso dobInfoNote.NoteDesc.ToUpper.Trim = "MVRDRIVERINFO" _
                                AndAlso dobInfoNote.NoteText.Contains(driverName) Then
                        Dim driverInfoDriverName As String = dobInfoNote.NoteText.Split(":")(1)
                        Dim driverInfoDOB As String = dobInfoNote.NoteText.Split(":")(2)
                        Dim driverInfoDLN As String = dobInfoNote.NoteText.Split(":")(3)
                        For Each driver In policy.Drivers
                            If driver.DriverStatus.ToUpper.Trim = "ACTIVE" Then
                                If (driver.EntityName1.ToUpper.Trim & " " & driver.EntityName2.ToUpper.Trim = driverInfoDriverName.ToUpper.Trim) _
                                        AndAlso (driver.DLN.ToUpper.Trim = driverInfoDLN.ToUpper.Trim) Then
                                    If driverInfoDOB <> driver.DOB.ToShortDateString Then
                                        mismatchExists = True
                                        Exit For
                                    End If
                                End If
                            End If
                        Next
                    End If
                    If mismatchExists Then
                        Exit For
                    End If
                Next
                If mismatchExists Then
                    policy.Notes = (AddNote(policy.Notes, "Ineligible Risk: " & note.NoteText, "DriverDOBMismatch", "IER", policy.Notes.Count))
                Else
                    policy.Notes = RemoveNotes(policy.Notes, "IER", "DriverDOBMismatch", "Ineligible Risk: " & note.NoteText)
                End If
            End If
        Next
    End Sub

#End Region

#Region "Check IER Overrides"
    Public Shadows Function CheckIER(ByVal oPolicy As clsPolicyPPA)
        Return MyBase.CheckIER(oPolicy)
    End Function

    Public Shadows Function CheckIER(ByVal oVehicle As clsVehicleUnit, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal sRateDate As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote)) As Boolean
        Try

            'remove all IER notes
            oNoteList = RemoveNotes(oNoteList, "IER")

            Dim dtRatingRules As DataTable

            ' Create a dummy policy to pass into GetDataTableOfRules
            Dim oPolicy As New clsPolicyPPA
            oPolicy.Product = "2"
            oPolicy.Program = sProgram
            oPolicy.RateDate = sRateDate
            oPolicy.CallingSystem = sCallingSystem
            oPolicy.StateCode = sStateCode
            oPolicy.Status = "4"


            dtRatingRules = GetDataTableOfRules(oPolicy, "IER", "VEHICLE", 4)
            dtRatingRules = ApplyTemporaryRulesOverride(dtRatingRules, oPolicy)
            For Each oRule As DataRow In dtRatingRules.Rows
                Dim sFunctionName As String
                sFunctionName = oRule("FunctionName")

                Dim oArgs(2) As Object
                oArgs(0) = oVehicle
                oArgs(1) = sProgram
                oArgs(2) = oNoteList

                If sFunctionName.ToUpper = "CHECKCOVERAGES" Then
                    ReDim Preserve oArgs(4)
                    oArgs(3) = sRateDate
                    oArgs(4) = sStateCode
                End If

                ' Each function takes a vehicle and a notes list as arguments
                CallByName(Me, sFunctionName, CallType.Method, oArgs)

            Next
        Catch ex As Exception
            Dim sError As String = ex.Message
        End Try
    End Function

    Public Shadows Function CheckIER(ByVal oDriver As clsEntityDriver, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal sRateDate As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote)) As Boolean
        Try

            'remove all IER notes
            oNoteList = RemoveNotes(oNoteList, "IER")

            Dim dtRatingRules As DataTable

            ' Create a dummy policy to pass into GetDataTableOfRules
            Dim oPolicy As New clsPolicyPPA
            oPolicy.Product = "2"
            oPolicy.Program = sProgram
            oPolicy.RateDate = sRateDate
            oPolicy.CallingSystem = sCallingSystem
            oPolicy.StateCode = sStateCode

            dtRatingRules = GetDataTableOfRules(oPolicy, "IER", "DRIVER", 4)
            dtRatingRules = ApplyTemporaryRulesOverride(dtRatingRules, oPolicy)
            For Each oRule As DataRow In dtRatingRules.Rows
                Dim sFunctionName As String
                sFunctionName = oRule("FunctionName")

                Dim oArgs(2) As Object
                oArgs(0) = oDriver
                oArgs(1) = sProgram
                oArgs(2) = oNoteList

                ' Each function takes a vehicle and a notes list as arguments
                CallByName(Me, sFunctionName, CallType.Method, oArgs)
            Next
        Catch ex As Exception
            Dim sError As String = ex.Message
        End Try
    End Function


#End Region

#Region "WRN Functions"

    Public Overridable Sub CheckPhysicianStatement(ByRef oPolicy As clsPolicyPPA)
        With oPolicy
            If .UWQuestions.Count > 0 Then
                For Each oUWQ As clsUWQuestion In .UWQuestions
                    Select Case oUWQ.QuestionCode
                        Case "301"
                            If Left(oUWQ.AnswerText.ToUpper.Trim, 3) = "YES" Then
                                If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                                    .Notes = (AddNote(.Notes, "Ineligible Risk: Please call 866-874-2741 to speak with an Imperial Agent to complete your application.  Drivers requiring a Physician's Statement require company approval.", "PhysStatement", "IER", .Notes.Count))
                                Else
                                    .Notes = (AddNote(.Notes, "Warning: A Physician Statement is required for all drivers that have a physical or mental impairment.", "PhysStatement", "WRN", .Notes.Count))
                                End If
                            End If
                    End Select
                Next
            End If
        End With
    End Sub

    Public Overridable Sub CheckExistingRenewal(ByVal oPolicy As clsPolicyPPA)
        With oPolicy
            If Not .RenewalQuote Is Nothing Then
                If .RenewalQuote.RenewalAmount > 0 Then
                    If .RenewalQuote.RenewalEffDate > Now() Then
                        If .RenewalQuote.QuoteNum.StartsWith("4" & .Product) Then
                            .Notes = (AddNote(.Notes, "Warning: The existing renewal quote will be regenerated when this endorsement is committed.", "EXSTRENEWAL", "WRN", .Notes.Count))
                        End If
                    End If
                End If
            End If

        End With
    End Sub

    Public Overridable Sub CheckNonInteractiveMVR(ByRef oPolicy As clsPolicyPPA)
        Dim sDriverList As String = String.Empty
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                If oDriver.MVROrderStatus.Length > 0 Then
                    If oDriver.MVROrderStatus = "NONINTERACTIVE" Then
                        If sDriverList = String.Empty Then
                            sDriverList = oDriver.IndexNum
                        Else
                            sDriverList &= ", " & oDriver.IndexNum
                        End If
                    End If
                End If
            End If
        Next
        If sDriverList <> String.Empty Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: Please note that the premium is subject to change pending return of Motor Vehicle Report(s) for the following driver(s) - " & sDriverList & ".", "NonInteractiveMVR", "WRN", oPolicy.Notes.Count))
        End If
    End Sub

    Public Overridable Sub CheckMilitaryDiscount(ByRef oPolicy As clsPolicyPPA)
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If DriverApplies(oDriver, oPolicy) Then
                If oDriver.Military AndAlso Not IsRewritePolicy(oPolicy) Then
                    If oPolicy.Program.ToUpper = "DIRECT" Then
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: An Application for Military Discount form will be mailed to you for completion.  If the form is not returned to Imperial, the Military Discount will be removed.", "AppMilitaryDiscountForm", "WRN", oPolicy.Notes.Count))
                    Else
                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: An Application for Military Discount form must be submitted for each active military personnel to avail of the Military Discount.", "AppMilitaryDiscountForm", "WRN", oPolicy.Notes.Count))
                    End If

                    Exit For
                End If
            End If
        Next
    End Sub

    Public Sub CheckMatureDriverDiscountDocsRequired(ByVal oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "DIRECT" Then
            Dim sDriverListWithDiscount As String = String.Empty

            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                    If oDriver.MatureDriver Then
                        If sDriverListWithDiscount = String.Empty Then
                            sDriverListWithDiscount = oDriver.IndexNum
                        Else
                            sDriverListWithDiscount &= ", " & oDriver.IndexNum
                        End If
                    End If
                End If
            Next

            If sDriverListWithDiscount <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: Certificate proving completion of a valid motor vehicle accident prevention course must be submitted for the following driver(s).  If certification is not received, the Mature Driver Discount will be removed and additional premium will be due. Please call 866-874-2741 for additional information. - " & sDriverListWithDiscount & ".", "CheckMatureDriverDiscountDocsRequired", "WRN", oPolicy.Notes.Count))
            End If
        End If
    End Sub


    Public Sub CheckScholasticDiscountDocsRequired(ByVal oPolicy As clsPolicyPPA)
        If oPolicy.Program.ToUpper = "DIRECT" Then
            Dim sDriverListWithDiscount As String = String.Empty

            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                    If oDriver.ScholasticHonor Then
                        If sDriverListWithDiscount = String.Empty Then
                            sDriverListWithDiscount = oDriver.IndexNum
                        Else
                            sDriverListWithDiscount &= ", " & oDriver.IndexNum
                        End If
                    End If
                End If
            Next

            If sDriverListWithDiscount <> "" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: Transcripts must be submitted for the following driver(s).  If documentation is not received, the Scholastic Honor Discount will be removed and additional premium will be due. Please call 866-874-2741 for additional information. - " & sDriverListWithDiscount & ".", "CheckScholasticDiscountDocsRequired", "WRN", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Overridable Sub CheckWindowEtch(ByVal oPolicy)
        Dim sVehicleList As String = String.Empty
        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If VehicleApplies(oVeh, oPolicy) And (Not IsRewritePolicy(oPolicy) Or oVeh.IsModified) Then
                If DetermineVehicleFactorExists(oVeh, "ETCH") Then
                    If sVehicleList = String.Empty Then
                        sVehicleList = oVeh.IndexNum
                    Else
                        sVehicleList &= ", " & oVeh.IndexNum
                    End If
                End If
            End If
        Next
        If sVehicleList <> String.Empty Then
            If oPolicy.Program.ToUpper = "DIRECT" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: Certification showing the VIN has been etched into all windows of the following vehicle(s) must be submitted. If proof is not received, the Window Etch Discount will be removed and additional premium will be due. Please call 866-874-2741 for additional information. - " & sVehicleList & ".", "EtchProofCert", "WRN", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: Certification showing the VIN has been etched into all windows of the following vehicle(s) must be submitted.  If proof is not received, the Window Etch Discount will be removed. - " & sVehicleList & ".", "EtchProofCert", "WRN", oPolicy.Notes.Count))
            End If
        End If
    End Sub

#End Region

#Region "Res Functions"
    Public Overridable Sub RemoveNOVIOLIfINEXPFactor(ByVal oPolicy As clsPolicyPPA)

        'default is to do nothing

    End Sub

    Public Overridable Sub CheckReportsOrdered(ByRef oPolicy As clsPolicyPPA)
        Dim bRequireReports As Boolean = True

        ' Do not require/show order reports  for RAC imported policies
        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.NoteDesc.ToUpper.Trim = "IMPORT" And oNote.SourceCode.ToUpper.Trim = "FLR" Then
                bRequireReports = False
            End If
        Next

        If bRequireReports Then
            With oPolicy
                If .Status = "4" Then
                    Dim bReportsOrdered As Boolean = True
                    For Each oDriver As clsEntityDriver In oPolicy.Drivers
                        If DriverApplies(oDriver, oPolicy) Then
                            If (oDriver.MVRTimesOrdered = 0 Or oDriver.MVROrderStatus.Length = 0) And oDriver.DriverStatus.ToUpper = "ACTIVE" Then
                                bReportsOrdered = False
                            End If
                        End If
                    Next
                    If bReportsOrdered = False Then
                        If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Reports have not been ordered for all Active drivers on the policy.  Please go to the Purchase screen and click ""Continue"" to order reports.", "OrderReports", "RES", .Notes.Count))
                        Else
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Reports have not been ordered for all active drivers on the policy", "OrderReports", "RES", .Notes.Count))
                        End If
                    End If
                End If
            End With
        End If
    End Sub

    Public Overridable Sub CheckCreditOrdered(ByRef oPolicy As clsPolicyPPA)
        With oPolicy
            If .Status = "4" Then

                If .PolicyInsured.CreditStatus.ToUpper <> "DECLINED" And .PolicyInsured.CreditStatus.ToUpper <> "SUCCESS" And .PolicyInsured.CreditStatus.ToUpper <> "NOSCORE" And .PolicyInsured.CreditStatus.ToUpper <> "NOHIT" And .PolicyInsured.CreditStatus.ToUpper <> "ERROR" Then '.PolicyInsured.CreditTimesOrdered = 0
                    If CreditRequired(oPolicy) Then
                        If .PolicyInsured.CreditScore > 0 Then 'credit score of 0 means they declined credit
                            .Notes = (AddNote(.Notes, "Ineligible Risk: Credit must be ordered or declined", "OrderCredit", "RES", .Notes.Count))
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckClaimsOrdered(ByRef oPolicy As clsPolicyPPA)

        With oPolicy
            If .Status = "4" Then
                If .PolicyInsured.ClaimsTimesOrdered = 0 Or .PolicyInsured.ClaimOrderStatus.Length = 0 Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Claims report has not been successfully ordered for this policy.", "OrderClaims", "RES", .Notes.Count))
                End If
            End If
        End With

    End Sub

#End Region

#Region "HelperFunctions"
    Public Sub AddPolicyNote(ByVal oPolicy As clsPolicyPPA, ByVal sDesc As String, ByVal sCode As String)
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String

        Try
            sSql = "  INSERT INTO PasCarrier..PolicyNote (CompanyCode,ProgramCode,PolicyTransactionNum,MonolineProductCode,PolicyNo,NoteTypeCode,NoteDesc,RedFlag,NoteText,TermEffDate,TermExpDate,AddedDateT,AddedUserCode,LastUpdatedDateT,LastUpdatedUserCode)"
            sSql &= " Values(@CompanyCode,@ProgramCode,@TransNum,'PA',@PolicyNo,'IER',@NoteDesc,null,@NoteText,@EffDate,@ExpDate,getdate(),'WEBRATER',null,null)"

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)
                ' Only want to compare alphanumeric chars, (i.e. don't want 123-45-6789)
                cmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = "IF"
                cmd.Parameters.Add("@ProgramCode", SqlDbType.VarChar).Value = oPolicy.ProgramCode
                cmd.Parameters.Add("@TransNum", SqlDbType.Int).Value = oPolicy.TransactionNum
                cmd.Parameters.Add("@PolicyNo", SqlDbType.VarChar).Value = oPolicy.PolicyID
                cmd.Parameters.Add("@NoteDesc", SqlDbType.VarChar).Value = sCode
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

    Public Sub ExpireWeatherOverrideByCounty(ByVal productCode As Integer, ByVal stateCode As String, ByVal stateInfos As List(Of StateInfo))

        If (productCode > 0 And Not String.IsNullOrEmpty(stateCode) And stateInfos.Any()) Then
            For Each info As StateInfo In stateInfos
                Dim localInfo = StateInfo.GetNew(info.ItemSubCode, "", info.Program)
                localInfo.ExpDate = localInfo.EffDate
                UpdateStateInfo(productCode, stateCode, localInfo)
            Next
        End If
    End Sub

    Public Sub ExpireActiveWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String)

        If (productCode > 0 And Not String.IsNullOrEmpty(stateCode)) Then

            Dim allStateInfo = GetAllStateInfo(productCode, stateCode, "WEATHEROVERRIDE")
            Dim activeStateInfo = allStateInfo.Where(Function(x) x.ExpDate > DateTime.Now).ToList()

            For Each info As StateInfo In activeStateInfo

                Try
                    Dim localInfo = StateInfo.GetNew(info.ItemSubCode, "", info.Program)
                    localInfo.ExpDate = localInfo.EffDate
                    UpdateStateInfo(productCode, stateCode, localInfo)
                Catch ex As Exception
                    'TODO: Log
                End Try
            Next
        End If
    End Sub

    Public Sub SetWeatherOverrideByZipCode(ByVal productCode As Integer, ByVal stateCode As String, ByVal userID As String, _
                                            ByVal programs As List(Of Integer), ByVal zipCodes As List(Of String))

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) And programs.Any() And zipCodes.Any()) Then
            'Build County List based on Zips passed in
            'Insert distinct county list as zipCodes map to more than one county
            Dim mapping = GetZipCountyMapping(productCode, stateCode)

            Dim resultCounties = (From m In mapping
                                  Join z In zipCodes On m.ZipCode Equals z
                                  Select m.County).Distinct().ToList()

            SetWeatherOverrideByCounty(productCode, stateCode, userID, programs, resultCounties)
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

    Public Sub SetWeatherOverrideByCounty(ByVal productCode As Integer, ByVal stateCode As String, ByVal userID As String, _
                                            ByVal programs As List(Of Integer), ByVal counties As List(Of String))

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) And programs.Any() And counties.Any()) Then

            Dim CRM As New MarketingCRMService.MarketingCRMService
            Dim allPrograms = CRM.GetActivePrograms(productCode, stateCode).ToList()

            Dim allStateInfo = GetAllStateInfo(productCode, stateCode, "WEATHEROVERRIDE")
            Dim activeStateInfo = allStateInfo.Where(Function(x) x.ExpDate > DateTime.Now).ToList()

            Dim activeProgramIDs = (From p In allPrograms
                                    Join p1 In activeStateInfo On
                                    p.ProgramCode Equals p1.Program
                                    Select p.ProgramID)

            Dim netPrograms = programs.Union(activeProgramIDs)

            'Set Individual, Expire All StateInfo, Set Individual Selected w/ Restriction StartTime
            If (allPrograms.Select(Function(x) x.ProgramID).Except(netPrograms).Any()) Then

                'Expire Active PPA
                For Each stateInfo As StateInfo In activeStateInfo.Where(Function(x) x.Program = "PPA")
                    stateInfo.ExpDate = stateInfo.EffDate
                    UpdateStateInfo(productCode, stateCode, stateInfo)
                Next

                For Each programID As Integer In programs

                    Dim existingProgram = allPrograms.SingleOrDefault(Function(x) x.ProgramID = programID)

                    'Update for this Program/All Counties
                    For Each county As String In counties.Except(activeStateInfo.Where(Function(x) x.Program = existingProgram.ProgramCode).Select(Function(x) x.ItemSubCode))
                        UpdateStateInfo(productCode, stateCode, StateInfo.GetNew(county, userID, existingProgram.ProgramCode))
                    Next
                    For Each county As String In counties.Except(activeStateInfo.Where(Function(x) x.Program = existingProgram.ProgramCode).Select(Function(x) x.ItemSubCode))
                        InsertStateInfo(productCode, stateCode, StateInfo.GetNew(county, userID, existingProgram.ProgramCode))
                    Next
                Next
            Else
                'Set All Override - Expire All Existing, Set Override w/ Restricion StartTime w/ PPA Program
                'Expire Active
                For Each stateInfo As StateInfo In activeStateInfo.Where(Function(x) x.Program <> "PPA")
                    stateInfo.ExpDate = stateInfo.EffDate
                    UpdateStateInfo(productCode, stateCode, stateInfo)
                Next

                'Update/Insert for PPA Program 
                For Each county As String In counties.Except(activeStateInfo.Where(Function(x) x.Program = "PPA").Select(Function(x) x.ItemSubCode))
                    UpdateStateInfo(productCode, stateCode, StateInfo.GetNew(county, userID))
                Next
                For Each county As String In counties.Except(activeStateInfo.Where(Function(x) x.Program = "PPA").Select(Function(x) x.ItemSubCode))
                    InsertStateInfo(productCode, stateCode, StateInfo.GetNew(county, userID))
                Next
            End If
        End If
    End Sub

    Public Overrides Sub SetWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal startDate As DateTime, _
                                            ByVal programs As List(Of Integer))

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) And programs.Any()) Then

            Dim CRM As New MarketingCRMService.MarketingCRMService
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
                If (allProgramSetting.Any(Function(x) x.Program = "PPA")) Then
                    Dim expireProgramSetting = ProgramSetting.GetNew(startDate, "PPA")
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
                For Each activeProgram As ProgramSetting In activeProgramSetting
                    activeProgram.ExpDate = activeProgram.EffDate
                    UpdateProgramSetting(productCode, stateCode, activeProgram)
                Next

                If (allProgramSetting.Any(Function(x) x.Program = "PPA")) Then
                    UpdateProgramSetting(productCode, stateCode, ProgramSetting.GetNew(startDate, "PPA"))
                Else
                    InsertProgramSetting(productCode, stateCode, ProgramSetting.GetNew(startDate, "PPA"))
                End If
            End If
        End If
    End Sub

    Public Function IsRestricted(ByVal sDLN As String, ByVal sDLNState As String) As Boolean
        Dim DLNSvc As New DLNService.DLNService
        Dim restrictions() As DLNService.DriverRestriction = DLNSvc.FindRestrictedDriverByDLN(sDLN, sDLNState)

        If restrictions IsNot Nothing Then
            If restrictions.Length > 0 Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function

    Public Function OnlyAlphaNumericChars(ByVal OrigString As String) As String
        Dim lLen As Long
        Dim sAns As String = String.Empty
        Dim lCtr As Long
        Dim sChar As String

        OrigString = Trim(OrigString)
        lLen = Len(OrigString)
        For lCtr = 1 To lLen
            sChar = Mid(OrigString, lCtr, 1)
            If IsAlphaNumeric(Mid(OrigString, lCtr, 1)) Then
                sAns = sAns & sChar
            End If
        Next

        OnlyAlphaNumericChars = sAns.ToUpper()
    End Function

    Private Function IsAlphaNumeric(ByVal sChr As String) As Boolean
        IsAlphaNumeric = sChr Like "[0-9A-Za-z]"
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

    Public Overloads Function RemoveNotes(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sSourceCode As String, ByVal sNoteDescription As String, ByVal sNoteText As String) As System.Collections.Generic.List(Of clsBaseNote)

        For i As Integer = oNoteList.Count - 1 To 0 Step -1
            If oNoteList.Item(i).SourceCode.ToUpper = sSourceCode.ToUpper Then
                If oNoteList.Item(i).NoteDesc.ToUpper = sNoteDescription.ToUpper Then
                    If oNoteList.Item(i).NoteText.ToUpper = sNoteText.ToUpper Then
                        oNoteList.RemoveAt(i)
                    End If
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

    Public Overridable Function DeterminePhysDamageExists(ByVal oVehicle As clsVehicleUnit) As Boolean
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
    Public Overridable Function DetermineUMPDExists(ByVal oVehicle As clsVehicleUnit) As Boolean

        Dim bUMPD As Boolean = False

        ' Check if this is a Physical Damage Policy (i.e. If there are COMP/COLL coverage)
        For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
            If Not oCoverage.IsMarkedForDelete Then
                If oCoverage.CovCode.Contains("UMPD") And Not oCoverage.IsMarkedForDelete Then
                    bUMPD = True
                    Exit For
                End If
            End If
        Next

        Return bUMPD

    End Function

    Public Overridable Function DeterminePhysDamageWithNoUMPDExists(ByVal oVehicle As clsVehicleUnit) As Boolean
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

    Public Function DeterminePhysDamageExistsUW(ByVal oVehicle As clsVehicleUnit) As Boolean
        Dim bPhysDamage As Boolean = False

        ' Check if this is a Physical Damage Policy (i.e. If there are COMP/COLL/ADI coverages)
        ' Different for UW question 8.
        For Each oCoverage As clsBaseCoverage In oVehicle.Coverages
            If (oCoverage.CovCode.Contains("OTC") Or oCoverage.CovCode.Contains("COL") Or oCoverage.CovCode.Contains("ADI")) And Not oCoverage.IsMarkedForDelete Then
                bPhysDamage = True
                Exit For
            End If
        Next

        Return bPhysDamage
    End Function

    Public Overridable Function ItemsToBeFaxedIn(ByVal oPolicy As clsPolicyPPA) As String
        Dim sItemsToBeFaxedIn As String = ""


        If oPolicy.CallingSystem <> "PAS" Then
            If oPolicy.Program.ToUpper = "DIRECT" Then
                sItemsToBeFaxedIn &= "E-signed application from the Insured" & vbNewLine
            Else
                sItemsToBeFaxedIn &= "Signed and dated Application by both insured and agent" & vbNewLine
            End If

        End If

        If oPolicy.IsEFT Then
            sItemsToBeFaxedIn &= "EFT Authorization Form" & vbNewLine
        End If

        'military and Acc Prev course
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete Then
                If oDrv.Military AndAlso Not IsRewritePolicy(oPolicy) Then
                    sItemsToBeFaxedIn &= "Proof of Military Discount for " & oDrv.EntityName1 & " " & oDrv.EntityName2 & vbNewLine
                End If
                If oDrv.DrivingCourse Then
                    sItemsToBeFaxedIn &= "Proof of completion of an Accident Prevention Course within the last 3 years for " & oDrv.EntityName1 & " " & oDrv.EntityName2 & vbNewLine

                End If
            End If
        Next

        Return sItemsToBeFaxedIn

    End Function

    Public Function ValidateCoverages(ByVal sCovCodes As String, ByVal sCovGroups As String, ByVal sProduct As String, ByVal sStateCode As String, ByVal sRateDate As String, ByVal sProgram As String, Optional ByVal sAppliesTo As String = "B") As String
        Dim sInvalidCoverages As String = String.Empty
        Dim sSql As String = String.Empty
        Dim dtInvalidCovCombos As New DataTable
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Try
            sSql = GetCoveragesSql(sProduct, sStateCode, sCovCodes, sCovGroups)

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 11).Value = sProgram.Trim
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = sRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesTo

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtInvalidCovCombos)

                Dim sPreviousCovCheck As String = String.Empty

                For Each oInvalidCovCombo As DataRow In dtInvalidCovCombos.Rows
                    Select Case oInvalidCovCombo("RequireType")
                        Case "L" ' Limit
                            sInvalidCoverages &= "Cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " and " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & "." & "<br />"
                        Case "I" ' Required
                            sInvalidCoverages &= "Cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " coverage without " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & " coverage." & "<br />"
                        Case "X" ' Excluded
                            sInvalidCoverages &= "Cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " coverage with " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & " coverage." & "<br />"
                        Case "O" ' Either Or (Must have Coverage A or Coverage B)
                            If sPreviousCovCheck = oInvalidCovCombo("CovCheck") Then
                                sInvalidCoverages &= " or " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & "<br />"
                            Else
                                sPreviousCovCheck = oInvalidCovCombo("CovCheck")
                                sInvalidCoverages &= "Cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " coverage without " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired"))
                            End If
                    End Select
                Next
            End Using

            oConn.Close()
        Catch ex As Exception
        End Try

        Return sInvalidCoverages
    End Function

    Private Function GetDisplayName(ByVal program As String, ByVal covGroup As String) As String
        Dim displayName As String = covGroup

        If program.ToUpper.Trim = "DIRECT" Then
            Select Case covGroup.ToUpper.Trim
                Case "UUMPD"
                    displayName = "Uninsured Motorist PD"
                Case "UMPD"
                    displayName = "Uninsured Motorist PD"
                Case "UUMBI"
                    displayName = "Uninsured Motorist BI"
                Case "MED"
                    displayName = "Med Pay"
                Case "OTC"
                    displayName = "Other Than Collision"
                Case "COL"
                    displayName = "Collision"
                Case "REN"
                    displayName = "Rental"
                Case "TOW"
                    displayName = "Towing"
                Case "UIMBI"
                    displayName = "Underinsured Motorist BI"
                Case "UMBI"
                    displayName = "Uninsured Motorist BI"
            End Select
        End If

        Return displayName

    End Function

    Private Function GetCoveragesSql(ByVal sProduct As String, ByVal sStateCode As String, ByVal sCovCodes As String, ByVal sCovGroups As String) As String
        Dim sSql As String = String.Empty

        sSql = "  SELECT CovCheck, CovRequired, RequireType "
        sSql &= "   FROM pgm" & sProduct.Trim & sStateCode.Trim & "..CodeCovCombo with(nolock) "
        sSql &= "  WHERE Program = @Program "
        sSql &= "    AND EffDate <= @RateDate "
        sSql &= "    AND ExpDate > @RateDate "
        sSql &= "    AND AppliesToCode IN ('B', @AppliesToCode) "
        sSql &= "    AND CovType = 'CovCode' "
        sSql &= "    AND CovCheck IN (" & sCovCodes & ") "
        sSql &= "    AND RequireType = 'L' "
        sSql &= "    AND CovRequired IN (" & sCovCodes & ") "
        sSql &= "  UNION "
        sSql &= " SELECT CovCheck, CovRequired, RequireType "
        sSql &= "   FROM pgm" & sProduct.Trim & sStateCode.Trim & "..CodeCovCombo with(nolock) "
        sSql &= "  WHERE Program = @Program "
        sSql &= "    AND  EffDate <= @RateDate "
        sSql &= "    AND ExpDate > @RateDate "
        sSql &= "    AND AppliesToCode IN ('B', @AppliesToCode) "
        sSql &= "    AND CovType = 'CovGroup' "
        sSql &= "    AND CovCheck IN (" & sCovGroups & ")"
        sSql &= "    AND ((RequireType = 'I' AND CovRequired NOT IN (" & sCovGroups & ")) "
        sSql &= "     OR  (RequireType = 'X' AND CovRequired IN (" & sCovGroups & "))) "
        sSql &= "  UNION "
        sSql &= " SELECT CovCheck, CovRequired, RequireType "
        sSql &= "   FROM pgm" & sProduct.Trim & sStateCode.Trim & "..CodeCovCombo with(nolock) "
        sSql &= "  WHERE Program = @Program "
        sSql &= "    AND  EffDate <= @RateDate "
        sSql &= "    AND ExpDate > @RateDate "
        sSql &= "    AND AppliesToCode IN ('B', @AppliesToCode) "
        sSql &= "    AND CovType = 'CovGroup' "
        sSql &= "    AND RequireType = 'O'"
        sSql &= "    AND CovCheck IN (" & sCovGroups & ")"
        sSql &= "    AND NOT EXISTS(SELECT CovCheck, CovRequired, RequireType "
        sSql &= "                   FROM pgm" & sProduct.Trim & sStateCode.Trim & "..CodeCovCombo with(nolock) "
        sSql &= "                   WHERE Program = @Program "
        sSql &= "                     AND EffDate <= @RateDate "
        sSql &= "                     AND ExpDate > @RateDate "
        sSql &= "                     AND AppliesToCode IN ('B', @AppliesToCode) "
        sSql &= "                     AND CovType = 'CovGroup' "
        sSql &= "                     AND RequireType = 'O'"
        sSql &= "                     AND CovRequired IN (" & sCovGroups & ")"
        sSql &= "                   )"

        Return sSql
    End Function

    Public Function ValidateCoverages(ByVal oVeh As clsVehicleUnit, ByVal sProduct As String, ByVal sStateCode As String, ByVal sRateDate As String, ByVal sProgram As String, Optional ByVal sAppliesTo As String = "B") As String

        Dim sInvalidCoverages As String = String.Empty
        Dim sCovGroups As String = String.Empty
        Dim sCovCodes As String = String.Empty
        Dim sSql As String = String.Empty
        Dim dtInvalidCovCombos As New DataTable
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        For i As Integer = 0 To oVeh.Coverages.Count - 1
            Dim oCov As clsPACoverage = oVeh.Coverages(i)
            If Not oCov.IsMarkedForDelete Then
                If Not sCovCodes = String.Empty Then
                    sCovCodes &= ","
                    sCovGroups &= ","
                End If

                sCovCodes &= "'" & oCov.CovCode & "'"
                sCovGroups &= "'" & oCov.CovGroup & "'"
            End If
        Next

        Try
            sSql = GetCoveragesSql(sProduct, sStateCode, sCovCodes, sCovGroups)

            oConn.Open()

            Using cmd As New SqlCommand(sSql, oConn)
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 11).Value = sProgram.Trim
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = sRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesTo

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtInvalidCovCombos)

                Dim sPreviousCovCheck As String = ""
                For Each oInvalidCovCombo As DataRow In dtInvalidCovCombos.Rows
                    Select Case oInvalidCovCombo("RequireType")
                        Case "L" ' Limit
                            sInvalidCoverages &= "Veh#" & oVeh.IndexNum & " cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " and " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & "." & "<br />"
                        Case "I" ' Required
                            sInvalidCoverages &= "Veh#" & oVeh.IndexNum & " cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " coverage without " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & " coverage." & "<br />"
                        Case "X" ' Excluded
                            sInvalidCoverages &= "Veh#" & oVeh.IndexNum & " cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " coverage with " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & " coverage." & "<br />"
                        Case "O" ' Either Or (Must have Coverage A or Coverage B)
                            If sPreviousCovCheck = oInvalidCovCombo("CovCheck") Then
                                sInvalidCoverages &= " or " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired")) & "<br />"
                            Else
                                sPreviousCovCheck = oInvalidCovCombo("CovCheck")
                                sInvalidCoverages &= "Veh#" & oVeh.IndexNum & " cannot have " & GetDisplayName(sProgram, oInvalidCovCombo("CovCheck")) & " coverage without " & GetDisplayName(sProgram, oInvalidCovCombo("CovRequired"))
                            End If
                    End Select
                Next
            End Using
            oConn.Close()
        Catch ex As Exception
        End Try

        Return sInvalidCoverages
    End Function

    Public Function DeterminePolicyFactorExists(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String) As Boolean
        Dim bFactorExists As Boolean = False

        For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
            If Not oPolicyFactor Is Nothing Then
                If oPolicyFactor.FactorCode = sFactorCode Then
                    bFactorExists = True
                    Exit For
                End If
            End If
        Next

        Return bFactorExists
    End Function

    Public Function DetermineVehicleFactorExists(ByVal oVehicle As clsVehicleUnit, ByVal sFactorCode As String) As Boolean
        Dim bFactorExists As Boolean = False

        For Each oVehicleFactor As clsBaseFactor In oVehicle.Factors
            If Not oVehicleFactor Is Nothing Then
                If oVehicleFactor.FactorCode = sFactorCode Then
                    bFactorExists = True
                    Exit For
                End If
            End If
        Next

        Return bFactorExists
    End Function

    Public Function DetermineDriverFactorExists(ByVal oDriver As clsEntityDriver, ByVal sFactorCode As String) As Boolean
        Dim bFactorExists As Boolean = False

        For Each oDriverFactor As clsBaseFactor In oDriver.Factors
            If Not oDriverFactor Is Nothing Then
                If oDriverFactor.FactorCode = sFactorCode Then
                    bFactorExists = True
                    Exit For
                End If
            End If
        Next

        Return bFactorExists
    End Function

    Public Function ValidatePayPlan(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bValidPayPlan As Boolean = False
        Dim oPayPlanDataSet As DataSet = LoadPayPlanTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        Dim DataRows() As DataRow
        DataRows = oPayPlanDataSet.Tables(0).Select("Program IN ('PPA', '" & oPolicy.Program & "')")

        For Each oRow As DataRow In DataRows
            If oRow.Item("PayPlanCode").ToString.ToUpper = oPolicy.PayPlanCode Then
                bValidPayPlan = True
                Exit For
            End If
        Next

        Return bValidPayPlan
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadPayPlanTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Program, PayPlanCode, Name, DownPayPct, NumInstallments, InstallmentType, UsePremWFeesInCalc "
                sSql = sSql & " FROM pgm" & sProduct & sStateCode & "..PayPlan with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Program, PayPlanCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "PayPlan")

                Return oDS
            End Using
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Function CreditRequired(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bCreditRequired As Boolean = False
        Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        Dim DataRows() As DataRow
        DataRows = oStateInfoDataSet.Tables(0).Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup = 'CREDIT' AND ItemCode = 'TYPE'")

        For Each oRow As DataRow In DataRows
            If oRow.Item("ItemValue").ToString.ToUpper = "CREDIT" Then
                bCreditRequired = True
                Exit For
            End If
        Next

        Return bCreditRequired
    End Function

    Public Overridable Function IsChargeableAccident(ByVal sViolGroup As String) As Boolean

        Dim bReturn As Boolean = False
        Dim sChargeableAccidents As String = "AFA"

        For Each sCharableAccident As String In Split(sChargeableAccidents, ",")
            If sCharableAccident.ToUpper.Trim = sViolGroup.ToUpper.Trim Then
                bReturn = True
            End If
        Next

        Return bReturn
    End Function

    Public Overridable Function GetNoViolDiscount(ByVal oDrv As clsEntityDriver, ByVal dtEffDate As Date) As Integer
        Dim oRatingRules As New CommonRulesFunctions
        Dim iDiscountToAdd As Integer = -1

        Dim iPts36 As Integer = 0
        Dim iPts18 As Integer = 0
        Dim iPts12 As Integer = 0


        ' Next check to see if there were any chargeable accidents or serious infractions
        ' in the past x months (none allowed to qualify for discount)
        For Each oViol As clsBaseViolation In oDrv.Violations
            Dim iTempMonthsOld As Integer = 0
            iTempMonthsOld = oRatingRules.CalculateViolAge(oViol.ViolDate, dtEffDate)

            If iTempMonthsOld < 0 Then
                iTempMonthsOld = 0
            End If

            ' Check to see if this is a serious violation or chargeable accident
            ' If the violation was within the past 18 months
            If iTempMonthsOld < 12 Then
                iPts12 += oViol.Points
            ElseIf iTempMonthsOld < 18 Then
                iPts18 += oViol.Points
            ElseIf iTempMonthsOld < 36 Then
                iPts36 += oViol.Points
            End If
        Next

        ' For the 36 month discount, all 3 must be at 0
        Dim bDiscountAdded As Boolean = False
        If iPts36 + iPts18 + iPts12 < 2 And Not bDiscountAdded Then
            ' give 36 month discount
            iDiscountToAdd = 36
            bDiscountAdded = True
        End If

        If iPts18 + iPts12 < 2 And Not bDiscountAdded Then
            ' Give the 18 month discount
            iDiscountToAdd = 18
            bDiscountAdded = True
        End If

        If iPts12 < 2 And Not bDiscountAdded Then
            ' Give the 12 month discount
            iDiscountToAdd = 12
            bDiscountAdded = True
        End If

        Return iDiscountToAdd ' -1 if no discount is added
    End Function

    Public Function GetViol(ByVal sViolGroup As String, ByVal sViolTypeCode As String, ByVal oDriver As clsEntityDriver) As clsBaseViolation

        Dim oReturnedViol As clsBaseViolation = Nothing
        For Each oViol As clsBaseViolation In oDriver.Violations
            If oViol.ViolGroup.ToUpper = sViolGroup.ToUpper And oViol.ViolTypeCode.ToUpper = sViolTypeCode.ToUpper Then
                oReturnedViol = oViol
                Exit For
            End If
        Next
        Return oReturnedViol
    End Function

    Public Overridable Sub AddInexperiencedViolation(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver)

        Dim oViolation As clsBaseViolation = GetViol("MED", "99999", oDriver)

        If oViolation Is Nothing Then
            oViolation = New clsBaseViolation
            oViolation.ViolTypeCode = "99999"
            oViolation.ViolDesc = "Operators licensed for less than 3 years (" & oDriver.LicenseStateDate & ")"
            oViolation.ViolTypeIndicator = "V"
            oViolation.ViolGroup = "MED"
            oViolation.ViolSourceCode = "M"
            oViolation.AtFault = False
            oViolation.ViolDate = Now()
            oViolation.ConvictionDate = Now()
            oViolation.Chargeable = True
            oViolation.IndexNum = oDriver.Violations.Count + 1
            oViolation.AddToXML = True

            oDriver.Violations.Add(oViolation)
        End If
    End Sub

    Public Overloads Sub RemoveInexperiencedViolation(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver)

        For Each oViol As clsBaseViolation In oDriver.Violations
            If oViol.ViolTypeCode = "99999" Then
                oDriver.Violations.Remove(oViol)
                Exit For
            End If
        Next
    End Sub


    Public Function HasDefaultSymbols(ByVal oVehicle As clsVehicleUnit) As Boolean
        Dim bHasDefaultSymbol As Boolean = False

        If oVehicle.VehicleYear < 2011 Then
            If oVehicle.VehicleSymbolCode.Trim = "999" Or oVehicle.VehicleSymbolCode.Trim = "65" Or oVehicle.VehicleSymbolCode.Trim = "66" Or oVehicle.VehicleSymbolCode.Trim = "67" Or oVehicle.VehicleSymbolCode.Trim = "68" _
                Or oVehicle.PriceNewSymbolCode.Trim = "999" Or oVehicle.PriceNewSymbolCode.Trim = "65" Or oVehicle.PriceNewSymbolCode.Trim = "66" Or oVehicle.PriceNewSymbolCode.Trim = "67" Or oVehicle.PriceNewSymbolCode.Trim = "68" Then
                bHasDefaultSymbol = True
            End If
        Else
            If oVehicle.VehicleSymbolCode.Trim = "999" Or oVehicle.VehicleSymbolCode.Trim = "965" Or oVehicle.VehicleSymbolCode.Trim = "966" Or oVehicle.VehicleSymbolCode.Trim = "967" Or oVehicle.VehicleSymbolCode.Trim = "968" _
                Or oVehicle.PriceNewSymbolCode.Trim = "999" Or oVehicle.PriceNewSymbolCode.Trim = "965" Or oVehicle.PriceNewSymbolCode.Trim = "966" Or oVehicle.PriceNewSymbolCode.Trim = "967" Or oVehicle.PriceNewSymbolCode.Trim = "968" Then
                bHasDefaultSymbol = True
            End If
        End If


        Return bHasDefaultSymbol
    End Function

    Public Overridable Sub AddAutoApplyFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Dim FactorPolicyDataRows() As DataRow
            Dim oFactorPolicyTable As DataTable = Nothing
            Dim oFactorPolicyDataSet As DataSet = LoadFactorPolicyTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            oFactorPolicyTable = oFactorPolicyDataSet.Tables(0)
            RemoveAutoApplyFactors(oPolicy, oFactorPolicyTable)

            Dim FactorDriverDataRows() As DataRow
            Dim oFactorDriverTable As DataTable = Nothing
            Dim oFactorDriverDataSet As DataSet = LoadFactorDriverTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            oFactorDriverTable = oFactorDriverDataSet.Tables(0)
            RemoveAutoApplyFactors(oPolicy, oFactorDriverTable)

            Dim FactorVehicleDataRows() As DataRow
            Dim oFactorVehicleTable As DataTable = Nothing
            Dim oFactorVehicleDataSet As DataSet = LoadFactorVehicleTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            oFactorVehicleTable = oFactorVehicleDataSet.Tables(0)
            RemoveAutoApplyFactors(oPolicy, oFactorVehicleTable)

            'add driver auto apply factors
            FactorDriverDataRows = oFactorDriverTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND AutoApply = 1 ")
            Dim bHasForeignLicenseFactor As Boolean = False

            For Each oRow As DataRow In FactorDriverDataRows 'AutoApply factors on Factor Driver table
                'all auto driver auto apply factors (EXCL)
                Select Case oRow.Item("FactorCode").ToString.ToUpper
                    Case "FOREIGN_LICENSE"
                        bHasForeignLicenseFactor = True
                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            If Not oDrv.IsMarkedForDelete Then
                                If oDrv.IndexNum < 98 Then
                                    If CommonRulesFunctions.HasForeignLicense(oDrv) And oDrv.DriverStatus.ToUpper = "ACTIVE" Then 'If oDrv.DLNState = "FN" Or oDrv.DLNState = "IT" Then
                                        If Not FactorOnDriver(oDrv, oRow.Item("FactorCode").ToString) Then
                                            AddDriverFactor(oPolicy, oDrv, oRow.Item("FactorCode").ToString)
                                        End If
                                        'Exit For
                                    End If
                                End If
                            End If
                        Next
                    Case "OTHERSTATE_LICENSE"
                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            If Not oDrv.IsMarkedForDelete Then
                                If oDrv.IndexNum < 98 Then
                                    If Not CommonRulesFunctions.HasForeignLicense(oDrv) And oDrv.DriverStatus.ToUpper = "ACTIVE" And clsCommonFunctions.GetStateCode(oDrv.DLNState.Trim) <> oPolicy.StateCode.Trim Then 'If oDrv.DLNState = "FN" Or oDrv.DLNState = "IT" Then
                                        If Not FactorOnDriver(oDrv, oRow.Item("FactorCode").ToString) Then
                                            AddDriverFactor(oPolicy, oDrv, oRow.Item("FactorCode").ToString)
                                        End If
                                        'Exit For
                                    End If
                                End If
                            End If
                        Next
                    Case "CLN_YOUTH"
                        Dim bAddGoodDrvDiscount As Boolean = False
                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            If Not oDrv.IsMarkedForDelete Then
                                If oDrv.IndexNum < 98 Then
                                    If oDrv.Age <= 18 And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                                        Dim iNumOfBadViolsOnThisStupidDriver As Integer = 0
                                        For Each oViol As clsBaseViolation In oDrv.Violations
                                            If oViol.ViolGroup.ToUpper = "NAF" Or oViol.ViolGroup.ToUpper = "OT1" Or oViol.ViolGroup.ToUpper = "OTC" Or oViol.ViolGroup.ToUpper = "MIN" Then
                                                'this is ok
                                            Else
                                                iNumOfBadViolsOnThisStupidDriver += 1
                                            End If
                                        Next
                                        If iNumOfBadViolsOnThisStupidDriver = 0 Then
                                            bAddGoodDrvDiscount = True
                                        End If
                                        If bAddGoodDrvDiscount Then
                                            If Not FactorOnDriver(oDrv, oRow.Item("FactorCode").ToString) Then
                                                AddDriverFactor(oPolicy, oDrv, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case "NO_VIOL"
                        Dim iNewestViolMonthsOld As Integer

                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            If oDrv.DriverStatus.ToUpper = "ACTIVE" And Not oDrv.SR22 Then
                                CheckViolations(oDrv, oPolicy.CallingSystem, oPolicy.Program, oPolicy.StateCode, oPolicy.RateDate, oPolicy.EffDate, "B")

                                iNewestViolMonthsOld = GetNoViolDiscount(oDrv, oPolicy.EffDate)

                                If iNewestViolMonthsOld >= 36 Then
                                    If Not FactorOnDriver(oDrv, "NO_VIOL") Then
                                        AddDriverFactor(oPolicy, oDrv, "NO_VIOL")
                                    End If
                                Else
                                    If iNewestViolMonthsOld >= 18 Then
                                        If Not FactorOnDriver(oDrv, "NO_VIOL_18") Then
                                            AddDriverFactor(oPolicy, oDrv, "NO_VIOL_18")
                                        End If
                                    Else
                                        If iNewestViolMonthsOld >= 12 Then
                                            If Not FactorOnDriver(oDrv, "NO_VIOL_12") Then
                                                AddDriverFactor(oPolicy, oDrv, "NO_VIOL_12")
                                            End If
                                        End If
                                    End If
                                End If
                            End If

                        Next
                    Case "INEXPERIENCED"
                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                            RemoveInexperiencedViolation(oPolicy, oDrv)

                            If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                                If oDrv.LicenseStateDate <> "#12:00:00 AM#" Then
                                    Dim iMonthDiff As Integer
                                    Dim dtAddedDate As Date
                                    dtAddedDate = oPolicy.EffDate
                                    If oDrv.AddedDate > oPolicy.EffDate Then
                                        dtAddedDate = oDrv.AddedDate
                                    End If


                                    iMonthDiff = DateDiff("m", CDate(oDrv.LicenseStateDate), CDate(dtAddedDate))

                                    ' DateDiff fix:  i.e. if ratedate is 1/15/2009 and license date is 1/30/2008
                                    ' that isn't a full 12 months, so need to subtract off one month
                                    If CDate(oDrv.LicenseStateDate).Month = CDate(dtAddedDate).Month Then
                                        If CDate(dtAddedDate).Day < CDate(oDrv.LicenseStateDate).Day Then
                                            iMonthDiff = iMonthDiff - 1
                                        End If
                                    End If

                                    If iMonthDiff < 36 Then
                                        If Not FactorOnDriver(oDrv, "INEXPERIENCED") Then
                                            AddDriverFactor(oPolicy, oDrv, "INEXPERIENCED")
                                        End If
                                        AddInexperiencedViolation(oPolicy, oDrv)
                                    End If
                                End If
                            End If
                        Next
                End Select
            Next

            RemoveNOVIOLIfINEXPFactor(oPolicy)

            'add vehicle auto apply factors
            FactorVehicleDataRows = oFactorVehicleTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND AutoApply = 1 ")
            For Each oRow As DataRow In FactorVehicleDataRows 'AutoApply factors on Factor Vehicle table
                'all vehicle auto apply factors (IR)
                Select Case oRow.Item("FactorCode").ToString.ToUpper
                    Case "IR" 'This applies to AR6
                        If Not HasSurchargeOverride(oPolicy, "VEH", "IR") Then
                            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                                Dim bIsIneligibleRisk As Boolean = False
                                Dim sReason As String = String.Empty
                                Dim iMaxSymbol As Integer
                                Dim iBusinessUseCount As Integer = 0

                                If Not oVehicle.IsMarkedForDelete Then
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
                                                    bIsIneligibleRisk = True
                                                    Exit For
                                                End If
                                            ElseIf iVehSymbol > iMaxSymbol Then
                                                sReason = "Vehicle with a value over $60,000.- " & oVeh.IndexNum
                                                bIsIneligibleRisk = True
                                                Exit For
                                            End If

                                            '2.     Vehicles rated a with physical damage symbol 25 or higher for model years 2010 and older.
                                            If oVeh.VehicleYear <= 2010 Then
                                                If oVeh.VehicleSymbolCode <> String.Empty Then
                                                    Try
                                                        'If CInt(oVeh.VehicleSymbolCode.Trim) >= 25 And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
                                                        If CInt(oVeh.VehicleSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 And CInt(oVeh.VehicleSymbolCode.Trim) <> 65 And CInt(oVeh.VehicleSymbolCode.Trim) <> 66 And CInt(oVeh.VehicleSymbolCode.Trim) <> 67 And CInt(oVeh.VehicleSymbolCode.Trim) <> 68 Then
                                                            sReason = "Vehicle with physical damage symbol greater than " & iMaxSymbol.ToString & " - " & oVeh.IndexNum
                                                            bIsIneligibleRisk = True
                                                            Exit For
                                                        End If
                                                    Catch ex As Exception
                                                    End Try
                                                End If
                                            End If

                                            '3.     Vehicles rated a with physical damage symbol 58 or higher for model years 2011 and newer.
                                            If oVeh.VehicleYear >= 2011 Then
                                                If oVeh.CollSymbolCode <> String.Empty Then
                                                    Try
                                                        If CInt(oVeh.CollSymbolCode.Trim) > iMaxSymbol And oVeh.VinNo.ToUpper <> "NONOWNER" And CInt(oVeh.VehicleSymbolCode.Trim) <> 999 And CInt(oVeh.VehicleSymbolCode.Trim) <> 965 And CInt(oVeh.VehicleSymbolCode.Trim) <> 966 And CInt(oVeh.VehicleSymbolCode.Trim) <> 967 And CInt(oVeh.VehicleSymbolCode.Trim) <> 968 Then
                                                            sReason = "Vehicle with physical damage symbol greater than " & iMaxSymbol.ToString & " - " & oVeh.IndexNum
                                                            bIsIneligibleRisk = True
                                                            Exit For
                                                        End If
                                                    Catch Ex As Exception
                                                    End Try
                                                End If
                                            End If

                                            '4.     Vehicles garaged out of state.
                                            If Not ValidateVehicleZipCode(oVeh.Zip, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode) Then
                                                sReason = "Vehicles is garaged out of state.- " & oVeh.IndexNum
                                                bIsIneligibleRisk = True
                                                Exit For
                                            End If

                                            '5.     More than 1 Business or Artisan use vehicle. 
                                            For Each oFactor As clsBaseFactor In oVeh.Factors
                                                If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                                                    iBusinessUseCount = iBusinessUseCount + 1
                                                    Exit For
                                                End If
                                            Next

                                            '6.     Vehicles that have a title or registration indicating that the vehicle has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.
                                            '   (These vehicles can be quoted for BI, PD, UMBI, UIMBI and MED coverages). 
                                            If DeterminePhysDamageExists(oVeh) Then
                                                For Each uw As clsUWQuestion In oPolicy.UWQuestions
                                                    If uw.QuestionCode = "307" Then
                                                        If Left(uw.AnswerText.ToUpper, 3) = "YES" Then
                                                            sReason = "Vehicle that has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.- " & oVeh.IndexNum
                                                            bIsIneligibleRisk = True
                                                            Exit For
                                                        End If
                                                    End If
                                                Next
                                            End If

                                            '7.     Vehicles over 15 years old are unacceptable for all physical damage coverage on new policies. 
                                            'If oVeh.VehicleAge > 15 AndAlso DeterminePhysDamageExists(oVeh) Then
                                            If oVeh.VehicleYear < Now.AddYears(-15).Year AndAlso DeterminePhysDamageExists(oVeh) Then
                                                sReason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " & oVeh.IndexNum
                                                bIsIneligibleRisk = True
                                                Exit For
                                            End If

                                            '8.     Vehicles over 40 years old are unacceptable for all coverages.
                                            If oVeh.VehicleAge > 40 Then
                                                sReason = "Vehicles over 40 years old are unacceptable for all coverages.- " & oVeh.IndexNum
                                                bIsIneligibleRisk = True
                                                Exit For
                                            End If
                                        End If
                                    Next

                                    If Not bIsIneligibleRisk Then
                                        If iBusinessUseCount > 1 Then
                                            sReason = "More than 1 Business or Artisan use vehicle."
                                            bIsIneligibleRisk = True
                                        End If
                                    End If
                                End If

                                If bIsIneligibleRisk Then
                                    If Not FactorOnVehicle(oVehicle, oRow.Item("FactorCode").ToString) Then
                                        oPolicy.Notes = (AddNote(oPolicy.Notes, "Warning: A surcharge has been applied to the added vehicle due to: " & oVehicle.IndexNum & ".", "IRSurcharge", "AAF", oPolicy.Notes.Count))
                                        AddVehicleFactor(oPolicy, oVehicle, oRow.Item("FactorCode").ToString)
                                    End If
                                    'Else
                                    '    If VehicleHasIneligibleRisk(oPolicy) Then
                                    '        If Not FactorOnVehicle(oVehicle, oRow.Item("FactorCode").ToString) Then
                                    '            AddVehicleFactor(oPolicy, oVehicle, oRow.Item("FactorCode").ToString)
                                    '        End If
                                    '    End If
                                End If
                            Next
                        End If
                    Case "RENT_TO_OWN"
                    Case "OTC1"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)

                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If oColVehCov.CovDeductible = "250" And oOtcVehCov.CovDeductible = "100" Then
                                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next

                    Case "OTC2"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)


                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If oColVehCov.CovDeductible = "250" And oOtcVehCov.CovDeductible = "250" Then
                                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case "OTC3"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)


                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If oColVehCov.CovDeductible = "250" And oOtcVehCov.CovDeductible = "150" Then
                                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case "OTC4"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)


                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If oColVehCov.CovDeductible = "500" And oOtcVehCov.CovDeductible = "250" Then
                                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case "OTC5"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)


                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If oColVehCov.CovDeductible = "500" And oOtcVehCov.CovDeductible = "500" Then
                                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case "OTC6"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)


                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If oColVehCov.CovDeductible = "1000" And oOtcVehCov.CovDeductible = "500" Then
                                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case "EXCL"
                        ExclFactorType = "VEHICLE"
                        AddExclFactor(oPolicy, oRow.Item("FactorCode").ToString)
                    Case "LIAB_ADJ"
                        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                            If Not oVeh.IsMarkedForDelete Then

                                Dim oColVehCov As clsBaseCoverage = GetCoverage("COL", oPolicy, oVeh)
                                Dim oOtcVehCov As clsBaseCoverage = GetCoverage("OTC", oPolicy, oVeh)

                                If Not oColVehCov Is Nothing Then
                                    If Not oOtcVehCov Is Nothing Then
                                        If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                            AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                                        End If
                                    End If
                                End If
                            End If
                        Next
                End Select
            Next

            ' apply Business Use surcharge if vehicle use is either “Business” or “Artisan”.  
            'add vehicle auto apply factors
            FactorVehicleDataRows = oFactorVehicleTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND FactorCode = 'BUS_USE'")
            For Each oRow As DataRow In FactorVehicleDataRows 'AutoApply factors on Factor Vehicle table
                For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                    If VehicleApplies(oVeh, oPolicy) Then
                        If oVeh.TypeOfUseCode.ToUpper = "ARTISAN" Or oVeh.TypeOfUseCode.ToUpper = "BUSINESS" Or oVeh.TypeOfUseCode.ToUpper = "ART" Then
                            If Not FactorOnVehicle(oVeh, oRow.Item("FactorCode").ToString) Then
                                AddVehicleFactor(oPolicy, oVeh, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    End If
                Next
            Next

            'add policy auto apply factors
            FactorPolicyDataRows = oFactorPolicyTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND AutoApply = 1 ")
            For Each oRow As DataRow In FactorPolicyDataRows 'AutoApply factors on Factor Policy table
                'all auto policy auto apply factors (6_TERM, ADV_QUOTE, EFT_DISC, NO_VIOL, etc)
                Select Case oRow.Item("FactorCode").ToString.ToUpper
                    Case "INELIGIBLE"
                        If PolicyHasIneligibleRisk(oPolicy) And Not HasSurchargeOverride(oPolicy, "POL", "INELIGIBLE") Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "PIF"
                        If Not HasOPF(oPolicy) Then
                            If oPolicy.PayPlanCode = "100" Or oPolicy.ApplyPIFDiscount() Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "HOMEOWNER"
                        If oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER" Or oPolicy.PolicyInsured.OccupancyType.ToUpper = "MOBILEHOMEOWNER" Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "6_TERM"
                        If oPolicy.Term = 6 Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "1_TERM"
                        If oPolicy.Term = 1 Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "3_TERM"
                        If oPolicy.Term = 3 Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "2_TERM"
                        If oPolicy.Term = 2 Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "12_TERM"
                        If oPolicy.Term = 12 Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "MULTICAR"
                        If GetVehicleCount(oPolicy) > 1 Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "TRANSFER"
                        Dim bIneligible As Boolean = False
                        If oPolicy.PolicyInsured.PriorLimitsCode <> "0" Then

                            Dim iMaxTransferDiscountPoints As Integer = 5
                            Try
                                iMaxTransferDiscountPoints = CInt(GetProgramSetting("MaxTransferDiscountPoints", oPolicy))
                            Catch ex As Exception
                                iMaxTransferDiscountPoints = 5
                            End Try

                            Dim iMaxDaysLapse As Integer = -1
                            Try
                                iMaxDaysLapse = CInt(GetProgramSetting("MaxTransferDaysLapse", oPolicy))
                            Catch ex As Exception
                                iMaxDaysLapse = -1
                            End Try

                            Dim iMinPriorMonths As Integer = -1
                            Try
                                iMinPriorMonths = CInt(GetProgramSetting("MinTransferPriorMonths", oPolicy))
                            Catch ex As Exception
                                iMinPriorMonths = -1
                            End Try

                            If iMaxDaysLapse > 0 AndAlso DateDiff(DateInterval.Day, oPolicy.PolicyInsured.PriorExpDate, oPolicy.EffDate) > iMaxDaysLapse Then
                                bIneligible = True
                            End If

                            If iMinPriorMonths > 0 AndAlso oPolicy.PolicyInsured.MonthsPriorContCov < iMinPriorMonths Then
                                bIneligible = True
                            End If

                            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                                If Not oDrv.IsMarkedForDelete Then
                                    If oDrv.DriverStatus.ToUpper = "ACTIVE" Or oDrv.DriverStatus.ToUpper = "PERMITTED" Then
                                        If oDrv.Points > iMaxTransferDiscountPoints Then
                                            bIneligible = True
                                            Exit For
                                        End If
                                    End If
                                End If
                            Next
                            If Not bIneligible Then
                                'If oPolicy.PolicyInsured.DaysLapse > 0 Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "ADV_QUOTE"
                        ' 6/4/2010 this using the origquotedate instead of the note
                        ' also add in the max of 30 months  for LA

                        If (oPolicy.OrigQuoteDate > Date.MinValue And oPolicy.OrigQuoteDate > CDate("1/1/1960")) And DateAdd(DateInterval.Day, 7, oPolicy.OrigQuoteDate) < oPolicy.OrigTermEffDate Then
                            'Limit the Advanced Quote Discount so that it only applies for a maximum of 30 months (4 renewals)
                            'If DateAdd(DateInterval.Month, 30, CDate(oNote.NoteText)) > oPolicy.EffDate Then

                            If oPolicy.PolicyInsured.DaysLapse = 2 Then
                                If oPolicy.PolicyInsured.MaritalStatus.ToUpper = "SINGLE" And oPolicy.PolicyInsured.Age < 19 Then
                                    'no discount
                                Else
                                    If oPolicy.PolicyInsured.MaritalStatus.ToUpper = "MARRIED" And oPolicy.PolicyInsured.Age < 19 Then
                                        'the spouse needs to be over 19 in order to get the discount
                                        For Each oDrv As clsEntityDriver In oPolicy.Drivers
                                            If Not oDrv.IsMarkedForDelete Then
                                                If oDrv.IndexNum < 98 Then
                                                    If oDrv.RelationToInsured.ToUpper = "SPOUSE" Then
                                                        If oDrv.Age >= 19 Then
                                                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                                            End If
                                                            Exit For
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Next
                                    Else
                                        If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                            AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Case "EFT_DISC", "EFT"
                        If oPolicy.PayPlanCode <> "100" And Not oPolicy.ApplyPIFDiscount() Then
                            'the 1st payment is the downpayment
                            If oPolicy.IsEFT Then
                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                    Case "NO_VIOL"
                        Dim bAddFactor As Boolean = True
                        If oPolicy.PolicyInsured.DaysLapse <> 0 Then
                            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                                If Not oDrv.IsMarkedForDelete Then
                                    If oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                                        If oDrv.IndexNum < 98 Then
                                            If oDrv.Age < 16 Then
                                                bAddFactor = False
                                                Exit For
                                            End If
                                            For Each oViol As clsBaseViolation In oDrv.Violations
                                                Dim bIgnoreViol As Boolean = False
                                                ' 6/14/2010 Need to skip if this is a UDR violation
                                                If CommonRulesFunctions.StateInfoContains("COMBINEDDRIVER", "VIOLGROUPIGNORE", oViol.ViolGroup, oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.Program) Then
                                                    'ignore viol
                                                    bIgnoreViol = True
                                                End If

                                                ' 6/14/2010 Need to skip if this is a UDR violation
                                                If CommonRulesFunctions.StateInfoContains("NOVIOL", "VIOLGROUPIGNORE", oViol.ViolGroup, oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.Program) Then
                                                    'ignore viol
                                                    bIgnoreViol = True
                                                End If


                                                Dim oStateInfoTable As DataTable = Nothing
                                                Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

                                                oStateInfoTable = oStateInfoDataSet.Tables(0)

                                                Dim StateInfoRows() As DataRow
                                                StateInfoRows = oStateInfoTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup='MERIT' AND ItemCode='IGNORE' AND ItemSubCode='ADMINMSG'")

                                                Dim dtIgnoreAdminStartDate As Date = Date.MinValue
                                                For Each Row As DataRow In StateInfoRows
                                                    dtIgnoreAdminStartDate = CDate(Row("ItemValue"))
                                                Next

                                                ' If this is an administration message, check the state info row to see when to start
                                                ' ignoring this violation, if no row then always ignore
                                                If oViol.ViolDesc.ToUpper.Trim = "ADMINISTRATION MESSAGE" Then
                                                    If dtIgnoreAdminStartDate = Date.MinValue OrElse oPolicy.RateDate >= dtIgnoreAdminStartDate Then
                                                        bIgnoreViol = True
                                                    End If
                                                End If


                                                If Not bIgnoreViol Then
                                                    If oPolicy.StateCode = "03" Then
                                                        If DateDiff(DateInterval.Month, oViol.ViolDate, oPolicy.EffDate) < 36 Then
                                                            bAddFactor = False
                                                            Exit For
                                                        End If
                                                    Else
                                                        If DateDiff(DateInterval.Month, oViol.ViolDate, oPolicy.EffDate) < 35 Then
                                                            bAddFactor = False
                                                            Exit For
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                            Next
                        Else
                            bAddFactor = False
                        End If
                        If bAddFactor Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "COMPANION_POLICY"
                        If oPolicy.CompanionHOMCarrierName <> "" Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "COMPANION_FLOOD"
                        Dim oNote = GetNote(oPolicy, "DIS", "Discount:Companion Flood")
                        If Not oNote Is Nothing Then
                            oPolicy.CompanionFloodCarrierName = "IMPERIAL"
                        End If

                        If oPolicy.CompanionFloodCarrierName <> "" Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "COMPANION_HOME"
                        If oPolicy.CompanionHOMCarrierName <> "" Then
                            If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                            End If
                        End If
                    Case "EXCL"
                        ExclFactorType = "POLICY"
                        AddExclFactor(oPolicy, oRow.Item("FactorCode").ToString)
                    Case "PIP_X_WL_NIO"
                        If oPolicy.VehicleUnits.Count > 0 Then
                            For Each oCoverage As clsPACoverage In oPolicy.VehicleUnits(0).Coverages
                                If oCoverage.CovGroup = "PIP" Then
                                    If oCoverage.UWQuestions.Count > 0 Then
                                        For Each oQuestion As clsUWQuestion In oCoverage.UWQuestions
                                            If oQuestion.AnswerText.ToUpper.Trim = "YES" Then
                                                If oCoverage.CovCode.Contains("NIO") Then
                                                    If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                                        AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                                    End If
                                                End If
                                            ElseIf oQuestion.AnswerText.ToUpper.Trim.Contains("NIO") Then
                                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            Next
                        End If
                    Case "PIP_X_WL_NIRR"
                        If oPolicy.VehicleUnits.Count > 0 Then
                            For Each oCoverage As clsPACoverage In oPolicy.VehicleUnits(0).Coverages
                                If oCoverage.CovGroup = "PIP" Then
                                    If oCoverage.UWQuestions.Count > 0 Then
                                        For Each oQuestion As clsUWQuestion In oCoverage.UWQuestions
                                            If oQuestion.AnswerText.ToUpper.Trim = "YES" Then
                                                If oCoverage.CovCode.Contains("NIRR") Then
                                                    If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                                        AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                                    End If
                                                End If
                                            ElseIf oQuestion.AnswerText.ToUpper.Trim.Contains("NIRR") Then
                                                If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            Next
                        End If
                    Case "COVERAGE_FEE"
                        If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                            AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                        End If
                    Case "CFRD"
                        If Not FactorOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then
                            If oPolicy.Type.ToUpper = "RENEWAL" Then
                                If Not CheckForHasClaimsViol(oPolicy) Then
                                    AddPolicyFactor(oPolicy, oRow.Item("FactorCode").ToString)
                                End If
                            End If
                        End If
                End Select
            Next

            ' Added for programs using the policy discount matrix
            ' so that print can know if homeowner/pif/multicar disocunt was applied
            If IsUsingPolicyDiscountMatrix(oPolicy) Then

                ' Remove existing discounts
                For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                    If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = "PIF" Then
                        oPolicy.PolicyFactors.RemoveAt(i)
                        Exit For
                    End If
                Next

                For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                    If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = "HOMEOWNER" Then
                        oPolicy.PolicyFactors.RemoveAt(i)
                        Exit For
                    End If
                Next

                For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                    If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = "MULTICAR" Then
                        oPolicy.PolicyFactors.RemoveAt(i)
                        Exit For
                    End If
                Next

                ' Add discoutns (if applicable)
                If Not HasOPF(oPolicy) Then
                    If oPolicy.PayPlanCode = "100" Or oPolicy.ApplyPIFDiscount() Then
                        If Not FactorOnPolicy(oPolicy, "PIF") Then
                            AddPolicyFactor(oPolicy, "PIF")
                        End If
                    End If
                End If

                If oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER" Or oPolicy.PolicyInsured.OccupancyType.ToUpper = "MOBILEHOMEOWNER" Then
                    If Not FactorOnPolicy(oPolicy, "HOMEOWNER") Then
                        AddPolicyFactor(oPolicy, "HOMEOWNER")
                    End If
                End If

                If GetVehicleCount(oPolicy) > 1 Then
                    If Not FactorOnPolicy(oPolicy, "MULTICAR") Then
                        AddPolicyFactor(oPolicy, "MULTICAR")
                    End If
                End If
            End If
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally

        End Try

    End Sub

#Region "UWW"
    Public Sub CheckSalvagedUWW(ByRef oPolicy As clsPolicyPPA)
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
                                If oPolicy.Program.ToUpper = "DIRECT" Then
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk:  Re-built, salvaged or water damaged vehicles are unacceptable.", "SAL", "IER", oPolicy.Notes.Count))
                                Else
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed:  Re-built, salvaged or water damaged vehicle.", "SAL", "UWW", oPolicy.Notes.Count))
                                End If

                            End If
                        End If
                End Select
            Next
        End If
    End Sub

    Public Sub CheckClaimActivity(ByVal oPolicy As clsPolicyPPA)
        Dim bViolationIERExists As Boolean = False
        Dim bHasPriorClaims As Boolean = False
        For Each note As clsBaseNote In oPolicy.Notes
            If note.NoteDesc = "MaxDriverPoints" OrElse note.NoteDesc = "MaxDriverViols" _
                OrElse note.NoteDesc = "PolPointsover2" OrElse note.NoteDesc = "PolPointsoverMax" _
                OrElse note.NoteDesc = "ChargeableViolationCount" OrElse note.NoteDesc = "CombinedPointsCount" _
                OrElse note.NoteDesc = "ChargeableDWICount" OrElse note.NoteDesc = "DriverPoints" _
                OrElse note.NoteDesc = "DriverViolCount" OrElse note.NoteDesc = "DriverPtTot" Then

                bViolationIERExists = True
                Exit For
            End If
        Next

        If Not bViolationIERExists Then
            For Each note As clsBaseNote In oPolicy.Notes
                If note.NoteDesc = "ClaimActivity" Then
                    If Integer.Parse(note.NoteText.Split(" ")(0)) >= 3 Then
                        bHasPriorClaims = True
                    End If
                End If
            Next
        End If

        If bHasPriorClaims Then
            If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Three or more claims within the last 35 months were found on the Underwriting reports.  Please call 866-874-2741 to speak to an Imperial agent.", "ClaimActivity", "UWW", oPolicy.Notes.Count))
            Else
                oPolicy.Notes = (AddNote(oPolicy.Notes, "Underwriting Approval Needed: Three or more claims within the last 35 months were found on the A+ report.", "ClaimActivity", "UWW", oPolicy.Notes.Count))
            End If
        End If
    End Sub

    Public Sub CheckClaimActivityActiveDriversOnly(ByVal policy As clsPolicyPPA)
        Dim violationIERExists As Boolean = False
        Dim claimCount As Integer = 0

        For Each note As clsBaseNote In policy.Notes
            If note.NoteDesc = "MaxDriverPoints" OrElse note.NoteDesc = "MaxDriverViols" _
                OrElse note.NoteDesc = "PolPointsover2" OrElse note.NoteDesc = "PolPointsoverMax" _
                OrElse note.NoteDesc = "ChargeableViolationCount" OrElse note.NoteDesc = "CombinedPointsCount" _
                OrElse note.NoteDesc = "ChargeableDWICount" OrElse note.NoteDesc = "DriverPoints" _
                OrElse note.NoteDesc = "DriverViolCount" OrElse note.NoteDesc = "DriverPtTot" Then

                violationIERExists = True
                Exit For
            End If
        Next

        If Not violationIERExists Then
            For Each driver As clsEntityDriver In policy.Drivers
                If driver.DriverStatus.ToUpper.Trim = "ACTIVE" Or driver.DriverStatus.ToUpper.Trim = "PERMITTED" Then
                    For Each viol As clsBaseViolation In driver.Violations
                        If viol.ViolSourceCode.ToUpper.Trim = "U" _
                            AndAlso viol.ViolDate >= DateAdd(DateInterval.Month, -36, policy.EffDate) Then
                            claimCount += 1
                        End If
                    Next
                End If
            Next
        End If

        If claimCount >= 3 Then
            policy.Notes = (AddNote(policy.Notes, "Underwriting Approval Needed: Three or more claims within the last 35 months were found on the A+ report.", "ClaimActivity", "UWW", policy.Notes.Count))
        End If
    End Sub

#End Region

    Public Function CheckForHasClaimsViol(ByVal oPolicy As clsPolicyPPA) As Boolean
        For Each drv In oPolicy.Drivers
            For Each viol In drv.Violations
                'Do they have the violation that disallows the Claims Free Discount?
                If viol.ViolTypeCode.Trim.Contains("99998") Then
                    Return True
                End If
            Next
        Next

        Return False

    End Function

    Public Overridable Function VehicleHasIneligibleRisk(ByVal oPolicy As clsPolicyPPA) As Boolean
        ' Should be overriden for each state
        Return False
    End Function

    Public Overridable Function PolicyHasIneligibleRisk(ByVal oPolicy As clsPolicyPPA) As Boolean
        ' Should be overriden for each state
        Return False
    End Function

    Public Overridable Sub AddExclFactor(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String)
        Dim bHasExcl As Boolean = False
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete Then
                If oDrv.IndexNum < 98 Then
                    If oDrv.RelationToInsured.ToUpper = "SPOUSE" Or oDrv.RelationToInsured.ToUpper = "PARENT" Then
                        If oDrv.DriverStatus.ToUpper = "EXCLUDED" Then
                            bHasExcl = True
                            Exit For
                        End If
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

    Private Function IsUsingPolicyDiscountMatrix(ByVal oPolicy As clsPolicyPPA) As Boolean
        ' TODO: Finish
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim bIsUsingPolicyDiscountMatrix As Boolean = False

        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Try
            oConn.Open()
            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorPolicyDiscountMatrix with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND UWTier = @UWTier "
                sSql = sSql & " AND MultiCar = @MultiCar "
                sSql = sSql & " AND PaidInFull = @PaidInFull "
                sSql = sSql & " AND HomeOwner = @HomeOwner "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.UWTier
                cmd.Parameters.Add("@MultiCar", SqlDbType.VarChar, 1).Value = IIf(GetVehicleCount(oPolicy) > 1, "Y", "N")


                If HasOPF(oPolicy) Then
                    cmd.Parameters.Add("@PaidInFull", SqlDbType.VarChar, 1).Value = "N"
                Else
                    Dim sPIF As String = "N"
                    If oPolicy.PayPlanCode = "100" Then
                        sPIF = "Y"
                    Else
                        If oPolicy.ApplyPIFDiscount Then
                            sPIF = "Y"
                        End If
                    End If
                    cmd.Parameters.Add("@PaidInFull", SqlDbType.VarChar, 1).Value = sPIF
                End If
                cmd.Parameters.Add("@HomeOwner", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER", "Y", "N")

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    bIsUsingPolicyDiscountMatrix = True
                End If
            End Using

            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

        Return bIsUsingPolicyDiscountMatrix
    End Function

    Public Function GetVehicleCount(ByVal oPolicy As clsPolicyPPA) As Integer
        Dim count As Integer = 0

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                count = count + 1
            End If
        Next

        Return count
    End Function

    Public Sub RemoveAutoApplyFactors(ByVal oPolicy As clsPolicyPPA, ByVal oFactorTable As DataTable)

        Dim DataRows() As DataRow

        DataRows = oFactorTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND AutoApply = 1 ")

        Select Case oFactorTable.TableName.ToUpper
            Case "FACTORPOLICY"
                For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
                    For Each oRow As DataRow In DataRows
                        If oRow.Item("FactorCode").ToString.ToUpper = oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper Then
                            oPolicy.PolicyFactors.RemoveAt(i)
                            Exit For
                        End If
                    Next
                Next
            Case "FACTORDRIVER"
                For Each oDrv As clsEntityDriver In oPolicy.Drivers
                    If Not oDrv.IsMarkedForDelete Then
                        For i As Integer = oDrv.Factors.Count - 1 To 0 Step -1
                            For Each oRow As DataRow In DataRows
                                If oRow.Item("FactorCode").ToString.ToUpper = oDrv.Factors.Item(i).FactorCode.ToUpper Then
                                    oDrv.Factors.RemoveAt(i)
                                    Exit For
                                End If
                            Next
                        Next
                    End If
                Next
            Case "FACTORVEHICLE"
                For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                    If Not oVeh.IsMarkedForDelete Then
                        For i As Integer = oVeh.Factors.Count - 1 To 0 Step -1
                            For Each oRow As DataRow In DataRows
                                If oRow.Item("FactorCode").ToString.ToUpper = oVeh.Factors.Item(i).FactorCode.ToUpper Then
                                    oVeh.Factors.RemoveAt(i)
                                    Exit For
                                End If
                            Next
                        Next
                    End If
                Next
        End Select
    End Sub

    Public Overloads Sub AddPolicyFactor(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String)
        Dim oPF As New clsBaseFactor
        oPF.FactorCode = sFactorCode
        oPF.IndexNum = oPolicy.PolicyFactors.Count + 1
        oPF.SystemCode = sFactorCode
        oPF.FactorNum = oPolicy.PolicyFactors.Count + 1
        oPF.FactorAmt = 0
        oPF.FactorDesc = GetFactorDesc(oPolicy, sFactorCode, "POLICY")
        oPF.FactorName = oPF.FactorDesc
        oPolicy.PolicyFactors.Add(oPF)
    End Sub

    Public Function GetFactorDesc(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String, ByVal sFactorType As String) As String

        Dim sFactorDesc As String = ""
        Dim DataRows() As DataRow
        Dim oFactorTable As DataTable = Nothing
        Dim oFactorDataSet As New DataSet

        Select Case sFactorType.ToUpper
            Case "POLICY"
                oFactorDataSet = LoadFactorPolicyTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            Case "DRIVER"
                oFactorDataSet = LoadFactorDriverTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
            Case "VEHICLE"
                oFactorDataSet = LoadFactorVehicleTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
        End Select

        oFactorTable = oFactorDataSet.Tables(0)
        DataRows = oFactorTable.Select("Program='" & oPolicy.Program & "'" & " AND FactorCode='" & sFactorCode & "'")

        For Each oRow As DataRow In DataRows
            sFactorDesc = oRow("Description").ToString
            Exit For
        Next

        Return sFactorDesc

    End Function

    Public Sub AddDriverFactor(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver, ByVal sFactorCode As String)
        Dim oDF As New clsBaseFactor
        oDF.FactorCode = sFactorCode
        oDF.IndexNum = oDriver.Factors.Count + 1
        oDF.SystemCode = sFactorCode
        oDF.FactorNum = oDriver.Factors.Count + 1
        oDF.FactorAmt = 0
        oDF.FactorDesc = GetFactorDesc(oPolicy, sFactorCode, "DRIVER")
        oDF.FactorName = oDF.FactorDesc
        oDriver.Factors.Add(oDF)
    End Sub

    Public Sub AddVehicleFactor(ByVal oPolicy As clsPolicyPPA, ByVal oVehicle As clsVehicleUnit, ByVal sFactorCode As String)
        Dim oVF As New clsVehicleFactor
        oVF.FactorCode = sFactorCode
        oVF.IndexNum = oVehicle.Factors.Count + 1
        oVF.SystemCode = sFactorCode
        oVF.FactorNum = oVehicle.Factors.Count + 1
        oVF.FactorAmt = 0
        oVF.FactorDesc = GetFactorDesc(oPolicy, sFactorCode, "VEHICLE")
        oVF.FactorName = oVF.FactorDesc
        oVehicle.Factors.Add(oVF)
    End Sub

    Public Function FactorOnDriver(ByVal oDriver As clsEntityDriver, ByVal sFactorCode As String) As Boolean

        For Each oFactor As clsBaseFactor In oDriver.Factors
            If oFactor.FactorCode.ToString.ToUpper = sFactorCode.ToString.ToUpper Then
                Return True
            End If
        Next

        Return False

    End Function

    Public Function FactorOnVehicle(ByVal oVehicle As clsVehicleUnit, ByVal sFactorCode As String) As Boolean

        For Each oFactor As clsBaseFactor In oVehicle.Factors
            If oFactor.FactorCode.ToString.ToUpper = sFactorCode.ToString.ToUpper Then
                Return True
            End If
        Next

        Return False

    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadFactorPolicyTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Program, Coverage, FactorCode, Description, AutoApply, Factor, FactorType "
                sSql &= " FROM pgm" & sProduct & sStateCode & "..FactorPolicy with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, FactorCode, Coverage "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "FactorPolicy")
                Return oDS
            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadFactorDriverTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, Coverage, FactorCode, Description, AutoApply, Factor, FactorType "
                sSql &= " FROM pgm" & sProduct & sStateCode & "..FactorDriver with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, FactorCode, Coverage "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "FactorDriver")
                Return oDS
            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadFactorVehicleTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Program, Coverage, FactorCode, Description, AutoApply, Factor, FactorType "
                sSql &= " FROM pgm" & sProduct & sStateCode & "..FactorVehicle with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, FactorCode, Coverage "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "FactorVehicle")
                Return oDS
            End Using

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Sub AddViolation(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver, ByVal sViolTypeCode As String, ByVal sViolDesc As String, ByVal sViolTypeIndicator As String, ByVal sViolGroup As String, ByVal sViolSourceCode As String, ByVal dtViolDate As Date)

        Dim oViolation As clsBaseViolation = New clsBaseViolation
        oViolation.ViolTypeCode = sViolTypeCode
        oViolation.ViolDesc = sViolDesc
        oViolation.ViolTypeIndicator = sViolTypeIndicator
        oViolation.ViolGroup = sViolGroup
        oViolation.ViolSourceCode = sViolSourceCode
        oViolation.AtFault = False
        oViolation.ViolDate = dtViolDate
        oViolation.ConvictionDate = dtViolDate
        oViolation.Chargeable = True
        oViolation.IndexNum = oDriver.Violations.Count + 1
        oViolation.AddToXML = True

        oDriver.Violations.Add(oViolation)

    End Sub

    Public Sub RemoveViolation(ByVal oPolicy As clsPolicyPPA, ByVal oDriver As clsEntityDriver, ByVal sViolTypeCode As String, ByVal sViolDesc As String, ByVal sViolTypeIndicator As String, ByVal sViolGroup As String, ByVal sViolSourceCode As String)

        For Each oViol As clsBaseViolation In oDriver.Violations
            If oViol.ViolTypeCode = sViolTypeCode And oViol.ViolDesc = sViolDesc And oViol.ViolTypeIndicator = sViolTypeIndicator And oViol.ViolGroup = sViolGroup And oViol.ViolSourceCode = sViolSourceCode Then
                oDriver.Violations.Remove(oViol)
                Exit For
            End If
        Next

    End Sub

    ' FactorType: POL,VEH, or DRV
    Public Function HasAddSurchargeOverride(ByVal Policy As clsBasePolicy) As Boolean

        Dim bHasOverride As Boolean = False


        'lookup in override table
        Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Try
            Dim sSQL As String = ""
            sSQL = " SELECT IneligibleRiskYN FROM PasCarrier..PolicyPAuto with(nolock) "
            sSQL &= " WHERE PolicyNo = @PolicyID "
            sSQL &= " AND TermEffDate = @TermEffDate"
            sSQL &= " AND PolicyTransactionNum = @TransNum "

            Dim cmd As New SqlCommand(sSQL, cn)

            cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar, 50).Value = Policy.PolicyID
            cmd.Parameters.Add("@TermEffDate", SqlDbType.DateTime).Value = Policy.EffDate
            cmd.Parameters.Add("@TransNum", SqlDbType.Int).Value = Policy.TransactionNum

            'Open the connection
            cn.Open()

            bHasOverride = CBool(cmd.ExecuteScalar())
        Catch Ex As Exception
            bHasOverride = False
        Finally
            cn.Close()
            cn.Dispose()
        End Try

        Return bHasOverride
    End Function

    ' FactorType: POL,VEH, or DRV
    Public Function HasSurchargeOverride(ByVal Policy As clsBasePolicy, ByVal sFactorType As String, ByVal sFactorCode As String) As Boolean

        Dim bHasOverride As Boolean = False


        'lookup in override table
        Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Try
            Dim sSQL As String = ""
            sSQL = " SELECT OverrideID FROM pgm" & Policy.Product & Policy.StateCode & "..RiskOverride  with(nolock) "
            sSQL &= " WHERE "
            If Policy.QuoteID.Trim <> "" Then
                sSQL &= "(QuoteID = @QuoteID "
            Else
                sSQL &= "("
            End If
            If Policy.PolicyID.Trim <> "" Then
                sSQL &= IIf(Policy.QuoteID.Trim <> "", "or PolicyNo = @PolicyID )", " PolicyNo = @PolicyID )")
            Else
                sSQL &= ")"
            End If
            sSQL &= " AND DeletedFlag = 0 "
            sSQL &= " AND RiskCode = @RiskCode "

            Dim cmd As New SqlCommand(sSQL, cn)
            cmd.Parameters.Add("@QuoteID", SqlDbType.VarChar, 50).Value = Policy.QuoteID
            cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar, 50).Value = Policy.PolicyID
            cmd.Parameters.Add("@RiskCode", SqlDbType.VarChar, 50).Value = sFactorType & ":" & sFactorCode
            ' Risk code will be saved to the RiskOverride table as TYPE:FactorCode.  EX: "VEH:IR"

            'Open the connection
            cn.Open()

            Dim oReader As SqlDataReader
            oReader = cmd.ExecuteReader()

            If oReader.HasRows Then
                bHasOverride = True
            End If
        Catch Ex As Exception
            Throw New Exception("Error with RiskOverride Lookup", Ex)
        Finally
            cn.Close()
            cn.Dispose()
        End Try

        Return bHasOverride
    End Function

    Private Shared Function CompareViolationsByDate(ByVal x As clsBaseViolation, ByVal y As clsBaseViolation) As Integer

        If x Is Nothing Then
            If y Is Nothing Then
                ' If x is Nothing and y is Nothing, they're
                ' equal. 
                Return 0
            Else
                ' If x is Nothing and y is not Nothing, y
                ' is greater. 
                Return -1
            End If
        Else
            ' If x is not Nothing...
            '
            If y Is Nothing Then
                ' ...and y is Nothing, x is greater.
                Return 1
            Else
                ' ...and y is not Nothing, compare the 
                ' lengths of the two strings.
                '
                Return x.ViolDate.CompareTo(y.ViolDate)
            End If
        End If

    End Function

    Private Shared Function CompareViolationsByIndex(ByVal x As clsBaseViolation, ByVal y As clsBaseViolation) As Integer

        If x Is Nothing Then
            If y Is Nothing Then
                ' If x is Nothing and y is Nothing, they're
                ' equal. 
                Return 0
            Else
                ' If x is Nothing and y is not Nothing, y
                ' is greater. 
                Return -1
            End If
        Else
            ' If x is not Nothing...
            '
            If y Is Nothing Then
                ' ...and y is Nothing, x is greater.
                Return 1
            Else
                ' ...and y is not Nothing, compare the 
                ' lengths of the two strings.
                '
                Return x.IndexNum.CompareTo(y.IndexNum)
            End If
        End If

    End Function

    Public Sub CheckViolations(ByVal oDrv As clsEntityDriver, ByVal sCallingSystem As String, ByVal sProgram As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal dtEffDate As Date, ByVal sAppliesToCode As String)
        Dim ratingrules2 As New CommonRulesFunctions
        Dim sProduct As String = "2"

        Dim DataRows() As DataRow
        Dim oViolGroupsTable As DataTable = Nothing
        Dim oViolGroupsDataSet As DataSet = ratingrules2.LoadCodeViolGroupsTable(sProduct, sStateCode, dtRateDate, sAppliesToCode)
        oViolGroupsTable = oViolGroupsDataSet.Tables(0)
        Dim bIgnoreViol As Boolean = False

        Dim dtViolations As DataTable = New DataTable("Violations")
        Dim dcViolationIndex As DataColumn = New DataColumn("ViolationIndex", Type.GetType("System.Int32"))
        Dim dcViolTypeCode As DataColumn = New DataColumn("ViolTypeCode", Type.GetType("System.String"))
        Dim dcViolGroup As DataColumn = New DataColumn("ViolGroup", Type.GetType("System.String"))
        Dim dcViolDate As DataColumn = New DataColumn("ViolDate", Type.GetType("System.DateTime"))
        Dim dcChargeable As DataColumn = New DataColumn("Chargeable", Type.GetType("System.Boolean"))
        Dim dcIsFirst As DataColumn = New DataColumn("IsFirst", Type.GetType("System.Boolean"))
        Dim dcIsSecond As DataColumn = New DataColumn("IsSecond", Type.GetType("System.Boolean"))
        Dim dcPoints As DataColumn = New DataColumn("Points", Type.GetType("System.Int32"))

        dtViolations.Columns.Add(dcViolationIndex)
        dtViolations.Columns.Add(dcViolTypeCode)
        dtViolations.Columns.Add(dcViolGroup)
        dtViolations.Columns.Add(dcViolDate)
        dtViolations.Columns.Add(dcChargeable)
        dtViolations.Columns.Add(dcIsFirst)
        dtViolations.Columns.Add(dcIsSecond)
        dtViolations.Columns.Add(dcPoints)

        oDrv.Points = 0
        dtViolations.Rows.Clear()

        Dim bIgnoreOutofAgeRange As Boolean = False
        bIgnoreOutofAgeRange = CommonRulesFunctions.StateInfoContainsRateDate("VIOLATION", "OCCURRENCE", "IGNOREOUTOFAGERANGE", sProduct & sStateCode, sAppliesToCode, dtRateDate)

        ' sort  by violation date if the stateinfo row exists, otherwise do need to rate like we used to (not sorting violations, counting old violation for occurrence)
        If bIgnoreOutofAgeRange Then
            oDrv.Violations.Sort(AddressOf CompareViolationsByDate)
        End If

        For i As Integer = 0 To oDrv.Violations.Count - 1
            Dim oViol As clsBaseViolation = oDrv.Violations.Item(i)
            Dim dtNewDate As Date = dtEffDate

            If CommonRulesFunctions.StateInfoContains("ALLOW", "RECALC", "POINTS", sProduct & sStateCode, sAppliesToCode) Then
                Dim sViolGroup As String
                sViolGroup = CommonRulesFunctions.LoadViolCodeGroup(oViol.ViolTypeCode, sProgram, sProduct, sStateCode, dtRateDate, sAppliesToCode)

                If sViolGroup Is Nothing Then
                    sViolGroup = String.Empty
                End If

                If Len(sViolGroup.Trim) > 0 Then
                    oViol.ViolGroup = sViolGroup
                End If
            End If

            If oViol.ViolGroup.ToUpper = "UDR" And oViol.ViolTypeCode = "55559" Then
                If sCallingSystem.ToUpper = "WEBRATER" Or sCallingSystem.ToUpper = "BRIDGE" Then
                    dtNewDate = dtEffDate
                    oViol.ViolDate = dtNewDate.AddDays(-1)
                End If
            End If
            oViol.ConvictionDate = oViol.ViolDate

            Dim drViolation As DataRow = dtViolations.NewRow
            drViolation("ViolationIndex") = oViol.IndexNum
            drViolation("ViolTypeCode") = oViol.ViolTypeCode.Trim
            drViolation("ViolGroup") = oViol.ViolGroup.Trim
            drViolation("ViolDate") = oViol.ViolDate
            drViolation("Chargeable") = True 'oViol.Chargeable
            Dim iMonthsOld = ratingrules2.CalculateViolAge(oViol.ViolDate, dtEffDate)

            ' OLE Lets you run mvr for a new driver
            ' those could be today's date, yet calcviolage would return a negative
            ' since we are basing off of the policy eff date
            If iMonthsOld < 0 Then
                iMonthsOld = 0
            End If

            DataRows = oViolGroupsTable.Select("Program = '" & sProgram & "' AND ViolGroup = '" & oViol.ViolGroup.Trim & "' AND MinAgeViol <= " & iMonthsOld & " AND MaxAgeViol > " & iMonthsOld)
            If DataRows.Length = 0 Then
                drViolation("Chargeable") = False
                drViolation("IsFirst") = False
                drViolation("IsSecond") = False
                drViolation("Points") = 0
            Else
                If drViolation("Chargeable") Then
                    For Each oRow As DataRow In DataRows
                        Dim iOccurrence As Integer = ratingrules2.GetOccurrence(oDrv.Violations, oViol.ViolGroup.Trim, i, CInt(oRow("MinAgeViol").ToString), CInt(oRow("MaxAgeViol").ToString), iMonthsOld, dtEffDate, bIgnoreOutofAgeRange)
                        Select Case iOccurrence
                            Case 1
                                drViolation("IsFirst") = True
                                drViolation("IsSecond") = False
                                drViolation("Points") = CInt(oRow("FirstOccurrence").ToString)

                                If drViolation("ViolGroup") = "UDR" Then
                                    Dim bBypassUDR As Boolean = False
                                    bBypassUDR = CommonRulesFunctions.StateInfoContains("IGNOREVIOL", "UDR", "UDR", "2" & sStateCode, sAppliesToCode, sProgram)

                                    If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And oDrv.Age <= 18 Then
                                        'ignore viol
                                        bIgnoreViol = True
                                    End If
                                    If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And CommonRulesFunctions.HasForeignLicense(oDrv) Then
                                        'ignore viol
                                        bIgnoreViol = True
                                    End If

                                    If bIgnoreViol Then
                                        drViolation("Points") = 0
                                    End If
                                End If
                            Case 2
                                drViolation("IsFirst") = False
                                drViolation("IsSecond") = True
                                drViolation("Points") = CInt(oRow("SecondOccurrence").ToString)

                                If drViolation("ViolGroup") = "UDR" Then
                                    Dim bBypassUDR As Boolean = False
                                    bBypassUDR = CommonRulesFunctions.StateInfoContains("IGNOREVIOL", "UDR", "UDR", "2" & sStateCode, sAppliesToCode, sProgram)

                                    If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And oDrv.Age <= 18 Then
                                        'ignore viol
                                        bIgnoreViol = True
                                    End If
                                    If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And CommonRulesFunctions.HasForeignLicense(oDrv) Then
                                        'ignore viol
                                        bIgnoreViol = True
                                    End If

                                    If bIgnoreViol Then
                                        drViolation("Points") = 0
                                    End If
                                End If
                            Case Else
                                drViolation("IsFirst") = False
                                drViolation("IsSecond") = False
                                drViolation("Points") = CInt(oRow("AddlOccurrence").ToString)

                                If drViolation("ViolGroup") = "UDR" Then
                                    Dim bBypassUDR As Boolean = False
                                    bBypassUDR = CommonRulesFunctions.StateInfoContains("IGNOREVIOL", "UDR", "UDR", "2" & sStateCode, sAppliesToCode, sProgram)

                                    If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And oDrv.Age <= 18 Then
                                        'ignore viol
                                        bIgnoreViol = True
                                    End If
                                    If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And CommonRulesFunctions.HasForeignLicense(oDrv) Then
                                        'ignore viol
                                        bIgnoreViol = True
                                    End If

                                    If bIgnoreViol Then
                                        drViolation("Points") = 0
                                    End If
                                End If
                        End Select
                    Next
                End If
            End If

            dtViolations.Rows.Add(drViolation)
        Next

        ' SAME DAY VIOLATION HANDLING
        Dim dtViolDates As DataTable = CommonRulesFunctions.SelectDistinct(dtViolations, "ViolDate")
        For Each drViolDate As DataRow In dtViolDates.Rows
            Dim dViolDate As DateTime = drViolDate("ViolDate")

            '' Same Day and Same Viol Code: Only charge first occurrence
            'Dim drDuplicateSameDayViols As DataRow() = dtViolations.Select("Chargeable = 'True' and ViolDate = '" & dViolDate & "'", "ViolTypeCode ASC, ViolationIndex ASC")
            'If drDuplicateSameDayViols.Length > 1 Then
            '    Dim sCurrentViolCode As String = ""
            '    For i As Integer = 0 To drDuplicateSameDayViols.Length - 1
            '        Dim drSameDayViol As DataRow = drDuplicateSameDayViols(i)
            '        If drSameDayViol.Item("ViolTypeCode").ToString() <> sCurrentViolCode Then
            '            ' First of this Viol Code. Set the temp code and move on
            '            sCurrentViolCode = drSameDayViol.Item("ViolTypeCode").ToString()
            '        Else
            '            ' Duplicate Viol Code, mark this one as non-chargeable
            '            For Each drTableViol As DataRow In dtViolations.Rows
            '                If drTableViol.Equals(drSameDayViol) Then
            '                    drTableViol("Chargeable") = False
            '                    drTableViol("Points") = 0
            '                    Exit For
            '                End If
            '            Next
            '        End If
            '    Next
            'End If

            ' Same Day, but differnt Viol Code(s): Only charge viol with highest points
            Dim drViols As DataRow() = dtViolations.Select("Chargeable = 'True' and ViolDate = '" & dViolDate & "'", "Points DESC")
            If drViols.Length = 1 Then
                ' Don't have to change anything.  There is only one violations for that date
            Else
                For i As Integer = 0 To drViols.Length - 1
                    Dim drViol As DataRow = drViols(i)
                    If i = 0 Then
                        ' Don't have to change anything. This should be the violations with the highest point value for that date
                    Else
                        For Each drTableViol As DataRow In dtViolations.Rows
                            If drTableViol.Equals(drViol) Then
                                drTableViol("Chargeable") = False
                                drTableViol("Points") = 0
                                Exit For
                            End If
                        Next
                    End If
                Next
            End If
        Next

        For Each drViolation As DataRow In dtViolations.Rows
            For Each oViolation As clsBaseViolation In oDrv.Violations
                bIgnoreViol = False

                Dim bBypassUDR As Boolean = False
                bBypassUDR = CommonRulesFunctions.StateInfoContains("IGNOREVIOL", "UDR", "UDR", "2" & sStateCode, sAppliesToCode, sProgram)

                If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And oDrv.Age <= 18 And oViolation.ViolGroup.ToUpper = "UDR" Then
                    'ignore viol
                    bIgnoreViol = True
                End If
                If (sProgram.ToUpper = "SUMMIT" Or bBypassUDR) And CommonRulesFunctions.HasForeignLicense(oDrv) And oViolation.ViolGroup.ToUpper = "UDR" Then
                    'ignore viol
                    bIgnoreViol = True
                End If
                If Not bIgnoreViol Then
                    If drViolation("ViolationIndex") = oViolation.IndexNum Then
                        oViolation.Points = drViolation("Points")
                        oViolation.Chargeable = drViolation("Chargeable")
                        oDrv.Points += oViolation.Points
                        Exit For
                    End If
                Else
                    If drViolation("ViolationIndex") = oViolation.IndexNum Then
                        'oViolation.Points = drViolation("Points")
                        oViolation.Chargeable = drViolation("Chargeable")
                        'oDrv.Points += oViolation.Points
                        Exit For
                    End If
                End If
            Next
        Next

    End Sub

    Public Function GetCoverage(ByVal sCovGroup As String, ByVal oPolicy As clsPolicyPPA, ByVal oVeh As clsVehicleUnit) As clsPACoverage

        'Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
        Dim oReturnedCov As clsPACoverage = Nothing

        For Each oCov As clsPACoverage In oVeh.Coverages
            If oCov.CovGroup.ToUpper = sCovGroup.ToUpper Then
                oReturnedCov = oCov
                Exit For
            End If
        Next

        Return oReturnedCov
    End Function

    Protected Function HasOPF(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bHasOPF As Boolean = False
        Try
            For Each oLienHolder As clsEntityLienHolder In oPolicy.LienHolders
                If oLienHolder.EntityType.ToUpper = "PFC" Then
                    bHasOPF = True
                    Exit For
                End If
            Next
            Return bHasOPF
        Catch ex As Exception
        Finally
        End Try

    End Function

    Protected Function HasBusinessUse(ByVal oVehicle As clsVehicleUnit) As Boolean

        Dim bHasBusinessUse As Boolean = False

        Try
            For Each oFactor As clsVehicleFactor In oVehicle.Factors
                If oFactor.FactorCode.ToUpper = "BUS_USE" Then
                    bHasBusinessUse = True
                    Exit For
                End If
            Next

            Return bHasBusinessUse

        Catch ex As Exception
        Finally

        End Try

    End Function

    Public Overridable Sub CheckNonOwner(ByVal oPolicy As clsPolicyPPA)
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
                        If oDrv.DriverStatus.ToUpper = "ACTIVE" Or oDrv.DriverStatus.ToUpper = "PERMITTED" Then
                            If oDrv.RelationToInsured.ToUpper = "SPOUSE" Then
                                If Not SpouseAllowed(oPolicy) Then
                                    oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Only one driver is allowed on a Non-owner policy", "NonOwner", "IER", oPolicy.Notes.Count))
                                    Exit For
                                End If
                            Else
                                oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Only the Policyholder and Spouse may be listed on a Non-owner policy", "NonOwner", "IER", oPolicy.Notes.Count))
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next

        End If
    End Sub

    Public Sub CheckNamedInsuredAge(ByVal oPolicy As clsPolicyPPA)
        Dim iNamedInsuredCount As Integer = 0
        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If Not oDrv.IsMarkedForDelete And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                If oDrv.RelationToInsured.ToUpper = "SELF" Then
                    iNamedInsuredCount = iNamedInsuredCount + 1
                    If oDrv.Age < 18 Then
                        If oPolicy.Program.ToUpper = "DIRECT" And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: The Policyholder must be at least 18 years of age.", "UnderAgeNamedInsured", "IER", oPolicy.Notes.Count))
                        Else
                            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Named insured must be at least 18 years of age.", "UnderAgeNamedInsured", "IER", oPolicy.Notes.Count))
                        End If
                    End If
                End If
            End If
        Next

        If iNamedInsuredCount > 1 Then
            oPolicy.Notes = (AddNote(oPolicy.Notes, "Ineligible Risk: Cannot have more than 1 driver with relationship of SELF.", "SelfCount", "IER", oPolicy.Notes.Count))
        End If

    End Sub

    Public Function SpouseAllowed(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bSpouseAllowed As Boolean = True
        Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        Dim DataRows() As DataRow
        DataRows = oStateInfoDataSet.Tables(0).Select("ItemGroup = 'NONOWNER' AND ItemCode = 'ALLOWSPOUSE'")

        For Each oRow As DataRow In DataRows
            If oRow.Item("ItemValue").ToString.ToUpper = "TRUE" Then
                bSpouseAllowed = True
                Exit For
            End If

            If oRow.Item("ItemValue").ToString.ToUpper = "FALSE" Then
                bSpouseAllowed = False
                Exit For
            End If
        Next

        Return bSpouseAllowed

    End Function

    Public Overridable Sub CheckEffectiveDate(ByRef oPolicy As clsPolicyPPA)
        With oPolicy
            If .PolicyID = "" Then
                If .EffDate < Today Then
                    If Not CBool(ConfigurationManager.AppSettings("IsTest")) Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have an Effective Date in the past", "PastEffDate", "IER", .Notes.Count))
                    End If
                ElseIf .EffDate > DateAdd(DateInterval.Day, 30, Today) Then
                    If Not CBool(ConfigurationManager.AppSettings("IsTest")) Then
                        .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have an Effective Date more than 30 days in the future", "FutureEffDate", "IER", .Notes.Count))
                    End If
                End If
            Else
                If .TransactionEffDate > .ExpDate Then
                    .Notes = (AddNote(.Notes, "Ineligible Risk: Cannot have an Effective Date past the policy expiration date", "FutureEffDateEnd", "IER", .Notes.Count))
                End If
            End If
        End With
    End Sub

    Public Overridable Sub CheckCoverages(ByRef oPolicy As clsPolicyPPA)
        Dim sInvalidCoverages As String = String.Empty
        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVeh.IsMarkedForDelete Then
                sInvalidCoverages &= ValidateCoverages(oVeh, oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.Program, oPolicy.AppliesToCode)
            End If
        Next

        With oPolicy
            If sInvalidCoverages <> String.Empty Then
                .Notes = (AddNote(.Notes, "Ineligible Risk: " & sInvalidCoverages, "InvalidCoverages", "IER", .Notes.Count))
            End If
        End With
    End Sub

    Public Overridable Function CheckCoverages(ByRef oVeh As clsVehicleUnit, ByVal sProgram As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sRateDate As String, ByVal sStateCode As String) As String
        Dim sInvalidCoverages As String = String.Empty

        sInvalidCoverages &= ValidateCoverages(oVeh, "2", sStateCode, sRateDate, sProgram)

        If sInvalidCoverages <> String.Empty Then
            oNoteList = (AddNote(oNoteList, "Ineligible Risk: " & sInvalidCoverages, "InvalidCoverages", "IER", oNoteList.Count, "AOLE"))
        End If

        Return ""

    End Function

    Public Overridable Function CheckCoverages(ByVal sCovCodes As String, ByVal sCovGroups As String, ByVal sProgram As String, ByRef oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sRateDate As String, ByVal sStateCode As String) As String
        Dim sInvalidCoverages As String = String.Empty

        RemoveNotes(oNoteList, "IER")

        sInvalidCoverages &= ValidateCoverages(sCovCodes, sCovGroups, "2", sStateCode, sRateDate, sProgram)

        If sInvalidCoverages <> String.Empty Then
            oNoteList = (AddNote(oNoteList, "Ineligible Risk: " & sInvalidCoverages, "InvalidCoverages", "IER", oNoteList.Count, "AOLE"))
        End If

        Return ""

    End Function

    Public Function VehicleApplies(ByVal oVeh As clsVehicleUnit, ByVal oPolicy As clsPolicyPPA) As Boolean
        If oPolicy.CallingSystem.ToUpper.Contains("OLE") Or oPolicy.CallingSystem.ToUpper.Contains("UWC") Then
            If oVeh.IsNew Then
                Return True
            Else
                If oVeh.IsModified Then
                    Return True
                Else
                    Return False
                End If
            End If
        Else
            Return True
        End If
    End Function

    Public Function DriverApplies(ByVal oDrv As clsEntityDriver, ByVal oPolicy As clsPolicyPPA) As Boolean
        If oPolicy.CallingSystem.ToUpper.Contains("OLE") Or oPolicy.CallingSystem.ToUpper.Contains("UWC") Then
            If oDrv.IsNew Then
                Return True
            Else
                If oDrv.IsModified Then
                    Return True
                Else
                    Return False
                End If
            End If
        Else
            Return True
        End If
    End Function

    Public Overridable Sub CalculateVehicleAge(ByVal oPolicy As clsPolicyPPA, ByVal bShowTrueAge As Boolean)

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

    Protected Overridable Function GetOriginalTermUWTierValues(ByVal policyID As String, ByVal origTermEffDate As DateTime, ByVal productCode As String, ByVal stateCode As String, ByVal programCode As String) As OriginalTermUWTierValues
        'Gets DaysLapse, PriorLiabilityLimitsCode, Months Prior Continuous Coverage  values for a Original Term Effective Date.
        'Need these original term values to retain the same tier for the life of the policy.

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Dim origTermUWTierValues As OriginalTermUWTierValues = Nothing
        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " Select p.PolicyNo, p.TermEffDate, p.PolicyTransactionNum, ISNULL(p.DaysLapse, 0) as DaysLapse, ISNULL(p.PriorLiabilityLimitsCode, 0) as PriorLiabilityLimitsCode, pn.NoteText "
                sSql &= " FROM PASCArrier..Policy p with (NOLOCK) JOIN PasCarrier..PolicyNote pn with (NOLOCK) "
                sSql &= " ON p.PolicyNo = pn.PolicyNo and p.TermEffDate = pn.TermEffDate "
                sSql &= " WHERE p.PolicyNo = @PolicyNo and p.ProgramCode = @ProgramCode and p.TermEffDate = @OrigEffDate "
                sSql &= " and p.PolicyTransactionNum = (Select Max(PolicyTransactionNum) from PASCArrier..Policy c where c.PolicyNo = @PolicyNo and TermEffDate = @OrigEffDate) "
                sSql &= " and pn.NoteTypeCode = 'PAI' and pn.NoteDesc = 'MonthsPriorContCov' "
                sSql &= " ORDER BY p.PolicyTransactionNum Desc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@PolicyNo", SqlDbType.VarChar).Value = policyID
                cmd.Parameters.Add("@OrigEffDate", SqlDbType.DateTime).Value = origTermEffDate
                cmd.Parameters.Add("@ProgramCode", SqlDbType.VarChar).Value = programCode
                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    origTermUWTierValues = New OriginalTermUWTierValues()

                    origTermUWTierValues.DaysLapse = CInt(oReader.Item("DaysLapse"))
                    origTermUWTierValues.PriorLimitsCode = MapPriorLimitsCode(oReader.Item("PriorLiabilityLimitsCode"), productCode, stateCode)
                    origTermUWTierValues.MonthsPriorContCov = ProcessMonthsPriorContCovNote(oReader.Item("NoteText"))

                    Exit Do
                Loop
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

        Return origTermUWTierValues

    End Function

    Private Function ProcessMonthsPriorContCovNote(ByVal noteText As String) As Integer
        Dim months As Integer = 0

        If Not String.IsNullOrEmpty(noteText) Then
            If noteText.Contains(":") Then
                months = CInt(noteText.Split(":")(1))
            End If
        End If

        Return months

    End Function

    Private Function MapPriorLimitsCode(ByVal pasPriorLimits As String, ByVal productCode As String, ByVal stateCode As String) As String
        Dim priorLimits As String = "0"

        If Not String.IsNullOrEmpty(pasPriorLimits) AndAlso CInt(pasPriorLimits) > 0 Then

            Dim sSql As String = ""
            Dim oReader As SqlDataReader = Nothing
            Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            oConn.Open()

            Try

                Using cmd As New SqlCommand(sSql, oConn)

                    sSql = " SELECT Code, MappingCode1, MappingCode2, MappingCode3 "
                    sSql &= " FROM pgm" & productCode & stateCode & "..CodeXRef with (NOLOCK) "
                    sSql &= " WHERE Source in ('PASRATE', 'PAS', 'PASLOAD') and CodeType = 'LIMITS' And Code = @PASPriorLimits "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@PASPriorLimits", SqlDbType.VarChar).Value = pasPriorLimits
                    oReader = cmd.ExecuteReader

                    Do While oReader.Read()

                        priorLimits = oReader.Item("MappingCode1")

                        Exit Do
                    Loop
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

        End If

        Return priorLimits

    End Function

#End Region

End Class
