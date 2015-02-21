Imports Microsoft.VisualBasic
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports CorPolicy.clsCommonFunctions
Imports System.Collections.Generic

Public Class clsPgm235
    Inherits clsPgm2

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


    Public Overrides Sub CheckCalculatedFee(ByRef oPolicy As CorPolicy.clsPolicyPPA, ByRef oFee As CorPolicy.clsBaseFee)

        If oFee.FeeCode = "MVR" Then

            Dim iNumOfMVROrders As Integer = 0
            Dim iDriverIndexNum As Integer = 0

            iDriverIndexNum = oFee.FeeDesc.Substring(oFee.FeeDesc.IndexOf("Driver #") + 8)


            For Each oDriver As clsEntityDriver In oPolicy.Drivers
                If oDriver.IndexNum = iDriverIndexNum Then
                    If Not oDriver.IsMarkedForDelete Then
                        If oDriver.MVROrderStatus.ToUpper = "CLEAR" Or oDriver.MVROrderStatus.ToUpper = "NOTCLEAR" Then
                            iNumOfMVROrders += 1
                        End If
                    End If
                End If
            Next


            oFee.FeeAmt = oFee.FeeAmt * IIf(iNumOfMVROrders = 0, 1, iNumOfMVROrders)
        End If

        Dim iNumOfSR22s As Integer = 0
        For Each oDriver As clsEntityDriver In oPolicy.Drivers
            If Not oDriver.IsMarkedForDelete Then
                If oDriver.SR22 Then
                    iNumOfSR22s += 1
                End If
            End If
        Next
        If oFee.FeeCode = "SR22" Then
            oFee.FeeAmt = oFee.FeeAmt * IIf(iNumOfSR22s = 0, 1, iNumOfSR22s)
        End If

    End Sub

    Public Overrides Function ConvertCoverages(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
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

    Public Overrides Function ConvertDriverFactors(ByVal oPolicy As clsPolicyPPA, ByVal sRatedProgram As String) As clsPolicyPPA
        Dim oConvertedPolicy As clsPolicyPPA = oPolicy
        Dim DataRows() As DataRow
        Dim oFactorDriverTable As DataTable = Nothing
        Dim bKeepFactor As Boolean = False

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

    Public Overrides Function VehContainsCov(ByVal oPolicy As clsPolicyPPA, ByVal sCov As String) As Boolean

        Dim bVehContainsCov As Boolean = False
        Dim oVeh As clsVehicleUnit = GetRatedVehicle(oPolicy)

        ' For OK, UUMBI is only applied to the first vehicle
        ' For all others, return False so that it is not applied
        If sCov.ToUpper = "UUMBI" Then
            If Not IsFirstRatedVeh() And UseFirstRatedVehUUMBI(oPolicy) Then
                Return False
            End If
        End If

        For Each oCov As clsBaseCoverage In oVeh.Coverages
            If oCov.CovGroup.ToUpper = sCov.ToUpper Then
                bVehContainsCov = True
                Exit For
            End If
        Next

        Return bVehContainsCov

    End Function

    Public Function UseFirstRatedVehUUMBI(ByVal oPolicy As clsPolicyPPA) As Boolean
        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        oStateInfoTable = moStateInfoDataSet.Tables(0)

        'get coverages and limits for newly selected program
        DataRows = oStateInfoTable.Select("Program In ('All','" & oPolicy.Program & "') AND ItemCode='RULES' AND ItemSubCode='UUMBI' AND ItemValue='VEHONEONLY'")

        If DataRows.Length > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Overrides Function dbGetCoverageFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable, ByVal sType As String) As System.Data.DataRow
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
                                oCov.CovLimit = CovValue
                            End If
                    End Select
                End If
            Next

            If oPolicy.Program.ToUpper = "MONTHLY" Then
                Dim oOTCCoverage As clsPACoverage = GetCoverage("OTC", oPolicy)
                If Not oOTCCoverage Is Nothing Then
                    dMonthlyOTCFactor = CalculateCovFactor(oOTCCoverage.CovGroup, oOTCCoverage.CovDeductible, oPolicy)
                End If
                Dim oCOLCoverage As clsPACoverage = GetCoverage("COL", oPolicy)
                If Not oCOLCoverage Is Nothing Then
                    dMonthlyCOLFactor = CalculateCovFactor(oCOLCoverage.CovGroup, oCOLCoverage.CovDeductible, oPolicy)
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
                'If oPolicy.Program.ToUpper = "SUMMIT" Then
                cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = oPolicy.PolicyInsured.UWTier
                'Else
                'cmd.Parameters.Add("@UWTier", SqlDbType.VarChar, 3).Value = 1
                'End If
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
                                If oReader.Item("Coverage").ToString.ToUpper = oCov.CovGroup.ToUpper Then
                                    If oPolicy.Program.ToUpper = "MONTHLY" And (oCov.CovGroup.ToUpper = "OTC" Or oCov.CovGroup.ToUpper = "COL") Then
                                        If oCov.CovGroup.ToUpper = "OTC" Then
                                            drFactorRow.Item(oReader.Item("Coverage")) = dMonthlyOTCFactor
                                        Else
                                            drFactorRow.Item(oReader.Item("Coverage")) = dMonthlyCOLFactor
                                        End If
                                        Exit For
                                    Else
                                        If oReader.Item("Code") = oCov.CovCode Then
                                            'add it to the data row
                                            drFactorRow.Item(oReader.Item("Coverage")) = oReader.Item("Factor")
                                            Exit For
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

End Class
