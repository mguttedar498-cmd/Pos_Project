using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces
{
    public interface IKOTDisplay_Repository
    {
        Task<string> getinfo(string tblname, string fieldname, string code, string parameter, string parameter1);

        Task<List<dynamic>> checkExists(KOTDisplayModel item);

        Task<List<TblKOTDisplayModel>> GetloadItems0();

        Task<List<KOTDisplayModel>> GetloadItems1(int depcode);

        Task<List<TblKOTDisplayModel>> GetloadItems2(string kotno, int priority, string itemcode);

        Task<List<TblKOTDisplayModel>> GetloadItems3(int depcode);

        Task<List<TblKOTDisplayModel>> GetloadItems4(string kotno, int priority, string tblno);

        Task<List<ViewKOTDisplayModel>> GetloadItems5(string kotno);

        Task<List<KOTDisplayModel>> GetloadItems6();

        Task<List<KOTModifyDetailsModel>> GetloadItems7(string kotno, string itemcode);

        Task<List<KOTModifyDetailsModel>> GetloadItems8(string kotno, string itemcode);

        Task<List<KOTnQtyModel>> GetloadItems9(string kotno, string itemcode);

        Task<List<KOTDisplayModel>> GetloadItems10(string tblno);

        Task<List<TblKOTDisplayModel>> GetloadItems11(int depcode, string tblno);

        Task<List<string>> GetloadItems12();

        Task<List<TblKOTDisplayModel>> GetloadItems13();
        
        Task<List<string>> GetloadItems14(string tblno);

        Task<bool> UpdateReadyAsync(TblKOTDisplayModel model);
        
        Task<bool> UpdateKotDetailsAsync(TblKOTDisplayModel model);

    }
}
