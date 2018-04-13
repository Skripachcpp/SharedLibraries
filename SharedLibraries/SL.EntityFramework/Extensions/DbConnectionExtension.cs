using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dapper;
using Microsoft.SqlServer.Server;
using WorkingTools.Extensions;

namespace SL.EntityFramework
{
    public static class DbConnectionExtension
    {
        public static Dictionary<string, string> QueryCache = new Dictionary<string, string>();

        public static IEnumerable<T> Merge<T>(this DbConnection cnt, IEnumerable<T> entities, Expression<Func<T, object>>[] mgrBy, bool insert = true, bool update = true, bool outAllFromDb = true)
        {
            if (mgrBy == null) throw new ArgumentNullException(nameof(mgrBy));
            if (mgrBy.Length <= 0) throw new ArgumentOutOfRangeException(nameof(mgrBy));

            var entityType = typeof(T);
            var entityTypeName = $"[{entityType.Name}]";
            var entityFields = entityType.GetFieldsInfo().ToArray();
            var entityFieldsNames = entityFields.Select(a => $"[{a.Name}]").ToArray();
            var magreByFields = mgrBy.Select(a => a.GetInfo());
            var magreByFieldsNames = magreByFields.Select(a => $"[{a.Name}]").ToArray();

            string sQuery;
            var key = $"{entityTypeName}|{str.Join(",", entityFieldsNames)}|{str.Join(",", magreByFieldsNames)}|{insert}|{update}|{outAllFromDb}";
            if (QueryCache.ContainsKey(key))
            {
                sQuery = QueryCache[key];
            }
            else
            {

                var entityFieldsJoinByComma = str.Join(", ", entityFieldsNames);

                var sbQuery = new StringBuilder();

                sbQuery.Append("SET NOCOUNT ON; \n");
                if (insert || update)
                {
                    sbQuery.Append("MERGE ");
                    sbQuery.Append(entityTypeName);
                    sbQuery.Append(" AS trg \nUSING \n(");
                    entityType.BuildQueryTableFromJson(sbQuery);
                    sbQuery.Append(") \nAS src (");
                    sbQuery.Append(entityFieldsJoinByComma);
                    sbQuery.Append(") ON (");

                    var first = true;
                    foreach (var name in magreByFieldsNames)
                    {
                        if (first) first = false;
                        else sbQuery.Append(" AND ");

                        sbQuery.Append("trg.");
                        sbQuery.Append(name);
                        sbQuery.Append(" = src.");
                        sbQuery.Append(name);
                    }

                    sbQuery.Append(") \n");

                    if (update)
                    {
                        sbQuery.Append("WHEN MATCHED THEN UPDATE SET ");
                        first = true;
                        foreach (var name in entityFieldsNames)
                        {
                            if (first) first = false;
                            else sbQuery.Append(", ");

                            sbQuery.Append("trg.");
                            sbQuery.Append(name);
                            sbQuery.Append(" = CASE WHEN src.");
                            sbQuery.Append(name);
                            sbQuery.Append(" IS NOT NULL THEN src.");
                            sbQuery.Append(name);
                            sbQuery.Append(" ELSE trg.");
                            sbQuery.Append(name);
                            sbQuery.Append(" END");
                        }
                    }

                    if (insert)
                    {
                        sbQuery.Append("\nWHEN NOT MATCHED THEN INSERT (");
                        sbQuery.Append(entityFieldsJoinByComma);
                        sbQuery.Append(")");

                        sbQuery.Append(" VALUES (");

                        int index = -1;
                        var addComma = false;
                        var anyAdded = false;
                        foreach (var name in entityFieldsNames)
                        {
                            index++;

                            if (!addComma) addComma = true;
                            else sbQuery.Append(", ");

                            if (string.Equals(name, "[id]", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var type = entityFields[index].Type;
                                if (type == typeof(Guid))
                                {
                                    sbQuery.Append("ISNULL(src.");
                                    sbQuery.Append(name);
                                    sbQuery.Append(", NEWID())");

                                    anyAdded = true;
                                    continue;
                                }

                                if (type == typeof(int))
                                {
                                    if (anyAdded == false)
                                        addComma = false;

                                    anyAdded = true;
                                    continue;
                                }
                            }

                            sbQuery.Append("src.");
                            sbQuery.Append(name);

                            anyAdded = true;
                        }
                        sbQuery.Append(") ");
                    }
                }

                if ((outAllFromDb && insert && update) || (!outAllFromDb))
                {
                    sbQuery.Append("\nOUTPUT inserted.*;");
                }
                else if (outAllFromDb)
                {
                    //закончить merge
                    if (insert || update) sbQuery.Append(";\n");

                    sbQuery.Append("\nSELECT trg.* FROM \n(");
                    entityType.BuildQueryTableFromJson(sbQuery);
                    sbQuery.Append(") src\n");
                    sbQuery.Append("LEFT JOIN ");
                    sbQuery.Append(entityTypeName);
                    sbQuery.Append(" trg ON ");
                    var any = false;
                    foreach (var name in magreByFieldsNames)
                    {
                        if (!any) any = true;
                        else sbQuery.Append(" AND ");

                        sbQuery.Append("trg.");
                        sbQuery.Append(name);
                        sbQuery.Append(" = src.");
                        sbQuery.Append(name);
                    }
                }

                sQuery = sbQuery.ToString();
                QueryCache.Add(key, sQuery);
            }

            var jsonEntities = entities.ToArray().ToJson();

            var items = cnt.Query<T>(sQuery, new { JSONData = jsonEntities });

            return items;
        }
    }

