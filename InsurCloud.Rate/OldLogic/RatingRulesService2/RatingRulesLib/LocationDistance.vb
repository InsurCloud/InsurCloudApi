Imports Microsoft.VisualBasic
Imports System.Math
Imports System.Collections.Generic
Imports System.Web.Services
Imports System.Web


Public Class LocationDistance

    Public Function DistanceFromWaterway(ByVal Lat As Double, ByVal Lon As Double) As Double

        Dim min As Double = Double.MaxValue
        Dim temp As Double

        For i As Integer = 1 To IntraCoastalWaterway.Length - 1
            temp = DistToSegment(Lat, Lon, IntraCoastalWaterway(i - 1).Lat, IntraCoastalWaterway(i - 1).Lon, IntraCoastalWaterway(i).Lat, IntraCoastalWaterway(i).Lon)
            If min > temp Then
                min = temp
            End If
        Next

        Return min

	End Function

	Public Shared Function EastWestOfHW146(ByVal Lat As Double, ByVal Lon As Double) As String
		Dim side As String = ""
		If HW146 Is Nothing Then
			HW146 = LoadGeoPointFile("HW146.txt")
		End If

		If Lon < HW146WestMostLong Then
			Return "WEST"
		ElseIf Lon > HW146EastMostLong Then
			Return "EAST"
		Else
		End If

		Dim max As Integer = 0
		Dim min As Integer = HW146.Count - 1
		Dim pos As Integer = max + (min - max) / 2

		While max < min
			If HW146(pos).Lat < Lat Then
				max = pos + 1
				pos = max + (min - max) / 2
			ElseIf HW146(pos).Lat > Lat Then
				min = pos - 1
				pos = max + (min - max) / 2
			Else
				Exit While
			End If
		End While

		If Lon > HW146(pos).Lon Then
			side = "EAST"
		ElseIf Lon < HW146(pos).Lon Then
			side = "WEST"
		End If

		Return side
		'Return String.Format("{0}, {1} is {2} of {3}, {4}. Pos is {5}", Lat, Lon, side, HW146(pos).Lat, HW146(pos).Lon, pos)
	End Function

	' Calculate the distance between the point and the segment.
	Private Function DistToSegment(ByVal pX As Double, ByVal pY As Double, ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As Double

		Dim dX, dY, near_x, near_y, t As Double

		dX = X2 - X1
		dY = Y2 - Y1

		' Calculate the t that minimizes the distance.
		t = ((pX - X1) * dX + (pY - Y1) * dY) / (dX * dX + dY * dY)

		' See if this represents one of the segment's end points or a point in the middle.
		If t < 0 Then
			near_x = X1
			near_y = Y1
		ElseIf t > 1 Then
			near_x = X2
			near_y = Y2
		Else
			near_x = X1 + t * dX
			near_y = Y1 + t * dY
		End If

		Return HaversineDistance(pX, pY, near_x, near_y)

	End Function

	'Calculate the distance between two lat and longitude points
	Public Function HaversineDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double

		'Haversine formula
		'------------------------------------------------------
		'R = earth 's radius (mean radius = 6,371km)
		'Δlat = lat2− lat1
		'Δlong = long2− long1
		'a = sin²(Δlat/2) + cos(lat1).cos(lat2).sin²(Δlong/2)
		'c = 2 * atan2(√a, √(1−a))
		'd = R * c
		'------------------------------------------------------
		Dim dLat, dLon, a, c, d As Double
		Dim R As Integer = 6371	'Radius of the Earth
		dLat = toRad(lat2 - lat1) 'Distance in Radians
		dLon = toRad(lon2 - lon1) 'Distance in Radians
		a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(toRad(lat1)) * Math.Cos(toRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
		c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))
		d = R * c ' d in KM 

		Return d * 0.6214  'multiply by .6214 for miles

	End Function

	Private Shared Function LoadGeoPointFile(ByVal FileName As String) As List(Of GeoPoint)
		Dim line As String
		Dim filePath As String = "HW146.txt"
		Dim GeoList As List(Of GeoPoint) = New List(Of GeoPoint)()
		Dim max As Double = -9999.99
		Dim min As Double = 9999.99

        Dim file As System.IO.StreamReader = New System.IO.StreamReader(HttpContext.Current.Server.MapPath(filePath))
		line = file.ReadLine()
		While Not line Is Nothing And Not file.EndOfStream
			Dim lineSplit As String() = line.Split(",")
			GeoList.Add(New GeoPoint(Double.Parse(lineSplit(0)), Double.Parse(lineSplit(1))))
			If GeoList(GeoList.Count - 1).Lon > max Then
				max = GeoList(GeoList.Count - 1).Lon
			ElseIf GeoList(GeoList.Count - 1).Lon < min Then
				min = GeoList(GeoList.Count - 1).Lon
			End If
			line = file.ReadLine()
		End While
		file.Close()

		HW146EastMostLong = max
		HW146WestMostLong = min
		Return GeoList
	End Function


#Region "Private Stuff"
	Private Shared HW146 As List(Of GeoPoint)
	Private Shared HW146EastMostLong As Double
	Private Shared HW146WestMostLong As Double

	Private IntraCoastalWaterway() As GeoPoint = {New GeoPoint(29.68328, -93.84796), New GeoPoint(29.67374, -94.01001), New GeoPoint(29.55435, -94.40002), New GeoPoint(29.36063, -94.80927), New GeoPoint(28.97931, -95.26245), New GeoPoint(28.69059, -95.85022), New GeoPoint(28.49766, -96.27869), New GeoPoint(28.29471, -96.62476), New GeoPoint(28.1834, -96.82526), New GeoPoint(27.88764, -97.09717), New GeoPoint(27.5034, -97.32513), New GeoPoint(27.1227, -97.42126), New GeoPoint(26.77749, -97.42401), New GeoPoint(26.46074, -97.30042), New GeoPoint(25.98767, -97.17407)}

	Private Function toRad(ByVal degree As Double) As Double
		Return degree * (Math.PI / 180)	'convert degrees to radians
	End Function

	Private Class GeoPoint
		Public Lat As Double
		Public Lon As Double

		Public Sub New(ByVal latitude As Double, ByVal longitude As Double)
			Lat = latitude
			Lon = longitude
		End Sub
		Public Sub New()
			Lat = 0
			Lon = 0
		End Sub

	End Class
#End Region

End Class
