Imports System.Data
Imports System.Data.Sql
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Configuration
Imports CorPolicy

Public Class CommonRulesFunctions
    Public Shared Function HasForeignLicense(ByVal oDriver As clsEntityDriver) As Boolean

        Select Case oDriver.DLNState
            Case "FN", "IT", "VI", "AS", "FM", "GU", "MH", "MP", "PR", "PW", "ON", "AE", "AP", "AA", "JZ"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Shared Function AllowCode(ByVal FunctionOrCodeName As String) As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet
        Dim allow As Boolean = False

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT ItemValue "
                sSql = sSql & " FROM Common..StateInfo with(nolock)"
                sSql = sSql & " WHERE EffDate <= @Date "
                sSql = sSql & " AND ExpDate > @Date "
                sSql = sSql & " AND AppliesToCode IN ('B') "
                sSql = sSql & " AND ItemGroup = 'ALLOW' "
                sSql = sSql & " AND ItemCode = 'CODE' "
                sSql = sSql & " AND ItemSubCode = @FunctionName "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = Now()
                cmd.Parameters.Add("@FunctionName", SqlDbType.VarChar, 50).Value = FunctionOrCodeName

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "StateInfo")

                If oDS.Tables(0).Rows.Count > 0 Then
                    allow = True
                End If
            End Using

        Catch ex As Exception
            Throw New Exception("AllowCode(" & FunctionOrCodeName & ") failed: " & ex.Message, ex)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
        Return allow
    End Function

    Public Shared Function StateInfoContains(ByVal group As String, ByVal code As String, ByVal subcode As String, ByVal program As String, ByVal AppliesToCode As String, Optional ByVal sProgram As String = "") As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim oDS As New DataSet
        Dim allow As Boolean = False

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT ItemValue "
                sSql = sSql & " FROM pgm" & program & "..StateInfo with(nolock)"
                sSql = sSql & " WHERE EffDate <= @Date "
                sSql = sSql & " AND ExpDate > @Date "
                sSql = sSql & " AND AppliesToCode IN ('B',@AppliesToCode) "
                sSql = sSql & " AND ItemGroup = @Group "
                sSql = sSql & " AND ItemCode = @Code "
                sSql = sSql & " AND ItemSubCode = @SubCode"
                If Len(sProgram) > 0 Then
                    sSql = sSql & " AND Program IN ('ALL', @Program )"
                End If

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = Now()
                cmd.Parameters.Add("@Group", SqlDbType.VarChar, 50).Value = group
                cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code
                cmd.Parameters.Add("@SubCode", SqlDbType.VarChar, 50).Value = subcode
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = AppliesToCode
                If Len(sProgram) > 0 Then
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = sProgram
                End If

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "StateInfo")

                If oDS.Tables(0).Rows.Count > 0 Then
                    allow = True
                End If
            End Using

        Catch ex As Exception
            Throw New Exception("StateInfoContains(" & group & ", " & code & ", " & subcode & ", " & program & ", " & AppliesToCode & ") failed: " & ex.Message, ex)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
        Return allow
    End Function
    Public Shared Function LoadStateInfoTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        oConn.Open()

        Try
            Using cmd As New SqlCommand(sSql, oConn)
                sSql = " SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue "
                sSql &= " FROM pgm" & sProduct & sStateCode & "..StateInfo with(nolock)"
                sSql &= " WHERE EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql &= " ORDER BY Program, ItemGroup, ItemCode "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = sAppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)
                adapter.Fill(oDS, "StateInfo")
                Return oDS
            End Using
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace)
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Function
End Class
