using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitQualityChecker.RevitAdapter.Views;

namespace RevitQualityChecker.RevitAdapter
{
    [Transaction(TransactionMode.Manual)]
    public class AboutCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            new AboutWindow().ShowDialog();
            return Result.Succeeded;
        }
    }
}
