using System;
using System.Collections.Generic;

namespace Common.Validation
{
    public interface IValidationContext
    {
        string CurrentPath { get; }
        List<ValidationError> Errors { get; }
        bool HasErrors { get; }
        void ThrowIfHasErrors();
        void AddError(string path, string code, string message);
        void ValidateAnnotations(object model, string path = null);
        IDisposable WithPrefix(string path);
    }
}
