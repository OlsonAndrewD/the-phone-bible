using System.Collections.Generic;
using System.Linq;
using Garnet.Api.ActionResults;
using Twilio.TwiML;
using Garnet.Api.Controllers;
using Garnet.Domain.Entities;
using Garnet.Domain.Extensions;
using System;
using Garnet.Api.Extensions;
using System.Threading.Tasks;

namespace Garnet.Api.TwilioRequestHandlers
{
    public class GroupBrowser : Browser
    {
        private ICollection<string> _options;
        private readonly BookGroup _bookGroup;

        public GroupBrowser(BookGroup bookGroup, IEnumerable<string> optionsInGroup)
            : base(bookGroup.GetTopmostAncestor().Name)
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

        protected override void HandleBrowseInternal(TwilioResponse response)
        {
            var optionNumber = 1;
            foreach (var option in _options)
            {
                response.AliceSay("For " + option + ", dial " + optionNumber++ + ".");
            }
        }

        protected override Task<TwilioResponseResult> HandleSelectionInternal(string phoneNumber, string selection)
        {
            var newGroupName = Name;

            int selectedOptionNumber;
            if (int.TryParse(selection, out selectedOptionNumber))
            {
                var selectedOptionName = _options.Skip(selectedOptionNumber - 1).FirstOrDefault();
                newGroupName = selectedOptionName ?? newGroupName;
            }

            return Task.FromResult<TwilioResponseResult>(
                new TwilioRedirectResult(TwilioVoiceController.GetBrowseUrl(newGroupName)));
        }
    }
}
