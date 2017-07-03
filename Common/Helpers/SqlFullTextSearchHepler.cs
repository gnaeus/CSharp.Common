using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Helpers
{
    public static class SqlFullTextSearchHepler
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
    }
}
