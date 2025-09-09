using System;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentTestSceneTools : MonoBehaviour
{
    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Animator animator;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int layerIndex;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int maxLayers;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float turnRate = 600f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int totalModels;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int totalBodyParts;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int randomMaterial;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float targetLayerWeight;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float endLayerWeight;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int currentModel;

    public AnimationClip anim;

    public GameObject attachPoint;

    public GameObject prefabAttachment;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public GameObject attached;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Renderer meshRenderer;

    [PublicizedFrom(EAccessModifier.Private)]
    public void Start()
    {
        //IL_0025: Unknown result type (might be due to invalid IL or missing references)
        //IL_002b: Expected O, but got Unknown
        animator = GetComponent<Animator>();
        if ((UnityEngine.Object)(object)animator != null)
        {
            AnimatorOverrideController val = new AnimatorOverrideController(animator.runtimeAnimatorController);
            List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            AnimationClip[] animationClips = ((RuntimeAnimatorController)val).animationClips;
            foreach (AnimationClip key in animationClips)
            {
                list.Add(new KeyValuePair<AnimationClip, AnimationClip>(key, anim));
            }

            val.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)list);
            animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val;
        }

        if (attached != null)
        {
            attached = UnityEngine.Object.Instantiate(prefabAttachment);
            attached.transform.parent = attachPoint.transform;
            attached.transform.localPosition = Vector3.zero;
            attached.transform.localEulerAngles = Vector3.zero;
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            base.transform.Rotate(0f, turnRate * Time.deltaTime * -1f, 0f);
        }

        if (Input.GetKey(KeyCode.D))
        {
            base.transform.Rotate(0f, turnRate * Time.deltaTime, 0f);
        }
    }
}
