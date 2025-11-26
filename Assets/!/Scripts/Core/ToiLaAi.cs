using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPattern.Observer
{
    public class ToiLaAi<T> where T : Enum
    {
        private static ToiLaAi<T> _instance;

        public static ToiLaAi<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ToiLaAi<T>();
                }
                return _instance;
            }
        }

        private Dictionary<T, Action<object>> _events = new Dictionary<T, Action<object>>();

        private ToiLaAi() { }

        public void DangKy(T eventID, Action<object> callback)
        {
            if (callback == null)
            {
                Debug.LogWarning($"Callback cho sự kiện '{eventID}' là NULL.");
                return;
            }

            if (eventID == null)
            {
                Debug.LogWarning($"EventID là NULL. Không thể đăng ký sự kiện.");
                return;
            }

            if (!_events.TryAdd(eventID, callback))
            {
                _events[eventID] += callback;
            }
        }

        public void HuyDangKy(T eventID, Action<object> callback)
        {
            if (_events.ContainsKey(eventID))
            {
                _events[eventID] -= callback;
                if (_events[eventID] == null)
                {
                    _events.Remove(eventID);
                }
            }
            else
            {
                Debug.LogWarning($"Sự kiện '{eventID}' không tìm thấy trong ObserverManager<{typeof(T).Name}>");
            }
        }

        public void HuyDangKyTatCa()
        {
            _events.Clear();
        }

        public void PhatSuKien(T eventID, object param = null)
        {
            if (!_events.ContainsKey(eventID))
            {
                Debug.LogWarning($"Sự kiện '{eventID}' không có người nghe trong ObserverManager<{typeof(T).Name}>");
                return;
            }

            if (_events[eventID] == null)
            {
                Debug.LogWarning($"Callback cho sự kiện '{eventID}' là NULL.");
                _events.Remove(eventID);
                return;
            }

            _events[eventID]?.Invoke(param);
        }
    }
}