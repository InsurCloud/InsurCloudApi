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

Public Class clsPgm202
    Inherits clsPgm2

    Public Overrides Sub CheckCalculatedFee(ByRef oPolicy As clsPolicyPPA, ByRef oFee As clsBaseFee)
        Dim iVehCount As Integer = 0

        If oFee.FeeCode = "THEFT" And oFee.FeeAmt < 1D Then
            For Each oVeh As clsVehicleUnit In oPolicy.VehicleUnits
                If Not oVeh.IsMarkedForDelete Then
                    iVehCount += 1
                End If
            Next

            oFee.FeeAmt = oFee.FeeAmt * iVehCount
        End If

    End Sub

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
                            Dim bHasSR22 As Boolean = False
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
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Sub


    Public Overrides Function dbGetDriverClassFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim iDriverAge As Integer = 0

        Try

            Dim oDriver As clsEntityDriver = GetAssignedDriver(oPolicy)
            iDriverAge = oDriver.Age
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

    Public Overrides Function dbGetHouseholdStructureFactor(ByVal oPolicy As clsPolicyPPA, ByVal FactorTable As DataTable) As System.Data.DataRow
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
                cmd.Parameters.Add("@HomeOwner", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.OccupancyType.ToUpper.Contains("HOMEOWNER"), "Y", "N")
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
                cmd.Parameters.Add("@HomeOwner", SqlDbType.VarChar, 1).Value = IIf(oPolicy.PolicyInsured.OccupancyType.ToUpper.Contains("HOMEOWNER"), "Y", "N")

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
