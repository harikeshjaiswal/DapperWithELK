using System.Runtime.CompilerServices;

namespace DapperWithELK.Common.Dapper
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CustomColumn : Attribute
    {
        public CustomColumn([CallerLineNumber] int order = 0, [CallerMemberName] string name = "")
        {
            Order = order;
            Primary = false;
            Identity = false;
            Ignore = false;
            ColumnName = name;
        }

        public int Order { get; private set; }
        public bool Primary { get; set; }
        public bool Identity { get; set; }
        public bool Ignore { get; set; }
        public string ColumnName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomTable : Attribute
    {
        public CustomTable([CallerMemberName] string name = "")
        {
            TableName = name;
        }
        public string TableName { get; set; }
    }
}
