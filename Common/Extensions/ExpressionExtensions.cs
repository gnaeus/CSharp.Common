using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Common.Extensions
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Get computed value of Expression.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static T GetValue<T>(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.Constant)
            {
                return (T)((ConstantExpression)expression).Value;
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)expression;

                if (member.Expression.NodeType == ExpressionType.Constant)
                {
                    var instance = (ConstantExpression)member.Expression;

                    if (instance.Type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute)))
                    {
                        return (T)instance.Type
                            .GetField(member.Member.Name)
                            .GetValue(instance.Value);
                    }
                }
            }

            // we can't interpret the expression but we can compile and run it
            return Expression.Lambda<Func<T>>(expression).Compile().Invoke();
        }

        /// <summary>
        /// Get computed value of Expression.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static object GetValue(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)expression;

                if (member.Expression.NodeType == ExpressionType.Constant)
                {
                    var instance = (ConstantExpression)member.Expression;

                    if (instance.Type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute)))
                    {
                        return instance.Type
                            .GetField(member.Member.Name)
                            .GetValue(instance.Value);
                    }
                }
            }

            // we can't interpret the expression but we can compile and run it
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            return getterLambda.Compile().Invoke();
        }
    }
}
