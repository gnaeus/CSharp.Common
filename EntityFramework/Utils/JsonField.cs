using System;
using Jil;

namespace EntityFramework.Common.Utils
{
    public struct JsonField<TValue>
        where TValue : class
    {
        private TValue _value;
        private string _json;
        private bool _isMaterialized;
        private bool _hasDefault;

        public string Json
        {
            get
            {
                if (_isMaterialized)
                {
                    _json = JSON.Serialize(_value);
                }
                return _json;
            }
            set
            {
                _json = value;
                _isMaterialized = false;
            }
        }

        public TValue Value
        {
            get
            {
                if (!_isMaterialized)
                {
                    if (String.IsNullOrEmpty(_json))
                    {
                        if (_hasDefault)
                        {
                            _hasDefault = false;
                        }
                        else
                        {
                            _value = null;
                        }
                    }
                    else
                    {
                        _value = JSON.Deserialize<TValue>(_json);
                    }
                    _isMaterialized = true;
                }
                return _value;
            }
            set
            {
                _value = value;
                _isMaterialized = true;
            }
        }

        public static implicit operator JsonField<TValue>(TValue value)
        {
            var field = new JsonField<TValue>();

            field._value = value;
            field._hasDefault = true;

            return field;
        }
    }
}
