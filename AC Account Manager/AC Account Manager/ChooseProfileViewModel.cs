using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC_Account_Manager
{
    class ChooseProfileViewModel
    {
        public DataTable ProfilesTable { get; set; }
        public DataView ProfilesView { get; set; }
        public ChooseProfileViewModel(List<Profile> profiles)
        {
            ProfilesTable = new DataTable();
            ProfilesTable.Columns.Add("Name", Type.GetType("System.String"));
            ProfilesTable.Columns.Add("LastLaunch", Type.GetType("System.DateTime"));
            ProfilesTable.Columns.Add("LastSaved", Type.GetType("System.DateTime"));
            ProfilesTable.Columns.Add("LastActivation", Type.GetType("System.DateTime"));
            ProfilesTable.Columns.Add("Description", Type.GetType("System.String"));
            foreach (var profile in profiles)
            {
                DataRow row = ProfilesTable.NewRow();
                row["Name"] = profile.Name;
                row["LastLaunch"] = profile.LastLaunchedDate;
                row["LastSaved"] = profile.LastSavedDate;
                row["LastActivation"] = profile.LastActivatedDate;
                row["Description"] = profile.Description;
                ProfilesTable.Rows.Add(row);
            }
            ProfilesView = new DataView(ProfilesTable);
        }
    }
}
