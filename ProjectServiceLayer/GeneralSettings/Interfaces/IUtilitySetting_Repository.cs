using Azure.Core;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using System.Data;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces
{
    public interface IUtilitySetting_Repository
    {
        #region Happy Hours Setting

        Task<HappyHoursModel> GetHappyHoursSettings(string branchcode);

        Task<bool> SaveorUpdateHappyHoursSettings(HappyHoursModel request);

        #endregion

        #region KOT Timer Setting

        Task<TimerSettingModel> GetKOTTimerSettings(string branchcode);

        Task<bool> SaveOrUpdateKOTTimerSettings(TimerSettingModel request);

        #endregion

        #region Financial Year Setting  

        Task<FinancialYearModel> GetFinancialSettings(string branchcode);

        Task<bool> SaveOrUpdateFinancialSettings(FinancialYearModel request);

        #endregion

        #region TaxMode Setting

        Task<IEnumerable<TaxModeSettingModel>> GetTaxModeSettings(string branchcode);
        
        Task<bool> UpdateTaxModeSettings(TaxModeSettingModel settings);

        #endregion

        #region DiscountMode Setting

        Task<IEnumerable<DiscountModeSetting>> GetDiscountModeSettings(string branchcode);

        Task<bool> UpdateDiscountModeSettings(DiscountModeSetting settings);

        #endregion

        #region SMS Sender Setting

        Task<SmsSettingModel> GetSMSSenderSettings(string branchcode);

        Task<bool> SaveOrUpdateSMSSenderSettings(SmsSettingModel settings);

        #endregion

        #region Printer Setting 

        Task<IEnumerable<PrinterModel>> GetPrinterDetails(string branchcode, IDbConnection con);

        Task<IEnumerable<PrinterConfigModel>> GetPrinterConfigurations(string branchcode, int OltCode, IDbConnection con);

        Task<IEnumerable<CartGroupModel>> GetCartGroupDetails(string branchcode, int OltCode, IDbConnection con);

        Task<bool> SaveOrUpdateCatGroupSettings(CartGroupModel request);

        Task<bool> SaveOrUpdatePrinterSettings(PrinterConfigModel request);

        Task<bool> DeleteCatGroupSettings(int GrpCode, int OltCode, string Branchcode);

        Task<bool> DeletePrinterSettings(int GrpCode, int OltCode, int UserCode, string Branchcode);

        #endregion


        #region Bill Geneation

        Task<bool> BillGeneration(BillGeneration model);

        Task<IEnumerable<BillGeneration>> GetBillGenerationData(string Branch_Code);

        #endregion

        #region KotConfig

        Task<bool> KotConfiguration(KotConfig model);

        Task<IEnumerable<KotConfig>> GetKotConfiguration(string Branch_Code);

        #endregion

        #region SaveBillConfigModel

        Task<bool> BillConfiguration(SaveBillConfigModel model);

        Task<IEnumerable<SaveBillConfigModel>> GetBillConfiguration(string Branch_Code);

        #endregion

        #region PosAndKotDataDelete

        Task<string> PosAndKotDataDelete();

        #endregion

        #region
        #endregion
        #region
        #endregion

        #region Common Code

        Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch);

        #endregion
    }
}
