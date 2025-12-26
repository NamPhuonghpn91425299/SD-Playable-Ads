using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets._Develop_.ThanhNT.Scripts.Observer
{
    public class Subject<T> : ISubject<T>, ISubjectBase
    {
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();

        public void Subscribe(IObserver<T> observer)
        {
            if (observer != null && !observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void Unsubscribe(IObserver<T> observer)
        {
            if (observer != null)
            {
                observers.Remove(observer);
            }
        }

        public void NotifyObservers(T data)
        {
            // Tạo copy để tránh modified collection exception
            var observersCopy = new List<IObserver<T>>(observers);

            foreach (var observer in observersCopy)
            {
                // Kiểm tra null trước khi gọi OnNotify
                if (observer == null)
                {
                    observers.Remove(observer);
                    continue;
                }

                try
                {
                    observer.OnNotify(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error notifying observer ({observer?.GetType()?.Name}): {ex.Message}");
                    Debug.LogError($"Stack trace: {ex.StackTrace}");
                    // Nếu observer bị lỗi, remove nó khỏi danh sách
                    observers.Remove(observer);
                }
            }
        }

        public void Clear()
        {
            observers.Clear();
        }

    }
}
