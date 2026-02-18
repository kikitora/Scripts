using UnityEngine;
namespace SteraCube.SpaceJourney {
    [System.Serializable]
    public class ReinEvent
    {
        [SerializeField] int eventAge;
        [SerializeField] string eventText;
        [SerializeField] ReinEventType eventType;
    public ReinEvent(int age, string text, ReinEventType type)
        {
            eventAge = age;
            eventText = text;
            eventType = type;
        }
    } 
}
