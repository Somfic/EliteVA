using System.Xml.Serialization;

[XmlRoot(ElementName="DictationCommandId")]
public class DictationCommandId { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}