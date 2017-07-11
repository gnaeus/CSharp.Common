using System;

namespace Common.Exceptions
{
    internal interface IBusinessException
    {
        object Code { get; }
    }

    /// <summary>
    /// Exception with error code and message that passed to end user of application.
    /// </summary>
    public class BusinessException : Exception, IBusinessException
    {
        public string Code { get; set; }

        object IBusinessException.Code => Code;

        public BusinessException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Exception with error code and message that passed to end user of application.
    /// </summary>
    public class BusinessException<TError> : Exception, IBusinessException
        where TError : struct
    {
        public TError Code { get; set; }

        object IBusinessException.Code => Code;

        public BusinessException(TError code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}
