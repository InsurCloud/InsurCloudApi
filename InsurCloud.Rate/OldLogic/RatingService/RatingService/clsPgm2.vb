Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Data.SqlClient
Imports System.Activator
Imports CorPolicy.clsCommonFunctions
Imports CorPolicy
Imports System.Collections.Generic
Imports log4net
Imports log4net.Config

Public Class clsPgm2
    Inherits clsPgm
    Private ReadOnly log4net As ILog

    Protected msRatedVehNum As String = String.Empty
    Protected moCappedFactorsTable As DataTable
    Protected msCappedFactors As String()
    Protected mbHasOPF As Boolean
    Protected msFirstVehNum As String = String.Empty

    Public Property RatedVehNum() As String
        Get
            Return msRatedVehNum
        End Get
        Set(ByVal value As String)
            msRatedVehNum = value

            If msFirstVehNum = String.Empty Then
                msFirstVehNum = msRatedVehNum
            End If
        End Set
    End Property

    Public Function IsFirstRatedVeh() As Boolean
        Return msRatedVehNum = msFirstVehNum
    End Function

    Public Sub New()
        log4net = LogManager.GetLogger(GetType(RatingService))
    End Sub

    Public Overloads Sub FinishLogging(ByVal bLogRate As Boolean)
        MyBase.FinishLogging(bLogRate)
        If bLogRate Then
            ErrorLogging("", "")
        End If
    End Sub

    Public Overloads Sub ErrorLogging(ByVal sMethodName As String, ByVal sMessage As String)
        moLogging.EndTimeStamp = Now()
        Dim oLogSvc As New ImperialFire.Logging
        oLogSvc.WriteAutoLog(moLogging, sMethodName, sMessage)
    End Sub

    Public Overridable Sub UpdateLog(ByVal oPolicy As CorPolicy.clsPolicyPPA, ByVal oFactorTable As System.Data.DataTable)
        CType(moLogging, ImperialFire.clsLogging2).Policy = oPolicy
        moLogging.DataTable.Add(oFactorTable)
    End Sub

    Public Overloads Function Rate(ByVal oPolicy As clsPolicyPPA, ByVal bLogRate As Boolean) As Boolean

        Dim oFactorTable As DataTable = Nothing
        Dim oFeeTable As DataTable = Nothing
        Dim drTotalsRow As DataRow = Nothing
        Dim dFullTermPrem As Decimal = 0
        Dim dPolicyFullTermPremium As Decimal = 0

        Try

            '**********************************************************************
            '*THE FACTORTABLE THAT IS LOGGED IS GOING TO BE THE LAST VEHICLE RATED*
            '**********************************************************************

            Call BeginLogging(moLogging, oPolicy, oFactorTable)

            Call InitializeConnection()

            moStateInfoDataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

            oPolicy.ExpDate = DateAdd(DateInterval.Month, oPolicy.Term, oPolicy.EffDate)

            dbGetTerritory(oPolicy)

            'clear premium factors
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                Dim i As Integer = 1
                For Each oCoverage As clsPACoverage In oVeh.Coverages

                    ' Temporary fix for webrater
                    If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                        oCoverage.IndexNum = i
                        i += 1
                    End If
                    oCoverage.Factors.Clear()
                Next
            Next

            For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
                'set this guy so we will have access to him without having to pass him around
                If Not oVehicle.IsMarkedForDelete Then
                    RatedVehNum = oVehicle.IndexNum  'sRatedVehNum

                    RentToOwnCheck(oPolicy)

                    oFactorTable = CreateDataTable("Factors", oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, oPolicy.Product, oPolicy.StateCode)

                    moCappedFactorsTable = CreateDataTable("CappedFactors", oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, oPolicy.Product, oPolicy.StateCode)

                    Dim drFactorRow As DataRow = Nothing
                    drFactorRow = moCappedFactorsTable.NewRow
                    drFactorRow.Item("FactorName") = "MaxDiscountAmt"
                    For i As Integer = 1 To moCappedFactorsTable.Columns.Count - 1
                        drFactorRow.Item(moCappedFactorsTable.Columns.Item(i).ColumnName) = MaxDiscountAmount(oPolicy, moCappedFactorsTable.Columns.Item(i).ColumnName)
                    Next
                    moCappedFactorsTable.Rows.Add(drFactorRow)
                    If Not drFactorRow Is Nothing Then
                        drFactorRow = Nothing
                    End If
                    moCappedFactorsTable.Rows.Add(CreateTotalsRow(moCappedFactorsTable))

                    Call GetCappedFactors(oPolicy)

                    Call GetFactors(oPolicy, oFactorTable)

                    oFactorTable.Rows.Add(CreateTotalsRow(oFactorTable))

                    Call Calculate(oPolicy, oFactorTable)

                    oFactorTable.AcceptChanges()

                    UpdateLog(oPolicy, oFactorTable)

                    Call GetTotalChgInPremPolFactors(oPolicy)

                    Call UpdateTotals(oPolicy, oFactorTable)

                    drTotalsRow = GetRow(oFactorTable, "Totals")

                    GetPremiums(oPolicy, drTotalsRow)

                    'populate the unit's full term premium and the policy's full term premium
                    'For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                    dFullTermPrem = 0
                    Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
                    For Each oCov As clsPACoverage In oVeh.Coverages
                        If Not oCov.IsMarkedForDelete Then
                            dFullTermPrem = dFullTermPrem + oCov.FullTermPremium
                        End If
                    Next
                    oVeh.FullTermPremium = dFullTermPrem
                    dPolicyFullTermPremium += oVeh.FullTermPremium
                End If
            Next

            oPolicy.FullTermPremium = dPolicyFullTermPremium

            'Load Fees
            Call LoadFees(oPolicy)

            oFeeTable = CreateFeesTable()

            dbGetFeeFactor(oPolicy, oFeeTable)

            If oPolicy.FullTermPremium > 0 And (oPolicy.Status = "1" Or oPolicy.Status = "2" Or oPolicy.Status = "3" Or (oPolicy.TransactionNum <= 1 And oPolicy.Status = "4" And oPolicy.Type.ToUpper <> "RENEWAL")) Then
                SetDownPayAmount(oPolicy)
            End If

            Call CleanDataTable(oPolicy, oFactorTable)

            'remove default or combined average drivers
            For x As Integer = oPolicy.Drivers.Count - 1 To 0 Step -1
                If oPolicy.Drivers.Item(x).IndexNum = 99 Or oPolicy.Drivers.Item(x).IndexNum = 98 Then
                    oPolicy.Drivers.Remove(oPolicy.Drivers.Item(x))
                End If
            Next

            ' don't log the rate if we already have an entry with this premium amount
            If bLogRate Then
                ' moLogging.WriteLogXML = DetermineIfLoggingNeeded(oPolicy)
                bLogRate = DetermineIfLoggingNeeded(oPolicy)
            End If

            Dim bLogEverything As Boolean = False
            Try
                bLogEverything = CBool(ConfigurationManager.AppSettings.Item("LogEverything"))
            Catch ex As Exception
            End Try

            If bLogEverything Then
                bLogRate = True
            End If

            Call FinishLogging(bLogRate)

        Catch ex As Exception
            Dim errCtx As ExceptionContext = New ExceptionContext(ex)
            errCtx.AddContext("Policy", oPolicy)
            If oPolicy IsNot Nothing And oPolicy.PolicyID <> "" Then
                errCtx.ReferenceID = oPolicy.PolicyID
                errCtx.ReferenceType = "PolicyID"
            ElseIf oPolicy IsNot Nothing And oPolicy.QuoteID <> "" Then
                errCtx.ReferenceID = oPolicy.QuoteID
                errCtx.ReferenceType = "QuoteID"
            End If
            errCtx.SourceSystem = "RatingService"
            errCtx.SystemTS = Date.Now
            errCtx.LogError()
            Throw
        Finally
            If Not drTotalsRow Is Nothing Then
                drTotalsRow = Nothing
            End If
            If Not oFactorTable Is Nothing Then
                oFactorTable.Dispose()
                oFactorTable = Nothing
            End If
            If Not oFeeTable Is Nothing Then
                oFeeTable.Dispose()
                oFeeTable = Nothing
            End If
            If moConn IsNot Nothing Then
                moConn.Close()
                moConn.Dispose()
            End If
        End Try

    End Function


    Private Function DetermineIfLoggingNeeded(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim bLogRate As Boolean = True
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing

        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT LogItemID FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "EventLog with(nolock)"
                sSql = sSql & " WHERE (QuoteID = @QuoteID OR PolicyID = @PolicyID)"
                sSql = sSql & " AND Premium = @Premium "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@QuoteID", SqlDbType.VarChar, 20).Value = oPolicy.QuoteID
                cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar, 20).Value = oPolicy.PolicyID
                cmd.Parameters.Add("@Premium", SqlDbType.Decimal).Value = oPolicy.FullTermPremium

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    bLogRate = False
                    Exit Do
                Loop

                oReader.Close()
                oReader = Nothing
            End Using
        Catch Ex As Exception
            Dim errCtx As ExceptionContext = New ExceptionContext(Ex)
            errCtx.AddContext("Policy", oPolicy)
            If oPolicy IsNot Nothing And oPolicy.PolicyID <> "" Then
                errCtx.ReferenceID = oPolicy.PolicyID
                errCtx.ReferenceType = "PolicyID"
            ElseIf oPolicy IsNot Nothing And oPolicy.QuoteID <> "" Then
                errCtx.ReferenceID = oPolicy.QuoteID
                errCtx.ReferenceType = "QuoteID"
            End If
            errCtx.SourceSystem = "RatingService"
            errCtx.SystemTS = Date.Now
            errCtx.LogError()
        Finally
        End Try


        If oPolicy.CallingSystem.ToUpper <> "WEBRATER" And oPolicy.CallingSystem.ToUpper <> "BRIDGE" Then
            bLogRate = True
        End If


        Dim bLogEverything As Boolean = False
        Try
            bLogEverything = CBool(ConfigurationManager.AppSettings.Item("LogEverything"))
        Catch ex As Exception
        End Try

        If bLogEverything Then
            Return True
        End If

        Return bLogRate
    End Function

	Public Overridable Sub UpdateTotals(ByVal oPolicy As clsPolicyPPA, ByVal oFactorTable As DataTable)

		Dim drTotalsRow As DataRow = Nothing

		Try
			drTotalsRow = GetRow(oFactorTable, "Totals")
			If Not drTotalsRow Is Nothing Then
				For Each oDataCol As DataColumn In oFactorTable.Columns
					If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
						If Not VehContainsCov(oPolicy, oDataCol.ColumnName.ToString) Then 'veh does not contain coverage then set the amount for this coverage to 0
							drTotalsRow(oDataCol.ColumnName.ToString) = 0
						End If
					End If
				Next
			End If
		Catch ex As Exception
			Throw New Exception(ex.Message & ex.StackTrace, ex)
		Finally
			If Not drTotalsRow Is Nothing Then
				drTotalsRow = Nothing
			End If
		End Try


	End Sub

	Public Overridable Function VehContainsCov(ByVal oPolicy As clsPolicyPPA, ByVal sCov As String) As Boolean

		Dim bVehContainsCov As Boolean = False
		Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

		For Each oCov As clsBaseCoverage In oVeh.Coverages
			If Not oCov.IsMarkedForDelete Then
				If oCov.CovGroup.ToUpper = sCov.ToUpper Then
					bVehContainsCov = True
					Exit For
				End If
			End If
		Next

		Return bVehContainsCov

	End Function

	Public Overridable Sub LoadFees(ByVal oPolicy As clsPolicyPPA)

		Dim sSql As String = ""
		Dim oReader As SqlDataReader = Nothing
		Dim oFee As clsBaseFee = Nothing
		Dim oEndorse As clsEndorsementFactor = Nothing

		Try
			'clear existing fees
			oPolicy.Fees.Clear()

			Using cmd As New SqlCommand(sSql, moConn)

				sSql = " SELECT FeeCode, Description, FeeApplicationType, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorFee with(nolock)"
				sSql = sSql & " WHERE Program = @Program "
				sSql = sSql & " AND EffDate <= @RateDate "
				sSql = sSql & " AND ExpDate > @RateDate "
				sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
				sSql = sSql & " ORDER BY FeeCode Asc "

				'Execute the query
				cmd.CommandText = sSql

				cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
				cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
				cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

				oReader = cmd.ExecuteReader

				Do While oReader.Read()
					'if FeeCode is POLICY then add it, we will always have the POLICY fee on the Policy
					Select Case oReader.Item("FeeCode").ToString.ToUpper
						Case "POLICY"
							oFee = New clsBaseFee
							oFee.FeeCode = oReader.Item("FeeCode").ToString
							oFee.FeeDesc = oReader.Item("Description").ToString
							oFee.FeeName = oReader.Item("Description").ToString
							oFee.FeeType = "P"
							oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
							oFee.FeeNum = oPolicy.Fees.Count + 1
							oFee.IndexNum = oPolicy.Fees.Count + 1
						Case "MVR"
							Dim bMVRFeeAdded As Boolean = False
							Dim oMVRNote As clsBaseNote = GetNote(oPolicy, "Rater MVR Order")

							If oMVRNote Is Nothing Then
								'If not a bridge quote that already ordered reports
								For Each oDrv As clsEntityDriver In oPolicy.Drivers
									If oDrv.MVROrderStatus.ToUpper.Trim = "" Then
										If oDrv.DriverStatus.ToUpper.Trim = "PERMITTED" Or oDrv.DriverStatus.ToUpper.Trim = "ACTIVE" Then
											oFee = New clsBaseFee
											oFee.FeeCode = oReader.Item("FeeCode").ToString
											oFee.FeeDesc = oReader.Item("Description").ToString & " for Driver #" & oDrv.IndexNum
											oFee.FeeName = oReader.Item("Description").ToString
											oFee.FeeType = "P"
											oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
											oFee.FeeNum = oPolicy.Fees.Count + 1
											oFee.IndexNum = oPolicy.Fees.Count + 1
											bMVRFeeAdded = True
											oPolicy.Fees.Add(oFee)
											oFee = Nothing
										End If
									ElseIf oDrv.MVROrderStatus.ToUpper.Trim <> "NEO" And oDrv.MVROrderStatus.ToUpper.Trim <> "ERROR" Then
										If oDrv.DriverStatus.ToUpper.Trim = "PERMITTED" Or oDrv.DriverStatus.ToUpper.Trim = "ACTIVE" Then
											oFee = New clsBaseFee
											oFee.FeeCode = oReader.Item("FeeCode").ToString
											oFee.FeeDesc = oReader.Item("Description").ToString & " for Driver #" & oDrv.IndexNum
											oFee.FeeName = oReader.Item("Description").ToString
											oFee.FeeType = "P"
											oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
											oFee.FeeNum = oPolicy.Fees.Count + 1
											oFee.IndexNum = oPolicy.Fees.Count + 1
											bMVRFeeAdded = True
											oPolicy.Fees.Add(oFee)
											oFee = Nothing
										End If
									End If
								Next
							End If
						Case "SR22"
							Dim bHasSR22 As Boolean = False
							For Each oDrv As clsEntityDriver In oPolicy.Drivers
                                If oDrv.IsMarkedForDelete = False And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                                    If oDrv.SR22 Then
                                        bHasSR22 = True
                                        Exit For
                                    End If
                                End If
							Next
							If bHasSR22 Then
								oFee = New clsBaseFee
								oFee.FeeCode = oReader.Item("FeeCode").ToString
								oFee.FeeDesc = oReader.Item("Description").ToString
								oFee.FeeName = oReader.Item("Description").ToString
								oFee.FeeType = "P"
								oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
								oFee.FeeNum = oPolicy.Fees.Count + 1
								oFee.IndexNum = oPolicy.Fees.Count + 1
                            End If
                        Case "THEFT"
                            oFee = New clsBaseFee
                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                            oFee.FeeDesc = oReader.Item("Description").ToString
                            oFee.FeeName = oReader.Item("Description").ToString
                            oFee.FeeType = "P"
                            oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
                            oFee.FeeNum = oPolicy.Fees.Count + 1
                            oFee.IndexNum = oPolicy.Fees.Count + 1
                        Case Else

                    End Select
					If Not oFee Is Nothing Then
						oPolicy.Fees.Add(oFee)
						oFee = Nothing
					End If
				Loop
                oReader.Close()
            End Using

            'oPolicy = LoadMVRFees(oPolicy)

        Catch ex As Exception
            Throw
        Finally

        End Try

    End Sub

    Public Function LoadMVRFees(ByVal policy As clsPolicyPPA) As clsPolicyPPA
        Dim sql As New StringBuilder
        Dim totalFeeAmt As Decimal = 0

        sql.Append(" SELECT * ")
        sql.Append("   FROM Common..Payment ")
        sql.Append("  WHERE PolicyNbr = @QuoteID ")
        sql.Append("    AND PaymentMethod = 'AEFTMVR' ")

        Try
            If Not String.IsNullOrWhiteSpace(policy.QuoteID) Then
                Using cmd As New SqlCommand(sql.ToString, moConn)
                    cmd.Parameters.Add("@QuoteID", SqlDbType.VarChar).Value = policy.QuoteID

                    Dim reader As SqlDataReader = cmd.ExecuteReader
                    While reader.Read
                        totalFeeAmt += reader.Item("PaymentAmt").ToString
                    End While
                    reader.Close()
                End Using

                If totalFeeAmt > 0 Then
                    Dim fee As New clsBaseFee
                    fee.FeeCode = "MVR"
                    fee.FeeDesc = "MVR Fee"
                    fee.FeeName = "MVR Fee"
                    fee.FeeType = "D"
                    fee.FeeApplicationType = "EARNED"
                    fee.FeeAmt = totalFeeAmt
                    fee.FeeNum = policy.Fees.Count + 1
                    fee.IndexNum = policy.Fees.Count + 1
                    policy.Fees.Add(fee)
                End If
            End If

            Return policy

        Catch ex As Exception
            Throw
        End Try

    End Function

    Public Function LoadMVRFeesToBilling(ByVal policy As clsPolicyPPA) As clsPolicyPPA
        Dim sql As New StringBuilder
        Dim totalFeeAmt As Decimal = 0

        sql.Append(" SELECT * ")
        sql.Append("   FROM Common..Payment ")
        sql.Append("  WHERE PolicyNbr = @QuoteID ")
        sql.Append("    AND PaymentMethod = 'AEFTMVR' ")


        Try
            Using cmd As New SqlCommand(sql.ToString, moConn)
                cmd.Parameters.Add("@QuoteID", SqlDbType.VarChar).Value = policy.QuoteID

                Dim reader As SqlDataReader = cmd.ExecuteReader
                While reader.Read
                    totalFeeAmt += reader.Item("PaymentAmt").ToString
                End While
                reader.Close()
            End Using

            If totalFeeAmt > 0 Then
                Dim fee As New clsBaseFee
                fee.FeeCode = "MVR"
                fee.FeeDesc = "MVR Fee"
                fee.FeeName = "MVR Fee"
                fee.FeeType = "D"
                fee.FeeApplicationType = "EARNED"
                fee.FeeAmt = totalFeeAmt
                fee.FeeNum = policy.Billing.Fees.Count + 1
                fee.IndexNum = policy.Billing.Fees.Count + 1
                policy.Billing.Fees.Add(fee)
            End If

            Return policy

        Catch ex As Exception
            Throw
        End Try

    End Function

    Public Overridable Sub GetFactors(ByVal oPolicy As clsPolicyPPA, ByVal oFactorTable As DataTable)

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            If oVeh.AssignedDriverNum = 99 Then
                'average driver, don't need to look up driver factors
                'add combined driver factors to data table
                Dim drFactorRow As DataRow = Nothing
                drFactorRow = oFactorTable.NewRow
                drFactorRow.Item("FactorName") = "CombinedDriver"
                Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
                For Each oFactor As clsBaseFactor In oDriver.Factors
                    For i As Integer = 1 To oFactorTable.Columns.Count - 1
                        If oFactor.CovType.ToUpper = oFactorTable.Columns.Item(i).ColumnName.ToUpper Then
                            'add it to the data row
                            drFactorRow.Item(oFactorTable.Columns.Item(i).ColumnName) = oFactor.FactorAmt
                            Exit For
                        End If
                    Next
                    drFactorRow.Item("FactorType") = oFactor.FactorType
                Next

                If Not drFactorRow Is Nothing Then
                    oFactorTable.Rows.Add(drFactorRow)
                End If
            End If

            dbGetDriverAdjustmentFactor(oPolicy, oFactorTable)
            dbGetDriverFactor(oPolicy, oFactorTable)
            dbGetDriverAgePointsFactor(oPolicy, oFactorTable)
            dbGetDriverClassFactor(oPolicy, oFactorTable)
            dbGetDriverPointsFactor(oPolicy, oFactorTable)
            dbGetMarketPointsFactor(oPolicy, oFactorTable)

            dbGetBaseRateFactor(oPolicy, oFactorTable)
            dbGetHouseholdStructureFactor(oPolicy, oFactorTable)
            dbGetModelYearFactor(oPolicy, oFactorTable)
            dbGetPolicyFactor(oPolicy, oFactorTable)
            dbGetPolicyDiscountMatrixFactor(oPolicy, oFactorTable)
            dbGetCoverageFactor(oPolicy, oFactorTable, "MidMult")
            dbGetCoverageFactor(oPolicy, oFactorTable, "MidAdd")
            If oVeh.StatedAmt > 0 _
                And (oVeh.VehicleSymbolCode = "999" Or oVeh.VehicleSymbolCode = "998" _
                     Or oVeh.VehicleSymbolCode = "997" Or oVeh.VehicleSymbolCode = "966" _
                     Or oVeh.VehicleSymbolCode = "65" Or oVeh.VehicleSymbolCode = "66" _
                     Or oVeh.VehicleSymbolCode = "67" Or oVeh.VehicleSymbolCode = "68") Then
                dbGetStatedValueFactor(oPolicy, oFactorTable)
            End If
            dbGetSymbolFactor(oPolicy, oFactorTable)

            dbGetTerritoryFactor_Summit(oPolicy, oFactorTable)
            dbGetMarketAdjustmentFactor(oPolicy, oFactorTable)

            dbGetTierMatrixFactor(oPolicy, oFactorTable)
            dbGetVehicleFactor(oPolicy, oFactorTable)
            dbGetDiscountFactor(oPolicy, oFactorTable)
            dbGetRatedFactor(oPolicy, oFactorTable)


        Catch ex As Exception
            Throw
        Finally

        End Try
    End Sub

    Public Overridable Function dbGetRatedFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try
            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                drFactorRow = FactorTable.NewRow
                drFactorRow.Item("FactorName") = oFactor.FactorCode

                For i As Integer = 1 To FactorTable.Columns.Count - 2
                    'add it to the data row
                    drFactorRow.Item(FactorTable.Columns.Item(i).ColumnName) = oFactor.RatedFactor
                Next
                If oFactor.FactorType <> String.Empty Then
                    drFactorRow.Item("FactorType") = oFactor.FactorType.Trim
                Else
                    drFactorRow.Item("FactorType") = "MidMult"
                End If

                If Not drFactorRow Is Nothing Then
                    Dim dRatedFactor As Decimal
                    Decimal.TryParse(oFactor.RatedFactor, dRatedFactor)
                    If dRatedFactor > 0 Then
                        FactorTable.Rows.Add(drFactorRow)
                    End If
                End If
            Next

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try
    End Function

    Public Overridable Function dbGetDiscountFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim drDiscountNoMaxRow As DataRow = Nothing
        Dim drDiscountMaxRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing
        Dim sMaxDiscountFactors As New List(Of String)
        Dim drSurchargeRow As DataRow = Nothing

        Try

            ' MAXDiscount
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT ItemSubCode,ItemValue FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "StateInfo with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND ItemGroup = 'MAXDISCOUNT' "
                sSql = sSql & " AND ItemCode = 'FACTOR' "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader
                Do While oReader.Read()
                    sMaxDiscountFactors.Add(oReader("ItemValue"))
                Loop

                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If
            End Using


            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Distinct Coverage, FactorCode,Factor FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDiscount with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader
                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Discount"

                    drDiscountNoMaxRow = FactorTable.NewRow
                    drDiscountNoMaxRow.Item("FactorName") = "DiscountNoMax"

                    drDiscountMaxRow = FactorTable.NewRow
                    drDiscountMaxRow.Item("FactorName") = "DiscountMax"

                    drSurchargeRow = FactorTable.NewRow
                    drSurchargeRow.Item("FactorName") = "Surcharge"


                    Dim bAddSR22 As Boolean = False
                    For Each oDrv As clsEntityDriver In oPolicy.Drivers
                        If Not oDrv.IsMarkedForDelete Then
                            For Each oFactor As clsBaseFactor In oDrv.Factors
                                If oFactor.FactorCode = "SR22" Then
                                    bAddSR22 = True
                                    Exit For
                                End If
                            Next
                        End If
                    Next


                    ' If this is a combined driver state and one of the drivers has SR22, then add it to the factors table
                    Dim bIsCombinedDriver As Boolean = False
                    For Each oFactorRow As DataRow In FactorTable.Rows
                        If oFactorRow("FactorName").ToString.Trim.ToUpper = "COMBINEDDRIVER" Then
                            bIsCombinedDriver = True
                        End If
                    Next

                    If bIsCombinedDriver And bAddSR22 Then
                        Dim drSR22Row As DataRow = Nothing
                        drSR22Row = FactorTable.NewRow
                        drSR22Row.Item("FactorName") = "SR22"

                        FactorTable.Rows.Add(drSR22Row)
                    End If

                End If

                Do While oReader.Read()
                    If IsDBNull(drFactorRow(oReader("Coverage"))) Then
                        drFactorRow(oReader("Coverage")) = CDec(1.0)
                    End If

                    If IsDBNull(drDiscountMaxRow(oReader("Coverage"))) Then
                        drDiscountMaxRow(oReader("Coverage")) = CDec(1.0)
                    End If


                    If IsDBNull(drDiscountNoMaxRow(oReader("Coverage"))) Then
                        drDiscountNoMaxRow(oReader("Coverage")) = CDec(1.0)
                    End If

                    If IsDBNull(drSurchargeRow(oReader("Coverage"))) Then
                        drSurchargeRow(oReader("Coverage")) = CDec(1.0)
                    End If


                    For Each oFactorRow As DataRow In FactorTable.Rows
                        If oFactorRow("FactorName").ToString.Trim.ToUpper = oReader("FactorCode").ToString.Trim.ToUpper Then
                            For i As Integer = 1 To FactorTable.Columns.Count - 1
                                If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                                    '*************************************
                                    'If we have .85 in the Discount factor already (then we're applying a 15% discount)
                                    'If we want to add another 5% discount, the factor row for the new discount has .95
                                    'So, the new factor on the Discount row needs to be .80
                                    ' What we do is (1 - ((1 - .85) + (1 - .95)))
                                    ' Step 1 is... (1 - (.15 + .05))
                                    ' Step 2 is... (1 - .20)
                                    ' Giving us... .80 to put in the Discount record for rating
                                    '*************************************
                                    If CDec(oReader("Factor")) > 1 Then
                                        drSurchargeRow(oReader("Coverage")) = CDec(1 + ((CDec(CDec(drSurchargeRow(oReader("Coverage")) - 1))) + (CDec(CDec(oReader("Factor") - 1)))))
                                    Else

                                        If sMaxDiscountFactors.Contains(oReader("FactorCode")) Then
                                            drDiscountMaxRow(oReader("Coverage")) = CDec(1 - ((CDec(1 - CDec(drDiscountMaxRow(oReader("Coverage"))))) + (CDec(1 - CDec(oReader("Factor"))))))
                                        Else
                                            drDiscountNoMaxRow(oReader("Coverage")) = CDec(1 - ((CDec(1 - CDec(drDiscountNoMaxRow(oReader("Coverage"))))) + (CDec(1 - CDec(oReader("Factor"))))))
                                        End If
                                        Exit For
                                    End If
                                End If
                            Next
                        End If
                    Next
                Loop

                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If
            End Using

            ' MAXDiscount
            If Not drDiscountNoMaxRow Is Nothing Then
                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT ItemSubCode,ItemValue FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "StateInfo with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND ItemGroup = 'MAXDISCOUNT' "
                    sSql = sSql & " AND ItemCode = 'PERCENT' "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                    oReader = cmd.ExecuteReader
                    Do While oReader.Read()
                        If CDec(drDiscountMaxRow(oReader("ItemSubCode"))) < CDec(oReader("ItemValue")) Then
                            drDiscountMaxRow(oReader("ItemSubCode")) = CDec(oReader("ItemValue"))
                        End If
                    Loop

                    If Not oReader Is Nothing Then
                        oReader.Close()
                        oReader = Nothing
                    End If
                End Using



                ' sum nomax and max rows and put into drFActorRow
                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT Distinct Coverage FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDiscount with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " ORDER BY Coverage Asc "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                    oReader = cmd.ExecuteReader
                    If oReader.HasRows Then
                        Do While oReader.Read()
                            drFactorRow(oReader("Coverage")) = (1 - ((1 - CDec(drDiscountNoMaxRow(oReader("Coverage")))) + (1 - CDec(drDiscountMaxRow(oReader("Coverage"))))))
                        Loop
                    End If

                    If Not oReader Is Nothing Then
                        oReader.Close()
                        oReader = Nothing
                    End If
                End Using
            End If

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            If Not drSurchargeRow Is Nothing Then
                FactorTable.Rows.Add(drSurchargeRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try



    End Function
    Public Overridable Function dbGetBaseRateFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim dMonthlyUUMPDFactor As Decimal = 0

        Try

            If oPolicy.Program.ToUpper = "MONTHLY" Then
                Dim oCov As clsPACoverage = GetCoverage("UUMPD", oPolicy)
                If Not oCov Is Nothing Then
                    dMonthlyUUMPDFactor = CalculateUUMPDMonthlyBaseRate(oPolicy)
                End If
            End If

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorBaseRate with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "BaseRate"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            If oPolicy.Program.ToUpper = "MONTHLY" And oReader.Item("Coverage").ToString.ToUpper = "UUMPD" Then
                                'add it to the data row
                                drFactorRow.Item(oReader.Item("Coverage")) = dMonthlyUUMPDFactor
                                Exit For
                            Else
                                'add it to the data row
                                drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                                Exit For
                            End If
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetDriverFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing

        Try

            'get the driver that is assigned to this vehicle
            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)

            If Not oDriver Is Nothing Then
                For Each oFactor As clsBaseFactor In oDriver.Factors
                    bFactorType = False
                    drFactorRow = Nothing

                    Using cmd As New SqlCommand(sSql, moConn)

                        sSql = " SELECT Coverage, Description, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriver with(nolock)"
                        sSql = sSql & " WHERE Program = @Program "
                        sSql = sSql & " AND EffDate <= @RateDate "
                        sSql = sSql & " AND ExpDate > @RateDate "
                        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                        sSql = sSql & " AND FactorCode = @FactorCode "
                        sSql = sSql & " ORDER BY Coverage Asc "

                        'Execute the query
                        cmd.CommandText = sSql

                        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                        cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode

                        oReader = cmd.ExecuteReader

                        If oReader.HasRows Then
                            drFactorRow = FactorTable.NewRow
                            drFactorRow.Item("FactorName") = oFactor.FactorCode
                        End If

                        Do While oReader.Read()
                            oFactor.FactorDesc = oReader.Item("Description").ToString
                            'this returns the factor and factor type for all coverages
                            'we will start with the 2nd column since we know the 1st is the factor name
                            For i As Integer = 1 To FactorTable.Columns.Count - 1
                                If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                                    Dim bIsCappedFactor As Boolean = False
                                    If Not msCappedFactors Is Nothing Then
                                        For q As Integer = 0 To msCappedFactors.Length - 1
                                            If oFactor.FactorCode.ToUpper = msCappedFactors(q).ToUpper Then
                                                bIsCappedFactor = True
                                                Exit For
                                            End If
                                        Next
                                    End If
                                    If bIsCappedFactor Then 'factor is part of max discount equation
                                        drTotalsRow = GetRow(moCappedFactorsTable, "Totals")
                                        drMaxDiscountRow = GetRow(moCappedFactorsTable, "MaxDiscountAmt")
                                        If (CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) Then
                                            'no more discounts, set to 1.0
                                            'add it to the data row
                                            drFactorRow.Item(oReader.Item("Coverage")) = 1
                                            'drTotalsRow.Item(oReader.Item("Coverage")) += drTotalsRow.Item(oReader.Item("Coverage"))
                                            Exit For
                                        ElseIf (CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage"))) * CDec(oReader.Item("Factor")) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) Then
                                            'set the factor to the difference between the MaxAmount and the current total
                                            Dim dDiscount As Decimal = 0
                                            dDiscount = CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) / CDec(drTotalsRow.Item(oReader.Item("Coverage")))
                                            drFactorRow.Item(oReader.Item("Coverage")) = dDiscount
                                            drTotalsRow.Item(oReader.Item("Coverage")) = CDec(drTotalsRow.Item(oReader.Item("Coverage"))) * dDiscount
                                        Else
                                            'add it to the data row
                                            drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                                            Dim dMultiplier As Decimal = 0
                                            dMultiplier = IIf(CDec(drTotalsRow.Item(oReader.Item("Coverage"))) = 0, 1, CDec(drTotalsRow.Item(oReader.Item("Coverage"))))
                                            drTotalsRow.Item(oReader.Item("Coverage")) = dMultiplier * CDec(oReader.Item("Factor"))
                                            Exit For
                                        End If
                                    Else
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                                        Exit For
                                    End If

                                End If
                            Next
                            If Not bFactorType Then
                                drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                bFactorType = True
                            End If
                        Loop

                    End Using
                    If Not drFactorRow Is Nothing Then
                        FactorTable.Rows.Add(drFactorRow)
                    End If
                    If Not oReader Is Nothing Then
                        oReader.Close()
                        oReader = Nothing
                    End If
                Next
            End If
            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function UseDriverAgeBumping(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim bAgeBump As Boolean = False

        If oPolicy.Program.ToUpper = "MONTHLY" Then
            bAgeBump = True
        End If

        If oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT" Then

            If oPolicy.StateCode = "17" Or oPolicy.StateCode = "42" Or oPolicy.StateCode = "03" Or oPolicy.StateCode = "35" Then

                If StateInfoContainsProgramSpecific("RATE", "DRIVER", "AGEBUMP", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.Program, oPolicy.RateDate) Then
                    bAgeBump = True
                End If

            End If

        End If

        Return bAgeBump

    End Function

    Public Overridable Function dbGetDriverAdjustmentFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim iDriverAge As Integer = 0

        Try

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
            iDriverAge = oDriver.Age
            'If oPolicy.Program.ToUpper = "SUMMIT" Or oPolicy.Program.ToUpper = "MONTHLY" Or (oPolicy.StateCode = 35 And oPolicy.Program.ToUpper = "CLASSIC") Then
            If UseDriverAgeBumping(oPolicy) Then
                'Drivers <= age 24 – if the DOB is within 30 days after inception, use the higher age for Driver Class factors.  This rule only applies to calculating age for driver class.  Do not use this rule for determining PNI Youthful or any other factor
                If oDriver.Age <= 24 And oDriver.DOB > "01/01/1900" Then

                    Dim dtNextBDay As Date
                    Try
                        dtNextBDay = oDriver.DOB.Month & "/" & oDriver.DOB.Day & "/" & oPolicy.EffDate.Year
                    Catch ex As Exception
                        ' Catches leap year issue
                        If oDriver.DOB.Month = 2 And oDriver.DOB.Day = 29 Then
                            dtNextBDay = "3/1/" & oPolicy.EffDate.Year
                        End If
                    End Try


                    If dtNextBDay < oPolicy.EffDate Then

                        Try
                            dtNextBDay = oDriver.DOB.Month & "/" & oDriver.DOB.Day & "/" & oPolicy.EffDate.Year + 1
                        Catch ex As Exception
                            ' Catches leap year issue
                            If oDriver.DOB.Month = 2 And oDriver.DOB.Day = 29 Then
                                dtNextBDay = "3/1/" & oPolicy.EffDate.Year + 1
                            End If
                        End Try


                    End If

                    If DateDiff(DateInterval.Day, oPolicy.EffDate, dtNextBDay) < 30 AndAlso DateDiff(DateInterval.Day, oPolicy.EffDate, dtNextBDay) > 0 Then
                        iDriverAge = oDriver.Age + 1
                    End If
                End If
            End If

            Dim sDriverClass As String = ""
            'DriverClass = Marital Staus + Gender + Age
            If StateInfoContains("RATE", "WIDOW", "MARRIED", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED" Or oDriver.MaritalStatus.Trim.ToUpper = "WIDOWED", "M", "S")
            Else
                sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED", "M", "S")
            End If

            sDriverClass &= IIf(oDriver.Gender.Trim.ToUpper.StartsWith("M"), "M", "F")
            If oDriver.Age > 99 Then
                sDriverClass &= "99"
            Else
                sDriverClass &= iDriverAge
            End If

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Points, DriverClass, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverAdjustment with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Points = (SELECT TOP 1 Points "
                sSql = sSql & "                FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverAdjustment with(nolock) "
                sSql = sSql & "                WHERE Cast(Points As Int) <= Cast(@Points As Int) "
                sSql = sSql & "                ORDER BY Cast(Points As Int) Desc) "
                sSql = sSql & " AND DriverClass = @DriverClass "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Points", SqlDbType.VarChar, 2).Value = oDriver.Points
                cmd.Parameters.Add("@DriverClass", SqlDbType.VarChar, 8).Value = sDriverClass

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "DriverAdjust"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Return Nothing
            'Throw New ArgumentException(ex.Message & ex.StackTrace,ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If


        End Try

    End Function

    Public Overridable Function dbGetDriverAgePointsFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverAgePoints with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND MinAge <= @InsuredAge "
                sSql = sSql & " AND MaxAge > @InsuredAge "
                sSql = sSql & " AND MinPoints <= @Points "
                sSql = sSql & " AND MaxPoints > @Points "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@InsuredAge", SqlDbType.Int, 22).Value = oDriver.Age 'oPolicy.PolicyInsured.Age
                cmd.Parameters.Add("@Points", SqlDbType.Int, 22).Value = oDriver.Points

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "DriverAgePoints"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetDriverClassFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim iDriverAge As Integer = 0

        Try

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
            iDriverAge = oDriver.Age
            'If oPolicy.Program.ToUpper = "SUMMIT" Or oPolicy.Program.ToUpper = "MONTHLY" Or (oPolicy.StateCode = 35 And oPolicy.Program.ToUpper = "CLASSIC") Then
            If UseDriverAgeBumping(oPolicy) Then
                'Drivers <= age 24 – if the DOB is within 30 days after inception, use the higher age for Driver Class factors.  This rule only applies to calculating age for driver class.  Do not use this rule for determining PNI Youthful or any other factor
                If oDriver.Age <= 24 And oDriver.DOB > "01/01/1900" Then

                    Dim dtNextBDay As Date
                    Try
                        dtNextBDay = oDriver.DOB.Month & "/" & oDriver.DOB.Day & "/" & oPolicy.EffDate.Year
                    Catch ex As Exception
                        ' Catches leap year issue
                        If oDriver.DOB.Month = 2 And oDriver.DOB.Day = 29 Then
                            dtNextBDay = "3/1/" & oPolicy.EffDate.Year
                        End If
                    End Try

                    If dtNextBDay < oPolicy.EffDate Then

                        Try
                            dtNextBDay = oDriver.DOB.Month & "/" & oDriver.DOB.Day & "/" & oPolicy.EffDate.Year + 1
                        Catch ex As Exception
                            ' Catches leap year issue
                            If oDriver.DOB.Month = 2 And oDriver.DOB.Day = 29 Then
                                dtNextBDay = "3/1/" & oPolicy.EffDate.Year + 1
                            End If
                        End Try


                    End If

                    If DateDiff(DateInterval.Day, oPolicy.EffDate, dtNextBDay) > 0 AndAlso DateDiff(DateInterval.Day, oPolicy.EffDate, dtNextBDay) < 30 Then
                        iDriverAge = oDriver.Age + 1
                    End If
                End If
            End If

            Dim sDriverClass As String = ""
            'DriverClass = Marital Staus + Gender + Age
            If StateInfoContains("RATE", "WIDOW", "MARRIED", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED" Or oDriver.MaritalStatus.Trim.ToUpper = "WIDOWED", "M", "S")
            Else
                sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED", "M", "S")
            End If
            sDriverClass &= IIf(oDriver.Gender.Trim.ToUpper.StartsWith("M"), "M", "F")
            If iDriverAge > 99 Then iDriverAge = 99
            sDriverClass &= iDriverAge 'oDriver.Age

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverClass with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND DriverClass = @DriverClass "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@DriverClass", SqlDbType.VarChar, 8).Value = sDriverClass

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "DriverClass"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetDriverPointsFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverPoints with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Points = (SELECT TOP 1 Points "
                sSql = sSql & "                FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverPoints  with(nolock) "
                sSql = sSql & "                WHERE Cast(Points As Int) <= Cast(@Points As Int) "
                sSql = sSql & "                 AND EffDate <= @RateDate "
                sSql = sSql & "                 AND ExpDate > @RateDate "
                sSql = sSql & "                 AND Program = @Program "
                sSql = sSql & "                 AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & "                ORDER BY Cast(Points As Int) Desc) "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Points", SqlDbType.VarChar, 2).Value = IIf(oDriver.Points > 30, 30, oDriver.Points)

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "DriverPoints"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetHouseholdStructureFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try
            'we are going to look up the factors based on the policy insured here

            oPolicy.PolicyInsured.PCRelationship = SetPCRelationship(oPolicy)

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorHouseholdStructure with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND MultiCar = @MultiCar "
                sSql = sSql & " AND MaritalStatus = @MaritalStatus "
                sSql = sSql & " AND Youthful = @Youthful "
                sSql = sSql & " AND HomeOwner = @HomeOwner "
                sSql = sSql & " AND PCRelationship = @PCRelationship "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@MultiCar", SqlDbType.VarChar, 1).Value = IIf(GetVehicleCount(oPolicy) > 1, "Y", "N")

                If StateInfoContains("RATE", "WIDOW", "MARRIED", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                    cmd.Parameters.Add("@MaritalStatus", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.MaritalStatus.Trim.ToUpper = "MARRIED" Or oPolicy.PolicyInsured.MaritalStatus.Trim.ToUpper = "WIDOWED", "M", "S")
                Else
                    cmd.Parameters.Add("@MaritalStatus", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.MaritalStatus.Trim.ToUpper = "MARRIED", "M", "S")
                End If
                cmd.Parameters.Add("@Youthful", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.Age < 21, "Y", "N")
                cmd.Parameters.Add("@HomeOwner", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER", "Y", "N")
                cmd.Parameters.Add("@PCRelationship", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.PCRelationship, "Y", "N")

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "HouseholdStructure"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

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

    Public Overridable Function dbGetMarketPointsFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorMarketPoints with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND UWTier = @UWTier "
                sSql = sSql & " AND MinPoints <= @Points "
                sSql = sSql & " AND MaxPoints > @Points "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.UWTier
                cmd.Parameters.Add("@Points", SqlDbType.Int, 22).Value = IIf(oDriver.Points > 30, 30, oDriver.Points)

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "MarketPoints"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetModelYearFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim lVehYear As Long = 0

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            If oPolicy.Program.ToUpper = "MONTHLY" Then
                lVehYear = oVeh.VehicleAge
            Else
                If oVeh.VehicleYear < 1980 And oVeh.VehicleYear > 1 Then
                    lVehYear = 1980
                ElseIf oVeh.VehicleYear > Now.Year Then
                    lVehYear = Now.Year
                Else
                    lVehYear = oVeh.VehicleYear
                End If
            End If

            If oVeh.VinNo = "NONOWNER" Then
                lVehYear = 1
            End If

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorModelYear with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & "	AND VehicleCountType IN ('A', @VehicleCountType ) "
                sSql = sSql & " AND ModelYear = (Select Max( Cast(ModelYear As Int)) "
                sSql = sSql & "                     FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorModelYear with(nolock)"
                sSql = sSql & "                     WHERE(ModelYear <= @ModelYear)"
                sSql = sSql & "                         AND Program = @Program "
                sSql = sSql & "                         AND EffDate <= @RateDate "
                sSql = sSql & "                         AND ExpDate > @RateDate "
                sSql = sSql & "							AND VehicleCountType IN ('A', @VehicleCountType ) "
                sSql = sSql & "                         AND AppliesToCode IN ('B',  @AppliesToCode ) )"
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@ModelYear", SqlDbType.VarChar, 4).Value = lVehYear 'IIf(oPolicy.Program.ToUpper = "MONTHLY", oVeh.VehicleAge, oVeh.VehicleYear)
                cmd.Parameters.Add("@VehicleCountType", SqlDbType.VarChar, 1).Value = GetVehicleCountType(oPolicy)

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "ModelYear"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetPolicyFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing

        Try
            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                bFactorType = False
                drFactorRow = Nothing

                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT Coverage, Description, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorPolicy with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND FactorCode = @FactorCode "
                    sSql = sSql & " ORDER BY Coverage Asc "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode

                    oReader = cmd.ExecuteReader

                    If oReader.HasRows Then
                        drFactorRow = FactorTable.NewRow
                        drFactorRow.Item("FactorName") = oFactor.FactorCode
                    End If

                    Do While oReader.Read()
                        oFactor.FactorDesc = oReader.Item("Description").ToString
                        'this returns the factor and factor type for all coverages
                        'we will start with the 2nd column since we know the 1st is the factor name
                        For i As Integer = 1 To FactorTable.Columns.Count - 1
                            If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                                Dim bIsCappedFactor As Boolean = False
                                If Not msCappedFactors Is Nothing Then
                                    For q As Integer = 0 To msCappedFactors.Length - 1
                                        If oFactor.FactorCode.ToUpper = msCappedFactors(q).ToUpper Then
                                            bIsCappedFactor = True
                                            Exit For
                                        End If
                                    Next
                                End If
                                If bIsCappedFactor Then 'factor is part of max discount equation
                                    drTotalsRow = GetRow(moCappedFactorsTable, "Totals")
                                    drMaxDiscountRow = GetRow(moCappedFactorsTable, "MaxDiscountAmt")
                                    If (CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) Then
                                        'no more discounts, set to 1.0
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage")) = 1
                                        Exit For
                                    ElseIf (CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage"))) * CDec(oReader.Item("Factor")) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) Then
                                        'set the factor to the difference between the MaxAmount and the current total
                                        Dim dDiscount As Decimal = 0
                                        dDiscount = CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) / CDec(drTotalsRow.Item(oReader.Item("Coverage")))
                                        drFactorRow.Item(oReader.Item("Coverage")) = dDiscount
                                        drTotalsRow.Item(oReader.Item("Coverage")) = CDec(drTotalsRow.Item(oReader.Item("Coverage"))) * dDiscount
                                    Else
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                                        Dim dMultiplier As Decimal = 0
                                        dMultiplier = IIf(CDec(drTotalsRow.Item(oReader.Item("Coverage"))) = 0, 1, CDec(drTotalsRow.Item(oReader.Item("Coverage"))))
                                        drTotalsRow.Item(oReader.Item("Coverage")) = dMultiplier * CDec(oReader.Item("Factor"))
                                        Exit For
                                    End If
                                Else
                                    'add it to the data row
                                    drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                                    Exit For
                                End If

                            End If
                        Next
                        If Not bFactorType Then
                            drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                            bFactorType = True
                        End If
                    Loop

                End Using
                If Not drFactorRow Is Nothing Then
                    FactorTable.Rows.Add(drFactorRow)
                End If
                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If
            Next

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetPolicyDiscountMatrixFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

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
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "PolicyDiscountMatrix"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetStatedValueFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorStatedValue with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND MinStatedValue <= @StatedValue "
                sSql = sSql & " AND MaxStatedValue >= @StatedValue "
                sSql = sSql & " AND MinVehYear <= @VehicleYear "
                sSql = sSql & " AND MaxVehYear >= @VehicleYear "
                sSql = sSql & " AND Description = @Description "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@StatedValue", SqlDbType.Int, 22).Value = oVeh.StatedAmt
                cmd.Parameters.Add("@VehicleYear", SqlDbType.Int, 22).Value = oVeh.VehicleYear
                cmd.Parameters.Add("@Description", SqlDbType.VarChar, 75).Value = oVeh.VehicleTypeCode

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "StatedValue"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Function GetVINSymbol(ByVal Policy As clsPolicyPPA, ByVal sType As String, ByVal sYear As String) As String
        Dim sSymbol As String = String.Empty

        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing

        Try
            Dim dtModelYear As Date
            dtModelYear = CDate("1/1/" & sYear)
            'load the program info from the StateInfo table
            Dim oStateInfoDataSet As DataSet = LoadCommonStateInfoTable(Policy.Product, Policy.StateCode, dtModelYear)

            oStateInfoTable = oStateInfoDataSet.Tables(0)
            DataRows = oStateInfoTable.Select("Program IN ('" & Policy.Program & "', 'ALL') " & " AND ItemGroup='VIN' AND ItemCode='Lookup' AND ItemSubCode='" & sType & "'")

            For Each oRow As DataRow In DataRows
                sSymbol = oRow("ItemValue").ToString
            Next
        Catch ex As Exception
        Finally
        End Try

        Return sSymbol
    End Function

    Public Function LoadCommonStateInfoTable(ByVal iProduct As Integer, ByVal sStateCode As String, ByVal dtRateDate As Date) As DataSet
        Dim sSql As String = ""
        Dim oDS As New DataSet

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue "
                sSql = sSql & " FROM Common..StateInfo with(nolock) "
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " ORDER BY Program, ItemGroup, ItemCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "StateInfo")

                Return oDS

            End Using

        Catch ex As Exception
            Throw
        End Try
    End Function


    Public Overridable Function dbGetSymbolFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            drFactorRow = FactorTable.NewRow
            drFactorRow.Item("FactorName") = "Symbol"

            For Each oCov As clsPACoverage In oVeh.Coverages
                If Not oCov.IsMarkedForDelete Then
                    'get the symbol for this coverage
                    Dim DataRows() As DataRow
                    Dim oStateInfoTable As DataTable = Nothing
                    Dim sSymbolGroup As String = ""
                    Dim sSymbol As String = ""
                    oStateInfoTable = moStateInfoDataSet.Tables(0)

                    DataRows = oStateInfoTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup='COVERAGE' AND ItemCode='SYMBOL' AND ItemSubCode='" & oCov.CovGroup & "' ")

                    For Each oRow As DataRow In DataRows
                        'sSymbolGroup should be either LIA or PIP
                        sSymbolGroup = oRow.Item("ItemValue").ToString
                    Next

                    If oVeh.StatedAmt > 0 _
                        And (oVeh.VehicleSymbolCode = "999" Or oVeh.VehicleSymbolCode = "998" _
                        Or oVeh.VehicleSymbolCode = "997" Or oVeh.VehicleSymbolCode = "966" _
                        Or oVeh.VehicleSymbolCode = "65" Or oVeh.VehicleSymbolCode = "66" _
                        Or oVeh.VehicleSymbolCode = "67" Or oVeh.VehicleSymbolCode = "68") Then
                        sSymbol = GetVINSymbol(oPolicy, "STATEDVALUE", oVeh.VehicleYear)
                    Else

                        Select Case sSymbolGroup
                            Case "LIA"
                                sSymbol = oVeh.LiabilitySymbolCode.Trim
                            Case "PIP"
                                sSymbol = oVeh.PIPMedLiabilityCode.Trim
                            Case "VEH"

                                If oVeh.VehicleYear >= 2011 And (oCov.CovGroup = "LLS" Or oCov.CovGroup = "COL" Or oCov.CovGroup = "UMPD") And oVeh.CollSymbolCode.Trim.Length > 0 Then
                                    sSymbol = oVeh.CollSymbolCode.Trim
                                ElseIf oVeh.VehicleYear >= 2011 And oCov.CovGroup = "OTC" And oVeh.CompSymbolCode.Trim.Length > 0 Then
                                    sSymbol = oVeh.CompSymbolCode.Trim
                                Else
                                    sSymbol = oVeh.VehicleSymbolCode.Trim
                                End If

                                ' There are no 1 digit symbols
                                ' add the 0 ex: user enters 8 and then it won't rate (should be 08)
                                If Len(sSymbol) = 1 Then
                                    sSymbol = "0" & sSymbol
                                End If

                                ' There are no 3 digit symbols that start with 0
                                ' remove the 0 ex: user enters 025 and then it won't rate
                                If Len(sSymbol) > 2 Then
                                    If sSymbol(0) = "0" Then
                                        sSymbol = Right(sSymbol, 2)
                                    End If
                                End If
                        End Select
                    End If

                    Using cmd As New SqlCommand(sSql, moConn)

                        sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorSymbol with(nolock)"
                        sSql = sSql & " WHERE Program = @Program "
                        sSql = sSql & " AND EffDate <= @RateDate "
                        sSql = sSql & " AND ExpDate > @RateDate "
                        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                        sSql = sSql & " AND Coverage = @Coverage "
                        sSql = sSql & " AND Symbol = @Symbol "
                        sSql = sSql & " AND MinVehYear <= @VehYear "
                        sSql = sSql & " AND MaxVehYear >= @VehYear "
                        sSql = sSql & " ORDER BY Coverage Asc "

                        'Execute the query
                        cmd.CommandText = sSql

                        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                        cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oCov.CovGroup

                        Dim sVehicleYear As String = oVeh.VehicleYear
                        If oVeh.VehicleYear.Trim = "1" Then
                            sVehicleYear = "1900"
                        End If
                        cmd.Parameters.Add("@VehYear", SqlDbType.VarChar, 5).Value = sVehicleYear
                        cmd.Parameters.Add("@Symbol", SqlDbType.VarChar, 4).Value = sSymbol

                        oReader = cmd.ExecuteReader

                        Do While oReader.Read()
                            'this returns the factor and factor type for all coverages
                            'we will start with the 2nd column since we know the 1st is the factor name
                            For i As Integer = 1 To FactorTable.Columns.Count - 1
                                If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                                    'add it to the data row
                                    drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                                    Exit For
                                End If
                            Next
                            If Not bFactorType Then
                                drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                bFactorType = True
                            End If
                        Loop

                    End Using

                    If Not oReader Is Nothing Then
                        oReader.Close()
                        oReader = Nothing
                    End If
                End If
            Next

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadFactorPolicyTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))

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
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadFactorDriverTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))

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
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadFactorVehicleTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))

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
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Overridable Function dbGetTerritoryFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorTerritory with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Territory = @Territory "
                sSql = sSql & " AND VehicleCountType IN ('A', @VehicleCountType ) "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Territory", SqlDbType.VarChar, 5).Value = oVeh.Territory
                cmd.Parameters.Add("@VehicleCountType", SqlDbType.VarChar, 1).Value = GetVehicleCountType(oPolicy)

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Territory"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function


    Public Overridable Function dbGetMarketAdjustmentFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        'Function dbGetTerritoryFactor() was cannibalized as the starting point for this function.

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorMarketAdjustment with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "

                'Specific to market adjustment.
                'NOTE:  The clsVehicleUnit object referenced later in this function does not have its
                'City property populated.  That was causing this query to return 0 rows, which resulted
                'in the LogXML, for a given quote, in the event log table not having a market adjustment
                'node.  Through discussion with Terry and Mindi it was determined that we don't actually
                'need to filter on city, so that criterion was taken out.
                '
                sSql = sSql & " AND Zip = @Zip "
                'sSql = sSql & " AND City = @City "
                sSql = sSql & " AND County = @County "

                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                'Until this function is verified as working, I left these lines in, just for reference.
                'They came from the function (dbGetTerritoryFactor()) cannibalized as the starting point 
                'of this function.
                'cmd.Parameters.Add("@Territory", SqlDbType.VarChar, 5).Value = oVeh.Territory
                'cmd.Parameters.Add("@VehicleCountType", SqlDbType.VarChar, 1).Value = GetVehicleCountType(oPolicy)

                'Specific to market adjustment.
                cmd.Parameters.Add("@Zip", SqlDbType.VarChar, 5).Value = oVeh.Zip
                '
                'NOTE:  The clsVehicleUnit object referenced later in this function does not have its
                'City property populated.  That was causing this query to return 0 rows, which resulted
                'in the LogXML, for a given quote, in the event log table not having a market adjustment
                'node.  Through discussion with Terry and Mindi it was determined that we don't actually
                'need to filter on city, so that criterion was taken out.
                '
                'cmd.Parameters.Add("@City", SqlDbType.VarChar, 75).Value = oVeh.City
                cmd.Parameters.Add("@County", SqlDbType.VarChar, 30).Value = oVeh.County

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "MarketAdjustFactor"  'Must be same string as in RateOrder worksheet
                End If

                Do While oReader.Read()
                    'NOTE (John Robottom):  The following comment came with the function cannibalized as
                    'the starting point of this function.  It, and the code associated with it, is the
                    'same for all the "dbGet" functions; ie, it didn't need to be customized for
                    'market adjustment.
                    '
                    'This returns the factor and factor type for all coverages.
                    'We will start with the 2nd column since we know the 1st is the factor name.
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetTerritoryFactor_Summit(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            drFactorRow = FactorTable.NewRow
            drFactorRow.Item("FactorName") = "Territory"

            For Each oCov As clsPACoverage In oVeh.Coverages
                If Not oCov.IsMarkedForDelete Then
                    Using cmd As New SqlCommand(sSql, moConn)

                        sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorTerritory with(nolock)"
                        sSql = sSql & " WHERE Program = @Program "
                        sSql = sSql & " AND EffDate <= @RateDate "
                        sSql = sSql & " AND ExpDate > @RateDate "
                        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                        sSql = sSql & " AND Coverage = @Coverage "
                        sSql = sSql & " AND Territory = @Territory "
                        sSql = sSql & " AND VehicleCountType IN ('A', @VehicleCountType ) "
                        sSql = sSql & " ORDER BY Coverage Asc "

                        'Execute the query
                        cmd.CommandText = sSql

                        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                        cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oCov.CovGroup
                        cmd.Parameters.Add("@Territory", SqlDbType.VarChar, 5).Value = oCov.Territory.Trim
                        cmd.Parameters.Add("@VehicleCountType", SqlDbType.VarChar, 1).Value = GetVehicleCountType(oPolicy)

                        oReader = cmd.ExecuteReader

                        Do While oReader.Read()
                            'this returns the factor and factor type for all coverages
                            'we will start with the 2nd column since we know the 1st is the factor name
                            For i As Integer = 1 To FactorTable.Columns.Count - 1
                                If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                                    'add it to the data row
                                    drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                                    Exit For
                                End If
                            Next
                            If Not bFactorType Then
                                drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                bFactorType = True
                            End If
                        Loop
                        If Not oReader Is Nothing Then
                            oReader.Close()
                            oReader = Nothing
                        End If
                    End Using
                End If
            Next

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function



    Public Overridable Function dbGetTierMatrixFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim sFormType As String = ""

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorTierMatrix with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND CreditTier = @CreditTier "
                sSql = sSql & " AND UWTier = @UWTier "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                'If sCreditTier = "" Then sCreditTier = "U5"
                cmd.Parameters.Add("@CreditTier", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.CreditTier
                cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.UWTier

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "TierMatrix"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetVehicleFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            For Each oFactor As clsBaseFactor In oVeh.Factors
                bFactorType = False
                drFactorRow = Nothing

                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT Coverage, Description, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorVehicle with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND FactorCode = @FactorCode "
                    sSql = sSql & " ORDER BY Coverage Asc "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode

                    oReader = cmd.ExecuteReader

                    If oReader.HasRows Then
                        drFactorRow = FactorTable.NewRow
                        drFactorRow.Item("FactorName") = oFactor.FactorCode
                    End If

                    Do While oReader.Read()
                        oFactor.FactorDesc = oReader.Item("Description").ToString
                        'this returns the factor and factor type for all coverages
                        'we will start with the 2nd column since we know the 1st is the factor name
                        For i As Integer = 1 To FactorTable.Columns.Count - 1
                            If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                                Dim bIsCappedFactor As Boolean = False
                                If Not msCappedFactors Is Nothing Then
                                    For q As Integer = 0 To msCappedFactors.Length - 1
                                        If oFactor.FactorCode.ToUpper = msCappedFactors(q).ToUpper Then
                                            bIsCappedFactor = True
                                            Exit For
                                        End If
                                    Next
                                End If
                                If bIsCappedFactor Then 'factor is part of max discount equation
                                    drTotalsRow = GetRow(moCappedFactorsTable, "Totals")
                                    drMaxDiscountRow = GetRow(moCappedFactorsTable, "MaxDiscountAmt")
                                    If (CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) Then
                                        'no more discounts, set to 1.0
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage")) = 1
                                        'drTotalsRow.Item(oReader.Item("Coverage")) += drTotalsRow.Item(oReader.Item("Coverage"))
                                        Exit For
                                    ElseIf (CDec(drTotalsRow.Item(oReader.Item("Coverage"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage"))) * CDec(oReader.Item("Factor")) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) Then
                                        'set the factor to the difference between the MaxAmount and the current total
                                        Dim dDiscount As Decimal = 0
                                        dDiscount = CDec(drMaxDiscountRow.Item(oReader.Item("Coverage"))) / CDec(drTotalsRow.Item(oReader.Item("Coverage")))
                                        drFactorRow.Item(oReader.Item("Coverage")) = dDiscount
                                        drTotalsRow.Item(oReader.Item("Coverage")) = CDec(drTotalsRow.Item(oReader.Item("Coverage"))) * dDiscount
                                    Else
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                                        Dim dMultiplier As Decimal = 0
                                        dMultiplier = IIf(CDec(drTotalsRow.Item(oReader.Item("Coverage"))) = 0, 1, CDec(drTotalsRow.Item(oReader.Item("Coverage"))))
                                        drTotalsRow.Item(oReader.Item("Coverage")) = dMultiplier * CDec(oReader.Item("Factor"))
                                        Exit For
                                    End If
                                Else
                                    'add it to the data row
                                    drFactorRow.Item(oReader.Item("Coverage")) = CDec(oReader.Item("Factor"))
                                    Exit For
                                End If

                            End If
                        Next
                        If Not bFactorType Then
                            drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                            bFactorType = True
                        End If
                    Loop

                End Using
                If Not drFactorRow Is Nothing Then
                    FactorTable.Rows.Add(drFactorRow)
                End If
                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If
            Next

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetCoverageFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable, ByVal sType As String) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
        Dim dMonthlyOTCFactor As Decimal = 0
        Dim dMonthlyCOLFactor As Decimal = 0

        Try
            'we will do a check here to make sure that the covdeductibles and covlimits are set on all of the coverages
            For Each oCov As clsPACoverage In oVeh.Coverages
                If Not oCov.IsMarkedForDelete Then
                    Dim CovValues() As String = oCov.CovCode.Split(":")
                    If CovValues.Length > 3 Then
                        Dim CovGroup As String = CovValues(0)
                        Dim CovValue As String = CovValues(1)
                        Dim CovType As String = CovValues(2) 'Limit or Deductible
                        Dim CovLevel As Boolean = IIf(CovValues(3) = "P", True, False) 'Policy or Vehicle
                        Select Case CovType.ToUpper
                            Case "D"
                                If oCov.CovDeductible = "" Then
                                    oCov.CovDeductible = CovValue
                                End If
                            Case "L"
                                If oCov.CovLimit = "" Then
                                    If oCov.CovGroup <> "PID" Then
                                        oCov.CovLimit = CovValue
                                    Else
                                        oCov.CovLimit = "0"
                                    End If
                                End If
                        End Select
                    End If
                End If
            Next

            ' todo: maybe leave this in for old rate structure?

            If oPolicy.Program.ToUpper = "MONTHLY" Then
                If Not StateInfoContains("RATE", "MONTHLY", "COVFACTOR", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                    Dim oOTCCoverage As clsPACoverage = GetCoverage("OTC", oPolicy)
                    If Not oOTCCoverage Is Nothing Then
                        dMonthlyOTCFactor = CalculateCovFactor(oOTCCoverage.CovGroup, oOTCCoverage.CovDeductible, oPolicy)
                    End If
                    Dim oCOLCoverage As clsPACoverage = GetCoverage("COL", oPolicy)
                    If Not oCOLCoverage Is Nothing Then
                        dMonthlyCOLFactor = CalculateCovFactor(oCOLCoverage.CovGroup, oCOLCoverage.CovDeductible, oPolicy)
                    End If
                End If
            End If

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Code, Description, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorCoverage with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND UWTier = @UWTier "
                sSql = sSql & " AND FactorType = @Type "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate '08/16/
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                If oPolicy.Program.ToUpper = "SUMMIT" Or ((oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT") And oPolicy.StateCode = "42") Then
                    cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.UWTier
                Else
                    cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = 1
                End If
                cmd.Parameters.Add("@Type", SqlDbType.VarChar, 8).Value = sType.Trim
                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow

                    Dim sFactorName As String
                    If sType.Trim.ToUpper = "MIDADD" Then
                        sFactorName = "CoverageAdd"
                    Else
                        sFactorName = "Coverage"
                    End If
                    drFactorRow.Item("FactorName") = sFactorName
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") = FactorTable.Columns.Item(i).ColumnName Then
                            For Each oCov As clsPACoverage In oVeh.Coverages
                                If Not oCov.IsMarkedForDelete Then
                                    If oReader.Item("Coverage").ToString.ToUpper = oCov.CovGroup.ToUpper Then
                                        ' todo: leave this in for old rate structure?
                                        If oPolicy.Program.ToUpper = "MONTHLY" And (oCov.CovGroup.ToUpper = "OTC" Or oCov.CovGroup.ToUpper = "COL") And dMonthlyOTCFactor > 0 And dMonthlyOTCFactor > 0 Then
                                            If oCov.CovGroup.ToUpper = "OTC" Then
                                                drFactorRow.Item(oReader.Item("Coverage")) = dMonthlyOTCFactor
                                            Else
                                                drFactorRow.Item(oReader.Item("Coverage")) = dMonthlyCOLFactor
                                            End If
                                            Exit For
                                        Else

                                            If oReader.Item("Code") = oCov.CovCode Then
                                                'add it to the data row
                                                If sType.ToUpper.Trim = "MIDADD" Then
                                                    drFactorRow.Item(oReader.Item("Coverage")) = UpdateMidAddFactorBasedOnTerm(oPolicy, oReader.Item("Factor"))
                                                Else
                                                    drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                                                End If
                                                Exit For
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                            Exit For
                        End If
                    Next
                    If Not bFactorType Then
                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                        bFactorType = True
                    End If
                Loop

            End Using

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetFeeFactor(ByVal oPolicy As clsPolicyPPA, ByVal FeeTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFeeRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim dTotalFees As Decimal = 0

        If oPolicy.CallingSystem.ToUpper <> "PAS" Then
            oPolicy.Billing.Fees.Clear()
        End If

        Try
            For Each oFee As clsBaseFee In oPolicy.Fees

                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT Factor, FactorType, FeeApplicationType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorFee with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND FeeCode = @FeeCode "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@FeeCode", SqlDbType.VarChar, 9).Value = oFee.FeeCode

                    oReader = cmd.ExecuteReader

                    If oReader.HasRows Then
                        drFeeRow = FeeTable.NewRow
                        drFeeRow.Item("FeeCode") = oFee.FeeCode
                    End If

                    Do While oReader.Read()
                        'add it to the data row and the fee object
                        drFeeRow.Item("Factor") = oReader.Item("Factor")
                        drFeeRow.Item("FactorType") = oReader.Item("FactorType")
                        drFeeRow.Item("FeeApplicationType") = oReader.Item("FeeApplicationType")
                        oFee.FeeAmt = oReader.Item("Factor")

                        If oFee.FeeType <> "P" Then
                            oFee.FeeType = oReader.Item("FactorType")
                        Else
                            If oPolicy.CallingSystem.ToUpper <> "PAS" Then
                                oPolicy.Billing.Fees.Add(oFee)
                            End If
                        End If
                        oFee.FeeApplicationType = oReader.Item("FeeApplicationType")
                    Loop

                    CheckCalculatedFee(oPolicy, oFee)

                End Using
                If Not drFeeRow Is Nothing Then
                    FeeTable.Rows.Add(drFeeRow)
                    drFeeRow = Nothing
                End If
                dTotalFees = dTotalFees + oFee.FeeAmt
                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If
            Next
            oPolicy.TotalFees = dTotalFees

            'oPolicy = LoadMVRFeesToBilling(oPolicy)

            Return drFeeRow

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function

    Public Overridable Sub CheckCalculatedFee(ByRef oPolicy As clsPolicyPPA, ByRef oFee As clsBaseFee)

    End Sub

    Public Overloads Sub Calculate(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)

        Dim drTotalsRow As DataRow = Nothing

        Try

            GetPreMultPremium(oPolicy, FactorTable)

            GetPreAddPremium(oPolicy, FactorTable)

            GetMidMultPremium(oPolicy, FactorTable)

            'set correct factor for mid add factors based off of current premium amount
            'UpdateMidAddFactorAmounts(oPolicy, FactorTable)

            GetMidAddPremium(oPolicy, FactorTable)

            'check for minimum premium amounts
            CheckMinPremAmounts(oPolicy, FactorTable)

            GetPostMultPremium(oPolicy, FactorTable)

            GetPostAddPremium(oPolicy, FactorTable)

            'set correct factor for fee add factors based off of current premium amount
            UpdateFeeAddFactorAmounts(oPolicy, FactorTable)

            GetFeeAddPremium(oPolicy, FactorTable)

            GetLastMultPremium(oPolicy, FactorTable)

            'round to nearest dollar
            drTotalsRow = GetRow(FactorTable, "Totals")
            For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                    drTotalsRow(oDataCol.ColumnName.ToString) = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                End If
            Next

        Catch ex As Exception
            Throw New ArgumentException(ex.Message)
        Finally
            If Not drTotalsRow Is Nothing Then
                drTotalsRow = Nothing
            End If
        End Try

    End Sub

    Public Sub GetLastMultPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' post mult according to the rate order table and process in the order according to the rate order table
        'Get the factor value and multiply it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0
        'Dim sColCov() As String

        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'LastMult' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dTotal = 0 Then dTotal = 1
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal * dFactor
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.VehicleUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.VehicleUnits(0).Coverages.Item(p).CovGroup Then

                                                Dim oCov As clsPACoverage = oPolicy.VehicleUnits(0).Coverages.Item(p)
                                                Dim oPremFactor As New clsPremiumFactor
                                                oPremFactor.Type = Right(FactorTable.Columns(y).ColumnName.ToUpper, 1)
                                                oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                                oPremFactor.FactorCode = oReader.Item("FactorName")
                                                oPremFactor.FactorName = oReader.Item("FactorName")

                                                oCov.Factors.Add(oPremFactor)
                                                Exit For
                                            End If
                                        Next p
                                    End If
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using
        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetPreMultPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' pre mult according to the rate order table and process in the order according to the rate order table
        'Get the factor value and multiply it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0
        'Dim sColCov() As String

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'PreMult' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dTotal = 0 Then dTotal = 1
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal * dFactor

                                    dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")
                                            If Not oCov.IsMarkedForDelete Then
                                                oCov.Factors.Add(oPremFactor)
                                            End If
                                            Exit For
                                        End If
                                    Next p
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub
    Public Sub GetPreAddPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' pre add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'PreAdd' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal + dFactor

                                    dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")
                                            If Not oCov.IsMarkedForDelete Then
                                                oCov.Factors.Add(oPremFactor)
                                            End If
                                            Exit For
                                        End If
                                    Next p
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Overridable Sub GetMidMultPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' mid mult according to the rate order table and process in the order according to the rate order table
        'Get the factor value and multiply it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0
        Dim drTotalsRow As DataRow = Nothing
        'Dim sColCov() As String

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'MidMult' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName").Trim = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dTotal = 0 Then dTotal = 1
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal * dFactor

                                    dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")
                                            If Not oCov.IsMarkedForDelete Then
                                                oCov.Factors.Add(oPremFactor)
                                            End If
                                            Exit For
                                        End If
                                    Next p
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

                'round to nearest dollar
                drTotalsRow = GetRow(FactorTable, "Totals")
                For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                    If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                        drTotalsRow(oDataCol.ColumnName.ToString) = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                    End If
                Next

            End Using
        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Overridable Sub GetMidAddPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' mid add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'MidAdd' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal + dFactor

                                    dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")
                                            If Not oCov.IsMarkedForDelete Then
                                                oCov.Factors.Add(oPremFactor)
                                            End If
                                            Exit For
                                        End If
                                    Next p
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using
        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetPostMultPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' post mult according to the rate order table and process in the order according to the rate order table
        'Get the factor value and multiply it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'PostMult' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dTotal = 0 Then dTotal = 1
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal * dFactor

                                    dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")
                                            If Not oCov.IsMarkedForDelete Then
                                                oCov.Factors.Add(oPremFactor)
                                            End If
                                            Exit For
                                        End If
                                    Next p
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using
        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetPostAddPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' post add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'PostAdd' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal + dFactor

                                    dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")
                                            If Not oCov.IsMarkedForDelete Then
                                                oCov.Factors.Add(oPremFactor)
                                            End If
                                            Exit For
                                        End If
                                    Next p
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using
        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetFeeAddPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' fee add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Double = 0
        Dim dTotal As Double = 0
        Dim dNewTotal As Double = 0
        Dim dPrevTotal As Double = 0

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'FeeAdd' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    For x As Integer = 0 To FactorTable.Rows.Count - 1
                        If oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewTotal = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If FactorTable.Rows(x).Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDbl(FactorTable.Rows(x).Item(y))
                                    dTotal = CDbl(FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y))
                                    If dNewTotal = 0 Then
                                        If dTotal = 1 Then dPrevTotal = 0
                                        dPrevTotal = dTotal
                                    Else
                                        dPrevTotal = dNewTotal
                                    End If
                                    dNewTotal = dTotal + dFactor
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal
                                End If
                            Next y
                            Exit For
                        End If
                    Next x
                Loop

            End Using
        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Overridable Function UpdateMidAddFactorBasedOnTerm(ByVal oPolicy As clsPolicyPPA, ByVal dFactorAmt As Decimal) As Decimal
        Return dFactorAmt
    End Function

    Public Overridable Sub UpdateMidAddFactorAmounts(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Decimal = 0
        Dim dTotal As Decimal = 0
        Dim dNewFactor As Decimal = 0
        Dim drMidAddRow As DataRow = Nothing
        Dim drTotalsRow As DataRow = Nothing

        Try

            drTotalsRow = GetRow(FactorTable, "Totals")

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'MidAdd' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    If oReader.Item("FactorName").ToString.ToUpper = "OTC2" Or oReader.Item("FactorName").ToString.ToUpper = "OTC1" Then
                        'don't update
                    Else
                        drMidAddRow = GetRow(FactorTable, oReader.Item("FactorName"))

                        If Not drMidAddRow Is Nothing Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewFactor = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If drMidAddRow.Item(y) IsNot System.DBNull.Value Then
                                    dFactor = CDec(drMidAddRow.Item(y))
                                    dTotal = RoundStandard(CDec(drTotalsRow.Item(y)), 0)
                                    dNewFactor = dTotal + dFactor
                                    drTotalsRow.Item(y) = RoundStandard(dNewFactor, 0)
                                End If
                            Next y
                        Else
                            'check to see if it is an endorsement
                            drMidAddRow = GetRow(FactorTable, oReader.Item("FactorName") & "-ENDORSE")

                            If Not drMidAddRow Is Nothing Then
                                For y As Integer = 1 To FactorTable.Columns.Count - 1
                                    dNewFactor = 0
                                    If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                        Exit For
                                    End If
                                    If drMidAddRow.Item(y) IsNot System.DBNull.Value Then
                                        dFactor = CDec(drMidAddRow.Item(y))
                                        dTotal = RoundStandard(CDec(drTotalsRow.Item(y)), 0)
                                        dNewFactor = dTotal + dFactor
                                        drTotalsRow.Item(y) = RoundStandard(dNewFactor, 0)
                                    End If
                                Next y
                            End If
                        End If
                        If Not drMidAddRow Is Nothing Then
                            drMidAddRow = Nothing
                        End If
                    End If
                Loop
            End Using

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub UpdateFeeAddFactorAmounts(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Decimal = 0
        Dim dTotal As Decimal = 0
        Dim dNewFactor As Decimal = 0
        Dim drFeeAddRow As DataRow = Nothing
        Dim drTotalsRow As DataRow = Nothing

        Try
            'this takes the factor amount for the feeadd factor and multiplies it by the premium amount
            drTotalsRow = GetRow(FactorTable, "Totals")

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND FactorType = 'FeeAdd' "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY RateOrder Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()

                    drFeeAddRow = GetRow(FactorTable, oReader.Item("FactorName"))

                    If Not drFeeAddRow Is Nothing Then
                        For y As Integer = 1 To FactorTable.Columns.Count - 1
                            dNewFactor = 0
                            If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                Exit For
                            End If
                            If drFeeAddRow.Item(y) IsNot System.DBNull.Value Then
                                dFactor = CDec(drFeeAddRow.Item(y))
                                dTotal = RoundStandard(CDec(drTotalsRow.Item(y)), 0)
                                dNewFactor = dTotal * dFactor
                                'no rounding
                                drFeeAddRow.Item(y) = dNewFactor
                            End If
                        Next y
                    End If
                    If Not drFeeAddRow Is Nothing Then
                        drFeeAddRow = Nothing
                    End If
                Loop
            End Using

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Sub

    Public Overridable Sub CheckMinPremAmounts(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)

        Dim dTotal As Decimal = 0
        Dim dTempTotal As Decimal = 0
        Dim drTotalsRow As DataRow = Nothing
        Dim bUpdatePrem As Boolean = False
        Dim dMinPremAmt As Decimal = 0

        Try
            'assume false
            oPolicy.MinPremApplied = False
            drTotalsRow = GetRow(FactorTable, "Totals")

            For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                If oDataCol.ColumnName.ToUpper = "FACTORTYPE" Then
                    Exit For
                End If
                If drTotalsRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                    If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                        dTempTotal = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                        If IsNumeric(dTempTotal) Then
                            drTotalsRow(oDataCol.ColumnName.ToString) = dTempTotal
                            dTotal += CDec(drTotalsRow(oDataCol.ColumnName.ToString))
                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            Throw
        Finally
            If Not drTotalsRow Is Nothing Then
                drTotalsRow = Nothing
            End If
        End Try
    End Sub


    Public Overridable Sub GetTotalChgInPremPolFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
                'initialize
                oPolicyFactor.FactorAmt = 0
                For Each oCov As clsPACoverage In oVeh.Coverages
                    If Not oCov.IsMarkedForDelete Then
                        For Each oPremFactor As clsPremiumFactor In oCov.Factors
                            If oPolicyFactor.FactorCode = oPremFactor.FactorCode Then
                                'update FactorAmt - total change in premium
                                oPolicyFactor.FactorAmt = RoundStandard(oPolicyFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                            End If
                        Next
                    End If
                Next
            Next

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
            If oDriver.IndexNum < 98 Then
                For Each oDriverFactor As clsBaseFactor In oDriver.Factors
                    'initialize
                    oDriverFactor.FactorAmt = 0
                    For Each oCov As clsPACoverage In oVeh.Coverages
                        If Not oCov.IsMarkedForDelete Then
                            For Each oPremFactor As clsPremiumFactor In oCov.Factors
                                If oDriverFactor.FactorCode = oPremFactor.FactorCode Then
                                    'update FactorAmt - total change in premium
                                    oDriverFactor.FactorAmt = RoundStandard(oDriverFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                                End If
                            Next
                        End If
                    Next
                Next
            End If

            For Each oVehFactor As clsBaseFactor In oVeh.Factors
                'initialize
                oVehFactor.FactorAmt = 0
                For Each oCov As clsPACoverage In oVeh.Coverages
                    If Not oCov.IsMarkedForDelete Then
                        For Each oPremFactor As clsPremiumFactor In oCov.Factors
                            If oVehFactor.FactorCode = oPremFactor.FactorCode Then
                                'update FactorAmt - total change in premium
                                oVehFactor.FactorAmt = RoundStandard(oVehFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                            End If
                        Next
                    End If
                Next
            Next

        Catch ex As Exception
            Throw
        End Try

    End Sub

    Public Overridable Function GetPremiums(ByRef oPolicy As clsPolicyPPA, ByVal drTotalsRow As DataRow) As Boolean

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            For Each oCov As clsPACoverage In oVeh.Coverages
                If Not oCov.IsMarkedForDelete Then
                    For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                        If oDataCol.ColumnName.ToUpper = oCov.CovGroup.ToUpper Then
                            oCov.FullTermPremium = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                        End If
                    Next
                End If
            Next

        Catch ex As Exception
            Throw
        Finally

        End Try

    End Function

    Protected Sub LogItems(ByVal oPolicy As clsPolicyPPA)

        Dim sLogging As String = ""

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            For Each oCov As clsPACoverage In oVeh.Coverages
                If Not oCov.IsMarkedForDelete Then
                    'log it
                    moLog = New ImperialFire.clsLogItem
                    moLog.Title = "COVERAGE INFO: "
                    sLogging = ""
                    sLogging += "CovGroup: " & oCov.CovGroup & vbCrLf
                    'sLogging += "CovAmount: " & oCov.CovAmount & vbCrLf
                    sLogging += "CovLimit: " & oCov.CovLimit & vbCrLf
                    sLogging += "FullTermPremium: " & oCov.FullTermPremium & vbCrLf
                    moLog.Description = sLogging
                    moLogging.LogItems.Add(moLog)
                    If Not moLog Is Nothing Then
                        moLog = Nothing
                    End If

                    For Each oPremFactor As clsPremiumFactor In oCov.Factors
                        'log it
                        moLog = New ImperialFire.clsLogItem
                        moLog.Title = "COVERAGE PREMFACTOR INFO: "
                        sLogging = ""
                        sLogging += "oPremFactor.FactorCode: " & oPremFactor.FactorCode & vbCrLf
                        sLogging += "oPremFactor.FactorName: " & oPremFactor.FactorName & vbCrLf
                        sLogging += "oPremFactor.FactorAmt - Premium Change Amount: " & oPremFactor.FactorAmt & vbCrLf
                        moLog.Description = sLogging
                        moLogging.LogItems.Add(moLog)
                        If Not moLog Is Nothing Then
                            moLog = Nothing
                        End If
                    Next
                End If
            Next

            For Each oFee As clsBaseFee In oPolicy.Fees
                'log it
                moLog = New ImperialFire.clsLogItem
                moLog.Title = "FEE INFO: "
                sLogging = ""
                sLogging += "FeeAmt: " & oFee.FeeAmt & vbCrLf
                sLogging += "FeeName: " & oFee.FeeName & vbCrLf
                sLogging += "FeeCode: " & oFee.FeeCode & vbCrLf
                sLogging += "FeeType: " & oFee.FeeType & vbCrLf
                sLogging += "IndexNum: " & oFee.IndexNum & vbCrLf
                moLog.Description = sLogging
                moLogging.LogItems.Add(moLog)
                If Not moLog Is Nothing Then
                    moLog = Nothing
                End If
            Next

            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                'log it
                moLog = New ImperialFire.clsLogItem
                moLog.Title = "POLICY FACTOR INFO: "
                sLogging = ""
                sLogging += "FactorCode: " & oFactor.FactorCode & vbCrLf
                sLogging += "FactorName: " & oFactor.FactorName & vbCrLf
                sLogging += "FactorDesc: " & oFactor.FactorDesc & vbCrLf
                sLogging += "FactorAmt: " & oFactor.FactorAmt & vbCrLf
                sLogging += "IndexNum: " & oFactor.IndexNum & vbCrLf
                moLog.Description = sLogging
                moLogging.LogItems.Add(moLog)
                If Not moLog Is Nothing Then
                    moLog = Nothing
                End If
            Next

        Catch ex As Exception
            Throw
        Finally

        End Try

    End Sub

    Public Overridable Sub CleanDataTable(ByVal oPolicy As clsPolicyPPA, ByVal oFactorTable As DataTable)

        Dim oCovs As Dictionary(Of String, String) = New Dictionary(Of String, String)
        Dim drTotalsRow As DataRow = Nothing
        Dim oProgs As New Dictionary(Of String, Dictionary(Of String, String))

        Try
            'remove any premium for covs that are not valid from the data table 

            oProgs.Add(oPolicy.Program, oCovs)

            GetCoverageList(oCovs, oProgs(oPolicy.Program), oPolicy.Program, oPolicy)

            oProgs.Remove(oPolicy.Program)
            oProgs.Add(oPolicy.Program, oCovs)

            drTotalsRow = GetRow(oFactorTable, "Totals")

            'use the covs on oProgs to determine if the total for the column needs to be updated
            For i As Integer = 1 To oFactorTable.Columns.Count - 1
                Dim oProg As Dictionary(Of String, String) = oProgs(oPolicy.Program)
                If oFactorTable.Columns.Item(i).ColumnName.ToUpper = "FACTORNAME" Or oFactorTable.Columns.Item(i).ColumnName.ToUpper = "FACTORTYPE" Or oFactorTable.Columns.Item(i).ColumnName.ToUpper = "FLATFACTOR" Then
                    'Don't mess with these columns
                Else
                    If Not ProgramContainsCov(oProg, oFactorTable.Columns.Item(i).ColumnName) Then
                        drTotalsRow(i) = 0
                    End If
                    If Not oProg Is Nothing Then
                        oProg = Nothing
                    End If
                End If
            Next i

        Catch ex As Exception
            Throw
        Finally

        End Try
    End Sub

    Private Sub GetCoverageList(ByRef oCovs As Dictionary(Of String, String), ByRef oProg As Dictionary(Of String, String), ByVal sProgram As String, ByVal oPolicy As clsPolicyPPA)

        Dim sSql As String = ""
        Dim lRow As Long = 0
        Dim sCoverage As String = ""
        Dim dtRateDate As Date = oPolicy.RateDate
        Dim sAppliesToCode As String = oPolicy.AppliesToCode
        Dim oReader As SqlDataReader = Nothing

        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorBaseRate with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = sProgram
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                oReader = cmd.ExecuteReader

                Do While oReader.Read()

                    sCoverage = (oReader.Item("Coverage"))
                    If Not ProgramContainsCov(oProg, sCoverage) Then
                        'add the cov
                        oCovs.Add(sCoverage, sCoverage)
                    End If
                Loop

            End Using

            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

        Catch ex As Exception
            Throw
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Private Function ProgramContainsCov(ByVal oProg As Dictionary(Of String, String), ByVal sCoverage As String) As Boolean
        Try
            Return Not oProg(sCoverage) Is Nothing
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetAssignedDriver(ByVal oPolicy As clsPolicyPPA) As clsEntityDriver

        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If oDriver.IndexNum = oVeh.AssignedDriverNum Then
                'this is our driver
                Return oDriver
                Exit For
            End If
        Next
        Return Nothing
    End Function

    Public Function GetRatedVehicle(ByVal oPolicy As clsPolicyPPA) As clsVehicleUnit

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If oVeh.IndexNum = RatedVehNum Then
                'this is our veh
                Return oVeh
                Exit For
            End If
        Next

        Return Nothing
    End Function

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

    Public Function FactorOnPolicy(ByVal oPolicy As clsPolicyPPA, ByVal sFactorCode As String) As Boolean

        For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
            If oFactor.FactorCode.ToString.ToUpper = sFactorCode.ToString.ToUpper Then
                Return True
            End If
        Next

        Return False
    End Function

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

    Public Overridable Function dbGetTerritory(ByVal oPolicy As clsPolicyPPA) As String
        Dim sZips As String = String.Empty

        For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
            If sZips = String.Empty Then
                sZips = "'" & oVeh.Zip & "'"
            Else
                sZips = sZips & ",'" & oVeh.Zip & "'"
            End If
        Next

        Dim DataRows() As DataRow
        Dim oCodeTerritoryDefinitionsTable As DataTable = Nothing
        Dim oCodeTerritoryDefinitionsDataSet As DataSet = LoadCodeTerritoryDefinitionsTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode, sZips)
        oCodeTerritoryDefinitionsTable = oCodeTerritoryDefinitionsDataSet.Tables(0)

        Try
            For v As Integer = 0 To oPolicy.VehicleUnits.Count - 1
                If Not oPolicy.VehicleUnits(v).IsMarkedForDelete Then
                    If Not oPolicy.VehicleUnits(v).Zip Is Nothing Then
                        If oPolicy.VehicleUnits(v).Zip <> "" Then
                            DataRows = oCodeTerritoryDefinitionsTable.Select("Program = '" & oPolicy.Program & "'" & " AND Zip = '" & oPolicy.VehicleUnits(v).Zip & "'")

                            For Each oRow As DataRow In DataRows
                                For c As Integer = 0 To oPolicy.VehicleUnits(v).Coverages.Count - 1
                                    If Not oPolicy.VehicleUnits(v).Coverages(c).IsMarkedForDelete Then
                                        If oPolicy.VehicleUnits(v).Coverages(c).CovGroup.ToUpper = oRow("Coverage").ToString.ToUpper Then
                                            oPolicy.VehicleUnits(v).Coverages(c).Territory = oRow("Territory").ToString
                                            oPolicy.VehicleUnits(v).Territory = oRow("Territory").ToString
                                            Exit For
                                        End If
                                    End If
                                Next
                            Next
                        End If
                    End If
                End If
            Next

            Return ""

        Catch ex As Exception
            Throw
        Finally

        End Try

    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadCodeTerritoryDefinitionsTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String, ByVal sZips As String) As DataSet
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
        Dim oDS As New DataSet

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, Coverage, Zip, County, City, State, Territory, Region, Disabled "
                sSql = sSql & " FROM pgm" & sProduct & sStateCode & "..CodeTerritoryDefinitions with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Zip IN (" & sZips & ") "
                sSql = sSql & " ORDER BY Program, Zip, Coverage "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "CodeTerritoryDefinitions")

                Return oDS

            End Using

        Catch ex As Exception
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Shared Function LoadEditCodeTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
        Dim oDS As New DataSet

        Try

            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, Category, SubCategory, EditCode, EditValue, EditDesc "
                sSql = sSql & " FROM pgm" & sProduct & sStateCode & "..EditCode with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Program, Category, SubCategory "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "EditCode")

                Return oDS

            End Using

        Catch ex As Exception
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadCodeViolCodesTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
        Dim oDS As New DataSet

        Try
            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, ViolGroup, ViolCode, Description "
                sSql = sSql & " FROM pgm" & sProduct & sStateCode & "..CodeViolCodes with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Program, ViolGroup, ViolCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "CodeViolCodes")

                Return oDS

            End Using

        Catch ex As Exception
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadPayPlanTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
        Dim oDS As New DataSet

        Try
            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Program, PayPlanCode, Name, DownPayPct, NumInstallments, InstallmentType, UsePremWFeesInCalc "
                sSql = sSql & " FROM pgm" & sProduct & sStateCode & "..PayPlan with(nolock)"
                sSql = sSql & " WHERE EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Program, PayPlanCode, DownPayPct "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "PayPlan")

                Return oDS

            End Using

        Catch ex As Exception
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Overridable Function PolicyConversion(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA

        Dim oConvertedPolicy As New clsPolicyPPA

        'convert coverages (including limits)
        oConvertedPolicy = ConvertCoverages(oPolicy, sRatedProgram)
        'If oConvertedPolicy.ConversionSuccessful Then
        'convert driver factors
        oConvertedPolicy = ConvertDriverFactors(oConvertedPolicy, sRatedProgram)

        'convert vehicle factors
        oConvertedPolicy = ConvertVehicleFactors(oConvertedPolicy, sRatedProgram)

        'convert policy factors
        oConvertedPolicy = ConvertPolicyFactors(oConvertedPolicy, sRatedProgram)

        'convert violations
        oConvertedPolicy = ConvertViolations(oConvertedPolicy, sRatedProgram)

        'convert pay plan
        oConvertedPolicy = ConvertPayPlan(oConvertedPolicy, sRatedProgram)

        If sRatedProgram.ToUpper = "MONTHLY" Then
            oConvertedPolicy.IsEFT = False
        End If

        'convert program
        oConvertedPolicy.Program = sRatedProgram
        oConvertedPolicy.ProgramType = sRatedProgram
        oConvertedPolicy.FormType = sRatedProgram

        'Else
        ''the programs don't have the same coverages for what is selected
        'End If

        If Not oConvertedPolicy.ConversionSuccessful Then oConvertedPolicy.FullTermPremium = 0

        Return oConvertedPolicy
    End Function

    Public Overridable Function ConvertCoverages(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oEditCodeTable As DataTable = Nothing
        Dim bKeepCoverage As Boolean = False
        'Dim bConversionSuccessful = True

        Dim oEditCodeDataSet As DataSet = LoadEditCodeTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        oEditCodeTable = oEditCodeDataSet.Tables(0)

        'get coverages and limits for newly selected program
        DataRows = oEditCodeTable.Select("Program='" & sRatedProgram & "'")

        For Each oVeh As clsVehicleUnit In oConvertedPolicy.VehicleUnits
            For i As Integer = 0 To oVeh.Coverages.Count - 1
                If i = oVeh.Coverages.Count Then Exit For
                'assume we will be removing this guy
                bKeepCoverage = False
                'look to see if the coverage that is on the policy is allowed in this program
                For Each oRow As DataRow In DataRows
                    If oVeh.Coverages.Item(i).CovCode.ToUpper = oRow("EditValue").ToString.ToUpper Then
                        'it is ok, we don't have to remove this guy
                        bKeepCoverage = True
                        Exit For
                    End If
                Next
                If Not bKeepCoverage Then
                    oConvertedPolicy.ConversionSuccessful = False
                    ''Return oConvertedPolicy 'return the policy without removing anything
                    'this coverage is not allowed on this program, so let's get rid of him
                    oVeh.Coverages.Remove(oVeh.Coverages.Item(i))
                    'restart loop
                    i = -1
                End If
            Next
        Next

        'all is well, it has all of the same coverages
        Return oConvertedPolicy

    End Function

    Public Overridable Function ConvertDriverFactors(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oFactorDriverTable As DataTable = Nothing
        Dim bKeepFactor As Boolean = False
        'Dim bConversionSuccessful = True

        Dim oFactorDriverDataSet As DataSet = LoadFactorDriverTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        oFactorDriverTable = oFactorDriverDataSet.Tables(0)

        'get factors for newly selected program
        DataRows = oFactorDriverTable.Select("Program='" & sRatedProgram & "'")

        For Each oDrv As clsEntityDriver In oConvertedPolicy.Drivers
            For i As Integer = 0 To oDrv.Factors.Count - 1
                If i = oDrv.Factors.Count Then Exit For
                'assume we will be removing this guy
                bKeepFactor = False
                'look to see if the factor that is on the driver is allowed in this program
                For Each oRow As DataRow In DataRows
                    If oDrv.Factors.Item(i).FactorCode.ToUpper = oRow("FactorCode").ToString.ToUpper Then
                        'We need to remove the SR22 factor non-ACTIVE drivers 
                        'if it was previously added in error.
                        If oDrv.Factors.Item(i).FactorCode.ToUpper = "SR22" And _
                            oDrv.SR22 And oDrv.DriverStatus.ToUpper <> "ACTIVE" Then
                            If FactorOnDriver(oDrv, "SR22") Then
                                bKeepFactor = False
                                Exit For
                            End If
                        Else
                            'it is ok, we don't have to remove this guy
                            bKeepFactor = True
                            Exit For
                        End If
                    End If
                Next
                If Not bKeepFactor Then
                    'this factor is not allowed on this program, so let's get rid of him
                    oDrv.Factors.Remove(oDrv.Factors.Item(i))
                    'restart loop
                    i = -1
                End If
            Next

            'all is well, so now let's see if we need to add any factors that weren't on the previous program
            'Do not add SR22 factor unless the driver is active.
            If oDrv.SR22 And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                If Not FactorOnDriver(oDrv, "SR22") Then
                    AddDriverFactor(oConvertedPolicy, oDrv, "SR22")
                End If
            End If
            If oDrv.Military Then
                If Not FactorOnDriver(oDrv, "MILITARY") Then
                    AddDriverFactor(oConvertedPolicy, oDrv, "MILITARY")
                End If
            End If
        Next

        Return oConvertedPolicy

    End Function

    Public Overridable Function ConvertVehicleFactors(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oFactorVehicleTable As DataTable = Nothing
        Dim bKeepFactor As Boolean = False
        'Dim bConversionSuccessful = True

        Dim oFactorVehicleDataSet As DataSet = LoadFactorVehicleTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        oFactorVehicleTable = oFactorVehicleDataSet.Tables(0)

        'get factors for newly selected program
        DataRows = oFactorVehicleTable.Select("Program='" & sRatedProgram & "'")

        For Each oVeh As clsVehicleUnit In oConvertedPolicy.VehicleUnits
            For i As Integer = 0 To oVeh.Factors.Count - 1
                If i = oVeh.Factors.Count Then Exit For
                'assume we will be removing this guy
                bKeepFactor = False
                'look to see if the factor that is on the vehicle is allowed in this program
                For Each oRow As DataRow In DataRows
                    If oVeh.Factors.Item(i).FactorCode.ToUpper = oRow("FactorCode").ToString.ToUpper Then
                        'it is ok, we don't have to remove this guy
                        bKeepFactor = True
                        Exit For
                    End If
                Next
                If Not bKeepFactor Then
                    'this factor is not allowed on this program, so let's get rid of him
                    oVeh.Factors.Remove(oVeh.Factors.Item(i))
                    'restart loop
                    i = -1
                End If
            Next

            'all is well, so now let's see if we need to add any factors that weren't on the previous program

        Next

        Return oConvertedPolicy

    End Function

    Public Overridable Function ConvertPolicyFactors(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oFactorPolicyTable As DataTable = Nothing
        Dim bKeepFactor As Boolean = False
        'Dim bConversionSuccessful = True

        Dim oFactorPolicyDataSet As DataSet = LoadFactorPolicyTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        oFactorPolicyTable = oFactorPolicyDataSet.Tables(0)

        'get factors for newly selected program
        DataRows = oFactorPolicyTable.Select("Program='" & sRatedProgram & "'")

        For i As Integer = 0 To oConvertedPolicy.PolicyFactors.Count - 1
            If i = oConvertedPolicy.PolicyFactors.Count Then Exit For
            'assume we will be removing this guy
            bKeepFactor = False
            'look to see if the factor that is on the policy is allowed in this program
            For Each oRow As DataRow In DataRows
                If oConvertedPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = oRow("FactorCode").ToString.ToUpper Then
                    'it is ok, we don't have to remove this guy
                    bKeepFactor = True
                    Exit For
                End If
            Next
            If Not bKeepFactor Then
                'this factor is not allowed on this program, so let's get rid of him
                oConvertedPolicy.PolicyFactors.Remove(oConvertedPolicy.PolicyFactors.Item(i))
                'restart loop
                i = -1
            End If
        Next

        'all is well, so now let's see if we need to add any factors that weren't on the previous program
        'all of the current policy factors are auto apply so there ain't none

        Return oConvertedPolicy

    End Function

    Public Overridable Function ConvertViolations(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oCodeViolCodesTable As DataTable = Nothing
        Dim oCodeXRefTable As DataTable = Nothing
        'Dim bConversionSuccessful = True

        Dim bConvertMonthly As Boolean = True
        If StateInfoContains("NOCONVERT", "MONTHLY", "VIOLATION", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
            bConvertMonthly = False
        End If

        If bConvertMonthly AndAlso (sRatedProgram.ToUpper = "MONTHLY" And oPolicy.Program.ToUpper <> "MONTHLY") Then 'switching to Monthly
            Dim oCodeXRefDataSet As DataSet = LoadCodeXRefTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

            oCodeXRefTable = oCodeXRefDataSet.Tables(0)

            'get viol code mappings
            DataRows = oCodeXRefTable.Select("Source='WEBRATER' AND CodeType='VIOLMAP'")

            For Each oDrv As clsEntityDriver In oConvertedPolicy.Drivers
                For Each oViol As clsBaseViolation In oDrv.Violations
                    For Each oRow As DataRow In DataRows
                        If oViol.ViolTypeCode = oRow("Code").ToString Then
                            'we found it now reset the viol
                            oViol.ViolGroup = oRow("MappingCode3").ToString
                            oViol.ViolTypeCode = oRow("MappingCode2").ToString
                            Exit For
                        End If
                    Next
                Next
            Next
        ElseIf bConvertMonthly AndAlso (sRatedProgram.ToUpper <> "MONTHLY" And oPolicy.Program.ToUpper = "MONTHLY") Then 'switching from Monthly
            Dim oCodeXRefDataSet As DataSet = LoadCodeXRefTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

            oCodeXRefTable = oCodeXRefDataSet.Tables(0)

            'get viol code mappings
            DataRows = oCodeXRefTable.Select("Source='WEBRATER' AND CodeType='VIOLMAP'")

            For Each oDrv As clsEntityDriver In oConvertedPolicy.Drivers
                For Each oViol As clsBaseViolation In oDrv.Violations
                    For Each oRow As DataRow In DataRows
                        If oViol.ViolTypeCode = oRow("MappingCode2").ToString Then
                            'we found it now reset the viol
                            oViol.ViolGroup = oRow("MappingCode1").ToString
                            oViol.ViolTypeCode = oRow("Code").ToString
                            Exit For
                        End If
                    Next
                Next
            Next
        Else 'Monthly is not involved
            Dim oCodeViolCodesDataSet As DataSet = LoadCodeViolCodesTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

            oCodeViolCodesTable = oCodeViolCodesDataSet.Tables(0)

            'get viol code mappings for newly selected program
            DataRows = oCodeViolCodesTable.Select("Program='" & sRatedProgram & "'")

            For Each oDrv As clsEntityDriver In oConvertedPolicy.Drivers
                For Each oViol As clsBaseViolation In oDrv.Violations
                    For Each oRow As DataRow In DataRows
                        If oViol.ViolTypeCode.Trim = oRow("ViolCode").ToString.Trim Then
                            'we found it now reset the violgroup
                            oViol.ViolGroup = oRow("ViolGroup").ToString
                            Exit For
                        End If
                    Next
                Next
            Next
        End If

        Return oConvertedPolicy

    End Function

    Public Overridable Function ConvertPayPlan(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oPayPlanTable As DataTable = Nothing
        Dim bConversionSuccessful = False

        Dim oPayPlanDataSet As DataSet = LoadPayPlanTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

        oPayPlanTable = oPayPlanDataSet.Tables(0)

        'get pay plans for newly selected program
        DataRows = oPayPlanTable.Select("Program='" & sRatedProgram & "'")

        For Each oRow As DataRow In DataRows
            If oConvertedPolicy.PayPlanCode = oRow("PayPlanCode").ToString Then
                'we found it now so we're good
                bConversionSuccessful = True
                Exit For
            End If
        Next

        If Not bConversionSuccessful Then
            'set it to the first one
            For Each oRow As DataRow In DataRows
                oConvertedPolicy.PayPlanCode = oRow("PayPlanCode").ToString
                Exit For
            Next
        End If

        Return oConvertedPolicy
    End Function

    Public Function MaxDiscountAmount(ByVal oPolicy As clsPolicyPPA, ByVal sCov As String) As Decimal

        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        Dim dMaxDiscountAmt As Decimal = 99
        oStateInfoTable = moStateInfoDataSet.Tables(0)

        DataRows = oStateInfoTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup='MAXDISCOUNT' AND ItemCode='PERCENT' AND ItemSubCode='" & sCov & "' ")

        For Each oRow As DataRow In DataRows
            dMaxDiscountAmt = CDec(oRow.Item("ItemValue").ToString)
        Next

        Return dMaxDiscountAmt

    End Function

    Public Overridable Function CalculateCovFactor(ByVal sCovGroup As String, ByVal sCovDed As String, ByVal oPolicy As clsPolicyPPA) As Decimal

        Dim dCovFactor As Decimal = 0
        Select Case sCovGroup.ToUpper
            Case "OTC"
                If sCovDed.Contains("/") Then
                    sCovDed = sCovDed.Split("/")(0)
                End If

                Dim oColCov As clsPACoverage = GetCoverage("COL", oPolicy)
                Dim sColCovDed As String = oColCov.CovDeductible
                If sColCovDed.Contains("/") Then
                    sColCovDed = sColCovDed.Split("/")(1)
                End If

                If Not oColCov Is Nothing Then
                    Select Case sCovDed
                        Case "100"
                            Select Case sColCovDed
                                Case "250"
                                    dCovFactor = 1
                            End Select
                        Case "150"
                            Select Case sColCovDed
                                Case "250"
                                    dCovFactor = 1
                            End Select
                        Case "250"
                            Select Case sColCovDed
                                Case "250"
                                    dCovFactor = 1
                                Case "500"
                                    dCovFactor = 0.88
                            End Select
                        Case "500"
                            Select Case sColCovDed
                                Case "500"
                                    dCovFactor = 0.8
                                Case "1000"
                                    dCovFactor = 0.75
                            End Select
                    End Select
                Else
                    'got a problem
                    Return 0
                End If
            Case "COL"
                If sCovDed.Contains("/") Then
                    sCovDed = sCovDed.Split("/")(1)
                End If

                Dim oOtcCov As clsPACoverage = GetCoverage("OTC", oPolicy)
                Dim sOTCCovDed As String = oOtcCov.CovDeductible
                If sOTCCovDed.Contains("/") Then
                    sOTCCovDed = sOTCCovDed.Split("/")(0)
                End If

                If Not oOtcCov Is Nothing Then
                    Select Case sCovDed
                        Case "250"
                            Select Case sOTCCovDed
                                Case "100"
                                    dCovFactor = 1
                                Case "150"
                                    dCovFactor = 1
                                Case "250"
                                    dCovFactor = 1
                            End Select
                        Case "500"
                            Select Case sOTCCovDed
                                Case "250"
                                    dCovFactor = 0.88
                                Case "500"
                                    dCovFactor = 0.8
                            End Select
                        Case "1000"
                            Select Case sOTCCovDed
                                Case "500"
                                    dCovFactor = 0.75
                            End Select
                    End Select
                Else
                    'got a problem
                    Return 0
                End If
        End Select

        Return dCovFactor

    End Function

    Public Function GetCoverage(ByVal sCovGroup As String, ByVal oPolicy As clsPolicyPPA) As clsPACoverage

        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
        Dim oReturnedCov As clsPACoverage = Nothing

        For Each oCov As clsPACoverage In oVeh.Coverages
            If Not oCov.IsMarkedForDelete Then
                If oCov.CovGroup.ToUpper = sCovGroup.ToUpper Then
                    oReturnedCov = oCov
                    Exit For
                End If
            End If
        Next

        Return oReturnedCov
    End Function

    Public Overridable Function CalculateUUMPDMonthlyBaseRate(ByVal oPolicy As clsPolicyPPA) As Decimal

        Dim dCovFactor As Decimal = 0
        Dim dOTCPremium As Decimal = 0
        Dim dCOLPremium As Decimal = 0
        Dim dOTCBaseRate As Decimal = GetMonthlyBaseRateFactor(oPolicy, "OTC") '7
        Dim dCOLBaseRate As Decimal = GetMonthlyBaseRateFactor(oPolicy, "COL") '26
        Dim dMonthlyOTCTerritoryFactor As Decimal = GetMonthlyTerritoryFactor(oPolicy, "OTC")
        Dim dMonthlyCOLTerritoryFactor As Decimal = GetMonthlyTerritoryFactor(oPolicy, "COL")
        Dim dMonthlyOTCDriverClassFactor As Decimal = GetMonthlyDriverClassFactor(oPolicy, "OTC")
        Dim dMonthlyCOLDriverClassFactor As Decimal = GetMonthlyDriverClassFactor(oPolicy, "COL")
        Dim dMonthlyOTCDriverPointsFactor As Decimal = GetMonthlyDriverPointsFactor(oPolicy, "OTC")
        Dim dMonthlyCOLDriverPointsFactor As Decimal = GetMonthlyDriverPointsFactor(oPolicy, "COL")
        Dim dMonthlyOTCCoverageFactor As Decimal = 1
        Dim dMonthlyOTCSymbolFactor As Decimal = GetMonthlySymbolFactor(oPolicy, "OTC")
        Dim dMonthlyCOLSymbolFactor As Decimal = GetMonthlySymbolFactor(oPolicy, "COL")
        Dim dMonthlyOTCModelYearFactor As Decimal = GetMonthlyModelYearFactor(oPolicy, "OTC")
        Dim dMonthlyCOLModelYearFactor As Decimal = GetMonthlyModelYearFactor(oPolicy, "COL")
        Dim dOTCFactor As Decimal = -2
        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing

        'calculate OTC premium
        dOTCPremium = RoundStandard((dOTCBaseRate * dMonthlyOTCTerritoryFactor * dMonthlyOTCDriverClassFactor * dMonthlyOTCDriverPointsFactor * dMonthlyOTCCoverageFactor * dMonthlyOTCSymbolFactor * dMonthlyOTCModelYearFactor) + dOTCFactor, 2)

        'calculate COL premium
        dCOLPremium = RoundStandard((dCOLBaseRate * dMonthlyCOLTerritoryFactor * dMonthlyCOLDriverClassFactor * dMonthlyCOLDriverPointsFactor * dMonthlyOTCCoverageFactor * dMonthlyCOLSymbolFactor * dMonthlyCOLModelYearFactor), 2)

        Dim dCovFactorPercentage As Decimal = 0.1
        oStateInfoTable = moStateInfoDataSet.Tables(0)
        DataRows = oStateInfoTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup='MONTHLY' AND ItemCode='UMPD' AND ItemSubCode='PERCENTAGE' ")
        For Each oRow As DataRow In DataRows
            dCovFactorPercentage = CDec(oRow.Item("ItemValue").ToString())
        Next

        dCovFactor = (dOTCPremium + dCOLPremium) * dCovFactorPercentage

        If dCovFactor < 6.0 Then
            dCovFactor = 6.0
        End If

        Return dCovFactor

    End Function

    Public Overridable Function GetVehicleCountType(ByVal oPolicy As clsPolicyPPA) As String
        Dim sVehicleCountType As String = String.Empty
        Dim iVehicleCount As Integer = 0

        For Each oVehicle As clsVehicleUnit In oPolicy.VehicleUnits
            If Not oVehicle.IsMarkedForDelete Then
                iVehicleCount += 1
            End If
        Next

        If iVehicleCount > 1 Then
            sVehicleCountType = "M"
        Else
            sVehicleCountType = "S"
        End If

        Return sVehicleCountType
    End Function


    Public Function GetMonthlyBaseRateFactor(ByVal oPolicy As clsPolicyPPA, ByVal sCoverage As String) As Decimal

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dMonthlyBaseRateFactor As Decimal = 0
        Dim oCov As clsPACoverage = GetCoverage("UUMPD", oPolicy)


        Using cmd As New SqlCommand(sSql, moConn)

            sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorBaseRate with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= " AND EffDate <= @RateDate "
            sSql &= " AND ExpDate > @RateDate "
            sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
            sSql &= " AND Coverage = @Coverage "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 5).Value = sCoverage
            oReader = cmd.ExecuteReader

            Do While oReader.Read()
                dMonthlyBaseRateFactor = oReader.Item("Factor")
                Exit Do
            Loop
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Using

        Return dMonthlyBaseRateFactor

    End Function

    Public Function GetMonthlyTerritoryFactor(ByVal oPolicy As clsPolicyPPA, ByVal sCoverage As String) As Decimal

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dMonthlyOTCTerritoryFactor As Decimal = 0
        Dim oCov As clsPACoverage = GetCoverage("UUMPD", oPolicy)


        Using cmd As New SqlCommand(sSql, moConn)

            sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorTerritory with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= " AND EffDate <= @RateDate "
            sSql &= " AND ExpDate > @RateDate "
            sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
            sSql &= " AND Coverage = @Coverage "
            sSql &= " AND Territory = @Territory "
            sSql &= " AND VehicleCountType IN ('A', @VehicleCountType ) "
            sSql &= " ORDER BY Coverage Asc "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 5).Value = sCoverage
            cmd.Parameters.Add("@Territory", SqlDbType.VarChar, 5).Value = oCov.Territory
            cmd.Parameters.Add("@VehicleCountType", SqlDbType.VarChar, 1).Value = GetVehicleCountType(oPolicy)
            oReader = cmd.ExecuteReader

            Do While oReader.Read()
                dMonthlyOTCTerritoryFactor = oReader.Item("Factor")
                Exit Do
            Loop
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Using

        Return dMonthlyOTCTerritoryFactor

    End Function

    Public Function GetMonthlyDriverClassFactor(ByVal oPolicy As clsPolicyPPA, ByVal sCoverage As String) As Decimal

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dMonthlyOTCDriverClassFactor As Decimal = 0
        Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)


        Dim iDriverAge As Integer = 0
        iDriverAge = oDriver.Age
        'If oPolicy.Program.ToUpper = "SUMMIT" Or oPolicy.Program.ToUpper = "MONTHLY" Or (oPolicy.StateCode = 35 And oPolicy.Program.ToUpper = "CLASSIC") Then
        If UseDriverAgeBumping(oPolicy) Then
            'Drivers <= age 24 – if the DOB is within 30 days after inception, use the higher age for Driver Class factors.  This rule only applies to calculating age for driver class.  Do not use this rule for determining PNI Youthful or any other factor
            If oDriver.Age <= 24 And oDriver.DOB > "01/01/1900" Then

                Dim dtNextBDay As Date
                Try
                    dtNextBDay = oDriver.DOB.Month & "/" & oDriver.DOB.Day & "/" & oPolicy.EffDate.Year
                Catch ex As Exception
                    ' Catches leap year issue
                    If oDriver.DOB.Month = 2 And oDriver.DOB.Day = 29 Then
                        dtNextBDay = "3/1/" & oPolicy.EffDate.Year
                    End If
                End Try


                If dtNextBDay < oPolicy.EffDate Then

                    Try
                        dtNextBDay = oDriver.DOB.Month & "/" & oDriver.DOB.Day & "/" & oPolicy.EffDate.Year + 1
                    Catch ex As Exception
                        ' Catches leap year issue
                        If oDriver.DOB.Month = 2 And oDriver.DOB.Day = 29 Then
                            dtNextBDay = "3/1/" & oPolicy.EffDate.Year + 1
                        End If
                    End Try


                End If

                If DateDiff(DateInterval.Day, oPolicy.EffDate, dtNextBDay) < 30 AndAlso DateDiff(DateInterval.Day, oPolicy.EffDate, dtNextBDay) > 0 Then
                    iDriverAge = oDriver.Age + 1
                End If
            End If
        End If



        Dim sDriverClass As String = ""
        'DriverClass = Marital Staus + Gender + Age
        If StateInfoContains("RATE", "WIDOW", "MARRIED", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
            sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED" Or oDriver.MaritalStatus.Trim.ToUpper = "WIDOWED", "M", "S")
        Else
            sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED", "M", "S")
        End If
        sDriverClass &= IIf(oDriver.Gender.Trim.ToUpper.StartsWith("M"), "M", "F")
        sDriverClass &= iDriverAge

        Using cmd As New SqlCommand(sSql, moConn)

            sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverClass with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= " AND EffDate <= @RateDate "
            sSql &= " AND ExpDate > @RateDate "
            sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
            sSql &= " AND DriverClass = @DriverClass "
            sSql &= " AND Coverage = @Coverage "
            sSql &= " ORDER BY Coverage Asc "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@DriverClass", SqlDbType.VarChar, 8).Value = sDriverClass
            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 5).Value = sCoverage

            oReader = cmd.ExecuteReader

            Do While oReader.Read()
                dMonthlyOTCDriverClassFactor = oReader.Item("Factor")
                Exit Do
            Loop

        End Using
        If Not oReader Is Nothing Then
            oReader.Close()
            oReader = Nothing
        End If

        Return dMonthlyOTCDriverClassFactor

    End Function

    Public Function GetMonthlyOTCPolicyFactor(ByVal oPolicy As clsPolicyPPA) As Decimal
    End Function

    Public Function GetMonthlyOTCDriverFactor(ByVal oPolicy As clsPolicyPPA) As Decimal
    End Function

    Public Function GetMonthlyOTCVehicleFactor(ByVal oPolicy As clsPolicyPPA) As Decimal
    End Function

    Public Function GetMonthlyDriverPointsFactor(ByVal oPolicy As clsPolicyPPA, ByVal sCoverage As String) As Decimal
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dMonthlyOTCDriverPointsFactor As Decimal = 0

        Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)

        Using cmd As New SqlCommand(sSql, moConn)

            sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDriverPoints with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= " AND EffDate <= @RateDate "
            sSql &= " AND ExpDate > @RateDate "
            sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
            sSql &= " AND Points = @Points "
            sSql &= " AND Coverage = @Coverage "
            sSql &= " ORDER BY Coverage Asc "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@Points", SqlDbType.VarChar, 2).Value = oDriver.Points
            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 5).Value = sCoverage
            oReader = cmd.ExecuteReader

            Do While oReader.Read()
                dMonthlyOTCDriverPointsFactor = oReader.Item("Factor")
                Exit Do
            Loop

        End Using
        If Not oReader Is Nothing Then
            oReader.Close()
            oReader = Nothing
        End If

        Return dMonthlyOTCDriverPointsFactor

    End Function

    Public Function GetMonthlySymbolFactor(ByVal oPolicy As clsPolicyPPA, ByVal sCoverage As String) As Decimal
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dMonthlyOTCSymbolFactor As Decimal = 0
        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

        'get the symbol for this coverage
        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        Dim sSymbolGroup As String = ""
        Dim sSymbol As String = ""
        oStateInfoTable = moStateInfoDataSet.Tables(0)

        DataRows = oStateInfoTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup='COVERAGE' AND ItemCode='SYMBOL' AND ItemSubCode='OTC' ")

        For Each oRow As DataRow In DataRows
            'sSymbolGroup should be either LIA or PIP
            sSymbolGroup = oRow.Item("ItemValue").ToString
        Next
        Select Case sSymbolGroup
            Case "LIA"
                sSymbol = oVeh.LiabilitySymbolCode.Trim
            Case "PIP"
                sSymbol = oVeh.PIPMedLiabilityCode.Trim
            Case "VEH"
                sSymbol = oVeh.VehicleSymbolCode.Trim
        End Select

        Using cmd As New SqlCommand(sSql, moConn)

            sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorSymbol with(nolock)"
            sSql = sSql & " WHERE Program = @Program "
            sSql = sSql & " AND EffDate <= @RateDate "
            sSql = sSql & " AND ExpDate > @RateDate "
            sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
            sSql = sSql & " AND Coverage = @Coverage "
            sSql = sSql & " AND Symbol = @Symbol "
            sSql = sSql & " AND MinVehYear <= @VehYear "
            sSql = sSql & " AND MaxVehYear >= @VehYear "
            sSql = sSql & " ORDER BY Coverage Asc "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = sCoverage

            Dim sVehicleYear As String = oVeh.VehicleYear
            If oVeh.VehicleYear.Trim = "1" Then
                sVehicleYear = "1900"
            End If

            cmd.Parameters.Add("@VehYear", SqlDbType.VarChar, 5).Value = sVehicleYear
            cmd.Parameters.Add("@Symbol", SqlDbType.VarChar, 4).Value = sSymbol

            oReader = cmd.ExecuteReader

            Do While oReader.Read()
                dMonthlyOTCSymbolFactor = oReader.Item("Factor")
                Exit Do
            Loop

        End Using

        If Not oReader Is Nothing Then
            oReader.Close()
            oReader = Nothing
        End If

        Return dMonthlyOTCSymbolFactor

    End Function

    Public Function GetMonthlyModelYearFactor(ByVal oPolicy As clsPolicyPPA, ByVal sCoverage As String) As Decimal
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dMonthlyOTCModelYearFactor As Decimal = 0

        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

        Using cmd As New SqlCommand(sSql, moConn)

            sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorModelYear with(nolock)"
            sSql &= " WHERE Program = @Program "
            sSql &= " AND EffDate <= @RateDate "
            sSql &= " AND ExpDate > @RateDate "
            sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
            sSql &= " AND ModelYear = @ModelYear "
            sSql &= " AND Coverage = @Coverage "
            sSql &= " AND VehicleCountType IN ('A', @VehicleCountType ) "
            sSql &= " ORDER BY Coverage Asc "

            'Execute the query
            cmd.CommandText = sSql

            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
            cmd.Parameters.Add("@ModelYear", SqlDbType.VarChar, 4).Value = oVeh.VehicleAge
            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 5).Value = sCoverage
            cmd.Parameters.Add("@VehicleCountType", SqlDbType.VarChar, 1).Value = GetVehicleCountType(oPolicy)
            oReader = cmd.ExecuteReader

            Do While oReader.Read()
                dMonthlyOTCModelYearFactor = oReader.Item("Factor")
                Exit Do
            Loop

        End Using
        If Not oReader Is Nothing Then
            oReader.Close()
            oReader = Nothing
        End If

        Return dMonthlyOTCModelYearFactor

    End Function

    <WebMethod(EnableSession:=True, CacheDuration:=30000)> _
    Public Function LoadCodeXRefTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
        Dim oDS As New DataSet

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Source, CodeType, Code, MappingCode1, MappingCode2, MappingCode3 FROM pgm" & sProduct & sStateCode & ".." & "CodeXRef with(nolock)"
                sSql &= " ORDER BY Source, CodeType, Code "

                'Execute the query
                cmd.CommandText = sSql
                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)
                adapter.Fill(oDS, "CodeXRef")
                Return oDS
            End Using

        Catch ex As Exception
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function

    Public Sub GetCappedFactors(ByVal oPolicy As clsPolicyPPA)

        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        oStateInfoTable = moStateInfoDataSet.Tables(0)

        DataRows = oStateInfoTable.Select("Program IN ('PPA', '" & oPolicy.Program & "') AND ItemGroup='MAXDISCOUNT' AND ItemCode='FACTOR' ")
        Dim i As Integer = 0
        For Each oRow As DataRow In DataRows
            ReDim Preserve msCappedFactors(i)
            msCappedFactors(i) = oRow.Item("ItemValue").ToString
            i += 1
        Next


    End Sub

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

    Protected Function SetPCRelationship(ByVal oPolicy As clsPolicyPPA) As Boolean

        Dim bPCRelationship As Boolean = False

        For Each oDrv As clsEntityDriver In oPolicy.Drivers
            If oDrv.DriverStatus.ToUpper = "ACTIVE" And Not oDrv.IsMarkedForDelete Then
                If oDrv.RelationToInsured.ToUpper = "CHILD" Or oDrv.RelationToInsured.ToUpper = "PARENT" Then
                    bPCRelationship = True
                    Exit For
                End If
            End If
        Next
        Return bPCRelationship
    End Function

    Public Sub RentToOwnCheck(ByVal oPolicy As clsPolicyPPA)

        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)
        Dim bHasRTO As Boolean = False
        For Each oFactor As clsBaseFactor In oVeh.Factors
            If oFactor.FactorCode.ToUpper = "RENT_TO_OWN" Then
                bHasRTO = True
                'add note
                oPolicy.Notes = (AddNote(oPolicy.Notes, oFactor.FactorCode & "-" & oVeh.IndexNum, oFactor.FactorCode & "-" & oVeh.IndexNum, "RTO", oPolicy.Notes.Count))
                Exit For
            End If
        Next
        If Not bHasRTO Then
            'remove note
            oPolicy.Notes = RemoveNotes(oPolicy.Notes, "RTO", "RENT_TO_OWN" & "-" & oVeh.IndexNum)
        End If
    End Sub

    Public Sub SetDownPayAmount(ByVal oPolicy As clsPolicyPPA)
        Dim DataRows() As DataRow
        Dim oPayPlanTable As DataTable = Nothing

        Dim oPayPlanDataSet As DataSet = LoadPayPlanTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
        oPayPlanTable = oPayPlanDataSet.Tables(0)

        'get pay plan
        DataRows = oPayPlanDataSet.Tables(0).Select("Program IN ('PPA', '" & oPolicy.Program & "') AND PayPlanCode = '" & oPolicy.PayPlanCode & "'")
        Dim dTotalForInstallCalcs As Double = oPolicy.FullTermPremium

        For Each oRow As DataRow In DataRows
            Dim dTotalForInstall As Double = oPolicy.FullTermPremium
            If Convert.ToBoolean(oRow("UsePremWFeesInCalc")) Then
                For Each oFee As clsBaseFee In oPolicy.Fees
                    If oFee.FeeApplicationType.ToUpper = "SPREAD" Then
                        dTotalForInstall += oFee.FeeAmt
                    End If
                Next
            End If

            oPolicy.DownPaymentAmt = RoundStandard((dTotalForInstall) * (oRow("DownPayPct") / 100), 2)
            For Each oFee As clsBaseFee In oPolicy.Fees
                If oFee.FeeApplicationType.ToUpper = "EARNED" Then
                    oPolicy.DownPaymentAmt += oFee.FeeAmt
                End If
            Next
        Next
    End Sub

    Public Overloads Function RemoveNotes(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sSourceCode As String, ByVal sNoteDescription As String) As System.Collections.Generic.List(Of clsBaseNote)

        For i As Integer = oNoteList.Count - 1 To 0 Step -1
            If oNoteList.Item(i).SourceCode.ToUpper = sSourceCode.ToUpper Then
                If oNoteList.Item(i).NoteDesc.ToUpper.StartsWith(sNoteDescription.ToUpper) Then
                    oNoteList.RemoveAt(i)
                End If
            End If
        Next
        Return oNoteList
    End Function

    Public Overridable Sub ApplyCapFactor(ByVal policy As clsPolicyPPA, ByVal bLogRate As Boolean)
        Dim ratedFactorCap As Double = 0
        Dim cappedRenewalFactor As Decimal = 0
        'commented out MP --- Dim corPolService As New ImperialFire.CorPolicyService
        Dim currentPolicy As New clsPolicyPPA
        'commented out MP --- Dim currentPolicyParams As New ImperialFire.clsPolicyParams

        Try
            'log4net.Info("-----")
            Dim applyRenewalCap As Boolean = GetStateInfoValue(policy.Product, policy.StateCode, policy.RateDate, policy.Program, "RENEWAL", "FACTOR", "CAPPED_RENEWAL_NEW") = "1"
            Dim applyRenewalCapWithoutFees As Boolean = GetStateInfoValue(policy.Product, policy.StateCode, policy.RateDate, policy.Program, "RENEWAL", "FACTOR", "CAPPED_RENEWAL_NO_FEES_NEW") = "1"
            'log4net.Info("ApplyCapFactor Before Check applyRenewalCap OrElse applyRenewalCapWithoutFees PolicyID:" & policy.PolicyID)
            If applyRenewalCap OrElse applyRenewalCapWithoutFees Then
                'log4net.Info("ApplyCapFactor Before Check EffDate And TransactionNum PolicyID:" & policy.PolicyID)
                If (policy.EffDate = policy.RenewalQuote.RenewalEffDate) AndAlso (policy.TransactionNum = 1) Then
                    'log4net.Info("ApplyCapFactor After Check EffDate And TransactionNum PolicyID:" & policy.PolicyID)
                    If policy.RenewalQuote.RenewalSourceTrans = "OLE" AndAlso policy.IsPremiumEndorsement Then
                        'log4net.Info("ApplyCapFactorOLE Premium Endorsement PolicyID:" & policy.PolicyID)
                        RemoveCapFactor(policy)
                    Else
                        Dim reRateWithOldRate As Boolean = GetStateInfoValue(policy.Product, policy.StateCode, policy.RateDate, policy.Program, "RENEWAL", "FACTOR", "CAPPED_RENEWAL_RERATE_CURRENT") = "1"
                        Dim applyReverseCapWithoutFees As Boolean = GetStateInfoValue(policy.Product, policy.StateCode, policy.RateDate, policy.Program, "RENEWAL", "FACTOR", "CAPPED_RENEWAL_NO_FEES_DECREASE") = "1"

                        'log4net.Info("ApplyCapFactor Before RemoveCapFactor PolicyID:" & policy.PolicyID)
                        RemoveCapFactor(policy)
                        'log4net.Info("ApplyCapFactor After RemoveCapFactor PolicyID:" & policy.PolicyID)

                        'commented out MP --- With currentPolicyParams
                        'commented out MP ---     .PolicyID = policy.PolicyID
                        'commented out MP ---     .AsOfDate = policy.EffDate
                        'commented out MP --- End With

                        'log4net.Info("ApplyCapFactor Before Load currentPolicy PolicyID:" & policy.PolicyID)
                        'commented out MP --- currentPolicy = corPolService.LoadPASPolicyUsingParams(currentPolicyParams)
                        'log4net.Info("ApplyCapFactor After Load currentPolicy PolicyID:" & policy.PolicyID)
                        If currentPolicy.EffDate = policy.EffDate Then
                            'commented out MP --- With currentPolicyParams
                            'commented out MP ---     .PolicyID = policy.PolicyID
                            'commented out MP ---     .AsOfDate = policy.PriorTermEffDate
                            'commented out MP --- End With

                            If policy.Type.ToUpper = "RENEWAL" And currentPolicy.EffDate = policy.EffDate Then
                                'commented out MP --- With currentPolicyParams
                                'commented out MP ---     .PolicyID = policy.PolicyID
                                'commented out MP ---     .AsOfDate = policy.PriorTermEffDate
                                'commented out MP --- End With

                                'commented out MP --- currentPolicy = corPolService.LoadPASPolicyUsingParams(currentPolicyParams)
                            End If

                            'commented out MP --- currentPolicy = corPolService.LoadPASPolicyUsingParams(currentPolicyParams)
                        End If

                        'log4net.Info("ApplyCapFactor Before Check Violations Added At Renewal PolicyID:" & policy.PolicyID)
                        'log4net.Info("currentPolicy EffDate: " & currentPolicy.EffDate)
                        'log4net.Info("renewalPolicy EffDate: " & policy.EffDate)
                        Dim violAddedAtRenewal As Boolean = DetermineIfViolAddedAtRenewal(currentPolicy, policy)
                        'log4net.Info("ApplyCapFactor After Check Violations Added At Renewal PolicyID:" & policy.PolicyID & ": " & violAddedAtRenewal)
                        
                        If Not violAddedAtRenewal Then
                            Dim extraDriver As clsEntityDriver = Nothing
                            For x As Integer = policy.Drivers.Count - 1 To 0 Step -1
                                If policy.Drivers.Item(x).IndexNum = 99 Or policy.Drivers.Item(x).IndexNum = 98 Then
                                    extraDriver = policy.Drivers.Item(x)
                                End If
                            Next

                            'REMOVE PIF DISCOUNT
                            Dim polFactorCapPIF As clsBaseFactor = Nothing
                            If policy.PolicyFactors IsNot Nothing Then
                                For Each polFactor As clsBaseFactor In policy.PolicyFactors
                                    If polFactor.FactorCode = "PIF" Then
                                        polFactorCapPIF = polFactor
                                        Exit For
                                    End If
                                Next
                            End If

                            Dim reApplyPIF As Boolean = policy.ApplyPIFDiscount
                            Dim callRateToDetermineFullTermPremium As Boolean = False

                            If reRateWithOldRate Then

                                Dim pifDiscountAppliedToCurrentPolicy As Boolean = currentPolicy.ApplyPIFDiscount
                                ' not pif and tthe current is not out then don't call rate
                                ' otherwise call rate 

                                If reApplyPIF Then
                                    ' IF reApplyPIF is TRUE THIS MEANS WE ARE CALLING RATE WITH PIF
                                    ' If current policy is already PIF then do not remove PIF from current policy 
                                    ' If it is PIF then i don't call rate, otherwise apply PIF to current policy and call rate 
                                    If pifDiscountAppliedToCurrentPolicy Then
                                        ' Do not remove the PIF factor or set ApplyPIFDiscount to false on renewal policy 
                                        policy.ApplyPIFDiscount = True ' just to be safe apply PIF
                                    Else
                                        currentPolicy.ApplyPIFDiscount = True
                                        callRateToDetermineFullTermPremium = True
                                    End If
                                Else
                                    ' IF reApplyPIF is FALSE THIS MEANS WE ARE CALLING RATE WITHOUT PIF
                                    ' If current policy is already PIF then remove PIF from current policy and call rate
                                    ' If current policy is not PIF then i don't call rate, otherwise apply PIF to current policy and call rate 
                                    If pifDiscountAppliedToCurrentPolicy Then
                                        ' just to be safe remove PIF factor and set set ApplyPIFDiscount to false
                                        If polFactorCapPIF IsNot Nothing Then
                                            policy.PolicyFactors.Remove(polFactorCapPIF)
                                        End If
                                        policy.ApplyPIFDiscount = False

                                        Dim currentPolFactorCapPIF As clsBaseFactor = Nothing
                                        If currentPolicy.PolicyFactors IsNot Nothing Then
                                            For Each currentPolFactor As clsBaseFactor In currentPolicy.PolicyFactors
                                                If currentPolFactor.FactorCode = "PIF" Then
                                                    currentPolFactorCapPIF = currentPolFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                        If currentPolFactorCapPIF IsNot Nothing Then
                                            currentPolicy.PolicyFactors.Remove(currentPolFactorCapPIF)
                                        End If
                                        currentPolicy.ApplyPIFDiscount = False

                                        callRateToDetermineFullTermPremium = True
                                    Else
                                        ' DO NOTHING, Do not remove the factor or set ApplyPIFDiscount to false
                                        If polFactorCapPIF IsNot Nothing Then
                                            policy.PolicyFactors.Remove(polFactorCapPIF)
                                        End If
                                    End If
                                End If
                            Else
                                'RATE WITHOUT CAP FACTOR AND PIF DISCOUNT          
                                If polFactorCapPIF IsNot Nothing Then
                                    policy.PolicyFactors.Remove(polFactorCapPIF)
                                End If

                                policy.ApplyPIFDiscount = False
                            End If

                            Rate(policy, bLogRate)

                            If extraDriver IsNot Nothing Then
                                policy.Drivers.Add(extraDriver)
                            End If

                            'RE-APPLY PIF IF NECESSARY
                            policy.ApplyPIFDiscount = reApplyPIF
                            If reApplyPIF Then
                                If polFactorCapPIF IsNot Nothing Then
                                    If Not FactorOnPolicy(policy, "PIF") Then
                                        policy.PolicyFactors.Add(polFactorCapPIF)
                                    End If
                                End If
                            End If

                            'log4net.Info("ApplyCapFactor Before Get cappedRenewalFactor PolicyID:" & policy.PolicyID)
                            cappedRenewalFactor = CDec(GetStateInfoValue(policy.Product, policy.StateCode, policy.RateDate, policy.Program, "RENEWAL", "FACTOR", "CAPPED_FACTOR"))
                            'log4net.Info("ApplyCapFactor After Get cappedRenewalFactor PolicyID:" & policy.PolicyID & ": " & cappedRenewalFactor)

                            'log4net.Info("ApplyCapFactor Before Check applyRenewalCap PolicyID:" & policy.PolicyID)
                            If applyRenewalCap Then
                                'log4net.Info("ApplyCapFactor After Check applyRenewalCap PolicyID:" & policy.PolicyID & ": " & applyRenewalCap)
                                Dim fullTermPremWFeesNoCap As Double = 0
                                Dim currentFullTermPremiumWithFees As Double = 0

                                fullTermPremWFeesNoCap = policy.FullTermPremium + policy.TotalFees

                                If callRateToDetermineFullTermPremium Then
                                    'commented out MP --- currentPolicy = corPolService.RatePolicy(currentPolicy, True)
                                    currentFullTermPremiumWithFees = currentPolicy.FullTermPremium + currentPolicy.TotalFees
                                Else
                                    currentFullTermPremiumWithFees = GetCurrentTermPremium(policy, True)
                                End If

                                If (fullTermPremWFeesNoCap) > (currentFullTermPremiumWithFees * (1 + cappedRenewalFactor)) Then
                                    ratedFactorCap = Math.Round(((currentFullTermPremiumWithFees * (1 + cappedRenewalFactor)) - policy.TotalFees) / (policy.FullTermPremium), 4)
                                Else
                                    ratedFactorCap = 1.0
                                End If
                            ElseIf applyRenewalCapWithoutFees Then
                                'log4net.Info("ApplyCapFactor After Check applyRenewalCapWithoutFees PolicyID:" & policy.PolicyID & ": " & applyRenewalCapWithoutFees)
                                Dim fullTermPremWithoutFeesNoCap As Double = 0
                                Dim currentFullTermPremiumWithoutFees As Double = 0

                                fullTermPremWithoutFeesNoCap = policy.FullTermPremium

                                If callRateToDetermineFullTermPremium Then
                                    'commented out MP --- currentPolicy = corPolService.RatePolicy(currentPolicy, True)
                                    currentFullTermPremiumWithoutFees = currentPolicy.FullTermPremium
                                Else
                                    currentFullTermPremiumWithoutFees = GetCurrentTermPremium(policy, False)
                                End If

                                If (fullTermPremWithoutFeesNoCap) > (currentFullTermPremiumWithoutFees * (1 + cappedRenewalFactor)) Then
                                    ratedFactorCap = Math.Round((currentFullTermPremiumWithoutFees * (1 + cappedRenewalFactor)) / (policy.FullTermPremium), 4)
                                ElseIf applyReverseCapWithoutFees _
                                    AndAlso (fullTermPremWithoutFeesNoCap) < (currentFullTermPremiumWithoutFees * (1 + (-cappedRenewalFactor))) Then
                                    ratedFactorCap = Math.Round((currentFullTermPremiumWithoutFees * (1 + -cappedRenewalFactor)) / (policy.FullTermPremium), 4)
                                Else
                                    ratedFactorCap = 1.0
                                End If
                            End If

                            'log4net.Info("ApplyCapFactor Before Check ratedFactorCap PolicyID:" & policy.PolicyID)
                            If ratedFactorCap <> 1.0 Then
                                'log4net.Info("ApplyCapFactor After Check ratedFactorCap PolicyID:" & policy.PolicyID & ": " & ratedFactorCap)
                                If policy.Notes Is Nothing Then
                                    policy.Notes = New Generic.List(Of clsBaseNote)
                                End If
                                Dim oNote As New clsBaseNote
                                With oNote
                                    .SourceCode = "DIS"
                                    If currentPolicy IsNot Nothing AndAlso currentPolicy.ApplyPIFDiscount Then
                                        .NoteDesc = "RENEWALCAPFACTOR_PIF"
                                    Else
                                        .NoteDesc = "RENEWALCAPFACTOR"
                                    End If
                                    .NoteText = ratedFactorCap
                                End With
                                policy.Notes.Add(oNote)

                                If policy.PolicyFactors Is Nothing Then
                                    policy.PolicyFactors = New Generic.List(Of clsBaseFactor)
                                End If

                                Dim oPolFactor As New clsBaseFactor
                                With oPolFactor
                                    .CovType = "N"
                                    .SystemCode = "CAPPED_RENEWAL"
                                    .FactorCode = "CAPPED_RENEWAL"
                                    .FactorDesc = "Capped Renewal Factor"
                                    .FactorNum = policy.PolicyFactors.Count + 1
                                    .FactorType = "LastMult"
                                    .IndexNum = policy.PolicyFactors.Count + 1
                                    .RatedFactor = ratedFactorCap
                                End With
                                policy.PolicyFactors.Add(oPolFactor)
                            End If
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Throw
        End Try

    End Sub

    Private Sub RemoveCapFactor(ByRef policy As clsBasePolicy)

        Try
            ' REMOVE FACTOR
            Dim noteCapFactor As clsBaseNote = Nothing
            Dim noteCapFactorPIF As clsBaseNote = Nothing
            If policy.Notes IsNot Nothing Then
                For Each note As clsBaseNote In policy.Notes
                    If note.NoteDesc.Trim = "RENEWALCAPFACTOR" Then
                        noteCapFactor = note
                        Exit For
                    End If
                Next
            End If
            If noteCapFactor IsNot Nothing Then
                policy.Notes.Remove(noteCapFactor)
            End If

            If policy.Notes IsNot Nothing Then
                For Each note As clsBaseNote In policy.Notes
                    If note.NoteDesc.Trim = "RENEWALCAPFACTOR_PIF" Then
                        noteCapFactorPIF = note
                        Exit For
                    End If
                Next
            End If
            If noteCapFactorPIF IsNot Nothing Then
                policy.Notes.Remove(noteCapFactorPIF)
            End If

            Dim polFactorCapRenewal As clsBaseFactor = Nothing
            If policy.PolicyFactors IsNot Nothing Then
                For Each polFactor As clsBaseFactor In policy.PolicyFactors
                    If polFactor.FactorCode = "CAPPED_RENEWAL" Then
                        polFactorCapRenewal = polFactor
                        Exit For
                    End If
                Next
            End If
            If polFactorCapRenewal IsNot Nothing Then
                policy.PolicyFactors.Remove(polFactorCapRenewal)
            End If

        Catch ex As Exception
            Throw
        End Try

    End Sub

    Private Function DetermineIfViolAddedAtRenewal(ByVal currentPolicy As clsPolicyPPA, ByVal renewalPolicy As clsPolicyPPA) As Boolean
        Dim violAdded As Boolean = False

        Try
            For Each driver As clsEntityDriver In currentPolicy.Drivers
                If Not violAdded Then
                    For Each renewalDriver As clsEntityDriver In renewalPolicy.Drivers
                        If driver.IndexNum = renewalDriver.IndexNum _
                            AndAlso driver.EntityName1 = renewalDriver.EntityName1 _
                            AndAlso driver.EntityName2 = renewalDriver.EntityName2 _
                            AndAlso driver.DOB = renewalDriver.DOB _
                            AndAlso driver.DLN = renewalDriver.DLN Then
                            log4net.Info(renewalDriver.Violations.Count & " : " & driver.Violations.Count)
                            If renewalDriver.Violations.Count > driver.Violations.Count Then
                                violAdded = True
                                Exit For
                            End If
                        End If
                    Next
                Else
                    Exit For
                End If
            Next

            Return violAdded

        Catch ex As Exception
            Throw
        End Try

    End Function

    Private Function GetCurrentTermPremium(ByVal policy As clsBasePolicy, ByVal includeFees As Boolean) As Double
        Dim sql As New StringBuilder
        Dim currentTermPremium As Double = 0
        'TODO: Change this to new policy system
        Try

            'If includeFees Then
            '    With sql
            '        .Append("  select sum(Amount) as Amount ")
            '        .Append("    from  ")
            '        .Append("       (select FullTermPremiumAmt as Amount  ")
            '        .Append("          from PasCarrier..Policy p with (nolock)  ")
            '        .Append("         inner join PasCarrier..PolicyControl pc with (nolock) ")
            '        .Append("            on pc.PolicyNo = p.PolicyNo  ")
            '        .Append("           and pc.CurrentTermEffDate = p.TermEffDate  ")
            '        .Append("         where p.PolicyNo = @PolicyNo  ")
            '        .Append("           and PolicyTransactionNum = ")
            '        .Append("                (select max(PolicyTransactionNum) ")
            '        .Append("                   from PasCarrier..Policy p with (nolock) ")
            '        .Append("                  inner join PasCarrier..PolicyControl pc with (nolock) ")
            '        .Append("                     on pc.PolicyNo = p.PolicyNo  ")
            '        .Append("                    and pc.CurrentTermEffDate = p.TermEffDate  ")
            '        .Append("                  where p.PolicyNo = @PolicyNo ) ")
            '        .Append("   union ")
            '        .Append("        select sum(TransactionAmt) as Amount ")
            '        .Append("          from PasCarrier..DirectBillAR ar with (nolock) ")
            '        .Append("         inner join PasCarrier..PolicyControl pc with (nolock) ")
            '        .Append("            on pc.PolicyNo = ar.PolicyNo  ")
            '        .Append("           and pc.CurrentTermEffDate = ar.TermEffDate")
            '        .Append("           and TransactionAccountCode in ('PF') ")
            '        .Append("         where ar.PolicyNo = @PolicyNo) as a ")
            '    End With
            'Else
            '    With sql
            '        .Append("  SELECT FullTermPremiumAmt AS Amount ")
            '        .Append("    FROM PasCarrier..Policy p WITH (nolock) ")
            '        .Append("   WHERE p.PolicyNo = @PolicyNo ")
            '        .Append("     AND p.TermEffDate = (SELECT MAX(TermEffDate) ")
            '        .Append("                            FROM PasCarrier..Policy p2 WITH (nolock) ")
            '        .Append("                           WHERE p.PolicyNo = p2.PolicyNo ")
            '        .Append("                             AND PolicyStatusInd = 'F') ")
            '        .Append("     AND PolicyTransactionNum = (SELECT MAX(PolicyTransactionNum) ")
            '        .Append("                                   FROM PasCarrier..Policy p3 WITH (nolock) ")
            '        .Append("                                  WHERE p.PolicyNo = p3.PolicyNo ")
            '        .Append("                                    AND p.TermEffDate = p3.TermEffDate) ")
            '    End With
            'End If

            Using cn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString)
                cn.Open()

                Using cmd As SqlCommand = New SqlCommand(sql.ToString, cn)
                    cmd.Parameters.Add("@PolicyNo", SqlDbType.VarChar, 15).Value = policy.PolicyID

                    Dim oRr As SqlDataReader = cmd.ExecuteReader()

                    If oRr.Read Then
                        currentTermPremium = oRr("Amount")
                    End If
                    oRr.Close()
                End Using
            End Using

            Return currentTermPremium

        Catch ex As Exception
            Throw ex
        Finally

        End Try

    End Function

End Class
