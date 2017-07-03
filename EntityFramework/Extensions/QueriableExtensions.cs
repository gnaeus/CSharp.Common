using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace EntityFramework.Common.Extensions
{
    public static class QueriableExtensions
    {
        /// <summary>
        /// Converts a query with big IN clause to multiple queries with smaller IN clausesand combines the results
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TParameter">Type of IN parameter</typeparam>
        /// <param name="baseQuery">Base query without IN</param>
        /// <param name="inValues">Values that need to be used in IN clause</param>
        /// <param name="propertyGetter">Property that needs to be checked against IN (like a => a.ID)</param>
        /// <param name="chunkSize">Size of chunk</param>
        public static async Task<IEnumerable<TResult>> LoadContainsQueryChunkedAsync<TResult, TParameter>(
            this IQueryable<TResult> baseQuery,
            Expression<Func<TResult, TParameter>> propertyGetter,
            IEnumerable<TParameter> inValues,
            int chunkSize = 500)
        {
            List<TResult> result = new List<TResult>();
            Queue<TParameter> paramQueue = new Queue<TParameter>(inValues);

            while (paramQueue.Count != 0)
            {
                List<TParameter> chunkParams = new List<TParameter>();
                for (int i = 0; i < chunkSize && paramQueue.Count != 0; i++)
                {
                    chunkParams.Add(paramQueue.Dequeue());
                }

                List<TResult> chunkReqult = await baseQuery
                    .Where(CreateWhereExpression(propertyGetter, chunkParams))
                    .ToListAsync();

                result.AddRange(chunkReqult);
            }

            return result;
        }

        /// <summary>
        /// Constructs obj => inValues.Contains(obj.inPropertyName) expression to use with Where() function
        /// </summary>        
        private static Expression<Func<TResult, bool>> CreateWhereExpression<TResult, TParameter>(
            Expression<Func<TResult, TParameter>> propertyGetter,
            IEnumerable<TParameter> inValues)
        {
            // contains extension method from LINQ
            MethodInfo containsMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "Contains" && m.GetParameters().Count() == 2)
                .MakeGenericMethod(typeof(TParameter));

            return Expression.Lambda<Func<TResult, bool>>(
                Expression.Call(null, containsMethod, Expression.Constant(inValues), propertyGetter.Body),
                propertyGetter.Parameters[0]
            );
        }
    }
}
