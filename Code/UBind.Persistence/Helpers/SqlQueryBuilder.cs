// <copyright file="SqlQueryBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Helpers
{
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using NodaTime;

    /// <summary>
    /// Builds sql queries.
    /// </summary>
    public class SqlQueryBuilder
    {
        private string _tableName;
        private int? _top;
        private List<(string columnName, string dataType)> _columnDataTypeDictionary = new List<(string columnName, string dataType)>();

        public SqlQueryBuilder(string tableName)
        {
            this._tableName = tableName;
        }

        public SqlQueryBuilder SetResultCount(int? resultCount)
        {
            this._top = resultCount;
            return this;
        }

        public SqlQueryBuilder SetColumnDataTypes(List<(string ColumnName, string DataType)> columnDataTypeDictionary)
        {
            this._columnDataTypeDictionary = columnDataTypeDictionary;
            return this;
        }

        /// <summary>
        /// Build the sql query.
        /// </summary>
        /// <param name="expression">The expression to be converted to sql.</param>
        /// <returns>Sql query string.</returns>
        public string Build(Expression expression)
        {
            var whereClause = this.ConstructWhereClause(expression);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"SELECT{(this._top == null ? string.Empty : (" TOP " + this._top.Value.ToString()))} * FROM {this._tableName}"
                   + (string.IsNullOrEmpty(whereClause)
                       ? string.Empty
                       : " WHERE " + whereClause)
                   + ";");

            return sb.ToString();
        }

        public string ConstructWhereClause(Expression expression)
        {
            return this.CreateQuery(expression);
        }

        private string CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                return string.Empty;
            }

            if (expression is LambdaExpression lambda)
            {
                return this.CreateQuery(lambda.Body);
            }
            else if (expression is BinaryExpression binary)
            {
                var left = this.CreateQuery(binary.Left);
                var right = this.CreateQuery(binary.Right);
                right = this.ConvertRight(left, right);
                var op = this.GetSqlOperator(binary.NodeType);

                return $"({left} {op} {right})";
            }
            else if (expression is MemberExpression member)
            {
                return member.Member.Name;
            }
            else if (expression is UnaryExpression unary)
            {
                if (unary.NodeType == ExpressionType.Convert)
                {
                    return this.CreateQuery(unary.Operand);
                }

                return this.CreateQuery(unary.Operand);
            }
            else if (expression is TryExpression tryExpression)
            {
                var result = this.CreateQuery(tryExpression.Body);
                return result;
            }
            else if (expression is ConstantExpression constant)
            {
                var val = constant.Value.ToString();
                val = this.CleanString(val);
                if (constant.Type == typeof(string)
                    || constant.Type == typeof(LocalTime))
                {
                    return $"'{val}'";
                }

                // creates a SQL query that will always return true or false
                // a way to convert 'True' or 'False' values inside the expression.
                if (constant.Type == typeof(bool))
                {
                    if ((bool)constant.Value)
                    {
                        return $"1=1";
                    }

                    return $"1!=1";
                }

                return val;
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                var methodCall = methodCallExpression.Method;

                if (methodCall.ReflectedType.Name == "PocoPathLookupResolver")
                {
                    var arg2 = (methodCallExpression.Arguments[1] as ConstantExpression).Value;
                    return arg2.ToString().Trim('/');
                }

                if (methodCall.ReflectedType.Name == "ProviderResultExtensions")
                {
                    var providerResult = methodCallExpression.Arguments.FirstOrDefault() as MethodCallExpression;
                    var arg2 = (providerResult.Arguments[1] as ConstantExpression).Value;
                    return arg2.ToString().Trim('/');
                }

                var result = string.Empty;
                if (methodCallExpression.Object != null)
                {
                    result = this.CreateQuery(methodCallExpression.Object);
                }

                var argument = methodCallExpression.Arguments.FirstOrDefault();
                var argStr = argument != null ? argument.ToString().Trim('\\', '"') : string.Empty;
                switch (methodCall)
                {
                    case MethodInfo a when a.ToString().Contains(nameof(string.StartsWith)):
                        return $"({result} LIKE '{argStr}%')";
                    case MethodInfo a when a.ToString().Contains(nameof(string.EndsWith)):
                        return $"({result} LIKE '%{argStr}')";
                    case MethodInfo a when a.ToString().Contains(nameof(string.Contains)):
                        return $"({result} LIKE '%{argStr}%')";
                    case MethodInfo a when a.Name == "ToLocalTimeFromIso8601OrhmmttOrhhmmttOrhhmmssttWithCulture":
                    case MethodInfo b when b.Name == "FromDateTimeOffset":
                    case MethodInfo c when c.Name == "Parse":
                    case MethodInfo d when d.Name == "ToLocalDateFromIso8601OrDateTimeIso8601":
                        return this.CreateQuery(argument);
                    default:
                        return result;
                }
            }

            // Handle other expression types (method calls, unary expressions, etc.)
            throw new NotImplementedException("Expression type not supported");
        }

        /// <summary>
        /// Convert the right side of the expression to the correct format for the column type.
        /// </summary>
        /// <param name="left">left operand.</param>
        /// <param name="right">righ operand.</param>
        /// <returns>a cast of the value.</returns>
        private string ConvertRight(string left, string right)
        {
            var column = this._columnDataTypeDictionary.LastOrDefault(x => left.Equals(x.columnName, StringComparison.CurrentCultureIgnoreCase));
            if (column.dataType != null)
            {
                switch (column.dataType)
                {
                    case "date":
                    case "datetime2":
                        if (long.TryParse(right, out long unixTicks))
                        {
                            DateTime unixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            DateTime resultDateTime = unixEpochStart.AddTicks(unixTicks);
                            var newResult = resultDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                            return $"CAST('{newResult}' AS {column.dataType.ToUpper()})";
                        }
                        return right;
                }
            }

            return right;
        }

        private string GetSqlOperator(ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.LessThan => "<",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                ExpressionType.And => "AND",
                ExpressionType.Or => "OR",

                // Add other operators as needed
                _ => throw new NotImplementedException("Operator not supported")
            };
        }

        private string CleanString(string input)
        {
            // replace ' to '' to escape single quotes
            return input.Replace("'", "''");
        }
    }
}
