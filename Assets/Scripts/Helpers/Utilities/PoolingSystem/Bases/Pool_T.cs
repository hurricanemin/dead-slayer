using System;
using System.Collections.Generic;
using System.Linq;
using Helpers.Utilities.PoolingSystem.Interfaces;
using UnityEngine;

namespace Helpers.Utilities.PoolingSystem.Bases
{
    public abstract class PoolBase<T0> : IPool<T0>, IFactory<T0>
    {
        protected PoolBase(Func<T0> createMethod, Func<T0> disposeMethod, Action<T0> onGet = null,
            Action<T0> onRelease = null,
            Action<T0> onDestroy = null,
            int capacity = 0, int maxSize = 0)
        {
            _hasLimit = maxSize > 0;
            _maxSize = _hasLimit ? capacity > maxSize ? capacity : maxSize : maxSize;
            _createMethod = createMethod;
            _disposeMethod = disposeMethod;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _availableObjects = new Dictionary<int, T0>(_maxSize);
            _busyObjects = new Dictionary<int, T0>(_maxSize);
            _allObjects = new List<T0>(_maxSize);
            for (int i = 0; i < capacity; i++) GetObject();
        }

        private readonly Dictionary<int, T0> _availableObjects;
        private readonly Dictionary<int, T0> _busyObjects;
        private readonly List<T0> _allObjects;

        private readonly Func<T0> _createMethod;
        private readonly Func<T0> _disposeMethod;
        private readonly Action<T0> _onGet;
        private readonly Action<T0> _onRelease;
        private readonly Action<T0> _onDestroy;

        public int AvailableObjectCount => PooledObjectCount - BusyObjectCount;
        public int BusyObjectCount => _busyObjects.Count;
        public int PooledObjectCount => _allObjects.Count;

        private readonly int _maxSize;
        private readonly bool _hasLimit;

        public virtual void ReleaseObject(T0 obj)
        {
            if (obj == null) return;
            int hashCode = obj.GetHashCode();
            if (!_busyObjects.Remove(hashCode)) return;
            _availableObjects.Add(hashCode, obj);
            _onRelease?.Invoke(obj);
        }

        public void DisposeObj(T0 obj)
        {
            if (obj == null) return;
            int hashCode = obj.GetHashCode();
            if (_busyObjects.ContainsKey(hashCode)) _busyObjects.Remove(hashCode);
            else if (_availableObjects.ContainsKey(hashCode)) _availableObjects.Remove(hashCode);
            else return;
            _onDestroy?.Invoke(obj);
            _allObjects.Remove(obj);
            _disposeMethod?.Invoke();
        }

        private void Clear()
        {
            while (PooledObjectCount > 0) DisposeObj(_allObjects[0]);
            _availableObjects.Clear();
            _busyObjects.Clear();
            _allObjects.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public T0 GetObject()
        {
            try
            {
                KeyValuePair<int, T0> lastElement;

                if (_availableObjects.Count == 0)
                {
                    if (_hasLimit && PooledObjectCount >= _maxSize)
                    {
                        int key = _busyObjects.First().Key;
                        DisposeObj(_busyObjects[key]);
                    }

                    T0 instantiatedObject = _createMethod.Invoke();
                    lastElement = new KeyValuePair<int, T0>(instantiatedObject.GetHashCode(), instantiatedObject);
                    _allObjects.Add(lastElement.Value);
                }
                else
                {
                    lastElement = _availableObjects.Last();
                    _availableObjects.Remove(lastElement.Key);
                }

                _busyObjects.Add(lastElement.Key, lastElement.Value);
                _onGet?.Invoke(lastElement.Value);
                return lastElement.Value;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't register object!\n{0}".Format(e));
                return default;
            }
        }
    }
}