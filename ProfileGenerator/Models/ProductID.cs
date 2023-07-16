using System.Xml.Serialization;

[XmlRoot(ElementName="ProductID")]
public class ProductID { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}