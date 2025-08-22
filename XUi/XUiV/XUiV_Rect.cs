using System;
using UnityEngine;

public class XUiV_Rect : XUiView
{
    [PublicizedFrom(EAccessModifier.Private)]
    public bool createUiWidget;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UIWidget widget;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool disableFallthrough;

    public bool DisableFallthrough
    {
        get
        {
            return disableFallthrough;
        }
        set
        {
            disableFallthrough = value;
        }
    }

    public XUiV_Rect(string _id)
        : base(_id)
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UIWidget>();
    }

    public override void InitView()
    {
        //IL_0043: Unknown result type (might be due to invalid IL or missing references)
        //IL_004d: Expected O, but got Unknown
        //IL_004d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0057: Expected O, but got Unknown
        base.InitView();
        widget = uiTransform.gameObject.GetComponent<UIWidget>();
        if (createUiWidget)
        {
            ((Behaviour)(object)widget).enabled = true;
            UIWidget obj = widget;
            obj.onChange = (OnDimensionsChanged)Delegate.Combine((Delegate)(object)obj.onChange, (Delegate)(OnDimensionsChanged)([PublicizedFrom(EAccessModifier.Private)] () =>
            {
                isDirty = true;
            }));
        }
        else
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)(object)widget);
            widget = null;
        }

        UpdateData();
    }

    public override void UpdateData()
    {
        //IL_0091: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b3: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b8: Unknown result type (might be due to invalid IL or missing references)
        //IL_0024: Unknown result type (might be due to invalid IL or missing references)
        if (!initialized)
        {
            initialized = true;
            if ((UnityEngine.Object)(object)widget != null)
            {
                widget.pivot = pivot;
                widget.depth = depth;
                uiTransform.localScale = Vector3.one;
                uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
            }
        }

        if ((UnityEngine.Object)(object)widget != null)
        {
            widget.pivot = pivot;
            widget.depth = depth;
            widget.keepAspectRatio = keepAspectRatio;
            widget.aspectRatio = aspectRatio;
            widget.autoResizeBoxCollider = true;
            parseAnchors(widget);
        }

        base.UpdateData();
    }

    public override void RefreshBoxCollider()
    {
        base.RefreshBoxCollider();
        if (disableFallthrough)
        {
            BoxCollider val = collider;
            if ((UnityEngine.Object)(object)val != null)
            {
                int num = 100;
                Vector3 center = val.center;
                center.z = num;
                val.center = center;
            }
        }
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        bool flag = base.ParseAttribute(attribute, value, _parent);
        if (!flag)
        {
            if (!(attribute == "disablefallthrough"))
            {
                if (!(attribute == "createuiwidget"))
                {
                    return false;
                }

                createUiWidget = StringParsers.ParseBool(value);
            }
            else
            {
                DisableFallthrough = StringParsers.ParseBool(value);
            }

            return true;
        }

        return flag;
    }
}
