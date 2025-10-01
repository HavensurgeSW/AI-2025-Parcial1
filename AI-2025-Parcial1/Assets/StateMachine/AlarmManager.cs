using System;
using UnityEngine;

namespace KarplusParcial1.Management
{
    public class AlarmManager : MonoBehaviour
    {
        public static event Action OnAlarmRaised;
        public static event Action OnAlarmCleared;
        public static bool IsAlarmActive { get; private set; } = false;
        public void RaiseAlarm()
        {
            OnAlarmRaised?.Invoke();
            Debug.Log("Alarm raised");
        }

        public void ClearAlarm()
        {
            OnAlarmCleared?.Invoke();
            Debug.Log("Alarm cleared");
        }
    }
}