using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces
{
    public interface IKOTPickUp_Service
    {
        Task<bool> Pick(PFoodModel food);
    }
}
