"""HTML report generator for quality check results."""

import os
import codecs
import webbrowser
from datetime import datetime


def generate_html_report(report, output_path=None):
    """Generate an HTML report from a CheckReport and optionally save to file.

    Args:
        report: CheckReport instance.
        output_path: File path to save the HTML. If None, saves to user's Desktop.

    Returns:
        The output file path.
    """
    if output_path is None:
        desktop = os.path.join(os.path.expanduser("~"), "Desktop")
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = "QualityReport_{}_{}.html".format(
            _safe_filename(report.model_name), timestamp)
        output_path = os.path.join(desktop, filename)

    html = _build_html(report)

    with codecs.open(output_path, "w", "utf-8") as f:
        f.write(html)

    return output_path


def open_report(file_path):
    """Open the report in the default browser."""
    webbrowser.open("file:///" + file_path.replace("\\", "/"))


def _safe_filename(name):
    """Remove characters that are not safe for filenames."""
    return "".join(c if c.isalnum() or c in "-_" else "_" for c in name)


def _score_class(score):
    if score >= 80:
        return "good"
    if score >= 50:
        return "moderate"
    return "poor"


def _escape(text):
    if not text:
        return ""
    return (text
            .replace("&", "&amp;")
            .replace("<", "&lt;")
            .replace(">", "&gt;")
            .replace('"', "&quot;"))


def _build_html(report):
    """Build the complete HTML report."""
    results_html = []
    for r in report.results:
        status_icon = "&#10004;" if r.passed else "&#10008;"
        status_class = "passed" if r.passed else "failed"
        severity_class = r.severity.lower()

        violations_html = ""
        if r.violations:
            rows = []
            for v in r.violations:
                rows.append(
                    "<tr>"
                    "<td>{eid}</td>"
                    "<td>{ename}</td>"
                    "<td>{cat}</td>"
                    "<td>{msg}</td>"
                    "<td>{fix}</td>"
                    "</tr>".format(
                        eid=_escape(str(v.element_id)),
                        ename=_escape(v.element_name),
                        cat=_escape(v.category),
                        msg=_escape(v.message),
                        fix=_escape(v.suggested_fix),
                    )
                )
            violations_html = (
                '<table><thead><tr>'
                '<th>Element ID</th><th>Name</th><th>Category</th>'
                '<th>Issue</th><th>Suggested Fix</th>'
                '</tr></thead><tbody>'
                + "\n".join(rows)
                + '</tbody></table>'
            )

        results_html.append(
            '<div class="rule {sc}">'
            '<h2>'
            '<span class="icon">{icon}</span> '
            '{name} '
            '<span class="badge {sev}">{severity}</span>'
            '</h2>'
            '<p>Checked: {checked} | Violations: {vcount}</p>'
            '{violations}'
            '</div>'.format(
                sc=status_class,
                icon=status_icon,
                name=_escape(r.rule_name),
                sev=severity_class,
                severity=r.severity,
                checked=r.checked_count,
                vcount=len(r.violations),
                violations=violations_html,
            )
        )

    timestamp_str = ""
    if report.timestamp:
        timestamp_str = report.timestamp.strftime("%Y-%m-%d %H:%M")

    return HTML_TEMPLATE.format(
        model_name=_escape(report.model_name),
        timestamp=timestamp_str,
        score=report.score,
        score_class=_score_class(report.score),
        total_checked=report.total_checked,
        error_count=report.error_count,
        warning_count=report.warning_count,
        info_count=report.info_count,
        results="\n".join(results_html),
    )


HTML_TEMPLATE = """<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Quality Report - {model_name}</title>
<style>
* {{ margin: 0; padding: 0; box-sizing: border-box; }}
body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: #f5f5f5; color: #333; line-height: 1.6; }}
.container {{ max-width: 960px; margin: 2rem auto; padding: 0 1rem; }}
h1 {{ margin-bottom: 0.5rem; }}
.meta {{ color: #666; margin-bottom: 1.5rem; }}
.summary {{ display: flex; gap: 2rem; align-items: center; background: #fff;
  padding: 1.5rem; border-radius: 8px; margin-bottom: 2rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1); flex-wrap: wrap; }}
.score {{ text-align: center; min-width: 120px; }}
.score-value {{ font-size: 3rem; font-weight: 700; }}
.score-label {{ display: block; font-size: 0.9rem; color: #999; }}
.score.good .score-value {{ color: #2e7d32; }}
.score.moderate .score-value {{ color: #f57f17; }}
.score.poor .score-value {{ color: #c62828; }}
.stats {{ display: flex; gap: 1.5rem; flex-wrap: wrap; }}
.stat {{ text-align: center; }}
.stat-value {{ display: block; font-size: 1.5rem; font-weight: 600; }}
.stat-label {{ font-size: 0.8rem; color: #999; text-transform: uppercase; }}
.stat.error .stat-value {{ color: #c62828; }}
.stat.warning .stat-value {{ color: #f57f17; }}
.stat.info .stat-value {{ color: #1565c0; }}
.rule {{ background: #fff; border-radius: 8px; padding: 1.5rem; margin-bottom: 1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1); }}
.rule.passed {{ border-left: 4px solid #2e7d32; }}
.rule.failed {{ border-left: 4px solid #c62828; }}
.rule h2 {{ font-size: 1.1rem; margin-bottom: 0.5rem; display: flex;
  align-items: center; gap: 0.5rem; flex-wrap: wrap; }}
.icon {{ font-size: 1.2rem; }}
.passed .icon {{ color: #2e7d32; }}
.failed .icon {{ color: #c62828; }}
.badge {{ font-size: 0.7rem; padding: 2px 8px; border-radius: 4px;
  font-weight: 600; text-transform: uppercase; }}
.badge.error {{ background: #ffebee; color: #c62828; }}
.badge.warning {{ background: #fff8e1; color: #f57f17; }}
.badge.info {{ background: #e3f2fd; color: #1565c0; }}
table {{ width: 100%; border-collapse: collapse; margin-top: 1rem; font-size: 0.85rem; }}
th {{ text-align: left; padding: 0.5rem; background: #f5f5f5; border-bottom: 2px solid #ddd; }}
td {{ padding: 0.5rem; border-bottom: 1px solid #eee; }}
tr:hover td {{ background: #fafafa; }}
.footer {{ text-align: center; margin-top: 2rem; color: #999; font-size: 0.8rem; }}
</style>
</head>
<body>
<div class="container">
<h1>Model Quality Report</h1>
<p class="meta">Model: <strong>{model_name}</strong> | Date: {timestamp}</p>

<div class="summary">
<div class="score {score_class}">
  <span class="score-value">{score}</span>
  <span class="score-label">/ 100</span>
</div>
<div class="stats">
  <div class="stat"><span class="stat-value">{total_checked}</span><span class="stat-label">Checked</span></div>
  <div class="stat error"><span class="stat-value">{error_count}</span><span class="stat-label">Errors</span></div>
  <div class="stat warning"><span class="stat-value">{warning_count}</span><span class="stat-label">Warnings</span></div>
  <div class="stat info"><span class="stat-value">{info_count}</span><span class="stat-label">Info</span></div>
</div>
</div>

{results}

<p class="footer">Generated by RevitQualityChecker</p>
</div>
</body>
</html>
"""
