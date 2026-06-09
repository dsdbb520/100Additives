using UnityEditor;
using UnityEngine;

public class CardInspectorPanel
{
    private CardDatabase _db;
    private CardData _target;
    private SerializedObject _serializedObj;    // 包装 CardData，支持 Undo

    public bool IsDirty { get; private set; }

    public CardInspectorPanel(CardDatabase db)
    {
        _db = db;
    }

    // 切换编辑目标时调用（由 CardEditor.OnCardSelected 触发）
    public void SetTarget(CardData card)
    {
        _target = card;
        // TODO: _serializedObj = new SerializedObject(card)
        IsDirty = false;
    }

    // 每帧由 CardEditor 调用，rect 是分配给右侧面板的区域
    public void Draw(Rect rect)
    {
        GUILayout.BeginArea(rect);

        if (_target == null)
        {
            EditorGUILayout.HelpBox("请在左侧选择一张卡牌", MessageType.Info);
            GUILayout.EndArea();
            return;
        }

        // TODO: _serializedObj.Update()    ← 每帧开头必须调，同步外部修改

        EditorGUI.BeginChangeCheck();
        DrawFields();
        if (EditorGUI.EndChangeCheck())
            IsDirty = true;

        // TODO: _serializedObj.ApplyModifiedProperties()   ← 每帧结尾必须调，写回数据

        EditorGUILayout.Space(8);
        DrawSaveBar();

        GUILayout.EndArea();
    }

    // 渲染所有可编辑字段
    private void DrawFields()
    {
        EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("cardName"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("icon"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("rarity"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("description"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("cost"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("tags"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("targetType"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("isUnplayable"))
        // TODO: EditorGUILayout.PropertyField(_serializedObj.FindProperty("exhaustOnPlay"))

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("数值", EditorStyles.boldLabel);
        // TODO: phyDamage / menDamage / selfDamage / shieldValue / healValue / pressure

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("特殊效果", EditorStyles.boldLabel);
        // TODO: specialEffectID / isFrozen
    }

    // 底部保存栏
    private void DrawSaveBar()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("保存", GUILayout.Width(80)))
            TrySave(forceOverride: false);

        if (GUILayout.Button("强制保存", GUILayout.Width(80)))
            TrySave(forceOverride: true);

        if (IsDirty)
        {
            var prevColor = GUI.color;
            GUI.color = Color.red;
            GUILayout.Label("● 未保存");
            GUI.color = prevColor;
        }

        EditorGUILayout.EndHorizontal();
    }

    // 保存逻辑：forceOverride=true 时跳过校验
    public void TrySave(bool forceOverride)
    {
        if (!forceOverride)
        {
            var result = CardValidator.Validate(_target);
            if (result.HasErrors)
            {
                // TODO: string.Join("\n", result.Errors) 拼成消息
                // TODO: EditorUtility.DisplayDialog("保存失败", msg, "确定")
                return;
            }
        }

        _db.Save(_target);
        IsDirty = false;
    }
}
