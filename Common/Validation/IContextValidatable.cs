namespace Common.Validation
{
    public interface IContextValidatable
    {
        void Validate(IValidationContext validationContext);
    }
}
