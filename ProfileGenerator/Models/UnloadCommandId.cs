using System.Xml.Serialization;

[XmlRoot(ElementName="UnloadCommandId")]
public class UnloadCommandId { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}