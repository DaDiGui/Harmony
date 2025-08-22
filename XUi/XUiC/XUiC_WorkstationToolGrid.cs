using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationToolGrid : XUiC_WorkstationGrid
{
    [PublicizedFrom(EAccessModifier.Private)]
    public bool isLocked;

    [PublicizedFrom(EAccessModifier.Private)]
    public string requiredTools = "";

    [PublicizedFrom(EAccessModifier.Private)]
    public bool requiredToolsOnly;

    public event XuiEvent_WorkstationItemsChanged OnWorkstationToolsChanged;

    public override void Init()
    {
        base.Init();
        string[] array = requiredTools.Split(',', StringSplitOptions.None);
        for (int i = 0; i < itemControllers.Length; i++)
        {
            if (i < array.Length)
            {
                ((XUiC_RequiredItemStack)itemControllers[i]).RequiredItemClass = ItemClass.GetItemClass(array[i]);
                ((XUiC_RequiredItemStack)itemControllers[i]).RequiredItemOnly = requiredToolsOnly;
            }
            else
            {
                ((XUiC_RequiredItemStack)itemControllers[i]).RequiredItemClass = null;
                ((XUiC_RequiredItemStack)itemControllers[i]).RequiredItemOnly = false;
            }
        }
    }

    public override bool HasRequirement(Recipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }

        if (recipe.craftingToolType == 0)
        {
            return true;
        }

        ItemStack[] slots = GetSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemValue.type == recipe.craftingToolType)
            {
                return true;
            }
        }

        return false;
    }

    public void SetToolLocks(bool locked)
    {
        if (locked != isLocked)
        {
            isLocked = locked;
            for (int i = 0; i < itemControllers.Length; i++)
            {
                itemControllers[i].ToolLock = locked;
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void UpdateBackend(ItemStack[] stackList)
    {
        base.UpdateBackend(stackList);
        workstationData.SetToolStacks(stackList);
        windowGroup.Controller.SetAllChildrenDirty();
    }

    public override bool ParseAttribute(string name, string value, XUiController _parent)
    {
        bool flag = base.ParseAttribute(name, value, _parent);
        if (!flag)
        {
            if (!(name == "required_tools"))
            {
                if (!(name == "required_tools_only"))
                {
                    return false;
                }

                requiredToolsOnly = StringParsers.ParseBool(value);
            }
            else
            {
                requiredTools = value;
            }

            return true;
        }

        return flag;
    }

    public bool TryAddTool(ItemClass newItemClass, ItemStack newItemStack)
    {
        if (!requiredToolsOnly || isLocked)
        {
            return false;
        }

        for (int i = 0; i < itemControllers.Length; i++)
        {
            XUiC_RequiredItemStack xUiC_RequiredItemStack = (XUiC_RequiredItemStack)itemControllers[i];
            if (xUiC_RequiredItemStack.RequiredItemClass == newItemClass && xUiC_RequiredItemStack.ItemStack.IsEmpty())
            {
                xUiC_RequiredItemStack.ItemStack = newItemStack.Clone();
                return true;
            }
        }

        return false;
    }

    public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
    {
        base.HandleSlotChangedEvent(slotNumber, stack);
        if (this.OnWorkstationToolsChanged != null)
        {
            this.OnWorkstationToolsChanged();
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        base.xui.currentWorkstationToolGrid = this;
    }

    public override void OnClose()
    {
        base.OnClose();
        base.xui.currentWorkstationToolGrid = null;
    }
}
