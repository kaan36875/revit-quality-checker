"""Test the rule engine and report generator without Revit.

Run with: python test_without_revit.py

This creates a sample report on your Desktop using fake element data.
"""

import sys
import os

# Add the lib directory to path so we can import quality_checker
lib_path = os.path.join(os.path.dirname(__file__),
                        "RevitQualityChecker.extension", "lib")
sys.path.insert(0, lib_path)

from quality_checker.models import ElementInfo
from quality_checker.engine import load_rules_from_file, run_check, get_default_rules_path
from quality_checker.report import generate_html_report, open_report


def create_test_elements():
    """Create fake elements that simulate a Revit model with some issues."""
    elements = [
        # Good walls
        ElementInfo("100001", "Brick_200", "Walls", parameters={"Fire Rating": "60"}),
        ElementInfo("100002", "Concrete_300", "Walls", parameters={"Fire Rating": "120"}),
        # Bad wall naming
        ElementInfo("100003", "Interior Partition", "Walls", parameters={"Fire Rating": "30"}),
        ElementInfo("100004", "My Custom Wall", "Walls", parameters={}),
        # Wall missing fire rating
        ElementInfo("100005", "Steel_150", "Walls", parameters={}),

        # Good levels
        ElementInfo("200001", "00-Ground Floor", "Levels"),
        ElementInfo("200002", "01-First Floor", "Levels"),
        # Bad level naming
        ElementInfo("200003", "Roof", "Levels"),
        ElementInfo("200004", "Basement", "Levels"),

        # Good doors with mark
        ElementInfo("300001", "Single Door", "Doors", parameters={"Mark": "D01"}),
        ElementInfo("300002", "Double Door", "Doors", parameters={"Mark": "D02"}),
        # Door missing mark
        ElementInfo("300003", "Fire Door", "Doors", parameters={}),
        ElementInfo("300004", "Glass Door", "Doors", parameters={"Mark": ""}),

        # Good rooms
        ElementInfo("400001", "Office 101", "Rooms", parameters={"Number": "101", "Area": "25.5"}),
        ElementInfo("400002", "Kitchen", "Rooms", parameters={"Number": "102", "Area": "15.0"}),
        # Room missing number
        ElementInfo("400003", "Storage", "Rooms", parameters={"Area": "8.0"}),
        # Unplaced room (zero area)
        ElementInfo("400004", "Meeting Room", "Rooms", parameters={"Number": "104", "Area": "0"}),

        # Good views
        ElementInfo("500001", "A1-Ground Floor Plan", "Views"),
        ElementInfo("500002", "A2-First Floor Plan", "Views"),
        # Bad view names
        ElementInfo("500003", "Copy of Level 1", "Views"),
        ElementInfo("500004", "Section 1", "Views"),
        # Duplicate view names
        ElementInfo("500005", "3D View", "Views"),
        ElementInfo("500006", "3D View", "Views"),
    ]
    return elements


def main():
    print("=" * 60)
    print("RevitQualityChecker - Test Without Revit")
    print("=" * 60)

    # Load rules
    rules_path = get_default_rules_path()
    print("\nLoading rules from: {}".format(rules_path))
    rules = load_rules_from_file(rules_path)
    print("Loaded {} rules".format(len(rules)))

    # Create test data
    elements = create_test_elements()
    print("Created {} test elements".format(len(elements)))

    # Run checks
    print("\nRunning quality checks...")
    report = run_check("TestModel_WithIssues.rvt", elements, rules)

    # Print results
    print("\n" + "-" * 60)
    print("RESULTS")
    print("-" * 60)
    print("Score: {}/100".format(report.score))
    print("Total checked: {}".format(report.total_checked))
    print("Errors: {}".format(report.error_count))
    print("Warnings: {}".format(report.warning_count))
    print("Info: {}".format(report.info_count))
    print()

    for result in report.results:
        icon = "[PASS]" if result.passed else "[FAIL]"
        print("{} {} ({}) - checked: {}, violations: {}".format(
            icon, result.rule_name, result.severity,
            result.checked_count, len(result.violations)))

        for v in result.violations:
            print("       Element {}: {}".format(v.element_id, v.message))

    # Generate HTML report
    print("\n" + "-" * 60)
    report_path = generate_html_report(report)
    print("HTML report saved to: {}".format(report_path))
    print("Opening in browser...")
    open_report(report_path)
    print("\nDone!")


if __name__ == "__main__":
    main()
