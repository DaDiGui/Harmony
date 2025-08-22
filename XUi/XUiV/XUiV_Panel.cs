using UnityEngine;

public class XUiV_Panel : XUiView
{
    [PublicizedFrom(EAccessModifier.Protected)]
    public string backgroundSpriteName = XUi.BlankTexture;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string borderSpriteName = XUi.BlankTexture;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color borderColor = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public XUi_Thickness borderThickness;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiSideSizes borderInset;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color backgroundColor = Color.white;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool useBackground = true;

    public bool createUiPanel;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiV_Sprite borderSprite;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiV_Sprite backgroundSprite;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UIPanel panel;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Clipping clipping;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2 clippingSize = new Vector2(-10000f, -10000f);

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2 clippingCenter = new Vector2(-10000f, -10000f);

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2 clippingSoftness;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool disableAutoBackground;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float globalOpacityModifier = 1f;

    public float GlobalOpacityModifier
    {
        get
        {
            return globalOpacityModifier;
        }
        set
        {
            if (globalOpacityModifier != value)
            {
                globalOpacityModifier = value;
                isDirty = true;
            }
        }
    }

    public Color BackgroundColor
    {
        get
        {
            return backgroundColor;
        }
        set
        {
            backgroundColor = value;
            isDirty = true;
            useBackground = true;
        }
    }

    public string BackgroundSpriteName
    {
        get
        {
            return backgroundSpriteName;
        }
        set
        {
            backgroundSpriteName = value;
            if (value == "")
            {
                backgroundSpriteName = XUi.BlankTexture;
            }
        }
    }

    public string BorderSpriteName
    {
        get
        {
            return borderSpriteName;
        }
        set
        {
            borderSpriteName = value;
            if (value == "")
            {
                borderSpriteName = XUi.BlankTexture;
            }
        }
    }

    public Color BorderColor
    {
        get
        {
            return borderColor;
        }
        set
        {
            borderColor = value;
            isDirty = true;
            useBackground = true;
        }
    }

    public XUi_Thickness BorderThickness
    {
        get
        {
            return borderThickness;
        }
        set
        {
            borderThickness = value;
            isDirty = true;
            useBackground = true;
        }
    }

