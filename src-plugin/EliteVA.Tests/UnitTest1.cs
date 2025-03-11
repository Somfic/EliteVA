using EliteAPI.Abstractions.Events;
using EliteAPI.Events;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using EliteVA.Proxy.Variables;
using EliteVA.Services.Bridge;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EliteVA.Tests;

public class Tests
{
    private IVoiceAttackProxy _proxy = new VoiceAttackProxy(new Mock<IVoiceAttackProxy>().Object)
        { Variables = new VoiceAttackVariables(new Mock<IVoiceAttackProxy>().Object) };
    private JournalEventsService _va = new(Mock.Of<ILogger<JournalEventsService>>(), Mock.Of<IEventParser>(), Mock.Of<IEvents>());
    private Events _events;

    [OneTimeSetUp]
    public void Setup()
    {
        var eventParser = new EventParser(Mock.Of<IServiceProvider>());
        eventParser.Use<LocalisedConverter>();
        _events = new Events(Mock.Of<ILogger<Events>>(), eventParser);
        _events.Register();
        
        _va = new JournalEventsService(Mock.Of<ILogger<JournalEventsService>>(), eventParser, _events);
    }

    [Test]
    public void Test1()
    {
        _va.OnStart(_proxy);
        _events.Invoke("{ \"timestamp\":\"2025-03-07T14:18:53Z\", \"event\":\"Fileheader\", \"part\":1, \"language\":\"English/UK\", \"Odyssey\":true, \"gameversion\":\"4.1.0.100\", \"build\":\"r311607/r0 \" }", new EventContext());

        _proxy.Variables.SetVariables.Should().NotBeNullOrEmpty();
       
        Assert.Pass();
    }
}