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

Public Class clsPgm203
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

End Class
