using System.Collections.Generic;
using System.Linq;
using Garnet.Api.ActionResults;
using Twilio.TwiML;
using Garnet.Api.Controllers;
using Garnet.Domain.Entities;

namespace Garnet.Api.TwilioRequestHandlers
{
    public class GroupBrowser : Browser
    {
        private ICollection<string> _options;
        private readonly BookGroup _bookGroup;

        public GroupBrowser(BookGroup bookGroup, IEnumerable<string> optionsInGroup)
        {
            _bookGroup = bookGroup;
            _options = optionsInGroup.ToList();
        }

        protected override string Name
        {
            get { return _bookGroup.Name; }
        }

        protected override string ParentName
        {
            get { return _bookGroup.Parent == null ? null : _bookGroup.Parent.Name; }
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
            var newGroupName = Name;

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
