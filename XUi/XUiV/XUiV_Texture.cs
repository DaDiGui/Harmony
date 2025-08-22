using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class XUiV_Texture : XUiView
{
    [PublicizedFrom(EAccessModifier.Protected)]
    public UITexture uiTexture;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Texture texture;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string pathName;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Material material;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Rect uvRect = new Rect(0f, 0f, 1f, 1f);

    [PublicizedFrom(EAccessModifier.Protected)]
    public Type type;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector4 border = Vector4.zero;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Flip flip;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Color color = Color.white;

    [PublicizedFrom(EAccessModifier.Protected)]
    public FillDirection fillDirection;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool fillCenter = true;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool isCreatedMaterial;

    [PublicizedFrom(EAccessModifier.Private)]
    public float globalOpacityModifier = 1f;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool originalAspectRatio;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UnityWebRequest www;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool wwwAssigned;

    public UITexture UITexture => uiTexture;

    public Texture Texture
    {
        get
        {
            return texture;
        }
        set
        {
            texture = value;
            isDirty = true;
        }
    }

    public Material Material
    {
        get
        {
            return material;
        }
        set
        {
            material = value;
            isDirty = true;
        }
    }

    public Rect UVRect
    {
        get
        {
            return uvRect;
        }
        set
        {
            uvRect = value;
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

    public Vector4 Border
    {
        get
        {
            return border;
        }
        set
        {
            border = value;
            isDirty = true;
        }
    }

    public Flip Flip
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return flip;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            flip = value;
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
            color = value;
            isDirty = true;
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
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            fillDirection = value;
            isDirty = true;
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
            fillCenter = value;
            isDirty = true;
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

    public bool OriginalAspectRatio
    {
        get
        {
            return originalAspectRatio;
        }
        set
        {
            originalAspectRatio = value;
            isDirty = true;
        }
    }

    public XUiV_Texture(string _id)
        : base(_id)
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UITexture>();
    }

    public void CreateMaterial()
    {
        Material = new Material(Shader.Find("Unlit/Transparent Colored Emissive TextureArray"));
        isCreatedMaterial = true;
    }

    public override void InitView()
    {
        base.InitView();
        uiTexture = uiTransform.gameObject.GetComponent<UITexture>();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        if (isCreatedMaterial)
        {
            UnityEngine.Object.Destroy(material);
            material = null;
            isCreatedMaterial = false;
        }
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
        //IL_0102: Unknown result type (might be due to invalid IL or missing references)
        //IL_0107: Unknown result type (might be due to invalid IL or missing references)
        //IL_0156: Unknown result type (might be due to invalid IL or missing references)
        //IL_0189: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a5: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b1: Unknown result type (might be due to invalid IL or missing references)
        //IL_0055: Unknown result type (might be due to invalid IL or missing references)
        //IL_005b: Invalid comparison between Unknown and I4
        //IL_025a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0068: Unknown result type (might be due to invalid IL or missing references)
        if (!wwwAssigned && !string.IsNullOrEmpty(pathName) && !pathName.StartsWith("@:") && pathName.Contains("@"))
        {
            if (!www.isDone)
            {
                return;
            }

            if ((int)www.result == 1)
            {
                Texture2D texture2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Texture = TextureUtils.CloneTexture(texture2D, _createMipMaps: false, _compress: false, _makeNonReadable: true);
                UnityEngine.Object.DestroyImmediate(texture2D);
            }
            else
            {
                Log.Warning("Retrieving XUiV_Texture file from '" + pathName + "' failed: " + www.error);
            }

            wwwAssigned = true;
        }

        if (!isDirty)
        {
            return;
        }

        ((Behaviour)(object)uiTexture).enabled = texture != null;
        ((UIWidget)uiTexture).mainTexture = texture;
        ((UIWidget)uiTexture).color = color;
        ((UIWidget)uiTexture).keepAspectRatio = keepAspectRatio;
        ((UIWidget)uiTexture).aspectRatio = aspectRatio;
        uiTexture.fixedAspect = originalAspectRatio;
        ((UIWidget)uiTexture).SetDimensions(size.x, size.y);
        ((UIBasicSprite)uiTexture).type = type;
        ((UIWidget)uiTexture).border = border;
        uiTexture.uvRect = uvRect;
        ((UIBasicSprite)uiTexture).flip = flip;
        ((UIBasicSprite)uiTexture).centerType = (AdvancedType)(fillCenter ? 1 : 0);
        ((UIBasicSprite)uiTexture).fillDirection = fillDirection;
        ((UIWidget)uiTexture).material = material;
        if (globalOpacityModifier != 0f && base.xui.ForegroundGlobalOpacity < 1f)
        {
            float a = Mathf.Clamp01(color.a * (globalOpacityModifier * base.xui.ForegroundGlobalOpacity));
            ((UIWidget)uiTexture).color = new Color(color.r, color.g, color.b, a);
        }

        if (!initialized)
        {
            initialized = true;
            ((UIWidget)uiTexture).pivot = pivot;
            ((UIWidget)uiTexture).depth = depth;
            uiTransform.localScale = Vector3.one;
            uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
            if (EventOnHover || EventOnPress || EventOnScroll || EventOnDrag)
            {
                BoxCollider obj = collider;
                obj.center = ((UIWidget)uiTexture).localCenter;
                obj.size = new Vector3(((UIWidget)uiTexture).localSize.x * colliderScale, ((UIWidget)uiTexture).localSize.y * colliderScale, 0f);
            }
        }

        parseAnchors((UIWidget)(object)uiTexture);
        base.UpdateData();
    }

    public void UnloadTexture()
    {
        if (!(Texture == null))
        {
            Texture assetToUnload = Texture;
            ((UIWidget)uiTexture).mainTexture = null;
            Texture = null;
            pathName = null;
            wwwAssigned = false;
            if (www == null)
            {
                Resources.UnloadAsset(assetToUnload);
            }

            www = null;
        }
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        //IL_0320: Unknown result type (might be due to invalid IL or missing references)
        //IL_0325: Unknown result type (might be due to invalid IL or missing references)
        switch (attribute)
        {
            case "texture":
                if (pathName == value)
                {
                    return true;
                }

                pathName = value;
                if (pathName.Length == 0)
                {
                    Texture = null;
                    return true;
                }

                try
                {
                    wwwAssigned = false;
                    string text = ModManager.PatchModPathString(pathName);
                    if (text != null)
                    {
                        fetchWwwTexture("file://" + text);
                    }
                    else if (pathName[0] == '@' && pathName[1] != ':')
                    {
                        string text2 = pathName.Substring(1);
                        if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                        {
                            string text3 = text2.Substring(5);
                            if (text3[0] != '/' && text3[0] != '\\')
                            {
                                text2 = new Uri(((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text3).AbsoluteUri;
                            }
                        }

                        fetchWwwTexture(text2);
                    }
                    else
                    {
                        base.xui.LoadData(pathName, [PublicizedFrom(EAccessModifier.Private)] (Texture o) =>
                        {
                            Texture = o;
                        });
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[XUi] Could not load texture: " + pathName);
                    Log.Exception(e);
                }

                return true;
            case "material":
                base.xui.LoadData(value, [PublicizedFrom(EAccessModifier.Private)] (Material o) =>
                {
                    material = new Material(o);
                });
                return true;
            case "rect_offset":
                {
                    Vector2 vector2 = StringParsers.ParseVector2(value);
                    Rect uVRect2 = uvRect;
                    uVRect2.x = vector2.x;
                    uVRect2.y = vector2.y;
                    UVRect = uVRect2;
                    return true;
                }
            case "rect_size":
                {
                    Vector2 vector = StringParsers.ParseVector2(value);
                    Rect uVRect = uvRect;
                    uVRect.width = vector.x;
                    uVRect.height = vector.y;
                    UVRect = uVRect;
                    return true;
                }
            case "type":
                type = EnumUtils.Parse<Type>(value, _ignoreCase: true);
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
            case "color":
                Color = StringParsers.ParseColor32(value);
                return true;
            case "original_aspect_ratio":
                OriginalAspectRatio = StringParsers.ParseBool(value);
                return true;
            default:
                return base.ParseAttribute(attribute, value, _parent);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void fetchWwwTexture(string _uri)
    {
        _uri = _uri.Replace("#", "%23").Replace("+", "%2B");
        www = UnityWebRequestTexture.GetTexture(_uri);
        www.SendWebRequest();
        ThreadManager.StartCoroutine(waitForWwwData());
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public IEnumerator waitForWwwData()
    {
        while (www != null && !www.isDone)
        {
            yield return null;
        }

        if (www != null)
        {
            isDirty = true;
        }
    }
}
