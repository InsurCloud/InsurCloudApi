Imports Microsoft.VisualBasic
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports CorPolicy.clsCommonFunctions
Imports System.Collections.Generic

Public Class clsPgm217
    Inherits clsPgm2

    Public Overrides Sub CheckCalculatedFee(ByRef oPolicy As CorPolicy.clsPolicyPPA, ByRef oFee As CorPolicy.clsBaseFee)

        If oPolicy.Program.ToUpper = "Monthly" Then
            If oFee.FeeCode = "POLICY" And oFee.FeeAmt < 1D Then
                oFee.FeeAmt = IIf(oPolicy.FullTermPremium * oFee.FeeAmt > 20, 20, IIf(oPolicy.FullTermPremium * oFee.FeeAmt < 8, 8, oPolicy.FullTermPremium * oFee.FeeAmt))
            End If
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

End Class
