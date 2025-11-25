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
    public bool isFrozen;

    // 克隆方法，返回一个新的 CardData 副本
    public CardData Clone()
    {
        CardData clone = Instantiate(this);  // 克隆 ScriptableObject
        clone.cardName = this.cardName;
        clone.cost = this.cost;
        clone.phyDamage = this.phyDamage;
        clone.menDamage = this.menDamage;
        clone.tags = new List<TagType>(this.tags);  // 深拷贝 tags 列表
        clone.description = this.description;
        clone.icon = this.icon;
        clone.isFrozen = this.isFrozen;
        return clone;
    }
}
