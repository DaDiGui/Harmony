using System;
using UnityEngine;

public class XUiV_Sprite : XUiView
{
    [PublicizedFrom(EAccessModifier.Private)]
    public string uiAtlas = string.Empty;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool uiAtlasChanged;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string spriteName = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color color = Color.white;

    [PublicizedFrom(EAccessModifier.Private)]
    public Color? gradientStart;

    [PublicizedFrom(EAccessModifier.Private)]
    public Color? gradientEnd;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Type type;

    [PublicizedFrom(EAccessModifier.Protected)]
    public FillDirection fillDirection;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool fillInvert;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Flip flip;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UISprite sprite;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool fillCenter;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float fillAmount;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool foregroundLayer;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool lastVisible;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string spriteNameXB1 = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string spriteNamePS4 = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float globalOpacityModifier = 1f;

    [PublicizedFrom(EAccessModifier.Private)]
    public int borderSize = -1;

    public UISprite Sprite => sprite;

    public string UIAtlas
    {
        get
        {
            return uiAtlas;
        }
        set
        {
            if (uiAtlas != value)
            {
                uiAtlas = value;
                uiAtlasChanged = true;
                isDirty = true;
            }
        }
    }

    public string SpriteName
    {
        get
        {
            return spriteName;
        }
        set
        {
            if (spriteName != value)
            {
                spriteName = value;
                isDirty = true;
            }
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
            if (color.r != value.r || color.g != value.g || color.b != value.b || color.a != value.a)
            {
                color = value;
                isDirty = true;
            }
        }
    }

