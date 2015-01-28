using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabasePool
{
    public class Pool<T>
    {
        protected Dictionary<string, T> Container = new Dictionary<string, T>();

        public virtual T Checkout(string key)
        {
            if (Container.ContainsKey(key))
            {
                return Container[key];
            }
            return default(T);
        }

        public virtual void Checkin(string key, T o)
        {
            if (Container.ContainsKey(key))
            {
                Container[key] = o;
            }
        }

        public virtual void Create(string key, T o)
        {
            if (!Container.ContainsKey(key))
            {
                Container[key] = o;
            }
        }
    }
}
