using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardDatabase
{
    private const string SCAN_PATH = "Assets/Data/Cards";

    public List<CardData> AllCards { get; private set; } = new List<CardData>();

    // 扫描文件夹，填充AllCards
    public void ScanAll()
    {
        //先清空AllCards，然后遍历扫描目录下的所有文件的GUID，加载成CardData并添加到AllCards
        AllCards.Clear();
        var guids = AssetDatabase.FindAssets("t:CardData", new[]{ SCAN_PATH });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            AllCards.Add(card);
        }
    }

    // 在SCAN_PATH下新建一张空卡
    public CardData Create()
    {
        ScriptableObject asset = ScriptableObject.CreateInstance<CardData>();
        AssetDatabase.CreateAsset(asset, GenerateUniquePath("NewCard"));
        AllCards.Add((CardData)asset);
        return (CardData)asset;
    }

    // 复制一张卡
    public CardData Duplicate(CardData source)
    {
        string srcPath = AssetDatabase.GetAssetPath(source);  //获取原卡的路径
        string destPath = GenerateUniquePath(source.cardName);  //生成新卡的路径，确保不重名
        AssetDatabase.CopyAsset(srcPath, destPath);   //复制文件
        var copy = AssetDatabase.LoadAssetAtPath<CardData>(destPath);  //创建副本对象减少内存占用
        AllCards.Add(copy); //添加到列表
        return copy;
    }

    // 删除一张卡
    public void Delete(CardData card)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(card));  //删除文件
        AllCards.Remove(card);  //从列表中移除
        return;
    }

    // 将修改写回磁盘
    public void Save(CardData card)
    {
        EditorUtility.SetDirty(card);  //标记对象已修改
        AssetDatabase.SaveAssets();    //保存所有修改的资产
    }

    // 生成不重名的新资产路径，例如 .../NewCard_01.asset
    private string GenerateUniquePath(string baseName)
    {
        int suffix = 1;
        while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID($"{SCAN_PATH}/{baseName}_{suffix:00}.asset"))) //如果路径已存在（即有同名文件），则增加后缀继续检查
            suffix++;
        return $"{SCAN_PATH}/{baseName}_{suffix:00}.asset"; 
    }
}
