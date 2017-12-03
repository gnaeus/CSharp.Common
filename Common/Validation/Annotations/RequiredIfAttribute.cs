using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Common.Validation.Annotations
{
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    /// <summary>
    /// Specifies that a data field value is required only if other boolean property is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;
        private readonly object _expectedValue;

        /// <summary>
        /// Should value be omitted or not, when condition is failed.
        /// </summary>
        public bool Strict { get; set; } = false;

        /// <summary>
        /// Is default(T) value allowed or not.
        /// </summary>
        public bool AllowDefaultValues { get; set; } = true;

        /// <summary>
        /// Is empty string value allowed or not.
        /// </summary>
        public bool AllowEmptyStrings { get; set; } = false;

        /// <param name="otherProperty">Name of the other property in model</param>
        /// <param name="expectedValue">The expected value of the other property</param>
        public RequiredIfAttribute(string otherProperty, object expectedValue = null)
        {
            _otherProperty = otherProperty;

            // default value for boolean property
            _expectedValue = expectedValue ?? true;

            ErrorMessage = $"The {{0}} field is required {(Strict ? "only " : "")}when {otherProperty} field is {_expectedValue}.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object otherValue = GetOtherValue(validationContext);

            if (Equals(_expectedValue, otherValue))
            {
                if (ValueIsEmpty(value))
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }
            }
            else
            {
                if (Strict && !ValueIsEmpty(value))
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }
            }

            return ValidationResult.Success;
        }

        private object GetOtherValue(ValidationContext validationContext)
        {
            Type modelType = validationContext.ObjectType;

            PropertyInfo property = modelType.GetProperty(_otherProperty);

            if (property != null)
            {
                return property.GetValue(validationContext.ObjectInstance, null);
            }

            MethodInfo method = modelType.GetMethod(_otherProperty);

            if (method != null)
            {
                return method.Invoke(validationContext.ObjectInstance, parameters: null);
            }

            throw new InvalidOperationException(
                $"Type '{modelType}' does not contain property or method '{_otherProperty}'");
        }

        private bool ValueIsEmpty(object value)
        {
            return value == null
                || !AllowEmptyStrings && value as string == String.Empty
                || !AllowDefaultValues && value == Activator.CreateInstance(value.GetType());
        }
    }
}
