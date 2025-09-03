namespace Microwave.Domain;

public interface IStateStorage
{
    void SetString(string key, string value);
    string? GetString(string key);
    void SetInt32(string key, int value);
    int? GetInt32(string key);
    void Remove(string key);
}