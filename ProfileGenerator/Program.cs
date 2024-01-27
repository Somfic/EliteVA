using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using EliteAPI;
using EliteAPI.Abstractions.Events;
using EliteAPI.Abstractions.Status;
using EliteAPI.Events;

var api = EliteDangerousApi.Create();
api.InitialiseAsync();

// Keep a list of active missions
var missions = new List<MissionAcceptedEvent>();

// Add new missions
api.Events.On<MissionAcceptedEvent>(m => missions.Add(m));

// Remove completed/abandoned/failed missions
api.Events.On<MissionCompletedEvent>(m => missions = missions.Where(x => x.MissionId != m.MissionId).ToList());
api.Events.On<MissionAbandonedEvent>(m => missions = missions.Where(x => x.MissionId != m.MissionId).ToList());
api.Events.On<MissionFailedEvent>(m => missions = missions.Where(x => x.MissionId != m.MissionId).ToList());

// Invoke all the journal files in the journals directory
var journalsDirectory = new DirectoryInfo(api.Config.JournalsPath);
var journalFiles = journalsDirectory.GetFiles(api.Config.JournalPattern).Where(x => x.LastWriteTime > DateTime.Now.AddDays(-8));
journalFiles
    .SelectMany(file => File.ReadAllLines(file.FullName))
    .ToList()
    .ForEach(json => api.Events.Invoke(json, new EventContext()));

var commands = api.Events.EventTypes.Select(type => (fullname: type.FullName, command: type.GetInterfaces().Contains(typeof(IStatusEvent))
        ? $"((EliteAPI.Status.{type.Name}))"
        : $"((EliteAPI.{type.Name}))"))
    .Select(x => (x.fullname, x.command.Replace("Event))", "))")))
    .Select(x => (x.fullname, Regex.Replace(x.Item2, @"([A-Za-z]+)Status", "$1")))
    .ToList();

var version = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
Console.WriteLine("Generating profile for EliteAPI v" + version);

var profile = new Profile
{
    Name = "EliteVA v" + version,
    Id = Guid.NewGuid().ToString(),
    Commands =
    {
        Command = new List<Command>()
    },
    LastEditedCommand = Guid.Empty.ToString(),
    ExportVAVersion = "1.10.6.14",
    ExportOSVersionMajor = 10,
    ExportOSVersionMinor = 0,
    ProcessOverrideAciveWindow = true,
    ReferencedProfile = new ReferencedProfile() { Nil = true },
    CatchAllId = new CatchAllId() { Nil = true },
    InitializeCommandId = new InitializeCommandId() { Nil = true},
    DictationCommandId = new DictationCommandId() { Nil = true },
    UnloadCommandId = new UnloadCommandId() { Nil = true},
    AuthorID = new AuthorID() { Nil = true },
    ProductID = new ProductID() { Nil = true },
    InternalID = new InternalID() { Nil = true },
    ExecOnRecognizedId = new ExecOnRecognizedId() { Nil = true },
    CategoryGroups = new CategoryGroups()
};

foreach (var (fullname, command) in commands)
{
    var categories = fullname.Replace("Ship.Events", "Ship").Split('.');
    categories = categories.Reverse().ToArray();
    var category = $"EliteAPI {categories.Skip(1).First()}";
    
    profile.Commands.Command.Add(new Command
    {
        Referrer = new Referrer { Nil = true },
        Id = Guid.NewGuid().ToString(),
        BaseId = Guid.NewGuid().ToString(),
        OriginId = Guid.Empty.ToString(),
        ExecType = 3,
        Category = category,
        SessionEnabled = true,
        CommandString = command,
        ActionSequence = new ActionSequence
        {
            CommandAction = new CommandAction
            {
                Caption = "Do nothing (ignore command)",
                CommandCaption = "Do nothing (ignore command)",
                Id = Guid.NewGuid().ToString(),
                ActionType = "InternalProcess_Ignore",
                KeyCodes = new Keycodes(),
                ConditionMet = new ConditionMet() { Nil = true },
                RandomSounds = new RandomSounds()
            },
        },
        Async = true,
        Enabled = true,
        KeyPassthru = true,
        UseSpokenPhrase = true,
        RepeatNumber = 2,
        ProcessOverrideActiveWindow = true,
        MousePassThru = true, 
        InternalId = new InternalId
        {
            Nil = true
        },
        LostFocusBackCompat = true,
        LastEditedAction =  Guid.Empty.ToString(),
        SourceProfile = Guid.Empty.ToString(),
    });
}

profile.LastEditedCommand = profile.Commands.Command.Last().Id;
profile.Commands.Command.ForEach(x => x.LastEditedAction = x.ActionSequence.CommandAction.Id);

var serializer = new XmlSerializer(typeof(Profile));
// To string
using var stringWriter = new StringWriter();
serializer.Serialize(stringWriter, profile);
var xml = stringWriter.ToString();
xml = xml.Replace("nil=", "xsi:nil=");
xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\"?>");

// To file
File.WriteAllText($"Profile.vap", xml);