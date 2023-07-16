using System.Xml.Serialization;

[XmlRoot(ElementName="AuthorID")]
public class AuthorID { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}