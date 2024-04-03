using System.Xml.Serialization;

[XmlRoot(ElementName="ConditionMet")]
public class ConditionMet { 

    [XmlAttribute(AttributeName="nil")] 
    public bool Nil { get; set; } 
}