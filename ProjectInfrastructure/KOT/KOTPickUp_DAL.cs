using Dapper;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.HMS_360_PMS.Infrastructure.KOT
{
    public class KOTPickUp_DAL : IKOTPickUp_Repository
    {
        private readonly DbConnectionFactory _factory;

        public KOTPickUp_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> Pick(PFoodModel food)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var updatequery = @"Update KotDetails set Qty = 1 where itemcode = @itemcode and kotno = @kotno and Qty is null and KDQty = '1' and KNQty = '1'";
            var rows = await connection.ExecuteAsync(updatequery, new { itemcode = food.Code, kotno = food.Kot });

            var updateDisplayQuery = @"Update Tbl_Kot_Display set Picked = 1, PickedTime = @pickedTime where itemcode = @itemcode and kotno = @kotno";
            var displayRows = await connection.ExecuteAsync(updateDisplayQuery, new { pickedTime = DateTime.Now.ToShortTimeString(), itemcode = food.Code, kotno = food.Kot });

            return rows > 0 && displayRows > 0;
        }
    }
}
