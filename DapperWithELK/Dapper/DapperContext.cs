using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;
using static Dapper.SqlMapper;

namespace DapperWithELK.Common.Dapper
{
    public class DapperContext : IDapperContext
    {
        private readonly string _ConnectionString;
        public DapperContext(string ConnectionString)
        {
            _ConnectionString = ConnectionString;
        }

        public T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.QueryFirstOrDefault<T>(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public IEnumerable<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.Query<T>(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public (IEnumerable<T1>, IEnumerable<T2>) GetAll<T1, T2>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                using (var reader = db.QueryMultiple(sp, parms, commandType: commandType, commandTimeout: commandTimeout))
                {
                    return (reader.Read<T1>(), reader.Read<T2>());
                }
            }
        }
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) GetAll<T1, T2, T3>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                using (var reader = db.QueryMultiple(sp, parms, commandType: commandType, commandTimeout: commandTimeout))
                {
                    return (reader.Read<T1>(), reader.Read<T2>(), reader.Read<T3>());
                }
            }
        }

        public int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.Execute(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public Tuple<int, Dictionary<string, object>> ExecuteWithOutput(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null, string[]? outParameters = null, int? noOfAttempts = 3)
        {
            var ret = Tuple.Create(0, new Dictionary<string, object>());
            var attempt = 0;
        retry:
            try
            {
                attempt++;
                using (IDbConnection db = new SqlConnection(_ConnectionString))
                {
                    var recAff = db.Execute(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
                    var reprm = new Dictionary<string, object>();
                    if (outParameters != null)
                    {
                        outParameters.ToList().ForEach(f => {
                            reprm.Add(f, parms.Get<object>(f));
                        });
                    }
                    ret = Tuple.Create(recAff, reprm);
                }
            }
            catch (SqlException ex) when (ex.Number == -2 || ex.Number == 11 || ex.Number == 1205)
            {
                if (attempt < noOfAttempts)
                    goto retry;
                else
                    throw ex;
            }
            return ret;
        }

        public T ExecuteScalar<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.ExecuteScalar<T>(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public int GetCountDynamic<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetDynamicCountQuery<T>(predicate);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.ExecuteScalar<int>(query.Sql, (object)query.Param, commandTimeout: commandTimeout);
            }
        }

        public T GetDynamic<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetDynamicQuery<T>(predicate);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.QueryFirstOrDefault<T>(query.Sql, (object)query.Param, commandTimeout: commandTimeout);
            }
        }

        public IEnumerable<T> GetAllDynamic<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetDynamicQuery<T>(predicate);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.Query<T>(query.Sql, (object)query.Param, commandTimeout: commandTimeout);
            }
        }
        public int Insert<T>(T obj, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetInsertQuery<T>(obj);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.Execute(query, obj, commandTimeout: commandTimeout);
            }
        }
        public bool Update<T>(T obj, int? commandTimeout = null)
        {
            bool ret = false;
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                ret = false;
                var query = DynamicQuery.GetUpdateQuery<T>(obj);
                var updated = db.Execute(query, obj, commandTimeout: commandTimeout);
                ret = (updated > 0);
            }
            return ret;
        }
        public bool Delete<T>(T obj, int? commandTimeout = null)
        {
            bool ret = false;
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                ret = false;
                var query = DynamicQuery.GetDeleteQuery<T>(obj);
                var deleted = db.Execute(query, obj, commandTimeout: commandTimeout);
                ret = (deleted > 0);
            }
            return ret;
        }

        #region Async Methods

        public async Task<T> GetAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.QueryFirstOrDefaultAsync<T>(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.QueryAsync<T>(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public async Task<(IEnumerable<T1>, IEnumerable<T2>)> GetAllAsync<T1, T2>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                using (var reader = await db.QueryMultipleAsync(sp, parms, commandType: commandType, commandTimeout: commandTimeout))
                {
                    return (await reader.ReadAsync<T1>(), await reader.ReadAsync<T2>());
                }
            }
        }
        public async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> GetAllAsync<T1, T2, T3>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                using (var reader = await db.QueryMultipleAsync(sp, parms, commandType: commandType, commandTimeout: commandTimeout))
                {
                    return (await reader.ReadAsync<T1>(), await reader.ReadAsync<T2>(), await reader.ReadAsync<T3>());
                }
            }
        }

        public async Task<int> ExecuteAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.ExecuteAsync(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.ExecuteScalarAsync<T>(sp, parms, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public Task<int> GetCountDynamicAsync<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetDynamicCountQuery<T>(predicate);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return db.ExecuteScalarAsync<int>(query.Sql, (object)query.Param, commandTimeout: commandTimeout);
            }
        }

        public async Task<T> GetDynamicAsync<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetDynamicQuery<T>(predicate);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.QueryFirstOrDefaultAsync<T>(query.Sql, (object)query.Param, commandTimeout: commandTimeout);
            }
        }

        public async Task<IEnumerable<T>> GetAllDynamicAsync<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetDynamicQuery<T>(predicate);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.QueryAsync<T>(query.Sql, (object)query.Param, commandTimeout: commandTimeout);
            }
        }


        public async Task<int> InsertAsync<T>(T obj, int? commandTimeout = null)
        {
            var query = DynamicQuery.GetInsertQuery<T>(obj);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                return await db.ExecuteAsync(query, obj, commandTimeout: commandTimeout);
            }
        }

        public async Task<bool> UpdateAsync<T>(T obj, int? commandTimeout = null)
        {
            bool ret = false;

            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                ret = false;
                var query = DynamicQuery.GetUpdateQuery<T>(obj);
                var updated = await db.ExecuteAsync(query, obj, commandTimeout: commandTimeout);
                ret = (updated > 0);
            }
            return ret;
        }

        public async Task<bool> DeleteAsync<T>(T obj, int? commandTimeout = null)
        {
            bool ret = false;
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                ret = false;
                var query = DynamicQuery.GetDeleteQuery<T>(obj);
                var deleted = await db.ExecuteAsync(query, obj, commandTimeout: commandTimeout);
                ret = (deleted > 0);
            }
            return ret;
        }

        public DataTable GetDataTable(string spName, DynamicParameters parms)
        {
            //var res = ExecuteSPWithoutTran<dynamic>(sp, parms);
            //return ToDataTable(res);
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                //var res = db.Query(spName, parms, commandType: CommandType.StoredProcedure);
                var res = db.Query(sql: spName, param: parms, commandTimeout: 0, commandType: CommandType.StoredProcedure);
                //var res = db.Query<T>(spName, parms, commandTimeout: 0, commandType: CommandType.StoredProcedure);
                return ToDataTable(res);
            }
        }

        public DataTable ToDataTable<dynamic>(IEnumerable<dynamic> items)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (items == null) return null;
