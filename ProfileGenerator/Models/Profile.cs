using System.Xml.Serialization;

[XmlRoot(ElementName="Profile")]
public class Profile { 

    [XmlElement(ElementName="HasMB")] 
    public bool HasMB { get; set; } 

    [XmlElement(ElementName="Id")] 
    public string Id { get; set; } 

    [XmlElement(ElementName="Name")] 
    public string Name { get; set; }

    [XmlElement(ElementName = "Commands")] public Commands Commands { get; set; } = new();

    [XmlElement(ElementName="OverrideGlobal")] 
    public bool OverrideGlobal { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyIndex")] 
    public int GlobalHotkeyIndex { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyEnabled")] 
    public bool GlobalHotkeyEnabled { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyValue")] 
    public int GlobalHotkeyValue { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyShift")] 
    public int GlobalHotkeyShift { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyAlt")] 
    public int GlobalHotkeyAlt { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyCtrl")] 
    public int GlobalHotkeyCtrl { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyWin")] 
    public int GlobalHotkeyWin { get; set; } 

    [XmlElement(ElementName="GlobalHotkeyPassThru")] 
    public bool GlobalHotkeyPassThru { get; set; } 

    [XmlElement(ElementName="OverrideMouse")] 
    public bool OverrideMouse { get; set; } 

    [XmlElement(ElementName="MouseIndex")] 
    public int MouseIndex { get; set; } 

    [XmlElement(ElementName="OverrideStop")] 
    public bool OverrideStop { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyEnabled")] 
    public bool StopCommandHotkeyEnabled { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyValue")] 
    public int StopCommandHotkeyValue { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyShift")] 
    public int StopCommandHotkeyShift { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyAlt")] 
    public int StopCommandHotkeyAlt { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyCtrl")] 
    public int StopCommandHotkeyCtrl { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyWin")] 
    public int StopCommandHotkeyWin { get; set; } 

    [XmlElement(ElementName="StopCommandHotkeyPassThru")] 
    public bool StopCommandHotkeyPassThru { get; set; } 

    [XmlElement(ElementName="DisableShortcuts")] 
    public bool DisableShortcuts { get; set; } 

    [XmlElement(ElementName="UseOverrideListening")] 
    public bool UseOverrideListening { get; set; } 

    [XmlElement(ElementName="OverrideJoystickGlobal")] 
    public bool OverrideJoystickGlobal { get; set; } 

    [XmlElement(ElementName="GlobalJoystickIndex")] 
    public int GlobalJoystickIndex { get; set; } 

    [XmlElement(ElementName="GlobalJoystickButton")] 
    public int GlobalJoystickButton { get; set; } 

    [XmlElement(ElementName="GlobalJoystickNumber")] 
    public int GlobalJoystickNumber { get; set; } 

    [XmlElement(ElementName="GlobalJoystickButton2")] 
    public int GlobalJoystickButton2 { get; set; } 

    [XmlElement(ElementName="GlobalJoystickNumber2")] 
    public int GlobalJoystickNumber2 { get; set; } 

    [XmlElement(ElementName="ReferencedProfile")] 
    public ReferencedProfile ReferencedProfile { get; set; }  = new();

    [XmlElement(ElementName="ExportVAVersion")] 
    public string ExportVAVersion { get; set; } 

    [XmlElement(ElementName="ExportOSVersionMajor")] 
    public int ExportOSVersionMajor { get; set; } 

    [XmlElement(ElementName="ExportOSVersionMinor")] 
    public int ExportOSVersionMinor { get; set; } 

    [XmlElement(ElementName="OverrideConfidence")] 
    public bool OverrideConfidence { get; set; } 

    [XmlElement(ElementName="Confidence")] 
    public int Confidence { get; set; } 

    [XmlElement(ElementName="CatchAllEnabled")] 
    public bool CatchAllEnabled { get; set; } 

    [XmlElement(ElementName="CatchAllId")] 
    public CatchAllId CatchAllId { get; set; } = new(); 

    [XmlElement(ElementName="InitializeCommandEnabled")] 
    public bool InitializeCommandEnabled { get; set; } 

    [XmlElement(ElementName="InitializeCommandId")] 
    public InitializeCommandId InitializeCommandId { get; set; }  = new();

    [XmlElement(ElementName="UseProcessOverride")] 
    public bool UseProcessOverride { get; set; } 

    [XmlElement(ElementName="ProcessOverrideAciveWindow")] 
    public bool ProcessOverrideAciveWindow { get; set; } 

    [XmlElement(ElementName="DictationCommandEnabled")] 
    public bool DictationCommandEnabled { get; set; } 

    [XmlElement(ElementName="DictationCommandId")] 
    public DictationCommandId DictationCommandId { get; set; }  = new();

    [XmlElement(ElementName="EnableProfileSwitch")] 
    public bool EnableProfileSwitch { get; set; } 

    [XmlElement(ElementName="CategoryGroups")] 
    public CategoryGroups CategoryGroups { get; set; } 

    [XmlElement(ElementName="GroupCategory")] 
    public bool GroupCategory { get; set; } 

    [XmlElement(ElementName="LastEditedCommand")] 
    public string LastEditedCommand { get; set; } 

    [XmlElement(ElementName="IS")] 
    public int IS { get; set; } 

    [XmlElement(ElementName="IO")] 
    public int IO { get; set; } 

    [XmlElement(ElementName="IP")] 
    public int IP { get; set; } 

    [XmlElement(ElementName="BE")] 
    public int BE { get; set; } 

    [XmlElement(ElementName="UnloadCommandEnabled")] 
    public bool UnloadCommandEnabled { get; set; } 

    [XmlElement(ElementName="UnloadCommandId")] 
    public UnloadCommandId UnloadCommandId { get; set; } = new(); 

    [XmlElement(ElementName="BlockExternal")] 
    public bool BlockExternal { get; set; } 

    [XmlElement(ElementName="AuthorID")] 
    public AuthorID AuthorID { get; set; }  = new();

    [XmlElement(ElementName="ProductID")] 
    public ProductID ProductID { get; set; }  = new();

    [XmlElement(ElementName="CR")] 
    public int CR { get; set; } 

    [XmlElement(ElementName="InternalID")] 
    public InternalID InternalID { get; set; }  = new();

    [XmlElement(ElementName="PR")] 
    public int PR { get; set; } 

    [XmlElement(ElementName="CO")] 
    public int CO { get; set; } 

    [XmlElement(ElementName="OP")] 
    public int OP { get; set; } 

    [XmlElement(ElementName="CV")] 
    public int CV { get; set; } 

    [XmlElement(ElementName="PD")] 
    public int PD { get; set; } 

    [XmlElement(ElementName="PE")] 
    public int PE { get; set; } 

    [XmlElement(ElementName="ExecOnRecognizedEnabled")] 
    public bool ExecOnRecognizedEnabled { get; set; } 

    [XmlElement(ElementName="ExecOnRecognizedId")] 
    public ExecOnRecognizedId ExecOnRecognizedId { get; set; }  = new();

    [XmlElement(ElementName="ExecOnRecognizedRejected")] 
    public bool ExecOnRecognizedRejected { get; set; } 

    [XmlElement(ElementName="ExcludeGlobalProfiles")] 
    public bool ExcludeGlobalProfiles { get; set; } 

    [XmlElement(ElementName="DisableAdvancedTTS")] 
    public bool DisableAdvancedTTS { get; set; } 

    [XmlElement(ElementName="RPR")] 
    public int RPR { get; set; } 

    [XmlElement(ElementName="Deleted")] 
    public bool Deleted { get; set; } 

    [XmlAttribute(AttributeName="xsd")] 
    public string Xsd { get; set; } 

    [XmlAttribute(AttributeName="xsi")] 
    public string Xsi { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}