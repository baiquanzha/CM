using System;
using System.Collections;
using System.Collections.Generic;

namespace MTool.Core.Collections
{
    public sealed class FastStack<T> : IEnumerable<T> where T : class
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const int DefaultCapacity = 4;
        private const int ResizingFactor = 2;
        private T[] _buffer;

        /// <summary>
        /// element count
        /// </summary>
        private int _numberOfItems;


        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        public int Count => _numberOfItems;
        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------


        public FastStack() : this(DefaultCapacity)
        {
        }

        public FastStack(int capacity)
        {
            _buffer = new T[capacity];
            _numberOfItems = 0;
        }


        public void Clear()
        {
            Array.Clear(_buffer, 0, _numberOfItems);
            _numberOfItems = 0;
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public bool Contains(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _numberOfItems; i++)
            {
                if (comparer.Equals(_buffer[i], item))
                    return true;
            }

            return false;
        }

        public void Push(T item)
        {
            int len = _buffer.Length;

            if (_numberOfItems == len)
            {
                T[] newBuffer = new T[ResizingFactor * len];
                Array.Copy(_buffer, 0, newBuffer, 0, _buffer.Length);
                _buffer = newBuffer;
            }

            _buffer[_numberOfItems] = item;
            _numberOfItems++;
        }

        public T Pop()
        {
            if (_numberOfItems == 0)
            {
                return null;
            }

            _numberOfItems--;
            T item = _buffer[_numberOfItems];
            _buffer[_numberOfItems] = default(T);
            return item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)new FastStackEnumator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)new FastStackEnumator(this);
        }

        #endregion

        #region Inner class

        public struct FastStackEnumator : IEnumerator<T>, IEnumerator
        {
            private FastStack<T> stack;
            private int index;
            private T current;

            internal FastStackEnumator(FastStack<T> stack)
            {
                this.stack = stack;
                this.index = 0;
                this.current = default(T);
            }

            public T Current => this.current;

            object IEnumerator.Current => (object)this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                FastStack<T> list = this.stack;

                if ((uint)this.index >= (uint)list._numberOfItems)
                    return this.MoveNextRare();

                this.current = list._buffer[this.index];
                ++this.index;
                return true;
            }

            private bool MoveNextRare()
            {
                this.index = this.stack._numberOfItems + 1;
                this.current = default(T);
                return false;
            }


            void IEnumerator.Reset()
            {
                this.index = 0;
                this.current = default(T);
            }

        }
        #endregion
    }
}
