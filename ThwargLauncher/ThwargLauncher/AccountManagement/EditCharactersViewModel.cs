using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonControls;

namespace ThwargLauncher.AccountManagement
{
    public class EditCharactersViewModel
    {

        // Local types
        public class EditableCharacterViewModel
        {
            public bool IsGlobal { get; set; }
            public string AccountName { get; set; }
            public string ServerName { get; set; }
            public string CharacterName { get; set; }
            public int CharacterLoginCommandsCount { get; set; }
            public string CharacterLoginCommandListString { get; set; }
            public int WaitTimeMs { get; set; }
            public ICommand SaveCurrentLoginCmdsCommand { get; set; }
        }

        // Properties
        public ObservableCollection<EditableCharacterViewModel> CharacterList { get { return _characters; } }
        public string DetailText { get; private set; }
        public EditableCharacterViewModel SelectedCharacter { get; set; }
        private ObservableCollection<EditableCharacterViewModel> _characters = new ObservableCollection<EditableCharacterViewModel>();
        private AccountManager _accountManager;

        private string CmdQueueToString(Queue<string> cmds) { return string.Join("\r\n", cmds); }
        internal EditCharactersViewModel(AccountManager accountManager)
        {
            _accountManager = accountManager;
            LoadCharacterConfiguration();
        }

        private void LoadCharacterConfiguration()
        {
            var globalCmds = MagFilter.LoginCommandsStorage.GetGlobalLoginCommands();
            _characters.Clear();
            _characters.Add(
                new EditableCharacterViewModel()
                {
                    IsGlobal = true,
                    AccountName = "",
                    ServerName = "",
                    CharacterName = "(Global)",
                    CharacterLoginCommandsCount = globalCmds.Commands.Count,
                    CharacterLoginCommandListString = CmdQueueToString(globalCmds.Commands),
                    WaitTimeMs = globalCmds.WaitMillisencds,
                    SaveCurrentLoginCmdsCommand = new DelegateCommand(PerformSaveCurrentLoginCmds)
                });

            foreach (var account in _accountManager.UserAccounts)
            {
                foreach (var server in account.Servers)
                {
                    foreach (var character in server.AvailableCharacters)
                    {
                        if (character.Id == 0) { continue; } // None
                        var cmds = MagFilter.LoginCommandsStorage.GetLoginCommands(account.Name, server.ServerName, character.Name);
                        _characters.Add(
                            new EditableCharacterViewModel()
                            {
                                IsGlobal = false,
                                AccountName = account.Name,
                                ServerName = server.ServerName,
                                CharacterName = character.Name,
                                CharacterLoginCommandsCount = cmds.Commands.Count,
                                CharacterLoginCommandListString = CmdQueueToString(cmds.Commands),
                                WaitTimeMs = cmds.WaitMillisencds,
                                SaveCurrentLoginCmdsCommand = new DelegateCommand(PerformSaveCurrentLoginCmds)
                            }
                            );
                    }
                }
            }
        }
        private void PerformSaveCurrentLoginCmds()
        {
            // SelectedCharacter doesn't work

            foreach (var charact in _characters)
            {
                SaveLoginCommands(charact);
            }
            LoadCharacterConfiguration();
            // doesn't work
            if (SelectedCharacter == null) { return; }
            SaveLoginCommands(SelectedCharacter);
        }
        private void SaveLoginCommands(EditableCharacterViewModel ecvm)
        {
            string text = ecvm.CharacterLoginCommandListString;
            if (ecvm.IsGlobal)
            {
                MagFilter.LoginCommandsStorage.SetGlobalLoginCommands(text, ecvm.WaitTimeMs);
            }
            else
            {
                MagFilter.LoginCommandsStorage.SetLoginCommands(ecvm.AccountName, ecvm.ServerName, ecvm.CharacterName, text, ecvm.WaitTimeMs);
            }
        }
    }
}
