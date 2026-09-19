using HMS_360_PMS.DAL_Layers.POS;
using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.Helper.Security;
using HMS_360_PMS.HMS_360_PMS.Infrastructure.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.Services_Layers.POS.Interfaces;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Services
{
    public class GraceTimeValidate : IGraceTimeValidate_Service
    {
        private readonly IPOS_Repository _repository;
        private readonly POS_DAL _posdal;

        public GraceTimeValidate(IPOS_Repository repository, POS_DAL posdal)
        {
            _repository = repository;
            _posdal = posdal;
        }

        public async Task<ApiResponse> ValidateDayAsync(DayValidationRequest request)
        {
            try
            {
                var SmsDetails = await _repository.GetSmsDetailsAsync(request.Branchcode);

                if (SmsDetails == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "No Records Found in utility Setting!"
                    };
                }

                DateTime sysDt = DateTime.Now;
                //DateTime sysDt = new DateTime(2026, 4, 10, 1, 25, 16);
                DateTime adjustedDateTime = sysDt.AddHours(-SmsDetails.DayCloseGraceHour);
                DateTime adjustedDate = adjustedDateTime.Date;

                bool isGraceExceeded = false;

                // Step 1: Check if POS Entry Date is older
                if (request.POSEntryDate.Date < adjustedDate)
                {
                    isGraceExceeded = true;

                    int runningKot = await _repository.GetRunningKotCountAsync(request.Branchcode);
                    if (runningKot > 0)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Clear all running KOTs in Running Order Window"
                        };
                    }

                    int pendingBills = await _repository.GetPendingBillsCountAsync(request.Branchcode);
                    if (pendingBills > 0)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Clear all pending bills from Settlement Window"
                        };
                    }
                }

                // Step 2: Check Shift Open
                var shiftDate = await _repository.GetOpenShiftDateAsync(request.Branchcode);

                if (shiftDate.HasValue)
                {
                    if (isGraceExceeded)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Grace time is completed, please close the day first to continue further!"
                        };
                    }
                    else if (request.POSEntryDate.Date == adjustedDate && request.POSEntryDate.Date == shiftDate.Value.Date)
                    {
                        return new ApiResponse
                        {
                            Success = true,
                            Message = "Validation successful"
                        };
                    }
                    else
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Please close the day first to continue further!"
                        };
                    }
                }
                //if (string.IsNullOrEmpty(shiftDate) || shiftDate.Length != 10)
                //{
                //    return new ApiResponse
                //    {
                //        Success = false,
                //        Message = "Please open the day first to continue further!"
                //    };
                //}
                else
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Please open the day first to continue further!"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"An error occurred during validation: {ex.Message}"
                };
            }
        }
    }
}
