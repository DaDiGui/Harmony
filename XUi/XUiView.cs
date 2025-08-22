using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class XUiView
{
    [PublicizedFrom(EAccessModifier.Private)]
    public static readonly Dictionary<Type, GameObject> componentTemplates = new Dictionary<Type, GameObject>();

    [PublicizedFrom(EAccessModifier.Private)]
    public static Transform templatesParent;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string id;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Transform uiTransform;

    [PublicizedFrom(EAccessModifier.Protected)]
    public BoxCollider collider;

    [PublicizedFrom(EAccessModifier.Protected)]
    public UIAnchor anchor;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2i size;

    [PublicizedFrom(EAccessModifier.Protected)]
    public XUiSideSizes padding;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool ignoreParentPadding;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2i position;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool positionDirty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float rotation;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool rotationDirty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool isVisible;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool forceHide;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Pivot pivot;

    [PublicizedFrom(EAccessModifier.Protected)]
    public XUi.Alignment alignment;

    [PublicizedFrom(EAccessModifier.Protected)]
    public int depth;

    [PublicizedFrom(EAccessModifier.Protected)]
    public XUiController controller;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string m_value;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool initialized;

    [PublicizedFrom(EAccessModifier.Protected)]
    public AspectRatioSource keepAspectRatio;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float aspectRatio;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorLeft;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorRight;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorBottom;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorTop;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorLeftParsed;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorRightParsed;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorBottomParsed;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorTopParsed;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool isAnchored;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Side anchorSide = (Side)8;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool anchorRunOnce = true;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Vector2 anchorOffset = Vector2.zero;

    [PublicizedFrom(EAccessModifier.Protected)]
    public string anchorContainerName;

    [PublicizedFrom(EAccessModifier.Protected)]
    public Transform rootNode;

    [PublicizedFrom(EAccessModifier.Private)]
    public AudioClip xuiSound;

    [PublicizedFrom(EAccessModifier.Private)]
    public AudioClip xuiHoverSound;

    [PublicizedFrom(EAccessModifier.Private)]
    public string toolTip;

    [PublicizedFrom(EAccessModifier.Private)]
    public string disabledToolTip;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool soundPlayOnClick;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool soundPlayOnHover;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool soundPlayOnOpen;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool soundLoop;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float soundVolume;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdDelay = 0.5f;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventIntervalInitial = 0.5f;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventIntervalFinal = 0.06f;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventIntervalAcceleration = 0.015f;

    public bool EventOnHover;

    public bool EventOnPress;

    public bool EventOnDoubleClick;

    public bool EventOnHeld;

    public bool EventOnScroll;

    public bool EventOnDrag;

    public bool EventOnSelect;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool enabled = true;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool isDirty;

    [PublicizedFrom(EAccessModifier.Protected)]
    public float colliderScale = 1f;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool _isNavigatable = true;

    public bool IsSnappable = true;

    public bool UseSelectionBox = true;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool gamepadSelectableSetFromAttributes;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool navLeftSetFromXML;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool navRightSetFromXML;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool navUpSetFromXML;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool navDownSetFromXML;

    [PublicizedFrom(EAccessModifier.Private)]
    public string navUpTargetString;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiView navUpTarget;

    [PublicizedFrom(EAccessModifier.Private)]
    public string navDownTargetString;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiView navDownTarget;

    [PublicizedFrom(EAccessModifier.Private)]
    public string navLeftTargetString;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiView navLeftTarget;

    [PublicizedFrom(EAccessModifier.Private)]
    public string navRightTargetString;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiView navRightTarget;

    public bool controllerOnlyTooltip;

    [PublicizedFrom(EAccessModifier.Private)]
    public int viewIndex = -1;

    [PublicizedFrom(EAccessModifier.Private)]
    public XUi mXUi;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool wasDragging;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool isPressed;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool isHold;

    [PublicizedFrom(EAccessModifier.Private)]
    public float pressStartTime;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdStartTime;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventIntervalCurrent;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventIntervalChangeSpeed;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventNextTime;

    [PublicizedFrom(EAccessModifier.Private)]
    public float holdEventLastTime;

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool isOver;

    [field: PublicizedFrom(EAccessModifier.Private)]
    public bool RepeatContent { get; set; }

    [field: PublicizedFrom(EAccessModifier.Private)]
    public virtual int RepeatCount { get; set; }

    public string ID => id;

    public bool IsNavigatable
    {
        get
        {
            if (_isNavigatable && Enabled && IsVisible)
            {
                return UiTransform.gameObject.activeInHierarchy;
            }

            return false;
        }
        set
        {
            _isNavigatable = value;
        }
    }

    public XUiView NavUpTarget
    {
        get
        {
            return navUpTarget;
        }
        set
        {
            navUpTarget = value;
            foreach (XUiController child in Controller.Children)
            {
                child.ViewComponent.NavUpTarget = value;
            }
        }
    }

    public XUiView NavDownTarget
    {
        get
        {
            return navDownTarget;
        }
        set
        {
            navDownTarget = value;
            foreach (XUiController child in Controller.Children)
            {
                child.ViewComponent.navDownTarget = value;
            }
        }
    }

    public XUiView NavLeftTarget
    {
        get
        {
            return navLeftTarget;
        }
        set
        {
            navLeftTarget = value;
            foreach (XUiController child in Controller.Children)
            {
                child.ViewComponent.navLeftTarget = value;
            }
        }
    }

    public XUiView NavRightTarget
    {
        get
        {
            return navRightTarget;
        }
        set
        {
            navRightTarget = value;
            foreach (XUiController child in Controller.Children)
            {
                child.ViewComponent.navRightTarget = value;
            }
        }
    }

    public bool IsDirty
    {
        get
        {
            return isDirty;
        }
        set
        {
            isDirty = value;
        }
    }

    public XUiController Controller
    {
        get
        {
            return controller;
        }
        set
        {
            controller = value;
            if (controller.ViewComponent != this)
            {
                controller.ViewComponent = this;
            }
        }
    }

    public Transform UiTransform => uiTransform;

    public Vector2i Size
    {
        get
        {
            return size;
        }
        set
        {
            if (size != value)
            {
                size = value;
                isDirty = true;
            }
        }
    }

    public Vector2i InnerSize => new Vector2i(size.x - padding.SumLeftRight, size.y - padding.SumTopBottom);

    public bool IgnoreParentPadding
    {
        get
        {
            return ignoreParentPadding;
        }
        set
        {
            if (ignoreParentPadding != value)
            {
                ignoreParentPadding = value;
                isDirty = true;
            }
        }
    }

    public Vector2i Position
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
            isDirty = true;
            positionDirty = true;
        }
    }

    public Vector2i PaddedPosition => position + (ignoreParentPadding ? Vector2i.zero : (Controller.Parent?.ViewComponent?.InnerPosition ?? Vector2i.zero));

    public Vector2i InnerPosition => new Vector2i(padding.Left, -padding.Top);

    public float Rotation
    {
        get
        {
            return rotation;
        }
        set
        {
            rotation = value;
            isDirty = true;
            rotationDirty = true;
        }
    }

    public Pivot Pivot
    {
        get
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            return pivot;
        }
        set
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            pivot = value;
            isDirty = true;
        }
    }

    public int Depth
    {
        get
        {
            return depth;
        }
        set
        {
            depth = value;
            isDirty = true;
        }
    }

    public virtual bool IsVisible
    {
        get
        {
            if (isVisible)
            {
                return !ForceHide;
            }

            return false;
        }
        set
        {
            if (isVisible != value && !(ForceHide && value))
            {
                isVisible = value;
                if (uiTransform != null && isVisible != uiTransform.gameObject.activeSelf)
                {
                    uiTransform.gameObject.SetActive(isVisible);
                }

                Controller.IsDirty = true;
                Controller.OnVisibilityChanged(isVisible);
                if (xui.playerUI.CursorController.navigationTarget == this)
                {
                    xui.playerUI.RefreshNavigationTarget();
                }

                isDirty = true;
            }
        }
    }

    public bool ForceHide
    {
        get
        {
            return forceHide;
        }
        set
        {
            forceHide = value;
        }
    }

    public virtual bool Enabled
    {
        get
        {
            return enabled;
        }
        set
        {
            if (value != enabled)
            {
                enabled = value;
                if (!enabled && xui.playerUI.CursorController.navigationTarget == this)
                {
                    xui.playerUI.RefreshNavigationTarget();
                }

                isDirty = true;
            }
        }
    }

    public string Value
    {
        get
        {
            return m_value;
        }
        set
        {
            m_value = value;
        }
    }

    public string ToolTip
    {
        get
        {
            return toolTip;
        }
        set
        {
            if (toolTip != value)
            {
                if (GameManager.Instance.GameIsFocused && enabled && isOver && xui.currentToolTip != null && xui.currentToolTip.ToolTip == toolTip)
                {
                    xui.currentToolTip.ToolTip = value;
                }

                toolTip = value;
            }
        }
    }

    public string DisabledToolTip
    {
        get
        {
            return disabledToolTip;
        }
        set
        {
            if (disabledToolTip != value)
            {
                if (GameManager.Instance.GameIsFocused && enabled && isOver && xui.currentToolTip != null && xui.currentToolTip.ToolTip == disabledToolTip)
                {
                    xui.currentToolTip.ToolTip = value;
                }

                disabledToolTip = value;
            }
        }
    }

    public Vector2 Center => ((Collider)collider).bounds.center;

    public float heightExtent => ((Collider)collider).bounds.size.y / 2f;

    public float widthExtent => ((Collider)collider).bounds.size.x / 2f;

    public Bounds bounds => ((Collider)collider).bounds;

    public Vector2 ScreenPosition => xui.playerUI.uiCamera.cachedCamera.WorldToScreenPoint(uiTransform.position);

    public bool HasCollider => (UnityEngine.Object)(object)collider != null;

    public bool ColliderEnabled
    {
        get
        {
            if (HasCollider)
            {
                return ((Collider)collider).enabled;
            }

            return false;
        }
    }

    public bool SoundPlayOnClick
    {
        get
        {
            return soundPlayOnClick;
        }
        set
        {
            soundPlayOnClick = value;
        }
    }

    public bool SoundPlayOnHover
    {
        get
        {
            return soundPlayOnHover;
        }
        set
        {
            soundPlayOnHover = value;
        }
    }

    public bool HasHoverSound => xuiHoverSound != null;

    public bool HasEvent
    {
        get
        {
            if (!EventOnPress && !EventOnDoubleClick && !EventOnHover && !EventOnHeld && !EventOnDrag && !EventOnScroll && !EventOnSelect)
            {
                return !string.IsNullOrEmpty(ToolTip);
            }

            return true;
        }
    }

    public XUi xui
    {
        get
        {
            return mXUi;
        }
        set
        {
            mXUi = value;
            if (viewIndex < 0)
            {
                viewIndex = mXUi.RegisterXUiView(this);
            }
        }
    }

    public XUiView(string _id)
    {
        //IL_0002: Unknown result type (might be due to invalid IL or missing references)
        id = _id;
    }

    public virtual void UpdateData()
    {
        if (positionDirty)
        {
            uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, uiTransform.localPosition.z);
            positionDirty = false;
        }

        if (rotationDirty)
        {
            uiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
            rotationDirty = false;
        }

        parseNavigationTargets();
    }

    public void TryUpdatePosition()
    {
        if (positionDirty && uiTransform != null)
        {
            uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, uiTransform.localPosition.z);
        }
    }

    public virtual void OnOpen()
    {
        if (xuiSound != null && soundPlayOnOpen)
        {
            Manager.PlayXUiSound(xuiSound, soundVolume);
        }

        isPressed = false;
        isHold = false;
    }

    public virtual void OnClose()
    {
        isPressed = false;
        isHold = false;
        if (!GameManager.Instance.IsQuitting)
        {
            if (xui.playerUI.CursorController.navigationTarget == this)
            {
                controller.Hovered(_isOver: false);
                xui.playerUI.CursorController.SetNavigationTarget(null);
            }

            if (xui.playerUI.CursorController.lockNavigationToView == this)
            {
                xui.playerUI.CursorController.SetNavigationLockView(null);
            }
        }
    }

    public virtual void Cleanup()
    {
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public virtual void CreateComponents(GameObject _go)
    {
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void BuildView()
    {
        Type type = GetType();
        if (componentTemplates.TryGetValue(type, out var value))
        {
            uiTransform = UnityEngine.Object.Instantiate(value).transform;
            return;
        }

        if (templatesParent == null)
        {
            Transform transform = ((Component)(object)xui.playerUI.uiCamera).transform;
            templatesParent = new GameObject("_ViewTemplates").transform;
            templatesParent.parent = transform;
        }

        GameObject gameObject = new GameObject(type.Name);
        gameObject.layer = 12;
        gameObject.transform.parent = templatesParent;
        ((Collider)gameObject.AddComponent<BoxCollider>()).enabled = false;
        ((Behaviour)(object)gameObject.AddComponent<UIAnchor>()).enabled = false;
        UIEventListener.Get(gameObject);
        CreateComponents(gameObject);
        componentTemplates[type] = gameObject;
        uiTransform = UnityEngine.Object.Instantiate(gameObject).transform;
    }

    public virtual void InitView()
    {
        //IL_01f6: Unknown result type (might be due to invalid IL or missing references)
        //IL_0200: Expected O, but got Unknown
        //IL_0200: Unknown result type (might be due to invalid IL or missing references)
        //IL_020a: Expected O, but got Unknown
        //IL_0198: Unknown result type (might be due to invalid IL or missing references)
        //IL_019d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0220: Unknown result type (might be due to invalid IL or missing references)
        //IL_022a: Expected O, but got Unknown
        //IL_022a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0234: Expected O, but got Unknown
        //IL_0258: Unknown result type (might be due to invalid IL or missing references)
        //IL_0262: Expected O, but got Unknown
        //IL_0262: Unknown result type (might be due to invalid IL or missing references)
        //IL_026c: Expected O, but got Unknown
        //IL_0282: Unknown result type (might be due to invalid IL or missing references)
        //IL_028c: Expected O, but got Unknown
        //IL_028c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0296: Expected O, but got Unknown
        //IL_02a4: Unknown result type (might be due to invalid IL or missing references)
        //IL_02ae: Expected O, but got Unknown
        //IL_02ae: Unknown result type (might be due to invalid IL or missing references)
        //IL_02b8: Expected O, but got Unknown
        //IL_02ce: Unknown result type (might be due to invalid IL or missing references)
        //IL_02d8: Expected O, but got Unknown
        //IL_02d8: Unknown result type (might be due to invalid IL or missing references)
        //IL_02e2: Expected O, but got Unknown
        //IL_02f8: Unknown result type (might be due to invalid IL or missing references)
        //IL_0302: Expected O, but got Unknown
        //IL_0302: Unknown result type (might be due to invalid IL or missing references)
        //IL_030c: Expected O, but got Unknown
        //IL_0322: Unknown result type (might be due to invalid IL or missing references)
        //IL_032c: Expected O, but got Unknown
        //IL_032c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0336: Expected O, but got Unknown
        //IL_0344: Unknown result type (might be due to invalid IL or missing references)
        //IL_034e: Expected O, but got Unknown
        //IL_034e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0358: Expected O, but got Unknown
        if (uiTransform == null)
        {
            BuildView();
        }

        uiTransform.name = id;
        collider = uiTransform.gameObject.GetComponent<BoxCollider>();
        anchor = uiTransform.gameObject.GetComponent<UIAnchor>();
        if (controller.Parent?.ViewComponent != null)
        {
            XUiView viewComponent = controller.Parent.ViewComponent;
            uiTransform.parent = viewComponent.UiTransform;
            uiTransform.localScale = Vector3.one;
            uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, 0f);
            uiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
        }
        else
        {
            setRootNode();
        }

        if (HasEvent)
        {
            ((Collider)collider).enabled = true;
            RefreshBoxCollider();
        }

        if (isAnchored)
        {
            ((Behaviour)(object)anchor).enabled = true;
            if (string.IsNullOrEmpty(anchorContainerName))
            {
                anchor.container = uiTransform.parent.gameObject;
            }
            else if (!anchorContainerName.EqualsCaseInsensitive("#none"))
            {
                anchor.container = Controller.Parent.GetChildById(anchorContainerName).ViewComponent.uiTransform.gameObject;
            }

            anchor.side = anchorSide;
            anchor.runOnlyOnce = anchorRunOnce;
            anchor.pixelOffset = anchorOffset;
        }

        if (HasEvent)
        {
            UIEventListener val = UIEventListener.Get(uiTransform.gameObject);
            if (EventOnPress)
            {
                val.onClick = (VoidDelegate)Delegate.Combine((Delegate)(object)val.onClick, (Delegate)new VoidDelegate(OnClick));
            }

            if (EventOnDoubleClick)
            {
                val.onDoubleClick = (VoidDelegate)Delegate.Combine((Delegate)(object)val.onDoubleClick, (Delegate)new VoidDelegate(OnDoubleClick));
            }

            if (EventOnHover || !string.IsNullOrEmpty(ToolTip))
            {
                val.onHover = (BoolDelegate)Delegate.Combine((Delegate)(object)val.onHover, (Delegate)new BoolDelegate(OnHover));
            }

            if (EventOnDrag)
            {
                val.onDrag = (VectorDelegate)Delegate.Combine((Delegate)(object)val.onDrag, (Delegate)new VectorDelegate(OnDrag));
                val.onPress = (BoolDelegate)Delegate.Combine((Delegate)(object)val.onPress, (Delegate)new BoolDelegate(OnPress));
            }

            if (EventOnScroll)
            {
                val.onScroll = (FloatDelegate)Delegate.Combine((Delegate)(object)val.onScroll, (Delegate)new FloatDelegate(OnScroll));
            }

            if (EventOnSelect)
            {
                val.onSelect = (BoolDelegate)Delegate.Combine((Delegate)(object)val.onSelect, (Delegate)new BoolDelegate(OnSelect));
            }

            if (EventOnHeld)
            {
                val.onPress = (BoolDelegate)Delegate.Combine((Delegate)(object)val.onPress, (Delegate)new BoolDelegate(OnHeldPress));
                val.onDragOut = (VoidDelegate)Delegate.Combine((Delegate)(object)val.onDragOut, (Delegate)new VoidDelegate(OnHeldDragOut));
            }
        }

        if (uiTransform.gameObject.activeSelf != isVisible)
        {
            uiTransform.gameObject.SetActive(isVisible);
        }

        if (!gamepadSelectableSetFromAttributes)
        {
            IsNavigatable = (IsSnappable = EventOnPress);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnScroll(GameObject _go, float _delta)
    {
        if (EventOnScroll)
        {
            controller.Scrolled(_delta);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnSelect(GameObject _go, bool _selected)
    {
        if (EventOnSelect && enabled)
        {
            controller.Selected(_selected);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnDrag(GameObject _go, Vector2 _delta)
    {
        if (EventOnDrag && enabled)
        {
            EDragType dragType = (wasDragging ? EDragType.Dragging : EDragType.DragStart);
            wasDragging = true;
            controller.Dragged(_delta, dragType);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnPress(GameObject _go, bool _pressed)
    {
        if (EventOnDrag && enabled && !_pressed && wasDragging)
        {
            wasDragging = false;
            controller.Dragged(default(Vector2), EDragType.DragEnd);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnHeldPress(GameObject _go, bool _pressed)
    {
        if (!EventOnHeld)
        {
            return;
        }

        if (_pressed && !isPressed && enabled)
        {
            isPressed = true;
            isHold = false;
            pressStartTime = Time.unscaledTime;
            holdStartTime = -1f;
        }
        else if (!_pressed)
        {
            isPressed = false;
            bool num = isHold;
            isHold = false;
            if (num)
            {
                controller.Held(EHoldType.HoldEnd, Time.unscaledTime - holdStartTime);
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnHeldDragOut(GameObject _go)
    {
        if (EventOnHeld && isPressed)
        {
            isPressed = false;
            bool num = isHold;
            isHold = false;
            if (num)
            {
                controller.Held(EHoldType.HoldEnd, Time.unscaledTime - holdStartTime);
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnClick(GameObject _go)
    {
        if (EventOnPress && enabled)
        {
            if (xuiSound != null && soundPlayOnClick && UICamera.currentTouchID == -1)
            {
                Manager.PlayXUiSound(xuiSound, soundVolume);
            }

            controller.Pressed(UICamera.currentTouchID);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void OnDoubleClick(GameObject _go)
    {
        if (EventOnDoubleClick && enabled)
        {
            if (xuiSound != null && soundPlayOnClick && UICamera.currentTouchID == -1)
            {
                Manager.PlayXUiSound(xuiSound, soundVolume);
            }

            controller.DoubleClicked(UICamera.currentTouchID);
        }
    }

    public virtual void OnHover(GameObject _go, bool _isOver)
    {
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_0016: Invalid comparison between Unknown and I4
        if ((int)((PlayerActionSet)xui.playerUI.playerInput).LastDeviceClass == 1 && !Cursor.visible)
        {
            _isOver = false;
        }

        bool flag = _isOver && !enabled && !string.IsNullOrEmpty(DisabledToolTip);
        _isOver &= enabled;
        bool flag2 = _isOver && !string.IsNullOrEmpty(ToolTip);
        if (controllerOnlyTooltip && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
        {
            flag = false;
            flag2 = false;
        }

        if (_isOver != isOver && _isOver)
        {
            PlayHoverSound();
        }

        isOver = _isOver;
        if (EventOnHover)
        {
            controller.Hovered(_isOver);
        }

        if (xui.currentToolTip != null)
        {
            if (flag2)
            {
                xui.currentToolTip.ToolTip = ToolTip;
            }
            else if (flag)
            {
                xui.currentToolTip.ToolTip = DisabledToolTip;
            }
            else
            {
                xui.currentToolTip.ToolTip = "";
            }
        }

        xui.playerUI.CursorController.HoverTarget = (_isOver ? this : null);
    }

    public void PlayHoverSound()
    {
        if (xuiHoverSound != null && soundPlayOnHover && enabled && GameManager.Instance.GameIsFocused)
        {
            Manager.PlayXUiSound(xuiHoverSound, soundVolume);
        }
    }

    public void PlayClickSound()
    {
        if (xuiSound != null)
        {
            Manager.PlayXUiSound(xuiSound, soundVolume);
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public virtual void setRootNode()
    {
        if (rootNode == null)
        {
            rootNode = xui.transform.Find("CenterTop").transform;
        }

        uiTransform.parent = rootNode;
        uiTransform.gameObject.layer = 12;
        uiTransform.localScale = Vector3.one;
        uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, 0f);
        uiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
    }

    public virtual void RefreshBoxCollider()
    {
        //IL_0038: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Unknown result type (might be due to invalid IL or missing references)
        //IL_003f: Unknown result type (might be due to invalid IL or missing references)
        //IL_006a: Expected I4, but got Unknown
        if ((UnityEngine.Object)(object)collider != null)
        {
            float num = (float)size.x * 0.5f;
            float num2 = (float)size.y * 0.5f;
            Pivot val = pivot;
            float x;
            float y;
            switch ((int)val)
            {
                case 0:
                    x = num;
                    y = 0f - num2;
                    break;
                case 1:
                    x = 0f;
                    y = 0f - num2;
                    break;
                case 2:
                    x = 0f - num;
                    y = 0f - num2;
                    break;
                case 3:
                    x = num;
                    y = 0f;
                    break;
                case 4:
                    x = 0f;
                    y = 0f;
                    break;
                case 5:
                    x = 0f - num;
                    y = 0f;
                    break;
                case 6:
                    x = num;
                    y = num2;
                    break;
                case 7:
                    x = 0f;
                    y = num2;
                    break;
                case 8:
                    x = 0f - num;
                    y = num2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            collider.center = new Vector3(x, y, 0f);
            collider.size = new Vector3((float)size.x * colliderScale, (float)size.y * colliderScale, 0f);
        }
    }

    public virtual void Update(float _dt)
    {
        if (isOver && UICamera.hoveredObject != UiTransform.gameObject)
        {
            OnHover(UiTransform.gameObject, _isOver: false);
        }

        if (isPressed && enabled)
        {
            float unscaledTime = Time.unscaledTime;
            if (!isHold)
            {
                isHold = unscaledTime - pressStartTime >= holdDelay;
                if (isHold)
                {
                    holdStartTime = unscaledTime;
                    controller.Held(EHoldType.HoldStart, 0f);
                    holdEventNextTime = 0f;
                    holdEventLastTime = unscaledTime;
                    holdEventIntervalChangeSpeed = 0f;
                    holdEventIntervalCurrent = holdEventIntervalInitial;
                }
            }
            else
            {
                controller.Held(EHoldType.Hold, unscaledTime - holdStartTime);
            }

            if (isHold && unscaledTime >= holdEventNextTime)
            {
                holdEventIntervalCurrent = Mathf.SmoothDamp(holdEventIntervalCurrent, holdEventIntervalFinal, ref holdEventIntervalChangeSpeed, holdEventIntervalAcceleration, float.PositiveInfinity, _dt);
                holdEventNextTime = unscaledTime + holdEventIntervalCurrent;
                controller.Held(EHoldType.HoldTimed, unscaledTime - holdStartTime, unscaledTime - holdEventLastTime);
                holdEventLastTime = unscaledTime;
            }
        }

        if (isDirty)
        {
            UpdateData();
            isDirty = false;
        }
    }

    public Vector2 GetClosestPoint(Vector3 point)
    {
        if ((UnityEngine.Object)(object)collider != null)
        {
            return ((Collider)collider).ClosestPointOnBounds(point);
        }

        Log.Warning("XUiView: Attempting to get closest point to a view without a box collider");
        return uiTransform.position;
    }

    public void ClearNavigationTargets()
    {
        XUiView xUiView2 = (NavRightTarget = null);
        XUiView xUiView4 = (NavLeftTarget = xUiView2);
        XUiView xUiView6 = (NavDownTarget = xUiView4);
        NavUpTarget = xUiView6;
    }

    public virtual void SetDefaults(XUiController _parent)
    {
        Pivot = (Pivot)0;
        RepeatContent = false;
        RepeatCount = 1;
        Size = Vector2i.min;
        IsVisible = true;
        Depth = (_parent?.ViewComponent?.Depth).GetValueOrDefault();
        ToolTip = "";
        soundLoop = false;
        soundPlayOnClick = true;
        soundPlayOnOpen = false;
        soundPlayOnHover = true;
        soundVolume = 1f;
    }

    public virtual void SetPostParsingDefaults(XUiController _parent)
    {
        XUiView xUiView = _parent?.ViewComponent;
        Vector2i vector2i = Size;
        if (vector2i.x == int.MinValue)
        {
            vector2i.x = ((!ignoreParentPadding) ? (xUiView?.InnerSize.x ?? 0) : (xUiView?.Size.x ?? 0));
        }

        if (vector2i.y == int.MinValue)
        {
            vector2i.y = ((!ignoreParentPadding) ? (xUiView?.InnerSize.y ?? 0) : (xUiView?.Size.y ?? 0));
        }

        Size = vector2i;
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public void parseNavigationTargets()
    {
        if (navUpSetFromXML)
        {
            NavUpTarget = findView(navUpTargetString);
        }

        if (navDownSetFromXML)
        {
            NavDownTarget = findView(navDownTargetString);
        }

        if (navLeftSetFromXML)
        {
            NavLeftTarget = findView(navLeftTargetString);
        }

        if (navRightSetFromXML)
        {
            NavRightTarget = findView(navRightTargetString);
        }

        [PublicizedFrom(EAccessModifier.Private)]
        XUiView findView(string _name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                return null;
            }

            XUiView xUiView = findHierarchyClosestView(_name);
            if (xUiView == null)
            {
                xUiView = controller.WindowGroup.Controller.GetChildById(_name)?.ViewComponent;
                if (xUiView == null)
                {
                    throw new ArgumentException("Invalid navigation target, view component with name '" + _name + "' not found.\nOn: " + controller.GetXuiHierarchy());
                }
            }

            return xUiView;
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public void parseAnchors(UIWidget _target, bool _fixSize = true)
    {
        if ((UnityEngine.Object)(object)_target == null)
        {
            return;
        }

        if (parseAnchorString(anchorLeft, ref anchorLeftParsed, ((UIRect)_target).leftAnchor, _target) | parseAnchorString(anchorRight, ref anchorRightParsed, ((UIRect)_target).rightAnchor, _target) | parseAnchorString(anchorBottom, ref anchorBottomParsed, ((UIRect)_target).bottomAnchor, _target) | parseAnchorString(anchorTop, ref anchorTopParsed, ((UIRect)_target).topAnchor, _target))
        {
            isDirty = true;
            ((UIRect)_target).ResetAnchors();
        }

        if (_fixSize)
        {
            if ((!((UIRect)_target).leftAnchor.target || !((UIRect)_target).rightAnchor.target) && _target.width != size.x)
            {
                _target.width = size.x;
            }

            if ((!((UIRect)_target).bottomAnchor.target || !((UIRect)_target).topAnchor.target) && _target.height != size.y)
            {
                _target.height = size.y;
            }
        }

        ThreadManager.StartCoroutine(markAsChangedLater(_target));
        [PublicizedFrom(EAccessModifier.Internal)]
        static IEnumerator markAsChangedLater(UIWidget _target)
        {
            yield return new WaitForEndOfFrame();
            _target.MarkAsChanged();
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public bool parseAnchorString(string _anchorString, ref string _parsedString, AnchorPoint _anchor, UIWidget _target)
    {
        //IL_015d: Unknown result type (might be due to invalid IL or missing references)
        if (string.IsNullOrEmpty(_anchorString))
        {
            return false;
        }

        if (_anchorString == _parsedString && _anchor.target != null)
        {
            return false;
        }

        _parsedString = _anchorString;
        int num = _anchorString.IndexOf(',');
        if (num < 0)
        {
            throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
        }

        string text = _anchorString.Substring(0, num);
        int num2 = _anchorString.IndexOf(',', num + 1);
        if (num2 < 0)
        {
            throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
        }

        float relative = StringParsers.ParseFloat(_anchorString, num + 1, num2 - 1);
        int absolute = StringParsers.ParseSInt32(_anchorString, num2 + 1);
        if (text.Length == 0)
        {
            throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
        }

        if (text.EqualsCaseInsensitive("#parent"))
        {
            _anchor.target = uiTransform.parent;
        }
        else if (text.EqualsCaseInsensitive("#cam"))
        {
            UICamera componentInParent = uiTransform.gameObject.GetComponentInParent<UICamera>();
            if ((UnityEngine.Object)(object)componentInParent == null)
            {
                throw new Exception("UICamera not found");
            }

            _anchor.target = ((Component)(object)componentInParent).transform;
        }
        else if (text[0] == '#')
        {
            string text2 = text.Substring(1);
            if (!EnumUtils.TryParse<Side>(text2, out Side _result, _ignoreCase: true))
            {
                throw new ArgumentException("Invalid anchor side name '" + text2 + "', expected any of '\tBottomLeft,Left,TopLeft,Top,TopRight,Right,BottomRight,Bottom,Center'");
            }

            _anchor.target = ((Component)(object)xui.GetAnchor(_result)).transform;
        }
        else
        {
            XUiView xUiView = findHierarchyClosestView(text);
            if (xUiView == null)
            {
                throw new ArgumentException("Invalid anchor string '" + _anchorString + "', view component with name '" + text + "' not found.\nOn: " + controller.GetXuiHierarchy());
            }

            _anchor.target = xUiView.UiTransform;
            if (xUiView is XUiV_Grid xUiV_Grid)
            {
                xUiV_Grid.OnSizeChangedSimple += ((UIRect)_target).UpdateAnchors;
            }
        }

        _anchor.relative = relative;
        _anchor.absolute = absolute;
        return true;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiView findHierarchyClosestView(string _name)
    {
        XUiController xUiController = Controller.WindowGroup.Controller;
        XUiController parent = Controller.Parent;
        do
        {
            XUiController childById = parent.GetChildById(_name);
            if (childById != null)
            {
                return childById.ViewComponent;
            }

            parent = parent.Parent;
        }
        while (parent != xUiController);
        return null;
    }

    public bool ParseAttributeViewAndController(string _attribute, string _value, XUiController _parent, bool _allowBindingCreation = true)
    {
        if (_value.Contains("{"))
        {
            if (_allowBindingCreation)
            {
                new BindingInfo(this, _attribute, _value);
                return true;
            }

            if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
            {
                Log.Warning("[XUi] Refreshed binding contained '{': " + _attribute + "='" + _value + "' on " + id);
            }
        }

        if (ParseAttribute(_attribute, _value, _parent))
        {
            return true;
        }

        if (Controller != null)
        {
            if (!Controller.ParseAttribute(_attribute, _value, _parent))
            {
                Controller.CustomAttributes[_attribute] = _value;
            }

            return true;
        }

        return false;
    }

    public virtual bool ParseAttribute(string _attribute, string _value, XUiController _parent)
    {
        //IL_0c30: Unknown result type (might be due to invalid IL or missing references)
        //IL_0c35: Unknown result type (might be due to invalid IL or missing references)
        //IL_0c9b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0ca0: Unknown result type (might be due to invalid IL or missing references)
        //IL_0afe: Unknown result type (might be due to invalid IL or missing references)
        switch (_attribute)
        {
            case "repeat_content":
                RepeatContent = StringParsers.ParseBool(_value);
                break;
            case "repeat_count":
                RepeatCount = StringParsers.ParseSInt32(_value);
                break;
            case "size":
                Size = StringParsers.ParseVector2i(_value);
                break;
            case "width":
                {
                    int y = Size.y;
                    int result3;
                    if (_value.Contains("%"))
                    {
                        _value = _value.Replace("%", "");
                        if (int.TryParse(_value, out result3))
                        {
                            result3 = (int)((float)result3 / 100f) * _parent.ViewComponent.Size.x;
                        }
                    }
                    else
                    {
                        int.TryParse(_value, out result3);
                    }

                    Size = new Vector2i(result3, y);
                    break;
                }
            case "height":
                {
                    int x = Size.x;
                    int result;
                    if (_value.Contains("%"))
                    {
                        _value = _value.Replace("%", "");
                        if (int.TryParse(_value, out result))
                        {
                            result = (int)((float)result / 100f) * _parent.ViewComponent.Size.y;
                        }
                    }
                    else
                    {
                        int.TryParse(_value, out result);
                    }

                    Size = new Vector2i(x, result);
                    break;
                }
            case "pos":
            case "position":
                Position = StringParsers.ParseVector2i(_value);
                break;
            case "padding_left":
                padding = padding.SetLeft(StringParsers.ParseSInt32(_value));
                break;
            case "padding_right":
                padding = padding.SetRight(StringParsers.ParseSInt32(_value));
                break;
            case "padding_top":
                padding = padding.SetTop(StringParsers.ParseSInt32(_value));
                break;
            case "padding_bottom":
                padding = padding.SetBottom(StringParsers.ParseSInt32(_value));
                break;
            case "padding":
                XUiSideSizes.TryParse(_value, out padding, _attribute);
                break;
            case "ignoreparentpadding":
                IgnoreParentPadding = StringParsers.ParseBool(_value);
                break;
            case "rotation":
                Rotation = int.Parse(_value);
                break;
            case "visible":
                IsVisible = StringParsers.ParseBool(_value);
                break;
            case "force_hide":
                ForceHide = StringParsers.ParseBool(_value);
                break;
            case "pivot":
                Pivot = EnumUtils.Parse<Pivot>(_value, _ignoreCase: true);
                break;
            case "on_hover":
                EventOnHover = StringParsers.ParseBool(_value);
                break;
            case "on_press":
                EventOnPress = StringParsers.ParseBool(_value);
                break;
            case "on_held":
                EventOnHeld = StringParsers.ParseBool(_value);
                break;
            case "hold_delay":
                holdDelay = StringParsers.ParseFloat(_value);
                break;
            case "hold_timed_initial_interval":
                holdEventIntervalInitial = StringParsers.ParseFloat(_value);
                break;
            case "hold_timed_final_interval":
                holdEventIntervalFinal = StringParsers.ParseFloat(_value);
                break;
            case "hold_timed_step_acceleration":
                holdEventIntervalAcceleration = StringParsers.ParseFloat(_value);
                break;
            case "on_doubleclick":
                EventOnDoubleClick = StringParsers.ParseBool(_value);
                break;
            case "on_scroll":
                EventOnScroll = StringParsers.ParseBool(_value);
                break;
            case "on_select":
                EventOnSelect = StringParsers.ParseBool(_value);
                break;
            case "on_drag":
                EventOnDrag = StringParsers.ParseBool(_value);
                break;
            case "depth":
                {
                    int.TryParse(_value, out var result2);
                    Depth = result2 + (_parent.ViewComponent?.Depth ?? 0);
                    break;
                }
            case "value":
                Value = _value;
                break;
            case "keep_aspect_ratio":
                keepAspectRatio = EnumUtils.Parse<AspectRatioSource>(_value, _ignoreCase: false);
                return true;
            case "aspect_ratio":
                aspectRatio = StringParsers.ParseFloat(_value);
                return true;
            case "anchor_left":
                anchorLeft = _value;
                isDirty = true;
                return true;
            case "anchor_right":
                anchorRight = _value;
                isDirty = true;
                return true;
            case "anchor_bottom":
                anchorBottom = _value;
                isDirty = true;
                return true;
            case "anchor_top":
                anchorTop = _value;
                isDirty = true;
                return true;
            case "anchor_side":
                isAnchored = true;
                anchorSide = EnumUtils.Parse<Side>(_value, _ignoreCase: true);
                break;
            case "anchor_run_once":
                anchorRunOnce = StringParsers.ParseBool(_value);
                break;
            case "anchor_offset":
                anchorOffset = StringParsers.ParseVector2(_value);
                break;
            case "anchor_parent_id":
                anchorContainerName = _value;
                break;
            case "name":
                id = _value;
                break;
            case "sound":
                xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
                {
                    xuiSound = _o;
                });
                break;
            case "sound_play_on_hover":
                xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
                {
                    xuiHoverSound = _o;
                });
                break;
            case "sound_play_on_press":
                soundPlayOnClick = StringParsers.ParseBool(_value);
                break;
            case "sound_play_on_open":
                soundPlayOnOpen = StringParsers.ParseBool(_value);
                break;
            case "sound_volume":
                soundVolume = StringParsers.ParseFloat(_value);
                break;
            case "tooltip":
                ToolTip = _value;
                break;
            case "tooltip_key":
                ToolTip = Localization.Get(_value);
                break;
            case "disabled_tooltip":
                DisabledToolTip = _value;
                break;
            case "disabled_tooltip_key":
                DisabledToolTip = Localization.Get(_value);
                break;
            case "snap":
                IsSnappable = StringParsers.ParseBool(_value);
                break;
            case "collider_scale":
                colliderScale = StringParsers.ParseFloat(_value);
                break;
            case "enabled":
                Enabled = StringParsers.ParseBool(_value);
                break;
            case "use_selection_box":
                UseSelectionBox = StringParsers.ParseBool(_value);
                break;
            case "gamepad_selectable":
                IsNavigatable = StringParsers.ParseBool(_value);
                gamepadSelectableSetFromAttributes = true;
                break;
            case "nav_up":
                if (!string.IsNullOrEmpty(_value))
                {
                    navUpTargetString = _value;
                    navUpSetFromXML = true;
                    isDirty = true;
                }

                break;
            case "nav_down":
                if (!string.IsNullOrEmpty(_value))
                {
                    navDownTargetString = _value;
                    navDownSetFromXML = true;
                    isDirty = true;
                }

                break;
            case "nav_left":
                if (!string.IsNullOrEmpty(_value))
                {
                    navLeftTargetString = _value;
                    navLeftSetFromXML = true;
                    isDirty = true;
                }

                break;
            case "nav_right":
                if (!string.IsNullOrEmpty(_value))
                {
                    navRightTargetString = _value;
                    navRightSetFromXML = true;
                    isDirty = true;
                }

                break;
            default:
                return false;
            case "hold_timed_step_divider":
                break;
        }

        return true;
    }

    public virtual void setRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
    {
    }
}
