using System.Data.SqlClient;
using Common.Api;
using Common.Exceptions;

partial class _Examples
{
    class Model { }

    enum ErrorCodes { GeneralError }

    class WebService
    {
        readonly ApiWrapper _apiWrapper;
        readonly ApplicationService _applicationService;

        public ApiResult<Model, ErrorCodes> DoSomething(Model argument)
        {
            return _apiWrapper.Execute<Model, ErrorCodes>(() =>
            {
                return _applicationService.DoSomething(argument);
            });
        }
    }
    
    class ApplicationService
    {
        public Model DoSomething(Model argument)
        {
            if (argument == null)
            {
                throw new ValidationException(
                    nameof(argument), "Required", $"Argument {nameof(argument)} is required");
            }
            try
            {
                // do something
                return argument;
            }
            catch (SqlException)
            {
                throw new BusinessException<ErrorCodes>(
                    ErrorCodes.GeneralError, "Something went wrong, please try again");
            }
        }
    }
}
