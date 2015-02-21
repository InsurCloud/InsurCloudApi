Imports Microsoft.VisualBasic
'Imports RatingRulesService2.gov.weather.www
Imports System.Xml
Imports RatingRulesLib.gov.weather.graphical
Imports System.Collections
Imports System.Collections.Specialized
Imports System.Configuration

Public Class Weather

#Region "Properties"
    Private dforcastDate As Date
    Public ReadOnly Property ForcastDate() As Date
        Get
            Return dforcastDate
        End Get
    End Property

    Private dLatitude As Double
    Public ReadOnly Property Latitude() As Double
        Get
            Return dLatitude
        End Get
    End Property

    Private dLongitude As Double
    Public ReadOnly Property Longitude() As Double
        Get
            Return dLongitude
        End Get
    End Property

    Private sWeatherConditions As String
    Public ReadOnly Property WeatherConditions() As String
        Get
            Return sWeatherConditions
        End Get
    End Property

    Private sHazardOutlook As String 'Convective Hazard Outlook
    Public ReadOnly Property HazardOutlook() As String
        Get
            Return sHazardOutlook
        End Get
    End Property

    Private iTemperature As Integer 'temperature in Fahrenheit
    Public ReadOnly Property Temperature() As String
        Get
            Return iTemperature
        End Get
    End Property

    Private iHumidity As Integer 'Relative Humidity
    Public ReadOnly Property Humidity() As String
        Get
            Return iHumidity
        End Get
    End Property

    Private iChanceOfRain As Integer '12 Hourly Probability of Precipitation
    Public ReadOnly Property ChanceOfRain() As String
        Get
            Return iChanceOfRain
        End Get
    End Property

    Private iSnowFall As Integer 'snow in inches
    Public ReadOnly Property SnowFall() As String
        Get
            Return iSnowFall
        End Get
    End Property

    Private oWeatherAlerts As NameValueCollection
    Public ReadOnly Property WeatherAlerts() As NameValueCollection
        Get
            Return oWeatherAlerts
        End Get
    End Property

    Private oWeatherWarnings As NameValueCollection
    Public ReadOnly Property WeatherWarnings() As NameValueCollection
        Get
            Return oWeatherWarnings
        End Get
    End Property

    Public ReadOnly Property HasWinterStormWarning() As Boolean
        Get
            If Not oWeatherWarnings.Get("WinterStormWarning") Is Nothing Then
                Return True
            Else
                Return False
            End If
            Return oWeatherAlerts.Get("Probability of Damaging Thunderstorm Winds")
        End Get
    End Property

    Public ReadOnly Property PStormWinds() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of Damaging Thunderstorm Winds")
        End Get
    End Property

    Public ReadOnly Property PExtremeStormWinds() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of Extreme Thunderstorm Winds")
        End Get
    End Property

    Public ReadOnly Property PHail() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of Hail")
        End Get
    End Property

    Public ReadOnly Property PExtremeHail() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of Extreme Hail")
        End Get
    End Property

    Public ReadOnly Property PTornado() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of Tornadoes")
        End Get
    End Property

    Public ReadOnly Property PExtremeTornado() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of Extreme Tornadoes")
        End Get
    End Property

    Public ReadOnly Property PMildTropicalStorm() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of a Tropical Cyclone Wind Speed above 34 Knots (Cumulative)")
        End Get
    End Property

    Public ReadOnly Property PTropicalStorm() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of a Tropical Cyclone Wind Speed above 50 Knots (Cumulative)")
        End Get
    End Property

    Public ReadOnly Property PExtremeTropicalStorm() As Integer
        Get
            Return oWeatherAlerts.Get("Probability of a Tropical Cyclone Wind Speed above 64 Knots (Cumulative)")
        End Get
    End Property

