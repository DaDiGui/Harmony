using System;
using System.Collections.Generic;
using UnityEngine;

public class XUiV_TextList : XUiView
{
    [PublicizedFrom(EAccessModifier.Protected)]
    public UITextList textList;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UILabel label;

    [PublicizedFrom(EAccessModifier.Protected)]
    public NGUIFont uiFont;

    [PublicizedFrom(EAccessModifier.Protected)]
    public int fontSize;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Effect effect;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color effectColor = new Color32(0, 0, 0, 80);

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2 effectDistance;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Crispness crispness;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color color;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Style listStyle;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string firstLinePrefix;

    [PublicizedFrom(EAccessModifier.Protected)]
    public int paragraphHistory = 50;

    [PublicizedFrom(EAccessModifier.Protected)]
    public new Alignment alignment;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool supportBbCode = true;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool supportUrls;

    [PublicizedFrom(EAccessModifier.Protected)]
    public HashSet<string> supportedUrlTypes;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool upperCase;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool lowerCase;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool bUpdateText;

    [PublicizedFrom(EAccessModifier.Private)]
    public float globalOpacityModifier = 1f;

    public UILabel Label => label;

    public UITextList TextList => textList;

    public NGUIFont UIFont
    {
        get
        {
            return uiFont;
        }
        set
        {
            uiFont = value;
            isDirty = true;
        }
    }

    public int FontSize
    {
        get
        {
            return fontSize;
        }
        set
        {
            fontSize = value;
            isDirty = true;
        }
    }

