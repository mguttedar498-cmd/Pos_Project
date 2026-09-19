namespace HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces
{
    public interface IGeneralManager_Service
    {
        Task<string> GenerateingOtp(string MobileNo);

    }
}
