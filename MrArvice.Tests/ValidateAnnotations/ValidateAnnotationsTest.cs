using Common.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using ValidationException = Common.Exceptions.ValidationException;

namespace MrArvice.Aspects.Tests.ValidateAnnotations
{
    [TestClass]
    public class ValidateAnnotationsTest
    {
        public class Model
        {
            [Required]
            public string Title { get; set; }

            [StringLength(5)]
            public string Text { get; set; }
        }

        [ValidateAnnotations]
        public void Method(Model first)
        {
        }
        
        [TestMethod]
        public void TestValidateAnnotationsValid()
        {
            Model model = new Model { Title = "test", Text = "test" };

            Method(model);
        }

        [TestMethod, ExpectedException(typeof(ValidationException))]
        public void TestValidateAnnotationsInvalid()
        {
            Model model = new Model { Title = null, Text = "test test" };

            try
            {
                Method(model);
            }
            catch (ValidationException exception)
            {
                Assert.AreEqual(2, exception.Errors.Length);

                Assert.AreEqual("Title", exception.Errors[0].PropertyPath);
                Assert.AreEqual("Required", exception.Errors[0].ErrorCode);
                Assert.AreEqual("Text", exception.Errors[1].PropertyPath);
                Assert.AreEqual("StringLength", exception.Errors[1].ErrorCode);

                throw;
            }
        }

        [ValidateAnnotations(ArgumentNames = true)]
        public void Method(Model first, Model second)
        {
        }

        [TestMethod]
        public void TestValidateAnnotationsArgumentsValid()
        {
            Model model = new Model { Title = "test", Text = "test" };

            Method(model, model);
        }

        [TestMethod, ExpectedException(typeof(ValidationException))]
        public void TestValidateAnnotationsArgumentsInvalid()
        {
            Model model = new Model { Title = null, Text = "test test" };

            try
            {
                Method(model, model);
            }
            catch (ValidationException exception)
            {
                Assert.AreEqual(4, exception.Errors.Length);

                Assert.AreEqual("first.Title", exception.Errors[0].PropertyPath);
                Assert.AreEqual("Required", exception.Errors[0].ErrorCode);
                Assert.AreEqual("first.Text", exception.Errors[1].PropertyPath);
                Assert.AreEqual("StringLength", exception.Errors[1].ErrorCode);

                Assert.AreEqual("second.Title", exception.Errors[2].PropertyPath);
                Assert.AreEqual("Required", exception.Errors[2].ErrorCode);
                Assert.AreEqual("second.Text", exception.Errors[3].PropertyPath);
                Assert.AreEqual("StringLength", exception.Errors[3].ErrorCode);

                throw;
            }
        }

        [WrapError, ValidateAnnotations]
        public ApiResult<string> CombineAspectsMethod(Model argument)
        {
            return "Ok";
        }

        [TestMethod]
        public void TestWrapErrorValidateAnnotations()
        {
            Model model = new Model { Title = null, Text = "test test" };

            ApiResult<string> result = CombineAspectsMethod(model);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(2, result.ValidationErrors.Length);
        }
    }
}
