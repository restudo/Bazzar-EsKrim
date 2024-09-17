using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_CustomerList", menuName = "Scriptable Objects/Customer List")]
public class SO_CustomerList : ScriptableObject
{
    public List<CustomerDetails> customerDetails;
}

[System.Serializable]
public struct CustomerDetails
{
    public float customerPatience;
    public float customerSpeed;
    // public Sprite[] customerMoods;
    public SkeletonDataAsset skeletonDataAsset;
    public Material material;
    public float positiveAnimationDuration;

    // for dummy
    // public Color customerColor;
    // public Sprite idleSprite;
    // TODO: add more sprite for moods
}
