using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CommandPalette
{
    [Transaction(TransactionMode.Manual)]
    public class OpenPaletteCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
                              ref string message,
                              ElementSet elements)
        {
            App.CurrentApp = commandData.Application;
            App.OpenPalette();
            return Result.Succeeded;
        }
    }
}