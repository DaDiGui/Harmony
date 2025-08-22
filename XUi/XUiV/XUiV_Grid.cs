using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class XUiV_Grid : XUiView
{
    [PublicizedFrom(EAccessModifier.Private)]
    public int columns;

    [PublicizedFrom(EAccessModifier.Private)]
    public int rows;

    [PublicizedFrom(EAccessModifier.Private)]
    public UIWidget widget;

    [CompilerGenerated]
    private OnSizeChanged m_OnSizeChanged;

    [field: PublicizedFrom(EAccessModifier.Private)]
    public UIGrid Grid { get; set; }

    [field: PublicizedFrom(EAccessModifier.Private)]
    public Arrangement Arrangement { get; set; }

    public int Columns
    {
        get
        {
            return columns;
        }
        set
        {
            //IL_0009: Unknown result type (might be due to invalid IL or missing references)
            if (initialized && (int)Arrangement == 0 && Grid.maxPerLine != value)
            {
                Grid.maxPerLine = value;
                Grid.Reposition();
            }

            columns = value;
            isDirty = true;
        }
    }

    public int Rows
    {
        get
        {
            return rows;
        }
        set
        {
            //IL_0009: Unknown result type (might be due to invalid IL or missing references)
            if (initialized && (int)Arrangement != 0 && Grid.maxPerLine != value)
            {
                Grid.maxPerLine = value;
                Grid.Reposition();
            }

            rows = value;
            isDirty = true;
        }
    }

    public override int RepeatCount
    {
        get
        {
            return Columns * Rows;
        }
        set
        {
        }
    }

    [field: PublicizedFrom(EAccessModifier.Private)]
    public int CellWidth { get; set; }

    [field: PublicizedFrom(EAccessModifier.Private)]
    public int CellHeight { get; set; }

    [field: PublicizedFrom(EAccessModifier.Private)]
    public bool HideInactive { get; set; }

    public event OnSizeChanged OnSizeChanged
    {
        [CompilerGenerated]
        add
        {
            //IL_0010: Unknown result type (might be due to invalid IL or missing references)
            //IL_0016: Expected O, but got Unknown
            OnSizeChanged val = this.m_OnSizeChanged;
            OnSizeChanged val2;
            do
            {
                val2 = val;
                OnSizeChanged value2 = (OnSizeChanged)Delegate.Combine((Delegate)(object)val2, (Delegate)(object)value);
                val = Interlocked.CompareExchange(ref this.m_OnSizeChanged, value2, val2);
            }
            while (val != val2);
        }
        [CompilerGenerated]
        remove
        {
            //IL_0010: Unknown result type (might be due to invalid IL or missing references)
            //IL_0016: Expected O, but got Unknown
            OnSizeChanged val = this.m_OnSizeChanged;
            OnSizeChanged val2;
            do
            {
                val2 = val;
                OnSizeChanged value2 = (OnSizeChanged)Delegate.Remove((Delegate)(object)val2, (Delegate)(object)value);
                val = Interlocked.CompareExchange(ref this.m_OnSizeChanged, value2, val2);
            }
            while (val != val2);
        }
    }

    public event Action OnSizeChangedSimple;

    public XUiV_Grid(string _id)
        : base(_id)
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void CreateComponents(GameObject _go)
    {
        _go.AddComponent<UIWidget>();
        _go.AddComponent<UIGrid>();
    }

    public override void InitView()
    {
        //IL_0023: Unknown result type (might be due to invalid IL or missing references)
        //IL_007a: Unknown result type (might be due to invalid IL or missing references)
        //IL_007f: Unknown result type (might be due to invalid IL or missing references)
        //IL_008b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0090: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a2: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ac: Expected O, but got Unknown
        //IL_00ad: Unknown result type (might be due to invalid IL or missing references)
        base.InitView();
        widget = uiTransform.gameObject.GetComponent<UIWidget>();
        widget.pivot = pivot;
        widget.depth = base.Depth + 2;
        widget.autoResizeBoxCollider = true;
        Grid = uiTransform.gameObject.GetComponent<UIGrid>();
        Grid.hideInactive = HideInactive;
        Grid.arrangement = Arrangement;
        Grid.pivot = pivot;
        Grid.onSizeChanged = new OnSizeChanged(OnGridSizeChanged);
        if ((int)Arrangement == 0)
        {
            Grid.maxPerLine = Columns;
        }
        else
        {
            Grid.maxPerLine = Rows;
        }

        Grid.cellWidth = CellWidth;
        Grid.cellHeight = CellHeight;
        uiTransform.localScale = Vector3.one;
        uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
        initialized = true;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnGridSizeChanged(Vector2Int _cells, Vector2 _size)
    {
        widget.width = Mathf.RoundToInt(_size.x);
        widget.height = Mathf.RoundToInt(_size.y);
        OnSizeChanged onSizeChanged = this.OnSizeChanged;
        if (onSizeChanged != null)
        {
            onSizeChanged.Invoke(_cells, _size);
        }

        this.OnSizeChangedSimple?.Invoke();
    }

    public override void Update(float _dt)
    {
        //IL_0009: Unknown result type (might be due to invalid IL or missing references)
        if (isDirty)
        {
            if ((int)Arrangement == 0)
            {
                Grid.maxPerLine = Columns;
            }
            else
            {
                Grid.maxPerLine = Rows;
            }
        }

        Grid.cellWidth = CellWidth;
        Grid.cellHeight = CellHeight;
        Grid.repositionNow = true;
        base.Update(_dt);
    }

    public override void SetDefaults(XUiController _parent)
    {
        base.SetDefaults(_parent);
        Columns = 0;
        Rows = 0;
        CellWidth = 0;
        CellHeight = 0;
        Arrangement = (Arrangement)0;
        HideInactive = true;
    }

    public override bool ParseAttribute(string attribute, string value, XUiController _parent)
    {
        //IL_009b: Unknown result type (might be due to invalid IL or missing references)
        bool flag = base.ParseAttribute(attribute, value, _parent);
        if (!flag)
        {
            switch (attribute)
            {
                case "cols":
                    Columns = int.Parse(value);
                    break;
                case "rows":
                    Rows = int.Parse(value);
                    break;
                case "cell_width":
                    CellWidth = int.Parse(value);
                    break;
                case "cell_height":
                    CellHeight = int.Parse(value);
                    break;
                case "arrangement":
                    Arrangement = EnumUtils.Parse<Arrangement>(value, _ignoreCase: true);
                    break;
                case "hide_inactive":
                    HideInactive = StringParsers.ParseBool(value);
                    break;
                default:
                    return false;
            }

            return true;
        }

        return flag;
    }

    public override void setRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
    {
        //IL_0009: Unknown result type (might be due to invalid IL or missing references)
        base.setRepeatContentTemplateParams(_templateParams, _curRepeatNum);
        int num;
        int num2;
        if ((int)Arrangement == 0)
        {
            num = _curRepeatNum % Columns;
            num2 = _curRepeatNum / Columns;
        }
        else
        {
            num = _curRepeatNum / Rows;
            num2 = _curRepeatNum % Rows;
        }

        _templateParams["repeat_col"] = num;
        _templateParams["repeat_row"] = num2;
    }
}
