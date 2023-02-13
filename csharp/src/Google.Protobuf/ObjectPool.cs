using System;
using System.Collections.Generic;

namespace Google.Protobuf
{
    public interface IObjectPool<T> where T : IMessage<T>
    {
        T Get();
        void Release(T t);
    }


    public interface IPoolItem
    {
        void Clear();

        void Recycle();
    }

    /// <summary>
    /// 消息池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageObjectPool<T> : IObjectPool<T> where T : IMessage<T>, IPoolItem, new()
    {
        private readonly Queue<T> objectPoolQueue;

        /// <summary>
        /// 消息池构造函数，从manager获取初始化池数量
        /// </summary>
        /// <param name="poolCount"></param>
        public MessageObjectPool(int poolCount = 10)
        {
            var type = typeof(T);
            var count = MessageObjectPoolManager.GetInitCount(type);
            if (count != -1)
            {
                poolCount = count;
            }

            objectPoolQueue = new Queue<T>(poolCount);

            if (MessageObjectPoolManager.getObjectPoolCount)
            {
                MessageObjectPoolManager.BindGetObjectPoolCountFuc(type, () => objectPoolQueue.Count);
            }

            for (int i = 0; i < poolCount; i++)
            {
                objectPoolQueue.Enqueue(new T());
            }
        }

        public T Get()
        {
            if (objectPoolQueue.Count <= 0)
            {
                return new T();
            }
            var t = objectPoolQueue.Dequeue();
            return t;
        }

        public void Release(T t)
        {
            t.Clear();
            objectPoolQueue.Enqueue(t);
        }
    }

    public static class MessageObjectPoolManager
    {
        private static Dictionary<Type, int> _InitPoolCount;

        public static readonly Dictionary<Type, Func<int>> objectPoolFucDic = new();

        public static bool getObjectPoolCount;

        /// <summary>
        /// 获取初始化池数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetInitCount(Type type)
        {
            if (_InitPoolCount != null && _InitPoolCount.TryGetValue(type, out var count))
            {
                return count;
            }

            return -1;
        }


        public static void BindGetObjectPoolCountFuc(Type type, Func<int> func)
        {
            objectPoolFucDic[type] = func;
        }

        /// <summary>
        /// 设置初始化池数量
        /// </summary>
        public static void SetInitCount(Dictionary<Type, int> dictionary)
        {
            _InitPoolCount = dictionary;
        }
    }
}