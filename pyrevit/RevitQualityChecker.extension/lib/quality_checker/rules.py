"""Rule implementations for the quality checker."""

import re
from quality_checker.models import RuleResult, RuleViolation


class NamingRule(object):
    """Checks element names against a regex pattern."""

    def __init__(self, rule_id, name, description, severity, target_category, pattern):
        self.rule_id = rule_id
        self.name = name
        self.description = description
        self.severity = severity
        self.target_category = target_category
        self._pattern = pattern
        self._regex = re.compile(pattern)

    def evaluate(self, elements):
        result = RuleResult(self.rule_id, self.name, self.severity, "Naming")

        for elem in elements:
            if self.target_category and elem.category != self.target_category:
                continue

            result.checked_count += 1

            if not elem.name or not self._regex.match(elem.name):
                result.violations.append(RuleViolation(
                    element_id=elem.element_id,
                    element_name=elem.name or "(empty)",
                    category=elem.category,
                    message="Name '{}' does not match pattern '{}'".format(
                        elem.name, self._pattern),
                    suggested_fix="Rename to match: {}".format(self._pattern)
                ))

        return result


class RequiredParameterRule(object):
    """Checks that a specific parameter is filled in."""

    def __init__(self, rule_id, name, description, severity, target_category, parameter_name):
        self.rule_id = rule_id
        self.name = name
        self.description = description
        self.severity = severity
        self.target_category = target_category
        self._parameter_name = parameter_name

    def evaluate(self, elements):
        result = RuleResult(self.rule_id, self.name, self.severity, "Parameters")

        for elem in elements:
            if self.target_category and elem.category != self.target_category:
                continue

            result.checked_count += 1
            value = elem.parameters.get(self._parameter_name, "")

            if not value or not value.strip():
                result.violations.append(RuleViolation(
                    element_id=elem.element_id,
                    element_name=elem.name,
                    category=elem.category,
                    message="Required parameter '{}' is missing or empty".format(
                        self._parameter_name),
                    suggested_fix="Fill in the '{}' parameter".format(
                        self._parameter_name)
                ))

        return result


class DuplicateNameRule(object):
    """Detects elements with duplicate names within a category."""

    def __init__(self, rule_id, name, description, severity, target_category):
        self.rule_id = rule_id
        self.name = name
        self.description = description
        self.severity = severity
        self.target_category = target_category

    def evaluate(self, elements):
        result = RuleResult(self.rule_id, self.name, self.severity, "Model Health")

        name_counts = {}
        category_elements = []

        for elem in elements:
            if self.target_category and elem.category != self.target_category:
                continue
            result.checked_count += 1
            category_elements.append(elem)
            key = elem.name
            if key not in name_counts:
                name_counts[key] = []
            name_counts[key].append(elem)

        for elem_name, dupes in name_counts.items():
            if len(dupes) > 1:
                for elem in dupes:
                    result.violations.append(RuleViolation(
                        element_id=elem.element_id,
                        element_name=elem.name,
                        category=elem.category,
                        message="Duplicate name '{}' shared by {} elements".format(
                            elem_name, len(dupes)),
                        suggested_fix="Rename to make unique"
                    ))

        return result


class UnplacedRoomRule(object):
    """Checks for rooms that are not placed or have zero area."""

    def __init__(self, rule_id, name, description, severity):
        self.rule_id = rule_id
        self.name = name
        self.description = description
        self.severity = severity
        self.target_category = "Rooms"

    def evaluate(self, elements):
        result = RuleResult(self.rule_id, self.name, self.severity, "Model Health")

        for elem in elements:
            if elem.category != "Rooms":
                continue

            result.checked_count += 1
            area = elem.parameters.get("Area", "0")

            try:
                area_val = float(area)
            except (ValueError, TypeError):
                area_val = 0

            if area_val == 0:
                result.violations.append(RuleViolation(
                    element_id=elem.element_id,
                    element_name=elem.name,
                    category=elem.category,
                    message="Room has zero area (unplaced or unbounded)",
                    suggested_fix="Place the room or fix its boundaries"
                ))

        return result
