using System.Collections.Generic;

namespace EliteVA.WebSocket;

public record PathsPayload(IReadOnlyCollection<EventPath> Paths, EventContext Context);