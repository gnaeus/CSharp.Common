using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Api;
using Common.Exceptions;

namespace MrArvice.Aspects.Tests.WrapError
{
    [TestClass]
    public class WrapErrorTest
    {
        #region ApiStatus

        [WrapError]
        public ApiStatus StatusOk()
        {
            return true;
        }

        [TestMethod]
        public void TestApiStatusOk()
        {
            ApiStatus status = StatusOk();

            Assert.IsNotNull(status);
            Assert.IsTrue(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus StatusValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public void TestApiStatusValidationException()
        {
            ApiStatus status = StatusValidationException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(1, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus StatusBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public void TestApiStatusBusinessException()
        {
            ApiStatus status = StatusBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.AreEqual("Code", status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus StatusGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public void TestApiStatusGenericBusinessException()
        {
            ApiStatus status = StatusGenericBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus StatusUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestApiStatusUnknownException()
        {
            ApiStatus status = StatusUnknownException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        #endregion

        #region ApiStatus<TError>

        [WrapError]
        public ApiStatus<int> GenericStatusOk()
        {
            return true;
        }

        [TestMethod]
        public void TestGenericApiStatusOk()
        {
            ApiStatus<int> status = GenericStatusOk();

            Assert.IsNotNull(status);
            Assert.IsTrue(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus<int> GenericStatusValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public void TestGenericApiStatusValidationException()
        {
            ApiStatus<int> status = GenericStatusValidationException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.IsNull(status.ErrorMessage);
            Assert.AreEqual(1, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus<int> GenericStatusBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public void TestGenericApiStatusBusinessException()
        {
            ApiStatus<int> status = GenericStatusBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus<int> GenericStatusGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public void TestGenericApiStatusGenericBusinessException()
        {
            ApiStatus<int> status = GenericStatusGenericBusinessException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.AreEqual(-1, status.ErrorCode);
            Assert.AreEqual("Message", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        [WrapError]
        public ApiStatus<int> GenericStatusUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestGenericApiStatusUnknownException()
        {
            ApiStatus<int> status = GenericStatusUnknownException();

            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsSuccess);
            Assert.IsNull(status.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", status.ErrorMessage);
            Assert.AreEqual(0, status.ValidationErrors.Length);
        }

        #endregion

        #region ApiResult<TResult>

        [WrapError]
        public ApiResult<string> ResultOk()
        {
            return "Ok";
        }

        [TestMethod]
        public void TestApiResultOk()
        {
            ApiResult<string> result = ResultOk();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Ok", result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string> ResultValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public void TestApiResultValidationException()
        {
            ApiResult<string> result = ResultValidationException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(1, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string> ResultBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public void TestApiResultBusinessException()
        {
            ApiResult<string> result = ResultBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.AreEqual("Code", result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string> ResultGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public void TestApiResultGenericBusinessException()
        {
            ApiResult<string> result = ResultGenericBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string> ResultUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestApiResultUnknownException()
        {
            ApiResult<string> result = ResultUnknownException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("The method or operation is not implemented.", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        #endregion

        #region ApiResult<TResult, TError>

        [WrapError]
        public ApiResult<string, int> GenericResultOk()
        {
            return "Ok";
        }

        [TestMethod]
        public void TestGenericApiResultOk()
        {
            ApiResult<string, int> result = GenericResultOk();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Ok", result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string, int> GenericResultValidationException()
        {
            throw new ValidationException("Path", "Code", "Message");
        }

        [TestMethod]
        public void TestGenericApiResultValidationException()
        {
            ApiResult<string, int> result = GenericResultValidationException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(1, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string, int> GenericResultBusinessException()
        {
            throw new BusinessException("Code", "Message");
        }

        [TestMethod]
        public void TestGenericApiResultBusinessException()
        {
            ApiResult<string, int> result = GenericResultBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string, int> GenericResultGenericBusinessException()
        {
            throw new BusinessException<int>(-1, "Message");
        }

        [TestMethod]
        public void TestGenericApiResultGenericBusinessException()
        {
            ApiResult<string, int> result = GenericResultGenericBusinessException();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.AreEqual(-1, result.ErrorCode);
            Assert.AreEqual("Message", result.ErrorMessage);
            Assert.AreEqual(0, result.ValidationErrors.Length);
        }

        [WrapError]
        public ApiResult<string, int> GenericResultUnknownException()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestGenericApiResultUnknownException()
        {
            ApiResult<string, int> result = GenericResultUnknownException();

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
