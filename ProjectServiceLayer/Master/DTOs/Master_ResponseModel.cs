using HMS_360_PMS.ProjectEntitiesModels.Master;

namespace HMS_360_PMS.ProjectServiceLayer.Master.DTOs
{
    //public class ApiResponse<T>
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //    public T Data { get; set; }

    //    public static ApiResponse<T> SuccessResult(T data, string message = null)
    //        => new() { Success = true, Data = data, Message = message };

    //    public static ApiResponse<T> Failure(string message)
    //        => new() { Success = false, Message = message };
    //}

    #region ItemMaster

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? Data { get; set; }
    }

    #endregion

    #region StewardMaster

    #endregion

    #region ImportItemMaster

    public class ImportResult
    {
        public List<ImportItemRow> ValidRows { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class ImportSummary
    {
        public int Total { get; set; }
        public int Inserted { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }

        public List<string> Errors { get; set; } = new();
    }
    #endregion


    #region OutletItemDetails

    public class IncrementResponse
    {
        public List<decimal> UpdatedValues { get; set; }
        public string Message { get; set; }
    }

    #endregion

}
