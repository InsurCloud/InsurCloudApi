Imports Microsoft.VisualBasic
Imports CorPolicy.clsCommonFunctions
Imports CorPolicy
Imports System.Data
Imports System.Data.SqlClient


Public Class clsLogging2
    Inherits clsLogging

    Protected moPolicy As CorPolicy.clsPolicyPPA

    Public Property Policy() As CorPolicy.clsPolicyPPA
        Get
            Return moPolicy
        End Get
        Set(ByVal value As CorPolicy.clsPolicyPPA)
            moPolicy = value
        End Set
    End Property

    Public Overrides Function GetInsertSQL() As String
        Dim ssql As String = ""
        ssql = " INSERT INTO " & GetTableOwner(Me.Policy.Product, Me.Policy.StateCode) & ".." & "EventLog"
        ssql = ssql & " VALUES ( @QuoteID , @PolicyID , @FilePath , @StartTS , @EndTS, @Premium , @Fees , @AgencyID , @UserID , @FirstName , @LastName, @LogXML ) "
        Return ssql
    End Function
    Public Overrides Sub GetInsertParms(ByVal sXML As System.Xml.XmlDocument, ByRef cmd As System.Data.SqlClient.SqlCommand)
        cmd.Parameters.Add("@QuoteID", SqlDbType.VarChar, 20).Value = IIf(Me.Policy.QuoteID = Nothing, DBNull.Value, Me.Policy.QuoteID)
        cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar, 20).Value = IIf(Me.Policy.PolicyID = Nothing, DBNull.Value, Me.Policy.PolicyID)
        cmd.Parameters.Add("@FilePath", SqlDbType.VarChar, 70).Value = ""
        cmd.Parameters.Add("@StartTS", SqlDbType.DateTime, 8).Value = IIf(Me.StartTimeStamp = Nothing, DBNull.Value, Me.StartTimeStamp)
        cmd.Parameters.Add("@EndTS", SqlDbType.DateTime, 8).Value = IIf(Me.EndTimeStamp = Nothing, DBNull.Value, Me.EndTimeStamp)
        cmd.Parameters.Add("@Premium", SqlDbType.Int, 22).Value = IIf(Me.Policy.FullTermPremium = Nothing, DBNull.Value, Me.Policy.FullTermPremium)
        Dim oParam As SqlParameter
        oParam = cmd.Parameters.Add("@Fees", SqlDbType.Decimal)
        oParam.Scale = 2
        oParam.Value = IIf(Me.Policy.TotalFees = Nothing, DBNull.Value, Me.Policy.TotalFees)
        cmd.Parameters.Add("@AgencyID", SqlDbType.VarChar, 50).Value = IIf(Me.Policy.Agency.AgencyID = Nothing, DBNull.Value, Me.Policy.Agency.AgencyID)
        cmd.Parameters.Add("@UserID", SqlDbType.VarChar, 50).Value = IIf(Me.Policy.UserID = Nothing, DBNull.Value, Me.Policy.UserID)
        cmd.Parameters.Add("@FirstName", SqlDbType.VarChar, 50).Value = IIf(Me.Policy.PolicyInsured.EntityName1 = Nothing, DBNull.Value, Me.Policy.PolicyInsured.EntityName1)
        cmd.Parameters.Add("@LastName", SqlDbType.VarChar, 50).Value = IIf(Me.Policy.PolicyInsured.EntityName2 = Nothing, DBNull.Value, Me.Policy.PolicyInsured.EntityName2)

        'cmd.Parameters.Add("@UnitNum", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).IndexNum = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).IndexNum)
        'cmd.Parameters.Add("@Territory", SqlDbType.VarChar, 5).Value = IIf(Me.Policy.DwellingUnits(0).Territory = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).Territory)
        'cmd.Parameters.Add("@Region", SqlDbType.VarChar, 5).Value = IIf(Me.Policy.DwellingUnits(0).Region = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).Region)
        'cmd.Parameters.Add("@Zip", SqlDbType.VarChar, 10).Value = IIf(Me.Policy.DwellingUnits(0).Zip = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).Zip)
        'cmd.Parameters.Add("@PlaceCode", SqlDbType.VarChar, 5).Value = IIf(Me.Policy.DwellingUnits(0).PlaceCode = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).PlaceCode)
        'cmd.Parameters.Add("@LossLevel", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).LossLevel = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).LossLevel)
        'cmd.Parameters.Add("@HomeAge", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).HomeAge = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).HomeAge)
        'cmd.Parameters.Add("@OwnerOccupiedFlag", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).OwnerOccupiedFlag = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).OwnerOccupiedFlag)
        'cmd.Parameters.Add("@ProtectionClass", SqlDbType.VarChar, 3).Value = IIf(Me.Policy.DwellingUnits(0).ProtectionClass = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).ProtectionClass)
        'cmd.Parameters.Add("@Construction", SqlDbType.VarChar, 20).Value = IIf(Me.Policy.DwellingUnits(0).Construction = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).Construction)
        'cmd.Parameters.Add("@DwellingAmt", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).DwellingAmt = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).DwellingAmt)
        'cmd.Parameters.Add("@ContentsAmt", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).ContentsAmt = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).ContentsAmt)
        'cmd.Parameters.Add("@FireDept", SqlDbType.VarChar, 50).Value = IIf(Me.Policy.DwellingUnits(0).FireDept = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).FireDept)
        'cmd.Parameters.Add("@OtherStructureAmt", SqlDbType.Int, 22).Value = IIf(Me.Policy.DwellingUnits(0).OtherStructureAmt = Nothing, DBNull.Value, Me.Policy.DwellingUnits(0).OtherStructureAmt)

        cmd.Parameters.Add("@LogXML", SqlDbType.Xml, 0).Value = sXML.OuterXml.ToString()
    End Sub
    Public Overrides Function GetInsertErrorSQL() As String
        Dim ssql As String = ""
        ssql = " INSERT INTO " & GetTableOwner(Me.Policy.Product, Me.Policy.StateCode) & ".." & "ErrorLog"
        ssql = ssql & " VALUES ( @QuoteID , @PolicyID , @StartTS , @EndTS,  @AgencyID , @MethodName , @ErrorMsg, @LogXML ) "
        Return ssql
    End Function

    Public Overrides Sub GetInsertErrorParms(ByVal sXML As System.Xml.XmlDocument, ByRef cmd As System.Data.SqlClient.SqlCommand, ByVal sMethodName As String, ByVal sErrorMsg As String)
        cmd.Parameters.Add("@QuoteID", SqlDbType.VarChar, 20).Value = IIf(Me.Policy.QuoteID = Nothing, DBNull.Value, Me.Policy.QuoteID)
        cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar, 20).Value = IIf(Me.Policy.PolicyID = Nothing, DBNull.Value, Me.Policy.PolicyID)
        cmd.Parameters.Add("@StartTS", SqlDbType.DateTime, 8).Value = IIf(Me.StartTimeStamp = Nothing, DBNull.Value, Me.StartTimeStamp)
        cmd.Parameters.Add("@EndTS", SqlDbType.DateTime, 8).Value = IIf(Me.EndTimeStamp = Nothing, DBNull.Value, Me.EndTimeStamp)
        cmd.Parameters.Add("@AgencyID", SqlDbType.VarChar, 50).Value = IIf(Me.Policy.Agency.AgencyID = Nothing, DBNull.Value, Me.Policy.Agency.AgencyID)
        cmd.Parameters.Add("@MethodName", SqlDbType.VarChar, 100).Value = IIf(sMethodName = Nothing, DBNull.Value, sMethodName)
        cmd.Parameters.Add("@ErrorMsg", SqlDbType.VarChar, 2000).Value = IIf(sErrorMsg = Nothing, DBNull.Value, sErrorMsg)
        cmd.Parameters.Add("@LogXML", SqlDbType.Xml, 0).Value = sXML.OuterXml.ToString()
    End Sub
End Class
