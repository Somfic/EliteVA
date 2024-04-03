using System.Xml.Serialization;

[XmlRoot(ElementName="CommandAction")]
public class CommandAction { 

    [XmlElement(ElementName="_caption")] 
    public string Caption { get; set; } 

    [XmlElement(ElementName="PairingSet")] 
    public bool PairingSet { get; set; } 

    [XmlElement(ElementName="PairingSetElse")] 
    public bool PairingSetElse { get; set; } 

    [XmlElement(ElementName="Ordinal")] 
    public int Ordinal { get; set; } 

    [XmlElement(ElementName="ConditionMet")] 
    public ConditionMet ConditionMet { get; set; } 

    [XmlElement(ElementName="IndentLevel")] 
    public int IndentLevel { get; set; } 

    [XmlElement(ElementName="ConditionSkip")] 
    public bool ConditionSkip { get; set; } 

    [XmlElement(ElementName="IsSuffixAction")] 
    public bool IsSuffixAction { get; set; } 

    [XmlElement(ElementName="DecimalTransient1")] 
    public int DecimalTransient1 { get; set; } 

    [XmlElement(ElementName="Caption")] 
    public string CommandCaption { get; set; } 

    [XmlElement(ElementName="Id")] 
    public string Id { get; set; } 

    [XmlElement(ElementName="ActionType")] 
    public string ActionType { get; set; } 

    [XmlElement(ElementName="Duration")] 
    public int Duration { get; set; } 

    [XmlElement(ElementName="Delay")] 
    public int Delay { get; set; } 

    [XmlElement(ElementName="KeyCodes")] 
    public Keycodes KeyCodes { get; set; }

    [XmlElement(ElementName="X")] 
    public int X { get; set; } 

    [XmlElement(ElementName="Y")] 
    public int Y { get; set; } 

    [XmlElement(ElementName="Z")] 
    public int Z { get; set; } 

    [XmlElement(ElementName="InputMode")] 
    public int InputMode { get; set; } 

    [XmlElement(ElementName="ConditionPairing")] 
    public int ConditionPairing { get; set; } 

    [XmlElement(ElementName="ConditionGroup")] 
    public int ConditionGroup { get; set; } 

    [XmlElement(ElementName="ConditionStartOperator")] 
    public int ConditionStartOperator { get; set; } 

    [XmlElement(ElementName="ConditionStartValue")] 
    public int ConditionStartValue { get; set; } 

    [XmlElement(ElementName="ConditionStartValueType")] 
    public int ConditionStartValueType { get; set; } 

    [XmlElement(ElementName="ConditionStartType")] 
    public int ConditionStartType { get; set; } 

    [XmlElement(ElementName="DecimalContext1")] 
    public int DecimalContext1 { get; set; } 

    [XmlElement(ElementName="DecimalContext2")] 
    public int DecimalContext2 { get; set; } 

    [XmlElement(ElementName="DateContext1")] 
    public DateTime DateContext1 { get; set; } 

    [XmlElement(ElementName="DateContext2")] 
    public DateTime DateContext2 { get; set; } 

    [XmlElement(ElementName="Disabled")] 
    public bool Disabled { get; set; } 

    [XmlElement(ElementName="RandomSounds")] 
    public RandomSounds RandomSounds { get; set; } 

    [XmlElement(ElementName="IntegerContext1")] 
    public int IntegerContext1 { get; set; } 

    [XmlElement(ElementName="IntegerContext2")] 
    public int IntegerContext2 { get; set; } 
}