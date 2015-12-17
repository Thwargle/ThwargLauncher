using System;
using System.ComponentModel;

namespace ThwargLauncher
{
    class ProfileChoiceViewModel : INotifyPropertyChanged
    {
        private readonly Profile _profile;
        public ProfileChoiceViewModel(Profile profile)
        {
            _profile = profile;
        }
        public string Name
        {
            get { return _profile.Name; }
            set
            {
                var profileManager = new ProfileManager();
                bool renamed = profileManager.RenameProfile(_profile.Name, value);
                if (renamed)
                {
                    OnPropertyChanged("CurrentProfileName");
                }
            }
        }
        public string Description { get { return _profile.Description; } }
        public int ActiveAccounts { get { return _profile.ActiveAccountCount; } }
        public int ActiveServers { get { return _profile.ActiveServerCount; } }
        public DateTime LastLaunch { get { return PopulateDate(_profile.LastLaunchedDate, DateTime.MinValue); } }

        private DateTime PopulateDate(DateTime? date, DateTime defval)
        {
            return (date.HasValue ? date.Value : defval);
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
