using Dapper;
using DapperWithELK.Common.Dapper;
using DapperWithELK.Model;
using System.Data;

namespace DapperWithELK.Repo
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private readonly IDapperContext _DMSDapperContext;
        public EmployeeRepo( string connectionString) {
            _DMSDapperContext = new DapperContext(connectionString);
        }
        

        public tbl_employee GetBaseddOnCode(int code)
        {
            return _DMSDapperContext.GetDynamic<tbl_employee>(w => w.code == code);

        }
        public List<tbl_employee> GetAll()
        {
            return _DMSDapperContext.GetAllDynamic<tbl_employee>(w=>w.code!=0).ToList();
        }

        public int Insert(tbl_employee obj)
        {
            return _DMSDapperContext.Insert<tbl_employee>(obj);
        }
        public bool Delete(tbl_employee objrole)
        {
            var status = _DMSDapperContext.Delete(objrole);
            return status;
        }

        public bool Update(tbl_employee obj)
        {
            return _DMSDapperContext.Update<tbl_employee>(obj);
        }
    }
}
