using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThwargLauncher
{
    class ProfileChoiceViewModel
    {
        private Profile _profile;
        public ProfileChoiceViewModel(Profile profile)
        {
            _profile = profile;
        }
        public string Name { get { return _profile.Name; } }
        public string Description { get { return _profile.Description; } }
        public int ActiveAccounts { get { return _profile.ActiveAccountCount; } }
        public int ActiveServers { get { return _profile.ActiveServerCount; } }
        public DateTime LastLaunch { get { return PopulateDate(_profile.LastLaunchedDate, DateTime.MinValue); } }

        private DateTime PopulateDate(DateTime? date, DateTime defval)
        {
            return (date.HasValue ? date.Value : defval);
        }
    }
}
