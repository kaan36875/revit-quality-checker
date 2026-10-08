using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.RevitAdapter.ViewModels
{
    public class ViolationViewModel
    {
        public string ElementId { get; }
        public string ElementName { get; }
        public string Category { get; }
        public string Message { get; }
        public string SuggestedFix { get; }

        public ViolationViewModel(RuleViolation v)
        {
            ElementId = v.ElementId;
            ElementName = v.ElementName;
            Category = v.Category;
            Message = v.Message;
            SuggestedFix = v.SuggestedFix;
        }
    }
}
