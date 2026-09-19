using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using System.Text.Json.Serialization;


namespace HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels
{
    #region Inventory Supplier
    public class InventorySupplierMaster
    {
        public decimal? SupCode { get; set; }
        public string? SupName { get; set; } = string.Empty;
        public string? SupCPerson { get; set; } = string.Empty;
        public string? SupAdd1 { get; set; } = string.Empty;
        public string? SupAdd2 { get; set; } = string.Empty;
        public string? SupAdd3 { get; set; } = string.Empty;
        public string? SupPhone { get; set; } = string.Empty;
        public string? SupFax { get; set; } = string.Empty;
        public string? SupMobile { get; set; } = string.Empty;
        public string? SupLSTNo { get; set; } = string.Empty;
        public DateTime? SupLSTDate { get; set; }
        public string? SupCSTNo { get; set; } = string.Empty;
        public DateTime? SupCSTDate { get; set; }
        public int? AcGroupCode { get; set; }
        public int? AcCode { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? BranchCode { get; set; } = string.Empty;
        public string? SuspPincode { get; set; } = string.Empty;
        public string? SupCity { get; set; } = string.Empty;
        public string? GSTNo { get; set; } = string.Empty;
        public string? TinNo { get; set; } = string.Empty;
    }
    #endregion

    #region CategoryMaster
    public class InventoryCategoryMaster
    {
        public int CatCode { get; set; }
        public string CatName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
    #endregion

    #region SubCategoryMaster

    public class InventorySubCategoryMaster
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

    #region Store Master
    public class InventoryStoreMaster
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreLocation { get; set; } = string.Empty;
        public string StoreIncharge { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
    }
    #endregion

    #region Item Store
    public class InventoryMasterItemModel
    {
        public int ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal CatCode { get; set; } = 0;
        public decimal SubCatCode { get; set; } = 0;
        public string Storeid { get; set; } = string.Empty;
        public string GrpCode { get; set; } = string.Empty;
        public decimal UnitCode { get; set; } = 0;
        public string UnitName { get; set; } = string.Empty;
        public string PurchaseRate { get; set; } = string.Empty;
        public double NoofUnits { get; set; } = 0.0;
        public double ItemRate { get; set; } = 0.0;
        public decimal ItemOpStock { get; set; } = 0;
        public decimal ItemOpRate { get; set; } = 0;
        public float ItemROQ { get; set; } = 0;
        public float ItemROL { get; set; } = 0;
        public string? BarCode { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string MostRunningItemSrNo { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public int FirstUnit { get; set; } = 0;
        public string FirstUnitDesc { get; set; } = string.Empty;
        public int FinalUnit { get; set; } = 0;
        public string FinalUnitDesc { get; set; } = string.Empty;
    }

    public class InventoryServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? Data { get; set; }
    }
    #endregion

    #region Misc Master
    public class MiscellaneousInventory
    {
        public int ChargeId { get; set; }
        public string ChargeName { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public int TaxCode { get; set; }
    }
    #endregion

    #region GRN Misc Master
    public class GRNMiscellaneousInventory
    {
        public int GRNId { get; set; }
        public int Pno { get; set; }
        public int ChargeId { get; set; }
        public decimal? ChargeAmt { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
    }
    #endregion

    #region MultipleItemMaster
    public class InventoryImportItemRequest
    {
        public List<InventoryImportItemRow> Items { get; set; }
        public string UserCode { get; set; }
        public string BranchCode { get; set; }
        public List<int> Storeids { get; set; }
    }

    public class InventoryImportItemRow
    {
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal ItemRate { get; set; } = 0;
        public string Tax { get; set; } = string.Empty;
        public int NoofUnits { get; set; } = 0;
        public decimal PurchaseRate { get; set; } = 0;
    }
    public class InventoryImportResult
    {
        public List<InventoryImportItemRow> ValidRows { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class InventoryImportSummary
    {
        public int Total { get; set; }
        public int Inserted { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    #endregion

    #region UnitConversionMaster
    public class UnitConversionMaster
    {
        public int UnitCode { get; set; } = 0;
        public string UnitName { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public bool IsActive { get; set; } = true;
        public string Branch_Code { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
    #endregion

    #region Terms And Conditions Master
    public class TermsAndConditionsMaster
    {
        public int TermsCode { get; set; }
        public string TermsTitle { get; set; } = string.Empty;
        public string TermsDescription { get; set; } = string.Empty;
        public string MasterName { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
    }
    #endregion
}
