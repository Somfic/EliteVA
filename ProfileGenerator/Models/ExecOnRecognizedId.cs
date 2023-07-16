using System.Xml.Serialization;

[XmlRoot(ElementName="ExecOnRecognizedId")]
public class ExecOnRecognizedId { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}