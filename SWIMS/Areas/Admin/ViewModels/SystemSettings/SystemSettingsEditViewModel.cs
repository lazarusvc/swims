using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SWIMS.Services.SystemSettings;

namespace SWIMS.Areas.Admin.ViewModels.SystemSettings
{
    public class SystemSettingsEditViewModel
    {
        [Required]
        public string Key { get; set; } = default!;

        [Required]
        public string DisplayName { get; set; } = default!;

        public string? Description { get; set; }

        public string? EnvironmentName { get; set; }

        public IList<Item> Items { get; set; } = new List<Item>();

        public class Item
        {
            [Required]
            public string KeyPath { get; set; } = default!;

            public string DisplayName { get; set; } = default!;

            public string? Description { get; set; }

            public string? Value { get; set; }

            public string DataType { get; set; } = "string";

            public bool IsSecret { get; set; }

            public bool IsRequired { get; set; }
        }

        public static SystemSettingsEditViewModel FromSection(SystemSettingsSection section)
        {
            var vm = new SystemSettingsEditViewModel
            {
                Key = section.Key,
                DisplayName = section.DisplayName,
                Description = section.Description,
                EnvironmentName = section.EnvironmentName
            };

            vm.Items = section.Items
                .Select(i => new Item
                {
                    KeyPath = i.KeyPath,
                    DisplayName = i.DisplayName,
                    Description = i.Description,
                    Value = i.Value,
                    DataType = i.DataType,
                    IsSecret = i.IsSecret,
                    IsRequired = i.IsRequired
                })
                .ToList();

            return vm;
        }

        public SystemSettingsSection ToSection()
        {
            var section = new SystemSettingsSection
            {
                Key = Key,
                DisplayName = DisplayName,
                Description = Description,
                EnvironmentName = EnvironmentName
            };

            foreach (var item in Items)
            {
                section.Items.Add(new SystemSettingsItem
                {
                    KeyPath = item.KeyPath,
                    DisplayName = item.DisplayName,
                    Description = item.Description,
                    Value = item.Value,
                    DataType = item.DataType,
                    IsSecret = item.IsSecret,
                    IsRequired = item.IsRequired
                });
            }

            return section;
        }
    }
}
