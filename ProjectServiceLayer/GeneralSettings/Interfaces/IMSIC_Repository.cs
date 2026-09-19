namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces
{
    public interface IMSIC_Repository
    {
        Task<bool> POSMSICResetTransaction();

        Task<bool> POSMSICResetTransactionFROMandTO(string Branchcode, DateTime Fromdate, DateTime Todate);

        Task<bool> POSMSICResetMasters();

        Task<bool> POSMSICTruncateAll();

        Task<bool> POSMSICUpdateBranch(string Branchcode);

        Task<bool> POSMSICCreateUserMaster(int usercode, string username, string password, string branchcode);
    }
}
