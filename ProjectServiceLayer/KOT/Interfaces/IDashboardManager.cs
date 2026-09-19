using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces
{
    public interface IDashboardManager
    {
        Task<DashboardModel> GetDashboardData();

    }
}
