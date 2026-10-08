"""Run quality checks on the active Revit model and generate an HTML report."""
# -*- coding: utf-8 -*-
__title__ = "Check\nModel"
__doc__ = "Run quality checks on the current model and generate an HTML report."
__author__ = "Kaan Beyazkilic"

import os
import sys

from pyrevit import revit, DB, forms, script

from quality_checker.compat import make_element_id, REVIT_VERSION
from quality_checker.engine import load_rules_from_file, run_check, get_default_rules_path
from quality_checker.collector import collect_elements
from quality_checker.report import generate_html_report, open_report

logger = script.get_logger()
output = script.get_output()


def main():
    doc = revit.doc
    if not doc:
        forms.alert("No document is open.", exitscript=True)
        return

    model_name = os.path.basename(doc.PathName) if doc.PathName else doc.Title

    # Load rules
    rules_path = get_default_rules_path()

    # Check for a project-specific rules file next to the model
    if doc.PathName:
        project_rules = os.path.join(
            os.path.dirname(doc.PathName),
            "quality-rules.json"
        )
        if os.path.exists(project_rules):
            rules_path = project_rules

    if not os.path.exists(rules_path):
        forms.alert("Rule file not found:\n{}".format(rules_path), exitscript=True)
        return

    try:
        rules = load_rules_from_file(rules_path)
    except Exception as e:
        forms.alert("Error loading rules:\n{}".format(str(e)), exitscript=True)
        return

    output.print_md("## RevitQualityChecker")
    output.print_md("**Revit:** {}".format(REVIT_VERSION))
    output.print_md("**Model:** {}".format(model_name))
    output.print_md("**Rules:** {} loaded from `{}`".format(len(rules), os.path.basename(rules_path)))
    output.print_md("---")
    output.print_md("Collecting elements...")

    # Collect elements from the model
    try:
        elements = collect_elements(doc)
    except Exception as e:
        forms.alert("Error collecting elements:\n{}".format(str(e)), exitscript=True)
        return

    output.print_md("Found **{}** elements. Running checks...".format(len(elements)))

    # Run checks
    report = run_check(model_name, elements, rules)

    # Print summary to pyRevit output
    output.print_md("---")
    output.print_md("### Results")
    output.print_md("**Score: {}/100**".format(report.score))
    output.print_md("")
    output.print_md("| | Count |")
    output.print_md("|---|---|")
    output.print_md("| Elements checked | {} |".format(report.total_checked))
    output.print_md("| Errors | {} |".format(report.error_count))
    output.print_md("| Warnings | {} |".format(report.warning_count))
    output.print_md("| Info | {} |".format(report.info_count))
    output.print_md("")

    for result in report.results:
        icon = u"✅" if result.passed else u"❌"
        output.print_md("{} **{}** — {} ({} checked, {} violations)".format(
            icon, result.rule_name, result.severity,
            result.checked_count, len(result.violations)))

        if result.violations:
            for v in result.violations[:10]:
                link = output.linkify(make_element_id(v.element_id))
                output.print_md("  - {} `{}`: {}".format(link, v.element_name, v.message))

            remaining = len(result.violations) - 10
            if remaining > 0:
                output.print_md("  - ... and {} more".format(remaining))

    # Generate HTML report
    output.print_md("---")
    try:
        report_path = generate_html_report(report)
        output.print_md(u"✅ **HTML report saved to:** `{}`".format(report_path))
        open_report(report_path)
    except Exception as e:
        output.print_md(u"❌ Error generating report: {}".format(str(e)))


if __name__ == "__main__":
    main()
