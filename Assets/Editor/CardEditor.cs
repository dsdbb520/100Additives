using UnityEditor;
using UnityEngine;

public class CardEditor : EditorWindow
{
    private CardDatabase _db;
    private CardListPanel _listPanel;
    private CardInspectorPanel _inspectorPanel;

    private const float SPLIT_RATIO = 0.35f;   //左侧列表占窗口宽度的比例

    [MenuItem("Window/卡牌编辑器")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<CardEditor>();
        wnd.titleContent = new GUIContent("卡牌编辑器");
        wnd.minSize = new Vector2(700, 400);
    }

    private void OnEnable()
    {
        _db = new CardDatabase();
        _db.ScanAll();
        _inspectorPanel = new CardInspectorPanel(_db);
        _listPanel = new CardListPanel(_db, OnCardSelected);
    }

    private void OnGUI()
    {
        DrawToolbar();

        float toolbarHeight = EditorStyles.toolbar.fixedHeight;
        Rect contentRect = new Rect(0, toolbarHeight, position.width, position.height - toolbarHeight);

        float leftWidth  = contentRect.width * SPLIT_RATIO;
        float rightWidth = contentRect.width - leftWidth;

        Rect leftRect  = new Rect(contentRect.x, contentRect.y, leftWidth,  contentRect.height);
        Rect rightRect = new Rect(leftWidth,      contentRect.y, rightWidth, contentRect.height);

        _listPanel.Draw(leftRect);

        //分割线
        EditorGUI.DrawRect(new Rect(leftWidth, contentRect.y, 1, contentRect.height), Color.gray);

        _inspectorPanel.Draw(rightRect);
    }

    //顶部工具栏：新建按钮+搜索框+排序下拉
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("+ 新建", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            // TODO: _db.Create(); 刷新列表（Repaint()）
        }

        GUILayout.Space(8);

        // TODO: 搜索框 → 写入 _listPanel.SearchKeyword
        // 可用 EditorGUILayout.TextField(_listPanel.SearchKeyword, EditorStyles.toolbarSearchField)

        GUILayout.FlexibleSpace();

        GUILayout.Label("排序:", EditorStyles.toolbarButton);
        // TODO: 下拉 EditorGUILayout.Popup(_listPanel.SortMode, new[]{"名称","稀有度","费用"}, EditorStyles.toolbarDropDown)
        // TODO: 写回 _listPanel.SortMode

        EditorGUILayout.EndHorizontal();
    }

    //选中一张卡时的处理（由CardListPanel回调）
    private void OnCardSelected(CardData card)
    {
        if (_inspectorPanel.IsDirty)
        {
            //TODO: EditorUtility.DisplayDialog 询问是否保存当前修改
            //选"保存" → _inspectorPanel.TrySave(false)
            //选"不保存" → 直接切换
        }

        _inspectorPanel.SetTarget(card);
        Repaint();
    }

    //关闭窗口时，如果有未保存修改则弹出警告
    private void OnDestroy()
    {
        if (_inspectorPanel != null && _inspectorPanel.IsDirty)
        {
            //TODO:EditorUtility.DisplayDialog提示未保存
            //可以在弹窗后 GetWindow<CardEditor>() 重新打开，让用户知道数据丢失
        }
    }
}
