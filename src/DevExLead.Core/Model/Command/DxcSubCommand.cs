using System.Reflection.Metadata;

namespace DevExLead.Core.Model.Command
{
    public class DxcSubCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Handler { get; set; }
        public DxcParameter[] Parameters { get; set; }
    }
}