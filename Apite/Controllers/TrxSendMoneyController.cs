using Microsoft.AspNetCore.Mvc;
using Utility.Exceptions;
using Utility;
using Bll.Services.TrxService.SendMoney.Interface;
using Microsoft.AspNetCore.Authorization;
using Dll.Models.RequestModels.TrxSendMoneyModels;
using Dll.Models.Dto.RequestModels.TrxSendMoneyModels;

namespace Apite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrxSendMoneyController : ControllerBase
    {
        private readonly ITrxSendMoneyService _trxSendMoneyService;

        public TrxSendMoneyController
        (
            ITrxSendMoneyService trxSendMoneyService
        )
        {
            _trxSendMoneyService = trxSendMoneyService;
        }
        [Authorize]
        [HttpPost]
        [Route("TrxInit")]
        public async Task<IActionResult> TrxInit([FromForm] TrxSendMoneyInitRequest request)
        {
            CommonResponse response = new();

            TrxSendMoneyInitRequestDto trxSendMoneyInitRequestDto = new();
            trxSendMoneyInitRequestDto.User = User;
            trxSendMoneyInitRequestDto.Amount = request.Amount;
            trxSendMoneyInitRequestDto.MobileNo = request.MobileNo;
            try
            {
                response = await _trxSendMoneyService.TrxInit(trxSendMoneyInitRequestDto);
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
        [Authorize]
        [HttpPost]
        [Route("TrxConfirm")]
        public async Task<IActionResult> TrxConfirm([FromForm] TrxSendMoneyConfirmRequest request)
        {
            CommonResponse response = new();

            TrxSendMoneyConfirmRequestDto trxSendMoneyConfirmRequestDto = new();
            trxSendMoneyConfirmRequestDto.User = User;
            trxSendMoneyConfirmRequestDto.TrxId = request.TrxId;
            try
            {
                response = await _trxSendMoneyService.TrxConfirm(trxSendMoneyConfirmRequestDto);
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
