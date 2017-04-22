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
            public string AccountName { get; set; }
            public string ServerName { get; set; }
            public string CharacterName { get; set; }
            public int CharacterLoginCommandsCount { get; set; }
            public string CharacterLoginCommandListString { get; set; }
        }

        // Properties
        public ObservableCollection<EditableCharacterViewModel> CharacterList { get { return _characters; } }
        public string DetailText { get; private set; }
        public EditableCharacterViewModel SelectedCharacter { get; set; }
        private ObservableCollection<EditableCharacterViewModel> _characters = new ObservableCollection<EditableCharacterViewModel>();

        private string CmdListToString(List<string> cmds) { return string.Join("\r\n", cmds); }
        internal EditCharactersViewModel(AccountManager accountManager)
        {
            var globalCmds = MagFilter.LoginCommandPersister.GetGlobalLoginCommands();
            _characters.Add(
                new EditableCharacterViewModel()
                {
                    AccountName = "",
                    ServerName = "",
                    CharacterName = "(Global)",
                    CharacterLoginCommandsCount = globalCmds.Count,
                    CharacterLoginCommandListString = CmdListToString(globalCmds)
                });

            foreach (var account in accountManager.UserAccounts)
            {
                foreach (var server in account.Servers)
                {
                    foreach (var character in server.AvailableCharacters)
                    {
                        if (character.Id == 0) { continue; } // None
                        var cmds = MagFilter.LoginCommandPersister.GetLoginCommands(account.Name, server.ServerName, character.Name);
                        _characters.Add(
                            new EditableCharacterViewModel()
                                {
                                AccountName = account.Name,
                                ServerName = server.ServerName,
                                CharacterName = character.Name,
                                CharacterLoginCommandsCount = cmds.Count,
                                CharacterLoginCommandListString = CmdListToString(cmds)
                                }
                            );
                    }

                }
            }
        }
    }
}
