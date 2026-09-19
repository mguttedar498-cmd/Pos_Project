using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Text.Json.Serialization;

namespace HMS_360_PMS.ProjectEntitiesModels.Master
{

    #region CompanyMaster

    public class CompaniesMaster
    {
        public int CompanyCode { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        //[JsonPropertyName("GSTNo")]
        public string GSTNo { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public DateTime? LastModify { get; set; } = DateTime.Now;
        public string Branch_code { get; set; } = string.Empty;
    }

    #endregion


    #region Tax Master
    public class BillTaxMaster
    {
        public int TaxCode { get; set; }
        public string? TaxName { get; set; } = string.Empty;
        public double TaxPercentage { get; set; } = 0.0;
        public bool IsActive { get; set; } = false;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string UserCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        //public DateTime CreatedOn { get; set; } = DateTime.Now;
    }

    public class BillTaxDescription
    {
        public int TaxDescId { get; set; } = 0;
        public int TaxCode { get; set; }
        public string? TaxDescription { get; set; } = string.Empty;
        public double TaxPercentage { get; set; } = 0.0;
        public bool IsActive { get; set; } = false;
        public string UserCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        //public DateTime CreatedOn { get; set; } = DateTime.Now;
    }

    #endregion

    #region DepartmentMaster

    public class DepartmentMaster
    {
        public int DepCode { get; set; }
        public string DepName { get; set; } = string.Empty;
        public string DepHead { get; set; } = string.Empty;
        public string POSCode { get; set; } = string.Empty;
        public string Branch_code { get; set; } = string.Empty;
    }

    #endregion

    #region OutletMaster

    public class OutletsMaster
    {
        public int OltCode { get; set; }
        public int POSCode { get; set; } = 0;
        public string OltName { get; set; } = string.Empty;
        public bool OltIsRoomService { get; set; } = false;
        public bool OltServiceTaxRequired { get; set; } = false;
        public string OltAddress1 { get; set; } = string.Empty;
        public string OltAddress2 { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public DateTime LastModify { get; set; } = DateTime.Now;
        public decimal ServiceCharge { get; set; } = 0.0m;
        public string Branch_Code { get; set; } = string.Empty;
        public bool OltIsParcelService { get; set; } = false;
        public char IsUploaded { get; set; } = 'N';
        public char IsModified { get; set; } = 'N';
        public string TinNo { get; set; } = string.Empty;
        public decimal SBCess { get; set; } = 0.0m;
        public decimal KKCess { get; set; } = 0.0m;
        public bool InExTax { get; set; } = false;
        public bool OltIsFastFood { get; set; } = false;
        public bool IsDirectKOTandBill { get; set; } = false;
        public bool IsDirectPaxandStw { get; set; } = false;
        public bool IsDirectBill { get; set; } = false;
    }

    #endregion

    #region TableMaster

    public class TablesMaster
    {
        public decimal? TblCode { get; set; } = 0;
        public string OltCode { get; set; } = string.Empty;
        public string TblNo { get; set; } = string.Empty;
        public int TblSeatCount { get; set; } = 0;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string POSCODE { get; set; } = string.Empty;
        public int c { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public string TableQRImage { get; set; } = string.Empty;
    }

    public class InsertUpdateTableMaster
    {
        public decimal TblCode { get; set; } = 0;
        public string OltCode { get; set; } = string.Empty;
        public string TblNo { get; set; } = string.Empty;
        public int TblSeatCount { get; set; } = 0;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string POSCODE { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string TableQRImage { get; set; } = string.Empty;

    }


    #endregion

    #region UnitMaster

    public class UnitMaster
    {
        public int UnitCode { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitSymbol { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
    }
    #endregion


    #region GroupMaster

    public class GroupMaster
    {
        public int GrpCode { get; set; }
        public string GrpName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string Isuploaded { get; set; } = string.Empty;
        public string Dep { get; set; } = string.Empty;
    }

    #endregion

    #region CategoryMaster

    public class CategoryMaster
    {
        public int CatCode { get; set; }
        public string CatName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string SubCat { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    #endregion

    #region SubCategoryMaster

    public class SubCategoryMaster
    {
        public int CatCode { get; set; }
        public string CatName { get; set; } = string.Empty;
        public int SubCatCode { get; set; } = 0;
        public string SubCatName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string TrDate { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string SubCat { get; set; } = string.Empty;
    }

    #endregion

    #region ItemMaster

    public class MasterItemModel
    {
        public int ItemCode { get; set; }              // numeric(10,0)
        public string ItemName { get; set; } = string.Empty;
        public string ItemDisplayName { get; set; } = string.Empty;
        public double QPB { get; set; } = 0.0;                    // float
        public decimal CatCode { get; set; } = 0;               // numeric(10,0)
        public string GrpCode { get; set; } = string.Empty;
        public bool ItemDiscountAllowed { get; set; } = false;      // bit
        public double ItemRate { get; set; } = 0.0;               // float
        public double ItemSaleQtyUnit { get; set; } = 0.0;        // float
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;             // nvarchar(max)
        public string Unit { get; set; } = string.Empty;            // nvarchar(max)
        public string ItemType { get; set; } = string.Empty;            // nvarchar(max)
        public string MostRunningItemSrNo { get; set; } = string.Empty;
        public int SubItem { get; set; } = 0;
        public decimal ItemOpStock { get; set; } = 0;         // numeric(18,2)
        public decimal ItemCurStock { get; set; } = 0;
        public decimal ItemOpRate { get; set; } = 0;           // money
        public decimal ItemCurRate { get; set; } = 0;
        public decimal UnitCode { get; set; } = 0;
        public float ItemROQ { get; set; } = 0;                // real
        public float ItemROL { get; set; } = 0;                 // real
        public string Dep { get; set; } = string.Empty;                   // char(1)
        public decimal Opstock { get; set; } = 0;
        public decimal Ctstock { get; set; } = 0;
        public decimal PerQty { get; set; } = 0;
        public string DepCode { get; set; } = string.Empty;
        public decimal PerRate { get; set; } = decimal.Zero;
        public string SUnit { get; set; } = string.Empty;
        public string branch_code { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Thumb { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public bool IsVeg { get; set; } = false;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public string OltCode { get; set; } = string.Empty;
        //public string OutletName { get; set; } = string.Empty;

    }
    public class InsertMasterItemModel
    {
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public string CatCode { get; set; } = string.Empty;
        public string SubCatCode { get; set; } = string.Empty; 
        public string GrpCode { get; set; } = string.Empty;
        public bool ItemDiscountAllowed { get; set; } = false;
        public double ItemRate { get; set; } = 0.0; 
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public decimal UnitCode { get; set; } = 0;
        public string UnitName { get; set; } = string.Empty;
        public string Dep { get; set; } = string.Empty;
        public string DepCode { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public string PrintDepartment { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string SACCode { get; set; } = string.Empty;
        public string Thumb { get; set; } = string.Empty;
        //public IFormFile Image { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public bool IsVeg { get; set; } = false;
        public List<int> OltCodes { get; set; } = new();
        //public string OutletName { get; set; } = string.Empty;
        //public bool IsOltchanged { get; set; } = false;
        //public List<int> oldOltCode { get; set; } = new();

    }

    public class ProductCreate
    {
        //public string Name { get; set; }

        public IFormFile Image { get; set; }
    }

    #endregion

    #region StewardMaster

    public class MasterSteward
    {
        public int StwCode { get; set; }
        public string POSCode { get; set; } = string.Empty;
        public string StwName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string MobNo { get; set; } = string.Empty;
    }

    #endregion

    #region NCDepartmentMaster

    public class NCDepartmentMaster
    {
        public int NCDepCode { get; set; }
        public string NCDepName { get; set; } = string.Empty;
        public string Userid { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
    }

    #endregion

    #region PrintingMaster

    public class PrintingMaster
    {
        public int DepCode { get; set; }
        public string DepName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string IsUploaded { get; set; } = string.Empty;
    }
    #endregion

    #region MultipleItemMaster
    public class ImportItemRequest
    {
        public List<ImportItemRow> Items { get; set; }
        public string UserCode { get; set; }
        public string BranchCode { get; set; }
        public List<int> OltCodes { get; set; }
    }

    public class ImportItemRow
    {
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        //public string Outlet { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;

        //public string SubGroup { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Rate { get; set; } = 0;
        public string Tax { get; set; } = string.Empty;
        public string PrintDept { get; set; } = string.Empty;
        //public string GuestPoint { get; set; } = string.Empty;
        //public string CapPoint { get; set; } = string.Empty;
        public string SacCode { get; set; } = string.Empty;
        public bool IsVeg { get; set; } = false;
        //public string UserCode { get; set; } = string.Empty;
        //public string BranchCode { get; set; } = string.Empty;
    }
    #endregion

    #region OutletItemDetails
    //public class OutletItemDetails
    //{
    //    //public string OltCode { get; set; }  = string.Empty;
    //    public int ItemCode { get; set; }  = 0;
    //    public string ItemName { get; set; } = string.Empty;
    //    public double ItemRate { get; set; } = 0.0;
    //    public double OIDRate { get; set; } = 0.0;
    //    public bool OIDAvailable { get; set; } = false;
    //    public string TaxCode { get; set; } = string.Empty;
    //    public string Vatper { get; set; } = string.Empty;
    //    public double Discount { get; set; } = 0.0;
    //    public string FreeItemCode { get; set; } = string.Empty;
    //    public string FreeItemName { get; set; } = string.Empty;
    //    public string FreeItemQty { get; set; } = string.Empty;
    //    public bool IsHappyHour { get; set; } = false;
    //    //public bool IsFree { get; set; } = false;
    //    public int GrpCode { get; set; } = 0;

    //    //public string branchcode { get; set; } = string.Empty;
    //    //public string UserCode { get; set; } = string.Empty;
    //}
    public class OutletItemDetails
    {
        public int ItemCode { get; set; }  = 0;
        public string ItemName { get; set; } = string.Empty;
        public double OIDRate { get; set; } = 0.0;
        public bool OIDAvailable { get; set; } = false;
        public double Discount { get; set; } = 0.0;
        public string FreeItemCode { get; set; } = string.Empty;
        public string FreeItemName { get; set; } = string.Empty;
        public string FreeItemQty { get; set; } = string.Empty;
        public bool IsHappyHour { get; set; } = false;
        public int GrpCode { get; set; } = 0;
    }

    public class CreateOutletRequest
    {
        public string OltCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;

        public bool IsTaxIncluded { get; set; } = false;

        public string? TaxCode { get; set; } = string.Empty;
        public string? TaxName { get; set; } = string.Empty;
        public string? ItemGroup { get; set; } = string.Empty;

        public List<OutletItemDetails> OltDetails { get; set; } = new List<OutletItemDetails>();
    }

    public class IncrementRequest
    {
        public decimal? Amount { get; set; } = 0;   // txtamt
        public decimal? Percentage { get; set; } = 0;   // txtper
        public int GrpCode { get; set; } = 0;
        public List<OutletItemDetails> Items { get; set; } = new List<OutletItemDetails>();
    }

    public class SaveExtraChargesRequest
    {
        public string OutletCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public List<int> ItemCodes { get; set; } = new List<int>();
    }
    #endregion

    #region PropertyMaster/ Branchmaster

    public class PropertyMasterModel
    {
        public string Company_Name { get; set; } = string.Empty;
        public int Company_code { get; set; } = 0;
        public DateTime StartYear { get; set; } = DateTime.MinValue;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Phone_number { get; set; } = string.Empty;
        public string Mob_number { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public long Owner_Number { get; set; } = 0;
        public long Fax_number { get; set; } = 0;
        public string Email_id { get; set; } = string.Empty;
        public string Tin_no { get; set; } = string.Empty;
        public string Licence_number { get; set; } = string.Empty;
        public string Branch_code { get; set; } = string.Empty;
        public string STDCODE { get; set; } = string.Empty;
    }

    public class BranchMasterModel
    {
        public int BrId { get; set; } = 0;
        public int Company_code { get; set; } = 0;
        public string Branch_code { get; set; } = string.Empty;
        public string Branch_name { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public long Phone_number { get; set; } = 0;
        public long Mob_number { get; set; } = 0;
        public long Fax_number { get; set; } = 0;
        public string Email_id { get; set; } = string.Empty;
        public string Tin_no { get; set; } = string.Empty;
        public string Licence_number { get; set; } = string.Empty;
    }

    #endregion

    #region AddOnItems

    public class AddOnItemModel
    {
        //public int AddOnId { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public int AddOnItemCode { get; set; } = 0;
        public string AddOnName { get; set; } = string.Empty;
        public decimal ItemRate { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string UserCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string thumb { get; set; } = string.Empty;
    }

    public class AdditionalOnItemModel
    {
        public int ItemCode { get; set; } = 0;
        public string AddOnName { get; set; } = string.Empty;
        public decimal ItemRate { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string UserCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string thumb { get; set; } = string.Empty;
    }

    #endregion

}
