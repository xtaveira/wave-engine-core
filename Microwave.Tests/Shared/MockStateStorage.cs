using Microwave.Domain;

namespace Microwave.Tests.Shared;

public class MockStateStorage : IStateStorage
{
    private readonly Dictionary<string, string> _storage = new();

    public void SetString(string key, string value) => _storage[key] = value;
    public string? GetString(string key) => _storage.TryGetValue(key, out var value) ? value : null;
    public void SetInt32(string key, int value) => _storage[key] = value.ToString();
    public int? GetInt32(string key) => _storage.TryGetValue(key, out var value) && int.TryParse(value, out var intValue) ? intValue : null;
    public void Remove(string key) => _storage.Remove(key);
}
