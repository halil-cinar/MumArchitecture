using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    [Table("Setting")]
    public class Setting : Entity
    {
        public string? Name { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public string SettingType { get; set; } = "[]";
    }
    public class SettingType
    {
        public string Name { get; set; } = "";
        public ESetting Type { get; set; } = ESetting.TEXT;
        public List<SettingType>? ElementType { get; set; } = null;//Sadece dizilerde
    }
}