    public virtual Type Type
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return type;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_000a: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            if (type != value)
            {
                type = value;
                isDirty = true;
            }
        }
    }

    public FillDirection FillDirection
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return fillDirection;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_000a: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            if (fillDirection != value)
            {
                fillDirection = value;
                isDirty = true;
            }
        }
    }

    public bool FillInvert
    {
        get
        {
            return fillInvert;
        }
        set
        {
            if (fillInvert != value)
            {
                fillInvert = value;
                isDirty = true;
            }
        }
    }

    public bool FillCenter
    {
        get
        {
            return fillCenter;
        }
        set
        {
            if (fillCenter != value)
            {
                fillCenter = value;
                isDirty = true;
            }
        }
    }

    public float Fill
    {
        get
        {
            return fillAmount;
        }
        set
        {
            if (fillAmount != value && (double)Math.Abs((value - fillAmount) / value) > 0.005)
            {
                fillAmount = Mathf.Clamp01(value);
                isDirty = true;
            }
        }
    }

    public Flip Flip
    {
        get
        {
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            return ((UIBasicSprite)sprite).flip;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_000a: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            if (flip != value)
            {
                flip = value;
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
            if (globalOpacityModifier != value)
            {
                globalOpacityModifier = value;
                isDirty = true;
            }
        }
    }

    public bool ForegroundLayer
    {
        get
        {
            return foregroundLayer;
        }
        set
        {
            if (foregroundLayer != value)
            {
                foregroundLayer = value;
                isDirty = true;
            }
        }
    }

    public XUiV_Sprite(string _id)
        : base(_id)
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UISprite>();
    }

    public override void InitView()
    {
        base.InitView();
        sprite = uiTransform.GetComponent<UISprite>();
        UpdateData();
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (base.xui.GlobalOpacityChanged)
        {
            isDirty = true;
        }

        UISprite val = sprite;
        if (val != null)
        {
            bool flag = ((UIWidget)val).isVisible;
            if (lastVisible != flag)
            {
                isDirty = true;
                lastVisible = flag;
            }
        }
    }

    public override void UpdateData()
    {
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        //IL_001b: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b7: Unknown result type (might be due to invalid IL or missing references)
        //IL_01c8: Invalid comparison between Unknown and I4
        //IL_01f4: Unknown result type (might be due to invalid IL or missing references)
        //IL_01fa: Unknown result type (might be due to invalid IL or missing references)
        //IL_0208: Unknown result type (might be due to invalid IL or missing references)
        //IL_01dc: Unknown result type (might be due to invalid IL or missing references)
        //IL_0260: Unknown result type (might be due to invalid IL or missing references)
        //IL_0266: Unknown result type (might be due to invalid IL or missing references)
        //IL_0284: Unknown result type (might be due to invalid IL or missing references)
        //IL_028a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0274: Unknown result type (might be due to invalid IL or missing references)
        //IL_0298: Unknown result type (might be due to invalid IL or missing references)
        //IL_02bb: Unknown result type (might be due to invalid IL or missing references)
        _ = initialized;
        applyAtlasAndSprite();
        ((UIWidget)sprite).keepAspectRatio = keepAspectRatio;
        ((UIWidget)sprite).aspectRatio = aspectRatio;
        if (((UIWidget)sprite).color != color)
        {
            ((UIWidget)sprite).color = color;
        }

        if (gradientStart.HasValue)
        {
            sprite.gradientTop = gradientStart.Value;
            sprite.applyGradient = true;
        }

        if (gradientEnd.HasValue)
        {
            sprite.gradientBottom = gradientEnd.Value;
            sprite.applyGradient = true;
        }

        if (globalOpacityModifier != 0f && (foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
        {
            float a = Mathf.Clamp01(color.a * (globalOpacityModifier * (foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
            ((UIWidget)sprite).color = new Color(color.r, color.g, color.b, a);
        }

        if (borderSize > 0 && ((UIWidget)sprite).border.x != (float)borderSize)
        {
            ((UIWidget)sprite).border = new Vector4(borderSize, borderSize, borderSize, borderSize);
        }

        if ((int)((UIBasicSprite)sprite).centerType != (fillCenter ? 1 : 0))
        {
            ((UIBasicSprite)sprite).centerType = (AdvancedType)(fillCenter ? 1 : 0);
        }

        parseAnchors((UIWidget)(object)sprite);
        if (((UIBasicSprite)sprite).fillDirection != fillDirection)
        {
            ((UIBasicSprite)sprite).fillDirection = fillDirection;
        }

        if (((UIBasicSprite)sprite).invert != fillInvert)
        {
            ((UIBasicSprite)sprite).invert = fillInvert;
        }

        if (((UIBasicSprite)sprite).fillAmount != fillAmount)
        {
            ((UIBasicSprite)sprite).fillAmount = fillAmount;
        }

        if (((UIBasicSprite)sprite).type != type)
        {
            ((UIBasicSprite)sprite).type = type;
        }

        if (((UIBasicSprite)sprite).flip != flip)
        {
            ((UIBasicSprite)sprite).flip = flip;
        }

        if (!initialized)
        {
            initialized = true;
            ((UIWidget)sprite).pivot = pivot;
            ((UIWidget)sprite).depth = depth;
            uiTransform.localScale = Vector3.one;
            uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
            if (EventOnHover || EventOnPress)
            {
                BoxCollider obj = collider;
                obj.center = ((UIWidget)sprite).localCenter;
                obj.size = new Vector3(((UIWidget)sprite).localSize.x * colliderScale, ((UIWidget)sprite).localSize.y * colliderScale, 0f);
            }
        }

        if (((UIRect)sprite).isAnchored)
        {
            ((UIWidget)sprite).autoResizeBoxCollider = true;
        }
        else
        {
            RefreshBoxCollider();
        }

        base.UpdateData();
    }

    public override void SetDefaults(XUiController _parent)
    {
        base.SetDefaults(_parent);
        FillCenter = true;
        Type = (Type)0;
        FillDirection = (FillDirection)0;
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        //IL_0327: Unknown result type (might be due to invalid IL or missing references)
        //IL_0339: Unknown result type (might be due to invalid IL or missing references)
        //IL_0301: Unknown result type (might be due to invalid IL or missing references)
        bool flag = base.ParseAttribute(attribute, value, _parent);
        if (!flag)
        {
            switch (attribute)
            {
                case "atlas":
                    UIAtlas = value;
                    break;
                case "sprite":
                    SpriteName = value;
                    break;
                case "sprite_xb1":
                    spriteNameXB1 = value;
                    break;
                case "sprite_ps4":
                    spriteNamePS4 = value;
                    break;
                case "color":
                    Color = StringParsers.ParseColor32(value);
                    break;
                case "fill":
                    Fill = StringParsers.ParseFloat(value);
                    break;
                case "fillcenter":
                    FillCenter = StringParsers.ParseBool(value);
                    break;
                case "filldirection":
                    FillDirection = EnumUtils.Parse<FillDirection>(value, _ignoreCase: true);
                    break;
                case "fillinvert":
                    FillInvert = StringParsers.ParseBool(value);
                    break;
                case "flip":
                    Flip = EnumUtils.Parse<Flip>(value, _ignoreCase: true);
                    break;
                case "type":
                    Type = EnumUtils.Parse<Type>(value, _ignoreCase: true);
                    break;
                case "globalopacity":
                    if (!StringParsers.ParseBool(value))
                    {
                        GlobalOpacityModifier = 0f;
                    }

                    break;
                case "globalopacitymod":
                    GlobalOpacityModifier = StringParsers.ParseFloat(value);
                    break;
                case "bordersize":
                    borderSize = int.Parse(value);
                    break;
                case "foregroundlayer":
                    foregroundLayer = StringParsers.ParseBool(value);
                    break;
                case "gradient_start":
                    gradientStart = StringParsers.ParseColor32(value);
                    break;
                case "gradient_end":
                    gradientEnd = StringParsers.ParseColor32(value);
                    break;
                default:
                    return false;
            }

            return true;
        }

        return flag;
    }

    public void SetSpriteImmediately(string spriteName)
    {
        this.spriteName = spriteName;
        applyAtlasAndSprite(_force: true);
    }

    public void SetColorImmediately(Color color)
    {
        if ((UnityEngine.Object)(object)sprite != null)
        {
            ((UIWidget)sprite).color = color;
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool applyAtlasAndSprite(bool _force = false)
    {
        if ((UnityEngine.Object)(object)sprite == null)
        {
            return false;
        }

        if (!_force && sprite.spriteName != null && sprite.spriteName == spriteName && sprite.atlas != null && !uiAtlasChanged)
        {
            return false;
        }

        uiAtlasChanged = false;
        sprite.atlas = (INGUIAtlas)(object)base.xui.GetAtlasByName(UIAtlas, spriteName);
        sprite.spriteName = spriteName;
        return true;
    }
}
