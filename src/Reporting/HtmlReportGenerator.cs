using System.Linq;
using System.Text;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Reporting
{
    public class HtmlReportGenerator
    {
        public string Generate(CheckReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            sb.AppendLine("<title>Model Quality Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(Css);
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine($"<h1>Model Quality Report</h1>");
            sb.AppendLine($"<p class=\"meta\">Model: <strong>{Escape(report.ModelName)}</strong> | Date: {report.Timestamp:yyyy-MM-dd HH:mm} UTC</p>");

            sb.AppendLine("<div class=\"summary\">");
            sb.AppendLine($"<div class=\"score {ScoreClass(report.Score)}\"><span class=\"score-value\">{report.Score}</span><span class=\"score-label\">/ 100</span></div>");
            sb.AppendLine("<div class=\"stats\">");
            sb.AppendLine($"<div class=\"stat\"><span class=\"stat-value\">{report.TotalChecked}</span><span class=\"stat-label\">Checked</span></div>");
            sb.AppendLine($"<div class=\"stat error\"><span class=\"stat-value\">{report.ErrorCount}</span><span class=\"stat-label\">Errors</span></div>");
            sb.AppendLine($"<div class=\"stat warning\"><span class=\"stat-value\">{report.WarningCount}</span><span class=\"stat-label\">Warnings</span></div>");
            sb.AppendLine($"<div class=\"stat info\"><span class=\"stat-value\">{report.InfoCount}</span><span class=\"stat-label\">Info</span></div>");
            sb.AppendLine("</div></div>");

            foreach (var result in report.Results)
            {
                var statusIcon = result.Passed ? "&#10004;" : "&#10008;";
                var statusClass = result.Passed ? "passed" : "failed";
                var badgeText = result.Passed ? "Passed" : result.Severity.ToString();
                var badgeClass = result.Passed ? "passed-badge" : result.Severity.ToString().ToLower();
                sb.AppendLine($"<div class=\"rule {statusClass}\">");
                sb.AppendLine($"<h2><span class=\"icon\">{statusIcon}</span> {Escape(result.RuleName)} <span class=\"badge {badgeClass}\">{badgeText}</span></h2>");
                sb.AppendLine($"<p>Checked: {result.CheckedCount} | Violations: {result.Violations.Count}</p>");

                if (result.Violations.Any())
                {
                    sb.AppendLine("<table><thead><tr><th>Element ID</th><th>Name</th><th>Category</th><th>Issue</th><th>Suggested Fix</th></tr></thead><tbody>");
                    foreach (var v in result.Violations)
                    {
                        sb.AppendLine($"<tr><td>{Escape(v.ElementId)}</td><td>{Escape(v.ElementName)}</td><td>{Escape(v.Category)}</td><td>{Escape(v.Message)}</td><td>{Escape(v.SuggestedFix)}</td></tr>");
                    }
                    sb.AppendLine("</tbody></table>");
                }

                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static string ScoreClass(double score)
        {
            if (score >= 80) return "good";
            if (score >= 50) return "moderate";
            return "poor";
        }

        private const string Css = @"
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; color: #333; line-height: 1.6; }
.container { max-width: 960px; margin: 2rem auto; padding: 0 1rem; }
h1 { margin-bottom: 0.5rem; }
.meta { color: #666; margin-bottom: 1.5rem; }
.summary { display: flex; gap: 2rem; align-items: center; background: #fff; padding: 1.5rem; border-radius: 8px; margin-bottom: 2rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.score { text-align: center; min-width: 120px; }
.score-value { font-size: 3rem; font-weight: 700; }
.score-label { display: block; font-size: 0.9rem; color: #999; }
.score.good .score-value { color: #2e7d32; }
.score.moderate .score-value { color: #f57f17; }
.score.poor .score-value { color: #c62828; }
.stats { display: flex; gap: 1.5rem; }
.stat { text-align: center; }
.stat-value { display: block; font-size: 1.5rem; font-weight: 600; }
.stat-label { font-size: 0.8rem; color: #999; text-transform: uppercase; }
.stat.error .stat-value { color: #c62828; }
.stat.warning .stat-value { color: #f57f17; }
.stat.info .stat-value { color: #1565c0; }
.rule { background: #fff; border-radius: 8px; padding: 1.5rem; margin-bottom: 1rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.rule.passed { border-left: 4px solid #2e7d32; }
.rule.failed { border-left: 4px solid #c62828; }
.rule h2 { font-size: 1.1rem; margin-bottom: 0.5rem; display: flex; align-items: center; gap: 0.5rem; }
.icon { font-size: 1.2rem; }
.passed .icon { color: #2e7d32; }
.failed .icon { color: #c62828; }
.badge { font-size: 0.7rem; padding: 2px 8px; border-radius: 4px; font-weight: 600; text-transform: uppercase; }
.badge.error { background: #ffebee; color: #c62828; }
.badge.warning { background: #fff8e1; color: #f57f17; }
.badge.info { background: #e3f2fd; color: #1565c0; }
.badge.passed-badge { background: #e8f5e9; color: #2e7d32; }
table { width: 100%; border-collapse: collapse; margin-top: 1rem; font-size: 0.85rem; }
th { text-align: left; padding: 0.5rem; background: #f5f5f5; border-bottom: 2px solid #ddd; }
td { padding: 0.5rem; border-bottom: 1px solid #eee; }
tr:hover td { background: #fafafa; }
";
    }
}
