using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Reporting
{
    public class BcfExporter
    {
        public void Export(CheckReport report, string outputPath)
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            using var zip = ZipFile.Open(outputPath, ZipArchiveMode.Create);

            WriteBcfVersion(zip);
            WriteProject(zip, report);

            var violations = report.Results
                .Where(r => !r.Passed)
                .SelectMany(r => r.Violations.Select(v => new { Rule = r, Violation = v }))
                .ToList();

            foreach (var item in violations)
            {
                var guid = Guid.NewGuid().ToString();
                WriteMarkup(zip, guid, item.Rule, item.Violation, report.ModelName);
            }
        }

        private void WriteBcfVersion(ZipArchive zip)
        {
            var entry = zip.CreateEntry("bcf.version");
            using var stream = entry.Open();
            using var writer = XmlWriter.Create(stream, XmlSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Version", "http://www.buildingsmart-tech.org/bcf/version/2.1");
            writer.WriteAttributeString("VersionId", "2.1");
            writer.WriteEndElement();
        }

        private void WriteProject(ZipArchive zip, CheckReport report)
        {
            var entry = zip.CreateEntry("project.bcfp");
            using var stream = entry.Open();
            using var writer = XmlWriter.Create(stream, XmlSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("ProjectExtension", "http://www.buildingsmart-tech.org/bcf/markup/2.1");
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("ProjectId", Guid.NewGuid().ToString());
            writer.WriteElementString("Name", $"Quality Check — {report.ModelName}");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteMarkup(ZipArchive zip, string topicGuid, RuleResult rule, RuleViolation violation, string modelName)
        {
            var entry = zip.CreateEntry($"{topicGuid}/markup.bcf");
            using var stream = entry.Open();
            using var writer = XmlWriter.Create(stream, XmlSettings);

            var ns = "http://www.buildingsmart-tech.org/bcf/markup/2.1";

            writer.WriteStartDocument();
            writer.WriteStartElement("Markup", ns);

            writer.WriteStartElement("Header");
            writer.WriteStartElement("File");
            writer.WriteElementString("Filename", modelName);
            writer.WriteElementString("Date", DateTime.UtcNow.ToString("o"));
            writer.WriteEndElement(); // File
            writer.WriteEndElement(); // Header

            writer.WriteStartElement("Topic");
            writer.WriteAttributeString("Guid", topicGuid);
            writer.WriteAttributeString("TopicType", SeverityToType(rule.Severity));
            writer.WriteAttributeString("TopicStatus", "Open");

            writer.WriteElementString("Title", $"[{rule.RuleId}] {violation.ElementName} — {violation.Message}");
            writer.WriteElementString("Description", BuildDescription(rule, violation));
            writer.WriteElementString("Priority", SeverityToPriority(rule.Severity));
            writer.WriteElementString("CreationDate", DateTime.UtcNow.ToString("o"));
            writer.WriteElementString("CreationAuthor", "RevitQualityChecker");
            writer.WriteElementString("ModifiedDate", DateTime.UtcNow.ToString("o"));

            writer.WriteEndElement(); // Topic

            if (!string.IsNullOrEmpty(violation.SuggestedFix))
            {
                writer.WriteStartElement("Comment");
                writer.WriteAttributeString("Guid", Guid.NewGuid().ToString());
                writer.WriteElementString("Date", DateTime.UtcNow.ToString("o"));
                writer.WriteElementString("Author", "RevitQualityChecker");
                writer.WriteElementString("Comment", $"Suggested fix: {violation.SuggestedFix}");
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // Markup
        }

        private string BuildDescription(RuleResult rule, RuleViolation violation)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Rule: {rule.RuleName} ({rule.RuleId})");
            sb.AppendLine($"Element: {violation.ElementName} (ID: {violation.ElementId})");
            sb.AppendLine($"Category: {violation.Category}");
            sb.AppendLine($"Issue: {violation.Message}");
            return sb.ToString();
        }

        private static string SeverityToType(Severity severity) => severity switch
        {
            Severity.Error => "Error",
            Severity.Warning => "Warning",
            _ => "Information"
        };

        private static string SeverityToPriority(Severity severity) => severity switch
        {
            Severity.Error => "Critical",
            Severity.Warning => "Normal",
            _ => "Low"
        };

        private static readonly XmlWriterSettings XmlSettings = new()
        {
            Indent = true,
            Encoding = new UTF8Encoding(false)
        };
    }
}
