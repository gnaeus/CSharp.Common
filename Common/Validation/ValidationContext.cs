using System;
using System.Collections.Generic;
using Common.Exceptions;

namespace Common.Validation
{
    public class ValidationContext
    {
        string _currentPath = "";

        List<ValidationError> _errors = new List<ValidationError>();

        static string Combine(string path, string prefix)
        {
            if (String.IsNullOrEmpty(path))
            {
                return prefix ?? "";
            }
            if (String.IsNullOrEmpty(prefix))
            {
                return path ?? "";
            }
            if (Char.IsLetter(prefix[0]))
            {
                return path + "." + prefix;
            }
            return path + prefix;
        }
        
        public void AddError(string path, string code, string message)
        {
            _errors.Add(new ValidationError(Combine(_currentPath, path), code, message));
        }
        
        public void ThrowIfHasErrors()
        {
            if (_errors.Count > 0)
            {
                throw new ValidationException(_errors.ToArray());
            }
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
                _previousPath = _context._currentPath;
                _context._currentPath = Combine(_context._currentPath, path);
            }

            public void Dispose()
            {
                _context._currentPath = _previousPath;
            }
        } 
    }
}