#End Region

    Public Sub New(ByVal zipCode As String)
        If Not String.IsNullOrEmpty(zipCode) Then
            'Time Stamp Vars for logging
            Dim startTime As DateTime = DateTime.Now
            Dim beforeCallingWeather As DateTime

            Dim conn As New Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
            conn.Open()
            Dim cmd As New Data.SqlClient.SqlCommand("SELECT LookupResult FROM Common..WeatherLookup (nolock) WHERE ZipCode = '" & zipCode & _
                "' AND LastLookupDate > DateAdd(hh, -2, GetDate())", conn)
            Try
                Dim result As String = cmd.ExecuteScalar()
                If result Is Nothing Then

                    ' if the last error was within 15 minutes, don't try again
                    ' look at lastupdatets where zip code is -1
                    cmd.CommandText = "SELECT LastLookupDate FROM Common..WeatherLookup (nolock) WHERE ZipCode = '-1' AND LastLookupDate > DateAdd(mi, -15, GetDate())"
                    result = cmd.ExecuteScalar()

                    If result Is Nothing Then
                        Try
                            'Get Latitude and Longitude form DB if present
                            LookUpLatitudeLongitudeInDB(zipCode)

                            Using WeatherService As RatingRulesLib.gov.weather.graphical.ndfdXMLPortTypeClient = New ndfdXMLPortTypeClient
                                Dim WeatherParameters As weatherParametersType = setWeatherParms()

                                'WeatherService.Timeout = 30000
                                Dim xmlResult As String

                                'if Latitude or Longitude is not in DB then Get form WeatherService
                                If (dLatitude = 0 Or dLongitude = 0) Then
                                    xmlResult = WeatherService.LatLonListZipCode(zipCode)

                                    Dim xmlDoc As XmlDocument = New XmlDocument()
                                    xmlDoc.LoadXml(xmlResult)

                                    Dim latlonlist() As String = xmlDoc.InnerText.Split(",")
                                    dLatitude = latlonlist(0)
                                    dLongitude = latlonlist(1)

                                    'Update Latitude and Longitude in DB
                                    UpdateLatitudeLongitudeInDB(zipCode, dLatitude, dLongitude)
                                End If

                                beforeCallingWeather = DateTime.Now

                                xmlResult = WeatherService.NDFDgen(dLatitude, dLongitude, "timeseries", Date.Now, Date.Now.AddDays(5), "e", WeatherParameters)
                                cmd.CommandText = "exec Common..UpdateWeatherLookup '" & zipCode & "','" & xmlResult.Replace("'", "''") & "'"
                                cmd.ExecuteNonQuery()
                                loadXML(xmlResult)
                            End Using
                        Catch ex As Exception
                            'Log the Error
                            'Dim exceptionTime As DateTime = DateTime.Now

                            'Dim errCtx As New CorFunctions.ExceptionContext(ex)

                            'errCtx.AddContext("ConstructorStartTime", startTime.ToString())
                            'errCtx.AddContext("BeforeCallingWeatherService", beforeCallingWeather.ToString())
                            'errCtx.AddContext("ExceptionTime", exceptionTime.ToString())
                            'errCtx.AddContext("ZipCode", zipCode)
                            'errCtx.AddContext("Latitude", dLatitude)
                            'errCtx.AddContext("Latitude", dLongitude)
                            'errCtx.AddContext("DBQueryResult", result)
                            'errCtx.SourceSystem = "RatingRulesWeather"
                            'errCtx.SystemTS = Now
                            'errCtx.LogError()


                            'If ex.Message.Contains("degrib") Or ex.Message.Contains("Error with one or more zip codes") Then
                            'Else
                            '    If Not ex.Message.Contains("valid points were found") Then
                            '        ' Zip code "-1" stores the timestamp of the last  failure of the weather service
                            '        ' we have encountered an error, so update the lastupdate timestamp on the error zip code
                            '        cmd.CommandText = "exec Common..UpdateWeatherLookup '-1','" & ex.Message & "'"
                            '        cmd.ExecuteNonQuery()
                            '    End If
                            '    Throw ex
                            'End If
                        End Try
                    Else
                        'Throw New Exception("WeatherLookup error in the last 15 minutes, skipping weather lookup")
                    End If
                Else
                    loadXML(result)
                End If
            Finally
                conn.Close()
            End Try
        End If
    End Sub

    Public Sub New(ByVal mLatitude As Double, ByVal mLongitude As Double)
        Using WeatherService As ndfdXMLPortTypeClient = New ndfdXMLPortTypeClient
            Dim WeatherParameters As weatherParametersType = setWeatherParms()

            'WeatherService.Timeout = 30000
            dLatitude = mLatitude
            dLongitude = mLongitude

            Dim xmlResult As String
            xmlResult = WeatherService.NDFDgen(mLatitude, mLongitude, "timeseries", Date.Now, Date.Now, "e", WeatherParameters) 'time-series returns all values for the set weather parms

            loadXML(xmlResult)
        End Using
    End Sub

    Public Function checkWeather() As Boolean
        Try
            If Me.PMildTropicalStorm > 20 Or Me.PTropicalStorm > 20 Or Me.PExtremeTropicalStorm > 20 Then
                Return False
            Else : Return True
            End If
        Catch
            Return True
        End Try
    End Function

    Private Sub loadXML(ByVal xmlResult As String)

        Dim xmlDoc As XmlDocument = New XmlDocument()
        xmlDoc.LoadXml(xmlResult)

        Dim xNode As XmlNode
        oWeatherAlerts = New NameValueCollection
        oWeatherWarnings = New NameValueCollection
        Try
            xNode = xmlDoc.SelectSingleNode("//dwml/head/product/creation-date")
            dforcastDate = xNode.InnerText
        Catch
            dforcastDate = Date.Now
        End Try

        xNode = xmlDoc.SelectSingleNode("//dwml/data/parameters")
        For Each xNode In xNode.ChildNodes
            Try

                Select Case xNode.Attributes.GetNamedItem("type").Value()
                    Case "apparent" 'temp
                        iTemperature = Integer.Parse(xNode.ChildNodes(1).InnerText)
                    Case "snow" 'inches of snowfall
                        iSnowFall = Integer.Parse(xNode.ChildNodes(1).InnerText)
                    Case "12 hour"
                        iChanceOfRain = Integer.Parse(xNode.ChildNodes(1).InnerText)
                    Case "cumulative34" 'tropical storm
                        oWeatherAlerts.Add(xNode.ChildNodes(0).InnerText, getMaxWarning(xNode))
                    Case "cumulative50"
                        oWeatherAlerts.Add(xNode.ChildNodes(0).InnerText, getMaxWarning(xNode))
                    Case "cumulative64"
                        oWeatherAlerts.Add(xNode.ChildNodes(0).InnerText, getMaxWarning(xNode))
                    Case "relative"
                        iHumidity = Integer.Parse(xNode.ChildNodes(1).InnerText)
                End Select
            Catch ex As Exception
                Try
                    Select Case xNode.ChildNodes(0).Attributes.GetNamedItem("type").Value()
                        Case "tornadoes" 'temp
                            oWeatherAlerts.Add(xNode.ChildNodes(0).ChildNodes(0).InnerText, xNode.ChildNodes(0).ChildNodes(1).InnerText)
                        Case "hail" 'inches of snowfall
                            oWeatherAlerts.Add(xNode.ChildNodes(0).ChildNodes(0).InnerText, xNode.ChildNodes(0).ChildNodes(1).InnerText)
                        Case "damaging thunderstorm winds"
                            oWeatherAlerts.Add(xNode.ChildNodes(0).ChildNodes(0).InnerText, xNode.ChildNodes(0).ChildNodes(1).InnerText)
                        Case "extreme tornadoes" 'tropical storm
                            oWeatherAlerts.Add(xNode.ChildNodes(0).ChildNodes(0).InnerText, xNode.ChildNodes(0).ChildNodes(1).InnerText)
                        Case "extreme hail"
                            oWeatherAlerts.Add(xNode.ChildNodes(0).ChildNodes(0).InnerText, xNode.ChildNodes(0).ChildNodes(1).InnerText)
                        Case "extreme thunderstorm winds"
                            oWeatherAlerts.Add(xNode.ChildNodes(0).ChildNodes(0).InnerText, xNode.ChildNodes(0).ChildNodes(1).InnerText)
                    End Select
                Catch
                End Try
            End Try
        Next

        If Not xmlDoc.SelectNodes("//dwml/data/parameters/hazards/hazard-conditions/hazard") Is Nothing Then
            For Each xNode In xmlDoc.SelectNodes("//dwml/data/parameters/hazards/hazard-conditions/hazard") 'xNode.ChildNodes
                Try

                    Select Case xNode.Attributes.GetNamedItem("hazardCode").Value()
                        Case "WS.W" 'temp
                            oWeatherWarnings.Add("WinterStorm" + xNode.Attributes.GetNamedItem("significance").InnerText, xNode.Attributes.GetNamedItem("significance").InnerText)
                    End Select
                Catch ex As Exception
                    Try
                        Select Case xNode.Attributes.GetNamedItem("hazardCode").Value()
                            Case "WS.W"" "
                                oWeatherWarnings.Add("WinterStorm", "Warning")
                        End Select
                    Catch
                    End Try
                End Try
            Next
        End If
    End Sub

    Private Function getMaxWarning(ByVal node As XmlNode) As Integer

        Dim max As Integer

        If node.ChildNodes(1).InnerText = "" Then
            Return 0
        End If

        max = node.ChildNodes(1).InnerText
        For i As Integer = 1 To node.ChildNodes.Count - 1
            If node.ChildNodes(i).InnerText > max Then
                max = node.ChildNodes(i).InnerText
            End If
        Next

        Return max

    End Function

    Private Function setWeatherParms() As weatherParametersType

        Dim WeatherParameters As weatherParametersType = New weatherParametersType

        'WeatherParameters.wx = True 'weather type coverage intensity
        'WeatherParameters.conhazo = True 'Convective hazard outlook
        WeatherParameters.appt = True 'temperature in Fahrenheit
        WeatherParameters.rh = True 'relative humidity
        WeatherParameters.pop12 = True 'Chance of rain in the next 12 hours
        WeatherParameters.snow = True ' snowfall in inches

        WeatherParameters.cumw34 = True '> 34 knot tropical storm
        WeatherParameters.cumw50 = True '> 50 knot tropical storm
        WeatherParameters.cumw64 = True '> 64 knot tropical storm

        WeatherParameters.ptornado = True 'Probability of tornadoes
        WeatherParameters.pxtornado = True 'Probability of extreme tornadoes

        WeatherParameters.phail = True '% chance hail
        WeatherParameters.pxhail = True '% chance extreme hail

        WeatherParameters.ptstmwinds = True '% chance storm winds
        WeatherParameters.pxtstmwinds = True '% chance extreme storm winds

        WeatherParameters.wwa = True

        Return WeatherParameters

    End Function
    'Check if Latitude and Longitude for the specified ZipCode are in DataBase before hitting weather.gov web service
    'If present in the DataBase set the dLatitude, dLongitude variables
    Public Sub LookUpLatitudeLongitudeInDB(ByVal zipCode As String)
        If Not String.IsNullOrEmpty(zipCode) Then

            Try
                Using conn As New Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())

                    Dim oReader As Data.SqlClient.SqlDataReader
                    Dim cmd As New Data.SqlClient.SqlCommand("SELECT Latitude, Longitude FROM Common..LatitudeLongitudeByZipCode (nolock) WHERE ZipCode = '" & zipCode & "'", conn)

                    conn.Open()
                    oReader = cmd.ExecuteReader

                    Do While oReader.Read()
                        'just get the first one, since only one record per Zip code should be there
                        dLatitude = Convert.ToDouble(oReader.Item("Latitude"))
                        dLongitude = Convert.ToDouble(oReader.Item("Longitude"))
                        Exit Do
                    Loop

                    oReader.Close()
                    conn.Close()

                End Using

            Catch
                'Log Exception??
                'No need to throw exception back, 
                'if we fail here we can hit the weather webservice to get the Latitude and Logitude
            End Try

        End If
    End Sub
    'Update the Database for future use
    Public Sub UpdateLatitudeLongitudeInDB(ByVal zipCode As String, ByVal latitude As Double, ByVal longitude As Double)
        If Not String.IsNullOrEmpty(zipCode) And latitude <> 0 And longitude <> 0 Then

            Try
                Using conn As New Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ToString())
                    conn.Open()

                    Dim selectCmd As New Data.SqlClient.SqlCommand("SELECT Latitude, Longitude FROM Common..LatitudeLongitudeByZipCode (nolock) WHERE ZipCode = '" & zipCode & "'", conn)
                    Dim oReader As Data.SqlClient.SqlDataReader
                    oReader = selectCmd.ExecuteReader

                    Dim updateCmd As Data.SqlClient.SqlCommand
                    If oReader.HasRows Then
                        'Update
                        updateCmd = New Data.SqlClient.SqlCommand("UPDATE Common..LatitudeLongitudeByZipCode SET Latitude = @Latitude, Longitude = @Longitude, UserID = @UserID, SystemTS = @SystemTS WHERE ZipCode = @ZipCode", conn)

                    Else
                        'Insert
                        updateCmd = New Data.SqlClient.SqlCommand("INSERT INTO Common..LatitudeLongitudeByZipCode (ZipCode, Latitude, Longitude, UserID, SystemTS) VALUES (@ZipCode, @Latitude, @Longitude, @UserID, @SystemTS)", conn)
                    End If

                    oReader.Close()

                    updateCmd.Parameters.Add("@ZipCode", SqlDbType.VarChar, 25).Value = zipCode
                    updateCmd.Parameters.Add("@Latitude", SqlDbType.Float).Value = latitude
                    updateCmd.Parameters.Add("@Longitude", SqlDbType.Float).Value = longitude
                    updateCmd.Parameters.Add("@UserID", SqlDbType.VarChar, 25).Value = "RatingRulesService"
                    updateCmd.Parameters.Add("@SystemTS", SqlDbType.DateTime).Value = DateTime.Now

                    updateCmd.ExecuteNonQuery()

                    conn.Close()

                End Using

            Catch
                'Log Exception??
                'No need to throw exception back, 
                'if we fail here we are not updating the database for future use
            End Try

        End If
    End Sub
End Class
