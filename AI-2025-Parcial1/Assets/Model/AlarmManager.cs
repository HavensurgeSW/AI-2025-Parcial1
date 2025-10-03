using System;
using UnityEngine;

namespace KarplusParcial1.Management
{
    public class AlarmManager
    {
        public static event Action OnAlarmRaised;
        public static event Action OnAlarmCleared;
        public static bool IsAlarmActive { get; private set; } = false;
        public void RaiseAlarm()
        {
            OnAlarmRaised?.Invoke();

        }
        public void ClearAlarm()
        {
            OnAlarmCleared?.Invoke();
        }
    }
}