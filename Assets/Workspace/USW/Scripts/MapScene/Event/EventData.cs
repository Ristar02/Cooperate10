using UnityEngine;

namespace Map
{
    public enum EventOutcome
    {
        Success,    
        Failure,    
        Declined   
    }

    [CreateAssetMenu(menuName = "Map/Event Data")]
    public class EventData : ScriptableObject
    {
        public string _eventTitle;
        
        [TextArea(3, 10)]
        public string _eventDescription;
        
        [Header("Energy Cost")]
        public int _energyCost = 1; // 선택지중 Yes 할시 코스트 드는거.
        
        [Header("Success ")]
        [TextArea(3, 10)]
        public string _successText;
        public int _successReward; // 성공 시 보상 
        
        [Header("Failure ")]
        [TextArea(3, 10)]
        public string _failureText;
        public int _failurePenalty; // 실패 시 추가 패널티 
        
        [Header("Declined (Nvm 선택)")]
        [TextArea(3, 10)]
        public string _declinedText;
    }
}