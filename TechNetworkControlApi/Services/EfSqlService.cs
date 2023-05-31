using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace TechNetworkControlApi.Services;

public static class EfSqlService
{
    private class PropertyMap
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public bool IsSame(PropertyMap map)
        {
            if (map == null)
                return false;

            bool same = map.Name == Name && map.Type == Type;
            return same;
        }
    }

    public static DbTransaction GetDbTransaction(this IDbContextTransaction source)
    {
        return (source as IInfrastructure<DbTransaction>).Instance;
    }

    public static IEnumerable<T> FromSqlQuery<T>
    (this DbContext context, string query, List<DbParameter> parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeOutInSeconds = null) where T : new()
    {
        return FromSqlQuery<T>(context.Database, query, parameters,
            commandType, commandTimeOutInSeconds);
    }

    public static IEnumerable<T> FromSqlQuery<T>
    (this DatabaseFacade database, string query,
        List<DbParameter> parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeOutInSeconds = null) where T : new()
    {
        const BindingFlags flags = BindingFlags.Public |
                                   BindingFlags.Instance | BindingFlags.NonPublic;
        List<PropertyMap> entityFields = typeof(T).GetProperties(flags)
            .Select(aProp => new PropertyMap
            {
                Name = aProp.Name, Type = Nullable.GetUnderlyingType(aProp.PropertyType) ?? aProp.PropertyType
            }).ToList();
        List<PropertyMap> dbDataReaderFields = new List<PropertyMap>();
        List<PropertyMap> commonFields = null;

        using (var command = database.GetDbConnection().CreateCommand())
        {
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            var currentTransaction = database.CurrentTransaction;
            if (currentTransaction != null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            command.CommandText = query;
            command.CommandType = commandType;
            if (commandTimeOutInSeconds != null)
            {
                command.CommandTimeout = (int) commandTimeOutInSeconds;
            }

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            using (var result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    if (commonFields == null)
                    {
                        for (int i = 0; i < result.FieldCount; i++)
                        {
                            dbDataReaderFields.Add(new PropertyMap
                            {
                                Name = result.GetName(i),
                                Type = result.GetFieldType(i)
                            });
                        }

                        commonFields = entityFields.Where
                        (x => dbDataReaderFields.Any(d =>
                            d.IsSame(x))).Select(x => x).ToList();
                    }

                    var entity = new T();
                    foreach (var aField in commonFields)
                    {
                        PropertyInfo propertyInfos =
                            entity.GetType().GetProperty(aField.Name);
                        var value = (result[aField.Name] == DBNull.Value)
                            ? null
                            : result[aField.Name]; //if field is nullable
                        propertyInfos.SetValue(entity, value, null);
                    }

                    yield return entity;
                }
            }
        }
    }

    public static IEnumerable<T> FromSqlQuery<T>
    (this DbContext context, string query, Func<DbDataReader, T> map,
        List<DbParameter> parameters = null, CommandType commandType = CommandType.Text,
        int? commandTimeOutInSeconds = null)
    {
        return FromSqlQuery(context.Database, query, map, parameters,
            commandType, commandTimeOutInSeconds);
    }

    public static IEnumerable<T> FromSqlQuery<T>
    (this DatabaseFacade database, string query, Func<DbDataReader, T> map,
        List<DbParameter> parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeOutInSeconds = null)
    {
        using (var command = database.GetDbConnection().CreateCommand())
        {
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            var currentTransaction = database.CurrentTransaction;
            if (currentTransaction != null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            command.CommandText = query;
            command.CommandType = commandType;
            if (commandTimeOutInSeconds != null)
            {
                command.CommandTimeout = (int) commandTimeOutInSeconds;
            }

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            using (var result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    yield return map(result);
                }
            }
        }
    }
}