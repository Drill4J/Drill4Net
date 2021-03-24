using System;
using System.Collections;
using System.Collections.Generic;

namespace Drill4Net.Target.Common
{
    public class NotEmptyStringEnumerator : IEnumerator<string>
    {
        private readonly string[] _data;
        private int _position = -1;

        /************************************************************/

        public NotEmptyStringEnumerator(string[] data)
        {
            _data = data;
        }

        /************************************************************/

        public string Current
        {
            get
            {
                if (_position == -1 || _position >= _data.Length)
                    throw new InvalidOperationException();
                return _data[_position];
            }
        }

        object IEnumerator.Current => throw new NotImplementedException();

        public bool MoveNext()
        {
            if (_position < _data.Length - 1)
            {
                _position = GetPosition();
                return true;
            }
            else
                return false;
        }

        private int GetPosition()
        {
            var pos = _position;
            while (pos < _data.Length - 1 && string.IsNullOrEmpty(_data[++pos]));
            return pos;
        }

        public void Reset()
        {
            _position = -1;
        }

        public void Dispose() { }
    }
}
