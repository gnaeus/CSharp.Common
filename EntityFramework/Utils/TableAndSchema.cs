namespace EntityFramework.Common.Utils
{
    /// <summary>
    /// Structure that represents table name and schema.
    /// </summary>
    public struct TableAndSchema
    {
        public string TableName;
        public string Schema;

        public TableAndSchema(string table, string schema)
        {
            TableName = table;
            Schema = schema;
        }

        public void Deconstruct(out string table, out string schema)
        {
            table = TableName;
            schema = Schema;
        }
    }
}
