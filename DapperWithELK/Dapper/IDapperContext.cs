using Dapper;
using System.Data;
using System.Linq.Expressions;
using static Dapper.SqlMapper;

namespace DapperWithELK.Common.Dapper
{
    public interface IDapperContext
    {
        T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        IEnumerable<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        (IEnumerable<T1>, IEnumerable<T2>) GetAll<T1, T2>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) GetAll<T1, T2, T3>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Tuple<int, Dictionary<string, object>> ExecuteWithOutput(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null, string[]? outParameters = null, int? noOfAttempts = 3);
        T ExecuteScalar<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        int GetCountDynamic<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null);
        T GetDynamic<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null);
        IEnumerable<T> GetAllDynamic<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null);
        int Insert<T>(T obj, int? commandTimeout = null);
        bool Update<T>(T obj, int? commandTimeout = null);
        bool Delete<T>(T obj, int? commandTimeout = null);
        T? Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<T> GetAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Task<IEnumerable<T>> GetAllAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Task<(IEnumerable<T1>, IEnumerable<T2>)> GetAllAsync<T1, T2>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> GetAllAsync<T1, T2, T3>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);

        Task<int> ExecuteAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Task<T> ExecuteScalarAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Task<int> GetCountDynamicAsync<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null);
        Task<T> GetDynamicAsync<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null);
        Task<IEnumerable<T>> GetAllDynamicAsync<T>(Expression<Func<T, bool>> predicate, int? commandTimeout = null);
        Task<int> InsertAsync<T>(T obj, int? commandTimeout = null);
        Task<bool> UpdateAsync<T>(T obj, int? commandTimeout = null);
        Task<bool> DeleteAsync<T>(T obj, int? commandTimeout = null);
        DataTable GetDataTable(string spName, DynamicParameters parms);
        DataTable ToDataTable<dynamic>(IEnumerable<dynamic> items);


        /**/
        Tuple<IEnumerable<T1>, IEnumerable<T2>> GetMultiple<T1, T2>(string sql, DynamicParameters parameters,
                                        Func<GridReader, IEnumerable<T1>> func1,
                                        Func<GridReader, IEnumerable<T2>> func2);
        Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> GetMultiple<T1, T2, T3>(string sql, DynamicParameters parameters,
                                       Func<GridReader, IEnumerable<T1>> func1,
                                       Func<GridReader, IEnumerable<T2>> func2,
                                       Func<GridReader, IEnumerable<T3>> func3);
        Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>> GetMultiple<T1, T2, T3, T4>(string sql, DynamicParameters parameters,
                                    Func<GridReader, IEnumerable<T1>> func1,
                                    Func<GridReader, IEnumerable<T2>> func2,
                                    Func<GridReader, IEnumerable<T3>> func3,
                                    Func<GridReader, IEnumerable<T4>> func4);
        Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>> GetMultiple<T1, T2, T3, T4, T5>(string sql, DynamicParameters parameters,
                                    Func<GridReader, IEnumerable<T1>> func1,
                                    Func<GridReader, IEnumerable<T2>> func2,
                                    Func<GridReader, IEnumerable<T3>> func3,
                                    Func<GridReader, IEnumerable<T4>> func4,
                                    Func<GridReader, IEnumerable<T5>> func5);
        Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>> GetMultiple<T1, T2, T3, T4, T5, T6>(string sql, DynamicParameters parameters,
                                   Func<GridReader, IEnumerable<T1>> func1,
                                   Func<GridReader, IEnumerable<T2>> func2,
                                   Func<GridReader, IEnumerable<T3>> func3,
                                   Func<GridReader, IEnumerable<T4>> func4,
                                   Func<GridReader, IEnumerable<T5>> func5,
                                   Func<GridReader, IEnumerable<T6>> func6);

        List<T> InsertNGetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
    }
}
