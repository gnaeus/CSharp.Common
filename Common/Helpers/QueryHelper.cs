using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Common.Helpers
{
    public static class QueryHelper
    {
        private static readonly Regex WhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// <para>Build query for SQL Server FTS Engine CONTAINS function.</para>
        /// <para>Result should be passed through ADO.NET SqlParameter due to preventing SQL Injection.</para>
        /// <para>If <paramref name="searchPhrase"/> is null or whitespace or contains only words
        /// shorter than <paramref name="minWordLength"/> letters then null would be returned.</para>
        /// <para>If length of <paramref name="searchPhrase"/> is greater than 1024
        /// then it would be truncated to 1024 symbols.</para>
        /// </summary>
        /// <param name="searchPhrase"> User input </param>
        /// <param name="fuzzy"> Search for inflectional forms </param>
        public static string PrepareFullTextQuery(
            string searchPhrase, bool fuzzy = false, int minWordLength = 3
        ) {
            if (String.IsNullOrWhiteSpace(searchPhrase)) {
                return null;
            }

            const int maxPhraseLength = 1024;
            
            if (searchPhrase.Length > maxPhraseLength) {
                searchPhrase = searchPhrase.Substring(0, maxPhraseLength);
            }
                        
            searchPhrase = searchPhrase.ToLowerInvariant();

            List<string> words = WhiteSpace.Split(searchPhrase)
                .Where(w => w.Length >= minWordLength).ToList();

            if (words.Count == 0) {
                return null;
            }

            string query = String.Join(" NEAR ",
                words.Select(w => "\"" + w + "*\"")
            );

            if (fuzzy) {
                query += "\n OR " + String.Join(" AND ",
                    words.Select(w => "FORMSOF(FREETEXT, \"" + w + "\")")
                );
            }

            return query;
        }

        /// <summary>
        /// Converts a query with big IN clause to multiple queries with smaller IN clauses and combines the results
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TParameter">Type of IN parameter</typeparam>
        /// <param name="baseQuery">Base query without IN</param>
        /// <param name="inValues">Values that need to be used in IN clause</param>
        /// <param name="propertyGetter">Property that needs to be checked against IN (like a => a.ID)</param>
        public static IEnumerable<TResult> ExecuteChunkedInQuery<TResult, TParameter>(
            IQueryable<TResult> baseQuery,
            IEnumerable<TParameter> inValues,
            Expression<Func<TResult, TParameter>> propertyGetter
        ) {
            List<TResult> res = new List<TResult>();
            int chunkSize = 500;
            Queue<TParameter> paramQueue = new Queue<TParameter>(inValues);

            while (paramQueue.Count != 0)
            {
                List<TParameter> chunkParams = new List<TParameter>();
                for (int i = 0; i < chunkSize && paramQueue.Count != 0; i++)
                {
                    chunkParams.Add(paramQueue.Dequeue());
                }
                res.AddRange(baseQuery.Where(CreateWhereExpression(chunkParams, propertyGetter)));
            }
            return res;
        }

        /// <summary>
        /// Constructs obj => inValues.Contains(obj.inPropertyName) expression to use with Where function
        /// </summary>        
        private static Expression<Func<TResult, bool>> CreateWhereExpression<TResult, TParameter>(
            IEnumerable<TParameter> inValues,
            Expression<Func<TResult, TParameter>> propertyGetter
        ) {
            //contains extension method from LINQ
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
