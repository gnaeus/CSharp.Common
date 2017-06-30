using System;

namespace Common.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; set; }

        public BusinessException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }

    public class BusinessException<TError> : Exception
        where TError : struct
    {
        public TError Code { get; set; }

        public BusinessException(TError code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}
