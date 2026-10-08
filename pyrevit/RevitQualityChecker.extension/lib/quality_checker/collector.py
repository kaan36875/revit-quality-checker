"""Revit element collector - gathers elements using FilteredElementCollector
and converts them to ElementInfo objects for the rule engine.

This module depends on the Revit API (available inside pyRevit).
"""

from quality_checker.models import ElementInfo
from quality_checker.compat import get_element_id_value

from Autodesk.Revit.DB import (
    FilteredElementCollector,
    BuiltInCategory,
    View,
    ViewType,
)


# Categories we collect and their BuiltInCategory mapping
CATEGORY_MAP = {
    "Walls": BuiltInCategory.OST_Walls,
    "Doors": BuiltInCategory.OST_Doors,
    "Windows": BuiltInCategory.OST_Windows,
    "Rooms": BuiltInCategory.OST_Rooms,
    "Levels": BuiltInCategory.OST_Levels,
    "Columns": BuiltInCategory.OST_Columns,
    "Floors": BuiltInCategory.OST_Floors,
    "Ceilings": BuiltInCategory.OST_Ceilings,
    "Stairs": BuiltInCategory.OST_Stairs,
    "Furniture": BuiltInCategory.OST_Furniture,
}

# Parameters we always try to read from each element
COMMON_PARAMETERS = [
    "Mark",
    "Comments",
    "Fire Rating",
    "Number",
    "Area",
    "Volume",
    "Level",
    "Type Name",
    "Family Name",
    "Workset",
]


def collect_elements(doc, categories=None):
    """Collect elements from the Revit document.

    Args:
        doc: The active Revit Document.
        categories: List of category names to collect. If None, collects all mapped categories.

    Returns:
        List of ElementInfo objects.
    """
    if categories is None:
        categories = list(CATEGORY_MAP.keys())

    all_elements = []

    for cat_name in categories:
        bic = CATEGORY_MAP.get(cat_name)
        if bic is None:
            continue

        collector = (
            FilteredElementCollector(doc)
            .OfCategory(bic)
            .WhereElementIsNotElementType()
        )

        for element in collector:
            info = _convert_element(element, cat_name)
            if info:
                all_elements.append(info)

    all_elements.extend(_collect_views(doc))

    return all_elements


def _convert_element(element, category_name):
    """Convert a Revit element to an ElementInfo."""
    try:
        elem_id = get_element_id_value(element.Id)
        name = _get_element_name(element)

        family_name = ""
        type_name = ""
        elem_type = element.Document.GetElement(element.GetTypeId())
        if elem_type:
            type_name = getattr(elem_type, "Name", "") or ""
            family_name = getattr(elem_type, "FamilyName", "") or ""

        params = _read_parameters(element)

        return ElementInfo(
            element_id=elem_id,
            name=name,
            category=category_name,
            family_name=family_name,
            type_name=type_name,
            parameters=params,
        )
    except Exception:
        return None


def _get_element_name(element):
    """Get the best available name for an element."""
    name = getattr(element, "Name", None)
    if name:
        return name

    elem_type = element.Document.GetElement(element.GetTypeId())
    if elem_type:
        return getattr(elem_type, "Name", "") or ""

    return ""


def _read_parameters(element):
    """Read common parameters from an element."""
    params = {}
    for param_name in COMMON_PARAMETERS:
        param = element.LookupParameter(param_name)
        if param and param.HasValue:
            try:
                params[param_name] = param.AsValueString() or param.AsString() or ""
            except Exception:
                params[param_name] = ""
    return params


def _collect_views(doc):
    """Collect views (excluding templates and system views)."""
    elements = []
    collector = (
        FilteredElementCollector(doc)
        .OfClass(View)
        .WhereElementIsNotElementType()
    )

    for view in collector:
        if view.IsTemplate:
            continue
        if view.ViewType in (ViewType.Internal, ViewType.Undefined):
            continue

        params = _read_parameters(view)
        elements.append(ElementInfo(
            element_id=get_element_id_value(view.Id),
            name=view.Name or "",
            category="Views",
            family_name="",
            type_name=str(view.ViewType),
            parameters=params,
        ))

    return elements
