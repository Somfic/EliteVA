using System.Xml.Serialization;

[XmlRoot(ElementName="CatchAllId")]
public class CatchAllId { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}