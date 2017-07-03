using System;

namespace Common.Exceptions
{
    /// <summary>
    /// Exception with error code and message that passed to end user of application.
    /// </summary>
    public class BusinessException : Exception
    {
        public string Code { get; set; }

        public BusinessException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Exception with error code and message that passed to end user of application.
    /// </summary>
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
