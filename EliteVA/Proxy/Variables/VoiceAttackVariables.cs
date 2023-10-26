using System.Globalization;
using Newtonsoft.Json.Linq;

namespace EliteVA.Proxy.Variables;

public class VoiceAttackVariables
{
    private readonly dynamic _proxy;

    private List<(string category, string name, string value)> _setVariables;

    public IReadOnlyList<(string category, string name, string value)> SetVariables => _setVariables;
    
    public event EventHandler? OnVariablesSet;

    internal VoiceAttackVariables(dynamic vaProxy)
    {
        _proxy = vaProxy;
        _setVariables = new List<(string, string, string)>();
    }
    
    public void ClearStartingWith(string name)
    {
        _setVariables = _setVariables.Where(x => !x.name.Split(':')[1].StartsWith(name)).ToList();
    }

    /// <summary>
    /// Set a variable
    /// </summary>
    /// <typeparam name="T">The type of variable</typeparam>
    /// <param name="name">The name of the variable</param>
    /// <param name="value">The value of the variable</param>
    public void Set(string category, string name, JToken value)
    {
        var code = value.Type;
        Set(category, name, value, code);
    }

    /// <summary>
    /// Set a variable
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <param name="value">The value of the variable</param>
    /// <param name="code">The type of variable</param>
    public void Set(string category, string name, object value, TypeCode code)
    {
        switch (code)
        {
            case TypeCode.Boolean:
                SetBoolean(category, name, bool.Parse(value.ToString()));
                break;

            case TypeCode.DateTime:
                SetDate(category, name, DateTime.Parse(value.ToString().Trim('"')));
                break;

            case TypeCode.Single:
            case TypeCode.Decimal:
            case TypeCode.Double:
                SetDecimal(category, name, decimal.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture));
                break;

            case TypeCode.Char:
            case TypeCode.String:
                SetText(category, name, value.ToString().Trim('"'));
                break;

            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.SByte:
                SetShort(category, name, short.Parse(value.ToString()));
                break;

            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                try
                {
                    SetInt(category, name, int.Parse(value.ToString()));
                }
                catch (OverflowException)
                {
                    SetDecimal(category, name, decimal.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture));
                }

                break;
        }
    }
    
    /// <summary>
    /// Set a variable
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <param name="value">The value of the variable</param>
    /// <param name="code">The type of variable</param>
    public void Set(string category, string name, object value, JTokenType code)
    {
        switch (code)
        {
            case JTokenType.Boolean:
                SetBoolean(category, name, bool.Parse(value.ToString()));
                break;

            case JTokenType.Date:
            case JTokenType.TimeSpan:
                SetDate(category, name, DateTime.Parse(value.ToString().Trim('"')));
                break;
                
            case JTokenType.Float:
                SetDecimal(category, name, decimal.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture));
                break;

            case JTokenType.String:
                SetText(category, name, value.ToString().Trim('"'));
                break;

            case JTokenType.Integer:
                try
                {
                    SetInt(category, name, int.Parse(value.ToString()));
                }
                catch (OverflowException)
                {
                    SetDecimal(category, name, decimal.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture));
                } 
                break;
        }
    }

    /// <summary>
    /// Get a variable
    /// </summary>
    /// <typeparam name="T">The type of variable</typeparam>
    /// <param name="name">The name of the variable</param>
    public T? Get<T>(string name, T @default = default)
    {
        var code = Type.GetTypeCode(typeof(T));
        T value = @default;
        switch (code)
        {
            case TypeCode.Boolean:
                value = GetBoolean(name).HasValue ? (T)Convert.ChangeType(GetBoolean(name), typeof(T)) : @default;
                break;
            case TypeCode.DateTime:
                value = GetDate(name).HasValue ? (T)Convert.ChangeType(GetDate(name), typeof(T)) : @default;
                break;
            case TypeCode.Single:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                value = GetDecimal(name).HasValue ? (T)Convert.ChangeType(GetDecimal(name), typeof(T)) : @default;
                break;
            case TypeCode.Char:
            case TypeCode.String:
                value = string.IsNullOrWhiteSpace(GetText(name)) ? @default : (T)Convert.ChangeType(GetText(name), typeof(T));
                break;
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.SByte:
                value = GetShort(name).HasValue ? (T)Convert.ChangeType(GetShort(name), typeof(T)) : @default;
                break;
            case TypeCode.Int32:
            case TypeCode.UInt32:
                value = GetInt(name).HasValue ? (T)Convert.ChangeType(GetInt(name), typeof(T)) : @default;
                break;
        }
        return value;
    }
    
    private short? GetShort(string name)
    {
        return _proxy.GetSmallInt(name);
    }

    private int? GetInt(string name)
    {
        return _proxy.GetInt(name);
    }

    private string GetText(string name)
    {
        return _proxy.GetText(name);
    }

    private decimal? GetDecimal(string name)
    {
        return _proxy.GetDecimal(name);
    }

    private bool? GetBoolean(string name)
    {
        return _proxy.GetBoolean(name);
    }

    private DateTime? GetDate(string name)
    {
        return _proxy.GetDate(name);
    }

    private void SetShort(string category, string name, short? value)
    {
        var variable = $"{{SHORT:{name}}}";
        SetVariable(category, variable, value.ToString());

        _proxy.SetSmallInt(name, value);
    }

    private void SetInt(string category, string name, int? value)
    {
        var variable = $"{{INT:{name}}}";
        SetVariable(category, variable, value.ToString());

        _proxy.SetInt(name, value);
    }

    private void SetText(string category, string name, string value)
    {
        var variable = $"{{TXT:{name}}}";
        SetVariable(category, variable, value ?? "");

        _proxy.SetText(name, value);
    }

    private void SetDecimal(string category, string name, decimal? value)
    {
        var variable = $"{{DEC:{name}}}";
        SetVariable(category, variable, value.ToString());

        _proxy.SetDecimal(name, value);
    }

    private void SetBoolean(string category, string name, bool? value)
    {
        var variable = $"{{BOOL:{name}}}";
        SetVariable(category, variable, value.ToString());

        _proxy.SetBoolean(name, value);
    }

    private void SetDate(string category, string name, DateTime? value)
    {
        var variable = $"{{DATE:{name}}}";
        SetVariable(category, variable, value.ToString());

        _proxy.SetDate(name, value);
    }

    private void SetVariable(string category, string name, string value)
    {
        var index = _setVariables.FindIndex(x => x.name == name);
        if (index >= 0)
            _setVariables[index] = (category, name, value);
        else
            _setVariables.Insert(0, (category, name, value));
        
        OnVariablesSet?.Invoke(this, EventArgs.Empty);
    }
}