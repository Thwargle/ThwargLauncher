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
            public int WaitTimeMs { get; set; }
        }

        // Properties
        public ObservableCollection<EditableCharacterViewModel> CharacterList { get { return _characters; } }
        public string DetailText { get; private set; }
        public EditableCharacterViewModel SelectedCharacter { get; set; }
        private ObservableCollection<EditableCharacterViewModel> _characters = new ObservableCollection<EditableCharacterViewModel>();

        private string CmdQueueToString(Queue<string> cmds) { return string.Join("\r\n", cmds); }
        internal EditCharactersViewModel(AccountManager accountManager)
        {
            var globalCmds = MagFilter.LoginCommandsStorage.GetGlobalLoginCommands();
            _characters.Add(
                new EditableCharacterViewModel()
                {
                    AccountName = "",
                    ServerName = "",
                    CharacterName = "(Global)",
                    CharacterLoginCommandsCount = globalCmds.Commands.Count,
                    CharacterLoginCommandListString = CmdQueueToString(globalCmds.Commands),
                    WaitTimeMs = globalCmds.WaitMillisencds
                });

            foreach (var account in accountManager.UserAccounts)
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
                                AccountName = account.Name,
                                ServerName = server.ServerName,
                                CharacterName = character.Name,
                                CharacterLoginCommandsCount = cmds.Commands.Count,
                                CharacterLoginCommandListString = CmdQueueToString(cmds.Commands),
                                WaitTimeMs = cmds.WaitMillisencds
                                }
                            );
                    }

                }
            }
        }
    }
}
