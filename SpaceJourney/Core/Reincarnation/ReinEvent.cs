using UnityEngine;
namespace SteraCube.SpaceJourney
{
    [System.Serializable]
    public class ReinEvent
    {
        [SerializeField] int eventAge;
        [SerializeField] string eventText;
        [SerializeField] ReinEventType eventType;
        [SerializeField] bool hideAge;

        public ReinEvent(int age, string text, ReinEventType type, bool hideAge = false)
        {
            eventAge = age;
            eventText = text;
            eventType = type;
            this.hideAge = hideAge;
        }

        public int Age => eventAge;
        public string Text => eventText;
        public ReinEventType EventType => eventType;

        /// <summary>trueのとき年齢を表示しない（スキル習得など補足行に使用）</summary>
        public bool HideAge => hideAge;
    }
}