using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
namespace DapperWithELK.Common.Dapper
{
    /// <summary>
    /// Dynamic query class.
    /// </summary>
    public sealed class DynamicQuery
    {
        /// <summary>
        /// Gets the insert query.
        /// </summary>
        /// <param name="context">System.Data.Entity.DbContext object.</param>
        /// <param name="item">The item.</param>        
        /// <returns>
        /// The Sql query based on the item properties.
        /// </returns>
        public static string GetInsertQuery<T>(T item)
        {
            var tableName = typeof(T).Name;           
            var tb = typeof(T).GetCustomAttributes(typeof(CustomTable), false).OfType<CustomTable>().FirstOrDefault();
            if (tb != null)
                tableName = tb.TableName;

            var pList = EntityToSqlData.GetProperties<T>();
            var props = pList.Select(s => new { Property = s, CustomAttributeData = s.GetCustomAttributes(typeof(CustomColumn), true).OfType<CustomColumn>().FirstOrDefault() }).Where(s => s.CustomAttributeData != null);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var columns = props.Where(p => !p.CustomAttributeData.Identity && !p.CustomAttributeData.Ignore).Select(s => new { ColumnName = s.CustomAttributeData.ColumnName, PropertyName = s.Property.Name });
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var identityColumn = props.Where(p => p.CustomAttributeData.Identity).Select(s => s.CustomAttributeData.ColumnName).FirstOrDefault();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return string.Format("INSERT INTO {0} ({1}) {2} VALUES (@{3})",
                                 tableName,
                                 string.Join(",", columns.Select(s => s.ColumnName)),
                                 (!string.IsNullOrWhiteSpace(identityColumn)) ? "OUTPUT inserted." + identityColumn : "",
                                 string.Join(",@", columns.Select(s => s.PropertyName)));
        }

        /// <summary>
        /// Gets the update query.
        /// </summary>
        /// <param name="context">System.Data.Entity.DbContext object.</param>
        /// <param name="item">The item.</param>
        /// <param name="conditionColumns"></param>
        /// <param name="columnsToUpdate"></param>
        /// <returns>
        /// The Sql query based on the item properties.
        /// </returns>
        public static string GetUpdateQuery<T>(T item)
        {
            var tableName = typeof(T).Name;
            var tb = typeof(T).GetCustomAttributes(typeof(CustomTable), false).OfType<CustomTable>().FirstOrDefault();
            if (tb != null)
                tableName = tb.TableName;

            var pList = EntityToSqlData.GetProperties<T>();

            var props = pList.Select(s => new { Property = s, CustomAttributeData = s.GetCustomAttributes(typeof(CustomColumn), true).OfType<CustomColumn>().FirstOrDefault() }).Where(s => s.CustomAttributeData != null);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var updateFields = props.Where(p => !p.CustomAttributeData.Identity && !p.CustomAttributeData.Ignore && !p.CustomAttributeData.Primary)
                                  .Select(s => s.CustomAttributeData.ColumnName + "=@" + s.Property.Name).ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var identityColumn = props.Where(p => p.CustomAttributeData.Identity).Select(s => s.CustomAttributeData.ColumnName).FirstOrDefault();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var condition = props.Where(p => p.CustomAttributeData.Primary)
                                .Select(s => new { ColumnName = s.CustomAttributeData.ColumnName, PropertyName = s.Property.Name })
                                .Select(s => s.ColumnName + "=@" + s.PropertyName).ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, string.Join(",", updateFields), string.Join(" AND ", condition));
        }

        public static string GetDeleteQuery<T>(T item)
        {
            var tableName = typeof(T).Name;
            var tb = typeof(T).GetCustomAttributes(typeof(CustomTable), false).OfType<CustomTable>().FirstOrDefault();
            if (tb != null)
                tableName = tb.TableName;

            var pList = EntityToSqlData.GetProperties<T>();

            var props = pList.Select(s => new { Property = s, CustomAttributeData = s.GetCustomAttributes(typeof(CustomColumn), true).OfType<CustomColumn>().FirstOrDefault() });

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var condition = props.Where(p => p.CustomAttributeData.Primary)
                                .Select(s => new { ColumnName = s.CustomAttributeData.ColumnName, PropertyName = s.Property.Name })
                                .Select(s => s.ColumnName + "=@" + s.PropertyName).ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return string.Format("DELETE FROM {0} WHERE {1}", tableName, string.Join(" AND ", condition));
        }

        public static QueryResult GetDynamicQuery<T>(Expression<Func<T, bool>> expression)
        {
            var tableName = typeof(T).Name;
            var tb = typeof(T).GetCustomAttributes(typeof(CustomTable), false).OfType<CustomTable>().FirstOrDefault();
            if (tb != null)
                tableName = tb.TableName;

            return GetDynamicQuery<T>(tableName, expression);
        }

