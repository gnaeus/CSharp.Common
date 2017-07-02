using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static Common.Validation.ValidationHelper;

namespace Common.Validation
{
    public static class ValidationExtensions
    {
        public static IEnumerable<ValidationError> ValidateAnnotations(
            this object model, string path = null)
        {
            return model.ValidateAnnotations(new HashSet<object>(), path);
        }

        private static IEnumerable<ValidationError> ValidateAnnotations(
            this object model, HashSet<object> visitedObjects, string path)
        {
            if (model == null) yield break;

            Type type = model.GetType();

            if (IsSimpleType(type)) yield break;

            if (visitedObjects.Contains(model)) yield break;

            visitedObjects.Add(model);

            IDictionary dictionary = model as IDictionary;

            if (dictionary != null)
            {
                IDictionaryEnumerator enumerator = dictionary.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    IEnumerable<ValidationError> errors = enumerator.Value
                        .ValidateAnnotations(visitedObjects, MakeDictionaryPath(path, enumerator.Key));

                    foreach (var error in errors) yield return error;
                }
                yield break;
            }

            IEnumerable enumerable = model as IEnumerable;

            if (enumerable != null)
            {
                int index = 0;
                foreach (object item in enumerable)
                {
                    IEnumerable<ValidationError> errors = item
                        .ValidateAnnotations(visitedObjects, $"{path}[{index++}]");

                    foreach (var error in errors) yield return error;
                }
                yield break;
            }
            
            foreach (PropertyInfo property in type.GetProperties())
            {
                // object properties without indexed property
                if (property.GetIndexParameters().Any()) continue;

                string propertyPath = CombinePath(path, property.Name);
                object propertyValue = property.GetValue(model);

                IEnumerable<ValidationAttribute> attributes = property
                    .GetCustomAttributes(typeof(ValidationAttribute), true)
                    .Cast<ValidationAttribute>();

                foreach (var attribute in attributes)
                {
                    if (!attribute.IsValid(propertyValue))
                    {
                        string attributeName = attribute.GetType().Name;

                        yield return new ValidationError(
                            propertyPath,
                            attributeName.Remove(attributeName.Length - 9),
                            attribute.FormatErrorMessage(property.Name));

                        break;
                    }
                }

                IEnumerable<ValidationError> errors = propertyValue
                    .ValidateAnnotations(visitedObjects, propertyPath);

                foreach (var error in errors) yield return error;
            }

            IEnumerableValidatable validatable = model as IEnumerableValidatable;

            if (validatable != null)
            {
                foreach (var error in validatable.Validate())
                {
                    error.PropertyPath = CombinePath(path, error.PropertyPath);

                    yield return error;
                }
            }
        }
    }
}