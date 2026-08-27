#if (DEBUG && !UI_DEMO) || (!DEBUG && UI_DEMO)
using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public sealed partial class MockCloudServiceClient : ICloudServiceClient, IAuthMessageClient, IVersionClient, IActiveUserClient, IAccelerateClient, IScriptClient, INoticeClient
    {
        readonly IToast toast;
        readonly IModelValidator validator;
        readonly ICloudServiceClient real;

        public MockCloudServiceClient(IToast toast, IModelValidator validator, CloudServiceClientBase real)
        {
            this.toast = toast;
            this.validator = validator;
            this.real = real;
        }

        public string ApiBaseUrl => real.ApiBaseUrl;

        public IScriptClient Script => this;

        public IAuthMessageClient AuthMessage => this;

        public IVersionClient Version => this;

        public IActiveUserClient ActiveUser => this;

        public IAccelerateClient Accelerate => this;

        public INoticeClient Notice => this;

        #region ModelValidator

        IApiResponse? ModelValidator<TRequestModel>(TRequestModel requestModel) => ModelValidator<TRequestModel, object>(requestModel);

        IApiResponse<TResponseModel>? ModelValidator<TRequestModel, TResponseModel>(TRequestModel requestModel)
        {
            if (requestModel != null && typeof(TRequestModel) != typeof(object))
            {
                if (!validator.Validate(requestModel, out var errorMessage))
                {
                    return ApiResponse.Code<TResponseModel>(
                        ApiResponseCode.RequestModelValidateFail, errorMessage);
                }
            }
            return null;
        }

        #endregion

        void ShowResponseErrorMessage(IApiResponse response)
        {
            if (response.Code == ApiResponseCode.Canceled) return;
            var message = response.Message;
            toast.Show(message);
        }

        void GlobalResponseIntercept(IApiResponse response)
        {
            if (!response.IsSuccess)
            {
                ShowResponseErrorMessage(response);
            }
        }

        public Task<IApiResponse<AppVersionDTO?>> CheckUpdate2(Guid id,
            Platform platform,
            DeviceIdiom deviceIdiom,
            Version version,
            Architecture architecture,
            DeploymentMode deploymentMode)
        {
            return Task.FromResult<IApiResponse<AppVersionDTO?>>(ApiResponse.Ok<AppVersionDTO?>(default));
        }

        public Task<IApiResponse> Download(bool isAnonymous, string requestUri, string cacheFilePath, IProgress<float>? progress, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IApiResponse>(ApiResponse.Ok());
        }

        public Task<HttpResponseMessage> Forward(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> Post(ActiveUserRecordDTO record)
        {
            return Task.FromResult<IApiResponse>(ApiResponse.Ok());
        }

        public ValueTask<IApiResponse> SendSms(SendSmsRequest request)
        {
            var rsp = ModelValidator(request) ?? ApiResponse.Ok();
            GlobalResponseIntercept(rsp);
            return new ValueTask<IApiResponse>(rsp);
        }

        public async Task<IApiResponse<ScriptResponse>> Basics(string? msg = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new ScriptResponse
            {
                Version = "00.1"
            });
        }

        public async Task<IApiResponse<PagedModel<ScriptDTO>>> ScriptTable(string? name = null, int pageIndex = 1, int pageSize = 15, string? msg = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new PagedModel<ScriptDTO> { });
        }

        public async Task<IApiResponse<IList<ScriptResponse>>> ScriptUpdateInfo(IEnumerable<Guid> ids, string? msg = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new List<ScriptResponse> { });
        }

        Task<string> ICloudServiceClient.Info()
        {
            return Task.FromResult(string.Empty);
        }

        public async Task<IApiResponse<NoticeTypeDTO[]>> Types()
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new NoticeTypeDTO[] { });
        }

        public async Task<IApiResponse<NoticeDTO[]>> NewMsg(Guid? typeId, DateTimeOffset? time)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new NoticeDTO[] { });
        }

        public async Task<IApiResponse<PagedModel<NoticeDTO>>> Table(Guid? typeId, int index, int? size = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new PagedModel<NoticeDTO> { });
        }

        public Task<IApiResponse<ClockInResponse>> AccountClockIn()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
