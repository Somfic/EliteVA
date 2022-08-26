namespace EliteVA.WebSocket;

public record EventContext(bool IsRaisedDuringCatchup, bool IsImplemented, string SourceFile);