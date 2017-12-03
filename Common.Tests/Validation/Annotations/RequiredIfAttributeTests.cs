using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Validation.Annotations;

namespace Common.Tests.Validation.Annotations
{
    [TestClass]
    public class RequiredIfAttributeTests
    {
        enum UserRole { None, Moderator, Admin }

        class User
        {
            public bool HasProfile { get; set; }

            [RequiredIf(nameof(HasProfile), Strict = true)]
            public UserProfile Profile { get; set; }

            public UserRole Role { get; set; }

            [RequiredIf(nameof(Role), UserRole.Admin, Strict = true)]
            public AdminOptions AdminOptions { get; set; }
        }

        class UserProfile { }

        class AdminOptions { }

        [TestMethod]
        public void RequiredWhenPropertyIsTrue()
        {
            var user = new User
            {
                HasProfile = true,
                Profile = new UserProfile(),
            };

            Assert.IsTrue(TestHelper.TryValidateObject(user));

            user.Profile = null;

            Assert.IsFalse(TestHelper.TryValidateObject(user));
        }

        [TestMethod]
        public void ForbiddenWhenPropertyIsFalse()
        {
            var user = new User
            {
                HasProfile = false,
                Profile = new UserProfile(),
            };

            Assert.IsFalse(TestHelper.TryValidateObject(user));

            user.Profile = null;

            Assert.IsTrue(TestHelper.TryValidateObject(user));
        }

        [TestMethod]
        public void RequiredWhenPropertyHasSameValue()
        {
            var user = new User
            {
                Role = UserRole.Admin,
                AdminOptions = new AdminOptions(),
            };

            Assert.IsTrue(TestHelper.TryValidateObject(user));

            user.AdminOptions = null;

            Assert.IsFalse(TestHelper.TryValidateObject(user));
        }

        [TestMethod]
        public void ForbiddenWhenPropertyHasDifferentValue()
        {
            var user = new User
            {
                Role = UserRole.Moderator,
                AdminOptions = new AdminOptions(),
            };

            Assert.IsFalse(TestHelper.TryValidateObject(user));

            user.AdminOptions = null;

            Assert.IsTrue(TestHelper.TryValidateObject(user));
        }
    }
}