#pragma warning restore CS8603 // Possible null reference return.
            var data = items.ToArray();
#pragma warning disable CS8603 // Possible null reference return.
            if (data.Length == 0) return null;
#pragma warning restore CS8603 // Possible null reference return.

            var dt = new DataTable();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var pair in (IDictionary<string, object>)data[0])
            {
                dt.Columns.Add(pair.Key, (pair.Value ?? string.Empty).GetType());
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            foreach (var d in data)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            return dt;
        }

        #endregion

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> GetMultiple<T1, T2>(string sql, DynamicParameters parameters,
                                       Func<GridReader, IEnumerable<T1>> func1,
                                       Func<GridReader, IEnumerable<T2>> func2)
        {
            var objs = getMultiple(sql, parameters, func1, func2);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Tuple.Create(objs[0] as IEnumerable<T1>, objs[1] as IEnumerable<T2>);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> GetMultiple<T1, T2, T3>(string sql, DynamicParameters parameters,
                                        Func<GridReader, IEnumerable<T1>> func1,
                                        Func<GridReader, IEnumerable<T2>> func2,
                                        Func<GridReader, IEnumerable<T3>> func3)
        {
            var objs = getMultiple(sql, parameters, func1, func2, func3);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Tuple.Create(objs[0] as IEnumerable<T1>, objs[1] as IEnumerable<T2>, objs[2] as IEnumerable<T3>);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        public Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>> GetMultiple<T1, T2, T3, T4>(string sql, DynamicParameters parameters,
                                    Func<GridReader, IEnumerable<T1>> func1,
                                    Func<GridReader, IEnumerable<T2>> func2,
                                    Func<GridReader, IEnumerable<T3>> func3,
                                    Func<GridReader, IEnumerable<T4>> func4)
        {
            var objs = getMultiple(sql, parameters, func1, func2, func3, func4);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Tuple.Create(objs[0] as IEnumerable<T1>, objs[1] as IEnumerable<T2>, objs[2] as IEnumerable<T3>, objs[3] as IEnumerable<T4>);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        public Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>> GetMultiple<T1, T2, T3, T4, T5>(string sql, DynamicParameters parameters,
                                    Func<GridReader, IEnumerable<T1>> func1,
                                    Func<GridReader, IEnumerable<T2>> func2,
                                    Func<GridReader, IEnumerable<T3>> func3,
                                    Func<GridReader, IEnumerable<T4>> func4,
                                    Func<GridReader, IEnumerable<T5>> func5)
        {
            var objs = getMultiple(sql, parameters, func1, func2, func3, func4, func5);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Tuple.Create(objs[0] as IEnumerable<T1>, objs[1] as IEnumerable<T2>, objs[2] as IEnumerable<T3>, objs[3] as IEnumerable<T4>, objs[4] as IEnumerable<T5>);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        public Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>> GetMultiple<T1, T2, T3, T4, T5, T6>(string sql, DynamicParameters parameters,
                                  Func<GridReader, IEnumerable<T1>> func1,
                                  Func<GridReader, IEnumerable<T2>> func2,
                                  Func<GridReader, IEnumerable<T3>> func3,
                                  Func<GridReader, IEnumerable<T4>> func4,
                                  Func<GridReader, IEnumerable<T5>> func5,
                                  Func<GridReader, IEnumerable<T6>> func6)
        {
            var objs = getMultiple(sql, parameters, func1, func2, func3, func4, func5, func6);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Tuple.Create(objs[0] as IEnumerable<T1>, objs[1] as IEnumerable<T2>, objs[2] as IEnumerable<T3>, objs[3] as IEnumerable<T4>, objs[4] as IEnumerable<T5>, objs[5] as IEnumerable<T6>);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        public List<object> getMultiple(string sql, DynamicParameters parameters, params Func<GridReader, object>[] readerFuncs)
        {
            var returnResults = new List<object>();
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                var gridReader = db.QueryMultiple(sql, parameters, commandType: CommandType.StoredProcedure);

                foreach (var readerFunc in readerFuncs)
                {
                    var obj = readerFunc(gridReader);
                    returnResults.Add(obj);
                }
            }

            return returnResults;
        }

        public T? Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            T? result;
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                try
                {
                    if (db.State == ConnectionState.Closed)
                        db.Open();

                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {
                            result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran).FirstOrDefault();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw new Exception("Message:", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Message:", ex);
                }
                finally
                {
                    if (db.State == ConnectionState.Open)
                        db.Close();
                }

                return result;
            }
        }

        public List<T> InsertNGetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            List<T> result;
            using (IDbConnection db = new SqlConnection(_ConnectionString))
            {
                try
                {
                    if (db.State == ConnectionState.Closed)
                        db.Open();

                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {
                            result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran).ToList();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw new Exception("Message:", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Message:", ex);
                }
                finally
                {
                    if (db.State == ConnectionState.Open)
                        db.Close();
                }

                return result;
            }
        }
    }
}
