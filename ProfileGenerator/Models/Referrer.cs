using System.Xml.Serialization;

[XmlRoot(ElementName="Referrer")]
public class Referrer { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}