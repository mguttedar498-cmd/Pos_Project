using HMS_360_PMS.DAL_Layers.KOT;
using HMS_360_PMS.HMS_360_PMS.Infrastructure.KOT;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;
using Microsoft.Extensions.Options;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Services
{
    public class KOTDisplay_Service : IKOTDisplay_Service
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKOTDisplay_Repository _repository;
        private readonly KOTDisplay_DAL _kotdal;

        public KOTDisplay_Service(IHttpClientFactory httpClientFactory, IKOTDisplay_Repository repository, KOTDisplay_DAL kotdal)
        {
            _httpClientFactory = httpClientFactory;
            _repository = repository;
            _kotdal = kotdal;
        }

        public async Task<List<TblKOTDisplayModel>> GetloadItems0()
        {
            var data = await _repository.GetloadItems0();
            return data;
        }

        // 🔹 Department pending items
        public async Task<List<KOTDisplayModel>> GetloadItems1(int depcode)
        {
            var data = await _repository.GetloadItems1(depcode);
            return data.ToList();
        }

        // 🔹 Check if item still pending

        public async Task<List<TblKOTDisplayModel>> GetloadItems2(string kotno, int priority, string itemcode)
        {
            var data = await _repository.GetloadItems2(kotno, priority, itemcode);

            return data;
        }

        // 🔹 Barked items for kitchen screen
        public async Task<List<TblKOTDisplayModel>> GetloadItems3(int depcode)
        {
            var data = await _repository.GetloadItems3(depcode);

            return data
                .Where(x => x.Barked == "1" && x.Ready == "0")
                .OrderBy(x => x.KotTime)
                .ToList();
        }

        // 🔹 Priority control (FIFO logic)
        public async Task<List<TblKOTDisplayModel>> GetloadItems4(string kotno, int priority, string tblno)
        {
            var data = await _repository.GetloadItems4(kotno, priority, tblno);

            // Business rule: older items first
            return data.OrderBy(x => x.Priority).ToList();
        }

        // 🔹 Parcel / All together / multi-checkin
        public async Task<List<ViewKOTDisplayModel>> GetloadItems5(string kotno)
        {
            return await _repository.GetloadItems5(kotno);
        }
        public async Task<List<KOTDisplayModel>> GetloadItems6()
        {
            return await _repository.GetloadItems6();
        }

        // 🔹 Modified items
        public async Task<List<KOTModifyDetailsModel>> GetloadItems7(string kotno, string itemcode)
        {
            var main = await _repository.GetloadItems7(kotno, itemcode);
            return main;
        }

        public async Task<List<KOTModifyDetailsModel>> GetloadItems8(string kotno, string itemcode)
        {
            var extra = await _repository.GetloadItems8(kotno, itemcode);
            return extra;
        }

        // 🔹 Cancelled items
        public async Task<List<KOTnQtyModel>> GetloadItems9(string kotno, string itemcode)
        {
            return await _repository.GetloadItems9(kotno, itemcode);
        }

        // 🔹 Grouped pending items
        public async Task<List<KOTDisplayModel>> GetloadItems10(string tblno)
        {
            return await _repository.GetloadItems10(tblno);
        }

        // 🔹 Department grouped items
        public async Task<List<TblKOTDisplayModel>> GetloadItems11(int depcode, string tblno)
        {
            return await _repository.GetloadItems11(depcode, tblno);
        }

        // 🔹 Ready but not picked (pickup counter)
        public async Task<List<string>> GetloadItems12()
        {
            return await _repository.GetloadItems12();
        }

        // 🔹 Active serving queue
        public async Task<List<TblKOTDisplayModel>> GetloadItems13()
        {
            var data = await _repository.GetloadItems13();

            return data
                .OrderBy(x => x.KotNo)
                .ToList();
        }

        // 🔹 Search running items
        public async Task<List<string>> GetloadItems14(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            return await _repository.GetloadItems14(searchText);
        }

        //public async Task<bool> Bark(TblKOTDisplayModel model)
        //{
        //    var update = await _repository.UpdateBarkAsync(model);
        //    var sound = await _repository.InsertSoundAsync(model);

        //    return update > 0 && sound > 0;
        //}

        public async Task<bool> Ready(TblKOTDisplayModel model)
        {
            var updateDisplay = await _repository.UpdateReadyAsync(model);
            var updateKot = await _repository.UpdateKotDetailsAsync(model);

            return updateDisplay == true && updateKot == true;
        }
    }
}
