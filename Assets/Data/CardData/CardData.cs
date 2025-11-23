using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardData", menuName = "CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public float phyDamage;
    public float menDamage;
    public List<TagType> tags;
    public string description;
    public Sprite icon;
}
