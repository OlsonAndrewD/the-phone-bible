using Garnet.Api.ActionResults;
using Garnet.Api.Controllers;
using Garnet.Domain.Services;
using System;
using Twilio.TwiML;

namespace Garnet.Api.TwilioRequestHandlers
{
    public abstract class Browser
    {
        protected abstract string GroupName { get; }
        protected abstract string ParentGroupName { get; }
        protected abstract int NumberOfOptions { get; }

        protected abstract void PromptForSelectionInternal(TwilioResponse response);
        protected abstract TwilioResponseResult HandleSelectionInternal(string phoneNumber, string selection);

        public TwilioResponseResult PromptForSelection()
        {
            var numDigits = Convert.ToInt32(Math.Floor(Math.Log10(NumberOfOptions))) + 1;
            return new TwilioResponseResult(response =>
            {
                response.BeginGather(new
                {
                    action = TwilioVoiceController.GetBrowseUrl(GroupName),
                    numDigits = numDigits,
                    timeout = 4
                });

                PromptForSelectionInternal(response);

                if (ParentGroupName != null)
                {
                    response.Say("Press 0 to go back.");
                }

                response.Say("Press pound for main menu.");
                response.EndGather();
                response.Redirect();
            });
        }

        public TwilioResponseResult HandleSelection(string phoneNumber, string selection)
        {
            if (selection == "#")
            {
                return new TwilioRedirectResult(Routes.MainMenu);
            }

            if (selection == "0")
            {
                return new TwilioRedirectResult(TwilioVoiceController.GetBrowseUrl(ParentGroupName));
            }

            return HandleSelectionInternal(phoneNumber, selection);
        }
    }
}
