using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThwargLauncher
{
    class ChooseProfileViewModel
    {
        private readonly ObservableCollection<ProfileChoiceViewModel> _profileModels = new ObservableCollection<ProfileChoiceViewModel>();
        public ObservableCollection<ProfileChoiceViewModel> AllProfiles { get { return _profileModels; } }
        public ChooseProfileViewModel(List<Profile> profiles)
        {
            foreach (var profile in profiles)
            {
                var profileModel = new ProfileChoiceViewModel(profile);
                _profileModels.Add(profileModel);
            }
        }
    }
}
