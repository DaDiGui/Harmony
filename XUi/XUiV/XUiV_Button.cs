using UnityEngine;

public class XUiV_Button : XUiView
{
    [PublicizedFrom(EAccessModifier.Protected)]
    public string uiAtlas = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UISprite sprite;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Type type;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Flip flip;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string defaultSpriteName = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string hoverSpriteName = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string selectedSpriteName = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string disabledSpriteName = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color defaultSpriteColor = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color hoverSpriteColor = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color selectedSpriteColor = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color disabledSpriteColor = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool manualColors;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color currentColor = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string currentSpriteName = string.Empty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool selected;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool lastVisible;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool colorDirty;

    [PublicizedFrom(EAccessModifier.Private)]
    public float hoverScale = 1f;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool foregroundLayer = true;

    [PublicizedFrom(EAccessModifier.Private)]
    public TweenScale tweenScale;

    [PublicizedFrom(EAccessModifier.Private)]
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
            uiAtlas = value;
            isDirty = true;
        }
    }

    public Type Type
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return type;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            type = value;
            isDirty = true;
        }
    }

    public string DefaultSpriteName
    {
        get
        {
            return defaultSpriteName;
        }
        set
        {
            defaultSpriteName = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public Color DefaultSpriteColor
    {
        get
        {
            return defaultSpriteColor;
        }
        set
        {
            defaultSpriteColor = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public string HoverSpriteName
    {
        get
        {
            if (hoverSpriteName == "")
            {
                return defaultSpriteName;
            }

            return hoverSpriteName;
        }
        set
        {
            hoverSpriteName = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public Color HoverSpriteColor
    {
        get
        {
            return hoverSpriteColor;
        }
        set
        {
            hoverSpriteColor = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public string SelectedSpriteName
    {
        get
        {
            if (selectedSpriteName == "")
            {
                return defaultSpriteName;
            }

            return selectedSpriteName;
        }
        set
        {
            selectedSpriteName = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public Color SelectedSpriteColor
    {
        get
        {
            return selectedSpriteColor;
        }
        set
        {
            selectedSpriteColor = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public string DisabledSpriteName
    {
        get
        {
            if (disabledSpriteName == "")
            {
                return defaultSpriteName;
            }

            return disabledSpriteName;
        }
        set
        {
            disabledSpriteName = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public Color DisabledSpriteColor
    {
        get
        {
            return disabledSpriteColor;
        }
        set
        {
            disabledSpriteColor = value;
            isDirty = true;
            updateCurrentSprite();
        }
    }

    public bool ManualColors
    {
        get
        {
            return manualColors;
        }
        set
        {
            if (value != manualColors)
            {
                manualColors = value;
                isDirty = true;
                updateCurrentSprite();
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

    public Color CurrentColor
    {
        get
        {
            return currentColor;
        }
        set
        {
            currentColor = value;
            isDirty = true;
            colorDirty = true;
        }
    }

    public string CurrentSpriteName
    {
        get
        {
            return currentSpriteName;
        }
        set
        {
            if (value != currentSpriteName)
            {
                currentSpriteName = value;
                isDirty = true;
                colorDirty = true;
            }
        }
    }

    public bool Selected
    {
        get
        {
            return selected;
        }
        set
        {
            if (selected != value)
            {
                selected = value;
                isDirty = true;
                updateCurrentSprite();
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

    public float HoverScale
    {
        get
        {
            return hoverScale;
        }
        set
        {
            hoverScale = value;
            isDirty = true;
        }
    }

    public override bool Enabled
    {
        set
        {
            bool flag = enabled;
            base.Enabled = value;
            if (value != flag)
            {
                updateCurrentSprite();
                if (!value && hoverScale != 1f && tweenScale.value != Vector3.one)
                {
                    ((UITweener)tweenScale).SetStartToCurrentValue();
                    tweenScale.to = Vector3.one;
                    ((Behaviour)(object)tweenScale).enabled = true;
                    ((UITweener)tweenScale).duration = 0.25f;
                    ((UITweener)tweenScale).ResetToBeginning();
                }

                if (!gamepadSelectableSetFromAttributes)
                {
                    base.IsNavigatable = value;
                }
            }
        }
    }

    public XUiV_Button(string _id)
        : base(_id)
    {
        UseSelectionBox = false;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void updateCurrentSprite()
    {
        if (Enabled)
        {
            if (Selected)
            {
                if (!manualColors)
                {
                    CurrentColor = selectedSpriteColor;
                }

                CurrentSpriteName = SelectedSpriteName;
            }
            else
            {
                if (!manualColors)
                {
                    CurrentColor = (isOver ? hoverSpriteColor : defaultSpriteColor);
                }

                CurrentSpriteName = (isOver ? HoverSpriteName : DefaultSpriteName);
            }
        }
        else
        {
            if (!manualColors)
            {
                CurrentColor = disabledSpriteColor;
            }

            CurrentSpriteName = DisabledSpriteName;
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UISprite>();
        ((Behaviour)(object)_go.AddComponent<TweenScale>()).enabled = false;
    }

    public override void InitView()
    {
        EventOnPress = true;
        EventOnHover = true;
        base.InitView();
        sprite = uiTransform.GetComponent<UISprite>();
        UpdateData();
        initialized = true;
        Enabled = true;
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (null != (Object)(object)sprite)
        {
            bool flag = ((UIWidget)sprite).isVisible;
            if (lastVisible != flag)
            {
                isDirty = true;
            }

            lastVisible = flag;
            if (isOver && UICamera.hoveredObject != uiTransform.gameObject)
            {
                OnHover(uiTransform.gameObject, _isOver: false);
            }
        }
    }

    public override void UpdateData()
    {
        //IL_012b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0137: Unknown result type (might be due to invalid IL or missing references)
        //IL_0154: Unknown result type (might be due to invalid IL or missing references)
        //IL_015a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0168: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b5: Unknown result type (might be due to invalid IL or missing references)
        sprite.spriteName = currentSpriteName;
        sprite.atlas = (INGUIAtlas)(object)base.xui.GetAtlasByName(uiAtlas, currentSpriteName);
        ((UIWidget)sprite).color = currentColor;
        if (globalOpacityModifier != 0f && (foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
        {
            float a = Mathf.Clamp01(currentColor.a * (globalOpacityModifier * (foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
            ((UIWidget)sprite).color = new Color(currentColor.r, currentColor.g, currentColor.b, a);
        }

        if (borderSize > 0)
        {
            ((UIWidget)sprite).border = new Vector4(borderSize, borderSize, borderSize, borderSize);
        }

        ((UIBasicSprite)sprite).centerType = (AdvancedType)1;
        ((UIBasicSprite)sprite).type = type;
        parseAnchors((UIWidget)(object)sprite);
        if (((UIBasicSprite)sprite).flip != flip)
        {
            ((UIBasicSprite)sprite).flip = flip;
        }

        if (hoverScale != 1f && (Object)(object)tweenScale == null)
        {
            tweenScale = uiTransform.gameObject.GetComponent<TweenScale>();
        }

        if (!initialized)
        {
            ((UIWidget)sprite).pivot = pivot;
            ((UIWidget)sprite).depth = depth;
            uiTransform.localScale = Vector3.one;
            uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
            BoxCollider obj = collider;
            obj.center = ((UIWidget)sprite).localCenter;
            obj.size = new Vector3(((UIWidget)sprite).localSize.x * colliderScale, ((UIWidget)sprite).localSize.y * colliderScale, 0f);
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

    public override void OnHover(GameObject _go, bool _isOver)
    {
        base.OnHover(_go, _isOver);
        updateCurrentSprite();
        if (Enabled && hoverScale != 1f)
        {
            tweenScale.to = (isOver ? (Vector3.one * hoverScale) : Vector3.one);
            ((UITweener)tweenScale).SetStartToCurrentValue();
            ((UITweener)tweenScale).duration = 0.25f;
            ((UITweener)tweenScale).ResetToBeginning();
            ((Behaviour)(object)tweenScale).enabled = true;
        }
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        //IL_03a3: Unknown result type (might be due to invalid IL or missing references)
        //IL_0352: Unknown result type (might be due to invalid IL or missing references)
        bool flag = base.ParseAttribute(attribute, value, _parent);
        if (!flag)
        {
            switch (attribute)
            {
                case "atlas":
                    UIAtlas = value;
                    break;
                case "sprite":
                    DefaultSpriteName = value;
                    CurrentSpriteName = value;
                    break;
                case "defaultcolor":
                    DefaultSpriteColor = StringParsers.ParseColor32(value);
                    CurrentColor = defaultSpriteColor;
                    break;
                case "hoversprite":
                    HoverSpriteName = value;
                    break;
                case "hovercolor":
                    HoverSpriteColor = StringParsers.ParseColor32(value);
                    break;
                case "selectedsprite":
                    SelectedSpriteName = value;
                    break;
                case "selectedcolor":
                    SelectedSpriteColor = StringParsers.ParseColor32(value);
                    break;
                case "disabledsprite":
                    DisabledSpriteName = value;
                    break;
                case "disabledcolor":
                    DisabledSpriteColor = StringParsers.ParseColor32(value);
                    break;
                case "manualcolors":
                    ManualColors = StringParsers.ParseBool(value);
                    break;
                case "selected":
                    Selected = StringParsers.ParseBool(value);
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
                case "hoverscale":
                    HoverScale = StringParsers.ParseFloat(value);
                    break;
                case "flip":
                    Flip = EnumUtils.Parse<Flip>(value, _ignoreCase: true);
                    break;
                case "foregroundlayer":
                    foregroundLayer = StringParsers.ParseBool(value);
                    break;
                default:
                    return false;
            }

            return true;
        }

        return flag;
    }

    public override void OnOpen()
    {
        base.OnOpen();
        updateCurrentSprite();
        uiTransform.localScale = Vector3.one;
    }
}
