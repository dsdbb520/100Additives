using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxPhyHP;
    public float maxMenHP;
    public List<TagType> weaknessTags;
    public List<TagType> resistTags;
}
