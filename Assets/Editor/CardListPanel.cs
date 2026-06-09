using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardListPanel
{
    private CardDatabase _db;
    private Action<CardData> _onSelected;   //选中一张卡时通知CardEditor
    private CardData _selected;
    private Vector2 _scrollPos;

    //由CardEditor从外部写入（工具栏那里输入的）
    public string SearchKeyword = "";
    public int SortMode = 0;    //排序条件：0=名称 1=稀有度 2=费用

    public CardListPanel(CardDatabase db, Action<CardData> onSelected)
    {
        _db = db;
        _onSelected = onSelected;
    }

    // 每帧由 CardEditor 调用，rect 是分配给左侧面板的区域
    public void Draw(Rect rect)
    {
        GUILayout.BeginArea(rect);

        // TODO: BeginScrollView(_scrollPos)
        // TODO: foreach GetFilteredSorted() → DrawRow(card, card == _selected)
        // TODO: EndScrollView

        DrawBottomButtons();

        GUILayout.EndArea();
    }

    // 渲染列表中的一行
    private void DrawRow(CardData card, bool isSelected)
    {
        // TODO: EditorGUILayout.BeginHorizontal，如果 isSelected 用高亮背景色
        // TODO: 小图标 EditorGUILayout.ObjectField(card.icon, ...)
        // TODO: GUILayout.Label(card.cardName)
        // TODO: 稀有度标签，用 RarityUtils.GetColor(card.rarity) 染色
        // TODO: 检测鼠标点击 Event.current → _selected = card; _onSelected(card)
        // TODO: EditorGUILayout.EndHorizontal
    }

    // 左侧底部的复制/删除按钮
    private void DrawBottomButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("复制"))
        {
            // TODO: if (_selected != null) _db.Duplicate(_selected); 刷新列表
        }
        if (GUILayout.Button("删除"))
        {
            // TODO: if (_selected != null) 弹 DisplayDialog 确认 → _db.Delete; 清空选中
        }
        EditorGUILayout.EndHorizontal();
    }

    // 根据搜索词和排序模式过滤排序
    private List<CardData> GetFilteredSorted()
    {
        // TODO: 从 _db.AllCards 按 SearchKeyword 过滤（cardName.Contains）
        // TODO: 按 SortMode 用 LINQ OrderBy 排序
        // TODO: return 结果列表
        return _db.AllCards;
    }
}
