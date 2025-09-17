namespace DevExLead.Core.Storage.Model.Investment
{
    public class InvestmentProfile
    {
        public List<InvestmentCategory> InvestmentCategories { get; set; }
        public List<DeveloperAllocation> DeveloperAllocations { get; set; }
    }
}