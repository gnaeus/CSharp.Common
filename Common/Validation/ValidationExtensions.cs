using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Common.Validation
{
    public static class ValidationExtensions
    {
        public static IEnumerable<ValidationError> ValidateAttributes(this object model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return model.ValidateAttributes(new HashSet<object>(), "");
        }

        private static IEnumerable<ValidationError> ValidateAttributes(
            this object model, HashSet<object> visitedObjects, string path)
        {
            if (model == null) yield break;

            Type type = model.GetType();

            if (type.IsSimple()) yield break;

            if (visitedObjects.Contains(model))
            {
                yield break;
            }
            else
            {
                visitedObjects.Add(model);
            }

            IDictionary dictionary = model as IDictionary;

            if (dictionary != null)
            {
                IDictionaryEnumerator enumerator = dictionary.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    IEnumerable<ValidationError> errors = enumerator.Value
                        .ValidateAttributes(visitedObjects, MakeDictionaryPath(path, enumerator.Key));

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
                        .ValidateAttributes(visitedObjects, $"{path}[{index++}]");

                    foreach (var error in errors) yield return error;
                }
                yield break;
            }

            if (path != "")
            {
                path += ".";
            }

            foreach (PropertyInfo property in type.GetProperties())
            {
                // object properties without indexed property
                if (property.GetIndexParameters().Any()) continue;

                string propertyPath = path + property.Name;
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
                    .ValidateAttributes(visitedObjects, propertyPath);

                foreach (var error in errors) yield return error;
            }

            IValidatable validatable = model as IValidatable;

            if (validatable != null)
            {
                foreach (var error in validatable.Validate(path)) yield return error;
            }
        }

        private static bool IsSimple(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }

            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal);
        }

        private static string MakeDictionaryPath(string path, object key)
        {
            bool isInt = key is sbyte
                || key is byte
                || key is short
                || key is ushort
                || key is int
                || key is uint
                || key is long
                || key is ulong;

            return isInt
                ? $"{path}[{key}]"
                : $"{path}[\"{key}\"]";
        }
    }
}