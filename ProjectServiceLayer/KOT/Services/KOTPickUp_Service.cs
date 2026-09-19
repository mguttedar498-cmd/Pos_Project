using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Services
{
    public class KOTPickUp_Service : IKOTPickUp_Service
    {
        private readonly IKOTPickUp_Repository _repository;

        public KOTPickUp_Service(IKOTPickUp_Repository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Pick(PFoodModel food)
        {
            var updateDisplay = await _repository.Pick(food);

            return updateDisplay;
        }
    }
}
