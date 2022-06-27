using System;
using System.Collections.Generic;

namespace Devcoons.Misc
{
    public class ListExt<T> : List<T>
    {
        public event EventHandler OnAdd;
        public event EventHandler OnRemove;
        public event EventHandler OnRemoveAt;
        public event EventHandler OnClear;

        public new void Add(T item)
        {
            base.Add(item);
            OnAdd?.Invoke(this, null);
        }

        public new void Remove(T item)
        {
            base.Add(item);
            OnRemove?.Invoke(this, null);
        }
        public new void RemoveAt(int pos)
        {
            base.RemoveAt(pos);
            OnRemoveAt?.Invoke(this, null);
        }

        public new void Clear()
        {
            base.Clear();
            OnClear?.Invoke(this, null);
        }
    }
}
