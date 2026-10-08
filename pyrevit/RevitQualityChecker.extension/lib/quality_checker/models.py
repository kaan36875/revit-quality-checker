"""Data models for the quality checker."""


class ElementInfo(object):
    """Revit-independent element representation."""

    def __init__(self, element_id, name, category, family_name="", type_name="", parameters=None):
        self.element_id = str(element_id)
        self.name = name or ""
        self.category = category or ""
        self.family_name = family_name or ""
        self.type_name = type_name or ""
        self.parameters = parameters or {}


class RuleViolation(object):
    """A single rule violation on an element."""

    def __init__(self, element_id, element_name, category, message, suggested_fix=""):
        self.element_id = element_id
        self.element_name = element_name
        self.category = category
        self.message = message
        self.suggested_fix = suggested_fix


class RuleResult(object):
    """Result of evaluating a single rule."""

    def __init__(self, rule_id, rule_name, severity, category=""):
        self.rule_id = rule_id
        self.rule_name = rule_name
        self.severity = severity
        self.rule_category = category
        self.violations = []
        self.checked_count = 0

    @property
    def passed(self):
        return len(self.violations) == 0


class CheckReport(object):
    """Complete quality check report."""

    def __init__(self, model_name):
        self.model_name = model_name
        self.timestamp = None
        self.results = []

    @property
    def total_checked(self):
        return sum(r.checked_count for r in self.results)

    @property
    def total_violations(self):
        return sum(len(r.violations) for r in self.results)

    @property
    def error_count(self):
        return sum(len(r.violations) for r in self.results if r.severity == "Error")

    @property
    def warning_count(self):
        return sum(len(r.violations) for r in self.results if r.severity == "Warning")

    @property
    def info_count(self):
        return sum(len(r.violations) for r in self.results if r.severity == "Info")

    @property
    def score(self):
        if self.total_checked == 0:
            return 100.0
        passed = self.total_checked - self.total_violations
        ratio = max(0, passed) / float(self.total_checked)
        return round(ratio * 100, 1)
