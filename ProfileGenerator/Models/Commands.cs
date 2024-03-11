using System.Xml.Serialization;

[XmlRoot(ElementName="Commands")]
public class Commands { 

    [XmlElement(ElementName="Command")] 
    public List<Command> Command { get; set; } 
}