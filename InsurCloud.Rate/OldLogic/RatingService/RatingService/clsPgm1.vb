Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Activator
Imports CorPolicy.clsCommonFunctions
Imports CorPolicy
Imports System.Collections.Generic

Public Class clsPgm1
    Inherits clsPgm

    Protected moCappedFactorsTable As DataTable
    Protected msCappedFactors As String()

    Public Sub New()

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
        oLogSvc.WriteHomeownersLog(moLogging, sMethodName, sMessage)
    End Sub

    Public Overridable Sub UpdateLog(ByVal oPolicy As CorPolicy.clsPolicyHomeOwner, ByVal oFactorTable As System.Data.DataTable)
        CType(moLogging, ImperialFire.clsLogging1).Policy = oPolicy
        moLogging.DataTable.Add(oFactorTable)
    End Sub

    Public Overridable Sub RoundDwellingContents(ByVal oPolicy As CorPolicy.clsPolicyHomeOwner)
        Dim lContentsAmt As Long = 0
        lContentsAmt = oPolicy.DwellingUnits(0).ContentsAmt
        lContentsAmt = Math.Ceiling(lContentsAmt / 40) * 40
        oPolicy.DwellingUnits(0).ContentsAmt = lContentsAmt

        Dim lDwellingAmount As Long = 0
        lDwellingAmount = oPolicy.DwellingUnits(0).DwellingAmt
        lDwellingAmount = Math.Ceiling(lDwellingAmount / 100) * 100
        oPolicy.DwellingUnits(0).DwellingAmt = lDwellingAmount
    End Sub

    Public Overloads Function Rate(ByVal oPolicy As clsPolicyHomeOwner, ByVal bLogRate As Boolean) As Boolean

        Dim oFactorTable As DataTable = Nothing
        Dim oFeeTable As DataTable = Nothing
        Dim drTotalsRow As DataRow = Nothing
        Dim dFullTermPrem As Decimal = 0
        Dim dPolicyFullTermPremium As Decimal = 0

        Try
            Call RoundDwellingContents(oPolicy)

            Call BeginLogging(moLogging, oPolicy, oFactorTable)

            Call InitializeConnection()

            moStateInfoDataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)

            Call SetLimitAmounts(oPolicy)

            oFactorTable = CreateDataTable("Factors", oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, oPolicy.Product, oPolicy.StateCode)

            Call GetFactors(oPolicy, oFactorTable)

            oFactorTable.Rows.Add(CreateTotalsRow(oFactorTable))

            Call Calculate(oPolicy, oFactorTable)

            oFactorTable.AcceptChanges()

            Call GetTotalChgInPremPolFactors(oPolicy)

            Call GetTotalChgInPremEndorseFactors(oPolicy, oFactorTable)

            Call UpdateTotals(oPolicy, oFactorTable)

            drTotalsRow = GetRow(oFactorTable, "Totals")

            GetPremiums(oPolicy, drTotalsRow)

            'populate the unit's full term premium and the policy's full term premium
            For Each oDwelling As clsDwellingUnit In oPolicy.DwellingUnits
                dFullTermPrem = 0
                For Each oCov As clsHomeOwnerCoverage In oDwelling.Coverages
                    dFullTermPrem = dFullTermPrem + oCov.FullTermPremium
                Next
                oDwelling.FullTermPremium = dFullTermPrem
                dPolicyFullTermPremium += oDwelling.FullTermPremium
            Next

            ' TODO: Don't add this in if the endorsment is a midmult
            For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
                If Not oEndorse.IsMarkedForDelete Then
                    Dim bAddtoPremium As Boolean = True
                    Dim drEndorseRow As DataRow
                    drEndorseRow = GetRow(oFactorTable, oEndorse.FactorCode & "-ENDORSE")


                    If Not drEndorseRow Is Nothing Then
                        If drEndorseRow("FactorType").ToString.ToUpper = "MIDMULT" Then
                            bAddtoPremium = False
                        End If
                    End If

                    ' Only do this if the factor is not a midmult
                    If bAddtoPremium Then
                        dPolicyFullTermPremium += oEndorse.FactorAmt
                    End If
                End If
            Next
            oPolicy.FullTermPremium = dPolicyFullTermPremium

            'Load Fees
            Call LoadFees(oPolicy)

            oFeeTable = CreateFeesTable()

            dbGetFeeFactor(oPolicy, oFeeTable)

            UpdateLog(oPolicy, oFactorTable)

            Call CleanDataTable(oPolicy, oFactorTable)

            ' don't log the rate if we already have an entry with this premium amount
            If bLogRate Then
                'moLogging.WriteLogXML = DetermineIfLoggingNeeded(oPolicy)
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
            Call ErrorLogging("HomeownersRate", ex.Message & ex.StackTrace)
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Private Function DetermineIfLoggingNeeded(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
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

    Public Overridable Sub UpdateTotals(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)

        Dim drTotalsRow As DataRow = Nothing
        Dim sCovInfo() As String

        Try
            drTotalsRow = GetRow(oFactorTable, "Totals")
            If Not drTotalsRow Is Nothing Then
                For Each oDataCol As DataColumn In oFactorTable.Columns
                    sCovInfo = oDataCol.ColumnName.ToString.Split("_")
                    If sCovInfo.Length > 1 Then
                        If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                            If Right(oDataCol.ColumnName.ToString.ToUpper, 1) = "D" And oPolicy.DwellingUnits.Item(0).DwellingAmt = 0 Then
                                drTotalsRow(oDataCol.ColumnName.ToString) = 0
                            ElseIf Right(oDataCol.ColumnName.ToString.ToUpper, 1) = "C" And oPolicy.DwellingUnits.Item(0).ContentsAmt = 0 Then
                                drTotalsRow(oDataCol.ColumnName.ToString) = 0
                            ElseIf Not PolicyContainsCov(oPolicy, sCovInfo(0), sCovInfo(1)) Then 'policy does not contain coverage then set the amount for this coverage to 0
                                drTotalsRow(oDataCol.ColumnName.ToString) = 0
                            End If
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not drTotalsRow Is Nothing Then
                drTotalsRow = Nothing
            End If
        End Try
    End Sub

    Public Overridable Function PolicyContainsCov(ByVal oPolicy As clsPolicyHomeOwner, ByVal sCov As String, ByVal sCovType As String) As Boolean

        Dim bPolicyContainsCov As Boolean = False

        If sCovType = "N" Then
            'assume this coverage is ok because it is either LIA or MED, which are always required, or an endorsement
            bPolicyContainsCov = True
        Else
            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                If oCov.CovGroup.ToUpper = sCov.ToUpper Then
                    If oCov.Type.ToUpper = sCovType.ToUpper Then
                        bPolicyContainsCov = True
                        Exit For
                    End If
                End If
            Next
        End If

        Return bPolicyContainsCov

    End Function

    Public Overridable Function AllowAEC(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

    End Function

    Public Overridable Sub LoadFees(ByVal oPolicy As clsPolicyHomeOwner)
    End Sub

    Public Overridable Sub GetFactors(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)

        Try

            ' clear out scheduled property amoutns
            For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                oProperty.PropertyPremiumAmt = 0
            Next

            dbGetBaseRateFactor(oPolicy, oFactorTable)
            dbGetAmtOfInsuranceFactor(oPolicy, oFactorTable)
            If oPolicy.DwellingUnits(0).OtherStructureAmt > (0.11 * (oPolicy.DwellingUnits(0).DwellingAmt)) Then
                dbGetAPSFactor(oPolicy, oFactorTable)
            End If
            dbGetDed1Factor(oPolicy, oFactorTable)
            dbGetDed2Factor(oPolicy, oFactorTable)
            dbGetDed3Factor(oPolicy, oFactorTable)
            dbGetTerritoryFactor(oPolicy, oFactorTable)
            dbGetRegionFactor(oPolicy, oFactorTable)
            dbGetPCFactor(oPolicy, oFactorTable)
            'dbGetMoldBuyBackFactor(oPolicy, oFactorTable)
            dbGetTierMatrixFactor(oPolicy, oFactorTable, dbGetCreditTier(oPolicy), dbGetUWTier(oPolicy))
            dbGetPolicyFactor(oPolicy, oFactorTable)
            dbGetEndorsementFactor(oPolicy, oFactorTable)
            dbGetRatedFactor(oPolicy, oFactorTable)

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
        End Try
    End Sub


    Public Overridable Function dbGetRatedFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
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
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try
    End Function


    Public Overridable Function dbGetBaseRateFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorBaseRate with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

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
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetMoldBuyBackFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim oDataCol As DataColumn = Nothing

        Try
            For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
                If Not oEndorse.IsMarkedForDelete Then
                    If oEndorse.FactorCode.ToUpper = "ADDLMOLD" Then
                        Using cmd As New SqlCommand(sSql, moConn)

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorMoldBuyBack with(nolock)"
                            sSql = sSql & " WHERE Program = @Program "
                            sSql = sSql & " AND EffDate <= @RateDate "
                            sSql = sSql & " AND ExpDate > @RateDate "
                            sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                            sSql = sSql & " AND DwellingAmt = @DwellingAmt "
                            sSql = sSql & " AND Coverage = @FactorCode "
                            sSql = sSql & " AND Limit = @Limit "
                            sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                            'Execute the query
                            cmd.CommandText = sSql

                            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                            cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt
                            cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 11).Value = oEndorse.FactorCode
                            cmd.Parameters.Add("@Limit", SqlDbType.Int, 22).Value = oEndorse.Limit

                            oReader = cmd.ExecuteReader

                            If oReader.HasRows Then
                                drFactorRow = FactorTable.NewRow
                                drFactorRow.Item("FactorName") = oEndorse.FactorCode
                            End If

                            Do While oReader.Read()
                                oDataCol = FactorTable.Columns("FlatFactor")
                                If IsNumeric(oReader.Item("Factor")) Then
                                    drFactorRow.Item(oDataCol.ColumnName) = RoundStandard(CDec(oReader.Item("Factor")), 0)
                                    oEndorse.FactorAmt += RoundStandard(CDec(oReader.Item("Factor")), 0)
                                Else
                                    drFactorRow.Item(oDataCol.ColumnName) = oReader.Item("Factor")
                                End If

                                If Not bFactorType Then
                                    drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                    bFactorType = True
                                End If
                            Loop

                        End Using
                    End If
                End If
            Next
            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetPCFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
		Dim bFactorType As Boolean = False

		Dim sContructionType As String = String.Empty
		sContructionType = MapContructionType(oPolicy)

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorPC with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Class = @ProtectionClass "
                sSql = sSql & " AND Construction = @Construction "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@ProtectionClass", SqlDbType.Int, 22).Value = CInt(oPolicy.DwellingUnits.Item(0).ProtectionClass)
				cmd.Parameters.Add("@Construction", SqlDbType.VarChar, 25).Value = sContructionType

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "PC"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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


	Private Function MapContructionType(ByVal oPolicy As clsPolicyHomeOwner) As String
		Dim sConstructionType As String = oPolicy.DwellingUnits(0).Construction
		Dim DataRows() As DataRow
		Dim oEditCodeTable As DataTable = Nothing

		Dim oEditCodeDataSet As DataSet = LoadEditCodeTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
		oEditCodeTable = oEditCodeDataSet.Tables(0)

		'get coverages and limits for newly selected program
		DataRows = oEditCodeTable.Select("Program='HOM' AND Category = 'CONSTRUCTION' AND SubCategory = 'TYPE' AND EditCode = '" & oPolicy.DwellingUnits(0).Construction & "'")

		If DataRows.Length > 0 Then
			For Each orow As DataRow In DataRows
				sConstructionType = orow("EditValue")
			Next
		End If

		Return sConstructionType
	End Function

	Public Function LoadEditCodeTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
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

	Public Sub GetCappedFactors(ByVal oPolicy As clsPolicyHomeOwner)

		Dim DataRows() As DataRow
		Dim oStateInfoTable As DataTable = Nothing
		oStateInfoTable = moStateInfoDataSet.Tables(0)

		DataRows = oStateInfoTable.Select("Program IN ('HOM', '" & oPolicy.Program & "') AND ItemGroup='MAXDISCOUNT' AND ItemCode='FACTOR' ")
		Dim i As Integer = 0
		For Each oRow As DataRow In DataRows
			ReDim Preserve msCappedFactors(i)
			msCappedFactors(i) = oRow.Item("ItemValue").ToString
			i += 1
		Next


	End Sub

    Public Function MaxDiscountAmount(ByVal oPolicy As clsPolicyHomeOwner, ByVal sCov As String, ByVal sCovType As String) As Decimal

        Dim sSql As String = String.Empty
        Dim dMaxDiscountAmt As Decimal = 99

        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Factor FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorPolicy with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND FactorCode = 'TIER_DISC_SUR_MAX' "
                sSql = sSql & " AND Coverage = @Coverage "
                sSql = sSql & " AND Type = @CovType "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 20).Value = sCov
                cmd.Parameters.Add("@CovType", SqlDbType.VarChar, 1).Value = sCovType

                dMaxDiscountAmt = cmd.ExecuteScalar
            End Using
        Catch Ex As Exception
        Finally
        End Try


        Return dMaxDiscountAmt

    End Function

    Public Overridable Function dbGetPolicyFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing

        moCappedFactorsTable = CreateDataTable("CappedFactors", oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, oPolicy.Product, oPolicy.StateCode)

        drFactorRow = moCappedFactorsTable.NewRow
        drFactorRow.Item("FactorName") = "MaxDiscountAmt"
        For i As Integer = 1 To moCappedFactorsTable.Columns.Count - 1
            If moCappedFactorsTable.Columns.Item(i).ColumnName.IndexOf("_") > 0 Then
                drFactorRow.Item(moCappedFactorsTable.Columns.Item(i).ColumnName) = MaxDiscountAmount(oPolicy, Split(moCappedFactorsTable.Columns.Item(i).ColumnName, "_")(0), Split(moCappedFactorsTable.Columns.Item(i).ColumnName, "_")(1))
            End If
        Next
        moCappedFactorsTable.Rows.Add(drFactorRow)
        If Not drFactorRow Is Nothing Then
            drFactorRow = Nothing
        End If
        moCappedFactorsTable.Rows.Add(CreateTotalsRow(moCappedFactorsTable))

        GetCappedFactors(oPolicy)

        drFactorRow = Nothing
        Try

            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                bFactorType = False
                drFactorRow = Nothing

                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT Coverage, Description, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorPolicy with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND FactorCode = @FactorCode "
                    If oFactor.CovType <> "" Then
                        sSql = sSql & " AND Type = @CovType "
                    End If
                    sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode
                    If oFactor.CovType <> "" Then
                        cmd.Parameters.Add("@CovType", SqlDbType.VarChar, 1).Value = oFactor.CovType
                    End If

                    oReader = cmd.ExecuteReader

                    If oReader.HasRows Then
                        drFactorRow = FactorTable.NewRow
                        drFactorRow.Item("FactorName") = oFactor.FactorCode
                    End If



                    Do While oReader.Read()
                        oFactor.FactorDesc = oReader.Item("Description").ToString
                        For i As Integer = 1 To FactorTable.Columns.Count - 1
                            Dim bIsCappedFactor As Boolean = False
                            If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
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
                                    If (CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) And CDec(oReader.Item("Factor")) < 1 Then
                                        'no more discounts, set to 1.0
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = 1
                                        Exit For
                                    ElseIf (CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) <> 0) And CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) * CDec(oReader.Item("Factor")) <= CDec(drMaxDiscountRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) Then
                                        'set the factor to the difference between the MaxAmount and the current total
                                        Dim dDiscount As Decimal = 0
                                        dDiscount = CDec(drMaxDiscountRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) / CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")))
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dDiscount
                                        drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) * dDiscount
                                    Else
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = CDec(oReader.Item("Factor"))
                                        Dim dMultiplier As Decimal = 0
                                        dMultiplier = IIf(CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))) = 0, 1, CDec(drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type"))))
                                        drTotalsRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dMultiplier * CDec(oReader.Item("Factor"))
                                        Exit For
                                    End If
                                Else
                                    'add it to the data row
                                    drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = CDec(oReader.Item("Factor"))
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetEndorsementFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim sFactorCode As String = ""
        Dim dFactor As Decimal = 0

        Dim dCappedFActor As Decimal = 1.0

        For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
            If oFactor.FactorCode = "CAPPED_RENEWAL" Then
                dCappedFActor *= CType(oFactor.RatedFactor, Decimal)
            End If
        Next

        Try

            Dim iIndex As Integer = 1
            For Each oFactor As clsEndorsementFactor In oPolicy.EndorsementFactors
                If Not oFactor.IsMarkedForDelete Then
                    dFactor = 0

                    bFactorType = False
                    Dim cmd As SqlCommand
                    cmd = dbGetEndorsementFactorCmd(oPolicy, oFactor)
                    Using cmd
                        oReader = cmd.ExecuteReader

                        If oReader.HasRows Then
                            drFactorRow = FactorTable.NewRow
                            drFactorRow.Item("FactorName") = oFactor.FactorCode & "-ENDORSE"
                        End If

                        Do While oReader.Read()
                            If oReader.Item("FactorCode").ToString = oFactor.FactorCode Then
                                oFactor.CovType = oReader.Item("Type").ToString
                            End If
                            'If IsMoldBuyBackEndorsement(oPolicy, oFactor.FactorCode) Then
                            If IsFlatFactorEndorsement(oPolicy, oFactor.FactorCode, moStateInfoDataSet) Then
                                'moldbuyback, water back up (LA)
                                'not at the coverage level so add it to the flat factor column
                                If dFactor = 0 Then
                                    dFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, FactorTable) * dCappedFActor
                                    drFactorRow.Item("FlatFactor") = dFactor
                                End If
                            Else
                                'this returns the factor and factor type for all coverages
                                'we will start with the 2nd column since we know the 1st is the factor name
                                For i As Integer = 1 To FactorTable.Columns.Count - 1
                                    If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                                        'add it to the data row
                                        dFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, FactorTable) * dCappedFActor

                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dFactor
                                        Exit For
                                    End If
                                Next
                                If Not bFactorType Then
                                    drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                    bFactorType = True
                                End If
                            End If
                        Loop

                    End Using
                    If Not drFactorRow Is Nothing Then
                        FactorTable.Rows.Add(drFactorRow)
                        drFactorRow = Nothing
                    End If
                    If Not oReader Is Nothing Then
                        oReader.Close()
                        oReader = Nothing
                    End If
                End If
            Next

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetEndorsementFactorCmd(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactor As clsEndorsementFactor) As SqlCommand
        Dim sSql As String = String.Empty
        Dim cmd As New SqlCommand(sSql, moConn)

        sSql = " SELECT FactorCode, Coverage, Type, Factor, FactorType, Crit1, Crit2, Crit3, Crit4 FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorEndorsement with(nolock)"
        sSql = sSql & " WHERE Program = @Program "
        sSql = sSql & " AND EffDate <= @RateDate "
        sSql = sSql & " AND ExpDate > @RateDate "
        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
        sSql = sSql & " AND FactorCode = @FactorCode "
        sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

        'Execute the query
        cmd.CommandText = sSql

        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
        cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode

        Return cmd
    End Function

    Public Overridable Function dbGetTierMatrixFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable, ByVal sCreditTier As String, ByVal sUWTier As String) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim sFormType As String = ""

        Try

            Using cmd As New SqlCommand(sSql, moConn)

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

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "TierMatrix"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetDed1Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded1) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded1) AND Region IN ( @Region , '99') AND (CovAmtStart < @ContentsAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    sSql = sSql & " AND ((CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded1) AND Region IN ( @Region , '99') AND (CovAmtStart < @DwellingAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'C' ) "
                    sSql = sSql & " OR (CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded1) AND Region IN ( @Region , '99') AND  (CovAmtStart < @DwellingAmt) AND Type = 'D' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'D' )) "
                End If
                'End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Ded1", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded1
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).ContentsAmt
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Ded1"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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

            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                If oCov.CovLimit <> "" Or oCov.CovDeductible <> "" Then

                    If oCov.CovLimit <> "0" Or oCov.CovDeductible <> "0" Then
                        Using cmd As New SqlCommand(sSql, moConn)

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
                            sSql = sSql & " WHERE Program = @Program "
                            sSql = sSql & " AND EffDate <= @RateDate "
                            sSql = sSql & " AND ExpDate > @RateDate "
                            sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                            sSql = sSql & " AND Coverage = @Coverage "
                            sSql = sSql & " AND Limit1 <= @CovLimit "
                            sSql = sSql & " AND Limit2 > @CovLimit "
                            sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                            'Execute the query
                            cmd.CommandText = sSql

                            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                            cmd.Parameters.Add("@CovLimit", SqlDbType.Int, 22).Value = IIf(oCov.CovLimit = "", 0, CInt(oCov.CovLimit))
                            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oCov.CovGroup

                            oReader = cmd.ExecuteReader

                            Do While oReader.Read()
                                'this returns the factor and factor type for all coverages
                                'we will start with the 2nd column since we know the 1st is the factor name
                                For i As Integer = 1 To FactorTable.Columns.Count - 1
                                    If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
                                        Exit For
                                    End If
                                Next
                                If Not bFactorType Then
                                    drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                    bFactorType = True
                                End If
                            Loop

                        End Using
                    End If
                End If
                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If

            Next

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetDed2Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed2 with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded2) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded2) AND Region IN ( @Region , '99') AND (CovAmtStart < @ContentsAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    sSql = sSql & " AND ((CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded2) AND Region IN ( @Region , '99') AND (CovAmtStart < @DwellingAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'C' ) "
                    sSql = sSql & " OR (CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded2) AND Region IN ( @Region , '99') AND (CovAmtStart < @DwellingAmt) AND Type = 'D' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'D' )) "
                End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Ded2", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded2
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).ContentsAmt
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Ded2"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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

            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                If oCov.CovLimit <> "" Or oCov.CovDeductible <> "" Then

                    If oCov.CovLimit <> "0" Or oCov.CovDeductible <> "0" Then
                        Using cmd As New SqlCommand(sSql, moConn)

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed2 with(nolock)"
                            sSql = sSql & " WHERE Program = @Program "
                            sSql = sSql & " AND EffDate <= @RateDate "
                            sSql = sSql & " AND ExpDate > @RateDate "
                            sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                            sSql = sSql & " AND Coverage = @Coverage "
                            sSql = sSql & " AND Limit1 <= @CovLimit "
                            sSql = sSql & " AND Limit2 > @CovLimit "
                            sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                            'Execute the query
                            cmd.CommandText = sSql

                            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                            cmd.Parameters.Add("@CovLimit", SqlDbType.Int, 22).Value = IIf(oCov.CovLimit = "", 0, CInt(oCov.CovLimit))
                            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oCov.CovGroup

                            oReader = cmd.ExecuteReader

                            Do While oReader.Read()
                                'this returns the factor and factor type for all coverages
                                'we will start with the 2nd column since we know the 1st is the factor name
                                For i As Integer = 1 To FactorTable.Columns.Count - 1
                                    If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
                                        Exit For
                                    End If
                                Next
                                If Not bFactorType Then
                                    drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                    bFactorType = True
                                End If
                            Loop

                        End Using
                    End If
                End If
                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If

            Next

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetDed3Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed3 with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded3) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded3) AND Region IN ( @Region , '99') AND (CovAmtStart < @ContentsAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    sSql = sSql & " AND ((CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded3) AND Region IN ( @Region , '99') AND (CovAmtStart < @DwellingAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'C' ) "
                    sSql = sSql & " OR (CovAmtStart = (SELECT TOP 1 CovAmtStart from pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock) "
                    sSql = sSql & "                      WHERE Program = @Program AND EffDate <= @RateDate AND ExpDate > @RateDate AND AppliesToCode IN ('B', @AppliesToCode) AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded3) AND Region IN ( @Region , '99') AND (CovAmtStart < @DwellingAmt) AND Type = 'C' order by CovAmtStart  DESC)"
                    sSql = sSql & " AND Type = 'D' )) "
                End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Ded3", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded3
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt / 1000

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Ded3"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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

            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                If oCov.CovLimit <> "" Or oCov.CovDeductible <> "" Then

                    If oCov.CovLimit <> "0" Or oCov.CovDeductible <> "0" Then
                        Using cmd As New SqlCommand(sSql, moConn)

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed2 with(nolock)"
                            sSql = sSql & " WHERE Program = @Program "
                            sSql = sSql & " AND EffDate <= @RateDate "
                            sSql = sSql & " AND ExpDate > @RateDate "
                            sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                            sSql = sSql & " AND Coverage = @Coverage "
                            sSql = sSql & " AND Limit1 <= @CovLimit "
                            sSql = sSql & " AND Limit2 > @CovLimit "
                            sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                            'Execute the query
                            cmd.CommandText = sSql

                            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                            cmd.Parameters.Add("@CovLimit", SqlDbType.Int, 22).Value = IIf(oCov.CovLimit = "", 0, CInt(oCov.CovLimit))
                            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oCov.CovGroup

                            oReader = cmd.ExecuteReader

                            Do While oReader.Read()
                                'this returns the factor and factor type for all coverages
                                'we will start with the 2nd column since we know the 1st is the factor name
                                For i As Integer = 1 To FactorTable.Columns.Count - 1
                                    If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
                                        Exit For
                                    End If
                                Next
                                If Not bFactorType Then
                                    drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                    bFactorType = True
                                End If
                            Loop

                        End Using
                    End If
                End If
                If Not oReader Is Nothing Then
                    oReader.Close()
                    oReader = Nothing
                End If

            Next

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetDedFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim oRow As New Object
        Return oRow
    End Function

    Public Overridable Function dbGetTerritoryFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorTerritory with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Territory = @Territory "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Territory", SqlDbType.VarChar, 5).Value = oPolicy.DwellingUnits(0).Territory

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Territory"
                Else                 
                    ' territory isn't valid, do a lookup to see what it should be
                    oReader.Close()
                    ResetTerritory(oPolicy)
                    cmd.Parameters("@Territory").Value = oPolicy.DwellingUnits(0).Territory
                    oReader = cmd.ExecuteReader
                    If oReader.HasRows Then
                        drFactorRow = FactorTable.NewRow
                        drFactorRow.Item("FactorName") = "Territory"
                    End If
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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
                    oConn.Close()

                End Using
            Catch ex As Exception
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try

            If Len(sTerritory) > 0 And Len(sRegion) > 0 Then
                oPolicy.DwellingUnits(0).Territory = sTerritory
                oPolicy.DwellingUnits(0).Region = sRegion
            End If

        End If
    End Sub

    Public Overridable Function dbGetRegionFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorRegion with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Region = @Region "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits(0).Region

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Region"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetAmtOfInsuranceFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim sType As String = ""

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAmtOfInsurance with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                'sSql = sSql & " AND ((Amount = @ContentsAmt and Type = 'C') "
                sSql = sSql & " AND ((Amount = (SELECT TOP 1 Amount "
                sSql = sSql & "                FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAmtOfInsurance with(nolock) "
                sSql = sSql & "                WHERE Cast(@ContentsAmt As Int) <= Cast(Amount As Int)"
                sSql = sSql & "                AND EffDate <= @RateDate "
                sSql = sSql & "                AND ExpDate > @RateDate  "
                sSql = sSql & "                AND Type = 'C'"
                sSql = sSql & "                AND @ContentsAmt > 0 "
                sSql = sSql & "                AND Program = @Program "
                sSql = sSql & "                AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & "                ORDER BY Cast(Amount As Int) Asc) and Type = 'C'"

                'sSql = sSql & " OR (Amount = @DwellingAmt and Type = 'D')) "
                sSql = sSql & " OR (Amount = (SELECT TOP 1 Amount "
                sSql = sSql & "                FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAmtOfInsurance with(nolock) "
                sSql = sSql & "                WHERE Cast(@DwellingAmt As Int) <= Cast(Amount As Int)"
                sSql = sSql & "                AND EffDate <= @RateDate "
                sSql = sSql & "                AND ExpDate > @RateDate  "
                sSql = sSql & "                AND Type = 'D'"
                sSql = sSql & "                AND Program = @Program "
                sSql = sSql & "                AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & "                ORDER BY Cast(Amount As Int) Asc) and Type = 'D'))) "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                ' As of the 4/1/2010 TX Rate change, HOT requires contents amount to be multiplied by 2.5
                ' prior to this date it just uses contentsamt
                If StateInfoContainsProgramSpecific("RATING", "FACTORAMTOFINSURANCE", "MULTCONTENTS", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.Program, oPolicy.RateDate) Then
                    cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits(0).ContentsAmt * 2.5
                Else
                    ' As of the 4/1/2010 TX Rate change, uboth dwelling and contents coverage are both looked up by dwelling value
                    ' HOT should still look up its value by taking the selected contents amount by 2.5
                    If StateInfoContainsProgramSpecific("RATING", "FACTORAMTOFINSURANCE", "USEDWELLINGFORCONTENTS", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.Program, oPolicy.RateDate) Then
                        cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits(0).DwellingAmt
                    Else
                        cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits(0).ContentsAmt
                    End If

                End If
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits(0).DwellingAmt

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "AmtOfInsurance"
                End If

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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

            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                If oCov.CovLimit <> "" Or oCov.CovDeductible <> "" Then
                    If oCov.CovLimit <> "0" Or oCov.CovDeductible <> "0" Then
                        Using cmd As New SqlCommand(sSql, moConn)

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAmtOfInsurance with(nolock)"
                            sSql = sSql & " WHERE Program = @Program "
                            sSql = sSql & " AND EffDate <= @RateDate "
                            sSql = sSql & " AND ExpDate > @RateDate "
                            sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                            ' sSql = sSql & " AND Limit = @Limit "
                            sSql = sSql & " AND Limit = (SELECT TOP 1 Limit "
                            sSql = sSql & "                FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAmtOfInsurance with(nolock) "
                            sSql = sSql & "                WHERE Cast(@Limit As Int) <= Cast(Limit As Int) "
                            sSql = sSql & "                AND EffDate <= @RateDate "
                            sSql = sSql & "                AND ExpDate > @RateDate  "
                            sSql = sSql & "                AND Program = @Program "
                            sSql = sSql & "                AND AppliesToCode IN ('B',  @AppliesToCode ) "
                            sSql = sSql & "                ORDER BY Cast(Limit As Int) Asc) "
                            sSql = sSql & " AND Coverage = @Coverage "
                            sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                            'Execute the query
                            cmd.CommandText = sSql

                            cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                            cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                            cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                            cmd.Parameters.Add("@Limit", SqlDbType.Int, 22).Value = IIf(oCov.CovLimit = "", 0, CInt(oCov.CovLimit))
                            cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oCov.CovGroup

                            oReader = cmd.ExecuteReader

                            Do While oReader.Read()
                                'this returns the factor and factor type for all coverages
                                'we will start with the 2nd column since we know the 1st is the factor name
                                For i As Integer = 1 To FactorTable.Columns.Count - 1
                                    If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                                        'add it to the data row
                                        drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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
                End If
            Next

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetAPSFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            drFactorRow = FactorTable.NewRow
            drFactorRow.Item("FactorName") = "APS"
            'For x As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAPS with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Amount = @APSCovAmount "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                Dim iOtherStructureAmount As Integer
                iOtherStructureAmount = Math.Ceiling((oPolicy.DwellingUnits.Item(0).OtherStructureAmt - (0.1 * (oPolicy.DwellingUnits(0).DwellingAmt))) / 1000) * 1000

                If iOtherStructureAmount > 60000 Then
                    iOtherStructureAmount = 60000
                End If
                cmd.Parameters.Add("@APSCovAmount", SqlDbType.Int, 22).Value = iOtherStructureAmount

                oReader = cmd.ExecuteReader

                Do While oReader.Read()
                    'this returns the factor and factor type for all coverages
                    'we will start with the 2nd column since we know the 1st is the factor name
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            'add it to the data row
                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader.Item("Factor")
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

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If

            Return drFactorRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Overridable Function dbGetFeeFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FeeTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFeeRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim dTotalFees As Decimal = 0

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
                        oFee.FeeType = oReader.Item("FactorType")
                        oFee.FeeApplicationType = oReader.Item("FeeApplicationType")
                    Loop

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

            Return drFeeRow

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetCreditTier(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""

        Try

            Using cmd As New SqlCommand(sSql, moConn)

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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function

    Public Overridable Function dbGetUWTier(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""

        Try

            If oPolicy.DwellingUnits.Item(0).HomeAge >= 999 Then
                oPolicy.DwellingUnits.Item(0).HomeAge = 998
            End If

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Tier FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeUWTiers  with(nolock)"
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

            Return sTier

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function


    Public Overridable Sub MapObjects(ByRef oPolicy As clsPolicyHomeOwner)
        
    End Sub

    Public Overloads Sub Calculate(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)

        Dim drTotalsRow As DataRow = Nothing

        Try

            GetPreMultPremium(oPolicy, FactorTable)

            GetPreAddPremium(oPolicy, FactorTable)

            GetMidMultPremium(oPolicy, FactorTable)

            'check for minimum premium amounts
            CheckMinPremAmounts(oPolicy, FactorTable)

            'set correct factor for mid add factors based off of current premium amount
            UpdateMidAddFactorAmounts(oPolicy, FactorTable)

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


    Public Sub GetPreMultPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
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

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder  with(nolock)"
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
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub
    Public Sub GetPreAddPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' pre add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

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
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetMidMultPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
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
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "RateOrder  with(nolock)"
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
                        If (oReader.Item("FactorName") = FactorTable.Rows(x).Item(0).ToString) Or (oReader.Item("FactorName") & "-ENDORSE" = FactorTable.Rows(x).Item(0).ToString) Then
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
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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

                'round to nearest dollar
                drTotalsRow = GetRow(FactorTable, "Totals")
                For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                    If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                        drTotalsRow(oDataCol.ColumnName.ToString) = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                    End If
                Next

            End Using
        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetMidAddPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' mid add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

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
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetLastMultPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
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
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetPostMultPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
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
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetPostAddPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' post add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

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
                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub GetFeeAddPremium(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)
        'get the values from the rate order table and use that to look up the factors on the data table that are
        ' fee add according to the rate order table and process in the order according to the rate order table
        'Get the factor value and add it to the Totals value for that coverage and replace the Totals value with the new value

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

                                    'make sure it is not an endorsement
                                    If Not Right(oReader.Item("FactorName"), 8).ToUpper = "-ENDORSE" Then
                                        For p As Integer = 0 To oPolicy.DwellingUnits(0).Coverages.Count - 1
                                            'sColCov = FactorTable.Columns(y).ColumnName.Split("_")
                                            'If sColCov(0) = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup Then
                                            If FactorTable.Columns(y).ColumnName = oPolicy.DwellingUnits(0).Coverages.Item(p).CovGroup & "_" & oPolicy.DwellingUnits(0).Coverages.Item(p).Type Then

                                                Dim oCov As clsHomeOwnerCoverage = oPolicy.DwellingUnits(0).Coverages.Item(p)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub UpdateMidAddFactorAmounts(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Decimal = 0
        Dim dTotal As Decimal = 0
        Dim dNewFactor As Decimal = 0
        Dim drMidAddRow As DataRow = Nothing
        Dim drTotalsRow As DataRow = Nothing

        Try
            'all midadds except moldbuyback

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
                                dNewFactor = dTotal * dFactor
                                drMidAddRow.Item(y) = RoundStandard(dNewFactor, 0)
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
                                    dNewFactor = dTotal * dFactor
                                    drMidAddRow.Item(y) = RoundStandard(dNewFactor, 0)
                                End If
                            Next y
                        End If
                    End If
                    If Not drMidAddRow Is Nothing Then
                        drMidAddRow = Nothing
                    End If
                Loop
            End Using

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Sub UpdateFeeAddFactorAmounts(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim dFactor As Decimal = 0
        Dim dTotal As Decimal = 0
        Dim dNewFactor As Decimal = 0
        Dim drFeeAddRow As DataRow = Nothing
        Dim drTotalsRow As DataRow = Nothing
        Dim sCovInfo() As String

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
                                drFeeAddRow.Item(y) = RoundStandard(dNewFactor, 4)
                            End If
                        Next y
                    Else
                        'check to see if it is an endorsement
                        drFeeAddRow = GetRow(FactorTable, oReader.Item("FactorName") & "-ENDORSE")

                        If Not drFeeAddRow Is Nothing Then
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewFactor = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                End If
                                If drFeeAddRow.Item(y) IsNot System.DBNull.Value Then

                                    'for this column loop through all of the rows and get a total of the endorsements and add that to the total
                                    'we can't just use the totals rows because it does not include -ENDORSE premium
                                    dTotal = 0
                                    For Each oRow As DataRow In FactorTable.Rows
                                        If oRow.Item(0).ToUpper = "TOTALS" Then
                                            Exit For
                                        End If
                                        If oRow.Item(0).ToString.ToUpper.Contains("-ENDORSE") Then
                                            If oRow.Item(y) IsNot System.DBNull.Value Then
                                                If IsNumeric(oRow(y)) Then
                                                    sCovInfo = FactorTable.Columns(y).ColumnName.ToString.Split("_")
                                                    If sCovInfo.Length > 1 Then
                                                        If Right(FactorTable.Columns(y).ColumnName.ToString.ToUpper, 1) = "D" And oPolicy.DwellingUnits.Item(0).DwellingAmt = 0 Then
                                                            'don't use this
                                                        ElseIf Right(FactorTable.Columns(y).ColumnName.ToString.ToUpper, 1) = "C" And oPolicy.DwellingUnits.Item(0).ContentsAmt = 0 Then
                                                            'don't use this
                                                        ElseIf Not PolicyContainsCov(oPolicy, sCovInfo(0), sCovInfo(1)) Then 'policy does not contain coverage 
                                                            'don't use this
                                                        Else
                                                            dTotal += oRow(y)
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next

                                    dFactor = CDec(drFeeAddRow.Item(y))
                                    dTotal = RoundStandard(CDec(dTotal + drTotalsRow(y)), 0)
                                    dNewFactor = dTotal * dFactor
                                    'no rounding
                                    drFeeAddRow.Item(y) = RoundStandard(dNewFactor, 4)
                                End If
                            Next y
                            'get the total for some endorsements in the flat factor column
                            For y As Integer = 1 To FactorTable.Columns.Count - 1
                                dNewFactor = 0
                                If FactorTable.Columns(y).ColumnName.ToUpper = "FACTORTYPE" Then
                                    Exit For
                                ElseIf FactorTable.Columns(y).ColumnName.ToUpper = "FLATFACTOR" Then
                                    'If drFeeAddRow.Item(y) IsNot System.DBNull.Value Then
                                    'don't set the factor so it will be whatever the last amount was
                                    'dFactor = CDec(drFeeAddRow.Item(y))
                                    'we need to total the flatfactor column to get the total of the endorsements
                                    dTotal = 0
                                    If Not drTotalsRow Is Nothing Then
                                        Dim oDataCol As DataColumn = FactorTable.Columns(y)
                                        If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                                            dTotal += RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                                        End If
                                    End If
                                    dNewFactor = dTotal * dFactor
                                    'no rounding
                                    drFeeAddRow.Item(y) = RoundStandard(dNewFactor, 4)
                                    'End If
                                End If
                            Next y
                        End If
                    End If
                    If Not drFeeAddRow Is Nothing Then
                        drFeeAddRow = Nothing
                    End If
                Loop
            End Using

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Sub

    Public Overridable Sub CheckMinPremAmounts(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)

        Dim dTotal As Decimal = 0
        Dim drTotalsRow As DataRow = Nothing
        Dim bUpdatePrem As Boolean = False
        Dim dMinPremAmt As Decimal = 0

        Try
            'assume false
            oPolicy.MinPremApplied = False
            drTotalsRow = GetRow(FactorTable, "Totals")

            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                If oCov.IsMarkedForDelete Then
                    For i As Integer = 1 To drTotalsRow.Table.Columns.Count - 1
                        If drTotalsRow.Table.Columns(i).ColumnName.ToUpper = oCov.CovGroup & "_" & oCov.Type Then
                            drTotalsRow(i) = 0.0
                        End If
                    Next i
                End If
            Next


            For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                If oDataCol.ColumnName.ToUpper = "FACTORTYPE" Then
                    Exit For
                End If
                If drTotalsRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                    If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                        drTotalsRow(oDataCol.ColumnName.ToString) = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                        dTotal += CDec(drTotalsRow(oDataCol.ColumnName.ToString))
                    End If
                End If
            Next

            If dTotal < 300 Then
                dMinPremAmt = 300
                bUpdatePrem = True
                oPolicy.MinPremApplied = True
            End If

            If bUpdatePrem Then
                'we need to update the premium to the minimum premium amount and allocate premium to coverages on a pro rata basis
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                    If Not oCov.IsMarkedForDelete Then
                        For i As Integer = 1 To drTotalsRow.Table.Columns.Count - 1
                            If drTotalsRow.Table.Columns(i).ColumnName.ToUpper = oCov.CovGroup & "_" & oCov.Type Then
                                'drTotalsRow(i) = dCovPremAmt
                                drTotalsRow(i) = dMinPremAmt * (drTotalsRow(i) / dTotal)
                            End If
                        Next i
                    End If
                Next
            End If

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not drTotalsRow Is Nothing Then
                drTotalsRow = Nothing
            End If
        End Try
    End Sub


    Public Overridable Sub GetTotalChgInPremPolFactors(ByVal oPolicy As clsPolicyHomeOwner)

        Try
            For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                    For Each oPremFactor As clsPremiumFactor In oCov.Factors
                        If oPolicyFactor.FactorCode = oPremFactor.FactorCode Then
                            'update FactorAmt - total change in premium
                            oPolicyFactor.FactorAmt = RoundStandard(oPolicyFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                        End If
                    Next
                Next
            Next
        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        End Try

    End Sub

    Public Overridable Sub GetTotalChgInPremEndorseFactors(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)

        Dim drEndorsementRow As DataRow = Nothing

        Try

            For Each oEndorseFactor As clsEndorsementFactor In oPolicy.EndorsementFactors
                If Not oEndorseFactor.IsMarkedForDelete Then
                    oEndorseFactor.FactorAmt = 0
                    drEndorsementRow = GetRow(oFactorTable, oEndorseFactor.FactorCode & "-ENDORSE")
                    If Not drEndorsementRow Is Nothing Then
                        For Each oDataCol As DataColumn In oFactorTable.Columns
                            If IsNumeric(drEndorsementRow(oDataCol.ColumnName.ToString)) Then
                                If oEndorseFactor.CovType = Right(oDataCol.ColumnName, 1) Or oEndorseFactor.CovType = "N" Then
                                    oEndorseFactor.FactorAmt += drEndorsementRow(oDataCol.ColumnName.ToString)
                                End If
                            End If
                        Next
                    End If

                    If Not drEndorsementRow Is Nothing Then
                        oEndorseFactor.FactorAmt = GetMinPremEndorsement(oEndorseFactor, drEndorsementRow("FactorType").ToString())
                    Else
                        oEndorseFactor.FactorAmt = GetMinPremEndorsement(oEndorseFactor, "")
                    End If


                    Dim dHO160Sum As Decimal = 0.0
                    For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                        Select Case oEndorseFactor.FactorCode
                            Case "HO160"
                                Select Case oProperty.PropertyCategoryDesc.ToUpper
                                    Case "JEWELRY", "FINE ARTS", "FINE ARTS WITH BREAKAGE", "FINE ARTS - BREAKAGE", "STAMPS", "COINS", "FIREARMS", "FURS", "CAMERAS", "CAMERA, FILMS AND RELATED", "MUSICAL INSTRUMENTS", "PROFESSIONAL MUSICAL INSTRUMENTS", "SILVERWARE", "GOLF EQUIPMENT", "GOLFER'S EQUIPMENT"
                                        Dim dPercentageofTotal As Decimal

                                        If oEndorseFactor.Limit > 0 Then
                                            dPercentageofTotal = oProperty.PropertyAmt / oEndorseFactor.Limit

                                            If RoundStandard(oEndorseFactor.FactorAmt * dPercentageofTotal, 2) + dHO160Sum > oEndorseFactor.FactorAmt Then
                                                oProperty.PropertyPremiumAmt += oEndorseFactor.FactorAmt - dHO160Sum
                                            Else
                                                oProperty.PropertyPremiumAmt += RoundStandard(oEndorseFactor.FactorAmt * dPercentageofTotal, 2)
                                            End If

                                            dHO160Sum += oProperty.PropertyPremiumAmt
                                        End If
                                End Select
                        End Select
                    Next
                End If
            Next
        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        End Try

    End Sub

    Public Overridable Function GetMinPremEndorsement(ByVal oEndorseFactor As clsEndorsementFactor, ByVal sFactorType As String) As Decimal
    End Function

    Public Overridable Function GetPremiums(ByRef oPolicy As clsPolicyHomeOwner, ByVal drTotalsRow As DataRow) As Boolean

        Try
            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                    If oDataCol.ColumnName.ToUpper = oCov.CovGroup.ToUpper & "_" & oCov.Type Then
                        oCov.FullTermPremium = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                    End If
                Next
            Next
        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
        End Try
    End Function

    Protected Sub LogItems(ByVal oPolicy As clsPolicyHomeOwner)

        Dim sLogging As String = ""

        Try
            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                'log it
                moLog = New ImperialFire.clsLogItem
                moLog.Title = "COVERAGE INFO: "
                sLogging = ""
                sLogging += "CovGroup: " & oCov.CovGroup & vbCrLf
                sLogging += "Type: " & oCov.Type & vbCrLf
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
                    sLogging += "oPremFactor.Type: " & oPremFactor.Type & vbCrLf
                    sLogging += "oPremFactor.FactorAmt - Premium Change Amount: " & oPremFactor.FactorAmt & vbCrLf
                    moLog.Description = sLogging
                    moLogging.LogItems.Add(moLog)
                    If Not moLog Is Nothing Then
                        moLog = Nothing
                    End If
                Next
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
        End Try
    End Sub

    Public Overridable Sub CleanDataTable(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)

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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
        End Try
    End Sub

    Private Sub GetCoverageList(ByRef oCovs As Dictionary(Of String, String), ByRef oProg As Dictionary(Of String, String), ByVal sProgram As String, ByVal oPolicy As clsPolicyHomeOwner)

        Dim sSql As String = ""
        Dim lRow As Long = 0
        Dim sCoverage As String = ""
        Dim dtRateDate As Date = oPolicy.RateDate
        Dim sAppliesToCode As String = oPolicy.AppliesToCode
        Dim oReader As SqlDataReader = Nothing

        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorBaseRate with(nolock)"
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

                    sCoverage = (oReader.Item("Coverage") & "_" & oReader.Item("Type"))
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
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

    Public Function IsFlatFactorEndorsement(ByVal oPolicy As clsPolicyHomeOwner, ByVal sEndorsementCode As String, ByVal oStateInfoDataSet As DataSet) As Boolean

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim bIsFlatFactorEndorsement As Boolean = False

        Try

            Dim DataRows() As DataRow
            Dim oStateInfoTable As DataTable = Nothing
            oStateInfoTable = oStateInfoDataSet.Tables(0)

            DataRows = oStateInfoTable.Select("Program IN ('HOM', '" & oPolicy.Program & "') AND ItemGroup='FLAT' AND ItemCode='ENDORSEMENT' ")

            For Each oRow As DataRow In DataRows
                If sEndorsementCode.ToUpper = oRow.Item("ItemValue").ToString.ToUpper Then
                    bIsFlatFactorEndorsement = True
                End If
            Next

            Return bIsFlatFactorEndorsement
        Catch ex As Exception
        Finally
        End Try
    End Function

    Public Overridable Sub SetLimitAmounts(ByVal oPolicy As clsPolicyHomeOwner)
    End Sub

    Public Function DedOnPolicy(ByVal oPolicy As clsPolicyHomeOwner, ByVal sFactorCode As String) As Boolean

        With oPolicy.DwellingUnits.Item(0)
            Select Case sFactorCode.ToUpper
                Case "DED1"
                    If .Ded1 <> 0 Then
                        Return True
                    End If
                Case "DED2"
                    If .Ded2 <> 0 Then
                        Return True
                    End If
                Case "DED3"
                    If .Ded3 <> 0 Then
                        Return True
                    End If
                Case "EC"
                    If .Ded1 <> 0 Then
                        Return True
                    End If
            End Select
            
        End With
        Return False
    End Function

End Class
