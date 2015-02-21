Imports Microsoft.VisualBasic
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports CorPolicy.clsCommonFunctions
Imports System.Collections.Generic

Public Class clsPgm242
    Inherits clsPgm2

    Public Overrides Sub LoadFees(ByVal oPolicy As clsPolicyPPA)

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
                            For Each oDrv As clsEntityDriver In oPolicy.Drivers
                                If oDrv.IsMarkedForDelete = False And oDrv.DriverStatus.ToUpper = "ACTIVE" Then
                                    If oDrv.SR22 Then
                                        oFee = New clsBaseFee
                                        oFee.FeeCode = oReader.Item("FeeCode").ToString
                                        oFee.FeeDesc = oReader.Item("Description").ToString
                                        oFee.FeeName = oReader.Item("Description").ToString
                                        oFee.FeeType = "P"
                                        oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
                                        oFee.FeeNum = oPolicy.Fees.Count + 1
                                        oFee.IndexNum = oPolicy.Fees.Count + 1

                                        If Not oFee Is Nothing Then
                                            oPolicy.Fees.Add(oFee)
                                            oFee = Nothing
                                        End If
                                    End If
                                End If
                            Next
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

    Public Overrides Function dbGetPolicyDiscountMatrixFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
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

                Dim sHomeOwner As String = String.Empty
                sHomeOwner = IIf(oPolicy.PolicyInsured.OccupancyType.ToUpper = "HOMEOWNER", "Y", "N")

                If sHomeOwner = "N" And (oPolicy.Program.ToUpper = "CLASSIC" Or oPolicy.Program.ToUpper = "DIRECT") Then
                    sHomeOwner = IIf(oPolicy.PolicyInsured.OccupancyType.ToUpper = "MOBILEHOMEOWNER", "Y", "N")
                End If
                cmd.Parameters.Add("@HomeOwner", SqlDbType.VarChar, 1).Value = sHomeOwner

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



    Public Overrides Sub CheckCalculatedFee(ByRef oPolicy As clsPolicyPPA, ByRef oFee As clsBaseFee)
        Dim iVehCount As Integer = 0

        If oFee.FeeCode = "THEFT" Then
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete Then
                    iVehCount += 1
                End If
            Next

            oFee.FeeAmt = oFee.FeeAmt * iVehCount
        End If

    End Sub

    Public Overrides Sub GetMidAddPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
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

                                    'dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")

                                            oCov.Factors.Add(oPremFactor)
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Overrides Sub UpdateMidAddFactorAmounts(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)

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
                                    'dTotal = RoundStandard(CDec(drTotalsRow.Item(y)), 0)
                                    dTotal = CDec(drTotalsRow.Item(y))
                                    dNewFactor = dTotal * dFactor
                                    'drMidAddRow.Item(y) = RoundStandard(dNewFactor, 0)
                                    drMidAddRow.Item(y) = dNewFactor
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
                                        'dTotal = RoundStandard(CDec(drTotalsRow.Item(y)), 0)
                                        dTotal = CDec(drTotalsRow.Item(y))
                                        dNewFactor = dTotal * dFactor
                                        'drMidAddRow.Item(y) = RoundStandard(dNewFactor, 0)
                                        drMidAddRow.Item(y) = dNewFactor
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
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try
    End Sub

    Public Overrides Sub GetMidMultPremium(ByRef oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable)
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

                                    'dNewTotal = RoundStandard(dNewTotal, 3)

                                    FactorTable.Rows(FactorTable.Rows.Count - 1).Item(y) = dNewTotal

                                    For p As Integer = 0 To oVeh.Coverages.Count - 1
                                        If FactorTable.Columns(y).ColumnName = oVeh.Coverages.Item(p).CovGroup Then

                                            Dim oCov As clsPACoverage = oVeh.Coverages.Item(p)
                                            Dim oPremFactor As New clsPremiumFactor
                                            oPremFactor.FactorAmt = dNewTotal - dPrevTotal 'change in premium
                                            oPremFactor.FactorCode = oReader.Item("FactorName")
                                            oPremFactor.FactorName = oReader.Item("FactorName")

                                            oCov.Factors.Add(oPremFactor)
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
                'drTotalsRow = GetRow(FactorTable, "Totals")
                'For Each oDataCol As DataColumn In drTotalsRow.Table.Columns
                '    If IsNumeric(drTotalsRow(oDataCol.ColumnName.ToString)) Then
                '        drTotalsRow(oDataCol.ColumnName.ToString) = RoundStandard(CDec(drTotalsRow(oDataCol.ColumnName.ToString)), 0)
                '    End If
                'Next

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

    Public Overrides Sub GetTotalChgInPremPolFactors(ByVal oPolicy As clsPolicyPPA)

        Try
            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
                'initialize
                oPolicyFactor.FactorAmt = 0
                For Each oCov As clsPACoverage In oVeh.Coverages
                    For Each oPremFactor As clsPremiumFactor In oCov.Factors
                        If oPolicyFactor.FactorCode = oPremFactor.FactorCode Then
                            'update FactorAmt - total change in premium
                            oPolicyFactor.FactorAmt = RoundStandard(oPolicyFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                        End If
                    Next
                Next
            Next

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
            If oDriver.IndexNum < 98 Then
                For Each oDriverFactor As clsBaseFactor In oDriver.Factors
                    'initialize
                    oDriverFactor.FactorAmt = 0
                    For Each oCov As clsPACoverage In oVeh.Coverages
                        For Each oPremFactor As clsPremiumFactor In oCov.Factors
                            If oDriverFactor.FactorCode = oPremFactor.FactorCode Then
                                'update FactorAmt - total change in premium
                                If oDriverFactor.FactorCode = "FOREIGN_LICENSE" Then
                                    oDriverFactor.FactorAmt = oDriverFactor.FactorAmt + oPremFactor.FactorAmt
                                Else
                                    oDriverFactor.FactorAmt = RoundStandard(oDriverFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                                End If
                            End If
                        Next
                    Next
                Next
            End If

            For Each oVehFactor As clsBaseFactor In oVeh.Factors
                'initialize
                oVehFactor.FactorAmt = 0
                For Each oCov As clsPACoverage In oVeh.Coverages
                    For Each oPremFactor As clsPremiumFactor In oCov.Factors
                        If oVehFactor.FactorCode = oPremFactor.FactorCode Then
                            'update FactorAmt - total change in premium
                            oVehFactor.FactorAmt = RoundStandard(oVehFactor.FactorAmt + oPremFactor.FactorAmt, 0)
                        End If
                    Next
                Next
            Next

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        End Try

    End Sub

    Public Overrides Function dbGetTerritory(ByVal oPolicy As clsPolicyPPA) As String
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
                            If oPolicy.VehicleUnits(v).County.Length > 0 Then
                                ' lookup based on the county
                                DataRows = oCodeTerritoryDefinitionsTable.Select("Program = '" & oPolicy.Program & "'" & " AND Zip = '" & oPolicy.VehicleUnits(v).Zip & "' AND County = '" & oPolicy.VehicleUnits(v).County.ToUpper.Trim & "'")

                                ' if we can't find by county, use the old method
                                If DataRows.Length = 0 Then
                                    DataRows = oCodeTerritoryDefinitionsTable.Select("Program = '" & oPolicy.Program & "'" & " AND Zip = '" & oPolicy.VehicleUnits(v).Zip & "'")
                                End If

                            Else
                                DataRows = oCodeTerritoryDefinitionsTable.Select("Program = '" & oPolicy.Program & "'" & " AND Zip = '" & oPolicy.VehicleUnits(v).Zip & "'")
                            End If
                            
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

End Class
