using Dapper;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HMS_360_PMS.HMS_360_PMS.Infrastructure.KOT
{
    public class KOTDisplay_DAL : IKOTDisplay_Repository
    {
        private readonly DbConnectionFactory _factory;

        public KOTDisplay_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<string> getinfo(string tblname, string fieldname, string code, string parameter, string parameter1)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string sql;

            if (string.IsNullOrEmpty(parameter))
                sql = $"select {fieldname} as info from {tblname} where {code} = {parameter1}";
            else
                sql = $"select {fieldname} as info from {tblname} where {code} = @param";

            return await con.QueryFirstOrDefaultAsync<string>(sql, new { param = parameter1 });
        }

       
        public async Task<List<dynamic>> checkExists(KOTDisplayModel item)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var sql = @"Select * from Tbl_Kot_Display 
                    where Res = @Res and Tbl = @Tbl and Qty = @Qty 
                    and Cmnt = @Cmnt and KotNo = @Kot and ItemCode = @ItemCode";

            return (await con.QueryAsync<dynamic>(sql, new
            {
                item.Res,
                item.Tbl,
                item.Qty,
                Cmnt = item.Cmnts,
                Kot = item.Kot,
                ItemCode = item.Itemcode
            })).ToList();
        }

      
        public async Task<List<TblKOTDisplayModel>> GetloadItems0()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select Res, Tbl, Item, sum(Qty) as Qty, Cmnts as Cmnt, Time, Kot, Priority, depcode, itemcode" +
                " From View_KotDisplay " +
                " where kdqty is NULL " +
                " Group by Res, Tbl, Item, Cmnts, Time, Kot, Itemcode, Priority, depcode, itemcode " +
                " ORDER BY Priority";

            var viewkotdisplaylist = (await connection.QueryAsync<KOTDisplayModel>(selectquery)).ToList();

            foreach (var item in viewkotdisplaylist)
            {
                bool returnready = await ReturnReadyStatus(item.Priority, item.Kot, item.Itemcode, connection);
                if (returnready == false)
                {
                    bool checckprio = await checkpriority(item.Priority, item.Kot, item.Tbl, connection);
                    if (checckprio == false)
                    {
                        await GetinsertIntoKOTDisplay(item, connection);
                    }
                }
            }

            var selectquery1 = "select Res, Tbl, Item, sum(Qty) as Qty, Cmnts as Cmnt, Time, Kot, Priority, depcode, itemcode " +
                " From View_KotDisplayFastFood " +
                " where kdqty is NULL " +
                " Group by Res, Tbl, Item, Cmnts, Time, Kot, Itemcode, Priority, depcode, itemcode " +
                " ORDER BY Priority";

            var kotdisplayfastfooblist = (await connection.QueryAsync<KOTDisplayModel>(selectquery1)).ToList();

            foreach (var item in kotdisplayfastfooblist)
            {
                bool returnready = await ReturnReadyStatus(item.Priority, item.Kot, item.Itemcode, connection);
                if (returnready == false)
                {
                    bool checckprio = await checkpriority(item.Priority, item.Kot, item.Tbl, connection);
                    if (checckprio == false)
                    {
                        await GetinsertIntoKOTDisplay(item, connection);
                    }
                }
            }

            var selectquery2 = "Select * from Tbl_Kot_Display Order by KotNo asc";
            var kotdisplaylist = (await connection.QueryAsync<TblKOTDisplayModel>(selectquery2)).ToList();

            return kotdisplaylist;
        }

        public async Task<bool> ReturnReadyStatus(int priority, string kotno, string itemcode, IDbConnection con)
        {
            bool status = false;

            var ds = await GetloadItems2(kotno, priority, itemcode, con);

            if (ds.Count > 0)
            {
                status = true;
            }

            return status;
        }

        public async Task<bool> checkpriority(int priority, string kotno, string tblno, IDbConnection con)
        {
            bool returnstatus = false;
            var ds = await GetloadItems4(kotno, priority, tblno, con);

            if (ds.Count > 0)
            {
                returnstatus = true;
            }
            if (priority > 2)
            {
                returnstatus = false;
            }
            bool cmntcheck = await checkcmnts(kotno, con);
            if (cmntcheck == true && returnstatus == true)
            {
                returnstatus = false;
            }
            return returnstatus;
        }

        public async Task<bool> checkcmnts(string kotno, IDbConnection con)
        {
            bool check = false;
            var ds = await GetloadItems5(kotno, con);
            if (ds.Count > 0)
            {
                check = true;
            }
            return check;
        }

        public async Task GetinsertIntoKOTDisplay(KOTDisplayModel item, IDbConnection con)
        {
            //using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select * from Tbl_Kot_Display where Res = @res and Tbl = @tbl and Qty = @qty and Cmnt = @cmnt and KotNo = @kotno and ItemCode = @itemcode";

            var kotdisplaylist = (await con.QueryAsync<KOTDisplayModel>(selectquery,
                new
                {
                    res = item.Res,
                    tbl = item.Tbl,
                    qty = item.Qty,
                    cmnt = item.Cmnts,
                    kotno = item.Kot,
                    itemcode = item.Itemcode
                })).ToList();

            if (kotdisplaylist.Count == 0)
            {
                await InsertIntoKOTDisplay(item, con);
            }
        }

        public async Task InsertIntoKOTDisplay(KOTDisplayModel item, IDbConnection con)
        {
            //using var connection = _factory.CreateConnection(DbNames.POS);
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Res", item.Res);
                param.Add("@Tbl", item.Tbl);
                param.Add("@Item", item.Item ?? "");
                param.Add("@Qty", item.Qty);
                param.Add("@Cmnt", item.Cmnts ?? "");
                param.Add("@KotTime", item.Time ?? "");
                param.Add("@KotNo", Convert.ToInt32(item.Kot));
                param.Add("@Barked", 0);
                param.Add("@BarkedTime", 0);
                param.Add("@Ready", 0);
                param.Add("@ReadyTime", 0);
                param.Add("@Picked", 0);
                param.Add("@PickedTime", 0);
                param.Add("@UserId", 0);
                param.Add("@ExtraField", 0);
                param.Add("@Extrafield1", 0);
                param.Add("@Priority", item.Priority);
                param.Add("@ItemCode", Convert.ToInt32(item.Itemcode));
                param.Add("@depCode", Convert.ToInt32(item.depcode));

                await con.ExecuteAsync("InsertIntoTblKotDisplay", param, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<List<KOTDisplayModel>> GetloadItems1(int depcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select Res, Tbl, Item, Qty, Cmnts, Time, Kot, Itemcode, Priority, depcode, itemcode " +
                " From View_KotDisplay " +
                " Where kdqty is NULL and depcode = @depcode " +
                " Group by Res, Tbl, Item, Qty, Cmnts, Time, Kot, Itemcode, Priority, depcode, itemcode" +
                " ORDER BY Priority";

            return (await connection.QueryAsync<KOTDisplayModel>(selectquery, new { depcode })).ToList();
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems2(string kotno, int priority, string itemcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select * from Tbl_Kot_Display where Ready = '0' and priority = @priority and kotno = @kotno and itemcode = @itemcode ";

            return (await connection.QueryAsync<TblKOTDisplayModel>(selectquery, new { kotno, priority, itemcode })).ToList();
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems3(int depcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT Res, Tbl, item, Qty, Cmnt, KotTime, KotNo, Barked, Ready, Itemcode, BarkedTime  " +
                " FROM Tbl_Kot_Display " +
                " WHERE Ready = '0' AND depcode = @depcode AND Barked = '1' " +
                " ORDER BY KotTime";

            var result = await connection.QueryAsync<TblKOTDisplayModel>(query, new { depcode });
            return result.ToList();
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems4(string kotno, int priority, string tblno)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT * FROM Tbl_Kot_Display " +
                " WHERE Ready = '0' AND Priority < @priority AND kotno = @kotno AND Tbl = @tblno";

            var result = await connection.QueryAsync<TblKOTDisplayModel>(query, new { kotno, priority, tblno });
            return result.ToList();
        }

        public async Task<List<ViewKOTDisplayModel>> GetloadItems5(string kotno)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT * FROM View_KotDisplay " +
                " WHERE (Cmnts = 'Parcel' OR Cmnts = 'All Together' OR LEN(checkinno) > 1) AND kot = @kotno";

            var result = await connection.QueryAsync<ViewKOTDisplayModel>(query, new { kotno });
            return result.ToList();
        }

        public async Task<List<KOTDisplayModel>> GetloadItems6()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT Res, Tbl, item, Qty, Cmnt, KotTime, KotNo, Barked, Ready, Itemcode, BarkedTime " +
                " FROM Tbl_Kot_Display " +
                " WHERE Ready = '0' " +
                " AND RES != 'BLU' " +
                " ORDER BY KotTime, KotNo";

            var result = await connection.QueryAsync<KOTDisplayModel>(query);
            return result.ToList();
        }

        public async Task<List<KOTModifyDetailsModel>> GetloadItems7(string kotno, string itemcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT km.KOTNo, kd.ItemQty AS KOTDQty " +
                " FROM KOTMaster km " +
                " INNER JOIN KOTModifyDetails kd  ON km.KOTNo = kd.KOTId  " +
                " WHERE km.KotNo = @kotno AND kd.itemcode = @itemcode";

            var result = await connection.QueryAsync<KOTModifyDetailsModel>(query, new { kotno, itemcode });
            return result.ToList();
        }

        public async Task<List<KOTModifyDetailsModel>> GetloadItems8(string kotno, string itemcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = @"SELECT * FROM KOTModifyDetails WHERE itemcode = @itemcode AND KOTId = @kotno";

            var result = await connection.QueryAsync<KOTModifyDetailsModel>(query, new { kotno, itemcode });
            return result.ToList();
        }

        public async Task<List<KOTnQtyModel>> GetloadItems9(string kotno, string itemcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT v.RefKotNo AS KOTNo, v.CancelQty AS KOTDQty " +
                " FROM View_VoidKOt v " +
                " INNER JOIN Tbl_Kot_Display dis ON dis.KotNo = v.RefKotNo " +
                " WHERE v.ItemCode = @itemcode AND v.RefKotNo = @kotno AND dis.Ready = '0' AND v.CancelQty = v.ItemQty";

            var result = await connection.QueryAsync<KOTnQtyModel>(query, new { kotno, itemcode });
            return result.ToList();
        }

        public async Task<List<KOTDisplayModel>> GetloadItems10(string tblno)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var orderBy = tblno == "0" ? "Tbl" : "Item";

            var query = $@"SELECT Res, Tbl, item, Qty, Cmnt, KotTime, KotNo, Barked, Ready, Itemcode 
                   FROM Tbl_Kot_Display 
                   WHERE Ready = '0'
                   ORDER BY {orderBy}";

            var result = await connection.QueryAsync<KOTDisplayModel>(query);
            return result.ToList();
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems11(int depcode, string tblno)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var orderBy = tblno == "0" ? "Tbl" : "Item";

            var query = $@"SELECT Res, Tbl, item, Qty, Cmnt, KotTime AS Time, KotNo AS Kot, Barked, Ready, Itemcode 
                   FROM Tbl_Kot_Display 
                   WHERE Ready = '0' AND depcode = @depcode
                   ORDER BY {orderBy}";

            var result = await connection.QueryAsync<TblKOTDisplayModel>(query, new { depcode });
            return result.ToList();
        }

        public async Task<List<string>> GetloadItems12()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = @"SELECT CONVERT(nvarchar, Tbl) + '-' + CONVERT(nvarchar, item) + '-' + '(' + CONVERT(nvarchar, KotNo) + ')' AS UnPickItem
                  FROM Tbl_Kot_Display 
                  WHERE Barked = '1' AND Ready = '1' AND Picked = '0'
                  ORDER BY item";

            var result = await connection.QueryAsync<string>(query);
            return result.ToList();
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems13()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = @"SELECT CONVERT(nvarchar, Tbl) AS Tbl, CONVERT(nvarchar, item) + '(' + CONVERT(nvarchar, dis.Qty) + ')' AS Res,
                    km.KOTNo, dis.ItemCode, Cmnt
                  FROM Tbl_Kot_Display dis
                  INNER JOIN KOTMaster km ON dis.KotNo = km.KOTNo
                  WHERE Ready = '1' AND Picked = '0' AND km.KOTSettled = '0'
                  ORDER BY item";

            var result = await connection.QueryAsync<TblKOTDisplayModel>(query);
            return result.ToList();
        }

        public async Task<List<string>> GetloadItems14(string tblno)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = @"SELECT  CONVERT(nvarchar, km.KOTTblNo) + '-' +  CONVERT(nvarchar, im.ItemName) + '-' + 
                    '(' + CONVERT(nvarchar, kdis.Qty) + ')' + '(' + CONVERT(nvarchar, RIGHT(CONVERT(CHAR(20), km.KOTTime, 22), 9)) + ')' AS UnPickItem
                  FROM kotmaster km
                  INNER JOIN kotdetails kd ON km.kotid = kd.kotid
                  INNER JOIN itemmaster im ON kd.itemcode = im.itemcode
                  INNER JOIN stewardmaster stw ON km.stwcode = stw.stwcode
                  INNER JOIN Tbl_Kot_Display kdis  ON kdis.Tbl = km.KOTTblNo AND kdis.KotNo = km.KOTNo AND kdis.ItemCode = kd.ItemCode
                  WHERE km.kotsettled = 0 AND km.KotChargeable = '1' AND km.kotcancelled = 0 AND kd.rawornot IS NULL AND im.ItemName LIKE '%' + @tblno + '%'
                  GROUP BY km.KOTTblNo, im.itemname, kdis.Qty, km.KotTime
                  ORDER BY im.ItemName";

            var result = await connection.QueryAsync<string>(query, new { tblno });
            return result.ToList();
        }

        public async Task<bool> UpdateReadyAsync(TblKOTDisplayModel model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var updatequery = @"Update Tbl_Kot_Display set Ready = '1',ReadyTime = @currentime
                                where tbl = @tbl and item = @item and kotno = @kotno";

            var rows = await connection.ExecuteAsync(updatequery, new { currentime = DateTime.Now.ToShortTimeString(), tbl = model.Tbl , item = model.Item, kotno = model.KotNo });

            return rows > 0;
        }

        public async Task<bool> UpdateKotDetailsAsync(TblKOTDisplayModel model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var updatequery = @"Update KotDetails set KNQty = 1 where itemcode = @itemcode and kotno = @kotno and (KNQty IS NULL OR KNQty = 1)";

            var rows = await connection.ExecuteAsync(updatequery, new { itemcode = model.ItemCode, kotno = model.KotNo });

            return rows > 0;
        }


        public async Task<List<TblKOTDisplayModel>> GetloadItems2(string kotno, int priority, string itemcode, IDbConnection con)
        {
            //using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select * from Tbl_Kot_Display where Ready = '0' and priority = @priority and kotno = @kotno and itemcode = @itemcode ";

            return (await con.QueryAsync<TblKOTDisplayModel>(selectquery, new { kotno = Convert.ToInt32(kotno), priority = Convert.ToInt32(priority), itemcode = Convert.ToInt32(itemcode) })).ToList();
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems4(string kotno, int priority, string tblno, IDbConnection con)
        {
            //using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT * FROM Tbl_Kot_Display " +
                " WHERE Ready = '0' AND Priority < @priority AND kotno = @kotno AND Tbl = @tblno";

            var result = await con.QueryAsync<TblKOTDisplayModel>(query, new { kotno, priority, tblno });
            return result.ToList();
        }

        public async Task<List<ViewKOTDisplayModel>> GetloadItems5(string kotno, IDbConnection con)
        {
            //using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT * FROM View_KotDisplay " +
                " WHERE (Cmnts = 'Parcel' OR Cmnts = 'All Together' OR LEN(checkinno) > 1) AND kot = @kotno";

            var result = await con.QueryAsync<ViewKOTDisplayModel>(query, new { kotno });
            return result.ToList();
        }
    }
}
