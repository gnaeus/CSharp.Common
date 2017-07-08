using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static Common.Validation.ValidationHelper;

namespace Common.Validation
{
    public class ValidationContext : IValidationContext
    {
        public string CurrentPath { get; private set; } = "";

        public List<ValidationError> Errors { get; } = new List<ValidationError>();

        public bool HasErrors => Errors.Count > 0;

        /// <exception cref="Exceptions.ValidationException" />
        public void ThrowIfHasErrors()
        {
            if (Errors.Count > 0)
            {
                throw new Exceptions.ValidationException(Errors.ToArray());
            }
        }

        public void AddError(string path, string code, string message)
        {
            Errors.Add(new ValidationError(CombinePath(CurrentPath, path), code, message));
        }
        
        public IDisposable WithPrefix(string path)
        {
            return new PrefixDisposable(this, path);
        }

        private struct PrefixDisposable : IDisposable
        {
            string _previousPath;
            ValidationContext _context;

            public PrefixDisposable(ValidationContext context, string path)
            {
                _context = context;
                _previousPath = _context.CurrentPath;
                _context.CurrentPath = CombinePath(_context.CurrentPath, path);
            }

            public void Dispose()
            {
                _context.CurrentPath = _previousPath;
            }
        }

        public void ValidateAnnotations(object model, string path = null)
        {
            ValidateAnnotations(new HashSet<object>(), model, path);
        }

        private void ValidateAnnotations(HashSet<object> visitedObjects, object model, string path)
        {
            if (model == null) return;

            Type type = model.GetType();

            if (IsSimpleType(type)) return;

            if (visitedObjects.Contains(model)) return;

            visitedObjects.Add(model);

            IDictionary dictionary = model as IDictionary;

            if (dictionary != null)
            {
                IDictionaryEnumerator enumerator = dictionary.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    ValidateAnnotations(
                        visitedObjects, enumerator.Value, MakeDictionaryPath(path, enumerator.Key));
                }
                return;
            }

            IEnumerable enumerable = model as IEnumerable;

            if (enumerable != null)
            {
                int index = 0;
                foreach (object item in enumerable)
                {
                    ValidateAnnotations(visitedObjects, item, $"{path}[{index++}]");
                }
                return;
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

                        AddError(
                            propertyPath,
                            attributeName.Remove(attributeName.Length - 9),
                            attribute.FormatErrorMessage(property.Name));

                        // accept error from only first invalid annotation per property
                        break;
                    }
                }

                ValidateAnnotations(visitedObjects, propertyValue, propertyPath);
            }

            IContextValidatable validatable = model as IContextValidatable;

            if (validatable != null)
            {
                using (WithPrefix(path))
                {
                    validatable.Validate(this);
                }
            }
        }
    }
}
