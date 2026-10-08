"""Rule engine - loads rules from JSON config and runs them."""

import json
import os
from datetime import datetime

from quality_checker.models import CheckReport
from quality_checker.rules import (
    NamingRule,
    RequiredParameterRule,
    DuplicateNameRule,
    UnplacedRoomRule,
)

RULE_TYPES = {
    "naming": NamingRule,
    "required_parameter": RequiredParameterRule,
    "duplicate_name": DuplicateNameRule,
    "unplaced_room": UnplacedRoomRule,
}


def load_rules_from_file(file_path):
    """Load rule definitions from a JSON file and create rule instances."""
    with open(file_path, "r") as f:
        data = json.load(f)

    return _create_rules(data)


def load_rules_from_json(json_string):
    """Load rule definitions from a JSON string."""
    data = json.loads(json_string)
    return _create_rules(data)


def _create_rules(data):
    """Create rule instances from parsed JSON data."""
    rules = []
    seen_ids = set()

    for rule_def in data.get("rules", []):
        rule_id = rule_def.get("id", "")
        if not rule_id:
            raise ValueError("Every rule must have an 'id'.")
        if rule_id in seen_ids:
            raise ValueError("Duplicate rule id: '{}'".format(rule_id))
        seen_ids.add(rule_id)

        rule_type = rule_def.get("type", "").lower()
        if rule_type not in RULE_TYPES:
            raise ValueError("Unknown rule type '{}' for rule '{}'".format(
                rule_type, rule_id))

        rule = _create_single_rule(rule_def, rule_type)
        rules.append(rule)

    return rules


def _create_single_rule(rule_def, rule_type):
    """Create a single rule instance from its definition."""
    rule_id = rule_def["id"]
    name = rule_def.get("name", rule_id)
    description = rule_def.get("description", "")
    severity = rule_def.get("severity", "Warning")
    target_category = rule_def.get("targetCategory", "")
    params = rule_def.get("parameters", {})

    if rule_type == "naming":
        pattern = params.get("pattern")
        if not pattern:
            raise ValueError("Rule '{}': naming rule requires 'pattern' parameter.".format(rule_id))
        return NamingRule(rule_id, name, description, severity, target_category, pattern)

    elif rule_type == "required_parameter":
        param_name = params.get("parameterName")
        if not param_name:
            raise ValueError("Rule '{}': required_parameter rule requires 'parameterName'.".format(rule_id))
        return RequiredParameterRule(rule_id, name, description, severity, target_category, param_name)

    elif rule_type == "duplicate_name":
        return DuplicateNameRule(rule_id, name, description, severity, target_category)

    elif rule_type == "unplaced_room":
        return UnplacedRoomRule(rule_id, name, description, severity)


def run_check(model_name, elements, rules):
    """Run all rules against the elements and return a CheckReport."""
    report = CheckReport(model_name)
    report.timestamp = datetime.now()

    for rule in rules:
        result = rule.evaluate(elements)
        report.results.append(result)

    return report


def get_default_rules_path():
    """Return path to the default rules JSON file shipped with the extension."""
    lib_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    return os.path.join(lib_dir, "rules", "default-rules.json")
