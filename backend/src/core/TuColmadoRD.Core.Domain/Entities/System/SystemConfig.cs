using System.ComponentModel.DataAnnotations;

namespace TuColmadoRD.Core.Domain.Entities.System;

public class SystemConfig
{
    [Key]
    public string Key { get; private set; }
    public string Value { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SystemConfig() 
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public SystemConfig(string key, string value)
    {
        Key = key;
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
