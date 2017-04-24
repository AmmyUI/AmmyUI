namespace Ammy.VisualStudio.Service.Extensions
{
    internal class OneSlotStack<TKey, TValue>
    {
        public bool IsFull => _stackTop >= StackSize - 1;

        private const int StackSize = 100;

        private readonly TKey[] _keys = new TKey[StackSize];
        private readonly TValue[] _values = new TValue[StackSize];
        private readonly object _sync = new object();
        private int _stackTop = -1;

        public void Push(TKey key, TValue value)
        {
            if (IsFull)
                return;

            lock (_sync) {
                _stackTop++;

                _keys[_stackTop] = key;
                _values[_stackTop] = value;

                // Find values with the same keys and 'remove' them
                for (var i = 0; i < _stackTop; i++)
                    if (Equals(_keys[i], key))
                        _values[i] = default(TValue);
            }
        }

        public TValue Pop()
        {
            lock (_sync) {
                if (_stackTop == -1)
                    return default(TValue);

                while (_stackTop >= 0)
                    if (!Equals(_values[_stackTop], default(TValue)))
                        return _values[_stackTop--];
                    else
                        _stackTop--;

                return default (TValue);
            }
        }
    }
}