using System.Collections.Generic;

namespace RevitQualityChecker.Core.Models
{
    public class ElementInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
