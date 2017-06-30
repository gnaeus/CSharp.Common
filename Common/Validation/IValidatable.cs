using System.Collections.Generic;

namespace Common.Validation
{
    public interface IValidatable
    {
        IEnumerable<ValidationError> Validate(string prefix);
    }
}