    public Clipping Clipping
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return clipping;
        }
        set
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            //IL_000a: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            if (value != clipping)
            {
                clipping = value;
                isDirty = true;
            }
        }
    }

    public Vector2 ClippingSize
    {
        get
        {
            return clippingSize;
        }
        set
        {
            if (value != clippingSize)
            {
                clippingSize = value;
                isDirty = true;
            }
        }
    }

    public Vector2 ClippingCenter
    {
        get
        {
            return clippingCenter;
        }
        set
        {
            if (value != clippingCenter)
            {
                clippingCenter = value;
                isDirty = true;
            }
        }
    }

    public Vector2 ClippingSoftness
    {
        get
        {
            return clippingSoftness;
        }
        set
        {
            if (value != clippingSoftness)
            {
                clippingSoftness = value;
                isDirty = true;
            }
        }
    }

    public override bool Enabled
    {
        get
        {
            return enabled;
        }
        set
        {
            enabled = value;
            RefreshEnabled();
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void RefreshEnabled()
    {
        if ((Object)(object)panel != null)
        {
            ((Component)(object)panel).gameObject.SetActive(enabled);
            ((Behaviour)(object)panel).enabled = enabled;
            borderSprite.Enabled = enabled;
            backgroundSprite.Enabled = enabled;
        }
    }

    public XUiV_Panel(string _id)
        : base(_id)
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UIPanel>();
    }

    public override void InitView()
    {
        //IL_033b: Unknown result type (might be due to invalid IL or missing references)
        //IL_02ff: Unknown result type (might be due to invalid IL or missing references)
        borderSprite = new XUiV_Sprite("_border");
        borderSprite.xui = base.xui;
        borderSprite.Controller = new XUiController(base.Controller);
        borderSprite.Controller.xui = base.xui;
        borderSprite.IgnoreParentPadding = true;
        backgroundSprite = new XUiV_Sprite("_background");
        backgroundSprite.xui = base.xui;
        backgroundSprite.Controller = new XUiController(base.Controller);
        backgroundSprite.Controller.xui = base.xui;
        backgroundSprite.IgnoreParentPadding = true;
        if (!disableAutoBackground && useBackground)
        {
            borderSprite.SetDefaults(base.Controller);
            borderSprite.GlobalOpacityModifier = 0f;
            borderSprite.Position = new Vector2i(borderInset.Left, -borderInset.Top);
            borderSprite.Size = new Vector2i(base.Size.x - borderInset.SumLeftRight, base.Size.y - borderInset.SumTopBottom);
            borderSprite.UIAtlas = "UIAtlas";
            borderSprite.SpriteName = borderSpriteName;
            borderSprite.Color = borderColor;
            borderSprite.Depth = depth + 1;
            borderSprite.Pivot = (Pivot)0;
            borderSprite.Type = (Type)1;
            borderSprite.Controller.WindowGroup = base.Controller.WindowGroup;
            backgroundSprite.SetDefaults(base.Controller);
            backgroundSprite.GlobalOpacityModifier = 2f;
            backgroundSprite.Position = new Vector2i(borderThickness.left, -borderThickness.top);
            backgroundSprite.Size = new Vector2i(base.Size.x - (borderThickness.left + borderThickness.right), base.Size.y - (borderThickness.top + borderThickness.bottom));
            backgroundSprite.UIAtlas = "UIAtlas";
            backgroundSprite.SpriteName = backgroundSpriteName;
            backgroundSprite.Color = backgroundColor;
            backgroundSprite.Depth = depth;
            backgroundSprite.Pivot = (Pivot)0;
            backgroundSprite.Type = (Type)1;
            backgroundSprite.Controller.WindowGroup = base.Controller.WindowGroup;
        }

        base.InitView();
        panel = uiTransform.gameObject.GetComponent<UIPanel>();
        if (!createUiPanel && (int)clipping == 0)
        {
            Object.Destroy((Object)(object)panel);
            panel = null;
        }
        else
        {
            ((Behaviour)(object)panel).enabled = true;
            panel.depth = depth;
            if ((int)clipping != 0)
            {
                if (clippingCenter == new Vector2(-10000f, -10000f))
                {
                    clippingCenter = new Vector2(size.x / 2, -size.y / 2);
                }

                if (clippingSize == new Vector2(-10000f, -10000f))
                {
                    clippingSize = new Vector2(size.x, size.y);
                }

                updateClipping();
            }
        }

        BoxCollider val = collider;
        if ((Object)(object)val != null)
        {
            float x = (float)size.x * 0.5f;
            float num = (float)size.y * 0.5f;
            val.center = new Vector3(x, 0f - num, 0f);
            val.size = new Vector3((float)size.x * colliderScale, (float)size.y * colliderScale, 0f);
        }

        if (!disableAutoBackground && useBackground && backgroundSprite != null && borderSprite != null)
        {
            backgroundSprite.Color = backgroundColor;
            borderSprite.Color = borderColor;
        }

        RefreshEnabled();
        isDirty = true;
    }

    public override void UpdateData()
    {
        if (isDirty)
        {
            if (!disableAutoBackground && useBackground && backgroundSprite != null)
            {
                borderSprite.FillCenter = false;
                borderSprite.Position = new Vector2i(borderInset.Left, -borderInset.Top);
                borderSprite.Size = new Vector2i(base.Size.x - borderInset.SumLeftRight, base.Size.y - borderInset.SumTopBottom);
                borderSprite.Color = borderColor;
                borderSprite.SpriteName = borderSpriteName;
                borderSprite.GlobalOpacityModifier = globalOpacityModifier;
                backgroundSprite.Size = new Vector2i(base.Size.x - (borderThickness.left + borderThickness.right), base.Size.y - (borderThickness.top + borderThickness.bottom));
                backgroundSprite.Position = new Vector2i(borderThickness.left, -borderThickness.top);
                backgroundSprite.Color = backgroundColor;
                backgroundSprite.SpriteName = backgroundSpriteName;
                backgroundSprite.GlobalOpacityModifier = globalOpacityModifier;
            }

            if ((Object)(object)panel != null)
            {
                panel.depth = depth;
            }

            updateClipping();
        }

        base.UpdateData();
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void updateClipping()
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0011: Unknown result type (might be due to invalid IL or missing references)
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_0025: Unknown result type (might be due to invalid IL or missing references)
        if ((int)clipping != 0)
        {
            if (panel.clipping != clipping)
            {
                panel.clipping = clipping;
            }

            if (panel.clipSoftness != clippingSoftness)
            {
                panel.clipSoftness = clippingSoftness;
            }

            if (clippingSize.x < 0f)
            {
                clippingSize.x = 0f;
            }

            if (clippingSize.y < 0f)
            {
                clippingSize.y = 0f;
            }

            Vector4 vector = new Vector4(clippingCenter.x, clippingCenter.y, clippingSize.x, clippingSize.y);
            if (panel.baseClipRegion != vector)
            {
                panel.baseClipRegion = vector;
            }
        }
    }

    public override void SetDefaults(XUiController _parent)
    {
        base.SetDefaults(_parent);
        backgroundColor = new Color32(96, 96, 96, byte.MaxValue);
        borderColor = new Color32(0, 0, 0, 0);
        borderThickness = new XUi_Thickness(3);
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        //IL_02e8: Unknown result type (might be due to invalid IL or missing references)
        //IL_02ed: Unknown result type (might be due to invalid IL or missing references)
        bool flag = base.ParseAttribute(attribute, value, _parent);
        if (!flag)
        {
            flag = true;
            switch (attribute)
            {
                case "globalopacity":
                    if (!StringParsers.ParseBool(value))
                    {
                        GlobalOpacityModifier = 0f;
                    }

                    break;
                case "globalopacitymod":
                    GlobalOpacityModifier = StringParsers.ParseFloat(value);
                    break;
                case "backgroundcolor":
                    BackgroundColor = StringParsers.ParseColor32(value);
                    break;
                case "backgroundspritename":
                    BackgroundSpriteName = value;
                    break;
                case "bordercolor":
                    BorderColor = StringParsers.ParseColor32(value);
                    break;
                case "borderspritename":
                    BorderSpriteName = value;
                    break;
                case "borderthickness":
                    BorderThickness = XUi_Thickness.Parse(value);
                    break;
                case "borderinset":
                    XUiSideSizes.TryParse(value, out borderInset, attribute);
                    break;
                case "disableautobackground":
                    disableAutoBackground = StringParsers.ParseBool(value);
                    break;
                case "clipping":
                    clipping = EnumUtils.Parse<Clipping>(value, _ignoreCase: true);
                    break;
                case "clippingsize":
                    clippingSize = StringParsers.ParseVector2(value);
                    break;
                case "clippingcenter":
                    clippingCenter = StringParsers.ParseVector2(value);
                    break;
                case "clippingsoftness":
                    clippingSoftness = StringParsers.ParseVector2(value);
                    break;
                case "createuipanel":
                    createUiPanel = StringParsers.ParseBool(value);
                    break;
                case "snapcursor":
                    EventOnHover = true;
                    break;
                default:
                    flag = false;
                    break;
            }
        }

        return flag;
    }
}
