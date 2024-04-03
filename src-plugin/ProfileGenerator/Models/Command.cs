using System.Xml.Serialization;

[XmlRoot(ElementName="Command")]
public class Command { 

    [XmlElement(ElementName="Referrer")] 
    public Referrer Referrer { get; set; } 

    [XmlElement(ElementName="ExecType")] 
    public int ExecType { get; set; } 

    [XmlElement(ElementName="Confidence")] 
    public int Confidence { get; set; } 

    [XmlElement(ElementName="PrefixActionCount")] 
    public int PrefixActionCount { get; set; } 

    [XmlElement(ElementName="IsDynamicallyCreated")] 
    public bool IsDynamicallyCreated { get; set; } 

    [XmlElement(ElementName="TargetProcessSet")] 
    public bool TargetProcessSet { get; set; } 

    [XmlElement(ElementName="TargetProcessType")] 
    public int TargetProcessType { get; set; } 

    [XmlElement(ElementName="TargetProcessLevel")] 
    public int TargetProcessLevel { get; set; } 

    [XmlElement(ElementName="CompareType")] 
    public int CompareType { get; set; } 

    [XmlElement(ElementName="ExecFromWildcard")] 
    public bool ExecFromWildcard { get; set; } 

    [XmlElement(ElementName="IsSubCommand")] 
    public bool IsSubCommand { get; set; } 

    [XmlElement(ElementName="IsOverride")] 
    public bool IsOverride { get; set; } 

    [XmlElement(ElementName="BaseId")] 
    public string BaseId { get; set; } 

    [XmlElement(ElementName="OriginId")] 
    public string OriginId { get; set; } 

    [XmlElement(ElementName="SessionEnabled")] 
    public bool SessionEnabled { get; set; } 

    [XmlElement(ElementName="DoubleTapInvoked")] 
    public bool DoubleTapInvoked { get; set; } 

    [XmlElement(ElementName="SingleTapDelayedInvoked")] 
    public bool SingleTapDelayedInvoked { get; set; } 

    [XmlElement(ElementName="LongTapInvoked")] 
    public bool LongTapInvoked { get; set; } 

    [XmlElement(ElementName="ShortTapDelayedInvoked")] 
    public bool ShortTapDelayedInvoked { get; set; } 

    [XmlElement(ElementName="SleepFlag")] 
    public int SleepFlag { get; set; } 

    [XmlElement(ElementName="Id")] 
    public string Id { get; set; } 

    [XmlElement(ElementName="CommandString")] 
    public string CommandString { get; set; } 

    [XmlElement(ElementName="ActionSequence")] 
    public ActionSequence ActionSequence { get; set; } 

    [XmlElement(ElementName="Async")] 
    public bool Async { get; set; } 

    [XmlElement(ElementName="Enabled")] 
    public bool Enabled { get; set; } 
    
    [XmlElement(ElementName="Category")] 
    public string Category { get; set; } 

    [XmlElement(ElementName="UseShortcut")] 
    public bool UseShortcut { get; set; } 

    [XmlElement(ElementName="keyValue")] 
    public int KeyValue { get; set; } 

    [XmlElement(ElementName="keyShift")] 
    public int KeyShift { get; set; } 

    [XmlElement(ElementName="keyAlt")] 
    public int KeyAlt { get; set; } 

    [XmlElement(ElementName="keyCtrl")] 
    public int KeyCtrl { get; set; } 

    [XmlElement(ElementName="keyWin")] 
    public int KeyWin { get; set; } 

    [XmlElement(ElementName="keyPassthru")] 
    public bool KeyPassthru { get; set; } 

    [XmlElement(ElementName="UseSpokenPhrase")] 
    public bool UseSpokenPhrase { get; set; } 

    [XmlElement(ElementName="onlyKeyUp")] 
    public bool OnlyKeyUp { get; set; } 

    [XmlElement(ElementName="RepeatNumber")] 
    public int RepeatNumber { get; set; } 

    [XmlElement(ElementName="RepeatType")] 
    public int RepeatType { get; set; } 

    [XmlElement(ElementName="CommandType")] 
    public int CommandType { get; set; } 

    [XmlElement(ElementName="SourceProfile")] 
    public string SourceProfile { get; set; } 

    [XmlElement(ElementName="UseConfidence")] 
    public bool UseConfidence { get; set; } 

    [XmlElement(ElementName="minimumConfidenceLevel")] 
    public int MinimumConfidenceLevel { get; set; } 

    [XmlElement(ElementName="UseJoystick")] 
    public bool UseJoystick { get; set; } 

    [XmlElement(ElementName="joystickNumber")] 
    public int JoystickNumber { get; set; } 

    [XmlElement(ElementName="joystickButton")] 
    public int JoystickButton { get; set; } 

    [XmlElement(ElementName="joystickNumber2")] 
    public int JoystickNumber2 { get; set; } 

    [XmlElement(ElementName="joystickButton2")] 
    public int JoystickButton2 { get; set; } 

    [XmlElement(ElementName="joystickUp")] 
    public bool JoystickUp { get; set; } 

    [XmlElement(ElementName="KeepRepeating")] 
    public bool KeepRepeating { get; set; } 

