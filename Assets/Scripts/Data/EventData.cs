using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Event Data")]
public class EventData : ScriptableObject
{
    public string eventID;        //事件ID
    public string title;          //标题
    public Sprite eventImage;     //配图

    [TextArea]
    public List<string> dialogueLines; //对话内容

    [System.Serializable]
    public class EventOption
    {
        public string description; //按钮文字
        public string hoverText;   //鼠标悬停时的详细提示
    }

    public List<EventOption> options; //选项列表 最多3-4个
}