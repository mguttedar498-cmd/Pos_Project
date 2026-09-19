using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces
{
    public interface IKOTPickUp_Repository
    {
        Task<bool> Pick(PFoodModel food);

    }
}
