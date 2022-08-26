namespace EliteVA.Proxy;

public class ProxyHolder
{
    private VoiceAttackProxy _proxy;
    
    public void Set(VoiceAttackProxy proxy)
    {
        _proxy = proxy;
    }
    
    public VoiceAttackProxy Get()
    {
        return _proxy;
    }
}