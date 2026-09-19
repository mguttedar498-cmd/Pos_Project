using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces
{
    public interface IUtilitySetting_Service
    {
        #region Happy Hours Setting

        Task<HappyHoursModel> GetHappyHoursSettings(string branchcode);

        Task<bool> SaveorUpdateHappyHoursSettings(HappyHoursModel request);

        #endregion

        #region KOT Timer Setting

        Task<TimerSettingModel> GetKOTTimerSettings(string branchcode);

        Task<bool> SaveorUpdateKOTTimerSettings(TimerSettingModel request);


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

        Task<PrintConfigResponse> GetPrinterSettings(string branchcode, int OltCode);

        Task<ServiceResult> SaveOrUpdateCatGroupSettings(CartGroupModel GrpRequest);

        Task<ServiceResult> SaveOrUpdatePrinterSettings(PrinterConfigModel printerRequest);

        Task<ServiceResult> DeleteCatGroupSettings(int GrpCode, int OltCode, string Branchcode);

        Task<ServiceResult> DeletePrinterSettings(int GrpCode, int OltCode, int UserCode, string Branchcode);

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
        #region
        #endregion
    }
}