    public Effect Effect
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return effect;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            effect = value;
            isDirty = true;
        }
    }

    public Color EffectColor
    {
        get
        {
            return effectColor;
        }
        set
        {
            effectColor = value;
            isDirty = true;
        }
    }

    public Vector2 EffectDistance
    {
        get
        {
            return effectDistance;
        }
        set
        {
            effectDistance = value;
            isDirty = true;
        }
    }

    public Crispness Crispness
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return crispness;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            crispness = value;
            isDirty = true;
        }
    }

    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color != value)
            {
                color = value;
                isDirty = true;
            }
        }
    }

    public Alignment Alignment
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return alignment;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_000a: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            if (alignment != value)
            {
                alignment = value;
                isDirty = true;
            }
        }
    }

    public bool SupportBbCode
    {
        get
        {
            return supportBbCode;
        }
        set
        {
            if (supportBbCode != value)
            {
                supportBbCode = value;
                isDirty = true;
            }
        }
    }

    public int ParagraphHistory
    {
        get
        {
            return paragraphHistory;
        }
        set
        {
            if (value != paragraphHistory)
            {
                paragraphHistory = value;
                isDirty = true;
            }
        }
    }

    public Style ListStyle
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return listStyle;
        }
        set
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            //IL_000a: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            if (value != listStyle)
            {
                listStyle = value;
                isDirty = true;
            }
        }
    }

    public float GlobalOpacityModifier
    {
        get
        {
            return globalOpacityModifier;
        }
        set
        {
            globalOpacityModifier = value;
            isDirty = true;
        }
    }

    public XUiV_TextList(string _id)
        : base(_id)
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UILabel>();
        _go.AddComponent<UITextList>();
    }

    public override void InitView()
    {
        if (supportUrls)
        {
            EventOnPress = true;
            controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
            {
                XUiUtils.HandleLabelUrlClick(this, label, supportedUrlTypes);
            };
        }

        EventOnDrag = true;
        EventOnScroll = true;
        base.InitView();
        label = uiTransform.GetComponent<UILabel>();
        textList = uiTransform.GetComponent<UITextList>();
        if ((UnityEngine.Object)(object)UIFont != null)
        {
            UpdateData();
        }

        initialized = true;
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (base.xui.GlobalOpacityChanged)
        {
            isDirty = true;
        }
    }

    public override void UpdateData()
    {
        //IL_008b: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ad: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b2: Unknown result type (might be due to invalid IL or missing references)
        //IL_00be: Unknown result type (might be due to invalid IL or missing references)
        //IL_010e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0113: Unknown result type (might be due to invalid IL or missing references)
        //IL_012a: Unknown result type (might be due to invalid IL or missing references)
        base.UpdateData();
        if ((UnityEngine.Object)(object)uiFont != null)
        {
            label.font = (INGUIFont)(object)uiFont;
        }

        ((UIWidget)label).depth = depth;
        label.fontSize = fontSize;
        ((UIWidget)label).width = size.x;
        ((UIWidget)label).height = size.y;
        ((UIWidget)label).color = color;
        label.alignment = alignment;
        label.supportEncoding = supportBbCode;
        label.keepCrispWhenShrunk = crispness;
        label.effectStyle = effect;
        label.effectColor = effectColor;
        label.effectDistance = effectDistance;
        label.spacingX = 1;
        textList.paragraphHistory = paragraphHistory;
        textList.style = listStyle;
        if (!initialized)
        {
            ((UIWidget)label).pivot = pivot;
            uiTransform.localScale = Vector3.one;
            uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
            if (EventOnHover || EventOnPress)
            {
                BoxCollider obj = collider;
                obj.center = ((UIWidget)Label).localCenter;
                obj.size = new Vector3(((UIWidget)label).localSize.x * colliderScale, ((UIWidget)label).localSize.y * colliderScale, 0f);
            }
        }
    }

    public void AddLine(string _line)
    {
        if (!string.IsNullOrEmpty(firstLinePrefix))
        {
            _line = firstLinePrefix + _line;
        }

        if (upperCase)
        {
            _line = _line.ToUpper();
        }
        else if (lowerCase)
        {
            _line = _line.ToLower();
        }

        textList.Add(_line);
    }

    public override void SetDefaults(XUiController _parent)
    {
        base.SetDefaults(_parent);
        Alignment = (Alignment)1;
        FontSize = 16;
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        //IL_0330: Unknown result type (might be due to invalid IL or missing references)
        //IL_0437: Unknown result type (might be due to invalid IL or missing references)
        //IL_043c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0321: Unknown result type (might be due to invalid IL or missing references)
        //IL_033f: Unknown result type (might be due to invalid IL or missing references)
        if (base.ParseAttribute(attribute, value, _parent))
        {
            return true;
        }

        switch (attribute)
        {
            case "font_face":
                UIFont = base.xui.GetUIFontByName(value, _showWarning: false);
                if ((UnityEngine.Object)(object)UIFont == null)
                {
                    Log.Warning("XUi TextList: Font not found: " + value + ", from: " + base.Controller.GetParentWindow().ID + "." + base.ID);
                }

                return true;
            case "font_size":
                FontSize = int.Parse(value);
                return true;
            case "color":
                Color = StringParsers.ParseColor32(value);
                return true;
            case "justify":
                Alignment = EnumUtils.Parse<Alignment>(value, _ignoreCase: true);
                return true;
            case "crispness":
                Crispness = EnumUtils.Parse<Crispness>(value, _ignoreCase: true);
                return true;
            case "effect":
                Effect = EnumUtils.Parse<Effect>(value, _ignoreCase: true);
                return true;
            case "effect_color":
                EffectColor = StringParsers.ParseColor32(value);
                return true;
            case "effect_distance":
                EffectDistance = StringParsers.ParseVector2(value);
                return true;
            case "upper_case":
                upperCase = StringParsers.ParseBool(value);
                return true;
            case "lower_case":
                lowerCase = StringParsers.ParseBool(value);
                return true;
            case "globalopacity":
                if (!StringParsers.ParseBool(value))
                {
                    GlobalOpacityModifier = 0f;
                }

                return true;
            case "globalopacitymod":
                GlobalOpacityModifier = StringParsers.ParseFloat(value);
                return true;
            case "support_bb_code":
                supportBbCode = StringParsers.ParseBool(value);
                return true;
            case "support_urls":
                if (value.EqualsCaseInsensitive("false"))
                {
                    supportUrls = false;
                }
                else
                {
                    supportUrls = true;
                    if (value.EqualsCaseInsensitive("true"))
                    {
                        supportedUrlTypes = new HashSet<string> { "HTTP" };
                    }
                    else
                    {
                        supportedUrlTypes = new HashSet<string>(value.Split(",", StringSplitOptions.None));
                    }
                }

                return true;
            case "max_paragraphs":
                paragraphHistory = StringParsers.ParseSInt32(value);
                return true;
            case "list_style":
                listStyle = EnumUtils.Parse<Style>(value, _ignoreCase: true);
                return true;
            case "prefix_first_line":
                firstLinePrefix = value;
                return true;
            default:
                return false;
        }
    }
}
