using System.Collections.Generic;

namespace Common.Validation
{
    public interface IEnumerableValidatable
    {
        IEnumerable<ValidationError> Validate();
    }
}
