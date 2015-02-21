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

Public Class clsPgm209
    Inherits clsPgm2

    Public Overrides Function dbGetModelYearFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim lVehYear As Long = 0

        Try

            Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

            ' FL Goes off of the vehicle age instead of the year
            lVehYear = oVeh.VehicleAge

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorModelYear with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND ModelYear = @ModelYear "
                sSql = sSql & " ORDER BY Coverage Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@ModelYear", SqlDbType.VarChar, 4).Value = lVehYear

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


    Public Overrides Function dbGetDiscountFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim drDiscountNoMaxRow As DataRow = Nothing
        Dim drDiscountMaxRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing
        Dim sMaxDiscountFactors As New List(Of String)

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
                                    If sMaxDiscountFactors.Contains(oReader("FactorCode")) Then
                                        drDiscountMaxRow(oReader("Coverage")) = CDec(1 - ((CDec(1 - CDec(drDiscountMaxRow(oReader("Coverage"))))) + (CDec(1 - CDec(oReader("Factor"))))))
                                    Else
                                        drDiscountNoMaxRow(oReader("Coverage")) = CDec(1 - ((CDec(1 - CDec(drDiscountNoMaxRow(oReader("Coverage"))))) + (CDec(1 - CDec(oReader("Factor"))))))
                                    End If
                                    Exit For
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

            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
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

    Public Overrides Function dbGetDriverClassFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim bBusinessUse As Boolean = False
        Dim sDriverClass As String = ""

        Try
            Dim oVehicle As clsVehicleUnit = GetRatedVehicle(oPolicy)
            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)

            For Each oFactor As clsBaseFactor In oVehicle.Factors
                If oFactor.FactorCode.ToUpper.Trim = "BUS_USE" Then
                    bBusinessUse = True
                    Exit For
                End If
            Next

            If bBusinessUse Then
                sDriverClass = "BUSI"
            Else
                Dim iDriverAge As Integer = 0
                iDriverAge = oDriver.Age

                ' To enable add this  row
                ' insert into pgmXXX..stateinfo values('All','RATE','WIDOW','MARRIED','TRUE','B','1/1/2010','12/31/2050','kevin.bowser',getdate())
                If StateInfoContains("RATE", "WIDOW", "MARRIED", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                    sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED" Or oDriver.MaritalStatus.Trim.ToUpper = "WIDOWED", "M", "S")
                Else
                    ' Default, rate widow as single
                    sDriverClass &= IIf(oDriver.MaritalStatus.Trim.ToUpper = "MARRIED", "M", "S")
                End If

                sDriverClass &= IIf(oDriver.Gender.Trim.ToUpper.StartsWith("M"), "M", "F")
                If iDriverAge > 99 Then iDriverAge = 99
                sDriverClass &= iDriverAge
            End If

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
                        Case "CAT"
                            oFee = New clsBaseFee
                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                            oFee.FeeDesc = oReader.Item("Description").ToString
                            oFee.FeeName = oReader.Item("Description").ToString
                            oFee.FeeType = "P"
                            oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
                            oFee.FeeNum = oPolicy.Fees.Count + 1
                            oFee.IndexNum = oPolicy.Fees.Count + 1
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
                        Case "INSTALLFE"
							If oPolicy.PayPlanCode <> "100" And Not oPolicy.ApplyPIFDiscount Then
								oFee = New clsBaseFee
								oFee.FeeCode = oReader.Item("FeeCode").ToString
								oFee.FeeDesc = oReader.Item("Description").ToString
								oFee.FeeName = oReader.Item("Description").ToString
								oFee.FeeType = "P"
								oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
								oFee.FeeNum = oPolicy.Fees.Count + 1
								oFee.IndexNum = oPolicy.Fees.Count + 1
							End If
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
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
          
        End Try

    End Sub

    Public Overrides Function UpdateMidAddFactorBasedOnTerm(ByVal oPolicy As clsPolicyPPA, ByVal dFactorAmt As Decimal) As Decimal
        If oPolicy.Term = 6 Then
            Return RoundStandard(dFactorAmt / 2, 0)
        Else
            Return dFactorAmt
        End If

    End Function

    Public Overrides Sub CheckCalculatedFee(ByRef oPolicy As CorPolicy.clsPolicyPPA, ByRef oFee As CorPolicy.clsBaseFee)

        If oFee.FeeCode = "CAT" And oFee.FeeAmt < 1D Then
            oFee.FeeAmt = RoundStandard(oPolicy.FullTermPremium * oFee.FeeAmt, 2)
        End If
    End Sub

End Class
