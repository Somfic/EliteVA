using System.Xml.Serialization;

[XmlRoot(ElementName="InternalId")]
public class InternalId { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}