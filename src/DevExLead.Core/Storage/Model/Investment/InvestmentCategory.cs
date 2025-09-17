using System.ComponentModel.DataAnnotations.Schema;

namespace DevExLead.Core.Storage.Model.Investment
{
    public class InvestmentCategory
    {
        public string Name { get; set; }
        public List<string> Epics { get; set; }

        [NotMapped]
        public double Points { get; set; }

        [NotMapped]
        public double Percentage { get; set; }
    }
}