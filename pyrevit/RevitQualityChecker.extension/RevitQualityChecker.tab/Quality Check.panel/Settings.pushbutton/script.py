"""Open the rules JSON file for editing or select a custom rules file."""
# -*- coding: utf-8 -*-
__title__ = "Settings"
__doc__ = "Open or select the quality check rules file."
__author__ = "Kaan Beyazkilic"

import os

from pyrevit import forms, script

from quality_checker.engine import get_default_rules_path

logger = script.get_logger()
output = script.get_output()


def main():
    options = [
        "Open default rules file",
        "Select custom rules file for this project",
        "Show rules file location",
    ]

    selected = forms.CommandSwitchWindow.show(
        options,
        message="Quality Checker Settings"
    )

    if not selected:
        return

    if selected == options[0]:
        rules_path = get_default_rules_path()
        if os.path.exists(rules_path):
            os.startfile(rules_path)
        else:
            forms.alert("Default rules file not found at:\n{}".format(rules_path))

    elif selected == options[1]:
        source = forms.pick_file(
            file_ext="json",
            title="Select Rules JSON File"
        )
        if source:
            from pyrevit import revit
            doc = revit.doc
            if doc and doc.PathName:
                import shutil
                dest = os.path.join(
                    os.path.dirname(doc.PathName),
                    "quality-rules.json"
                )
                shutil.copy2(source, dest)
                forms.alert(
                    "Custom rules copied to:\n{}\n\n"
                    "The checker will use this file for this project.".format(dest)
                )
            else:
                forms.alert("Save the model first so the rules file can be placed next to it.")

    elif selected == options[2]:
        rules_path = get_default_rules_path()
        output.print_md("**Default rules file:**")
        output.print_md("`{}`".format(rules_path))

        from pyrevit import revit
        doc = revit.doc
        if doc and doc.PathName:
            project_rules = os.path.join(
                os.path.dirname(doc.PathName),
                "quality-rules.json"
            )
            if os.path.exists(project_rules):
                output.print_md("**Project rules file (active):**")
                output.print_md("`{}`".format(project_rules))
            else:
                output.print_md("*No project-specific rules file found.*")


if __name__ == "__main__":
    main()
