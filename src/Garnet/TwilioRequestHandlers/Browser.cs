using Garnet.Api.ActionResults;
using Garnet.Api.Controllers;
using System;
using Twilio.TwiML;

namespace Garnet.Api.TwilioRequestHandlers
{
    public abstract class Browser : IBrowser
    {
        private readonly string _topGroupName;

        protected Browser(string topGroupName)
        {
            _topGroupName = topGroupName;
        }

        protected abstract string Name { get; }
        protected abstract string ParentName { get; }
        protected abstract int NumberOfOptions { get; }

        protected abstract void HandleBrowseInternal(TwilioResponse response);
        protected abstract TwilioResponseResult HandleSelectionInternal(string phoneNumber, string selection);

        public TwilioResponseResult HandleBrowse(string phoneNumber, bool navigatingUp)
        {
            if (NumberOfOptions == 1)
            {
                return navigatingUp ?
                    new TwilioRedirectResult(TwilioVoiceController.GetBrowseUrl(ParentName, true)) :
                    HandleSelection(phoneNumber, 1.ToString());
            }

            var numDigits = NumberOfOptions == 0 ?
                0 :
                Convert.ToInt32(Math.Floor(Math.Log10(NumberOfOptions))) + 1;

            return new TwilioResponseResult(response =>
            {
                response.BeginGather(new
                {
                    action = TwilioVoiceController.GetBrowseUrl(Name),
                    numDigits = numDigits,
                    timeout = 4
                });

                response.Say(Name);
                response.Pause();

                HandleBrowseInternal(response);

                if (ParentName != null)
                {
                    response.Say(string.Concat("Press 0 to exit ", Name, "."));
                    response.Say("Press star to go elsewhere.");
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

            if (selection == "*")
            {
                return new TwilioRedirectResult(TwilioVoiceController.GetBrowseUrl(_topGroupName));
            }

            if (selection == "0")
            {
                return new TwilioRedirectResult(TwilioVoiceController.GetBrowseUrl(ParentName, true));
            }

            return HandleSelectionInternal(phoneNumber, selection);
        }
    }
}
