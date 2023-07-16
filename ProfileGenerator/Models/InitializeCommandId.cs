using System.Xml.Serialization;

[XmlRoot(ElementName="InitializeCommandId")]
public class InitializeCommandId { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}