    public static class DbQueryExtension
    {
        public static StringBuilder BuildQueryTableFromJson(this Type type, StringBuilder sbQuery = null, string paramName = "JSONData")
        {
            if (sbQuery == null) sbQuery = new StringBuilder();

            sbQuery.Append("SELECT ");

            var fields = type.GetFieldsInfo().ToArray();

            bool any = false;
            foreach (var field in fields)
            {
                if (!any) any = true;
                else sbQuery.Append(", ");

                if (field.Type == typeof(Guid))
                {
                    sbQuery.Append("\nCASE WHEN [");
                    sbQuery.Append(field.Name);
                    sbQuery.Append("] != '00000000-0000-0000-0000-000000000000' THEN [");
                    sbQuery.Append(field.Name);
                    sbQuery.Append("] ELSE NULL END as [");
                    sbQuery.Append(field.Name);
                    sbQuery.Append("]");
                }
                else
                {
                    sbQuery.Append("\n[");
                    sbQuery.Append(field.Name);
                    sbQuery.Append("] AS [");
                    sbQuery.Append(field.Name);
                    sbQuery.Append("]");
                }
            }

            sbQuery.Append("");
            sbQuery.Append("FROM OPENJSON ( @JSONData) WITH (");


            any = false;
            foreach (var field in fields)
            {
                if (!any) any = true;
                else sbQuery.Append(", ");

                sbQuery.Append("\n[");
                sbQuery.Append(field.Name);
                sbQuery.Append("]  ");

                var sqlType = field.Type.ToSqlType();
                sbQuery.Append(sqlType);

                sbQuery.Append(" '$.");
                sbQuery.Append(field.Name);
                sbQuery.Append("'");
            }

            sbQuery.Append(")");

            return sbQuery;
        }

        public static StringBuilder BuildQueryTableFromXml(this Type type, StringBuilder sbQuery = null, string paramName = "XMLData")
        {
            if (sbQuery == null) sbQuery = new StringBuilder();

            sbQuery.Append("SELECT ");

            var fields = type.GetFieldsInfo();
            bool any = false;
            foreach (var field in fields)
            {
                if (!any) any = true;
                else sbQuery.Append(", ");

                sbQuery.Append("\n[");
                sbQuery.Append(field.Name);
                sbQuery.Append("] = ");

                var sqlType = field.Type.ToSqlType();
                if (string.Equals(sqlType, "UNIQUEIDENTIFIER", StringComparison.CurrentCultureIgnoreCase))
                {
                    sbQuery.Append("CASE WHEN Node.Data.value('(");
                    sbQuery.Append(field.Name);
                    sbQuery.Append(")[1]', 'UNIQUEIDENTIFIER') != '00000000-0000-0000-0000-000000000000' THEN Node.Data.value('(");
                    sbQuery.Append(field.Name);
                    sbQuery.Append(")[1]', 'UNIQUEIDENTIFIER') ELSE NULL END");
                }
                else
                {
                    sbQuery.Append("Node.Data.value('(");
                    sbQuery.Append(field.Name);
                    sbQuery.Append(")[1]', '");
                    sbQuery.Append(sqlType);
                    sbQuery.Append("')");
                }
            }

            sbQuery.Append(" \nFROM @");
            sbQuery.Append(paramName);
            sbQuery.Append(".nodes('/*/*') Node(Data)");

            return sbQuery;
        }

        public static IEnumerable<FieldInfo> GetFieldsInfo(this Type type)
        {
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(type);

            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                var propertyDescriptor = propertyDescriptorCollection[i];
                var propertyType = propertyDescriptor.PropertyType;

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    propertyType = Nullable.GetUnderlyingType(propertyType);

                if (propertyType == null) throw new Exception("не удалось получить тип");

                if (propertyType.IsClass && propertyType != typeof(string)) continue;

                yield return new FieldInfo() { Name = propertyDescriptor.Name, Type = propertyType };
            }
        }

        private static readonly Dictionary<Type, string> _sqlTypeComparer = new Dictionary<Type, string>();
        public static string ToSqlType(this Type type)
        {
            if (_sqlTypeComparer.Count <= 0)
            {
                lock (_sqlTypeComparer)
                {
                    if (_sqlTypeComparer.Count <= 0)
                    {
                        _sqlTypeComparer.Add(typeof(Guid), "UNIQUEIDENTIFIER");
                        _sqlTypeComparer.Add(typeof(int), "INT");
                        _sqlTypeComparer.Add(typeof(string), "NVARCHAR(MAX)");
                    }
                }
            }

            //if (!_sqlTypeComparer.ContainsKey(type))
            //{
            //    if (type.IsEnum) lock (_sqlTypeComparer) if (!_sqlTypeComparer.ContainsKey(type)) _sqlTypeComparer.Add(type, "INT");
            //}

            return _sqlTypeComparer[type];
        }

        public static ExpressionInfo<T> GetInfo<T>(this Expression<Func<T, object>> expression)
        {
            var name = expression.GetTypeName();
            //var func = expression.Compile();

            return new ExpressionInfo<T>() { Name = name, /*Func = func*/ };
        }

        public static string GetTypeName<T>(this Expression<Func<T, object>> targetForeignKeyField)
        {
            var body = targetForeignKeyField.Body as MemberExpression;

            if (body == null)
            {
                var ubody = (UnaryExpression)targetForeignKeyField.Body;
                body = ubody.Operand as MemberExpression;
            }

            string fieldName = body?.Member?.Name;

            if (fieldName == null) throw new Exception("не удалось получить имя поля");

            ////первый символ к нижнему регистру, хотя может это и не важно
            //if (fieldName.Length <= 1) fieldName = fieldName.ToLowerInvariant();
            //else fieldName = char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);

            return fieldName;
        }
    }

    public class FieldInfo
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class ExpressionInfo<T>
    {
        public string Name { get; set; }
        //public Func<T, object> Func { get; set; }
    }
}