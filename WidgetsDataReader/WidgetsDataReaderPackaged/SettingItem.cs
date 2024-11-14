using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WidgetsDataReaderPackaged
{
    public class SettingItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SettingItem()
        {
            this.Name = string.Empty;
            this.Value = string.Empty;
        }

        public SettingItem(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
