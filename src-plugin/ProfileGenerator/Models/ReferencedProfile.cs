using System.Xml.Serialization;

[XmlRoot(ElementName="ReferencedProfile")]
public class ReferencedProfile { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}