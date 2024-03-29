﻿using System;
using System.Collections.Generic;


namespace BloonsClicker.Upgrades.Path2.SlowAreaClasses;

/// <summary>
    /// Generic object pool implementation.
    /// </summary>
    /// <typeparam name="T">Type of the object pool.</typeparam>
    public sealed class ObjectPool<T> : IDisposable where T : class
    {
        internal readonly Stack<T> m_Stack;
        readonly Func<T> m_CreateFunc;
        readonly Action<T> m_ActionOnGet;
        readonly Action<T> m_ActionOnRelease;
        readonly Action<T> m_ActionOnDestroy;
        readonly int m_MaxSize; // Used to prevent catastrophic memory retention.
        internal bool m_CollectionCheck;

        /// <summary>
        /// The total number of active and inactive objects.
        /// </summary>
        public int CountAll { get; private set; }

        /// <summary>
        /// Number of objects that have been created by the pool but are currently in use and have not yet been returned.
        /// </summary>
        public int CountActive { get { return CountAll - CountInactive; } }

        /// <summary>
        /// Number of objects that are currently available in the pool.
        /// </summary>
        public int CountInactive { get { return m_Stack.Count; } }

        /// <summary>
        /// Creates a new ObjectPool.
        /// </summary>
        /// <param name="createFunc">Use to create a new instance when the pool is empty. In most cases this will just be <code>() => new T()</code></param>
        /// <param name="actionOnGet">Called when the instance is being taken from the pool.</param>
        /// <param name="actionOnRelease">Called when the instance is being returned to the pool. This could be used to clean up or disable the instance.</param>
        /// <param name="actionOnDestroy">Called when the element can not be returned to the pool due to it being equal to the maxSize.</param>
        /// <param name="collectionCheck">Collection checks are performed when an instance is returned back to the pool. An exception will be thrown if the instance is already in the pool. Collection checks are only performed in the Editor.</param>
        /// <param name="defaultCapacity">The default capacity the stack will be created with.</param>
        /// <param name="maxSize">The maximum size of the pool. When the pool reaches the max size then any further instances returned to the pool will be ignored and can be garbage collected. This can be used to prevent the pool growing to a very large size.</param>
        public ObjectPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));

            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));

            m_Stack = new Stack<T>(defaultCapacity);
            m_CreateFunc = createFunc;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = m_CreateFunc();
                CountAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            m_ActionOnGet?.Invoke(element);
            return element;
        }

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="element">Object to release.</param>
        public void Release(T element)
        {
            if (m_CollectionCheck && m_Stack.Count > 0 && m_Stack.Contains(element)) 
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");

            m_ActionOnRelease?.Invoke(element);

            if (CountInactive < m_MaxSize)
            {
                m_Stack.Push(element);
            }
            else
            {
                m_ActionOnDestroy?.Invoke(element);
            }
        }

        /// <summary>
        /// Releases all pooled objects so they can be garbage collected.
        /// </summary>
        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
                foreach (var item in m_Stack)
                {
                    m_ActionOnDestroy(item);
                }
            }

            m_Stack.Clear();
            CountAll = 0;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // Ensure we do a clear so the destroy action can be called.
            Clear();
        }
    }