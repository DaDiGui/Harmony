using System;
using System.Collections.Generic;
using System.Linq;
using ShinyScreenSpaceRaytracedReflections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public static class SDCSUtils
{
    public class SlotData
    {
        public enum HairMaskTypes
        {
            Full,
            Hat,
            Bald,
            None
        }

        public string PrefabName;

        public string PartName;

        public string BaseToTurnOff;

        public float CullDistance = 0.32f;

        public HairMaskTypes HairMaskType;

        public HairMaskTypes FacialHairMaskType;
    }

    public class TransformCatalog : Dictionary<string, Transform>
    {
        public TransformCatalog(Transform _transform)
        {
            AddRecursive(_transform);
        }

        [PublicizedFrom(EAccessModifier.Private)]
        public void AddRecursive(Transform _transform)
        {
            string name = _transform.name;
            if (ContainsKey(name))
            {
                base[name] = _transform;
            }
            else
            {
                Add(name, _transform);
            }

            foreach (Transform item in _transform)
            {
                AddRecursive(item);
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static readonly List<Cloth> tempCloths = new List<Cloth>();

    [PublicizedFrom(EAccessModifier.Private)]
    public static readonly List<Material> tempMats = new List<Material>();

    [PublicizedFrom(EAccessModifier.Private)]
    public static readonly List<SkinnedMeshRenderer> tempSMRs = new List<SkinnedMeshRenderer>();

    [PublicizedFrom(EAccessModifier.Private)]
    public static readonly string[] shortHairNames = new string[7] { "buzzcut", "comb_over", "cornrows", "flattop_fro", "mohawk", "pixie_cut", "small_fro" };

    [PublicizedFrom(EAccessModifier.Private)]
    public const string ORIGIN = "Origin";

    [PublicizedFrom(EAccessModifier.Private)]
    public const string RIGCON = "RigConstraints";

    [PublicizedFrom(EAccessModifier.Private)]
    public const string IKRIG = "IKRig";

    public const string HEAD = "head";

    public const string EYES = "eyes";

    public const string BEARD = "beard";

    public const string HAIR = "hair";

    public const string BODY = "body";

    public const string HANDS = "hands";

    public const string FEET = "feet";

    public const string HELMET = "helmet";

    public const string TORSO = "torso";

    public const string GLOVES = "gloves";

    public const string BOOTS = "boots";

    public const string SEX_MARKER = "{sex}";

    public const string RACE_MARKER = "{race}";

    public const string VARIANT_MARKER = "{variant}";

    public const string HAIR_MARKER = "{hair}";

    [PublicizedFrom(EAccessModifier.Private)]
    public static Archetype tmpArchetype;

    [PublicizedFrom(EAccessModifier.Private)]
    public static string[] baseParts = new string[4] { "head", "body", "hands", "feet" };

    [PublicizedFrom(EAccessModifier.Private)]
    public static string[] ignoredParts = new string[0];

    [PublicizedFrom(EAccessModifier.Private)]
    public static string[] basePartsFP = new string[2] { "body", "hands" };

    [PublicizedFrom(EAccessModifier.Private)]
    public static string[] ignoredPartsFP = new string[4] { "head", "helmet", "feet", "boots" };

    public static string baseBodyLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/Common/Meshes/player" + tmpArchetype.Sex + ".fbx";
        }
    }

    public static string baseHeadLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/Heads/" + tmpArchetype.Race + "/" + tmpArchetype.Variant.ToString("00") + "/Meshes/player" + tmpArchetype.Sex + tmpArchetype.Race + tmpArchetype.Variant.ToString("00") + ".fbx";
        }
    }

    public static string baseHairLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/Hair/" + tmpArchetype.Hair + "/HairMorphMatrix/" + tmpArchetype.Race + tmpArchetype.Variant.ToString("00");
        }
    }

    public static string baseMustacheLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/FacialHair/Mustache/" + tmpArchetype.MustacheName + "/HairMorphMatrix/" + tmpArchetype.Race + tmpArchetype.Variant.ToString("00");
        }
    }

    public static string baseChopsLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/FacialHair/Chops/" + tmpArchetype.ChopsName + "/HairMorphMatrix/" + tmpArchetype.Race + tmpArchetype.Variant.ToString("00");
        }
    }

    public static string baseBeardLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/FacialHair/Beard/" + tmpArchetype.BeardName + "/HairMorphMatrix/" + tmpArchetype.Race + tmpArchetype.Variant.ToString("00");
        }
    }

    public static string baseHairColorLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/Common/HairColorSwatches";
        }
    }

    public static string baseEyeColorMatLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/Common/Eyes/Materials/" + tmpArchetype.EyeColorName + ".mat";
        }
    }

    public static string baseBodyMatLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/Heads/" + tmpArchetype.Race + "/" + tmpArchetype.Variant.ToString("00") + "/Materials/player" + tmpArchetype.Sex + tmpArchetype.Race + tmpArchetype.Variant.ToString("00") + "_Body.mat";
        }
    }

    public static string baseHeadMatLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/Heads/" + tmpArchetype.Race + "/" + tmpArchetype.Variant.ToString("00") + "/Materials/player" + tmpArchetype.Sex + tmpArchetype.Race + tmpArchetype.Variant.ToString("00") + "_Head.mat";
        }
    }

    public static string baseHandsMatLoc
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/" + tmpArchetype.Sex + "/Heads/" + tmpArchetype.Race + "/" + tmpArchetype.Variant.ToString("00") + "/Materials/player" + tmpArchetype.Sex + tmpArchetype.Race + tmpArchetype.Variant.ToString("00") + "_Hand.mat";
        }
    }

    public static string baseRigPrefab
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/Common/BaseRigs/baseRigPrefab.prefab";
        }
    }

    public static string baseRigFPPrefab
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return "@:Entities/Player/Common/BaseRigs/baseRigFPPrefab.prefab";
        }
    }

    public static RuntimeAnimatorController UIAnimController
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return DataLoader.LoadAsset<RuntimeAnimatorController>("@:Entities/Player/Common/AnimControllers/MenuSDCSController.controller", _ignoreDlcEntitlements: false);
        }
    }

    public static RuntimeAnimatorController TPAnimController
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return DataLoader.LoadAsset<RuntimeAnimatorController>("@:Entities/Player/Common/AnimControllers/3PPlayer" + tmpArchetype.Sex + "Controller" + (tmpArchetype.IsMale ? ".controller" : ".overrideController"), _ignoreDlcEntitlements: false);
        }
    }

    public static RuntimeAnimatorController FPAnimController
    {
        [PublicizedFrom(EAccessModifier.Private)]
        get
        {
            return DataLoader.LoadAsset<RuntimeAnimatorController>("@:Entities/Player/Common/AnimControllers/FPPlayerController.controller", _ignoreDlcEntitlements: false);
        }
    }

    public static void Stitch(GameObject sourceObj, GameObject parentObj, TransformCatalog boneCatalog, EModelSDCS emodel = null, bool isFPV = false, float cullDist = 0f, bool isUI = false, Material eyeMat = null, bool isGear = false)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(sourceObj, parentObj.transform);
        gameObject.name = sourceObj.name;
        gameObject.GetComponentsInChildren(tempSMRs);
        foreach (SkinnedMeshRenderer tempSMR in tempSMRs)
        {
            GameObject gameObject2 = tempSMR.gameObject;
            string name = gameObject2.name;
            tempSMR.bones = TranslateTransforms(tempSMR.bones, boneCatalog);
            tempSMR.rootBone = Find(boneCatalog, tempSMR.rootBone.name);
            tempSMR.updateWhenOffscreen = true;
            Material[] sharedMaterials = tempSMR.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material material = sharedMaterials[i];
                if ((bool)material)
                {
                    string name2 = material.name;
                    if (name2.Contains("_Body"))
                    {
                        Material material2 = DataLoader.LoadAsset<Material>(baseBodyMatLoc);
                        sharedMaterials[i] = (material2 ? material2 : sharedMaterials[i]);
                    }
                    else if (name2.Contains("_Head"))
                    {
                        Material material3 = DataLoader.LoadAsset<Material>(baseHeadMatLoc);
                        sharedMaterials[i] = (material3 ? material3 : sharedMaterials[i]);
                    }
                    else if (name2.Contains("_Hand"))
                    {
                        Material material4 = DataLoader.LoadAsset<Material>(baseHandsMatLoc);
                        sharedMaterials[i] = (material4 ? material4 : sharedMaterials[i]);
                    }
                }
            }

            if (name == "eyes" && (bool)eyeMat)
            {
                sharedMaterials[0] = eyeMat;
            }

            tempSMR.sharedMaterials = sharedMaterials;
            Material material5 = sharedMaterials[0];
            string text = (material5 ? material5.shader.name : "");
            if (text.Equals("Game/SDCS/Skin") || text.Equals("Game/SDCS/Hair"))
            {
                ((Behaviour)(object)gameObject2.AddComponent<ExcludeReflections>()).enabled = !isFPV;
            }

            if (!text.Equals("Game/Character") && !text.Equals("Game/CharacterPlayerSkin") && !text.Equals("Game/CharacterPlayerOutfit") && !text.Equals("Game/CharacterCloth"))
            {
                continue;
            }

            Material material6 = tempSMR.material;
            if (name == "hands" || name == "gloves")
            {
                material6.SetFloat("_FirstPerson", 0f);
                material6.SetFloat("_ClipRadius", 0f);
            }
            else if (!isUI)
            {
                if ((bool)emodel && isFPV)
                {
                    emodel.ClipMaterialsFP.Add(material6);
                }

                material6.SetFloat("_FirstPerson", ((bool)emodel && isFPV) ? 1 : 0);
                material6.SetFloat("_ClipRadius", cullDist);
            }
            else
            {
                material6.SetFloat("_FirstPerson", 0f);
                material6.SetFloat("_ClipRadius", 0f);
            }

            material6.SetVector("_ClipCenter", boneCatalog["Head"].position);
            if (!isUI && (bool)emodel && isFPV && isGear)
            {
                RemoveFPViewObstructingGearPolygons(tempSMR);
            }
        }

        tempSMRs.Clear();
        Transform transform = boneCatalog["Hips"];
        gameObject.GetComponentsInChildren(tempCloths);
        foreach (Cloth tempCloth in tempCloths)
        {
            tempCloth.capsuleColliders = transform.GetComponentsInChildren<CapsuleCollider>();
        }

        tempCloths.Clear();
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void RemoveFPViewObstructingGearPolygons(SkinnedMeshRenderer smr)
    {
        if (!smr || !smr.sharedMesh)
        {
            return;
        }

        Mesh mesh2 = (smr.sharedMesh = UnityEngine.Object.Instantiate(smr.sharedMesh));
        Color[] colors = mesh2.colors;
        if (colors.Length == 0)
        {
            return;
        }

        int[] triangles = mesh2.triangles;
        int num = triangles.Length / 3;
        _ = mesh2.vertexCount;
        int[] array = new int[num * 3];
        int num2 = 0;
        for (int i = 0; i < num; i++)
        {
            int num3 = triangles[i * 3];
            int num4 = triangles[i * 3 + 1];
            int num5 = triangles[i * 3 + 2];
            if (colors[num3].r == 0f && colors[num4].r == 0f && colors[num5].r == 0f)
            {
                array[num2 * 3] = num3;
                array[num2 * 3 + 1] = num4;
                array[num2 * 3 + 2] = num5;
                num2++;
            }
        }

        Array.Resize(ref array, num2 * 3);
        mesh2.triangles = array;
        if (num > num2)
        {
            Debug.Log("SDCSUtils::RemoveFPViewObstructingGearPolygons -> Removed " + (num - num2) + " obstructing polygons from " + mesh2.name);
        }
        else
        {
            Debug.Log("SDCSUtils::RemoveFPViewObstructingGearPolygons -> " + mesh2.name + " has no obstructing polygons");
        }
    }

    public static void MatchRigs(SlotData wornItem, Transform source, Transform target, TransformCatalog transformCatalog)
    {
        Transform transform = source.Find("Origin");
        Transform transform2 = target.Find("Origin");
        if ((bool)transform && (bool)transform2)
        {
            AddMissingChildren(wornItem, transform, transform2, transformCatalog);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void AddMissingChildren(SlotData wornItem, Transform sourceT, Transform targetT, TransformCatalog transformCatalog)
    {
        if (!sourceT || !targetT)
        {
            return;
        }

        foreach (Transform item in sourceT)
        {
            string name = item.name;
            Transform transform2 = null;
            bool flag = false;
            foreach (Transform item2 in targetT)
            {
                string name2 = item2.name;
                if (name2 == name)
                {
                    transform2 = item2;
                    flag = true;
                    if (!transformCatalog.ContainsKey(name2))
                    {
                        transformCatalog.Add(name2, transform2);
                    }

                    break;
                }
            }

            if (!flag)
            {
                transform2 = UnityEngine.Object.Instantiate(item.gameObject).transform;
                transform2.SetParent(targetT, worldPositionStays: false);
                transform2.name = name;
                transformCatalog[name] = transform2;
            }

            if (!flag)
            {
                transform2.SetLocalPositionAndRotation(item.localPosition, item.localRotation);
                transform2.localScale = item.localScale;
            }

            TransferCharacterJoint(item, transform2.gameObject, transformCatalog);
            AddMissingChildren(wornItem, item, transform2, transformCatalog);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void SetupRigConstraints(RigBuilder rigBuilder, Transform sourceRootT, Transform targetRootT, TransformCatalog transformCatalog)
    {
        //IL_0094: Unknown result type (might be due to invalid IL or missing references)
        //IL_009e: Expected O, but got Unknown
        if (!(UnityEngine.Object)(object)sourceRootT.GetComponent<RigBuilder>())
        {
            return;
        }

        Transform transform = sourceRootT.Find("RigConstraints");
        if (!transform)
        {
            return;
        }

        string text = transform.name + "_" + transform.parent.name;
        Transform transform2 = targetRootT.Find(text);
        if (!transform2)
        {
            transform2 = UnityEngine.Object.Instantiate(transform, targetRootT);
            transform2.name = text;
            transform2.SetLocalPositionAndRotation(transform.localPosition, transform.localRotation);
            transform2.localScale = transform.localScale;
        }

        Rig component = transform2.GetComponent<Rig>();
        if (!(UnityEngine.Object)(object)component)
        {
            return;
        }

        rigBuilder.layers.Add(new RigLayer(component, true));
        BlendConstraint[] componentsInChildren = transform.GetComponentsInChildren<BlendConstraint>();
        BlendConstraint[] componentsInChildren2 = transform2.GetComponentsInChildren<BlendConstraint>();
        foreach (BlendConstraint val in componentsInChildren2)
        {
            string name = ((UnityEngine.Object)(object)val).name;
            BlendConstraint[] array = componentsInChildren;
            foreach (BlendConstraint val2 in array)
            {
                if (((UnityEngine.Object)(object)val2).name == name)
                {
                    ((BlendConstraintData)(ref ((RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>)(object)val).data)).constrainedObject = Find(transformCatalog, ((BlendConstraintData)(ref ((RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>)(object)val2).data)).constrainedObject.name);
                    ((BlendConstraintData)(ref ((RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>)(object)val).data)).sourceObjectA = Find(transformCatalog, ((BlendConstraintData)(ref ((RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>)(object)val2).data)).sourceObjectA.name);
                    ((BlendConstraintData)(ref ((RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>)(object)val).data)).sourceObjectB = Find(transformCatalog, ((BlendConstraintData)(ref ((RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>)(object)val2).data)).sourceObjectB.name);
                    break;
                }
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void TransferCharacterJoint(Transform source, GameObject newBone, TransformCatalog transformCatalog)
    {
        CharacterJoint component;
        CharacterJoint val;
        if ((UnityEngine.Object)(object)(component = source.GetComponent<CharacterJoint>()) != null && (UnityEngine.Object)(object)(val = NGUITools.AddMissingComponent<CharacterJoint>(newBone)) != null)
        {
            ((Joint)val).connectedBody = Find(transformCatalog, ((UnityEngine.Object)(object)((Joint)component).connectedBody).name)?.GetComponent<Rigidbody>();
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static GameObject AddChild(GameObject source, Transform parent)
    {
        source.transform.parent = parent;
        foreach (Transform item in source.transform)
        {
            UnityEngine.Object.Destroy(item.gameObject);
        }

        return source;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static SkinnedMeshRenderer AddSkinnedMeshRenderer(SkinnedMeshRenderer source, GameObject parent)
    {
        GameObject gameObject = new GameObject(source.name);
        gameObject.transform.parent = parent.transform;
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.sharedMesh = source.sharedMesh;
        skinnedMeshRenderer.sharedMaterials = source.sharedMaterials;
        return skinnedMeshRenderer;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static Transform[] TranslateTransforms(Transform[] transforms, TransformCatalog transformCatalog)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if ((bool)transform)
            {
                transforms[i] = Find(transformCatalog, transform.name);
            }
            else
            {
                Log.Error("Null transform in bone list");
            }
        }

        return transforms;
    }

    public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
    {
        source.TryGetValue(key, out var value);
        return value;
    }

    public static void CreateVizTP(Archetype _archetype, ref GameObject baseRig, ref TransformCatalog boneCatalog, EntityAlive entity, bool isFPV)
    {
        DestroyViz(baseRig, _keepRig: true);
        tmpArchetype = _archetype;
        setupRig(ref baseRig, ref boneCatalog, baseRigPrefab, null, TPAnimController);
        if (!isFPV)
        {
            setupBase(baseRig, boneCatalog, baseParts, isFPV);
            setupEquipment(baseRig, boneCatalog, ignoredParts, entity, isUI: false, !(entity is EntityPlayerLocal));
            setupHairObjects(baseRig, boneCatalog, ignoredParts, entity, isUI: false);
        }
    }

    public static void CreateVizFP(Archetype _archetype, ref GameObject baseRigFP, ref TransformCatalog boneCatalogFP, EntityAlive entity, bool isFPV)
    {
        DestroyViz(baseRigFP, _keepRig: true);
        tmpArchetype = _archetype;
        Transform transform = entity.transform.FindInChildren("Camera");
        if (transform == null)
        {
            if (!(GameObject.Find("Camera") != null))
            {
                Log.Error("Unable to find first person camera!");
                return;
            }

            transform = GameObject.Find("Camera").transform;
        }

        Transform transform2 = transform.FindInChildren("Pivot");
        if (transform2 != null)
        {
            transform = transform2.parent;
        }

        setupRig(ref baseRigFP, ref boneCatalogFP, baseRigFPPrefab, transform, FPAnimController);
        setupBase(baseRigFP, boneCatalogFP, basePartsFP, isFPV);
        setupEquipment(baseRigFP, boneCatalogFP, ignoredPartsFP, entity, isUI: false, !(entity is EntityPlayerLocal));
        setupHairObjects(baseRigFP, boneCatalogFP, ignoredPartsFP, entity, isUI: false);
        Transform transform3 = baseRigFP.transform;
        transform3.SetParent(transform, worldPositionStays: false);
        transform3.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        baseRigFP.name = "baseRigFP";
        NGUITools.AddMissingComponent<AnimationEventBridge>(baseRigFP);
        SkinnedMeshRenderer[] componentsInChildren = baseRigFP.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("HoldingItem");
        }

        HingeJoint[] componentsInChildren2 = baseRigFP.GetComponentsInChildren<HingeJoint>();
        foreach (HingeJoint val in componentsInChildren2)
        {
            if ((UnityEngine.Object)(object)((Joint)val).connectedBody == null)
            {
                Log.Warning("SDCSUtils::CreateVizFP: No connected body for " + ((Component)(object)val).transform.name + "'s HingeJoint! Disabling for FP as it is never seen.");
                ((Component)(object)val).gameObject.SetActive(value: false);
            }
        }

        baseRigFP.GetComponentsInChildren(tempCloths);
        foreach (Cloth tempCloth in tempCloths)
        {
            tempCloth.enabled = false;
        }

        tempCloths.Clear();
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void SetupBodyColliders(GameObject baseRig)
    {
    }

    public static void CreateVizUI(Archetype _archetype, ref GameObject baseRigUI, ref TransformCatalog boneCatalogUI, EntityAlive entity, bool useTempCosmetics)
    {
        DestroyViz(baseRigUI, _keepRig: true);
        tmpArchetype = _archetype;
        setupRig(ref baseRigUI, ref boneCatalogUI, baseRigPrefab, null, UIAnimController);
        SetupBodyColliders(baseRigUI);
        setupBase(baseRigUI, boneCatalogUI, baseParts, isFPV: false);
        setupEquipment(baseRigUI, boneCatalogUI, ignoredParts, entity, isUI: true, useTempCosmetics, useTempCosmetics);
        setupHairObjects(baseRigUI, boneCatalogUI, ignoredParts, entity, isUI: true);
        Transform transform = baseRigUI.transform.Find("IKRig");
        if (transform != null)
        {
            transform.GetComponent<Rig>().weight = 0f;
        }

        HingeJoint[] componentsInChildren = baseRigUI.GetComponentsInChildren<HingeJoint>();
        foreach (HingeJoint val in componentsInChildren)
        {
            if ((UnityEngine.Object)(object)((Joint)val).connectedBody == null)
            {
                Log.Warning("SDCSUtils::CreateVizUI: No connected body for " + ((Component)(object)val).transform.name + "'s HingeJoint! Disabling for UI until this is solved.");
                ((Component)(object)val).gameObject.SetActive(value: false);
            }
        }
    }

    public static void CreateVizUI(Archetype _archetype, ref GameObject baseRigUI, ref TransformCatalog boneCatalogUI)
    {
        DestroyViz(baseRigUI);
        tmpArchetype = _archetype;
        setupRig(ref baseRigUI, ref boneCatalogUI, baseRigPrefab, null, UIAnimController);
        setupBase(baseRigUI, boneCatalogUI, baseParts, isFPV: false);
        setupHairObjects(baseRigUI, boneCatalogUI, null, _isFPV: false, ignoredParts, isUI: true, _archetype.Hair, _archetype.MustacheName, _archetype.ChopsName, _archetype.BeardName);
        setupEquipment(baseRigUI, boneCatalogUI, ignoredParts, isUI: true, _archetype.Equipment, _ignoreDlcEntitlements: false);
        Transform transform = baseRigUI.transform.Find("IKRig");
        if (transform != null)
        {
            transform.GetComponent<Rig>().weight = 0f;
        }

        HingeJoint[] componentsInChildren = baseRigUI.GetComponentsInChildren<HingeJoint>();
        foreach (HingeJoint val in componentsInChildren)
        {
            if ((UnityEngine.Object)(object)((Joint)val).connectedBody == null)
            {
                Log.Warning("SDCSUtils::CreateVizUI: No connected body for " + ((Component)(object)val).transform.name + "'s HingeJoint! Disabling for UI until this is solved.");
                ((Component)(object)val).gameObject.SetActive(value: false);
            }
        }
    }

    public static void DestroyViz(GameObject _baseRigUI, bool _keepRig = false)
    {
        if (!_baseRigUI)
        {
            return;
        }

        Transform transform = _baseRigUI.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!(child.name != "Origin"))
            {
                continue;
            }

            child.GetComponentsInChildren(includeInactive: true, tempSMRs);
            foreach (SkinnedMeshRenderer tempSMR in tempSMRs)
            {
                Mesh sharedMesh = tempSMR.sharedMesh;
                if (MeshMorph.IsInstance(sharedMesh))
                {
                    UnityEngine.Object.Destroy(sharedMesh);
                }

                tempSMR.GetSharedMaterials(tempMats);
                Utils.CleanupMaterials(tempMats);
                tempMats.Clear();
            }
        }

        if (!_keepRig)
        {
            UnityEngine.Object.DestroyImmediate(_baseRigUI);
        }
    }

    public static void SetVisible(GameObject _baseRigUI, bool _visible)
    {
        if (!_baseRigUI)
        {
            return;
        }

        Transform transform = _baseRigUI.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!(child.name != "Origin"))
            {
                continue;
            }

            child.GetComponentsInChildren(includeInactive: true, tempSMRs);
            foreach (SkinnedMeshRenderer tempSMR in tempSMRs)
            {
                tempSMR.gameObject.SetActive(_visible);
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupRig(ref GameObject _rigObj, ref TransformCatalog _boneCatalog, string prefabLocation, Transform parent, RuntimeAnimatorController animController)
    {
        if (!_rigObj)
        {
            _rigObj = UnityEngine.Object.Instantiate(DataLoader.LoadAsset<GameObject>(prefabLocation), parent);
            _boneCatalog = new TransformCatalog(_rigObj.transform);
            Animator component = _rigObj.GetComponent<Animator>();
            if ((bool)(UnityEngine.Object)(object)component && (UnityEngine.Object)(object)component.runtimeAnimatorController != (UnityEngine.Object)(object)animController)
            {
                component.runtimeAnimatorController = animController;
            }

            BoneRenderer[] componentsInChildren = _rigObj.GetComponentsInChildren<BoneRenderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                ((Behaviour)(object)componentsInChildren[i]).enabled = false;
            }
        }
        else
        {
            cleanupEquipment(_rigObj);
        }

        if (!tmpArchetype.IsMale)
        {
            CapsuleCollider orAddComponent = _boneCatalog["Hips"].gameObject.GetOrAddComponent<CapsuleCollider>();
            orAddComponent.center = new Vector3(0f, 0f, -0.03f);
            orAddComponent.radius = 0.15f;
            orAddComponent.height = 0.375f;
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void cleanupEquipment(GameObject _rigObj)
    {
        RigBuilder component = _rigObj.GetComponent<RigBuilder>();
        if ((bool)(UnityEngine.Object)(object)component)
        {
            List<RigLayer> layers = component.layers;
            for (int num = layers.Count - 1; num >= 0; num--)
            {
                if (layers[num].name != "IKRig")
                {
                    layers.RemoveAt(num);
                }
            }

            component.Clear();
        }

        Animator component2 = _rigObj.GetComponent<Animator>();
        if ((bool)(UnityEngine.Object)(object)component2)
        {
            AnimatorJobExtensions.UnbindAllStreamHandles(component2);
        }

        GameUtils.DestroyAllChildrenBut(_rigObj.transform, new List<string> { "Origin", "IKRig" });
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupBase(GameObject _rig, TransformCatalog _boneCatalog, string[] baseParts, bool isFPV)
    {
        foreach (string text in baseParts)
        {
            GameObject gameObject = ((text == "head") ? DataLoader.LoadAsset<GameObject>(baseHeadLoc) : ((!(text == "hands")) ? DataLoader.LoadAsset<GameObject>(baseBodyLoc) : DataLoader.LoadAsset<GameObject>(baseBodyLoc)));
            if (gameObject == null)
            {
                break;
            }

            GameObject bodyPartContainingName;
            if (!((bodyPartContainingName = getBodyPartContainingName(gameObject.transform, text)) == null))
            {
                bodyPartContainingName.name = text;
                Material eyeMat = DataLoader.LoadAsset<Material>(baseEyeColorMatLoc);
                Stitch(bodyPartContainingName, _rig, _boneCatalog, null, isFPV, 0f, isUI: false, eyeMat);
                if (text == "head")
                {
                    Transform transform = _rig.transform;
                    Transform transform2 = gameObject.transform;
                    CharacterGazeController orAddComponent = transform.FindRecursive("Head").gameObject.GetOrAddComponent<CharacterGazeController>();
                    orAddComponent.rootTransform = transform.FindRecursive("Origin");
                    orAddComponent.neckTransform = _boneCatalog["Neck"];
                    orAddComponent.headTransform = _boneCatalog["Head"];
                    orAddComponent.leftEyeTransform = _boneCatalog["LeftEye"];
                    orAddComponent.rightEyeTransform = _boneCatalog["RightEye"];
                    orAddComponent.eyeMaterial = transform.FindRecursive("eyes").GetComponent<SkinnedMeshRenderer>().material;
                    orAddComponent.leftEyeLocalPosition = transform2.FindInChildren("LeftEye").localPosition;
                    orAddComponent.rightEyeLocalPosition = transform2.FindInChildren("RightEye").localPosition;
                    orAddComponent.eyeLookAtTargetAngle = 35f;
                    orAddComponent.eyeRotationSpeed = 30f;
                    orAddComponent.twitchSpeed = 25f;
                    orAddComponent.headLookAtTargetAngle = 45f;
                    orAddComponent.headRotationSpeed = 7f;
                    orAddComponent.maxLookAtDistance = 5f;
                    EyeLidController orAddComponent2 = transform.FindRecursive("Head").gameObject.GetOrAddComponent<EyeLidController>();
                    orAddComponent2.leftTopTransform = _boneCatalog["LeftEyelidTop"];
                    orAddComponent2.leftBottomTransform = _boneCatalog["LeftEyelidBot"];
                    orAddComponent2.rightTopTransform = _boneCatalog["RightEyelidTop"];
                    orAddComponent2.rightBottomTransform = _boneCatalog["RightEyelidBot"];
                    orAddComponent2.leftTopLocalPosition = transform2.FindInChildren("LeftEyelidTop").localPosition;
                    orAddComponent2.leftBottomLocalPosition = transform2.FindInChildren("LeftEyelidBot").localPosition;
                    orAddComponent2.leftTopRotation = transform2.FindInChildren("LeftEyelidTop").localRotation;
                    orAddComponent2.leftBottomRotation = transform2.FindInChildren("LeftEyelidBot").localRotation;
                    orAddComponent2.rightTopLocalPosition = transform2.FindInChildren("RightEyelidTop").localPosition;
                    orAddComponent2.rightBottomLocalPosition = transform2.FindInChildren("RightEyelidBot").localPosition;
                    orAddComponent2.rightTopRotation = transform2.FindInChildren("RightEyelidTop").localRotation;
                    orAddComponent2.rightBottomRotation = transform2.FindInChildren("RightEyelidBot").localRotation;
                }
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupEquipment(GameObject _rig, TransformCatalog _boneCatalog, string[] ignoredParts, bool isUI, List<SlotData> slotData, bool _ignoreDlcEntitlements)
    {
        if (slotData == null)
        {
            return;
        }

        List<Transform> allGears = new List<Transform>();
        Transform transform = _rig.transform.Find("Origin");
        if ((bool)transform)
        {
            List<Transform> list = findStartsWith(transform, "RigConstraints");
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object.DestroyImmediate(list[i].gameObject);
            }
        }

        foreach (SlotData slotDatum in slotData)
        {
            if ("head".Equals(slotDatum.PartName, StringComparison.OrdinalIgnoreCase) && slotDatum.PrefabName != null && slotDatum.PrefabName.Contains("HeadGearMorphMatrix", StringComparison.OrdinalIgnoreCase))
            {
                setupHeadgearMorph(_rig, _boneCatalog, null, _isFPV: false, isUI, slotDatum, _ignoreDlcEntitlements);
                continue;
            }

            Transform transform2 = setupEquipmentSlot(_rig, _boneCatalog, ignoredParts, slotDatum, allGears, _ignoreDlcEntitlements);
            if ((bool)transform2)
            {
                float cullDistance = slotDatum.CullDistance;
                Stitch(transform2.gameObject, _rig, _boneCatalog, null, isFPV: false, cullDistance, isUI);
            }
        }

        List<RigBuilder> rbs = new List<RigBuilder>();
        setupEquipmentCommon(_rig, _boneCatalog, allGears, rbs);
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupEquipment(GameObject _rig, TransformCatalog _boneCatalog, string[] ignoredParts, EntityAlive entity, bool isUI, bool _ignoreDlcEntitlements, bool useTempCosmetics = false)
    {
        if (!entity)
        {
            return;
        }

        EModelSDCS eModelSDCS = entity.emodel as EModelSDCS;
        if (!eModelSDCS)
        {
            return;
        }

        eModelSDCS.HairMaskType = SlotData.HairMaskTypes.Full;
        eModelSDCS.FacialHairMaskType = SlotData.HairMaskTypes.Full;
        if (!isUI && eModelSDCS.IsFPV)
        {
            eModelSDCS.ClipMaterialsFP.Clear();
        }

        List<Transform> allGears = new List<Transform>();
        Transform transform = _rig.transform.Find("Origin");
        if ((bool)transform)
        {
            List<Transform> list = findStartsWith(transform, "RigConstraints");
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object.DestroyImmediate(list[i].gameObject);
            }
        }

        int slotCount = entity.equipment.GetSlotCount();
        for (int j = 0; j < slotCount; j++)
        {
            ItemValue slotItem = entity.equipment.GetSlotItem(j);
            ItemClass cosmeticSlot = entity.equipment.GetCosmeticSlot(j, useTempCosmetics);
            bool flag = slotItem?.ItemClass?.SDCSData != null;
            bool flag2 = cosmeticSlot != null && (useTempCosmetics || (flag && (_ignoreDlcEntitlements || entity.equipment.HasCosmeticUnlocked(cosmeticSlot).isUnlocked)));
            if (!(flag || flag2) || cosmeticSlot == ItemClass.MissingItem)
            {
                continue;
            }

            ItemClass obj = (flag2 ? cosmeticSlot : slotItem.ItemClass);
            SlotData sDCSData = obj.SDCSData;
            if (cosmeticSlot != null && !flag2)
            {
                entity.equipment.SetCosmeticSlot(j, 0);
                entity.bPlayerStatsChanged = true;
            }

            if (obj is ItemClassArmor { EquipSlot: EquipmentSlots.Head })
            {
                eModelSDCS.HairMaskType = sDCSData.HairMaskType;
                eModelSDCS.FacialHairMaskType = sDCSData.FacialHairMaskType;
            }

            if ("head".Equals(sDCSData.PartName, StringComparison.OrdinalIgnoreCase) && sDCSData.PrefabName != null && sDCSData.PrefabName.Contains("HeadGearMorphMatrix", StringComparison.OrdinalIgnoreCase))
            {
                setupHeadgearMorph(_rig, _boneCatalog, eModelSDCS, eModelSDCS.IsFPV, isUI, sDCSData, _ignoreDlcEntitlements);
            }
            else if (!"head".Equals(sDCSData.PartName, StringComparison.OrdinalIgnoreCase) || !eModelSDCS.IsFPV || isUI)
            {
                Transform transform2 = setupEquipmentSlot(_rig, _boneCatalog, ignoredParts, sDCSData, allGears, _ignoreDlcEntitlements);
                if ((bool)transform2)
                {
                    float cullDistance = sDCSData.CullDistance;
                    Stitch(transform2.gameObject, _rig, _boneCatalog, eModelSDCS, eModelSDCS.IsFPV, cullDistance, isUI, null, isGear: true);
                }
            }
        }

        List<RigBuilder> rbs = new List<RigBuilder>();
        setupEquipmentCommon(_rig, _boneCatalog, allGears, rbs);
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static Transform setupEquipmentSlot(GameObject _rig, TransformCatalog _boneCatalog, string[] ignoredParts, SlotData wornItem, List<Transform> allGears, bool _ignoreDlcEntitlements)
    {
        string pathForSlotData = GetPathForSlotData(wornItem);
        if (string.IsNullOrEmpty(pathForSlotData))
        {
            return null;
        }

        GameObject gameObject = DataLoader.LoadAsset<GameObject>(pathForSlotData, _ignoreDlcEntitlements);
        if (!gameObject)
        {
            Log.Warning("SDCSUtils::" + pathForSlotData + " not found for item " + wornItem.PrefabName + "!");
            return null;
        }

        MatchRigs(wornItem, gameObject.transform, _rig.transform, _boneCatalog);
        if (!allGears.Contains(gameObject.transform))
        {
            allGears.Add(gameObject.transform);
        }

        Transform clothingPartWithName = getClothingPartWithName(gameObject, parseSexedLocation(wornItem.PartName, tmpArchetype.Sex));
        if ((bool)clothingPartWithName)
        {
            string baseToTurnOff = wornItem.BaseToTurnOff;
            if (baseToTurnOff != null && baseToTurnOff.Length > 0)
            {
                string[] array = wornItem.BaseToTurnOff.Split(',', StringSplitOptions.None);
                foreach (string name in array)
                {
                    Transform transform = _rig.transform.FindInChildren(name);
                    if ((bool)transform)
                    {
                        UnityEngine.Object.Destroy(transform.gameObject);
                    }
                }
            }

            if (!clothingPartWithName.gameObject.activeSelf)
            {
                clothingPartWithName.gameObject.SetActive(value: true);
            }
        }

        return clothingPartWithName;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupEquipmentCommon(GameObject _rigObj, TransformCatalog _boneCatalog, List<Transform> allGears, List<RigBuilder> rbs)
    {
        RigBuilder val = _rigObj.GetComponent<RigBuilder>();
        if (!(UnityEngine.Object)(object)val)
        {
            val = _rigObj.AddComponent<RigBuilder>();
        }

        ((Behaviour)(object)val).enabled = false;
        rbs.Add(val);
        foreach (Transform allGear in allGears)
        {
            SetupRigConstraints(val, allGear, _rigObj.transform, _boneCatalog);
        }

        HingeJoint[] componentsInChildren = _rigObj.GetComponentsInChildren<HingeJoint>();
        foreach (HingeJoint val2 in componentsInChildren)
        {
            if ((UnityEngine.Object)(object)((Joint)val2).connectedBody != null && _boneCatalog.ContainsKey(((Component)(object)((Joint)val2).connectedBody).transform.name))
            {
                ((Joint)val2).connectedBody = _boneCatalog[((Component)(object)((Joint)val2).connectedBody).transform.name].GetComponent<Rigidbody>();
            }

            ((Joint)val2).autoConfigureConnectedAnchor = true;
        }

        ((Behaviour)(object)val).enabled = true;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupHairObjects(GameObject _rig, TransformCatalog _boneCatalog, string[] ignoredParts, EntityAlive entity, bool isUI)
    {
        if (!entity)
        {
            return;
        }

        EModelSDCS eModelSDCS = entity.emodel as EModelSDCS;
        if (!eModelSDCS)
        {
            return;
        }

        if (!isUI && eModelSDCS.IsFPV)
        {
            eModelSDCS.ClipMaterialsFP.Clear();
        }

        Transform transform = _rig.transform.Find("Origin");
        if ((bool)transform)
        {
            List<Transform> list = findStartsWith(transform, "RigConstraints");
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object.DestroyImmediate(list[i].gameObject);
            }
        }

        if (!eModelSDCS.IsFPV || isUI)
        {
            setupHairObjects(_rig, _boneCatalog, eModelSDCS, eModelSDCS.IsFPV, ignoredParts, isUI, tmpArchetype.Hair, tmpArchetype.MustacheName, tmpArchetype.ChopsName, tmpArchetype.BeardName);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupHairObjects(GameObject _rig, TransformCatalog _boneCatalog, EModelSDCS _emodel, bool _isFPV, string[] ignoredParts, bool isUI, string hairName, string mustacheName, string chopsName, string beardName)
    {
        HairColorSwatch hairColorSwatch = null;
        if (!string.IsNullOrEmpty(tmpArchetype.HairColor))
        {
            string text = baseHairColorLoc + "/" + tmpArchetype.HairColor + ".asset";
            ScriptableObject scriptableObject = DataLoader.LoadAsset<ScriptableObject>(text);
            if (scriptableObject == null)
            {
                Log.Warning("SDCSUtils::" + text + " not found for hair color " + tmpArchetype.HairColor + "!");
            }
            else
            {
                hairColorSwatch = scriptableObject as HairColorSwatch;
            }
        }

        if (!string.IsNullOrEmpty(hairName))
        {
            if (_emodel != null)
            {
                if (_emodel.HairMaskType != SlotData.HairMaskTypes.None)
                {
                    string text2 = "";
                    if (_emodel.HairMaskType != 0)
                    {
                        text2 = "_" + _emodel.HairMaskType.ToString().ToLower();
                    }

                    setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseHairLoc + "/hair_" + hairName + text2 + ".asset", hairName);
                }
            }
            else
            {
                setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseHairLoc + "/hair_" + hairName + ".asset", hairName);
            }
        }

        if (!string.IsNullOrEmpty(mustacheName))
        {
            if (_emodel != null)
            {
                if (_emodel.FacialHairMaskType != SlotData.HairMaskTypes.None)
                {
                    setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseMustacheLoc + "/hair_facial_mustache" + mustacheName + ".asset", mustacheName);
                }
            }
            else
            {
                setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseMustacheLoc + "/hair_facial_mustache" + mustacheName + ".asset", mustacheName);
            }
        }

        if (!string.IsNullOrEmpty(chopsName))
        {
            if (_emodel != null)
            {
                if (_emodel.FacialHairMaskType != SlotData.HairMaskTypes.None)
                {
                    setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseChopsLoc + "/hair_facial_sideburns" + chopsName + ".asset", chopsName);
                }
            }
            else
            {
                setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseChopsLoc + "/hair_facial_sideburns" + chopsName + ".asset", chopsName);
            }
        }

        if (!string.IsNullOrEmpty(beardName))
        {
            if (_emodel != null)
            {
                if (_emodel.FacialHairMaskType != SlotData.HairMaskTypes.None)
                {
                    setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseBeardLoc + "/hair_facial_beard" + beardName + ".asset", beardName);
                }
            }
            else
            {
                setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, baseBeardLoc + "/hair_facial_beard" + beardName + ".asset", beardName);
            }
        }

        if (hairColorSwatch != null)
        {
            ApplySwatchToGameObject(_rig, hairColorSwatch);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void ApplySwatchToGameObject(GameObject targetGameObject, HairColorSwatch hairSwatch)
    {
        if (targetGameObject != null)
        {
            Renderer[] componentsInChildren = targetGameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer renderer in componentsInChildren)
            {
                Material[] array = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
                Material[] array2 = array;
                foreach (Material material in array2)
                {
                    if (material.shader.name == "Game/SDCS/Hair" && !material.name.Contains("lashes"))
                    {
                        hairSwatch.ApplyToMaterial(material);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No target GameObject selected.");
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupHair(GameObject _rig, TransformCatalog _boneCatalog, EModelSDCS _emodel, bool _isFPV, string[] ignoredParts, bool isUI, string path, string hairName)
    {
        if (string.IsNullOrEmpty(hairName))
        {
            return;
        }

        GameObject gameObject = DataLoader.LoadAsset<MeshMorph>(path)?.GetMorphedSkinnedMesh();
        if (gameObject == null)
        {
            Log.Warning("SDCSUtils::" + path + " not found for hair " + hairName + "!");
        }
        else
        {
            MatchRigs(null, gameObject.transform, _rig.transform, _boneCatalog);
            if (!gameObject.gameObject.activeSelf)
            {
                gameObject.gameObject.SetActive(value: true);
            }

            Stitch(gameObject.gameObject, _rig, _boneCatalog, _emodel, _isFPV, 0f, isUI);
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static string GetPathForSlotData(SlotData _slotData, bool _headgearShortHairMask = false)
    {
        if (_slotData == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(_slotData.PartName))
        {
            return null;
        }

        if (_slotData.PartName.Equals("head", StringComparison.OrdinalIgnoreCase))
        {
            if (ignoredParts.Contains("head"))
            {
                return null;
            }

            string text = _slotData.PrefabName;
            if (text.Contains("{sex}"))
            {
                text = text.Replace("{sex}", tmpArchetype.Sex);
            }

            if (text.Contains("{race}"))
            {
                text = text.Replace("{race}", tmpArchetype.Race);
            }

            if (text.Contains("{variant}"))
            {
                text = text.Replace("{variant}", tmpArchetype.Variant.ToString("00"));
            }

            if (text.Contains("{hair}"))
            {
                text = ((!_headgearShortHairMask || (!string.IsNullOrEmpty(tmpArchetype.Hair) && !shortHairNames.ContainsCaseInsensitive(tmpArchetype.Hair))) ? text.Replace("{hair}", "") : text.Replace("{hair}", "Bald"));
            }

            return text;
        }

        if (string.IsNullOrEmpty(_slotData.PrefabName))
        {
            return null;
        }

        string[] array = ignoredParts;
        foreach (string text2 in array)
        {
            if (_slotData.PartName.Contains(text2, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        return parseSexedLocation(_slotData.PrefabName, tmpArchetype.Sex);
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void setupHeadgearMorph(GameObject _rig, TransformCatalog _boneCatalog, EModelSDCS _emodel, bool _isFPV, bool isUI, SlotData _slotData, bool _ignoreDlcEntitlements)
    {
        string pathForSlotData = GetPathForSlotData(_slotData, _headgearShortHairMask: true);
        if (string.IsNullOrEmpty(pathForSlotData) || (!isUI && _isFPV))
        {
            return;
        }

        MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(pathForSlotData, _ignoreDlcEntitlements);
        if (meshMorph == null)
        {
            pathForSlotData = GetPathForSlotData(_slotData);
            meshMorph = DataLoader.LoadAsset<MeshMorph>(pathForSlotData, _ignoreDlcEntitlements);
        }

        GameObject gameObject = meshMorph?.GetMorphedSkinnedMesh();
        if (gameObject == null)
        {
            Log.Warning("SDCSUtils::" + pathForSlotData + " not found for headgear " + _slotData.PrefabName + "!");
            return;
        }

        MatchRigs(null, gameObject.transform, _rig.transform, _boneCatalog);
        if (!gameObject.gameObject.activeSelf)
        {
            gameObject.gameObject.SetActive(value: true);
        }

        DataLoader.LoadAsset<Material>(baseBodyMatLoc);
        Stitch(gameObject.gameObject, _rig, _boneCatalog, _emodel, _isFPV, 0f, isUI);
        UnityEngine.Object.Destroy(gameObject);
    }

    public static bool BasePartsExist(Archetype _archetype)
    {
        tmpArchetype = _archetype;
        if (!DataLoader.LoadAsset<GameObject>(baseBodyLoc))
        {
            Log.Error("base body not found at " + baseBodyLoc);
            return false;
        }

        if (!DataLoader.LoadAsset<GameObject>(baseHeadLoc))
        {
            Log.Error("base head not found at " + baseHeadLoc);
            return false;
        }

        if (!DataLoader.LoadAsset<Material>(baseBodyMatLoc))
        {
            Log.Error("body material not found at " + baseBodyMatLoc);
            return false;
        }

        if (!DataLoader.LoadAsset<Material>(baseHeadMatLoc))
        {
            Log.Error("head material not found at " + baseHeadMatLoc);
            return false;
        }

        return true;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static List<Transform> findStartsWith(Transform parent, string key)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform item in parent)
        {
            if (item.name.StartsWith(key))
            {
                list.Add(item);
            }
        }

        return list;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static GameObject getBodyPartContainingName(Transform parent, string name)
    {
        foreach (Transform item in parent.transform)
        {
            if (item.name.ToLower().Contains(name))
            {
                return item.gameObject;
            }
        }

        return null;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static Transform getClothingPartWithName(GameObject clothingPrefab, string partName)
    {
        foreach (Transform item in clothingPrefab.transform)
        {
            if (item.name.ToLower() == partName.ToLower())
            {
                return item;
            }
        }

        return null;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static string parseSexedLocation(string sexedLocation, string sex)
    {
        return sexedLocation.Replace("{sex}", sex);
    }
}