    [XmlElement(ElementName="UseProcessOverride")] 
    public bool UseProcessOverride { get; set; } 

    [XmlElement(ElementName="ProcessOverrideActiveWindow")] 
    public bool ProcessOverrideActiveWindow { get; set; } 

    [XmlElement(ElementName="LostFocusStop")] 
    public bool LostFocusStop { get; set; } 

    [XmlElement(ElementName="PauseLostFocus")] 
    public bool PauseLostFocus { get; set; } 

    [XmlElement(ElementName="LostFocusBackCompat")] 
    public bool LostFocusBackCompat { get; set; } 

    [XmlElement(ElementName="UseMouse")] 
    public bool UseMouse { get; set; } 

    [XmlElement(ElementName="Mouse1")] 
    public bool Mouse1 { get; set; } 

    [XmlElement(ElementName="Mouse2")] 
    public bool Mouse2 { get; set; } 

    [XmlElement(ElementName="Mouse3")] 
    public bool Mouse3 { get; set; } 

    [XmlElement(ElementName="Mouse4")] 
    public bool Mouse4 { get; set; } 

    [XmlElement(ElementName="Mouse5")] 
    public bool Mouse5 { get; set; } 

    [XmlElement(ElementName="Mouse6")] 
    public bool Mouse6 { get; set; } 

    [XmlElement(ElementName="Mouse7")] 
    public bool Mouse7 { get; set; } 

    [XmlElement(ElementName="Mouse8")] 
    public bool Mouse8 { get; set; } 

    [XmlElement(ElementName="Mouse9")] 
    public bool Mouse9 { get; set; } 

    [XmlElement(ElementName="MouseUpOnly")] 
    public bool MouseUpOnly { get; set; } 

    [XmlElement(ElementName="MousePassThru")] 
    public bool MousePassThru { get; set; } 

    [XmlElement(ElementName="joystickExclusive")] 
    public bool JoystickExclusive { get; set; } 

    [XmlElement(ElementName="lastEditedAction")] 
    public string LastEditedAction { get; set; } 

    [XmlElement(ElementName="UseProfileProcessOverride")] 
    public bool UseProfileProcessOverride { get; set; } 

    [XmlElement(ElementName="ProfileProcessOverrideActiveWindow")] 
    public bool ProfileProcessOverrideActiveWindow { get; set; } 

    [XmlElement(ElementName="RepeatIfKeysDown")] 
    public bool RepeatIfKeysDown { get; set; } 

    [XmlElement(ElementName="RepeatIfMouseDown")] 
    public bool RepeatIfMouseDown { get; set; } 

    [XmlElement(ElementName="RepeatIfJoystickDown")] 
    public bool RepeatIfJoystickDown { get; set; } 

    [XmlElement(ElementName="AH")] 
    public int AH { get; set; } 

    [XmlElement(ElementName="CL")] 
    public int CL { get; set; } 

    [XmlElement(ElementName="HasMB")] 
    public bool HasMB { get; set; } 

    [XmlElement(ElementName="UseVariableHotkey")] 
    public bool UseVariableHotkey { get; set; } 

    [XmlElement(ElementName="CLE")] 
    public int CLE { get; set; } 

    [XmlElement(ElementName="EX1")] 
    public bool EX1 { get; set; } 

    [XmlElement(ElementName="EX2")] 
    public bool EX2 { get; set; } 

    [XmlElement(ElementName="InternalId")] 
    public InternalId InternalId { get; set; } 

    [XmlElement(ElementName="HasInput")] 
    public bool HasInput { get; set; } 

    [XmlElement(ElementName="HotkeyDoubleTapLevel")] 
    public int HotkeyDoubleTapLevel { get; set; } 

    [XmlElement(ElementName="MouseDoubleTapLevel")] 
    public int MouseDoubleTapLevel { get; set; } 

    [XmlElement(ElementName="JoystickDoubleTapLevel")] 
    public int JoystickDoubleTapLevel { get; set; } 

    [XmlElement(ElementName="HotkeyLongTapLevel")] 
    public int HotkeyLongTapLevel { get; set; } 

    [XmlElement(ElementName="MouseLongTapLevel")] 
    public int MouseLongTapLevel { get; set; } 

    [XmlElement(ElementName="JoystickLongTapLevel")] 
    public int JoystickLongTapLevel { get; set; } 

    [XmlElement(ElementName="AlwaysExec")] 
    public bool AlwaysExec { get; set; } 

    [XmlElement(ElementName="ResourceBalance")] 
    public int ResourceBalance { get; set; } 

    [XmlElement(ElementName="PreventExec")] 
    public bool PreventExec { get; set; } 

    [XmlElement(ElementName="ExternalEventsEnabled")] 
    public bool ExternalEventsEnabled { get; set; } 

    [XmlElement(ElementName="ExcludeExecOnRecognized")] 
    public bool ExcludeExecOnRecognized { get; set; } 

    [XmlElement(ElementName="UseVariableMouseShortcut")] 
    public bool UseVariableMouseShortcut { get; set; } 

    [XmlElement(ElementName="UseVariableJoystickShortcut")] 
    public bool UseVariableJoystickShortcut { get; set; } 
}