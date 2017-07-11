using System.Data.SqlClient;
using Common.Api;
using Common.Exceptions;
using Common.MethodMiddleware;
using static Common.Api.ApiHelper;

partial class _Examples
{
    class Model { }

    enum ErrorCodes { GeneralError }

    class WebService
    {
        readonly MethodDecorator _methodDecorator;
        readonly ApplicationService _applicationService;

        public WebService(ApplicationService applicationService)
        {
            _applicationService = applicationService;

            _methodDecorator = new MethodDecorator()
                .Use(new WrapExceptionMiddleware());
        }

        public ApiResult<Model, ErrorCodes> DoSomething(Model argument)
        {
            return _methodDecorator.Execute(new { argument }, () =>
            {
                return _applicationService.DoSomething(argument);
            });
        }

        public ApiResult<Model, ErrorCodes> DoSomethingElse(Model argument)
        {
            if (argument == null)
            {
                return Error(ErrorCodes.GeneralError, $"Argument {nameof(argument)} is required");
            }
            return Ok(new Model());
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
