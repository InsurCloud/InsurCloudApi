Imports Microsoft.VisualBasic

Public Class clsPgm3
    Inherits clsPgm

    Public Sub New()

    End Sub

    Public Overloads Sub FinishLogging(ByVal bLogRate As Boolean)
        MyBase.FinishLogging(bLogRate)
        If bLogRate Then
            ErrorLogging("", "")
        End If
    End Sub

    Public Overloads Sub ErrorLogging(ByVal sMethodName As String, ByVal sMessage As String)
        Dim oLogSvc As New ImperialFire.Logging
        oLogSvc.WriteHomeownersLog(moLogging, sMethodName, sMessage)
    End Sub
End Class
