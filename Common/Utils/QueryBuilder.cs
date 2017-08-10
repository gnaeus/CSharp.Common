using System;
using System.Text;

namespace Common.Utils
{
    public class QueryBuilder
    {
        readonly StringBuilder _builder = new StringBuilder();

        public Action<string> Append;

        public QueryBuilder()
        {
            Append = sql => _builder.AppendLine(sql);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        public void Deconstruct(out QueryBuilder query, out Action<string> append)
        {
            query = this;
            append = Append;
        }
    }
}
