Imports Microsoft.VisualBasic
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports CorPolicy.clsCommonFunctions
Imports System.Collections.Generic

Public Class clsPgm117
    Inherits clsPgm1

    Public Overrides Function LookUpSubCode(ByVal oFactor As clsEndorsementFactor) As String

        Select Case oFactor.FactorCode
            
            Case "HO207"
                Dim bLittle As Boolean = False
                Dim bFast As Boolean = False
                Dim bOutboard As Boolean = False
                For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                    Select Case oUWQuestion.QuestionCode
                        Case "H02"
                            If oUWQuestion.AnswerCode = "001" Then
                                '<26 feet
                                bLittle = True
                            ElseIf oUWQuestion.AnswerCode = "002" Then
                                '>=26 feet
                                bLittle = False
                            End If
                        Case "H03" 'motor type
                            If oUWQuestion.AnswerCode = "001" Then
                                'Inboard
                                bOutboard = False
                            ElseIf oUWQuestion.AnswerCode = "002" Then
                                'Outboard
                                bOutboard = True
                            End If
                        Case "H05" 'speed
                            If CInt(oUWQuestion.AnswerCode) < 4 Then
                                '0-9 MPH = 001
                                '10-19 MPH = 002
                                '20-29 MPH = 003
                                bFast = False
                            ElseIf CInt(oUWQuestion.AnswerCode) >= 4 Then
                                '30-39 MPH = 004
                                '40-49 MPH = 005
                                'over 49 MPH = 006
                                bFast = True
                            End If
                    End Select
                Next
                If bLittle And Not bFast Then
                    'A or B
                    If bOutboard Then
                        'A
                        oFactor.FactorCode += "-A"
                    Else
                        'B
                        oFactor.FactorCode += "-B"
                    End If
                Else
                    'C or D
                    If bOutboard Then
                        'C
                        oFactor.FactorCode += "-C"
                    Else
                        'D
                        oFactor.FactorCode += "-D"
                    End If
                End If
            Case "HO208"
                For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                    Select Case oUWQuestion.QuestionCode
                        Case "192"  'Total Acreage
                            If oUWQuestion.AnswerText.ToUpper = "OWNER" Then
                                oFactor.FactorCode += "-O"
                            ElseIf oUWQuestion.AnswerText.ToUpper = "TENANT" Then
                                oFactor.FactorCode += "-T"
                            End If
                    End Select
                Next
        End Select
        Return oFactor.FactorCode

    End Function

    Public Overrides Function dbGetEndorsementFactorCmd(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactor As clsEndorsementFactor) As SqlCommand
        Dim sSql As String = String.Empty
        Dim cmd As New SqlCommand(sSql, moConn)

        sSql = " SELECT FactorCode, Coverage, Type, Factor, FactorType, Crit1, Crit2, Crit3, Crit4 FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorEndorsement with(nolock)"
        sSql = sSql & " WHERE Program = @Program "
        sSql = sSql & " AND EffDate <= @RateDate "
        sSql = sSql & " AND ExpDate > @RateDate "
        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
        If oFactor.FactorCode = "HO212" Then
            sSql = sSql & " AND FactorCode like '" & oFactor.FactorCode & "%'"
        Else
            sSql = sSql & " AND FactorCode = @FactorCode "
        End If
        sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

        'Execute the query
        cmd.CommandText = sSql

        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

        If Not oFactor.FactorCode = "HO212" Then
            cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode
        End If


        Return cmd
    End Function

    Public Overrides Sub GetTotalChgInPremEndorseFactors(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)

        Dim drEndorsementRow As DataRow = Nothing

        Try

            For Each oEndorseFactor As clsEndorsementFactor In oPolicy.EndorsementFactors
                If Not oEndorseFactor.IsMarkedForDelete Then

                    ' HO212 doesn't get this assigned since the actual factors in the table are something like HO212A
                    If oEndorseFactor.FactorCode = "HO212" Then
                        oEndorseFactor.CovType = "N"
                    End If

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


                    Dim dHO206JSum As Decimal = 0.0
                    Dim dHO206OSum As Decimal = 0.0

                    For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                        Select Case oEndorseFactor.FactorCode
                            Case "HO206-J"
                                Select Case oProperty.PropertyCategoryDesc.ToUpper
                                    Case "JEWELRY", "FINE ARTS", "STAMPS", "COINS", "FIREARMS"
                                        Dim dPercentageofTotal As Decimal

                                        If oEndorseFactor.Limit > 0 Then
                                            dPercentageofTotal = oProperty.PropertyAmt / oEndorseFactor.Limit
                                            If RoundStandard(oEndorseFactor.FactorAmt * dPercentageofTotal, 2) + dHO206JSum > oEndorseFactor.FactorAmt Then
                                                oProperty.PropertyPremiumAmt += oEndorseFactor.FactorAmt - dHO206JSum
                                            Else
                                                oProperty.PropertyPremiumAmt += RoundStandard(oEndorseFactor.FactorAmt * dPercentageofTotal, 2)
                                            End If

                                            dHO206JSum += oProperty.PropertyPremiumAmt
                                        End If
                                End Select
                            Case "HO206-O"
                                Select Case oProperty.PropertyCategoryDesc.ToUpper
                                    Case "FURS", "CAMERAS", "CAMERA, FILMS AND RELATED", "MUSICAL INSTRUMENTS", "SILVERWARE", "GOLF EQUIPMENT", "GOLFER'S EQUIPMENT"
                                        Dim dPercentageofTotal As Decimal

                                        If oEndorseFactor.Limit > 0 Then
                                            dPercentageofTotal = oProperty.PropertyAmt / oEndorseFactor.Limit
                                            If RoundStandard(oEndorseFactor.FactorAmt * dPercentageofTotal, 2) + dHO206OSum > oEndorseFactor.FactorAmt Then
                                                oProperty.PropertyPremiumAmt += oEndorseFactor.FactorAmt - dHO206OSum
                                            Else
                                                oProperty.PropertyPremiumAmt += RoundStandard(oEndorseFactor.FactorAmt * dPercentageofTotal, 2)
                                            End If

                                            dHO206OSum += oProperty.PropertyPremiumAmt
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

    Public Overrides Function CalculateEndorsementFactor(ByVal oReader As SqlDataReader, ByVal oFactor As clsEndorsementFactor, ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As Decimal

        Dim dEndorsementFactor As Decimal = 0
        Dim drFactorRow As DataRow = Nothing
        Dim dTempFactor As Decimal = 0
        Dim dLimitFactor As Decimal = 0
        Dim dLimit As Decimal = 0
        Dim iTerm As Integer = 0

        Try

            Select Case oFactor.FactorCode
                Case "HO204", "DW204"
                    'Total = Factor * EC TerrFactor
                    drFactorRow = GetRow(FactorTable, "Territory")
                    For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                        If oDataCol.ColumnName.ToString = "EC_" & oReader.Item("Type") Then
                            If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                Exit For
                            Else
                                dTempFactor = CDec(drFactorRow("EC_D"))
                                Exit For
                            End If
                        End If
                    Next

                    dEndorsementFactor = CDec(oReader.Item("Factor")) * dTempFactor
                Case "HO205"
                    'Total = (Limit)/Crit1 * Factor * EC TerrFactor
                    drFactorRow = GetRow(FactorTable, "Territory")
                    For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                        If oDataCol.ColumnName.ToString = "EC_" & oReader.Item("Type") Then
                            If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                Exit For
                            Else
                                dTempFactor = CDec(drFactorRow("EC_D"))
                                Exit For
                            End If
                        End If
                    Next

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "173" Then
                            dLimit = GetNumbersOnlyFromString(oUWQuestion.AnswerText)
                        End If
                    Next

                    dEndorsementFactor = (dLimit) / CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Factor")) * dTempFactor
                Case "HO209"
                    'Total = Factor * LimitFactor * TerrFactor
                    drFactorRow = GetRow(FactorTable, "Territory")
                    For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                        If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_N" Then 'If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_" & oReader.Item("Type") Then
                            If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                Exit For
                            Else
                                dTempFactor = 0 'CDec(drFactorRow(oReader.Item("Coverage") & "_N"))
                                Exit For
                            End If
                        End If
                    Next
                    If Not drFactorRow Is Nothing Then
                        drFactorRow = Nothing
                    End If
                    'use the Increased Limit Factor
                    If oReader.Item("Coverage").ToString.ToUpper = "LIA" Then
                        drFactorRow = GetIncreasedLimitRow(FactorTable, "INCR_LIA")
                        For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                            If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_N" Then 'If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_" & oReader.Item("Type") Then
                                If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                    dLimitFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                    Exit For
                                Else
                                    dLimitFactor = CDec(drFactorRow(oReader.Item("Coverage") & "_N"))
                                    Exit For
                                End If
                            End If
                        Next
                    ElseIf oReader.Item("Coverage").ToString.ToUpper = "MED" Then
                        drFactorRow = GetIncreasedLimitRow(FactorTable, "INCR_MED")
                        For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                            If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_N" Then 'If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_" & oReader.Item("Type") Then
                                If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                    dLimitFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                    Exit For
                                Else
                                    dLimitFactor = CDec(drFactorRow(oReader.Item("Coverage") & "_N"))
                                    Exit For
                                End If
                            End If
                        Next
                    End If

                    If StateInfoContains("RATE", "HO209", "WITHOUTLIMITFACTOR", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                        dLimitFactor = 1
                    End If


                    dEndorsementFactor = CDec(oReader.Item("Factor")) * (dLimitFactor) * dTempFactor * oFactor.NumberOfEndorsements
                Case "DW211"
                    'Total = Factor * Limit/100 * Fire TerrFactor * Term
                    drFactorRow = GetRow(FactorTable, "Territory")
                    For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                        If oDataCol.ColumnName.ToString = "FIRE_" & oReader.Item("Type") Then
                            If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                Exit For
                            Else
                                dTempFactor = CDec(drFactorRow("FIRE_D"))
                                Exit For
                            End If
                        End If
                    Next

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "202" Then
                            dLimit = oUWQuestion.AnswerText
                            'round to nearest 100
                            dLimit = RoundStandard(dLimit / 10, 1) * 10
                            'ElseIf oUWQuestion.QuestionCode = "221" Then
                            '    iTerm = CInt(oUWQuestion.AnswerText)
                        End If
                    Next

                    dEndorsementFactor = CDec(oReader.Item("Factor")) * (CDec(dLimit) / 100) * dTempFactor '* iTerm
                Case "HO201", "HO202", "HO203", "DW202", "DW203", "DW201", "CITFEE", "CAF", "HO903"
                    'Total = Factor * Premium
                    'we are justing setting the Factor at this point

                    dEndorsementFactor = CDec(oReader.Item("Factor"))
                Case "HO206-J", "HO206-O"

                    'Total = (Limit-Crit2)/Crit1 * Factor * EC TerrFactor
                    drFactorRow = GetRow(FactorTable, "Territory")
                    For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                        If oDataCol.ColumnName.ToString = "EC_" & oReader.Item("Type") Then
                            If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                Exit For
                            Else
                                dTempFactor = CDec(drFactorRow("EC_D"))
                                Exit For
                            End If
                        End If
                    Next

                    'For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                    '    'total the limits for all of the amts listed on this endorsement
                    '    'Select Case oUWQuestion.QuestionCode
                    '    'Case "175", "177", "179", "181", "183", "185", "187", "189", "191"
                    '    If oUWQuestion.AnswerText <> "" Then
                    '        If IsNumeric(oUWQuestion.AnswerText) Then
                    '            dLimit += CDec(oUWQuestion.AnswerText)
                    '        End If
                    '    End If
                    '    'End Select
                    'Next
                    dLimit = oFactor.Limit

                    'round to nearest 100
                    dLimit = RoundStandard(dLimit / 10, 1) * 10
                    dEndorsementFactor = RoundStandard(((dLimit - CDec(oReader.Item("Crit2"))) / CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Factor")) * dTempFactor), 0)
                Case "HO207-A", "HO207-B", "HO207-C", "HO207-D"
                    'Total = Factor * Crit1 * Crit2 * TerrFactor
                    drFactorRow = GetRow(FactorTable, "Territory")
                    For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                        If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_" & oReader.Item("Type") Then
                            If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                Exit For
                            Else
                                dTempFactor = 0 'CDec(drFactorRow(oReader.Item("Coverage") & "_N"))
                                Exit For
                            End If
                        End If
                    Next

                    'dEndorsementFactor = RoundStandard((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Crit2")) * dTempFactor) * oFactor.NumberOfEndorsements, 0)
                    dEndorsementFactor = RoundStandard((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Crit2")) * dTempFactor), 0)

                Case "HO208"
                    'Total = Factor * Crit1 * LimitFactor
                    'use the Increased Limit Factor

                    'crit1 - Owner Occ
                    'crit 2 - Tenant Occ

                    If oReader.Item("Coverage").ToString.ToUpper = "LIA" Then
                        drFactorRow = GetIncreasedLimitRow(FactorTable, "INCR_LIA")
                        For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                            If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_N" Then 'If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_" & oReader.Item("Type") Then
                                If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                    dLimitFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                    Exit For
                                Else
                                    dLimitFactor = CDec(drFactorRow(oReader.Item("Coverage") & "_N"))
                                    Exit For
                                End If
                            End If
                        Next
                    ElseIf oReader.Item("Coverage").ToString.ToUpper = "MED" Then
                        drFactorRow = GetIncreasedLimitRow(FactorTable, "INCR_MED")
                        For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                            If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_N" Then 'If oDataCol.ColumnName.ToString = oReader.Item("Coverage") & "_" & oReader.Item("Type") Then
                                If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                                    dLimitFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                                    Exit For
                                Else
                                    dLimitFactor = CDec(drFactorRow(oReader.Item("Coverage") & "_N"))
                                    Exit For
                                End If
                            End If
                        Next
                    End If

                    If (oPolicy.Status.Trim = "3" Or oPolicy.Status.Trim = "2" Or oPolicy.Status.Trim = "1") And oPolicy.CallingSystem.ToUpper = "WEBRATER" Then 'oFactor.UWQuestions.Count = 2 Then 'Web Rater Quick Quote
                        Dim iNumOfOwnerOccupied As Integer = 0
                        Dim iNumOfTenantOccupied As Integer = 0

                        For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                            Select Case oUWQuestion.QuestionCode
                                Case "307" ' WebRater
                                    If oUWQuestion.AnswerText <> "" Then
                                        iNumOfOwnerOccupied = oUWQuestion.AnswerText
                                    End If
                                Case "306" ' WebRater
                                    If oUWQuestion.AnswerText <> "" Then
                                        iNumOfTenantOccupied = oUWQuestion.AnswerText
                                    End If
                            End Select
                        Next
                        'assume med pay included
                        dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * (1 + dLimitFactor)) * iNumOfOwnerOccupied) + ((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit2")) * (1 + dLimitFactor) * iNumOfTenantOccupied)), 0)

                    Else
                        For i As Integer = 1 To oFactor.NumberOfEndorsements
                            Dim bOwnerOccupied As Boolean = False
                            Dim bMedExcluded As Boolean = False
                            For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                                If oUWQuestion.IndexNum = i Then
                                    If oReader.Item("Coverage").ToString.ToUpper = "MED" Then
                                        For Each oUWQ As clsUWQuestion In oFactor.UWQuestions
                                            If oUWQ.IndexNum = i Then
                                                Select Case oUWQ.QuestionCode
                                                    Case "H13", "H17", "H21", "H25"
                                                        If oUWQ.AnswerText = "Excluded" Then
                                                            bMedExcluded = True
                                                            Exit For
                                                        End If
                                                End Select
                                            End If
                                        Next
                                    End If
                                    For Each oUWQ As clsUWQuestion In oFactor.UWQuestions
                                        If oUWQ.IndexNum = i Then
                                            Select Case oUWQ.QuestionCode
                                                Case "H14", "H18", "H22", "H26"
                                                    If oUWQ.AnswerText = "Owner" Then
                                                        bOwnerOccupied = True
                                                        Exit For
                                                    End If
                                            End Select
                                        End If
                                    Next

                                    If CDec(oReader.Item("Factor")) <> 0 Then
                                        dEndorsementFactor += IIf(bMedExcluded, 0, CDec(oReader.Item("Factor")) * IIf(bOwnerOccupied, CDec(oReader.Item("Crit1")), CDec(oReader.Item("Crit2"))) * (1 + dLimitFactor))
                                        Exit For
                                    End If
                                End If
                            Next
                        Next

                        dEndorsementFactor = RoundStandard(dEndorsementFactor, 0)
                    End If
                Case "DW212"
                    'Water Back Up
                    'the premium amount is just the amopunt in crit1 ($30)
                    dEndorsementFactor = RoundStandard(CDec(oReader.Item("Crit1")), 0)

                Case "DW213"
                    ' Dwelling Liability Coverage
                    Dim iDwellingCount As Integer = 0
                    Dim dLiaLimit As Decimal
                    Dim dMedLimit As Decimal
                    Dim iSplitPos As Integer = 0


                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        Select Case oUWQuestion.QuestionCode
                            Case "311" ' WebRater
                                If oPolicy.DwellingUnits(0).BuildingTypeCode = "BLD1" OrElse oPolicy.DwellingUnits(0).BuildingTypeCode = "SF" Then
                                    iDwellingCount = 1
                                Else
                                    iDwellingCount = 2
                                End If

                                ' Set the LIA limit
                                iSplitPos = oUWQuestion.AnswerText.IndexOf("/")
                                dLiaLimit = Convert.ToDecimal(oUWQuestion.AnswerText.Substring(1, iSplitPos - 1))
                                dMedLimit = Convert.ToDecimal(oUWQuestion.AnswerText.Substring(iSplitPos + 2))

                                ' Find the row that matches the Criteria and Set the Factor accordingly
                                If oReader.Item("Crit1") = iDwellingCount And oReader.Item("Crit2") = dLiaLimit And oReader.Item("Crit3") = dMedLimit Then
                                    dEndorsementFactor = RoundStandard(CDec(oReader.Item("Factor")), 0)
                                End If

                        End Select
                    Next
                Case "HO212"
                    ' Loss of Use Coverage
                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        Select Case oUWQuestion.QuestionCode
                            Case "313" ' WebRater
                                Dim sLimit As String
                                sLimit = oUWQuestion.AnswerText

                                Try
                                    'While oReader.Read()
                                    If RoundStandard(CDec(oReader.Item("Crit1")), 0) = RoundStandard(CDec(sLimit), 0) Then
                                        dEndorsementFactor = RoundStandard(CDec(oReader.Item("Crit2")), 0)
                                        '    Exit While
                                    End If
                                Catch Ex As Exception
                                    Throw New Exception("Error trying to get factor for HO212: " & Ex.Message)
                                End Try
                                'End While
                        End Select
                    Next

                Case Else
                    dEndorsementFactor = 0
            End Select

            'oFactor.FactorAmt = RoundStandard(dEndorsementFactor, 0)

            Return dEndorsementFactor

        Catch ex As Exception
			Throw New ArgumentException("ErrorMsg:" & ex.Message & ex.StackTrace, ex)
        End Try

    End Function

    Public Overrides Sub MapObjects(ByRef oPolicy As clsPolicyHomeOwner)
		Dim sSql As String = ""
		Dim XRefTable As DataTable = Nothing
		Dim drXRefRows() As DataRow = Nothing
		Dim bCovSet As Boolean = False
		Dim oEndorsement As clsEndorsementFactor = Nothing

		Try
			Using Conn As New SqlConnection(ConfigurationManager.AppSettings("RatingConnStr"))
				Using cmd As New SqlCommand(sSql, Conn)

                    sSql = " SELECT Source, CodeType, Code, MappingCode1, MappingCode2, MappingCode3 FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeXRef with(nolock)"
                    sSql = sSql & " WHERE Source = @Source "
                    sSql = sSql & " ORDER BY CodeType, Code "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Source", SqlDbType.VarChar, 20).Value = "PAS" 'oPolicy.CallingSystem

					'Execute the query
					cmd.CommandText = sSql

					Dim adp As New SqlDataAdapter(cmd)
					Dim ds As New DataSet()
					Conn.Open()
					adp.Fill(ds)

					XRefTable = ds.Tables.Item(0)

				End Using
			End Using

			For Each oUnit As clsDwellingUnit In oPolicy.DwellingUnits
				For Each oCov As clsHomeOwnerCoverage In oUnit.Coverages
                    If Not String.IsNullOrEmpty(oCov.SystemCode) Then
                        'we need to map this guy
                        'using oCov.SystemCode find/set the MappingCode1/CovGroup and Type
                        drXRefRows = XRefTable.Select("Code='" & oCov.SystemCode & "' AND CodeType='Coverage'")
                        oCov.CovGroup = drXRefRows(0).Item("MappingCode1")
                        oCov.SystemCode = ""

                        If drXRefRows(0).Item("MappingCode2") IsNot System.DBNull.Value Then
                            oCov.Type = drXRefRows(0).Item("MappingCode2")
                        Else
                            oCov.Type = "N"
                        End If
                    End If
				Next
			Next

			For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
				If oPolicyFactor.FactorCode = "" Then
					'we need to map this guy
					'using oPolicyFactor.SystemCode find/set the MappingCode1/FactorCode
					drXRefRows = XRefTable.Select("Code='" & oPolicyFactor.SystemCode & "' AND CodeType='PolicyFactor'")
					If (drXRefRows.Length <> 0) Then
						If drXRefRows(0).Item("MappingCode1") IsNot System.DBNull.Value Then
							oPolicyFactor.FactorCode = drXRefRows(0).Item("MappingCode1")
						End If
					End If
				End If
			Next

			For i As Integer = 0 To oPolicy.EndorsementFactors.Count - 1
				If oPolicy.EndorsementFactors.Item(i).FactorCode = "" Then
					'we need to map this guy
					'using oEndorsement.SystemCode find/set the MappingCode1/FactorCode
					drXRefRows = XRefTable.Select("Code='" & oPolicy.EndorsementFactors.Item(i).SystemCode & "' AND CodeType='Endorsement'")
					oPolicy.EndorsementFactors.Item(i).FactorCode = drXRefRows(0).Item("MappingCode1")
					If drXRefRows(0).Item("MappingCode1") = "ADDLMOLD" Then
						oPolicy.EndorsementFactors.Item(i).Limit = drXRefRows(0).Item("MappingCode2")
					Else
						oPolicy.EndorsementFactors.Item(i).HasSubCode = drXRefRows(0).Item("MappingCode2")

						If oPolicy.EndorsementFactors.Item(i).HasSubCode Then
							oPolicy.EndorsementFactors.Item(i).FactorCode = LookUpSubCode(oPolicy.EndorsementFactors.Item(i))

						End If
					End If
				Else
					'we have a factor code but we may need to add a sub code to it
					Select Case Left(oPolicy.EndorsementFactors.Item(i).FactorCode.ToUpper, 5)
						Case "HO207"
							oPolicy.EndorsementFactors.Item(i).FactorCode = Left(oPolicy.EndorsementFactors.Item(i).FactorCode, 5)
							drXRefRows = XRefTable.Select("MappingCode1='" & oPolicy.EndorsementFactors.Item(i).FactorCode & "' AND CodeType='Endorsement'")

							oPolicy.EndorsementFactors.Item(i).HasSubCode = drXRefRows(0).Item("MappingCode2")

							If oPolicy.EndorsementFactors.Item(i).HasSubCode Then
								oPolicy.EndorsementFactors.Item(i).FactorCode = LookUpSubCode(oPolicy.EndorsementFactors.Item(i))
							End If

						Case Else
							'not needed
					End Select
				End If

			Next

		Catch ex As Exception
			Throw New ArgumentException(ex.Message)
		Finally
			If Not drXRefRows Is Nothing Then
				drXRefRows = Nothing
			End If
			If Not XRefTable Is Nothing Then
				XRefTable.Dispose()
				XRefTable = Nothing
			End If
		End Try

	End Sub

    Public Overrides Sub LoadFees(ByVal oPolicy As clsPolicyHomeOwner)

        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim oFee As clsBaseFee = Nothing
        Dim oEndorse As clsEndorsementFactor = Nothing

        Try
            'clear existing fees
            oPolicy.Fees.Clear()

            ' Based on Rate Date
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
                            oFee.FeeType = oReader.Item("FactorType").ToString
                            oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
                            oFee.FeeNum = oPolicy.Fees.Count + 1
                            oFee.IndexNum = oPolicy.Fees.Count + 1

                        Case "HOMEINSP3"
                            If oPolicy.Type.ToUpper = "NEW" Then
                                oFee = New clsBaseFee
                                oFee.FeeCode = oReader.Item("FeeCode").ToString
                                oFee.FeeDesc = oReader.Item("Description").ToString
                                oFee.FeeName = oReader.Item("Description").ToString
                                oFee.FeeType = oReader.Item("FeeCode").ToString
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

            End Using

            oReader.Close()
            sSql = ""


            If oPolicy.EffDate <> "#12:00:00 AM#" Then
                ' Citizens needs to be based on Policy EffDate rather than rate date
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
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.EffDate
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                    oReader = cmd.ExecuteReader

                    Do While oReader.Read()
                        'if FeeCode is POLICY then add it, we will always have the POLICY fee on the Policy
                        Select Case oReader.Item("FeeCode").ToString.ToUpper
                            Case "CITIZENS"
                                'If Not oPolicy.PriorCarrierName Is Nothing Then
                                '    If oPolicy.PriorCarrierName.ToUpper = "CITIZENS" Then
                                oFee = New clsBaseFee
                                oFee.FeeCode = oReader.Item("FeeCode").ToString
                                oFee.FeeDesc = oReader.Item("Description").ToString
                                oFee.FeeName = oReader.Item("Description").ToString
                                oFee.FeeType = oReader.Item("FactorType").ToString
                                oFee.FeeApplicationType = oReader.Item("FeeApplicationType").ToString
                                oFee.FeeNum = oPolicy.Fees.Count + 1
                                oFee.IndexNum = oPolicy.Fees.Count + 1
                                '    End If
                                'End If
                            Case Else

                        End Select
                        If Not oFee Is Nothing Then
                            oPolicy.Fees.Add(oFee)
                            oFee = Nothing
                        End If
                    Loop
                End Using
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

    Public Overrides Function GetMinPremEndorsement(ByVal oEndorseFactor As clsEndorsementFactor, ByVal sFactorType As String) As Decimal

        Select Case oEndorseFactor.FactorCode
            Case "HO206-J", "HO206-O", "DW211"
                If oEndorseFactor.FactorAmt < 20 Then
                    oEndorseFactor.FactorAmt = 20
                End If

            Case Else
                'don't do anything
        End Select

        ' don't round for the fee that is applied in PAS
        ' TODO:   Don() 't do this if it is a mid mult
        ' this is what is causing PAS to be different than what it rates at
        If sFactorType.ToUpper = "MIDMULT" Then
            Return 0
        ElseIf oEndorseFactor.FactorCode.ToUpper = "CITFEE" Or oEndorseFactor.FactorCode.ToUpper = "CAF" Then
            Return RoundStandard(oEndorseFactor.FactorAmt, 4)
        Else
            Return RoundStandard(oEndorseFactor.FactorAmt, 0)
        End If

    End Function

    Public Overrides Function AllowAEC(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Dim bAllowAEC As Boolean = False

        Select Case oPolicy.Program
            Case "HO20", "HO30", "DW20", "DW30", "DW10"
                bAllowAEC = True
            Case Else
                bAllowAEC = False
        End Select

        Return bAllowAEC

    End Function

    Public Overrides Sub SetLimitAmounts(ByVal oPolicy As clsPolicyHomeOwner)

        'set limit amounts for select endorsements
        Dim dJewelryTotalAmt As Decimal = 0
        Dim dFurTotalAmt As Decimal = 0
        Dim dCamerasTotalAmt As Decimal = 0
        Dim dMusicTotalAmt As Decimal = 0
        Dim dSilverwareTotalAmt As Decimal = 0
        Dim dGolfTotalAmt As Decimal = 0
        Dim dArtTotalAmt As Decimal = 0
        Dim dStampTotalAmt As Decimal = 0
        Dim dCoinTotalAmt As Decimal = 0
        Dim dFirearmsTotalAmt As Decimal = 0


        For i As Integer = 0 To oPolicy.EndorsementFactors.Count - 1
            Select Case oPolicy.EndorsementFactors.Item(i).FactorCode
                Case "HO205"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    'add $3000 to the limit amount selected since it is an increased limit from $3000
                    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        If Not oUWQuestion.QuestionCode = "" Then
                            If oUWQuestion.QuestionCode.ToUpper = "173" Then
                                If Not oUWQuestion.AnswerText = "" Then
                                    If IsNumeric(oUWQuestion.AnswerText) Then
                                        oPolicy.EndorsementFactors.Item(i).Limit = CInt(oUWQuestion.AnswerText) + 3000
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                Case "HO206-J" 'Jewelry, Art, Stamp and Coin      
                    If oPolicy.CallingSystem <> "PAS" Then
                        oPolicy.EndorsementFactors.Item(i).Limit = 0

                        For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                            Select Case oProperty.PropertyCategoryDesc.ToUpper
                                Case "JEWELRY"
                                    dJewelryTotalAmt += oProperty.PropertyAmt
                                Case "FINE ARTS"
                                    dArtTotalAmt += oProperty.PropertyAmt
                                Case "STAMPS"
                                    dStampTotalAmt += oProperty.PropertyAmt
                                Case "COINS"
                                    dCoinTotalAmt += oProperty.PropertyAmt
                                Case "FIREARMS"
                                    dFirearmsTotalAmt += oProperty.PropertyAmt
                            End Select
                        Next

                        ' Removed 6/27/2011 replaced with HomeScheduledProperty object
                        'For x As Integer = 1 To oPolicy.EndorsementFactors.Item(i).NumberOfEndorsements
                        '    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '        If oUWQuestion.IndexNum = x Then
                        '            If oUWQuestion.QuestionCode.ToUpper = "300" Or oUWQuestion.QuestionCode.ToUpper = "301" Then
                        '                Select Case oUWQuestion.AnswerText.ToUpper
                        '                    Case "JEWELRY"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dJewelryTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "FINE ARTS"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dArtTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "STAMPS"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dStampTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "COINS"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dCoinTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next

                        '                    Case "FIREARMS"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "303" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dFirearmsTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                End Select
                        '            End If
                        '        End If
                        '    Next
                        'Next
                        oPolicy.EndorsementFactors.Item(i).Limit = dJewelryTotalAmt + dArtTotalAmt + dStampTotalAmt + dCoinTotalAmt + dFirearmsTotalAmt
                    End If
                Case "HO206-O" 'Furs, Cameras, Musical Instruments, Silverware and Golf Equipment
                    If oPolicy.CallingSystem <> "PAS" Then
                        oPolicy.EndorsementFactors.Item(i).Limit = 0

                        For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                            Select Case oProperty.PropertyCategoryDesc.ToUpper
                                Case "FURS"
                                    dFurTotalAmt += oProperty.PropertyAmt
                                Case "CAMERAS", "CAMERA, FILMS AND RELATED"
                                    dCamerasTotalAmt += oProperty.PropertyAmt
                                Case "MUSICAL INSTRUMENTS"
                                    dMusicTotalAmt += oProperty.PropertyAmt
                                Case "SILVERWARE"
                                    dSilverwareTotalAmt += oProperty.PropertyAmt
                                Case "GOLF EQUIPMENT", "GOLFER'S EQUIPMENT"
                                    dGolfTotalAmt += oProperty.PropertyAmt
                            End Select
                        Next


                        ' removed 6/27/2011 replaced with homescheduledproperty object
                        'For x As Integer = 1 To oPolicy.EndorsementFactors.Item(i).NumberOfEndorsements
                        '    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '        If oUWQuestion.IndexNum = x Then
                        '            If oUWQuestion.QuestionCode.ToUpper = "300" Or oUWQuestion.QuestionCode.ToUpper = "301" Then
                        '                Select Case oUWQuestion.AnswerText.ToUpper
                        '                    Case "FURS"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dFurTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "CAMERAS", "CAMERA, FILMS AND RELATED"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dCamerasTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "MUSICAL INSTRUMENTS"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dMusicTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "SILVERWARE"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dSilverwareTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                    Case "GOLF EQUIPMENT"
                        '                        'loop through the questions again and find the amt for the index
                        '                        For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        '                            If oUWQ.IndexNum = x Then
                        '                                If oUWQ.QuestionCode.ToUpper = "305" Then
                        '                                    If oUWQ.AnswerText <> "" Then
                        '                                        dGolfTotalAmt += CDec(oUWQ.AnswerText)
                        '                                    End If
                        '                                End If
                        '                            End If
                        '                        Next
                        '                End Select
                        '            End If
                        '        End If
                        '    Next
                        'Next

                        oPolicy.EndorsementFactors.Item(i).Limit = dFurTotalAmt + dCamerasTotalAmt + dMusicTotalAmt + dSilverwareTotalAmt + dGolfTotalAmt
                    End If
                Case "DW211"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        If Not oUWQuestion.QuestionCode = "" Then
                            If oUWQuestion.QuestionCode.ToUpper = "202" Then
                                If Not oUWQuestion.AnswerText = "" Then
                                    If IsNumeric(oUWQuestion.AnswerText) Then
                                        oPolicy.EndorsementFactors.Item(i).Limit = CInt(oUWQuestion.AnswerText)
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
            End Select
        Next

    End Sub

    Public Overrides Sub CheckMinPremAmounts(ByRef oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable)

        Dim dTotal As Decimal = 0
        Dim drTotalsRow As DataRow = Nothing
        Dim bUpdatePrem As Boolean = False
        Dim dMinPremAmt As Decimal = 0

        Try
            'assume false
            oPolicy.MinPremApplied = False

            UpdateTotals(oPolicy, FactorTable)

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
                        dTotal += CDec(drTotalsRow(oDataCol.ColumnName.ToString))
                    End If
                End If
            Next

            Select Case oPolicy.Program
                Case "HO30T"
                    If dTotal < 100 Then
                        dMinPremAmt = 100
                        bUpdatePrem = True
                        oPolicy.MinPremApplied = True
                    End If
                Case Else
                    If dTotal < 300 Then
                        dMinPremAmt = 300
                        bUpdatePrem = True
                        oPolicy.MinPremApplied = True
                    End If
            End Select
            '    Case Else
            'If dTotal < 250 Then
            '    dMinPremAmt = 250
            '    bUpdatePrem = True
            '    oPolicy.MinPremApplied = True
            'End If
            'End Select

            If bUpdatePrem Then
                'we need to update the premium to the minimum premium amount and allocate premium to coverages on a pro rata basis
                'iNumOfCovs = oPolicy.DwellingUnits(0).Coverages.Count
                'dCovPremAmt = RoundStandard(dMinPremAmt / iNumOfCovs, 0)
                For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
                    For i As Integer = 1 To drTotalsRow.Table.Columns.Count - 1
                        If drTotalsRow.Table.Columns(i).ColumnName.ToUpper = oCov.CovGroup & "_" & oCov.Type Then
                            'drTotalsRow(i) = dCovPremAmt
                            drTotalsRow(i) = dMinPremAmt * (drTotalsRow(i) / dTotal)
                        End If
                    Next i
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

    Public Overrides Function dbGetFeeFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FeeTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFeeRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim dTotalFees As Decimal = 0

        Try
            For Each oFee As clsBaseFee In oPolicy.Fees

                Using cmd As New SqlCommand(sSql, moConn)

                    sSql = " SELECT Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorFee with(nolock)"
                    sSql = sSql & " WHERE Program = @Program "
                    sSql = sSql & " AND EffDate <= @RateDate "
                    sSql = sSql & " AND ExpDate > @RateDate "
                    sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                    sSql = sSql & " AND FeeCode = @FeeCode "

                    'Execute the query
                    cmd.CommandText = sSql

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program

                    If oFee.FeeCode.ToUpper = "CITIZENS" Then
                        ' Citizens is based on policy effdate, all others are based on rate date
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.EffDate
                    Else
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                    End If

                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                    cmd.Parameters.Add("@FeeCode", SqlDbType.VarChar, 9).Value = oFee.FeeCode

                    oReader = cmd.ExecuteReader

                    If oReader.HasRows Then
                        drFeeRow = FeeTable.NewRow
                        drFeeRow.Item("FeeCode") = oFee.FeeCode
                    End If

                    Do While oReader.Read()
                        'add it to the data row and the fee object
                        If oFee.FeeCode.ToUpper = "CITIZENS" Then
                            drFeeRow.Item("Factor") = oReader.Item("Factor") * oPolicy.FullTermPremium
                            drFeeRow.Item("FactorType") = oReader.Item("FactorType")
							oFee.FeeAmt = oReader.Item("Factor") * oPolicy.FullTermPremium
							oFee.FeeAmt = RoundStandard(oFee.FeeAmt, 2)
                            oFee.FeeType = oReader.Item("FactorType")
                        Else
                            drFeeRow.Item("Factor") = oReader.Item("Factor")
                            drFeeRow.Item("FactorType") = oReader.Item("FactorType")
                            oFee.FeeAmt = oReader.Item("Factor")
                            oFee.FeeType = oReader.Item("FactorType")
                        End If
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

    Public Overrides Function dbGetUWTier(ByVal oPolicy As clsPolicyHomeOwner) As String
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim sTier As String = ""
        Dim bReCalcHomeAge As Boolean = False 'we can only recalculate the home age every 3 years, it comes into rating already recalculated for the current term
        Dim iHomeAge As Integer = oPolicy.DwellingUnits.Item(0).HomeAge

        Try

            If oPolicy.CallingSystem.ToUpper = "WEBRATER" Then
                bReCalcHomeAge = False
            Else
                Dim iPolicyAge As Integer = CalculatePolicyAge(oPolicy.OrigTermEffDate, oPolicy.EffDate)

                If iPolicyAge Mod 3 = 0 Then
                    'it has been 3 years so let's use the recalculated home age
                Else
                    bReCalcHomeAge = True
                End If
                If bReCalcHomeAge Then
                    iHomeAge = CInt(oPolicy.OrigTermEffDate.Year) - CInt(oPolicy.DwellingUnits.Item(0).YearOfConstruction)
                End If
            End If

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Tier FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeUWTiers with(nolock)"
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
                cmd.Parameters.Add("@HomeAge", SqlDbType.Int, 22).Value = iHomeAge 'oPolicy.DwellingUnits.Item(0).HomeAge
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

    Private Function CalculatePolicyAge(ByVal dtOrigEffDate As Date, ByVal dtEffDate As Date) As Integer

        Dim dPolicyAge As Double = 0
        dPolicyAge = (DateDiff("m", dtOrigEffDate, dtEffDate))
        If (DatePart("d", dtOrigEffDate) > DatePart("d", dtEffDate)) Then
            dPolicyAge = dPolicyAge - 1
        End If
        If dPolicyAge < 0 Then
            dPolicyAge = dPolicyAge + 1
        End If

        'Return CInt(dPolicyAge Mod 12)
        Return CInt(dPolicyAge)

  End Function

  Public Overrides Sub GetFactors(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)
    Dim oSurvey As clsWindMitSurvey = New clsWindMitSurvey
    Dim sSql As String = ""

    Try
      MyBase.GetFactors(oPolicy, oFactorTable)

      Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT SurveyID FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "SurveyXML with(nolock)"
        sSql = sSql & " WHERE PolicyID = @PolicyID AND ExpDate IS NULL "

        cmd.CommandText = sSql
        cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar, 20).Value = oPolicy.PolicyID
				Dim result As Decimal = cmd.ExecuteScalar
				oSurvey = oSurvey.Load(result, oPolicy.Product, oPolicy.StateCode)
      End Using

            If Not oSurvey Is Nothing Then
                If AllowWindMitigationDiscount(oPolicy) Then
                    CalculateWindMitigationFactor(oSurvey, oPolicy, oFactorTable)
                End If
            End If
        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally

        End Try
    End Sub

    Public Function AllowWindMitigationDiscount(ByVal oPolicy As clsPolicyHomeOwner) As Boolean
        Dim bAllow As Boolean = True
        Dim bHasEC As Boolean = False

        If oPolicy.DwellingUnits(0).OwnerOccupiedFlag = 0 Then
            bAllow = False
        End If

        If oPolicy.Program = "DW10" Then
            For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits.Item(0).Coverages
                If oCov.CovGroup.ToString.ToUpper = "EC" Then
                    bHasEC = True
                End If
            Next

            If Not bHasEC Then
                bAllow = False
            End If
        End If

        Return bAllow
    End Function

  Public Function CalculateWindMitigationFactor(ByVal oSurvey As clsWindMitSurvey, ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader
        Dim drFactorRow As DataRow = Nothing
        Dim dTempFactorCode As String = ""
        Dim bFactorType As Boolean = False
        Dim drTotalsRow As DataRow = Nothing
        Dim drMaxDiscountRow As DataRow = Nothing

        Try

            For Each question As clsUWQuestion In oSurvey.SurveyQuestions

                Select Case question.QuestionCode
                    Case "1"
                        'Building Code
                        dTempFactorCode &= "BUILDING_CODE_" & question.IndexNum & ","
                    Case "2"
                        'Design Wind Speed
                        dTempFactorCode &= "DESIGN_WIND_SPEED_" & question.IndexNum & ","
                    Case "4"
                        'Secondary Roof Water Intrusion System
                        dTempFactorCode &= "SEC_ROOF_WATER_INT_" & question.IndexNum & ","
                    Case "5"
                        'Extent of Debris Protection
                        dTempFactorCode &= "EXTENT_WIND_DEBRIS_" & question.IndexNum & ","
                    Case "6"
                        'Type of Debris Protection
                        dTempFactorCode &= "TYPE_WIND_DEBRIS_" & question.IndexNum & ","
                    Case "7"
                        'Roof Geometry
                        dTempFactorCode &= "ROOF_GEOMETRY_" & question.IndexNum & ","
                    Case "8"
                        'Roof Covering
                        dTempFactorCode &= "ROOF_COVERING_" & question.IndexNum & ","
                    Case "11"
                        'Roof Wall Connection
                        dTempFactorCode &= "ROOF_WALL_CONN_" & question.IndexNum & ","
                    Case "12"
                        'Gable Roof Bracing
                        dTempFactorCode &= "GABLE_ROOF_BRACE_" & question.IndexNum & ","
                    Case "13"
                        'Foundation Restraint
                        dTempFactorCode &= "FOUND_REST_" & question.IndexNum & ","
                End Select
            Next

            dTempFactorCode = dTempFactorCode.TrimEnd(",")

            Using cmd As New SqlCommand(sSql, moConn)
                sSql = " exec pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CalcWindMitFactor '"
                sSql = sSql & oPolicy.Program & "','"
                sSql = sSql & oPolicy.RateDate & "','"
                sSql = sSql & oPolicy.AppliesToCode & "',"
                sSql = sSql & oPolicy.DwellingUnits(0).Region & ",'"
                sSql = sSql & dTempFactorCode & "'"

                'Execute the query
                cmd.CommandText = sSql

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "WindMitigation"
                    drFactorRow.Item("FactorType") = "MidMult"
                End If

                While oReader.Read()
                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                        'If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                        '                drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = oReader("Factor")

                        'End If
                        Dim bIsCappedFactor As Boolean = False
                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                            If Not msCappedFactors Is Nothing Then
                                For q As Integer = 0 To msCappedFactors.Length - 1
                                    If msCappedFactors(q).ToUpper = "WINDMITIGATION" Then
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
                End While

            End Using

            If Not drFactorRow Is Nothing Then
                ' TODO: Check to see if the maxdiscount amount has been hit
                FactorTable.Rows.Add(drFactorRow)
                drFactorRow = Nothing
            End If
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

            Return drFactorRow

        Catch ex As Exception
            Throw New ArgumentException("ErrorMsg:" & ex.Message & ex.StackTrace, ex)
        End Try

    End Function

    Public Overrides Function dbGetDed3Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                If oPolicy.Program.Trim.ToUpper = "HO30T" Then
                    sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
                Else
                    sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed3 with(nolock)"
                End If
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded3) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart < @ContentsAmt "
                    sSql = sSql & " AND CovAmtEnd >= @ContentsAmt "
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    sSql = sSql & " AND ((CovAmtStart < @DwellingAmt "
                    sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    sSql = sSql & " AND Type = 'C' ) "
                    sSql = sSql & " OR (CovAmtStart < @DwellingAmt "
                    sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
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
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5)
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

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



End Class
