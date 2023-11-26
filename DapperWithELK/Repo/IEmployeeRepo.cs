using DapperWithELK.Model;

namespace DapperWithELK.Repo
{
    public interface IEmployeeRepo
    {
        List<tbl_employee> GetAll();
        tbl_employee GetBaseddOnCode(int code);
        int Insert(tbl_employee obj);
        bool Delete(tbl_employee objrole);
        bool Update(tbl_employee obj);

    }
}
