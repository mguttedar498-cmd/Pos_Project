using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces
{
    public interface IGraceTimeValidate_Service
    {
        Task<ApiResponse> ValidateDayAsync(DayValidationRequest request);
    }
}
