Imports Microsoft.VisualBasic

Public Class clsLogItem

    Protected msTitle As String
    Protected msDescription As String
    Protected miStepNum As Integer

#Region "Properties"
    Public Property Title() As String
        Get
            Title = msTitle
        End Get
        Set(ByVal value As String)
            msTitle = value
        End Set
    End Property

    Public Property Description() As String
        Get
            Description = msDescription
        End Get
        Set(ByVal value As String)
            msDescription = value
        End Set
    End Property

    Public Property StepNum() As Integer
        Get
            StepNum = miStepNum
        End Get
        Set(ByVal value As Integer)
            miStepNum = value
        End Set
    End Property
#End Region
End Class
