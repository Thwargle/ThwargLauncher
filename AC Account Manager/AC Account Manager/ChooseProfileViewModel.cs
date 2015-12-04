using System;
using System.Collections.Generic;
using System.Data;

namespace ThwargLauncher
{
    class ChooseProfileViewModel
    {
        public DataTable ProfilesTable { get; set; }
        public DataView ProfilesView { get; set; }
        public ChooseProfileViewModel(List<Profile> profiles)
        {
            ProfilesTable = new DataTable();
            ProfilesTable.Columns.Add("Name", Type.GetType("System.String"));
            ProfilesTable.Columns.Add("ActiveAccounts", Type.GetType("System.Int32"));
            ProfilesTable.Columns.Add("ActiveServers", Type.GetType("System.Int32"));
            ProfilesTable.Columns.Add("LastLaunch", Type.GetType("System.DateTime"));
            ProfilesTable.Columns.Add("Description", Type.GetType("System.String"));
            foreach (var profile in profiles)
            {
                DataRow row = ProfilesTable.NewRow();
                row["Name"] = profile.Name;
                row["ActiveAccounts"] = profile.ActiveAccountCount;
                row["ActiveServers"] = profile.ActiveServerCount;
                row["LastLaunch"] = PopulateDate(profile.LastLaunchedDate, DateTime.MinValue);
                row["Description"] = profile.Description;
                ProfilesTable.Rows.Add(row);
            }
            ProfilesView = new DataView(ProfilesTable);
        }
        private DateTime PopulateDate(DateTime? date, DateTime defval)
        {
            return (date.HasValue ? date.Value : defval);
        }
    }
}
