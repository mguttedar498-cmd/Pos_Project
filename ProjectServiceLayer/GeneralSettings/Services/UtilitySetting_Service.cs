using HMS_360_PMS.DAL_Layers.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services
{
    public class UtilitySetting_Service : IUtilitySetting_Service
    {
        public readonly IUtilitySetting_Repository _repository;
        private readonly DbConnectionFactory _factory;

        public UtilitySetting_Service(IUtilitySetting_Repository repository, DbConnectionFactory factory)
        {
            _repository = repository;
            _factory = factory;

        }

        #region Happy Hours Setting

        public async Task<HappyHoursModel> GetHappyHoursSettings(string branchcode)
        {
            var happyList = await _repository.GetHappyHoursSettings(branchcode);
            return happyList;
        }

        public async Task<bool> SaveorUpdateHappyHoursSettings(HappyHoursModel request)
        {
            var result = await _repository.SaveorUpdateHappyHoursSettings(request);
           return result;
        }

        #endregion

        #region KOT Timer Setting

        public async Task<TimerSettingModel> GetKOTTimerSettings(string branchcode)
        {
            var timerList = await _repository.GetKOTTimerSettings(branchcode);
            return timerList;
        }

        public async Task<bool> SaveorUpdateKOTTimerSettings(TimerSettingModel request)
        {
            var result = await _repository.SaveOrUpdateKOTTimerSettings(request);
            return result;
        }

        #endregion

        #region Financial Year Setting

        public async Task<FinancialYearModel> GetFinancialSettings(string branchcode)
        {
            var financialList = await _repository.GetFinancialSettings(branchcode);
            return financialList;
        }

        public async Task<bool> SaveOrUpdateFinancialSettings(FinancialYearModel request)
        {
            var result = await _repository.SaveOrUpdateFinancialSettings(request);
            return result;
        }

        #endregion

        #region TaxMode Setting

        public async Task<IEnumerable<TaxModeSettingModel>> GetTaxModeSettings(string branchcode)
        {
            var financialList = await _repository.GetTaxModeSettings(branchcode);
            return financialList;
        }

        public async Task<bool> UpdateTaxModeSettings(TaxModeSettingModel settings)
        {
            var result = await _repository.UpdateTaxModeSettings(settings);
            return result;
        }

        #endregion

        #region DiscountMode Setting

        public async Task<IEnumerable<DiscountModeSetting>> GetDiscountModeSettings(string branchcode)
        {
            var discList = await _repository.GetDiscountModeSettings(branchcode);
            return discList;
        }

        public async Task<bool> UpdateDiscountModeSettings(DiscountModeSetting request)
        {
            var result = await _repository.UpdateDiscountModeSettings(request);
            return result;
        }

        #endregion

        #region SMS Sender Setting

        public async Task<SmsSettingModel> GetSMSSenderSettings(string branchcode)
        {
            var smsList = await _repository.GetSMSSenderSettings(branchcode);
            return smsList;
        }

        public async Task<bool> SaveOrUpdateSMSSenderSettings(SmsSettingModel request)
        {
            var result = await _repository.SaveOrUpdateSMSSenderSettings(request);
            return result;
        }


        #endregion

        #region Printer Setting 

        public async Task<PrintConfigResponse> GetPrinterSettings(string branchcode, int OltCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var response = new PrintConfigResponse();

            response.Printers = (await _repository.GetPrinterDetails(branchcode, con)).ToList();

            response.PrinterConfigurations = (await _repository.GetPrinterConfigurations(branchcode, OltCode, con)).ToList();

            response.CartGroup = (await _repository.GetCartGroupDetails(branchcode, OltCode, con)).ToList();

            return response;
        }

        public async Task<ServiceResult> SaveOrUpdateCatGroupSettings(CartGroupModel request)
        {
            int NxtGrp = await _repository.Findnextnumber("Tbl_CatGrp_Kot", "GRP", "Branch_Code", request.Branch_code);

            if (request.Grp == 0)
            {
                request.Grp = NxtGrp;
            }

            var result = await _repository.SaveOrUpdateCatGroupSettings(request);

            return new ServiceResult
            {
                Success = true,
                Message = "Cat Group saved successfully",
                Data = Convert.ToInt32(result)
            };
        }

        public async Task<ServiceResult> SaveOrUpdatePrinterSettings(PrinterConfigModel request)
        {
            var result = await _repository.SaveOrUpdatePrinterSettings(request);
            return new ServiceResult
            {
                Success = true,
                Message = "Printer Configuration saved successfully",
                Data = Convert.ToInt32(result)
            };
        }

        public async Task<ServiceResult> DeleteCatGroupSettings(int GrpCode, int OltCode, string Branchcode)
        {
            var success = await _repository.DeleteCatGroupSettings(GrpCode, OltCode, Branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Branch Detail not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Branch Detail deleted successfully",
                Data = null
            };
        }

        public async Task<ServiceResult> DeletePrinterSettings(int GrpCode, int OltCode, int UserCode, string Branchcode)
        {
            var success = await _repository.DeletePrinterSettings(GrpCode, OltCode, UserCode, Branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Branch Detail not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Branch Detail deleted successfully",
                Data = null
            };
        }

        #endregion

        #region Bill Geneation

        public async Task<bool> BillGeneration(BillGeneration model)
        {
            var result = await _repository.BillGeneration(model);
            return result;
        }

        public async Task<IEnumerable<BillGeneration>> GetBillGenerationData(string Branch_Code)
        {
            var result = await _repository.GetBillGenerationData(Branch_Code);
            return result;
        }

        #endregion

        #region KotConfig

        public async Task<bool> KotConfiguration(KotConfig model)
        {
            var result = await _repository.KotConfiguration(model);
            return result;
        }

        public async Task<IEnumerable<KotConfig>> GetKotConfiguration(string Branch_Code)
        {
            var result = await _repository.GetKotConfiguration(Branch_Code);
            return result;
        }

        #endregion

        #region SaveBillConfigModel

        public async Task<bool> BillConfiguration(SaveBillConfigModel model)
        {
            var result = await _repository.BillConfiguration(model);
            return result;
        }

        public async Task<IEnumerable<SaveBillConfigModel>> GetBillConfiguration(string Branch_Code)
        {
            var result = await _repository.GetBillConfiguration(Branch_Code);
            return result;
        }

        #endregion

        #region PosAndKotDataDelete
        public async Task<string> PosAndKotDataDelete()
        {
            var result = await _repository.PosAndKotDataDelete();
            return result;
        }

        #endregion

        #region
        #endregion
        #region
        #endregion
        #region
        #endregion

    }
}
