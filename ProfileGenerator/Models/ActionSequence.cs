using System.Xml.Serialization;

[XmlRoot(ElementName="ActionSequence")]
public class ActionSequence { 

    [XmlElement(ElementName="CommandAction")] 
    public CommandAction CommandAction { get; set; } 
}