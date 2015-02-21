Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports RatingRulesLib.MarketingCRMService
Imports Enumerable = System.Linq.Enumerable
Imports System.Configuration

Public MustInherit Class clsRules
    Private moStateInfoDS As DataSet
    Private moProgramSettingsDS As DataSet

#Region "Abstract Methods"

    Public MustOverride Sub SetWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal startDate As DateTime, _
                                           ByVal programs As List(Of Integer))

    Public MustOverride Sub ExpireWeatherOverride(ByVal productCode As Integer, ByVal stateCode As String, ByVal programs As List(Of ProgramSetting))
#End Region

    Public Function CheckNEI(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

    End Function

    Public Overridable Sub CheckRules(ByVal oPolicy As clsBasePolicy, ByVal sRuleType As String)
        Try
            ' Needed to load the moProgramSettings object when only calling for Driver/Vehicle/etc and the policy
            ' object isn't available
            GetProgramSetting("", oPolicy)


            'remove all sRuleType notes
            oPolicy.Notes = RemoveNotes(oPolicy.Notes, sRuleType)

            Dim dtRatingRules As DataTable

            If oPolicy.Status.ToUpper.Trim <> "BOUND" Then
                dtRatingRules = GetDataTableOfRules(oPolicy, sRuleType, "POLICY", oPolicy.Status)
                dtRatingRules = ApplyTemporaryRulesOverride(dtRatingRules, oPolicy)
                For Each oRule As DataRow In dtRatingRules.Rows
                    Dim sFunctionName As String
                    sFunctionName = oRule("FunctionName")

                    Try
                        CallByName(Me, sFunctionName, CallType.Method, oPolicy)
                    Catch ex As Exception ' Call other functions if an error occurs in one
                        'Dim errCtx As New CorFunctions.ExceptionContext(ex)
                        'errCtx.AddContext("RuleType", sRuleType)
                        'errCtx.AddContext("FunctionName", sFunctionName)
                        'errCtx.AddContext("Policy", oPolicy)
                        'errCtx.SourceSystem = "RatingRules"
                        'errCtx.SystemTS = Now

                        'If Not oPolicy.PolicyID Is Nothing Then
                        '    If Not oPolicy.PolicyID.Length < 8 Then
                        '        errCtx.ReferenceID = oPolicy.PolicyID
                        '        errCtx.ReferenceType = "PolicyID"
                        '    Else
                        '        errCtx.ReferenceID = oPolicy.QuoteID
                        '        errCtx.ReferenceType = "QuoteID"
                        '    End If
                        'Else
                        '    errCtx.ReferenceID = oPolicy.QuoteID
                        '    errCtx.ReferenceType = "QuoteID"
                        'End If
                        'errCtx.LogError()
                    End Try
                Next
            End If
        Catch ex As Exception
            Dim sError As String = ex.Message
            Throw New Exception("Check " & sRuleType & ": " & sError)
        End Try
    End Sub

    Public Overridable Function CheckWRN(ByVal oPolicy As clsBasePolicy) As Boolean
        CheckRules(oPolicy, "WRN")
    End Function

    Public Overridable Function CheckIER(ByVal oPolicy As clsBasePolicy) As Boolean
        CheckRules(oPolicy, "IER")
    End Function

    Public Overridable Function ApplyTemporaryRulesOverride(ByVal ratingRules As DataTable, ByVal policy As clsBasePolicy) As DataTable
        Return ratingRules
    End Function

    Public Overridable Function CheckUWW(ByVal oPolicy As clsBasePolicy) As Boolean
        CheckRules(oPolicy, "UWW")
    End Function

    Public Overridable Function CheckRES(ByVal oPolicy As clsBasePolicy) As Boolean
        CheckRules(oPolicy, "RES")
    End Function

    Public Overridable Function AddNote(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sNoteText As String, ByVal sNoteDescription As String, ByVal sSourceCode As String, ByVal iIndex As Integer, Optional ByVal sCallingSystem As String = "Webrater") As System.Collections.Generic.List(Of clsBaseNote)

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
            oNote.UserID = sCallingSystem
            oNote.IndexNum = iIndex + 1
            oNote.IsNew = True
            oNoteList.Add(oNote)
            If Not oNote Is Nothing Then
                oNote = Nothing
            End If
        End If

        Return oNoteList

    End Function

    Public Overridable Function AddNote(ByVal iTransNum As Integer, ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sNoteText As String, ByVal sNoteDescription As String, ByVal sSourceCode As String, ByVal iIndex As Integer, Optional ByVal sCallingSystem As String = "Webrater") As System.Collections.Generic.List(Of clsBaseNote)

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
            oNote.UserID = sCallingSystem
            oNote.IndexNum = iIndex + 1
            oNote.PolicyTransactionNum = iTransNum
            oNoteList.Add(oNote)
            If Not oNote Is Nothing Then
                oNote = Nothing
            End If
        End If

        Return oNoteList

    End Function

    Public Overridable Function RemoveNotes(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sSourceCode As String) As System.Collections.Generic.List(Of clsBaseNote)

        For i As Integer = oNoteList.Count - 1 To 0 Step -1
            If oNoteList.Item(i).SourceCode.ToUpper.Trim = sSourceCode.ToUpper.Trim Then
                oNoteList.RemoveAt(i)
            End If
        Next

        Return oNoteList

    End Function

    Public Overridable Sub UpdateNote(ByVal oNoteList As System.Collections.Generic.List(Of clsBaseNote), ByVal sExistingNoteDescription As String, ByVal sNewNoteDescription As String)
        Dim oNote As New clsBaseNote
        For Each oNote In oNoteList
            If oNote.NoteDesc = sExistingNoteDescription Then
                oNote.NoteDesc = sNewNoteDescription
            End If
        Next
    End Sub

    Public Overridable Function GetNote(ByVal oPolicy As clsBasePolicy, ByVal sNoteDesc As String) As clsBaseNote

        For Each oNote As clsBaseNote In oPolicy.Notes
            If oNote.NoteDesc.ToString.ToUpper = sNoteDesc.ToString.ToUpper Then
                Return oNote
                Exit For
            End If
        Next

        Return Nothing

    End Function

    Public Shared Sub RemovePolicyFactor(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String)
        For i As Integer = oPolicy.PolicyFactors.Count - 1 To 0 Step -1
            If oPolicy.PolicyFactors.Item(i).FactorCode.ToUpper = sFactorCode.ToUpper Then
                'remove it
                oPolicy.PolicyFactors.RemoveAt(i)
            End If
        Next
    End Sub

    Public Shared Function GetActiveZipCountyRestrictions(ByVal productCode As Integer, ByVal stateCode As String) As List(Of StateInfo)

        Dim results As New List(Of StateInfo)
        Dim reader As SqlDataReader
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim transaction As SqlTransaction
        oConn.Open()
        transaction = oConn.BeginTransaction()

        Try
            Dim sSql As String = String.Empty

            Using cmd As New SqlCommand(sSql, oConn, transaction)

                sSql = "SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue, AppliesToCode, EffDate, ExpDate, UserId, SystemTS FROM " _
                    & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..StateInfo "
                sSql &= "WHERE ItemGroup = 'WEATHEROVERRIDE' AND ExpDate > GetDate()"

                cmd.CommandText = sSql
                cmd.Transaction = transaction

                reader = cmd.ExecuteReader()

                While (reader.Read())
                    results.Add(New StateInfo With _
                                {
                                    .Program = reader(0),
                                    .ItemGroup = reader(1),
                                    .ItemCode = reader(2),
                                    .ItemSubCode = reader(3),
                                    .ItemValue = reader(4),
                                    .AppliesToCode = reader(5),
                                    .EffDate = reader(6),
                                    .ExpDate = reader(7),
                                    .UserID = reader(8),
                                    .SystemTS = reader(9)
                                })
                End While

                reader.Close()
                transaction.Commit()

            End Using
        Catch ex As Exception
            If (Not transaction Is Nothing) Then
                transaction.Rollback()
            End If
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Return results
    End Function

    Public Shared Function GetActiveRestrictions(ByVal productCode As Integer, ByVal stateCode As String) As List(Of ProgramSetting)

        Dim results As New List(Of ProgramSetting)
        Dim reader As SqlDataReader
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim transaction As SqlTransaction
        oConn.Open()
        transaction = oConn.BeginTransaction()

        Try
            Dim sSql As String = String.Empty

            Using cmd As New SqlCommand(sSql, oConn, transaction)

                sSql = "SELECT Program, SettingName, SettingDesc, Value, AppliesToCode, EffDate, ExpDate FROM " _
                    & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..ProgramSettings "
                sSql &= "WHERE SettingName = 'WeatherOverrideDate' AND ExpDate > GetDate()"

                cmd.CommandText = sSql
                cmd.Transaction = transaction

                reader = cmd.ExecuteReader()

                While (reader.Read())
                    results.Add(New ProgramSetting With _
                                {
                                    .Program = reader(0),
                                    .SettingName = reader(1),
                                    .SettingDesc = reader(2),
                                    .ProgramValue = reader(3),
                                    .AppliesToCode = reader(4),
                                    .EffDate = reader(5),
                                    .ExpDate = reader(6)
                                })
                End While

                reader.Close()
                transaction.Commit()

            End Using
        Catch ex As Exception
            If (Not transaction Is Nothing) Then
                transaction.Rollback()
            End If
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Return results
    End Function

    Public Shared Function GetRatingRules(ByVal productCode As Integer, ByVal stateCode As String) As List(Of String)

        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim transaction As SqlTransaction
        oConn.Open()
        transaction = oConn.BeginTransaction()

        Try
            Dim sSql As String = String.Empty

            Using cmd As New SqlCommand(sSql, oConn, transaction)

                sSql = "SELECT DISTINCT RuleType FROM " & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..RatingRules"

                cmd.CommandText = sSql
                cmd.Transaction = transaction

                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(oDS, "RatingRules")

                transaction.Commit()
            End Using
        Catch ex As Exception
            If (Not transaction Is Nothing) Then
                transaction.Rollback()
            End If
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Dim ratingRulesTable As DataTable
        ratingRulesTable = oDS.Tables(0)

        Return Enumerable.Cast(Of String)((From oRow As DataRow In ratingRulesTable Select oRow.Item(0))).ToList()

    End Function

    Public Sub UpdateProgramSetting(ByVal productCode As Integer, ByVal stateCode As String, ByVal program As ProgramSetting)

        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim transaction As SqlTransaction
        oConn.Open()
        transaction = oConn.BeginTransaction()

        Try
            Dim sSql As String = String.Empty

            Using cmd As New SqlCommand(sSql, oConn, transaction)

                sSql = "UPDATE " & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..ProgramSettings "
                sSql &= "SET [ExpDate] = @ExpDate, Value = @Value "
                sSql &= "WHERE [Program] = @Program AND [SettingName] = @SettingName"

                cmd.CommandText = sSql
                cmd.Transaction = transaction
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = program.Program
                cmd.Parameters.Add("@SettingName", SqlDbType.VarChar, 50).Value = program.SettingName
                cmd.Parameters.Add("@ExpDate", SqlDbType.DateTime).Value = program.ExpDate
                cmd.Parameters.Add("@Value", SqlDbType.VarChar, 50).Value = program.ProgramValue

                cmd.ExecuteNonQuery()
                transaction.Commit()
            End Using
        Catch ex As Exception
            If (Not transaction Is Nothing) Then
                transaction.Rollback()
            End If
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

    End Sub

    Public Function GetAllProgramSetting(ByVal productCode As Integer, ByVal stateCode As String, ByVal settingName As String) As List(Of ProgramSetting)

        Dim results As New List(Of ProgramSetting)

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) _
            And Not String.IsNullOrEmpty(settingName)) Then

            Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            Dim reader As SqlDataReader
            Dim transaction As SqlTransaction
            oConn.Open()
            transaction = oConn.BeginTransaction()

            Try
                Dim sSql As String = String.Empty

                Using cmd As New SqlCommand(sSql, oConn, transaction)

                    sSql = "SELECT Program, SettingName, SettingDesc, Value, AppliesToCode, EffDate, ExpDate FROM " & _
                        CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..ProgramSettings "
                    sSql &= "WHERE SettingName = @SettingName"


                    cmd.CommandText = sSql
                    cmd.Transaction = transaction
                    cmd.Parameters.Add("@SettingName", SqlDbType.VarChar, 50).Value = settingName

                    reader = cmd.ExecuteReader()

                    While (reader.Read())
                        results.Add(New ProgramSetting With _
                                    {
                                        .Program = reader(0),
                                        .SettingName = reader(1),
                                        .SettingDesc = reader(2),
                                        .ProgramValue = reader(3),
                                        .AppliesToCode = reader(4),
                                        .EffDate = reader(5),
                                        .ExpDate = reader(6)
                                    })
                    End While

                    reader.Close()
                    transaction.Commit()
                End Using
            Catch ex As Exception
                If (Not transaction Is Nothing) Then
                    transaction.Rollback()
                End If
                Throw
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
        End If

        Return results

    End Function

    Public Sub InsertProgramSetting(ByVal productCode As Integer, ByVal stateCode As String, ByVal program As ProgramSetting)

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) _
            And Not program Is Nothing And program.IsValid()) Then

            Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            Dim transaction As SqlTransaction
            oConn.Open()
            transaction = oConn.BeginTransaction()

            Try
                Dim sSql As String = String.Empty

                Using cmd As New SqlCommand(sSql, oConn, transaction)

                    sSql = "INSERT INTO " & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..ProgramSettings"
                    sSql &= "([Program],[SettingName],[SettingDesc],[Value],[AppliesToCode],[EffDate],[ExpDate])"
                    sSql &= "VALUES (@Program, @SettingName, @SettingDesc, @Value, @AppliesToCode, @EffDate, @ExpDate)"

                    cmd.CommandText = sSql
                    cmd.Transaction = transaction
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = program.Program
                    cmd.Parameters.Add("@SettingName", SqlDbType.VarChar, 50).Value = program.SettingName
                    cmd.Parameters.Add("@SettingDesc", SqlDbType.VarChar, 200).Value = program.SettingDesc
                    cmd.Parameters.Add("@Value", SqlDbType.VarChar, 50).Value = program.ProgramValue
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = program.AppliesToCode.ToString()
                    cmd.Parameters.Add("@EffDate", SqlDbType.DateTime).Value = program.EffDate
                    cmd.Parameters.Add("@ExpDate", SqlDbType.DateTime).Value = program.ExpDate

                    cmd.ExecuteNonQuery()
                    transaction.Commit()
                End Using
            Catch ex As Exception
                If (Not transaction Is Nothing) Then
                    transaction.Rollback()
                End If
                Throw
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
        End If
    End Sub

    Public Function GetAllStateInfo(ByVal productCode As Integer, ByVal stateCode As String, ByVal settingName As String) As List(Of StateInfo)

        Dim results As New List(Of StateInfo)

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) _
            And Not String.IsNullOrEmpty(settingName)) Then

            Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            Dim reader As SqlDataReader
            Dim transaction As SqlTransaction
            oConn.Open()
            transaction = oConn.BeginTransaction()

            Try
                Dim sSql As String = String.Empty

                Using cmd As New SqlCommand(sSql, oConn, transaction)

                    sSql = "Select Program, ItemGroup, ItemCode, ItemSubCode, ItemValue, AppliesToCode, EffDate, ExpDate, UserID, SystemTS FROM " & _
                        CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..StateInfo "
                    sSql &= "WHERE ItemGroup = @ItemGroup"


                    cmd.CommandText = sSql
                    cmd.Transaction = transaction
                    cmd.Parameters.Add("@ItemGroup", SqlDbType.VarChar, 50).Value = settingName

                    reader = cmd.ExecuteReader()

                    While (reader.Read())
                        results.Add(New StateInfo With _
                                    {
                                        .Program = reader(0),
                                        .ItemGroup = reader(1),
                                        .ItemCode = reader(2),
                                        .ItemSubCode = reader(3),
                                        .ItemValue = reader(4),
                                        .AppliesToCode = reader(5),
                                        .EffDate = reader(6),
                                        .ExpDate = reader(7),
                                        .UserID = reader(8),
                                        .SystemTS = reader(9)
                                    })
                    End While

                    reader.Close()
                    transaction.Commit()
                End Using
            Catch ex As Exception
                If (Not transaction Is Nothing) Then
                    transaction.Rollback()
                End If
                Throw
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
        End If

        Return results
    End Function

    Public Sub UpdateStateInfo(ByVal productCode As Integer, ByRef stateCode As String, ByVal stateInfo As StateInfo)

        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim transaction As SqlTransaction
        oConn.Open()
        transaction = oConn.BeginTransaction()

        Try
            Dim sSql As String = String.Empty

            Using cmd As New SqlCommand(sSql, oConn, transaction)

                sSql = "UPDATE " & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..StateInfo "
                sSql &= "SET [ExpDate] = @ExpDate "
                sSql &= "WHERE [Program] = @Program AND [ItemGroup] = @ItemGroup AND ItemSubCode = @ItemSubCode"

                cmd.CommandText = sSql
                cmd.Transaction = transaction
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = stateInfo.Program
                cmd.Parameters.Add("@ItemGroup", SqlDbType.VarChar, 50).Value = stateInfo.ItemGroup
                cmd.Parameters.Add("@ItemSubCode", SqlDbType.NVarChar, 50).Value = stateInfo.ItemSubCode
                cmd.Parameters.Add("@ExpDate", SqlDbType.DateTime).Value = stateInfo.ExpDate

                cmd.ExecuteNonQuery()
                transaction.Commit()
            End Using
        Catch ex As Exception
            If (Not transaction Is Nothing) Then
                transaction.Rollback()
            End If
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try
    End Sub

    Public Sub InsertStateInfo(ByVal productCode As Integer, ByVal stateCode As String, ByVal stateInfo As StateInfo)

        If (Not String.IsNullOrEmpty(productCode) And Not String.IsNullOrEmpty(stateCode) _
            And Not stateInfo Is Nothing) Then

            Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            Dim transaction As SqlTransaction
            oConn.Open()
            transaction = oConn.BeginTransaction()

            Try
                Dim sSql As String = String.Empty

                Using cmd As New SqlCommand(sSql, oConn, transaction)

                    sSql = "INSERT INTO " & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..StateInfo "
                    sSql &= "([Program],[ItemGroup],[ItemCode],[ItemSubCode],[ItemValue],[AppliesToCode],[EffDate],[ExpDate],[UserId],[SystemTS])"
                    sSql &= "VALUES (@Program, @ItemGroup, @ItemCode, @ItemSubCode, @ItemValue, @AppliesToCode, @EffDate, @ExpDate, @UserId, GetDate())"

                    cmd.CommandText = sSql
                    cmd.Transaction = transaction
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = stateInfo.Program
                    cmd.Parameters.Add("@ItemGroup", SqlDbType.VarChar, 50).Value = stateInfo.ItemGroup
                    cmd.Parameters.Add("@ItemCode", SqlDbType.VarChar, 50).Value = stateInfo.ItemCode
                    cmd.Parameters.Add("@ItemSubCode", SqlDbType.VarChar, 50).Value = stateInfo.ItemSubCode
                    cmd.Parameters.Add("@ItemValue", SqlDbType.VarChar, 200).Value = stateInfo.ItemValue
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = stateInfo.AppliesToCode.ToString()
                    cmd.Parameters.Add("@EffDate", SqlDbType.DateTime).Value = stateInfo.EffDate
                    cmd.Parameters.Add("@ExpDate", SqlDbType.DateTime).Value = stateInfo.ExpDate
                    cmd.Parameters.Add("@UserId", SqlDbType.VarChar, 25).Value = stateInfo.UserID

                    cmd.ExecuteNonQuery()
                    transaction.Commit()
                End Using
            Catch ex As Exception
                If (Not transaction Is Nothing) Then
                    transaction.Rollback()
                End If
                Throw
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
        End If
    End Sub

    Public Function GetProgramSetting(ByVal sSettingName As String, Optional ByVal oPolicy As clsBasePolicy = Nothing) As String
        Dim sReturn As String = String.Empty

        If Not moProgramSettingsDS Is Nothing Then
            ' do nothing
        Else
            If oPolicy Is Nothing Then
                Throw New Exception("Error in GetProgramSettings: moProgramSettingsDS was not loaded")
            End If

            Dim oDS As New DataSet
            Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            oConn.Open()

            Try
                Dim sSql As String = ""
                Using cmd As New SqlCommand(sSql, oConn)
                    sSql = " SELECT SettingName,Value "
                    sSql &= " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..ProgramSettings with(nolock)"
                    sSql &= " WHERE EffDate <= @RateDate "
                    sSql &= " AND ExpDate > @RateDate "
                    sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql &= " AND Program IN ('PPA', 'HOM',  @Program ) "
                    sSql &= " ORDER BY SettingName "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 50).Value = oPolicy.Program


                    Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)
                    adapter.Fill(oDS, "ProgramSettings")

                    moProgramSettingsDS = oDS
                End Using
            Catch ex As Exception
                Throw New ArgumentException(ex.Message & ex.StackTrace)
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
        End If


        Dim oSettingsTable As DataTable
        oSettingsTable = moProgramSettingsDS.Tables(0)


        Dim DataRows() As DataRow
        DataRows = oSettingsTable.Select("SettingName = '" & sSettingName & "'")

        For Each oRow As DataRow In DataRows
            sReturn = oRow("Value")
        Next


        Return sReturn
    End Function

    Public Shared Function GetZipCountyMapping(ByVal productCode As Integer, ByVal stateCode As String) As List(Of ZipCountyMapping)

        'TODO: Validate Inputs
        Dim oDS As New DataSet
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim transaction As SqlTransaction
        oConn.Open()
        transaction = oConn.BeginTransaction()

        Try
            Dim sSql As String = String.Empty

            Using cmd As New SqlCommand(sSql, oConn, transaction)

                sSql = "SELECT DISTINCT Zip, UPPER(County) FROM " & CorFunctions.CommonFunctions.GetTableOwner(productCode, stateCode) & "..CodeTerritoryDefinitions "
                sSql &= "WHERE LEN(Zip) = 5 AND County LIKE '%[^0-9]%' AND ExpDate > GetDate()"

                cmd.CommandText = sSql
                cmd.Transaction = transaction

                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(oDS, "ZipCountyMapping")

                transaction.Commit()
            End Using
        Catch ex As Exception
            If (Not transaction Is Nothing) Then
                transaction.Rollback()
            End If
            Throw
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Dim zipCountyMapping As DataTable
        zipCountyMapping = oDS.Tables(0)

        Return (From oRow As DataRow In zipCountyMapping _
            Select New ZipCountyMapping With _
                {
                .ZipCode = oRow.Item(0),
                .County = oRow.Item(1)
                }).ToList()
    End Function

    Public Sub AddPolicyFactor(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String)
        Dim bExists As Boolean = False

        For Each oFactor As CorPolicy.clsBaseFactor In oPolicy.PolicyFactors
            If oFactor.FactorCode.ToUpper = sFactorCode.ToUpper Then
                bExists = True
                Exit For
            End If
        Next

        If Not bExists Then
            Dim oPF As New CorPolicy.clsBaseFactor
            oPF.FactorCode = sFactorCode
            oPF.IndexNum = oPolicy.PolicyFactors.Count + 1
            oPF.SystemCode = sFactorCode
            oPF.FactorNum = oPolicy.PolicyFactors.Count + 1
            oPF.FactorAmt = 0
            oPolicy.PolicyFactors.Add(oPF)
        End If
    End Sub

    Public Function FactorOnPolicy(ByVal oPolicy As clsBasePolicy, ByVal sFactorCode As String) As Boolean

        For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
            If oFactor.FactorCode.ToString.ToUpper = sFactorCode.ToString.ToUpper Then
                Return True
            End If
        Next

        Return False

    End Function

    Public Function LoadStateInfoTable(ByVal sProduct As String, ByVal sStateCode As String, ByVal dtRateDate As Date, ByVal sAppliesToCode As String) As DataSet
        Dim sSql As String = ""


        If Not moStateInfoDS Is Nothing Then
            Return moStateInfoDS
        Else
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

                    moStateInfoDS = oDS
                    Return oDS
                End Using
            Catch ex As Exception
                Throw New ArgumentException(ex.Message & ex.StackTrace)
            Finally
                oConn.Close()
                oConn.Dispose()
            End Try
        End If
    End Function

    Public Function GetStateInfoValue(ByVal oPolicy As clsPolicyPPA, ByVal sProgram As String, ByVal sItemGroup As String, ByVal sItemCode As String, ByVal sItemSubCode As String) As String

        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
        Dim sStateInfoValue As String = ""

        oStateInfoTable = oStateInfoDataSet.Tables(0)

        'if you don't need to filter by the column then send in a blank string
        Dim sWhereClause As String = ""
        sWhereClause &= "Program IN ('PPA', '" & sProgram & "') "
        If sItemGroup <> "" Then
            sWhereClause &= "AND ItemGroup='" & sItemGroup & "' "
        End If
        If sItemCode <> "" Then
            sWhereClause &= "AND ItemCode='" & sItemCode & "' "
        End If
        If sItemSubCode <> "" Then
            sWhereClause &= "AND ItemSubCode='" & sItemSubCode & "' "
        End If

        DataRows = oStateInfoTable.Select(sWhereClause)

        For Each oRow As DataRow In DataRows
            sStateInfoValue = oRow("ItemValue").ToString
        Next

        Return sStateInfoValue

    End Function
    Public Function GetPropertyStateInfoValue(ByVal oPolicy As clsPolicyHomeOwner, ByVal sProgram As String, ByVal sItemGroup As String, ByVal sItemCode As String, ByVal sItemSubCode As String) As String

        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        Dim oStateInfoDataSet As DataSet = LoadStateInfoTable(oPolicy.Product, oPolicy.StateCode, oPolicy.RateDate, oPolicy.AppliesToCode)
        Dim sStateInfoValue As String = ""

        oStateInfoTable = oStateInfoDataSet.Tables(0)

        'if you don't need to filter by the column then send in a blank string
        Dim sWhereClause As String = ""
        sWhereClause &= "Program IN ('HOM', '" & sProgram & "') "
        If sItemGroup <> "" Then
            sWhereClause &= "AND ItemGroup='" & sItemGroup & "' "
        End If
        If sItemCode <> "" Then
            sWhereClause &= "AND ItemCode='" & sItemCode & "' "
        End If
        If sItemSubCode <> "" Then
            sWhereClause &= "AND ItemSubCode='" & sItemSubCode & "' "
        End If

        DataRows = oStateInfoTable.Select(sWhereClause)

        For Each oRow As DataRow In DataRows
            sStateInfoValue = oRow("ItemValue").ToString
        Next

        Return sStateInfoValue

    End Function
    Public Function GetStartDate(ByVal oPolicy As clsBasePolicy) As Date

        Dim oNote As clsBaseNote = GetNote(oPolicy, "QuoteGen")
        Dim dtStartDate As Date = #1/1/1900#

        If Not oNote Is Nothing Then
            dtStartDate = oNote.NoteText
        End If

        Return dtStartDate

    End Function

    Public Function GetDataTableOfRules(ByRef oPolicy As clsBasePolicy, ByVal sRuleType As String, ByVal sSubType As String, ByVal sStatus As String) As DataTable

        Dim dtRatingRules As New DataTable
        Dim oConn = New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sSql As String = String.Empty
        Dim iStatus As Integer

        Try
            sSql = "  SELECT FunctionName, OrderNumber "
            sSql &= " FROM Common..RatingRules with(nolock) "
            sSql &= " WHERE Product = @Product "
            sSql &= "    AND EffDate <=@RateDate "
            sSql &= "    AND ExpDate > @RateDate "
            sSql &= "    AND CallingSystem IN ('ALL', @CallingSystem) "
            sSql &= "    AND RuleType = @RuleType "
            sSql &= "    AND State IN ('ALL', @StateCode) "
            sSql &= "    AND Program IN ('ALL', @Program) "
            sSql &= "    AND SubType = @SubType "
            sSql &= "    AND Status <= @Status "

            sSql &= "  UNION "

            sSql &= " SELECT FunctionName, OrderNumber "
            sSql &= " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..RatingRules with(nolock)"
            sSql &= " WHERE EffDate <=@RateDate "
            sSql &= "    AND ExpDate > @RateDate "
            sSql &= "    AND CallingSystem IN ('ALL', @CallingSystem) "
            sSql &= "    AND RuleType = @RuleType "
            sSql &= "    AND State IN ('ALL', @StateCode) "
            sSql &= "    AND Program IN ('ALL', @Program) "
            sSql &= "    AND SubType = @SubType "
            sSql &= "    AND Status <= @Status "


            sSql &= "    ORDER BY OrderNumber Asc"
            oConn.Open()

            If oPolicy.CallingSystem.ToUpper = "BRIDGE" Then
                Integer.TryParse(sStatus, iStatus)
                If iStatus < 4 Then
                    iStatus = 3
                End If
            Else
                If Not Integer.TryParse(sStatus, iStatus) Then
                    iStatus = 4
                End If
            End If

            Using cmd As New SqlCommand(sSql, oConn)
                cmd.Parameters.Add("@Product", SqlDbType.Int).Value = Int32.Parse(oPolicy.Product)
                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 11).Value = oPolicy.Program
                cmd.Parameters.Add("@RuleType", SqlDbType.VarChar, 11).Value = sRuleType
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@CallingSystem", SqlDbType.VarChar, 11).Value = GetCallingSystem(oPolicy.CallingSystem)
                cmd.Parameters.Add("@Status", SqlDbType.Int).Value = iStatus
                cmd.Parameters.Add("@StateCode", SqlDbType.VarChar, 11).Value = oPolicy.StateCode
                cmd.Parameters.Add("@SubType", SqlDbType.VarChar, 11).Value = sSubType

                cmd.CommandText = sSql
                Dim adp As New SqlDataAdapter(cmd)
                adp.Fill(dtRatingRules)

            End Using

            oConn.Close()

        Catch ex As Exception
            Dim exCtx As New CorFunctions.ExceptionContext(ex)
            exCtx.SourceSystem = "RatingRules"
            exCtx.SystemTS = Date.Now
            If Not String.IsNullOrEmpty(oPolicy.PolicyID) Then
                exCtx.ReferenceID = oPolicy.PolicyID
                exCtx.ReferenceType = "PolicyID"
            ElseIf Not String.IsNullOrEmpty(oPolicy.QuoteID) Then
                exCtx.ReferenceID = oPolicy.QuoteID
                exCtx.ReferenceType = "QuoteID"
            Else
                exCtx.ReferenceID = oPolicy.Product & oPolicy.StateCode
                exCtx.ReferenceType = "PGM"
            End If
            exCtx.AddContext("Policy", oPolicy)
            exCtx.AddContext("sql", sSql)
            exCtx.AddContext("RuleType", sRuleType)
            exCtx.AddContext("sSubType", sSubType)
            exCtx.AddContext("sStatus", sStatus)
            exCtx.LogError()
        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        Return dtRatingRules


    End Function

    Public Function GetCallingSystem(ByVal sCallingSystem As String) As String
        Dim sReturn As String = ""

        sReturn = sCallingSystem.Trim

        If sReturn.ToUpper = "BRIDGE" Or sReturn.ToUpper = "BRG" Or sReturn.ToUpper = "EZLYNX" Then
            sReturn = "WEBRATER"
        ElseIf sReturn.ToUpper.Contains("OLE") Then
            sReturn = "OLE"
        End If

        Return sReturn
    End Function

    Public Overridable Sub ValidateAgent(ByVal policy As clsBasePolicy)

        Dim svcMarketingCRM As New MarketingCRMService.InsurCloudAMSServiceSoapClient
        Dim agency As MarketingCRMService.Agency = Nothing
        Dim isValid As Boolean = False

        Try
            agency = svcMarketingCRM.LoadAgency(policy.Agency.AgencyID, True, String.Empty)

            For Each agencyLocation As MarketingCRMService.Location In agency.Locations
                If agencyLocation.DefaultAgencyCode = policy.Agency.AgencyID Then
                    For Each locationProgram As MarketingCRMService.LocationProgram In agencyLocation.LocationPrograms
                        If locationProgram.pgmProgram.ProgramCode = policy.ProgramCode _
                            OrElse locationProgram.pgmProgram.Name.ToUpper.Trim = policy.Program.ToUpper.Trim Then
                            If locationProgram.EffDate <= policy.EffDate _
                                AndAlso locationProgram.ExpDate > policy.EffDate Then
                                If locationProgram.AppliesToCode = policy.AppliesToCode _
                                    OrElse locationProgram.AppliesToCode = "B" Then
                                    isValid = True
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                End If
            Next

            If Not isValid Then
                policy.Notes = (AddNote(policy.Notes, "Ineligible Risk: Agent Code " & policy.Agency.AgencyID.Trim & " is not currently licensed for this program - " & policy.ProgramCode.ToUpper.Trim, "ValidateAgent", "IER", policy.Notes.Count))
            End If

        Catch ex As Exception
            Throw
        End Try

    End Sub

    Public Function IsRewritePolicy(ByVal policy As clsBasePolicy) As Boolean
        Return Not policy.TransactionNum > 1 AndAlso Not policy.Notes.ToList().FirstOrDefault(Function(f) f.NoteDesc.ToUpper().Contains("ISREWRITE")) Is Nothing
    End Function
End Class
