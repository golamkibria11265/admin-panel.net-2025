using Bll.Services.TrxService.SendMoney.Interface;
using Dll.Models.Dto.RequestModels.TrxStoredProcedureModels;
using Microsoft.AspNetCore.Mvc;
using Utility.Exceptions;
using Utility;
using Microsoft.AspNetCore.Authorization;

namespace Apite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrxStoredProcedureController : ControllerBase
    {
        private readonly ITrxStoredProcedureService _trxStoredProcedureService;

        public TrxStoredProcedureController
        (
            ITrxStoredProcedureService trxStoredProcedureService
        )
        {
            _trxStoredProcedureService = trxStoredProcedureService;
        }
        [Authorize]
        [HttpPost]
        [Route("TrxDailyDayClose")]
        public async Task<IActionResult> TrxDailyDayClose()
        {
            CommonResponse response = new();

            TrxDailyDayCloseDto trxDailyDayCloseDto = new();
            try
            {
                response = await _trxStoredProcedureService.TrxDailyDayClose(trxDailyDayCloseDto);
            }
            catch (CustomApplicationException ex)
            {
                response.code = StatusCodes.Status400BadRequest;
                response.messages = new List<string> { ex.Message };
                return Ok(response);
            }
            catch (Exception)
            {
                response.code = StatusCodes.Status500InternalServerError;
                response.messages = new List<string> { "something went wrong. please try again later." };
                return Ok(response);
            }
            response.code = response.code;
            response.messages = response.messages;
            return Ok(response);
        }
    }
}
