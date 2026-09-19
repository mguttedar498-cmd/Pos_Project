using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services
{
    public class MSIC_Service : IMSIC_Services
    {
        private readonly IMSIC_Repository _repository;
        
        public MSIC_Service(IMSIC_Repository repository)
        {
            _repository = repository;
        }

        public async Task<bool> POSMSICResetTransaction()
        {
            return await _repository.POSMSICResetTransaction();
        }

        public async Task<bool> POSMSICResetTransactionFROMandTO(string Branchcode, DateTime Fromdate, DateTime Todate)
        {
            return await _repository.POSMSICResetTransactionFROMandTO(Branchcode, Fromdate, Todate);
        }

        public async Task<bool> POSMSICResetMasters()
        {
            return await _repository.POSMSICResetMasters();
        }

        public async Task<bool> POSMSICTruncateAll()
        {
            return await _repository.POSMSICTruncateAll();
        }

        public async Task<bool> POSMSICUpdateBranch(string branchCode)
        {
            return await _repository.POSMSICUpdateBranch(branchCode);
        }

        public async Task<bool> POSMSICCreateUserMaster(int usercode, string username, string password, string branchcode)
        {
            return await _repository.POSMSICCreateUserMaster(usercode, username, password, branchcode);
        }
    }
}
