Imports Microsoft.VisualBasic
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient
Imports CorPolicy.clsCommonFunctions
Imports System.Collections.Generic
Imports log4net
Imports log4net.Config

Public Class clsPgm142
    Inherits clsPgm1

    Private ReadOnly log4net As ILog

    Public Sub New()
        log4net = LogManager.GetLogger(GetType(RatingService))
        XmlConfigurator.Configure()
    End Sub

    Public Overrides Sub ResetTerritory(ByVal oPolicy As clsPolicyHomeOwner)

        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
        Dim sTerritory As String = String.Empty
        Dim sRegion As String = String.Empty

        Dim oReader As SqlDataReader
        Try
            oConn.Open()
            Dim sSql As String = ""
            Using cmd As New SqlCommand(sSql, oConn)

                sSql = " SELECT Territory,Region "
                sSql = sSql & " FROM pgm" & oPolicy.Product & oPolicy.StateCode & "..CodeTerritoryDefinitions with(nolock)"
                sSql = sSql & " WHERE Zip = @Zip "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND County = @County "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@Zip", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).Zip.Trim
                cmd.Parameters.Add("@County", SqlDbType.VarChar).Value = oPolicy.DwellingUnits(0).County.Trim

                oReader = cmd.ExecuteReader
                While oReader.Read()
                    sTerritory = oReader("Territory")
                    sRegion = oReader("Region")
                End While

            End Using
        Catch ex As Exception

        Finally
            oConn.Close()
            oConn.Dispose()
        End Try

        If Len(sTerritory) > 0 And Len(sRegion) > 0 Then
            oPolicy.DwellingUnits(0).Territory = sTerritory
            oPolicy.DwellingUnits(0).Region = sRegion
        End If
    End Sub


    Public Overrides Function LookUpSubCode(ByVal oFactor As clsEndorsementFactor) As String

        Select Case oFactor.FactorCode
            Case "HO210"
                Dim lTotalAcreage As Long = 0
                For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                    Select Case oUWQuestion.QuestionCode
                        Case "H43", "H45", "H47", "H49"  'Farm 1, 2, 3 and 4 Acreage
                            If oUWQuestion.AnswerText <> "" Then
                                lTotalAcreage += oUWQuestion.AnswerText
                            Else
                                'not there
                            End If
                    End Select
                Next

                If lTotalAcreage <= 100 Then
                    oFactor.FactorCode += "-L"
                Else
                    oFactor.FactorCode += "-M"
                End If
            Case "HO215"
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
            Case "HO225"
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

    Public Overrides Function CalculateEndorsementFactor(ByVal oReader As SqlDataReader, ByVal oFactor As clsEndorsementFactor, ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As Decimal

        Dim dEndorsementFactor As Decimal = 0
        Dim drFactorRow As DataRow = Nothing
        Dim dTempFactor As Decimal = 0
        Dim dLimitFactor As Decimal = 0
        Dim dLimit As Decimal = 0
        Dim iTerm As Integer = 0

        Try

            Select Case oFactor.FactorCode
                Case "HO105", "TDP009", "TDP170P", "TDP170W", "HO214", "HO903"
                    'Total = Factor 

                    dEndorsementFactor = CDec(oReader.Item("Factor"))
                Case "HO110"
                    'Total = (Limit)/Crit1 * Factor

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "173" Then
                            dLimit = GetNumbersOnlyFromString(oUWQuestion.AnswerText)
                        End If
                    Next

                    dEndorsementFactor = (dLimit) / CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Factor"))
                Case "HO120"
                    'Total = (Limit-Crit2)/Crit1 * Factor 

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "166" Then
                            dLimit = oUWQuestion.AnswerText
                            'round to nearest 100
                            dLimit = RoundStandard(dLimit / 10, 1) * 10
                        End If
                    Next

                    dEndorsementFactor = (dLimit / CDec(oReader.Item("Crit1"))) * CDec(oReader.Item("Factor"))
                Case "HO301"
                    'Total = Factor * LimitFactor 
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

                    ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                    If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                        dEndorsementFactor = CDec(oReader.Item("Factor")) * (dLimitFactor + 1) * oFactor.NumberOfEndorsements
                    Else
                        dEndorsementFactor = CDec(oReader.Item("Factor")) * (dLimitFactor) * oFactor.NumberOfEndorsements
                    End If
                Case "TDP017"
                    'Total = Factor * Limit/100 * Fire TerrFactor * Term
                    'Total = (Rental Amount*No of Months/100)*Factor

                    'drFactorRow = GetRow(FactorTable, "Territory")
                    'For Each oDataCol As DataColumn In drFactorRow.Table.Columns
                    '    If oDataCol.ColumnName.ToString = "FIRE_" & oReader.Item("Type") Then
                    '        If drFactorRow(oDataCol.ColumnName.ToString) IsNot System.DBNull.Value Then
                    '            dTempFactor = CDec(drFactorRow(oDataCol.ColumnName.ToString))
                    '            Exit For
                    '        Else
                    '            dTempFactor = CDec(drFactorRow("FIRE_D"))
                    '            Exit For
                    '        End If
                    '    End If
                    'Next

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "202" Then
                            dLimit = oUWQuestion.AnswerText
                            'round to nearest 100
                            dLimit = RoundStandard(dLimit / 10, 1) * 10
                        ElseIf oUWQuestion.QuestionCode = "221" Then
                            iTerm = CInt(oUWQuestion.AnswerText)
                        End If
                    Next

                    dEndorsementFactor = ((dLimit * iTerm) / 100) * CDec(oReader.Item("Factor"))
                Case "HO101", "HO901P", "HO901PR", "HO902P", "TDP901P", "TDP901PR", "TDP902P"
                    'Total = Factor * Premium
                    'we are justing setting the Factor at this point

                    dEndorsementFactor = CDec(oReader.Item("Factor"))
                Case "HO161-25", "HO161-50", "HO161-100", "HO162-25", "HO162-50", "HO162-100", "TDP004-25", "TDP004-50", "TDP004-100"
                    'Total = Crit1 * DwellingAmt
                    dEndorsementFactor = CDec(oReader.Item("Crit1")) * oPolicy.DwellingUnits(0).DwellingAmt
                Case "HO164-25", "HO164-50", "HO164-100"
                    'Total = Crit1 * ContentsAmt
                    dEndorsementFactor = CDec(oReader.Item("Crit1")) * oPolicy.DwellingUnits(0).ContentsAmt
                Case "HO160-J", "HO160-O", "HO160"
                    'HO160-J and HO160-O

                    'Total = (Limit-Crit2)/Crit1 * Factor 

                    dLimit = oFactor.Limit

                    'round to nearest 100
                    dLimit = RoundStandard(dLimit / 10, 1) * 10
                    dEndorsementFactor = RoundStandard(((dLimit - CDec(oReader.Item("Crit2"))) / CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Factor"))), 0)

                Case "HO210-L", "HO210-M"
                    Dim iNumOfFarms As Integer = 0
                    Dim lNumOfAnimals As Long = 0
                    Dim lPayroll As Long = 0

                    'HO210-L and HO210-M

                    'Total = (Factor * Limit Factor * TerrFactor * # of farms) + (Crit1 * trunc(# of animals/Crit2)) + (Crit3 * trunc(payroll/Crit4) * Limit Factor)
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

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        'If oUWQuestion.QuestionCode = "H45" Then 'Total Number of Farms
                        '    iNumOfFarms = CInt(GetNumbersOnlyFromString(oUWQuestion.AnswerText))
                        'Else

                        Select Case oUWQuestion.QuestionCode
                            Case "H43", "H45", "H47", "H49"  'Farm 1, 2, 3 and 4 Acreage
                                If oUWQuestion.AnswerText <> "" Then
                                    iNumOfFarms += 1
                                Else
                                    'not there
                                End If
                        End Select

                        If oUWQuestion.QuestionCode = "H51" Then 'Animal Collision Included?
                            If oUWQuestion.AnswerCode = "001" Then 'YES
                                'get the answer to the question
                                For i As Integer = 0 To oFactor.UWQuestions.Count - 1
                                    If oFactor.UWQuestions.Item(i).QuestionCode = "H55" Then 'Number of Animals
                                        lNumOfAnimals = CLng(GetNumbersOnlyFromString(oFactor.UWQuestions.Item(i).AnswerText))
                                    End If
                                Next
                            End If
                        ElseIf oUWQuestion.QuestionCode = "H53" Then 'Payroll Included?
                            If oUWQuestion.AnswerCode = "001" Then 'YES
                                'get the answer to the question
                                For i As Integer = 0 To oFactor.UWQuestions.Count - 1
                                    If oFactor.UWQuestions.Item(i).QuestionCode = "H54" Then 'Total payroll
                                        lPayroll = CLng(GetNumbersOnlyFromString(oFactor.UWQuestions.Item(i).AnswerText))
                                    End If
                                Next
                            End If
                        End If
                    Next

                    If CDec(oReader.Item("Factor")) <> 0 Then
                        If dTempFactor = 0 Then dTempFactor = 1
                        ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                        If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * (dLimitFactor + 1) * dTempFactor * iNumOfFarms) + (CDec(oReader.Item("Crit1")) * Math.Floor(lNumOfAnimals / CDec(oReader.Item("Crit2")))) + (CDec(oReader.Item("Crit3")) * Math.Floor(lPayroll / CDec(oReader.Item("Crit4"))) * (dLimitFactor + 1))), 0)
                        Else
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * (dLimitFactor) * dTempFactor * iNumOfFarms) + (CDec(oReader.Item("Crit1")) * Math.Floor(lNumOfAnimals / CDec(oReader.Item("Crit2")))) + (CDec(oReader.Item("Crit3")) * Math.Floor(lPayroll / CDec(oReader.Item("Crit4"))) * (dLimitFactor))), 0)
                        End If
                    End If
                    'Me.FactorAmt = dEndorsementFactor

                Case "HO215-A", "HO215-B", "HO215-C", "HO215-D"
                    'Total = Factor * Crit1 * Crit2 

                    dEndorsementFactor = RoundStandard((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Crit2"))), 0)

                Case "HO225"
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
                        ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                        If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * (dLimitFactor + 1)) * iNumOfOwnerOccupied) + ((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit2")) * (dLimitFactor + 1) * iNumOfTenantOccupied)), 0)
                        Else
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * (dLimitFactor)) * iNumOfOwnerOccupied) + ((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit2")) * (dLimitFactor) * iNumOfTenantOccupied)), 0)
                        End If

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
                                        ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                                        If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                                            dEndorsementFactor += IIf(bMedExcluded, 0, CDec(oReader.Item("Factor")) * IIf(bOwnerOccupied, CDec(oReader.Item("Crit1")), CDec(oReader.Item("Crit2"))) * (dLimitFactor + 1))
                                        Else
                                            dEndorsementFactor += IIf(bMedExcluded, 0, CDec(oReader.Item("Factor")) * IIf(bOwnerOccupied, CDec(oReader.Item("Crit1")), CDec(oReader.Item("Crit2"))) * (dLimitFactor))
                                        End If
                                        Exit For
                                    End If
                                End If
                            Next
                        Next

                        dEndorsementFactor = RoundStandard(dEndorsementFactor, 0)
                    End If

                Case "TDP213"
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
                Case Else
                    dEndorsementFactor = 0
            End Select

            'oFactor.FactorAmt = RoundStandard(dEndorsementFactor, 0)
            ' Do not round if this is a mid mult
            If oReader("FactorType") = "MidMult" Then
                Return dEndorsementFactor
            Else
                Return IIf(dEndorsementFactor < 1, dEndorsementFactor, RoundStandard(dEndorsementFactor, 0))
            End If


        Catch ex As Exception
            Throw New ArgumentException("ErrorMsg:" & ex.Message & ex.StackTrace, ex)
        End Try

    End Function

    Public Overloads Function CalculateEndorsementFactor(ByVal oReader As SqlDataReader, ByVal oFactor As clsEndorsementFactor, ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable, ByVal iLimitAmt As Integer, ByVal sScheduledPropertyType As String) As Decimal

        Dim dEndorsementFactor As Decimal = 0
        Dim drFactorRow As DataRow = Nothing
        Dim dTempFactor As Decimal = 0
        Dim dLimitFactor As Decimal = 0
        Dim dLimit As Decimal = 0
        Dim iTerm As Integer = 0

        Try

            Select Case oFactor.FactorCode
                Case "HO105", "TDP009", "TDP170P", "TDP170W"
                    'Total = Factor 

                    dEndorsementFactor = CDec(oReader.Item("Factor"))
                Case "HO110"
                    'Total = (Limit)/Crit1 * Factor

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "173" Then
                            dLimit = GetNumbersOnlyFromString(oUWQuestion.AnswerText)
                        End If
                    Next

                    dEndorsementFactor = (dLimit) / CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Factor"))
                Case "HO120"
                    'Total = (Limit-Crit2)/Crit1 * Factor 

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        If oUWQuestion.QuestionCode = "166" Then
                            dLimit = oUWQuestion.AnswerText
                            'round to nearest 100
                            dLimit = RoundStandard(dLimit / 10, 1) * 10
                        End If
                    Next

                    dEndorsementFactor = (dLimit / CDec(oReader.Item("Crit1"))) * CDec(oReader.Item("Factor"))
                Case "HO301"
                    'Total = Factor * LimitFactor 
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

                    ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                    If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                        dEndorsementFactor = CDec(oReader.Item("Factor")) * (dLimitFactor + 1) * oFactor.NumberOfEndorsements
                    Else
                        dEndorsementFactor = CDec(oReader.Item("Factor")) * (dLimitFactor) * oFactor.NumberOfEndorsements
                    End If
                Case "TDP017"
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
                        ElseIf oUWQuestion.QuestionCode = "221" Then
                            iTerm = CInt(oUWQuestion.AnswerText)
                        End If
                    Next

                    dEndorsementFactor = CDec(oReader.Item("Factor")) * (CDec(dLimit) / 100) * dTempFactor * iTerm
                Case "HO101", "HO901P", "HO901PR", "HO902P", "TDP901P", "TDP901PR", "TDP902P"
                    'Total = Factor * Premium
                    'we are justing setting the Factor at this point

                    dEndorsementFactor = CDec(oReader.Item("Factor"))
                Case "HO161-25", "HO161-50", "HO161-100", "HO162-25", "HO162-50", "HO162-100", "TDP004-25", "TDP004-50", "TDP004-100"
                    'Total = Crit1 * DwellingAmt
                    dEndorsementFactor = CDec(oReader.Item("Crit1")) * oPolicy.DwellingUnits(0).DwellingAmt
                Case "HO164-25", "HO164-50", "HO164-100"
                    'Total = Crit1 * ContentsAmt
                    dEndorsementFactor = CDec(oReader.Item("Crit1")) * oPolicy.DwellingUnits(0).ContentsAmt
                Case "HO160-J", "HO160-O", "HO160"
                    'HO160-J and HO160-O

                    'Total = (Limit-Crit2)/Crit1 * Factor 

                    'dLimit = oFactor.Limit
                    dLimit = iLimitAmt

                    'round to nearest 100
                    dLimit = RoundStandard(dLimit / 10, 1) * 10
                    dEndorsementFactor = RoundStandard(((dLimit - CDec(oReader.Item("Crit2"))) / CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Factor"))), 0)

                Case "HO210-L", "HO210-M"
                    Dim iNumOfFarms As Integer = 1
                    Dim lNumOfAnimals As Long = 0
                    Dim lPayroll As Long = 0

                    'HO210-L and HO210-M

                    'Total = (Factor * Limit Factor * TerrFactor * # of farms) + (Crit1 * trunc(# of animals/Crit2)) + (Crit3 * trunc(payroll/Crit4) * Limit Factor)
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

                    For Each oUWQuestion As clsUWQuestion In oFactor.UWQuestions
                        'If oUWQuestion.QuestionCode = "H45" Then 'Total Number of Farms
                        '    iNumOfFarms = CInt(GetNumbersOnlyFromString(oUWQuestion.AnswerText))
                        'Else
                        If oUWQuestion.QuestionCode = "H47" Then 'Animal Collision Included?
                            If oUWQuestion.AnswerCode = "001" Then 'YES
                                'get the answer to the question
                                For i As Integer = 0 To oFactor.UWQuestions.Count - 1
                                    If oFactor.UWQuestions.Item(i).QuestionCode = "H48" Then 'Number of Animals
                                        lNumOfAnimals = CLng(GetNumbersOnlyFromString(oFactor.UWQuestions.Item(i).AnswerText))
                                    End If
                                Next
                            End If
                        ElseIf oUWQuestion.QuestionCode = "H49" Then 'Payroll Included?
                            If oUWQuestion.AnswerCode = "001" Then 'YES
                                'get the answer to the question
                                For i As Integer = 0 To oFactor.UWQuestions.Count - 1
                                    If oFactor.UWQuestions.Item(i).QuestionCode = "H50" Then 'Total payroll
                                        lPayroll = CLng(GetNumbersOnlyFromString(oFactor.UWQuestions.Item(i).AnswerText))
                                    End If
                                Next
                            End If
                        End If
                    Next

                    If CDec(oReader.Item("Factor")) <> 0 Then
                        ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                        If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * (dLimitFactor + 1) * dTempFactor * iNumOfFarms) + (CDec(oReader.Item("Crit1")) * Math.Floor(lNumOfAnimals / CDec(oReader.Item("Crit2")))) + (CDec(oReader.Item("Crit3")) * Math.Floor(lPayroll / CDec(oReader.Item("Crit4"))) * (dLimitFactor + 1))), 0)
                        Else
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * (dLimitFactor) * dTempFactor * iNumOfFarms) + (CDec(oReader.Item("Crit1")) * Math.Floor(lNumOfAnimals / CDec(oReader.Item("Crit2")))) + (CDec(oReader.Item("Crit3")) * Math.Floor(lPayroll / CDec(oReader.Item("Crit4"))) * (dLimitFactor))), 0)
                        End If
                    End If
                    'Me.FactorAmt = dEndorsementFactor

                Case "HO215-A", "HO215-B", "HO215-C", "HO215-D"
                    'Total = Factor * Crit1 * Crit2 

                    dEndorsementFactor = RoundStandard((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * CDec(oReader.Item("Crit2"))), 0)

                Case "HO225"
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
                        ' Prior to the 4/1/2010 rate change, this calculation added a 1 to dLimitFactor
                        If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * (dLimitFactor + 1)) * iNumOfOwnerOccupied) + ((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit2")) * (dLimitFactor + 1) * iNumOfTenantOccupied)), 0)
                        Else
                            dEndorsementFactor = RoundStandard(((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit1")) * (dLimitFactor)) * iNumOfOwnerOccupied) + ((CDec(oReader.Item("Factor")) * CDec(oReader.Item("Crit2")) * (dLimitFactor) * iNumOfTenantOccupied)), 0)
                        End If

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
                                        If StateInfoContains("LIA", "END", "PLUS1", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.RateDate) Then
                                            dEndorsementFactor += IIf(bMedExcluded, 0, CDec(oReader.Item("Factor")) * IIf(bOwnerOccupied, CDec(oReader.Item("Crit1")), CDec(oReader.Item("Crit2"))) * (dLimitFactor + 1))
                                        Else
                                            dEndorsementFactor += IIf(bMedExcluded, 0, CDec(oReader.Item("Factor")) * IIf(bOwnerOccupied, CDec(oReader.Item("Crit1")), CDec(oReader.Item("Crit2"))) * (dLimitFactor))
                                        End If
                                        Exit For
                                    End If
                                End If
                            Next
                        Next

                        dEndorsementFactor = RoundStandard(dEndorsementFactor, 0)
                    End If

                Case "TDP213"
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

                Case Else
                    dEndorsementFactor = 0
            End Select

            'oFactor.FactorAmt = RoundStandard(dEndorsementFactor, 0)

            Return IIf(dEndorsementFactor < 1, dEndorsementFactor, RoundStandard(dEndorsementFactor, 0))

        Catch ex As Exception
            Throw New ArgumentException("ErrorMsg:" & ex.Message & ex.StackTrace, ex)
        End Try

    End Function

    Public Overrides Sub MapObjects(ByRef oPolicy As clsPolicyHomeOwner)
        InitializeConnection()
        Dim sSql As String = ""
        Dim XRefTable As DataTable = Nothing
        Dim drXRefRows() As DataRow = Nothing
        Dim bCovSet As Boolean = False
        Dim oEndorsement As clsEndorsementFactor = Nothing

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Source, CodeType, Code, MappingCode1, MappingCode2, MappingCode3 FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "CodeXRef with(nolock)"
                sSql = sSql & " WHERE Source = @Source "
                sSql = sSql & " ORDER BY CodeType, Code "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Source", SqlDbType.VarChar, 20).Value = "PAS" 'oPolicy.CallingSystem

                Dim adp As New SqlDataAdapter(cmd)
                Dim ds As New DataSet()
                adp.Fill(ds)

                XRefTable = ds.Tables.Item(0)

            End Using

            For Each oUnit As clsDwellingUnit In oPolicy.DwellingUnits
                For Each oCov As clsHomeOwnerCoverage In oUnit.Coverages
                    If Not String.IsNullOrEmpty(oCov.SystemCode) Then
                        'we need to map this guy
                        'using oCov.SystemCode find/set the MappingCode1/CovGroup and Type
                        drXRefRows = XRefTable.Select("Code='" & oCov.SystemCode & "' AND CodeType='Coverage'")

                        If drXRefRows.Count > 0 Then
                            oCov.CovGroup = drXRefRows(0).Item("MappingCode1")
                            oCov.SystemCode = ""

                            If drXRefRows(0).Item("MappingCode2") IsNot System.DBNull.Value Then
                                oCov.Type = drXRefRows(0).Item("MappingCode2")
                            Else
                                oCov.Type = "N"
                            End If
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
                        Case "HO215", "HO210"
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
            moConn.Close()
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
                            oFee.FeeType = oReader.Item("FeeCode").ToString
                            oFee.FeeNum = oPolicy.Fees.Count + 1
                            oFee.IndexNum = oPolicy.Fees.Count + 1
                        Case "HOMEINSP1"
                            'HOMEINSP1	Only applies if 25% or 50% Mold Buyback is selected
                            For i As Integer = 0 To 7
                                Select Case i
                                    Case 0
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO161-25")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 1
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO161-50")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 2
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO162-25")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 3
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO162-50")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 4
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO164-25")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 5
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO164-50")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 6
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "TDP004-25")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 7
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "TDP004-50")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                End Select
                            Next

                        Case "HOMEINSP2"
                            'HOMEINSP2	Only applies if 100% Mold Buyback is selected
                            For i As Integer = 0 To 3
                                Select Case i
                                    Case 0
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO161-100")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 1
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO162-100")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 2
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "HO164-100")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                    Case 3
                                        If Not oEndorse Is Nothing Then
                                            oEndorse = Nothing
                                        End If
                                        oEndorse = GetEndorsement(oPolicy, "TDP004-100")
                                        If Not oEndorse Is Nothing Then
                                            If Not oEndorse Is Nothing Then
                                                oEndorse = Nothing
                                            End If
                                            'has endorsement
                                            oFee = New clsBaseFee
                                            oFee.FeeCode = oReader.Item("FeeCode").ToString
                                            oFee.FeeDesc = oReader.Item("Description").ToString
                                            oFee.FeeName = oReader.Item("Description").ToString
                                            oFee.FeeType = oReader.Item("FeeCode").ToString
                                            oFee.FeeNum = oPolicy.Fees.Count + 1
                                            oFee.IndexNum = oPolicy.Fees.Count + 1
                                            Exit For
                                        End If
                                End Select
                            Next

                            ' Removed 4/6/2010 
                            ' Per(David) 's request please add an Improve item for Texas Property requesting that
                            ' we remove the $25.00 inspection fee which is being added to policies with an age 
                            ' of home 20 years or greater.  This amount should be refunded on any policy which received the charge.
                            '
                            ' 11/11/2011: Re-Adding HOMEINSP3 as a flat fee on all new business policies
                        Case "HOMEINSP3"
                            If oPolicy.Type.ToUpper = "NEW" Then
                                oFee = New clsBaseFee
                                oFee.FeeCode = oReader.Item("FeeCode").ToString
                                oFee.FeeDesc = oReader.Item("Description").ToString
                                oFee.FeeName = oReader.Item("Description").ToString
                                oFee.FeeType = oReader.Item("FeeCode").ToString
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
        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If
        End Try

    End Sub



    Public Function GetMinimumPremium(ByVal oPolicy As clsPolicyHomeOwner) As Decimal

        Dim DataRows() As DataRow
        Dim oStateInfoTable As DataTable = Nothing
        oStateInfoTable = moStateInfoDataSet.Tables(0)
        Dim dMinimumPremium As Decimal = -1

        DataRows = oStateInfoTable.Select("Program IN ('HOM', '" & oPolicy.Program & "') AND ItemGroup='MINIMUM' AND ItemCode='PREMIUM' ")
        For Each oRow As DataRow In DataRows
            dMinimumPremium = CDec(oRow.Item("ItemValue").ToString)
        Next

        ' old minimums, if not overriden by stateinfo row
        If dMinimumPremium < 0 Then
            Select Case oPolicy.Program
                Case "HOA", "HOB"
                    dMinimumPremium = 300
                Case "TDP1"
                    dMinimumPremium = 175
                Case Else 'HOT
                    dMinimumPremium = 100
            End Select
        End If

        Return dMinimumPremium

    End Function

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

            Dim dMinimumPremium As Decimal = -1
            dMinimumPremium = GetMinimumPremium(oPolicy)
            Select Case oPolicy.Program
                Case "HOA", "HOB"
                    If dTotal < dMinimumPremium Then
                        dMinPremAmt = dMinimumPremium
                        bUpdatePrem = True
                        oPolicy.MinPremApplied = True
                    End If
                Case "TDP1"
                    If dTotal < dMinimumPremium Then
                        dMinPremAmt = dMinimumPremium
                        bUpdatePrem = True
                        oPolicy.MinPremApplied = True
                    End If
                Case Else 'HOT
                    If dTotal < dMinimumPremium Then
                        dMinPremAmt = dMinimumPremium
                        bUpdatePrem = True
                        oPolicy.MinPremApplied = True
                    End If
            End Select

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

    Public Overrides Function GetMinPremEndorsement(ByVal oEndorseFactor As clsEndorsementFactor, ByVal sFactorType As String) As Decimal

        Select Case oEndorseFactor.FactorCode
            Case "HO160-J", "HO160-O", "HO160"
                If oEndorseFactor.FactorAmt < 20 Then
                    oEndorseFactor.FactorAmt = 20
                End If
            Case "TDP017"
                If oEndorseFactor.FactorAmt < 10 Then
                    oEndorseFactor.FactorAmt = 10
                End If

            Case Else
                'don't do anything
        End Select

        ' TODO: Don't do this if it is a mid mult
        ' this is what is causing PAS to be different than what it rates at
        If sFactorType.ToUpper = "MIDMULT" Then
            Return 0
        Else
            Return RoundStandard(oEndorseFactor.FactorAmt, 0)
        End If

    End Function

    Public Overrides Function AllowAEC(ByVal oPolicy As clsPolicyHomeOwner) As Boolean

        Dim bAllowAEC As Boolean = False

        Dim bHasAECPlus As Boolean = False
        Dim bHasWaterBackUp As Boolean = False
        For Each oEndorse As clsEndorsementFactor In oPolicy.EndorsementFactors
            If Not oEndorse.IsMarkedForDelete Then
                If oEndorse.Type = "AEC_Plus" Then
                    bHasAECPlus = True
                ElseIf oEndorse.Type = "WaterBackUp" Then
                    bHasWaterBackUp = True
                End If
            End If
        Next
        If bHasAECPlus And bHasWaterBackUp Then
            bAllowAEC = True
        End If

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
                Case "HO161-25", "HO162-25", "HO164-25", "TDP004-25"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    oPolicy.EndorsementFactors.Item(i).Limit = oPolicy.DwellingUnits.Item(0).DwellingAmt * 0.25
                Case "HO161-50", "HO162-50", "HO164-50", "TDP004-50"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    oPolicy.EndorsementFactors.Item(i).Limit = oPolicy.DwellingUnits.Item(0).DwellingAmt * 0.5
                Case "HO161-100", "HO162-100", "HO164-100", "TDP004-100"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    oPolicy.EndorsementFactors.Item(i).Limit = oPolicy.DwellingUnits.Item(0).DwellingAmt
                Case "HO110"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    'add $500 to the limit amount selected since it is an increased limit from $500
                    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        If Not oUWQuestion.QuestionCode = "" Then
                            If oUWQuestion.QuestionCode.ToUpper = "173" Then
                                If Not oUWQuestion.AnswerText = "" Then
                                    If IsNumeric(oUWQuestion.AnswerText) Then
                                        oPolicy.EndorsementFactors.Item(i).Limit = CInt(oUWQuestion.AnswerText) + 500
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next

                    ' 6/27/2011 KB HO160-J and HO160-O are not used for Texas
                    ' only the HO160 is used
                    ' ''Case "HO160-J" 'Jewelry, Art, Stamp and Coin
                    ' ''    If oPolicy.CallingSystem <> "PAS" Then
                    ' ''        oPolicy.EndorsementFactors.Item(i).Limit = 0

                    ' ''        For x As Integer = 1 To oPolicy.EndorsementFactors.Item(i).NumberOfEndorsements
                    ' ''            For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                If oUWQuestion.IndexNum = x Then
                    ' ''                    If oUWQuestion.QuestionCode.ToUpper = "300" Or oUWQuestion.QuestionCode.ToUpper = "301" Then
                    ' ''                        Select Case oUWQuestion.AnswerText.ToUpper
                    ' ''                            Case "JEWELRY"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "303" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dJewelryTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "FINE ARTS", "FINE ARTS WITH BREAKAGE", "FINE ARTS - BREAKAGE"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "303" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dArtTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "STAMPS"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "303" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dStampTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "COINS"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "303" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dCoinTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "FIREARMS"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "303" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dFirearmsTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                        End Select
                    ' ''                    End If
                    ' ''                End If
                    ' ''            Next
                    ' ''        Next
                    ' ''        oPolicy.EndorsementFactors.Item(i).Limit = dJewelryTotalAmt + dArtTotalAmt + dStampTotalAmt + dCoinTotalAmt + dFirearmsTotalAmt
                    ' ''    End If
                    ' ''Case "HO160-O" 'Furs, Cameras, Musical Instruments, Silverware and Golf Equipment
                    ' ''    If oPolicy.CallingSystem <> "PAS" Then
                    ' ''        oPolicy.EndorsementFactors.Item(i).Limit = 0

                    ' ''        For x As Integer = 1 To oPolicy.EndorsementFactors.Item(i).NumberOfEndorsements
                    ' ''            For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                If oUWQuestion.IndexNum = x Then
                    ' ''                    If oUWQuestion.QuestionCode.ToUpper = "300" Or oUWQuestion.QuestionCode.ToUpper = "301" Then
                    ' ''                        Select Case oUWQuestion.AnswerText.ToUpper
                    ' ''                            Case "FURS"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "305" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dFurTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "CAMERAS"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "305" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dCamerasTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "MUSICAL INSTRUMENTS", "PROFESSIONAL MUSICAL INSTRUMENTS"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "305" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dMusicTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "SILVERWARE"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "305" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dSilverwareTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                            Case "GOLF EQUIPMENT"
                    ' ''                                'loop through the questions again and find the amt for the index
                    ' ''                                For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                    ' ''                                    If oUWQ.IndexNum = x Then
                    ' ''                                        If oUWQ.QuestionCode.ToUpper = "305" Then
                    ' ''                                            If oUWQ.AnswerText <> "" Then
                    ' ''                                                dGolfTotalAmt += CDec(oUWQ.AnswerText)
                    ' ''                                            End If
                    ' ''                                        End If
                    ' ''                                    End If
                    ' ''                                Next
                    ' ''                        End Select
                    ' ''                    End If
                    ' ''                End If
                    ' ''            Next
                    ' ''        Next

                    ' ''        oPolicy.EndorsementFactors.Item(i).Limit = dFurTotalAmt + dCamerasTotalAmt + dMusicTotalAmt + dSilverwareTotalAmt + dGolfTotalAmt
                    ' ''    End If
                Case "HO120"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0
                    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        If Not oUWQuestion.QuestionCode = "" Then
                            If oUWQuestion.QuestionCode.ToUpper = "166" Then
                                If Not oUWQuestion.AnswerText = "" Then
                                    If IsNumeric(oUWQuestion.AnswerText) Then
                                        oPolicy.EndorsementFactors.Item(i).Limit = CInt(oUWQuestion.AnswerText)
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                Case "TDP017"
                    oPolicy.EndorsementFactors.Item(i).Limit = 0


                    Dim iTotalRentalAmt As Integer = 0
                    Dim iRentalAmt As Integer = 0
                    Dim iTerm As Integer = 0
                    For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                        If oUWQuestion.QuestionCode = "202" Then 'Coverage Amount
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    iRentalAmt = CInt(oUWQuestion.AnswerText)
                                End If
                            End If
                        ElseIf oUWQuestion.QuestionCode = "221" Then 'Term
                            If oUWQuestion.AnswerText <> "" Then
                                If IsNumeric(oUWQuestion.AnswerText) Then
                                    iTerm = CInt(oUWQuestion.AnswerText)
                                End If
                            End If
                        End If
                    Next
                    oPolicy.EndorsementFactors.Item(i).Limit = CInt(iRentalAmt * iTerm)
                Case "HO160" 'scheduled prop
                    If oPolicy.CallingSystem <> "PAS" Then
                        oPolicy.EndorsementFactors.Item(i).Limit = 0

                        For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                            Select Case oProperty.PropertyCategoryDesc.ToUpper
                                Case "JEWELRY"
                                    dJewelryTotalAmt += oProperty.PropertyAmt
                                Case "FINE ARTS", "FINE ARTS WITH BREAKAGE", "FINE ARTS - BREAKAGE"
                                    dArtTotalAmt += oProperty.PropertyAmt
                                Case "STAMPS"
                                    dStampTotalAmt += oProperty.PropertyAmt
                                Case "COINS"
                                    dCoinTotalAmt += oProperty.PropertyAmt
                                Case "FIREARMS"
                                    dFirearmsTotalAmt += oProperty.PropertyAmt
                                Case "FURS"
                                    dFurTotalAmt += oProperty.PropertyAmt
                                Case "CAMERAS"
                                    dCamerasTotalAmt += oProperty.PropertyAmt
                                Case "MUSICAL INSTRUMENTS", "PROFESSIONAL MUSICAL INSTRUMENTS"
                                    dMusicTotalAmt += oProperty.PropertyAmt
                                Case "SILVERWARE"
                                    dSilverwareTotalAmt += oProperty.PropertyAmt
                                Case "GOLF EQUIPMENT"
                                    dGolfTotalAmt += oProperty.PropertyAmt
                            End Select


                            ' removed 6/27/2011 replaced with HomeScheduledProperty object
                            'For Each oUWQuestion As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '    If oUWQuestion.IndexNum = x Then
                            '        If oUWQuestion.QuestionCode.ToUpper = "300" Or oUWQuestion.QuestionCode.ToUpper = "301" Then
                            '            Select Case oUWQuestion.AnswerText.ToUpper
                            '                Case "JEWELRY"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dJewelryTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "FINE ARTS", "FINE ARTS WITH BREAKAGE", "FINE ARTS - BREAKAGE"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dArtTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "STAMPS"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dStampTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "COINS"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dCoinTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "FIREARMS"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dFirearmsTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "FURS"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dFurTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "CAMERAS"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dCamerasTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "MUSICAL INSTRUMENTS", "PROFESSIONAL MUSICAL INSTRUMENTS"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dMusicTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "SILVERWARE"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dSilverwareTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '                Case "GOLF EQUIPMENT"
                            '                    'loop through the questions again and find the amt for the index
                            '                    For Each oUWQ As clsUWQuestion In oPolicy.EndorsementFactors.Item(i).UWQuestions
                            '                        If oUWQ.IndexNum = x Then
                            '                            If oUWQ.QuestionCode.ToUpper = "303" Then
                            '                                If oUWQ.AnswerText <> "" Then
                            '                                    dGolfTotalAmt += CDec(oUWQ.AnswerText)
                            '                                End If
                            '                            End If
                            '                        End If
                            '                    Next
                            '            End Select
                            '        End If
                            '    End If
                            'Next
                        Next
                        oPolicy.EndorsementFactors.Item(i).Limit = dJewelryTotalAmt + dArtTotalAmt + dStampTotalAmt + dCoinTotalAmt + dFirearmsTotalAmt + dFurTotalAmt + dCamerasTotalAmt + dMusicTotalAmt + dSilverwareTotalAmt + dGolfTotalAmt

                    End If
            End Select
        Next

    End Sub

    Public Overrides Function dbGetDed1Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded1) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart <= @ContentsAmt "
                    sSql = sSql & " AND CovAmtEnd >= @ContentsAmt "
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    'sSql = sSql & " AND ((CovAmtStart < @DwellingAmt "
                    'sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    'sSql = sSql & " AND Type = 'D' ) "
                    'sSql = sSql & " OR (CovAmtStart < @DwellingAmt "
                    'sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    'sSql = sSql & " AND Type = 'D' )) "
                    sSql = sSql & " AND CovAmtStart <= @DwellingAmt "
                    sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                End If
                'End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Ded1", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded1
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5)
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Ded1"
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

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed1 with(nolock)"
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

    Public Overrides Function dbGetDed2Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed2 with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded2) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart <= @ContentsAmt "
                    sSql = sSql & " AND CovAmtEnd > @ContentsAmt "
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    'sSql = sSql & " AND ((CovAmtStart < @DwellingAmt "
                    'sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    'sSql = sSql & " AND Type = 'D' ) "
                    'sSql = sSql & " OR (CovAmtStart < @DwellingAmt "
                    'sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    'sSql = sSql & " AND Type = 'D' )) "
                    sSql = sSql & " AND CovAmtStart <= @DwellingAmt "
                    sSql = sSql & " AND CovAmtEnd > @DwellingAmt "
                End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Ded2", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded2
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5)
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Ded2"
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

    Public Overrides Function dbGetDed3Factor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed3 with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded3) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart <= @ContentsAmt "
                    sSql = sSql & " AND CovAmtEnd >= @ContentsAmt "
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    'sSql = sSql & " AND ((CovAmtStart < @DwellingAmt "
                    'sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    'sSql = sSql & " AND Type = 'D' ) "
                    'sSql = sSql & " OR (CovAmtStart < @DwellingAmt "
                    'sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                    'sSql = sSql & " AND Type = 'D' )) "
                    sSql = sSql & " AND CovAmtStart <= @DwellingAmt "
                    sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@Ded3", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded3
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                If StateInfoContainsProgramSpecific("RATING", "FACTORAMTOFINSURANCE", "MULTCONTENTS", oPolicy.Product & oPolicy.StateCode, oPolicy.AppliesToCode, oPolicy.Program, oPolicy.RateDate) Then
                    cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5)
                Else
                    cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).ContentsAmt
                End If
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

                            sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed3 with(nolock)"
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

    Public Overrides Function dbGetDedFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim oDS As New DataSet


        Try
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT DISTINCT(FactorCode) FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed with(nolock)"
                sSql &= " WHERE Program = @Program "
                sSql &= " AND EffDate <= @RateDate "
                sSql &= " AND ExpDate > @RateDate "
                sSql &= " AND AppliesToCode IN ('B',  @AppliesToCode ) "

                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                Dim adapter As New System.Data.SqlClient.SqlDataAdapter(cmd)

                adapter.Fill(oDS, "Deds")

            End Using

            sSql = ""

            Dim oUniqueDedsTable As New DataTable("Deds")
            oUniqueDedsTable = oDS.Tables.Item(0)
            For Each oRow As DataRow In oUniqueDedsTable.Rows
                If DedOnPolicy(oPolicy, oRow.Item("FactorCode").ToString) Then

                    ' Prior to 4/1/2010 rate change
                    ' use this method
                    Using cmd As New SqlCommand(sSql, moConn)

                        sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed with(nolock)"
                        sSql = sSql & " WHERE Program = @Program "
                        sSql = sSql & " AND EffDate <= @RateDate "
                        sSql = sSql & " AND ExpDate > @RateDate "
                        sSql = sSql & " AND FactorCode = @FactorCode "
                        sSql = sSql & " AND Convert(Decimal(10,4),Deductible) = Convert(Decimal(10,4),@Ded) "
                        sSql = sSql & " AND Region IN ( @Region , '99') "
                        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                        sSql = sSql & " AND (Deductible1 = -1 AND Deductible2 = -1) "
                        If oPolicy.ProgramType = "TENANT" Then
                            sSql = sSql & " AND (CovAmtStart <= @ContentsAmt "
                            sSql = sSql & " AND CovAmtEnd > @ContentsAmt "
                            sSql = sSql & " AND Type = 'C' ) "
                        Else
                            sSql = sSql & " AND CovAmtStart <= @DwellingAmt "
                            sSql = sSql & " AND CovAmtEnd > @DwellingAmt "
                        End If
                        sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                        'Execute the query
                        cmd.CommandText = sSql

                        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode

                        cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oRow.Item("FactorCode").ToString
                        Select Case oRow.Item("FactorCode").ToString.ToUpper
                            Case "DED1"
                                cmd.Parameters.Add("@Ded", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded1
                            Case "DED2"
                                cmd.Parameters.Add("@Ded", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded2
                            Case "DED3"
                                cmd.Parameters.Add("@Ded", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded3
                            Case "EC"
                                cmd.Parameters.Add("@Ded", SqlDbType.Decimal, 5).Value = oPolicy.DwellingUnits.Item(0).Ded1
                        End Select

                        cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                        cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5)
                        cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

                        oReader = cmd.ExecuteReader

                        If oReader.HasRows Then
                            drFactorRow = FactorTable.NewRow
                            drFactorRow.Item("FactorName") = oRow.Item("FactorCode").ToString
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


                End If
                If Not drFactorRow Is Nothing Then
                    FactorTable.Rows.Add(drFactorRow)
                    drFactorRow = Nothing
                End If
            Next

            ' New Method (after 4/1/2010 rate change
            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorDed with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible1) = Convert(Decimal(10,4),@Ded1) "
                sSql = sSql & " AND Convert(Decimal(10,4),Deductible2) = Convert(Decimal(10,4),@Ded2) "
                sSql = sSql & " AND Region IN ( @Region , '99') "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Deductible = -1  "
                If oPolicy.ProgramType = "TENANT" Then
                    sSql = sSql & " AND (CovAmtStart <= @ContentsAmt "
                    sSql = sSql & " AND CovAmtEnd > @ContentsAmt "
                    sSql = sSql & " AND Type = 'C' ) "
                Else
                    sSql = sSql & " AND CovAmtStart <= @DwellingAmt "
                    sSql = sSql & " AND CovAmtEnd >= @DwellingAmt "
                End If
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                If oPolicy.DwellingUnits.Item(0).Ded1 = 0 Then
                    If GetEndorsement(oPolicy, "HO140") IsNot Nothing OrElse GetEndorsement(oPolicy, "TDP001") IsNot Nothing Then
                        cmd.Parameters.Add("@Ded1", SqlDbType.VarChar, 50).Value = "0.01"
                    Else
                        cmd.Parameters.Add("@Ded1", SqlDbType.VarChar, 50).Value = oPolicy.DwellingUnits.Item(0).Ded1
                    End If
                Else
                    cmd.Parameters.Add("@Ded1", SqlDbType.VarChar, 50).Value = oPolicy.DwellingUnits.Item(0).Ded1
                End If
                cmd.Parameters.Add("@Ded2", SqlDbType.VarChar, 50).Value = oPolicy.DwellingUnits.Item(0).Ded2
                cmd.Parameters.Add("@Region", SqlDbType.VarChar, 3).Value = oPolicy.DwellingUnits.Item(0).Region
                cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = (oPolicy.DwellingUnits.Item(0).ContentsAmt * 2.5)
                cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).DwellingAmt

                oReader = cmd.ExecuteReader

                If oReader.HasRows Then
                    drFactorRow = FactorTable.NewRow
                    drFactorRow.Item("FactorName") = "Ded"
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

            If Not drFactorRow Is Nothing Then
                FactorTable.Rows.Add(drFactorRow)
            End If
            ' End New Method


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

    Public Overrides Function dbGetAPSFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try

            'In TX we are dividing the factor by the number of Dwelling coverages on the policy in order to spread the premium across coverages

            drFactorRow = FactorTable.NewRow
            drFactorRow.Item("FactorName") = "APS"

            'Dim iNumOfDwellingCovs As Integer = 0
            'For Each oCov As clsHomeOwnerCoverage In oPolicy.DwellingUnits(0).Coverages
            '    If oCov.Type.ToUpper = "D" Then
            '        iNumOfDwellingCovs += 1
            '    End If
            'Next

            Using cmd As New SqlCommand(sSql, moConn)

                sSql = " SELECT Coverage, Type, Factor, FactorType FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorAPS with(nolock)"
                sSql = sSql & " WHERE Program = @Program "
                sSql = sSql & " AND EffDate <= @RateDate "
                sSql = sSql & " AND ExpDate > @RateDate "
                sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                sSql = sSql & " AND Amount = @APSCovAmount "
                'sSql = sSql & " AND Coverage = @Coverage "
                sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                'Execute the query
                cmd.CommandText = sSql

                cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                cmd.Parameters.Add("@APSCovAmount", SqlDbType.Int, 22).Value = oPolicy.DwellingUnits.Item(0).OtherStructureAddnAmt
                'cmd.Parameters.Add("@Coverage", SqlDbType.VarChar, 11).Value = oPolicy.DwellingUnits(0).Coverages.Item(x).CovGroup

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

            If Not oReader Is Nothing Then
                oReader.Close()
                oReader = Nothing
            End If

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

    Public Overrides Function dbGetEndorsementFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False
        Dim sFactorCode As String = ""
        Dim dFactor As Decimal = 0

        Dim dJewelryTotalAmt As Decimal = 0
        Dim dFurTotalAmt As Decimal = 0
        Dim dCamerasTotalAmt As Decimal = 0
        Dim dMusicTotalAmt As Decimal = 0
        Dim dProMusicTotalAmt As Decimal = 0
        Dim dSilverwareTotalAmt As Decimal = 0
        Dim dGolfTotalAmt As Decimal = 0
        Dim dArtTotalAmt As Decimal = 0
        Dim dArtBreakageTotalAmt As Decimal = 0
        Dim dStampTotalAmt As Decimal = 0
        Dim dCoinTotalAmt As Decimal = 0
        Dim dFirearmsTotalAmt As Decimal = 0

        Dim oSchedPropTable As DataTable = Nothing
        Dim drSchedPropTypeRow As DataRow = Nothing
        Dim sSchedPropFactorType As String = ""

        Try

            For Each oFactor As clsEndorsementFactor In oPolicy.EndorsementFactors
                If Not oFactor.IsMarkedForDelete Then
                    dFactor = 0

                    bFactorType = False
                    'if this is scheduled property then we need to loop through all of the endorsement types and calculate each types premium and then add them up
                    If oFactor.Type.ToUpper.Contains("SCHEDULEDPROPERTY") Then
                        If oSchedPropTable Is Nothing Then
                            'create table with a row for each type of scheduled property on the endorsement
                            oSchedPropTable = CreateDataTable("ScheduledProperty", oPolicy.Program, oPolicy.RateDate, oPolicy.AppliesToCode, oPolicy.Product, oPolicy.StateCode)
                            oSchedPropTable.Rows.Add(CreateTotalsRow(oSchedPropTable))
                        End If

                        For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                            Select Case oProperty.PropertyCategoryDesc.ToUpper
                                Case "JEWELRY"
                                    dJewelryTotalAmt += oProperty.PropertyAmt
                                Case "FINE ARTS"
                                    dArtTotalAmt += oProperty.PropertyAmt
                                Case "FINE ARTS WITH BREAKAGE", "FINE ARTS - BREAKAGE"
                                    dArtBreakageTotalAmt += oProperty.PropertyAmt
                                Case "STAMPS"
                                    dStampTotalAmt += oProperty.PropertyAmt
                                Case "COINS"
                                    dCoinTotalAmt += oProperty.PropertyAmt
                                Case "FIREARMS"
                                    dFirearmsTotalAmt += oProperty.PropertyAmt
                                Case "FURS"
                                    dFurTotalAmt += oProperty.PropertyAmt
                                Case "CAMERAS", "CAMERA, FILMS AND RELATED"
                                    dCamerasTotalAmt += oProperty.PropertyAmt
                                Case "MUSICAL INSTRUMENTS"
                                    dMusicTotalAmt += oProperty.PropertyAmt
                                Case "PROFESSIONAL MUSICAL INSTRUMENTS"
                                    dProMusicTotalAmt += oProperty.PropertyAmt
                                Case "SILVERWARE"
                                    dSilverwareTotalAmt += oProperty.PropertyAmt
                                Case "GOLF EQUIPMENT", "GOLFER'S EQUIPMENT"
                                    dGolfTotalAmt += oProperty.PropertyAmt
                            End Select
                        Next


                        For n As Integer = 1 To oPolicy.DwellingUnits(0).HomeScheduledProperty.Count
                            Select Case True
                                Case dJewelryTotalAmt > 0 And GetRow(oSchedPropTable, "JEWELRY") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "JEWELRY"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dArtTotalAmt > 0 And GetRow(oSchedPropTable, "FINE ARTS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "FINE ARTS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dStampTotalAmt > 0 And GetRow(oSchedPropTable, "STAMPS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "STAMPS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dCoinTotalAmt > 0 And GetRow(oSchedPropTable, "COINS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "COINS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dFirearmsTotalAmt > 0 And GetRow(oSchedPropTable, "FIREARMS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "FIREARMS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dFurTotalAmt > 0 And GetRow(oSchedPropTable, "FURS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "FURS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dCamerasTotalAmt > 0 And GetRow(oSchedPropTable, "CAMERAS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "CAMERAS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dMusicTotalAmt > 0 And GetRow(oSchedPropTable, "MUSICAL INSTRUMENTS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "MUSICAL INSTRUMENTS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dSilverwareTotalAmt > 0 And GetRow(oSchedPropTable, "SILVERWARE") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "SILVERWARE"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dGolfTotalAmt > 0 And GetRow(oSchedPropTable, "GOLF EQUIPMENT") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "GOLF EQUIPMENT"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dArtBreakageTotalAmt > 0 And GetRow(oSchedPropTable, "FINE ARTS WITH BREAKAGE") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "FINE ARTS WITH BREAKAGE"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                                Case dProMusicTotalAmt > 0 And GetRow(oSchedPropTable, "PROFESSIONAL MUSICAL INSTRUMENTS") Is Nothing
                                    'add row to our little endorsement table
                                    drSchedPropTypeRow = oSchedPropTable.NewRow
                                    drSchedPropTypeRow.Item("FactorName") = "PROFESSIONAL MUSICAL INSTRUMENTS"
                                    If Not drSchedPropTypeRow Is Nothing Then
                                        oSchedPropTable.Rows.Add(drSchedPropTypeRow)
                                        drSchedPropTypeRow = Nothing
                                    End If
                            End Select
                        Next
                    End If


                    Using cmd As New SqlCommand(sSql, moConn)

                        sSql = " SELECT FactorCode, Coverage, Type, Factor, FactorType, Crit1, Crit2, Crit3, Crit4, Description FROM pgm" & oPolicy.Product & oPolicy.StateCode & ".." & "FactorEndorsement with(nolock)"
                        sSql = sSql & " WHERE Program = @Program "
                        sSql = sSql & " AND EffDate <= @RateDate "
                        sSql = sSql & " AND ExpDate > @RateDate "
                        sSql = sSql & " AND AppliesToCode IN ('B',  @AppliesToCode ) "
                        sSql = sSql & " AND FactorCode = @FactorCode "
                        sSql = sSql & " ORDER BY Coverage Asc, Type Asc "

                        'Execute the query
                        cmd.CommandText = sSql

                        cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = oPolicy.Program
                        cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = oPolicy.RateDate
                        cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = oPolicy.AppliesToCode
                        cmd.Parameters.Add("@FactorCode", SqlDbType.VarChar, 20).Value = oFactor.FactorCode

                        oReader = cmd.ExecuteReader

                        If oReader.HasRows Then
                            drFactorRow = FactorTable.NewRow
                            drFactorRow.Item("FactorName") = oFactor.FactorCode & "-ENDORSE"
                        End If

                        Dim dCappedFActor As Decimal = 1.0
                        Dim dCappedRenewalFactor As Decimal = 1.0
                        For Each oPolicyFactor As clsBaseFactor In oPolicy.PolicyFactors
                            If oPolicyFactor.FactorCode = "CAPPED_RENEWAL" Then
                                dCappedRenewalFactor *= CType(oPolicyFactor.RatedFactor, Decimal)
                            End If
                        Next

                        Do While oReader.Read()
                            If oReader.Item("FactorCode").ToString = oFactor.FactorCode Then
                                oFactor.CovType = oReader.Item("Type").ToString
                            End If

                            ' if this is a mid mult, don't use the renewal factor
                            If oReader.Item("FactorType").ToString.Contains("Mult") Then
                                dCappedFActor = 1.0
                            Else
                                dCappedFActor = dCappedRenewalFactor
                            End If

                            If oFactor.Type.ToUpper.Contains("SCHEDULEDPROPERTY") Then
                                If sSchedPropFactorType = "" Then sSchedPropFactorType = oReader.Item("FactorType").ToString

                                Select Case oReader.Item("Description").ToString.ToUpper
                                    Case "JEWELRY"
                                        If dJewelryTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                log4net.Debug("INSIDE JEWELRY")
                                                log4net.Debug(oSchedPropTable.Columns.Count)
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dJewelryTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    log4net.Debug(dSchedPropFactor)
                                                    log4net.Debug(drSchedPropTypeRow)
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "FINE ARTS"
                                        If dArtTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dArtTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "FINE ARTS WITH BREAKAGE", "FINE ARTS - BREAKAGE"
                                        If dArtBreakageTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dArtBreakageTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "STAMPS"
                                        If dStampTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dStampTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "COINS"
                                        If dCoinTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dCoinTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "FIREARMS"
                                        If dFirearmsTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dFirearmsTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "FURS"
                                        If dFurTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dFurTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "CAMERAS", "CAMERA, FILMS AND RELATED"
                                        If dCamerasTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dCamerasTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "MUSICAL INSTRUMENTS"
                                        If dMusicTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dMusicTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "PROFESSIONAL MUSICAL INSTRUMENTS"
                                        If dProMusicTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dProMusicTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "SILVERWARE"
                                        If dSilverwareTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dSilverwareTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "GOLF EQUIPMENT", "GOLFER'S EQUIPMENT"
                                        If dGolfTotalAmt > 0 Then
                                            For i As Integer = 1 To oSchedPropTable.Columns.Count - 1
                                                If oReader.Item("Coverage") & "_" & oReader.Item("Type") = oSchedPropTable.Columns.Item(i).ColumnName Then
                                                    'add it to the data row
                                                    Dim dSchedPropFactor As Decimal = 0
                                                    dSchedPropFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, oSchedPropTable, dGolfTotalAmt, oReader.Item("Description").ToString) * dCappedFActor
                                                    drSchedPropTypeRow = GetRow(oSchedPropTable, oReader.Item("Description").ToString.ToUpper)
                                                    drSchedPropTypeRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dSchedPropFactor
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                End Select

                            Else
                                'If IsMoldBuyBackEndorsement(oPolicy, oFactor.FactorCode) Then
                                If IsFlatFactorEndorsement(oPolicy, oFactor.FactorCode, moStateInfoDataSet) Then
                                    'moldbuyback, water back up (LA)
                                    'not at the coverage level so add it to the flat factor column
                                    If dFactor = 0 Then
                                        dFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, FactorTable) * dCappedFActor
                                        drFactorRow.Item("FlatFactor") = dFactor
                                    End If
                                Else
                                    'this returns the factor and factor type for all coverages
                                    'we will start with the 2nd column since we know the 1st is the factor name
                                    For i As Integer = 1 To FactorTable.Columns.Count - 1
                                        If oReader.Item("Coverage") & "_" & oReader.Item("Type") = FactorTable.Columns.Item(i).ColumnName Then
                                            'add it to the data row
                                            dFactor = CalculateEndorsementFactor(oReader, oFactor, oPolicy, FactorTable) * dCappedFActor

                                            drFactorRow.Item(oReader.Item("Coverage") & "_" & oReader.Item("Type")) = dFactor
                                            Exit For
                                        End If
                                    Next
                                    If Not bFactorType Then
                                        drFactorRow.Item("FactorType") = oReader.Item("FactorType")
                                        bFactorType = True
                                    End If
                                End If
                            End If
                        Loop

                        If oFactor.Type.ToUpper.Contains("SCHEDULEDPROPERTY") Then
                            'use our scheduled property table to add the row to the factor table
                            ''UpdateTotals(oPolicy, oSchedPropTable)
                            If Not oSchedPropTable Is Nothing Then
                                Dim drTotalsRow As DataRow = Nothing
                                log4net.Debug(oSchedPropTable.Rows.Count)

                                drTotalsRow = GetRow(oSchedPropTable, "Totals")
                                log4net.Debug(drTotalsRow)


                                If Not drTotalsRow Is Nothing Then
                                    For Each oDataCol As DataColumn In oSchedPropTable.Columns
                                        Dim dTotal As Decimal = 0
                                        If oDataCol.ColumnName.ToUpper = "FACTORTYPE" Then
                                            Exit For
                                        Else
                                            For Each oDataRow As DataRow In oSchedPropTable.Rows
                                                If oDataRow.Item(oDataCol.ColumnName) IsNot System.DBNull.Value Then
                                                    If IsNumeric(oDataRow.Item(oDataCol.ColumnName)) Then
                                                        dTotal += oDataRow.Item(oDataCol.ColumnName)

                                                    End If
                                                End If
                                            Next
                                        End If
                                        drTotalsRow.Item(oDataCol.ColumnName) = dTotal
                                        log4net.Debug(dTotal)

                                    Next
                                End If

                                drTotalsRow.Item("FactorType") = sSchedPropFactorType
                                drTotalsRow.Item("FactorName") = oFactor.FactorCode & "-ENDORSE"
                                For i As Integer = 1 To FactorTable.Columns.Count - 1
                                    If oSchedPropTable.Columns.Item(i).ColumnName = FactorTable.Columns.Item(i).ColumnName Then
                                        'add it to the data row

                                        drFactorRow.Item(i) = drTotalsRow.Item(i)
                                        log4net.Debug(drTotalsRow.Item(i))

                                        'Exit For
                                    End If
                                Next
                                If Not bFactorType Then
                                    drFactorRow.Item("FactorType") = drTotalsRow.Item("FactorType")
                                    bFactorType = True
                                End If
                            End If
                        End If
                    End Using
                    If Not drFactorRow Is Nothing Then
                        FactorTable.Rows.Add(drFactorRow)
                        drFactorRow = Nothing
                    End If
                    If Not oReader Is Nothing Then
                        oReader.Close()
                        oReader = Nothing
                    End If
                End If
            Next

            Return drFactorRow

        Catch ex As Exception
            log4net.Debug(ex.Message & ex.StackTrace)

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

    Public Overrides Sub GetFactors(ByVal oPolicy As clsPolicyHomeOwner, ByVal oFactorTable As DataTable)

        Try

            ' clear out scheduled property amoutns
            For Each oProperty As clsHomeScheduledProperty In oPolicy.DwellingUnits(0).HomeScheduledProperty
                oProperty.PropertyPremiumAmt = 0
            Next


            dbGetBaseRateFactor(oPolicy, oFactorTable)
            dbGetAmtOfInsuranceFactor(oPolicy, oFactorTable)
            If oPolicy.DwellingUnits(0).OtherStructureAddnAmt > 0 Then 'TX uses OtherStructureAddnAmt instead of just the OtherStructuresAmt
                dbGetAPSFactor(oPolicy, oFactorTable)
            End If
            dbGetDed1Factor(oPolicy, oFactorTable)
            dbGetDed2Factor(oPolicy, oFactorTable)
            dbGetDed3Factor(oPolicy, oFactorTable)
            dbGetDedFactor(oPolicy, oFactorTable)
            dbGetTerritoryFactor(oPolicy, oFactorTable)
            dbGetRegionFactor(oPolicy, oFactorTable)
            dbGetPCFactor(oPolicy, oFactorTable)
            'dbGetMoldBuyBackFactor(oPolicy, oFactorTable)
            dbGetTierMatrixFactor(oPolicy, oFactorTable, dbGetCreditTier(oPolicy), dbGetUWTier(oPolicy))
            dbGetPolicyFactor(oPolicy, oFactorTable)
            dbGetEndorsementFactor(oPolicy, oFactorTable)
            dbGetRatedFactor(oPolicy, oFactorTable)
            log4net.Debug(dbGetRatedFactor(oPolicy, oFactorTable))



        Catch ex As Exception
			Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally

        End Try
    End Sub


    Public Overrides Function dbGetRatedFactor(ByVal oPolicy As clsPolicyHomeOwner, ByVal FactorTable As DataTable) As System.Data.DataRow
        Dim sSql As String = ""
        Dim oReader As SqlDataReader = Nothing
        Dim drFactorRow As DataRow = Nothing
        Dim bFactorType As Boolean = False

        Try
            For Each oFactor As clsBaseFactor In oPolicy.PolicyFactors
                drFactorRow = FactorTable.NewRow
                drFactorRow.Item("FactorName") = oFactor.FactorCode

                For i As Integer = 1 To FactorTable.Columns.Count - 2
                    log4net.Debug(FactorTable.Columns.Count)
                    'add it to the data row
                    drFactorRow.Item(FactorTable.Columns.Item(i).ColumnName) = oFactor.RatedFactor
                    log4net.Debug(oFactor.RatedFactor)
                Next
                If oFactor.FactorType <> String.Empty Then
                    drFactorRow.Item("FactorType") = oFactor.FactorType.Trim
                    log4net.Debug(oFactor.FactorType.Trim)
                Else
                    drFactorRow.Item("FactorType") = "MidMult"
                    log4net.Debug("MidMult")
                End If

                If Not drFactorRow Is Nothing Then
                    Dim dRatedFactor As Decimal
                    Decimal.TryParse(oFactor.RatedFactor, dRatedFactor)
                    If dRatedFactor > 0 Then
                        FactorTable.Rows.Add(drFactorRow)
                    End If
                End If
            Next

            Return drFactorRow
            log4net.Debug(drFactorRow)

        Catch ex As Exception
            Throw New ArgumentException(ex.Message & ex.StackTrace, ex)
        Finally
            If Not drFactorRow Is Nothing Then
                drFactorRow = Nothing
            End If
        End Try
    End Function

   
End Class
