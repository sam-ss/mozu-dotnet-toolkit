using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozu.Api.ToolKit.Models
{
    public class TenantAdminSettings
    {
        public string Name { get; set; }
        public bool EntityManagerVisible { get; set; }
        public bool SiteBuilderContentListsVisible { get; set; }
        public bool CustomRoutesVisible { get; set; }
        public bool EnableBetaAdmin { get; set; }
    }
}
