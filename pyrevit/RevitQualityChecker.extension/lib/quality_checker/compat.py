"""Revit API version compatibility layer.

Detects the running Revit version and provides helper functions
that work across Revit 2020-2026+.

Key breaking changes handled:
- Revit 2024+: ElementId stores int64, .IntegerValue replaced with .Value
- Revit 2024+: ElementId constructor takes int64 instead of int32
"""

try:
    from Autodesk.Revit.DB import ElementId
    from pyrevit import HOST_APP
    REVIT_VERSION = int(HOST_APP.version)
except Exception:
    REVIT_VERSION = 0

IS_2024_OR_LATER = REVIT_VERSION >= 2024


def get_element_id_value(element_id):
    """Get the integer value from an ElementId (works across all versions)."""
    if IS_2024_OR_LATER:
        return element_id.Value
    return element_id.IntegerValue


def make_element_id(int_value):
    """Create an ElementId from an integer (works across all versions)."""
    try:
        return ElementId(int_value)
    except Exception:
        return ElementId(int(int_value))
