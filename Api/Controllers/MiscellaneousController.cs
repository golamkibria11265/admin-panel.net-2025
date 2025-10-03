using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Utility.Exceptions;
using Utility;
using Dll.Models.Miscellaneous.ResponseModels;
using Bll.Services.TrxService.SendMoney.Interface;
using Bll.Services.FileStorageService.Files.Concrete;
using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Factories.Concrete.Root;
using Bll.Services.FileStorageService.Managers.Root;
using Bll.Services.FileStorageService.Context.Concrete.Ftp;
using Bll.Services.FileStorageService.Factories.Concrete.Ftp;
using Bll.Services.FileStorageService.Managers.Ftp;
using Dll.Models.Common;
using Polly;
using Bll.Services.Queue;
using Bll.Services.CommonService;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MiscellaneousController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _iIdentityService;
        private readonly IHttpClientFactory _iHttpClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _iConfiguration;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly Serilog.ILogger _logger;
        public MiscellaneousController
        (
            IHttpContextAccessor httpContextAccessor,
            IIdentityService iIdentityService,
            IHttpClientFactory iHttpClientFactory,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration iConfiguration,
            IBackgroundTaskQueue taskQueue,
            Serilog.ILogger logger
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _iIdentityService = iIdentityService;
            _iHttpClientFactory = iHttpClientFactory;
            _webHostEnvironment = webHostEnvironment;
            _iConfiguration = iConfiguration;
            _taskQueue = taskQueue;
            _logger = logger;
        }
        #region Comment Region
        /* Here The Example Is Shown, How To Handle Http Request Timeout Exaption */
        #endregion
        [HttpGet]
        [Route("Quotes")]
        public async Task<IActionResult> Quotes()
        {
            ApiResponse apiResponse = new();
            string uri = "https://api.api-ninjas.com/v1/quotes?category=fitness";
            if (uri is null)
            {
                throw new CustomApplicationException("Uri is not found");
            }
            string authKey = "Nw2mPPNgVWbmupZg6fJy/A==IEugT6hpQ5qQ5yQo";
            if (authKey is null)
            {
                throw new CustomApplicationException("Auth Key is not found");
            }
            var client = _iHttpClientFactory.CreateClient("QuoteClient");
            client.Timeout = TimeSpan.FromSeconds(30);
            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            httpRequestMessage.Headers.Add("X-Api-Key", authKey);
            string body = string.Empty;
            try
            {
                using HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                if(ex is TimeoutException)
                {
                    apiResponse.code = StatusCodes.Status408RequestTimeout;
                    apiResponse.messages = new List<string> { "Request timed out. Please try again later." };
                    return Ok(apiResponse);
                }
                else
                {
                    apiResponse.code = StatusCodes.Status500InternalServerError;
                    apiResponse.messages = new List<string> { "Something went wrong. Please try again later." };
                    return Ok(apiResponse);
                }
            }
            if (body == null)
            {
                throw new CustomApplicationException("Response body is not found");
            }
            object? data = JsonConvert.DeserializeObject<object>(body);
            if (data == null)
            {
                throw new CustomApplicationException("Data is not found");
            }
            JObject jObjectData = new JObject();
            if (data is JObject)
            {
                jObjectData = (JObject)data;
            }
            var message = jObjectData.SelectToken("message");
            if (message != null)
            {
                throw new CustomApplicationException(message.ToString());
            }
            var error = jObjectData.SelectToken("error");
            if (error != null)
            {
                throw new CustomApplicationException(error.ToString());
            }
            JToken jTokenData = new JObject();
            if (data is JToken)
            {
                jTokenData = (JToken)data;
            }
            JArray? jArray = (JArray)jTokenData;
            JToken? jToken = jArray[0];
            JObject? jObject = jToken as JObject;
            if (jObject == null)
            {
                throw new CustomApplicationException("Quote is not found");
            }
            QuoteResponseModel quote = new();
            if (jObject.TryGetValue("quote", out JToken? quoteToken))
            {
                quote.quote = quoteToken.ToString();
            }
            if (jObject.TryGetValue("author", out JToken? authorToken))
            {
                quote.author = authorToken.ToString();
            }
            if (jObject.TryGetValue("category", out JToken? categoryToken))
            {
                quote.category = categoryToken.ToString();
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string> { "success" };
            apiResponse.data = quote;
            return Ok(apiResponse);
        }
        #region Comment Region
        /* Here The Example Is Shown, How To Handle File Upload Request */
        #endregion
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequestModel request)
        {
            CommonResponse commonResponse = new CommonResponse();
            if(request.File is null)
            {
                commonResponse.code = StatusCodes.Status400BadRequest;
                commonResponse.messages = new List<string> { "No file provided" };
                return BadRequest(commonResponse);
            }
            var file = new ImageFile(request.File);
            var rootFileUploadContext = new RootFileUploadContext
            {
                File = file,
                DestinationPath = Path.Combine(_webHostEnvironment.WebRootPath, "images","temporary"),   
            };
            var rootImageUploadFactory = new RootImageUploadFactory();
            var rootFileStorageUploadFactoryManager = new RootStorageFileUploadFactoryManager<RootFileUploadContext>(rootImageUploadFactory);
            commonResponse = await rootFileStorageUploadFactoryManager.UploadAsync(rootFileUploadContext);

            if(commonResponse.code != StatusCodes.Status200OK)
            {
                return Ok(commonResponse);
            }

            var ftpFileUploadContext = new FtpFileUploadContext
            {
                SourcePath = Path.Combine(_webHostEnvironment.WebRootPath, "images","temporary"),
                DestinationPath = Path.Combine("san","images"),
                Identifier = file.Identifier
            };
            var ftpImageUploadFactory = new FtpImageUploadFactory(_iConfiguration, _logger, _iHttpClientFactory);
            var ftpStorageFileUploadFactoryManager = new FtpStorageFileUploadFactoryManager<FtpFileUploadContext>(ftpImageUploadFactory);
            commonResponse = await ftpStorageFileUploadFactoryManager.UploadAsync(ftpFileUploadContext);

            return Ok(commonResponse);
        }
        public class FileUploadRequestModel
        {
            public IFormFile? File { get; set; }
        }
        #region Comment Region
        /* Here The Example Is Shown, How To Handle Enque Request */
        #endregion
        [Authorize]
        [HttpPost("Enqueue")]
        public  IActionResult Enqueue([FromForm] RequestModel request)
        {
            CommonResponse commonResponse = new CommonResponse();

            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return BadRequest("Mobile number is required");
            }
            if (_httpContextAccessor?.HttpContext?.Request?.Headers is null)
            {
                throw new CustomApplicationException("request headers are not available");
            }
            string? authToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(authToken))
            {
                commonResponse.code = StatusCodes.Status401Unauthorized;
                commonResponse.messages = new List<string> { "authorization token is missing" };
                return Ok(commonResponse);
            }
            bool queued = _taskQueue.TryQueueBackgroundWorkItem(async cancellationToken =>
            {
                await _iIdentityService.GetAspNetUserByMobile(request.MobileNumber, authToken, cancellationToken);
            });
            if (!queued)
            {
                commonResponse.code = StatusCodes.Status429TooManyRequests;
                commonResponse.messages = new List<string> { "Server is busy. Please try again later." };
            }
            commonResponse.code = StatusCodes.Status200OK;
            commonResponse.messages = new List<string> { "Request queued" };
            commonResponse.data = request.MobileNumber;
            return Ok(commonResponse);
        }
        public class RequestModel
        {
            public string? MobileNumber { get; set; }
        }
    }
}