        public static QueryResult GetDynamicCountQuery<T>(Expression<Func<T, bool>> expression)
        {
            var tableName = typeof(T).Name;
            var tb = typeof(T).GetCustomAttributes(typeof(CustomTable), false).OfType<CustomTable>().FirstOrDefault();
            if (tb != null)
                tableName = tb.TableName;

            return GetDynamicQuery<T>(tableName, expression);
        }

        /// <summary>
        /// Gets the dynamic query.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>A result object with the generated sql and dynamic params.</returns>
        public static QueryResult GetDynamicCountQuery<T>(string tableName, Expression<Func<T, bool>> expression)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT Count(1) FROM ");
            builder.Append(tableName);
            builder.Append(" WHERE ");

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            IDictionary<string, Object> expando = new ExpandoObject();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.


            SqlVisitor visitor = new SqlVisitor();
            var res = visitor.GetSqlAndParameters(expression);
            builder.Append(res.Item1);

            for (int i = 0; i < res.Item2.Count; i++)
            {
                expando[$"p{i}"] = res.Item2[i];
            }

            return new QueryResult(builder.ToString().TrimEnd(), expando);
        }

        /// <summary>
        /// Gets the dynamic query.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>A result object with the generated sql and dynamic params.</returns>
        public static QueryResult GetDynamicQuery<T>(string tableName, Expression<Func<T, bool>> expression)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT * FROM ");
            builder.Append(tableName);
            builder.Append(" WHERE ");

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            IDictionary<string, Object> expando = new ExpandoObject();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.


            SqlVisitor visitor = new SqlVisitor();
            var res = visitor.GetSqlAndParameters(expression);
            builder.Append(res.Item1);

            for (int i = 0; i < res.Item2.Count; i++)
            {
                expando[$"p{i}"] = res.Item2[i];
            }

            return new QueryResult(builder.ToString().TrimEnd(), expando);

            #region Old Code
            /*
            var queryProperties = new List<QueryParameter>();
            var body = (BinaryExpression)expression.Body;           

            // walk the tree and build up a list of query parameter objects
            // from the left and right branches of the expression tree
            WalkTree(body, ExpressionType.Default, ref queryProperties);

            // convert the query parms into a SQL string and dynamic property object
            

            for (int i = 0; i < queryProperties.Count(); i++)
            {
                QueryParameter item = queryProperties[i];

                if (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0)
                {
                    builder.Append(string.Format("{0} {1} {2} @{1} ", item.LinkingOperator, item.PropertyName,
                                                 item.QueryOperator));
                }
                else
                {
                    builder.Append(string.Format("{0} {1} @{0} ", item.PropertyName, item.QueryOperator));
                }

                expando[item.PropertyName] = item.PropertyValue;
            }

            return new QueryResult(builder.ToString().TrimEnd(), expando);
            */
            #endregion
        }

        /// <summary>
        /// Walks the tree.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="linkingType">Type of the linking.</param>
        /// <param name="queryProperties">The query properties.</param>
        private static void WalkTree(BinaryExpression body, ExpressionType linkingType,
                                     ref List<QueryParameter> queryProperties)
        {
            if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
            {
                string propertyName = GetPropertyName(body);
                dynamic propertyValue = body.Right;
                string opr = GetOperator(body.NodeType);
                string link = GetOperator(linkingType);

                try
                {
                    queryProperties.Add(new QueryParameter(link, propertyName, propertyValue.Value, opr));
                }
                catch (Exception)
                {
                    object val = GetValue((MemberExpression)propertyValue);
                    queryProperties.Add(new QueryParameter(link, propertyName, val, opr));
                }
            }
            else
            {
                WalkTree((BinaryExpression)body.Left, body.NodeType, ref queryProperties);
                WalkTree((BinaryExpression)body.Right, body.NodeType, ref queryProperties);
            }
        }

        private static object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>The property name for the property expression.</returns>
        private static string GetPropertyName(BinaryExpression body)
        {
            string propertyName = body.Left.ToString().Split(new char[] { '.' })[1];

            if (body.Left.NodeType == ExpressionType.Convert)
            {
                // hack to remove the trailing ) when converting.
                propertyName = propertyName.Replace(")", string.Empty);
            }

            return propertyName;
        }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The expression types SQL server equivalent operator.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private static string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Default:
                    return string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Class that models the data structure in converting the expression tree into SQL and Params.
    /// </summary>
    internal class QueryParameter
    {
        public string LinkingOperator { get; set; }
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
        public string QueryOperator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParameter" /> class.
        /// </summary>
        /// <param name="linkingOperator">The linking operator.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="queryOperator">The query operator.</param>
        internal QueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator)
        {
            this.LinkingOperator = linkingOperator;
            this.PropertyName = propertyName;
            this.PropertyValue = propertyValue;
            this.QueryOperator = queryOperator;
        }
    }
}
