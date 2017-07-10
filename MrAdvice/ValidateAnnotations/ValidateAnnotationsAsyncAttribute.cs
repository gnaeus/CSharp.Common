using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;
using Common.Exceptions;
using Common.Validation;

namespace MrArvice.Aspects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateAnnotationsAsyncAttribute : Attribute, IMethodAsyncAdvice
    {
        public bool ArgumentNames { get; set; }

        public Task Advise(MethodAsyncAdviceContext context)
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (ArgumentNames)
            {
                ParameterInfo[] parameters =  context.TargetMethod.GetParameters();

                int i = 0;
                foreach (object argument in context.Arguments)
                {
                    errors.AddRange(argument.ValidateAnnotations(parameters[i++].Name));
                }
            }
            else
            {
                foreach (object argument in context.Arguments)
                {
                    errors.AddRange(argument.ValidateAnnotations());
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors.ToArray());
            }

            return context.ProceedAsync();
        }
    }
}