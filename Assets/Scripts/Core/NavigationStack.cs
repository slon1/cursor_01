using System.Collections.Generic;

namespace Game.Core
{
    public sealed class NavigationStack<T>
    {
        readonly Stack<T> _items = new Stack<T>();

        public int Count => _items.Count;

        public void Push(T item) => _items.Push(item);

        public T Peek() => _items.Peek();

        public T Pop() => _items.Pop();

        public void Clear() => _items.Clear();
    }
}
