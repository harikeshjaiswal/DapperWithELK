using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DapperWithELK.Common.Dapper
{
    public class SqlVisitor : ExpressionVisitor
    {
        private StringBuilder _builder;
        private List<object> _parameters;

        public SqlVisitor()
        {
            _builder = new StringBuilder();
            _parameters = new List<object>();
        }

        public (string, List<object>) GetSqlAndParameters(Expression expression)
        {
            _builder.Clear();
            _parameters.Clear();
            Visit(expression);
            return (_builder.ToString(), _parameters);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _builder.Append("(");
            this.Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _builder.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _builder.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    if (IsNullConstant(node.Right))
                    {
                        _builder.Append(" IS NULL)");
                        return node;
                    }
                    else
                    {
                        _builder.Append(" = ");
                    }
                    break;
                case ExpressionType.NotEqual:
                    if (IsNullConstant(node.Right))
                    {
                        _builder.Append(" IS NOT NULL)");
                        return node;
                    }
                    else
                    {
                        _builder.Append(" != ");
                    }
                    break;
                case ExpressionType.GreaterThan:
                    _builder.Append(" > ");
                    break;
                case ExpressionType.LessThan:
                    _builder.Append(" < ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _builder.Append(" >= ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _builder.Append(" <= ");
                    break;
                default:
                    throw new NotSupportedException($"Binary operator '{node.NodeType}' is not supported");
            }
            this.Visit(node.Right);
            _builder.Append(")");
            return node;
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
            {
                switch (node.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        _builder.Append(node.Member.Name);
                        return node;
                    case ExpressionType.Constant:
                    case ExpressionType.MemberAccess:
                        var lambda = Expression.Lambda(node);
                        var compiled = lambda.Compile();
                        var value = compiled.DynamicInvoke();
                        if (value != null)
                            _parameters.Add(value);

                        _builder.Append("@p" + (_parameters.Count - 1));
                        return node;
                }
            }
            throw new NotSupportedException($"Expression '{node}' is not supported");
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value != null)
                _parameters.Add(node.Value);
            else
                _parameters.Add(DBNull.Value);

            _builder.Append("@p" + (_parameters.Count - 1));
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string) && node.Method.Name == "Contains")
            {
                this.Visit(node.Object);
                _builder.Append(" LIKE ");

                var constantExpression = node.Arguments[0] as ConstantExpression;
                if (constantExpression != null)
                {
                    _parameters.Add("%" + constantExpression.Value + "%");
                    _builder.Append("@p" + (_parameters.Count - 1));
                }
                else
                {
                    throw new NotSupportedException($"Argument of method call '{node.Method.Name}' must be a constant expression");
                }
            }
            else if (node.Method.DeclaringType == typeof(Enumerable) && node.Method.Name == "Contains")
            {
                this.Visit(node.Arguments[1]);
                _builder.Append(" IN ");
                this.Visit(node.Arguments[0]);
            }
            else if (node.Method.DeclaringType != null && node.Method.DeclaringType.IsGenericType && node.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>) && node.Method.Name == "Contains")
            {
                this.Visit(node.Arguments[0]);
                _builder.Append(" IN ");
                this.Visit(node.Object);
            }
            else
            {
                throw new NotSupportedException($"Unsupported method call: {node.Method.Name}");
            }

            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            _builder.Append("(");
            for (int i = 0; i < node.Expressions.Count; i++)
            {
                this.Visit(node.Expressions[i]);
                if (i < node.Expressions.Count - 1)
                {
                    _builder.Append(", ");
                }
            }
            _builder.Append(")");
            return node;
        }

        private bool IsNullConstant(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                var constantExpression = (ConstantExpression)expression;
                return constantExpression.Value == null;
            }

            return false;
        }
    }
}