using Dapper;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.KOT;
using HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces;
using System.Data;
using System.Data.Common;

namespace HMS_360_PMS.ProjectInfrastructure.KOT
{
    //public class GeneralManager_DAL
    public class GeneralManager_DAL : IBasicSettingsManager, IDashboardManager
    {
        private readonly DbConnectionFactory _factory;

        public GeneralManager_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

   
        public async Task<List<SmsRights>> GetSmsRights()
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            var qry = "select * from Tbl_Sms_Rights";

            var settings = connection.Query<SmsRights>(qry).ToList();
            return settings;
        }

        //public async Task<SmsSender> GetBaseSettings()
        //{
        //    using var connection = _factory.CreateConnection(DbNames.HMS);

        //    var qry = "select * from tbl_sms_sender";

        //    var settings = connection.Query<SmsSender>(qry).FirstOrDefault();
        //    return settings;
        //}

        public async Task<SmsSettings> GetBaseSettings()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "select * from Tbl_SmsSender";

            var settings = connection.Query<SmsSettings>(qry).FirstOrDefault();
            return settings;
        }

        public async Task<PurcharserDetalis> GetPurcharserDetalis()
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            var sql = "select * from Tbl_Purcharser_Detalis";
            var dt = connection.Query<PurcharserDetalis>(sql).FirstOrDefault();
            return dt;
        }

        public async Task<DashboardModel> GetDashboardData()
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            var dash = new DashboardModel();

            var floors = await GetAllFloors();
            var rooms = (await GetAllRooms())
                        .Where(r => r.Prioritys == "P")
                        .OrderBy(r => r.RoomNo)
                        .ToList();

            dash.Floors = floors;
            dash.Rooms = rooms;

            dash.Vacant = rooms.Count(r => r.Status == "V");
            dash.Occupied = rooms.Count(r => r.Status == "O");
            dash.Dirty = rooms.Count(r => r.Status == "D");
            dash.Blocked = rooms.Count(r => r.Status == "B");
            dash.Un_Settel = rooms.Count(r => r.Status == "U");
            dash.Management = rooms.Count(r => r.Status == "M");

            if (rooms.Count > 0)
            {
                var occ = (Convert.ToDecimal(dash.Occupied) * 100) / rooms.Count;
                dash.Occ = decimal.Round(occ, 2);
            }

            // ✅ Today Checkin
            var tcn = connection.ExecuteScalar<int>(
                @"SELECT COUNT(CheckInNo) 
                  FROM Tbl_CheckIn_Master 
                  WHERE CAST(TrDate AS DATE) = CAST(GETDATE() AS DATE)"
            );

            // ✅ Today Checkout
            var tco = connection.ExecuteScalar<int>(
                @"SELECT COUNT(CheckInNo) 
                  FROM Tbl_NewCheckout_Master 
                  WHERE CAST(TrDate AS DATE) = CAST(GETDATE() AS DATE)"
            );

            dash.TodayCheckin = tcn;
            dash.TodayCheckout = tco;

            return dash;
        }

        public async Task<List<FloorMaster>> GetAllFloors()
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            var qry = "select * from tbl_floor_master";
            return connection.Query<FloorMaster>(qry).ToList();
        }

        public async Task<List<RoomDetails>> GetAllRooms()
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            var qry = "select * from tbl_Room_Details";
            return connection.Query<RoomDetails>(qry).ToList();
        }

        #region  Email Notfication

        public async Task<EmailSenderModel> GetEmailSenderData()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var qry = "select * from Tbl_Email_Sender";
            return await connection.QueryFirstOrDefaultAsync<EmailSenderModel>(qry);
        }

        public async Task<IEnumerable<EmailReceiver>> GetEmailReceiverData()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var qry = "select * from Tbl_Email_Master";
            return await connection.QueryAsync<EmailReceiver>(qry);
        }

        #endregion
    }
}
