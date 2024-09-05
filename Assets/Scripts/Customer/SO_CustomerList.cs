using System.Collections;
using System.Collections.Generic;
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
    public Sprite[] customerMoods;

    // for dummy
    // public Color customerColor;
    // public Sprite idleSprite;
    // TODO: add more sprite for moods
}
