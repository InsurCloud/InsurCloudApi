Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports CorPolicy.clsCommonFunctions
Imports CorPolicy

Public MustInherit Class clsPgm

    Protected moLogging As ImperialFire.clsLogging
    Protected moLog As ImperialFire.clsLogItem = Nothing
    Protected moConn As SqlConnection
    Protected moStateInfoDataSet As DataSet

#Region "Properties"

#End Region

    Protected Overridable Sub BeginLogging(ByVal oLogging As ImperialFire.clsLogging, ByVal oPolicy As clsBasePolicy, ByVal oFactorTable As DataTable)
        'log it
        Select Case Val(oPolicy.Product)
            Case 1
                moLogging = New ImperialFire.clsLogging1
                CType(moLogging, ImperialFire.clsLogging1).Policy = oPolicy
            Case 2
                moLogging = New ImperialFire.clsLogging2
                CType(moLogging, ImperialFire.clsLogging2).Policy = oPolicy
            Case 4
                moLogging = New ImperialFire.clsLogging4
                CType(moLogging, ImperialFire.clsLogging4).Policy = oPolicy
            Case Else
                moLogging = New ImperialFire.clsLogging1
                CType(moLogging, ImperialFire.clsLogging1).Policy = oPolicy
        End Select

        moLogging.StartTimeStamp = now()

        AddLog("Starting Rate", "Starting Rate")

        If Not oFactorTable Is Nothing Then
            moLogging.DataTable.Add(oFactorTable)
        End If

    End Sub

    Protected Overridable Sub AddLog(ByVal sDescription As String, ByVal sTitle As String)
        moLog = New ImperialFire.clsLogItem
        moLog.Description = sDescription & ": " & Now & Now.Millisecond & vbCrLf
        moLog.Title = sTitle
        moLogging.LogItems.Add(moLog)
    End Sub

    Protected Overridable Sub FinishLogging(ByVal bLogRate As Boolean)
        'log it
        AddLog("Finishing Rate", "Finishing Rate")        

    End Sub


    Protected Overridable Sub ErrorLogging(ByVal sMethodName As String, ByVal sMessage As String)
        'log it
        moLog = New ImperialFire.clsLogItem
        moLog.Description = sMessage & Now & Now.Millisecond & vbCrLf
        moLog.Title = "Error " & sMethodName
        moLogging.LogItems.Add(moLog)

        If Not moLog Is Nothing Then
            moLog = Nothing
        End If
        

    End Sub

    Public Overridable Sub InitializeConnection()
        moConn = New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
        moConn.Open()
    End Sub


    Public Overridable Function LookUpSubCode(ByVal oFactor As clsEndorsementFactor) As String
        'overridden by each state
        Return ""
    End Function

    Public Overridable Function CalculateEndorsementFactor(ByVal oReader As SqlDataReader, ByVal oFactor As clsEndorsementFactor, ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As Decimal
        'overridden by each state
        Return 0D
    End Function

    Protected Overridable Function CreateDataTable(ByVal sTableName As String, ByVal sProgram As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String, ByVal sProduct As String, ByVal sStateCode As String) As DataTable

        Dim sSql As String = ""
        Dim oFactorTable As DataTable = Nothing
        Dim oReader As SqlDataReader = Nothing
        Dim dcFactorName As DataColumn = Nothing
        Dim dcFactorType As DataColumn = Nothing
        Dim sType As String = ""

        Try
            'Create Data Table
            'oFactorTable = New DataTable("Factors")
            oFactorTable = New DataTable(sTableName)

            dcFactorName = New DataColumn("FactorName")
            oFactorTable.Columns.Add(dcFactorName)

            'add coverage columns using FactorBaseRate as master coverage list
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT DISTINCT(Coverage) FROM pgm" & sProduct & sStateCode & ".." & "FactorBaseRate with(nolock)"
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
                    Select Case sProduct
                        Case "1" 'HomeOwners
                            'loop through coverages and add a column for each with each type
                            For i As Integer = 0 To 2
                                Select Case i
                                    Case 0
                                        sType = "D"
                                    Case 1
                                        sType = "C"
                                    Case 2
                                        sType = "N"
                                    Case Else
                                        sType = ""
                                End Select

                                Dim dcCov As DataColumn = New DataColumn(oReader.Item("Coverage") & "_" & sType)
                                oFactorTable.Columns.Add(dcCov)
                                If Not dcCov Is Nothing Then
                                    dcCov.Dispose()
                                    dcCov = Nothing
                                End If
                            Next i
                        Case "2" 'Personal Auto
                            'loop through coverages and add a column for each
                            Dim dcCov As DataColumn = New DataColumn(oReader.Item("Coverage"))
                            oFactorTable.Columns.Add(dcCov)
                            If Not dcCov Is Nothing Then
                                dcCov.Dispose()
                                dcCov = Nothing
                            End If
                        Case "3" 'Commercial Auto

                        Case "4" 'Flood
                    End Select
                Loop

            End Using

            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Dim dcFlat As DataColumn = New DataColumn("FlatFactor")
            oFactorTable.Columns.Add(dcFlat)
            If Not dcFlat Is Nothing Then
                dcFlat.Dispose()
                dcFlat = Nothing
            End If

            dcFactorType = New DataColumn("FactorType")
            oFactorTable.Columns.Add(dcFactorType)

            Return oFactorTable

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not dcFactorName Is Nothing Then
                dcFactorName.Dispose()
                dcFactorName = Nothing
            End If
            If Not dcFactorType Is Nothing Then
                dcFactorType.Dispose()
                dcFactorType = Nothing
            End If
            If Not oFactorTable Is Nothing Then
                oFactorTable.Dispose()
                oFactorTable = Nothing
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Function

    Protected Overridable Function CreateTotalsRow(ByVal FactorTable As DataTable) As System.Data.DataRow

        Dim drFactorRow As DataRow = Nothing

        Try
            drFactorRow = FactorTable.NewRow

            drFactorRow.Item("FactorName") = "Totals"

            For i As Integer = 1 To FactorTable.Columns.Count - 1
                'add it to the data row
                If FactorTable.Columns(i).ColumnName.ToUpper = "FACTORTYPE" Then
                    drFactorRow.Item(i) = "Premium"
                    Exit For
                End If
                drFactorRow.Item(i) = 0
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

    Protected Overridable Function CreateFeesTable() As DataTable

        Dim oFeeTable As DataTable = Nothing
        Dim dcFeeApplicationType As DataColumn = Nothing
        Dim dcFactorType As DataColumn = Nothing
        Dim dcFeeCode As DataColumn = Nothing
        Dim dcFactor As DataColumn = Nothing

        Try
            oFeeTable = New DataTable("Fees")
            dcFeeCode = New DataColumn("FeeCode")
            oFeeTable.Columns.Add(dcFeeCode)

            dcFeeApplicationType = New DataColumn("FeeApplicationType")
            oFeeTable.Columns.Add(dcFeeApplicationType)

            dcFactor = New DataColumn("Factor")
            oFeeTable.Columns.Add(dcFactor)
            dcFactorType = New DataColumn("FactorType")
            oFeeTable.Columns.Add(dcFactorType)

            Return oFeeTable

        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not dcFactorType Is Nothing Then
                dcFactorType.Dispose()
                dcFactorType = Nothing
            End If
            If Not dcFeeCode Is Nothing Then
                dcFeeCode.Dispose()
                dcFeeCode = Nothing
            End If
            If Not dcFeeApplicationType Is Nothing Then
                dcFeeApplicationType.Dispose()
                dcFeeApplicationType = Nothing
            End If
            If Not dcFactor Is Nothing Then
                dcFactor.Dispose()
                dcFactor = Nothing
            End If
            If Not oFeeTable Is Nothing Then
                oFeeTable.Dispose()
                oFeeTable = Nothing
            End If
        End Try

    End Function

    Public Overridable Sub AddPolicyFactor(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String)
        Dim oPF As New CorPolicy.clsBaseFactor
        oPF.FactorCode = sFactorCode
        oPF.IndexNum = oPolicy.PolicyFactors.Count + 1
        oPF.SystemCode = sFactorCode
        oPF.FactorNum = oPolicy.PolicyFactors.Count + 1
        oPF.FactorAmt = 0
        'oPF.FactorDesc = GetFactorDesc(oPolicy, sFactorCode)
        oPF.FactorName = oPF.FactorDesc
        oPolicy.PolicyFactors.Add(oPF)
    End Sub

    Public Shared Function GetEndorsement(ByVal oPolicy As clsBasePolicy, ByVal sEndorsementCode As String) As clsEndorsementFactor

        For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
            If oEndorse.HasSubCode Then
                If Left(oEndorse.FactorCode.ToString.ToUpper, 5) = sEndorsementCode.ToString.ToUpper Then
                    Return oEndorse
                    Exit For
                End If
            Else
                If oEndorse.FactorCode.ToString.ToUpper = sEndorsementCode.ToString.ToUpper Then
                    Return oEndorse
                    Exit For
                End If
            End If
        Next

        Return Nothing

    End Function

    Public Overridable Function GetNote(ByVal oPolicy As clsBasePolicy, ByVal sNoteDesc As String) As clsBaseNote

        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.NoteDesc.ToString.ToUpper = sNoteDesc.ToString.ToUpper Then
                Return oNote
                Exit For
            End If
        Next

        Return Nothing

    End Function

    Protected Overridable Sub Calculate()

    End Sub

    Public Sub New()

    End Sub

    Public Function LoadStateInfoTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""

        Dim oDS As New DataSet

        Try

            Using cmd As New SqlCommand(sSql, moConn)

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
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
        End Try
    End Function

    Public Overridable Function AddNote(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sNoteText As String, ByVal sNoteDescription As String, ByVal sSourceCode As String, ByVal iIndex As Integer) As System.Collections.Generic.List(Of clsBaseNote)

        Dim bAddNote As Boolean = True

        'check to see if the note already exists
        'if it does do nothing, if it don't then add it
        For Each oNoteEntry As clsBaseNote In oNoteList
            If oNoteEntry.NoteDesc.ToUpper = sNoteDescription.ToUpper And oNoteEntry.SourceCode.ToUpper = sSourceCode.ToUpper Then
                bAddNote = False
                Exit For
            End If
        Next

        If bAddNote Then
            Dim oNote As New clsBaseNote
            oNote.NoteText = sNoteText
            oNote.NoteDesc = sNoteDescription
            oNote.SourceCode = sSourceCode
            oNote.SystemTS = Now()
            oNote.UserID = "WebRater"
			oNote.IndexNum = iIndex + 1
			oNote.IsNew = True
            oNoteList.Add(oNote)
            If Not oNote Is Nothing Then
                oNote = Nothing
            End If
        End If

        Return oNoteList

    End Function

    Public Overridable Function RemoveNotes(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sSourceCode As String) As System.Collections.Generic.List(Of clsBaseNote)

        For i As Integer = oNoteList.Count - 1 To 0 Step -1
            If oNoteList.Item(i).SourceCode.ToUpper = sSourceCode.ToUpper Then
                oNoteList.RemoveAt(i)
            End If
        Next

        Return oNoteList

    End Function

    Public Shared Function StateInfoContains(ByVal group As String, ByVal code As String, ByVal subcode As String, ByVal program As String, ByVal AppliesToCode As String, ByVal dtRateDate As Date) As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
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

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@Group", SqlDbType.VarChar, 50).Value = group
                cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code
                cmd.Parameters.Add("@SubCode", SqlDbType.VarChar, 50).Value = subcode
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = AppliesToCode

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

    Public Shared Function StateInfoContainsProgramSpecific(ByVal group As String, ByVal code As String, ByVal subcode As String, ByVal program As String, ByVal AppliesToCode As String, ByVal sProgram As String, ByVal dtRateDate As Date) As Boolean
        Dim sSql As String = ""
        Dim oConn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
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
                sSql = sSql & " AND Program = @Program "
                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Date", SqlDbType.DateTime, 8).Value = dtRateDate
                cmd.Parameters.Add("@Group", SqlDbType.VarChar, 50).Value = group
                cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code
                cmd.Parameters.Add("@SubCode", SqlDbType.VarChar, 50).Value = subcode
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = AppliesToCode
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = sProgram

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


End Class
