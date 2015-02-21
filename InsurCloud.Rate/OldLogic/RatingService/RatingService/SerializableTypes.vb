Imports System.Xml.Serialization
Imports System.Xml

Public Class SerializableDictionary(Of T)
    Inherits Dictionary(Of String, T)
    Implements IXmlSerializable

    Public Function GetSchema() As System.Xml.Schema.XmlSchema Implements IXmlSerializable.GetSchema
        Return Nothing
    End Function

    Public Sub ReadXml(ByVal reader As System.Xml.XmlReader) Implements IXmlSerializable.ReadXml
        Dim valSerializer As New XmlSerializer(GetType(T))
        Dim wasEmpty As Boolean = reader.IsEmptyElement
        reader.Read()
        If (wasEmpty) Then Return

        While reader.NodeType <> XmlNodeType.EndElement
            Dim key As String = reader.Name
            reader.ReadStartElement()
            Dim value As T = valSerializer.Deserialize(reader.ReadSubtree())
            reader.ReadEndElement()
            Me.Add(key, value)
            reader.ReadEndElement()
            reader.MoveToContent()
        End While
        reader.ReadEndElement()
    End Sub
    Public Sub WriteXml(ByVal writer As System.Xml.XmlWriter) Implements IXmlSerializable.WriteXml
        For Each key As String In Me.Keys
            Try
                Dim value As T = Me(key)
                If value IsNot Nothing Then

                    Dim valSerializer As New XmlSerializer(value.GetType)

                    writer.WriteStartElement(key)
                    valSerializer.Serialize(writer, value)
                    writer.WriteEndElement()
                Else
                    Dim valSerializer As New XmlSerializer(GetType(String))

                    writer.WriteStartElement(key)
                    valSerializer.Serialize(writer, "NULL")
                    writer.WriteEndElement()
                End If
            Catch e As Exception
                Dim valSerializer As New XmlSerializer(GetType(String))

                writer.WriteStartElement(key)
                valSerializer.Serialize(writer, "Exception During Serialization: " & e.Message)
                writer.WriteEndElement()
            End Try

        Next
    End Sub
End Class

Public Class SerializableException
    Inherits System.Exception
    Implements IXmlSerializable

    Public innerEx As Exception

    Public Function GetSchema() As System.Xml.Schema.XmlSchema Implements System.Xml.Serialization.IXmlSerializable.GetSchema
        Return Nothing
    End Function

    Public Sub ReadXml(ByVal reader As System.Xml.XmlReader) Implements IXmlSerializable.ReadXml
        'Dim valSerializer As New XmlSerializer(GetType(String))
        'Dim wasEmpty As Boolean = reader.IsEmptyElement
        'reader.Read()
        'If (wasEmpty) Then Return

        'While reader.NodeType <> Xml.XmlNodeType.EndElement
        '	Dim key As String = reader.Name
        '	reader.ReadStartElement()
        '	Dim value As T = valSerializer.Deserialize(reader.ReadSubtree())
        '	reader.ReadEndElement()
        '	Me.Add(key, value)
        '	reader.ReadEndElement()
        '	reader.MoveToContent()
        'End While

        'reader.ReadEndElement()

    End Sub
    Public Sub WriteXml(ByVal writer As System.Xml.XmlWriter) Implements IXmlSerializable.WriteXml
        If Me.innerEx IsNot Nothing Then
            WriteXml(writer, Me.innerEx)
        End If
    End Sub

    Public Sub WriteXml(ByVal writer As System.Xml.XmlWriter, ByVal ex As Exception)

        Dim valSerializer As New XmlSerializer(GetType(String))

        If ex.Message IsNot Nothing Then
            writer.WriteStartElement("Message")
            valSerializer.Serialize(writer, ex.Message)
            writer.WriteEndElement()
        End If

        If ex.StackTrace IsNot Nothing Then
            writer.WriteStartElement("StackTrace")
            valSerializer.Serialize(writer, ex.StackTrace)
            writer.WriteEndElement()
        End If

        If ex.TargetSite IsNot Nothing Then
            writer.WriteStartElement("FunctionName")
            valSerializer.Serialize(writer, ex.TargetSite.Name)
            writer.WriteEndElement()
        End If

        If ex.InnerException IsNot Nothing Then
            writer.WriteStartElement("InnerException")
            WriteXml(writer, ex.InnerException)
            writer.WriteEndElement()
        End If
    End Sub
End Class
