using System;
using System.Collections.Generic;
using System.Linq;
using Garnet.Api.ActionResults;
using Twilio.TwiML;
using Garnet.Api.Controllers;

namespace Garnet.Api.TwilioRequestHandlers
{
    public abstract class GroupBrowser : Browser
    {
        private ICollection<string> _options;

        public GroupBrowser(IEnumerable<string> options)
        {
            _options = options.ToList();
        }

        protected override int NumberOfOptions
        {
            get { return _options.Count; }
        }

        protected override void PromptForSelectionInternal(TwilioResponse response)
        {
            var optionNumber = 1;
            foreach (var option in _options)
            {
                response.Say("Enter " + optionNumber++ + " for " + option + ".");
            }
        }

        protected override TwilioResponseResult HandleSelectionInternal(string phoneNumber, string selection)
        {
            var newGroupName = GroupName;

            int selectedOptionNumber;
            if (int.TryParse(selection, out selectedOptionNumber))
            {
                var selectedOptionName = _options.Skip(selectedOptionNumber - 1).FirstOrDefault();
                newGroupName = selectedOptionName ?? newGroupName;
            }

            return new TwilioRedirectResult(TwilioVoiceController.GetBrowseUrl(newGroupName));
        }
    }
}
