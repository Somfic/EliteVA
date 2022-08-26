using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EliteVA.Proxy.Variables
{
    public class VoiceAttackVariables
    {
        private readonly dynamic _proxy;

        private Dictionary<(string category, string name), string> _setVariables;

        public IReadOnlyDictionary<(string category, string name), string> SetVariables => _setVariables;

        internal VoiceAttackVariables(dynamic vaProxy)
        {
            _proxy = vaProxy;
            _setVariables = new Dictionary<(string, string), string>();
        }

        /// <summary>
        /// Set a variable
        /// </summary>
        /// <typeparam name="T">The type of variable</typeparam>
        /// <param name="name">The name of the variable</param>
        /// <param name="value">The value of the variable</param>
        public void Set<T>(string category, string name, T value)
        {
            var code = Convert.GetTypeCode(value);
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
                    SetBoolean(category, name, (bool) Convert.ChangeType(value, typeof(bool)));
                    break;

                case TypeCode.DateTime:
                    SetDate(category, name, (DateTime) Convert.ChangeType(value, typeof(DateTime)));
                    break;

                case TypeCode.Single:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    SetDecimal(category, name, (decimal) Convert.ChangeType(value, typeof(decimal)));
                    break;

                case TypeCode.Char:
                case TypeCode.String:
                    SetText(category, name, (string) Convert.ChangeType(value, typeof(string)));
                    break;

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.SByte:
                    SetShort(category, name, (short) Convert.ChangeType(value, typeof(short)));
                    break;

                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    try
                    {
                        SetInt(category, name, (int) Convert.ChangeType(value, typeof(int)));
                    }
                    catch (OverflowException)
                    {
                        SetDecimal(category, name, (decimal) Convert.ChangeType(value, typeof(decimal)));
                    } 
                    break;

                case TypeCode.Object:
                    var newCode = Convert.GetTypeCode(value);
                    Set(category, name, value, newCode);
                    break;

                case TypeCode.Empty:
                    break;
            }
        }

        /// <summary>
        /// Get a variable
        /// </summary>
        /// <typeparam name="T">The type of variable</typeparam>
        /// <param name="name">The name of the variable</param>
        public T Get<T>(string name)
        {
            var code = Type.GetTypeCode(typeof(T));

            switch (code)
            {
                case TypeCode.Boolean:
                    return (T) Convert.ChangeType(GetBoolean(name), typeof(T));

                case TypeCode.DateTime:
                    return (T) Convert.ChangeType(GetDate(name), typeof(T));

                case TypeCode.Single:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return (T) Convert.ChangeType(GetDecimal(name), typeof(T));

                case TypeCode.Char:
                case TypeCode.String:
                    return (T) Convert.ChangeType(GetText(name), typeof(T));

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.SByte:
                    return (T) Convert.ChangeType(GetShort(name), typeof(T));

                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return (T) Convert.ChangeType(GetInt(name), typeof(T));

                default:
                    return default;
            }
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
            if (_setVariables.ContainsKey((category, name))) { _setVariables[(category, name)] = value; }
            else { _setVariables.Add((category, name), value); }
        }
    }
}