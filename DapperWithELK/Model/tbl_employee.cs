
using DapperWithELK.Common.Dapper;

namespace DapperWithELK.Model
{
    public class tbl_employee
    {
        [CustomColumn(Primary = true,Identity =true)] public int code { get; set; } = default!;
        [CustomColumn] public string? name { get; set; } = default!;
        [CustomColumn] public string? email { get; set; } = default!;
        [CustomColumn] public string? phone { get; set; } = default!;
        [CustomColumn] public string? designation { get; set; } = default!;
    }
}
