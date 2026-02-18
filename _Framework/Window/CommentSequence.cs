using System;
using System.Collections.Generic;
using UnityEngine;

public enum StepType
{
    Comment,            // テキストだけ（未指定は既定値を使用）
    SetCommentDefaults, // 以降に使う既定値を更新（空で保存すると既定値も空に）
    SetAndComment,      // この1行だけで全部指定（既定値は使わない／空はそのまま空）
    ShowImage,
    HideImage,
    ShowChoices,
}

[CreateAssetMenu(fileName = "CommentSequence", menuName = "Comment/Sequence", order = 0)]
public class CommentSequence : ScriptableObject
{
    [Serializable]
    public class ChoiceItem
    {
        public string label;
        public int value;
    }

    [Serializable]
    public class Step
    {
        public StepType type = StepType.Comment;

        // 共通で使う可能性がある項目
        public string targetSlotName = "";

        // Comment 系で使う
        public string header = "";
        [TextArea] public string text = "";

        // コメントと同時に出す画像（任意）
        public string linkedImageSlotName = "";
        public Sprite linkedImageSprite;

        // ShowImage 専用
        public Sprite image;

        // ShowChoices 専用
        public List<ChoiceItem> choices = new List<ChoiceItem>();
    }

    public List<Step> steps = new List<Step>();
}
