using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Helpers;

namespace Common.Tests.Helpers
{
    [TestClass]
    public class SqlFullTextSearchHeplerTest
    {
        [TestMethod]
        public void TestPrepareFullTextQuery()
        {
            string query = SqlFullTextSearchHepler.PrepareFullTextQuery("Я на Cолнышке лежу", true);

            Debug.Write(query);

            Assert.AreEqual(
                "\"cолнышке*\" NEAR \"лежу*\"\n OR FORMSOF(FREETEXT, \"cолнышке\") AND FORMSOF(FREETEXT, \"лежу\")",
                query
            );
        }

        [TestMethod]
        public void TestPrepareFullTextQueryEmptyQuery()
        {
            Assert.IsNull(SqlFullTextSearchHepler.PrepareFullTextQuery(null));
            Assert.IsNull(SqlFullTextSearchHepler.PrepareFullTextQuery(""));
            Assert.IsNull(SqlFullTextSearchHepler.PrepareFullTextQuery("  \r\n\t "));
            Assert.IsNull(SqlFullTextSearchHepler.PrepareFullTextQuery("я он в на за"));
        }
        
        [TestMethod]
        public void TestPrepareFullTextQueryMaxLength()
        {
            string phrase = new String('A', 2048);

            string query = SqlFullTextSearchHepler.PrepareFullTextQuery(phrase);

            Assert.AreEqual(1027, query.Length);
        }
    }
}
