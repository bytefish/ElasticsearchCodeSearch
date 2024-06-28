// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using ElasticsearchCodeSearch.Web.Client.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Infrastructure
{
    public class ApplicationErrorMessageService
    {
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly ApplicationErrorTranslator _applicationErrorTranslator;
        private readonly IMessageService _messageService;
        private readonly NavigationManager _navigationManager;


        public ApplicationErrorMessageService(IStringLocalizer<SharedResource> sharedLocalizer, IMessageService messageService, NavigationManager navigationManager, ApplicationErrorTranslator applicationErrorTranslator)
        {
            _sharedLocalizer = sharedLocalizer;
            _navigationManager = navigationManager;
            _applicationErrorTranslator = applicationErrorTranslator;
            _messageService = messageService;
        }

        public void ShowErrorMessage(Exception exception, Action<MessageOptions>? configure = null)
        {
            (var errorCode, var errorMessage) = _applicationErrorTranslator.GetErrorMessage(exception);

            _messageService.ShowMessageBar(options =>
            {
                options.Section = App.MESSAGES_TOP;
                options.Intent = MessageIntent.Error;
                options.ClearAfterNavigation = false;
                options.Title = _sharedLocalizer["Message_Error_Title"];
                options.Body = errorMessage;
                options.Timestamp = DateTime.Now;
                options.Link = new ActionLink<Message>
                {
                    Text = _sharedLocalizer["Message_ShowHelp"],
                    OnClick = (message) =>
                    {
                        _navigationManager.NavigateTo($"https://www.bytefish.de");

                        return Task.CompletedTask;
                    }
                };

                // If we need to customize it like using a different section or intent, we should
                // use the action passed to us ...
                if (configure != null)
                {
                    configure(options);
                }
            });
        }
    }
}
