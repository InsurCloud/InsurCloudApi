Public Class ZipCountyMapping
    Private _zipCode As String
    Public Property ZipCode() As String
        Get
            Return _zipCode
        End Get
        Set(ByVal value As String)
            _zipCode = value
        End Set
    End Property

    Private _county As String
    Public Property County() As String
        Get
            Return _county
        End Get
        Set(ByVal value As String)
            _county = value
        End Set
    End Property
End Class
