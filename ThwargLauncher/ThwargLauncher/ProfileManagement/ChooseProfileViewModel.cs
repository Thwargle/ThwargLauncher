using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThwargLauncher
{
    class ChooseProfileViewModel
    {
        private MainWindowViewModel _mainWindowViewModel = null;
        private readonly ObservableCollection<ProfileChoiceViewModel> _profileModels = new ObservableCollection<ProfileChoiceViewModel>();
        public ObservableCollection<ProfileChoiceViewModel> AllProfiles { get { return _profileModels; } }
        public ChooseProfileViewModel(MainWindowViewModel mwvm, List<Profile> profiles)
        {
            _mainWindowViewModel = mwvm;
            foreach (var profile in profiles)
            {
                var profileModel = new ProfileChoiceViewModel(profile);
                _profileModels.Add(profileModel);
            }
            _profileModels.CollectionChanged += ProfileModelsCollectionChanged;
        }

        private void ProfileModelsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var deletedProfiles = new List<ProfileChoiceViewModel>();
            var idsToDelete = new Dictionary<string, int>();
            foreach (var item in e.OldItems)
            {
                var profile = item as ProfileChoiceViewModel;
                idsToDelete[profile.Name] = 1;
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var profile = item as ProfileChoiceViewModel;
                    if (idsToDelete.ContainsKey(profile.Name))
                    {
                        idsToDelete.Remove(profile.Name);
                    }
                }
            }
            foreach (var id in idsToDelete.Keys)
            {
                _mainWindowViewModel.DeleteProfileByName(id);
            }
        }
    }
}
