using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Api;
using Common.Exceptions;

namespace MrArvice.Aspects.Tests.WrapError
{
    [TestClass]
    public class WrapErrorAsyncTest
    {
        #region ApiStatus

        [WrapErrorAsync]
        public async Task<ApiStatus> StatusOk()
        {
            return true;
        }

        [TestMethod]
        public async Task TestApiStatusOk()
        {
            ApiStatus status = await StatusOk();

            Assert.IsNotNull(status);
            Assert.IsTrue(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus> StatusValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public async Task TestApiStatusValidationException()
        {
            ApiStatus status = await StatusValidationException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(1, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus> StatusBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public async Task TestApiStatusBusinessException()
        {
            ApiStatus status = await StatusBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.AreEqual("Code", status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus> StatusGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public async Task TestApiStatusGenericBusinessException()
        {
            ApiStatus status = await StatusGenericBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus> StatusUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task TestApiStatusUnknownException()
        {
            ApiStatus status = await StatusUnknownException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        #endregion

        #region ApiStatus<TError>

        [WrapErrorAsync]
        public async Task<ApiStatus<int>> GenericStatusOk()
        {
            return true;
        }

        [TestMethod]
        public async Task TestGenericApiStatusOk()
        {
            ApiStatus<int> status = await GenericStatusOk();

            Assert.IsNotNull(status);
            Assert.IsTrue(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus<int>> GenericStatusValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public async Task TestGenericApiStatusValidationException()
        {
            ApiStatus<int> status = await GenericStatusValidationException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(1, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus<int>> GenericStatusBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public async Task TestGenericApiStatusBusinessException()
        {
            ApiStatus<int> status = await GenericStatusBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus<int>> GenericStatusGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public async Task TestGenericApiStatusGenericBusinessException()
        {
            ApiStatus<int> status = await GenericStatusGenericBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.AreEqual(-1, status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiStatus<int>> GenericStatusUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task TestGenericApiStatusUnknownException()
        {
            ApiStatus<int> status = await GenericStatusUnknownException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        #endregion

        #region ApiResult<TResult>

        [WrapErrorAsync]
        public async Task<ApiResult<string>> ResultOk()
        {
            return "Ok";
        }

        [TestMethod]
        public async Task TestApiResultOk()
        {
            ApiResult<string> result = await ResultOk();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Ok", result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string>> ResultValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public async Task TestApiResultValidationException()
        {
            ApiResult<string> result = await ResultValidationException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(1, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string>> ResultBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public async Task TestApiResultBusinessException()
        {
            ApiResult<string> result = await ResultBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.AreEqual("Code", result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string>> ResultGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public async Task TestApiResultGenericBusinessException()
        {
            ApiResult<string> result = await ResultGenericBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string>> ResultUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task TestApiResultUnknownException()
        {
            ApiResult<string> result = await ResultUnknownException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        #endregion

        #region ApiResult<TResult, TError>

        [WrapErrorAsync]
        public async Task<ApiResult<string, int>> GenericResultOk()
        {
            return "Ok";
        }

        [TestMethod]
        public async Task TestGenericApiResultOk()
        {
            ApiResult<string, int> result = await GenericResultOk();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Ok", result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string, int>> GenericResultValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public async Task TestGenericApiResultValidationException()
        {
            ApiResult<string, int> result = await GenericResultValidationException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(1, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string, int>> GenericResultBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public async Task TestGenericApiResultBusinessException()
        {
            ApiResult<string, int> result = await GenericResultBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string, int>> GenericResultGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public async Task TestGenericApiResultGenericBusinessException()
        {
            ApiResult<string, int> result = await GenericResultGenericBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.AreEqual(-1, result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapErrorAsync]
        public async Task<ApiResult<string, int>> GenericResultUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task TestGenericApiResultUnknownException()
        {
            ApiResult<string, int> result = await GenericResultUnknownException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        #endregion
    }
}
