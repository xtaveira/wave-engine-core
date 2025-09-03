using Microwave.Domain;
using Microsoft.AspNetCore.Http;

namespace WaveEngineCore.Infrastructure;

public class SessionStateStorage : IStateStorage
{
    private readonly ISession _session;

    public SessionStateStorage(ISession session)
    {
        _session = session;
    }

    public void SetString(string key, string value)
    {
        _session.SetString(key, value);
    }

    public string? GetString(string key)
    {
        return _session.GetString(key);
    }

    public void SetInt32(string key, int value)
    {
        _session.SetInt32(key, value);
    }

    public int? GetInt32(string key)
    {
        return _session.GetInt32(key);
    }

    public void Remove(string key)
    {
        _session.Remove(key);
    }
}