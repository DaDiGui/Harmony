using System;
using System.Collections.Generic;
using System.Xml.Linq;

public class RequirementBase : IRequirement
{
    [PublicizedFrom(EAccessModifier.Protected)]
    public enum OperationTypes
    {
        None,
        Equals,
        EQ,
        E,
        NotEquals,
        NEQ,
        NE,
        Less,
        LessThan,
        LT,
        Greater,
        GreaterThan,
        GT,
        LessOrEqual,
        LessThanOrEqualTo,
        LTE,
        GreaterOrEqual,
        GreaterThanOrEqualTo,
        GTE
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public OperationTypes operation;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string cvarName;

    public bool invert;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float value;

    public string Description;

    public virtual bool IsValid(MinEventParams _params)
    {
        if (cvarName != null)
        {
            value = _params.Self.Buffs.GetCustomVar(cvarName);
        }

        return true;
    }

    public void SetDescription(string desc)
    {
        Description = desc;
    }

    public virtual void GetInfoStrings(ref List<string> list)
    {
    }

    public virtual bool ParseXAttribute(XAttribute _attribute)
    {
        switch (_attribute.Name.LocalName)
        {
            case "operation":
                operation = EnumUtils.Parse<OperationTypes>(_attribute.Value, _ignoreCase: true);
                return true;
            case "value":
                {
                    string text = _attribute.Value;
                    if (text.Length > 0)
                    {
                        if (text[0] == '@')
                        {
                            cvarName = text.Substring(1);
                        }
                        else
                        {
                            value = StringParsers.ParseFloat(text);
                        }
                    }

                    return true;
                }
            case "invert":
                invert = StringParsers.ParseBool(_attribute.Value);
                return true;
            default:
                return false;
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public static bool compareValues(float _valueA, OperationTypes _operation, float _valueB)
    {
        switch (_operation)
        {
            case OperationTypes.Equals:
            case OperationTypes.EQ:
            case OperationTypes.E:
                return _valueA == _valueB;
            case OperationTypes.NotEquals:
            case OperationTypes.NEQ:
            case OperationTypes.NE:
                return _valueA != _valueB;
            case OperationTypes.Less:
            case OperationTypes.LessThan:
            case OperationTypes.LT:
                return _valueA < _valueB;
            case OperationTypes.Greater:
            case OperationTypes.GreaterThan:
            case OperationTypes.GT:
                return _valueA > _valueB;
            case OperationTypes.LessOrEqual:
            case OperationTypes.LessThanOrEqualTo:
            case OperationTypes.LTE:
                return _valueA <= _valueB;
            case OperationTypes.GreaterOrEqual:
            case OperationTypes.GreaterThanOrEqualTo:
            case OperationTypes.GTE:
                return _valueA >= _valueB;
            default:
                return false;
        }
    }

    public static IRequirement ParseRequirement(XElement _element)
    {
        string text = _element.GetAttribute("name");
        if (text.Length == 0)
        {
            return null;
        }

        bool flag = text[0] == '!';
        if (flag)
        {
            text = text.Substring(1);
        }

        Type type = Type.GetType(text);
        if (type == null)
        {
            return null;
        }

        RequirementBase requirementBase = (RequirementBase)Activator.CreateInstance(type);
        requirementBase.invert = flag;
        string attribute = _element.GetAttribute("desc_key");
        if (attribute.Length > 0)
        {
            requirementBase.SetDescription(Localization.Get(attribute));
        }

        foreach (XAttribute item in _element.Attributes())
        {
            requirementBase.ParseXAttribute(item);
        }

        return requirementBase;
    }

    public static List<IRequirement> ParseRequirements(XElement _element)
    {
        List<IRequirement> list = new List<IRequirement>();
        foreach (XElement item in _element.Elements("requirement"))
        {
            list.Add(ParseRequirement(item));
        }

        return list;
    }

    public string GetInfoString()
    {
        if (Description != null)
        {
            return Description;
        }

        List<string> list = new List<string>();
        GetInfoStrings(ref list);
        if (list.Count > 0)
        {
            return list[0];
        }

        return null;
    }
}
