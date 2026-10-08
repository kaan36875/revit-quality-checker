using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.RevitAdapter.Compat;

namespace RevitQualityChecker.RevitAdapter
{
    public class RevitElementCollector
    {
        private static readonly Dictionary<string, BuiltInCategory> CategoryMap = new()
        {
            ["Walls"] = BuiltInCategory.OST_Walls,
            ["Doors"] = BuiltInCategory.OST_Doors,
            ["Windows"] = BuiltInCategory.OST_Windows,
            ["Rooms"] = BuiltInCategory.OST_Rooms,
            ["Levels"] = BuiltInCategory.OST_Levels,
            ["Columns"] = BuiltInCategory.OST_Columns,
            ["Floors"] = BuiltInCategory.OST_Floors,
            ["Ceilings"] = BuiltInCategory.OST_Ceilings,
            ["Stairs"] = BuiltInCategory.OST_Stairs,
            ["Furniture"] = BuiltInCategory.OST_Furniture,
        };

        private static readonly string[] CommonParameters =
        {
            "Mark", "Comments", "Fire Rating", "Number",
            "Area", "Volume", "Level", "Type Name",
            "Family Name", "Workset"
        };

        public IReadOnlyList<ElementInfo> CollectElements(Document doc)
        {
            var all = new List<ElementInfo>();
            var taggedIds = CollectTaggedElementIds(doc);

            foreach (var (categoryName, bic) in CategoryMap)
            {
                var collector = new FilteredElementCollector(doc)
                    .OfCategory(bic)
                    .WhereElementIsNotElementType();

                foreach (var element in collector)
                {
                    var info = ConvertElement(element, categoryName);
                    if (info == null) continue;

                    if (categoryName is "Rooms" or "Doors" or "Windows")
                    {
                        var idVal = RevitVersionHelper.GetElementIdValue(element.Id);
                        info.Parameters["_HasTag"] = taggedIds.Contains(idVal) ? "Tagged" : "";
                    }

                    if (categoryName == "Walls" && element is Wall wall)
                    {
                        AddWallJoinInfo(wall, info);
                    }

                    all.Add(info);
                }
            }

            CollectViews(doc, all);
            return all;
        }

        private HashSet<long> CollectTaggedElementIds(Document doc)
        {
            var ids = new HashSet<long>();
            try
            {
                var tags = new FilteredElementCollector(doc)
                    .OfClass(typeof(IndependentTag))
                    .WhereElementIsNotElementType();

                foreach (var tag in tags)
                {
                    var tagIds = RevitVersionHelper.GetTaggedElementIds(tag);
                    foreach (var id in tagIds)
                        ids.Add(id);
                }
            }
            catch
            {
                // IndependentTag collection not available
            }

            try
            {
                var roomTags = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsNotElementType();

                foreach (var tagElement in roomTags)
                {
                    if (tagElement is RoomTag roomTag && roomTag.Room != null)
                    {
                        ids.Add(RevitVersionHelper.GetElementIdValue(roomTag.Room.Id));
                    }
                }
            }
            catch
            {
                // Room tags not available
            }

            return ids;
        }

        private void AddWallJoinInfo(Wall wall, ElementInfo info)
        {
            try
            {
                bool end0 = WallUtils.IsWallJoinAllowedAtEnd(wall, 0);
                bool end1 = WallUtils.IsWallJoinAllowedAtEnd(wall, 1);
                info.Parameters["_WallJoinStatus"] = (end0 && end1) ? "Joined" : "";
            }
            catch
            {
                // API not available
            }
        }

        private ElementInfo ConvertElement(Element element, string categoryName)
        {
            try
            {
                var elemType = element.Document.GetElement(element.GetTypeId());
                var typeName = elemType?.Name ?? "";
                var familyName = (elemType as FamilySymbol)?.FamilyName ?? "";

                return new ElementInfo
                {
                    Id = RevitVersionHelper.GetElementIdString(element.Id),
                    Name = element.Name ?? typeName,
                    Category = categoryName,
                    FamilyName = familyName,
                    TypeName = typeName,
                    Parameters = ReadParameters(element)
                };
            }
            catch
            {
                return null;
            }
        }

        private void CollectViews(Document doc, List<ElementInfo> results)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType();

            foreach (View view in collector)
            {
                if (view.IsTemplate)
                    continue;
                if (view.ViewType is ViewType.Internal or ViewType.Undefined)
                    continue;

                results.Add(new ElementInfo
                {
                    Id = RevitVersionHelper.GetElementIdString(view.Id),
                    Name = view.Name ?? "",
                    Category = "Views",
                    FamilyName = "",
                    TypeName = view.ViewType.ToString(),
                    Parameters = ReadParameters(view)
                });
            }
        }

        private Dictionary<string, string> ReadParameters(Element element)
        {
            var dict = new Dictionary<string, string>();
            foreach (var name in CommonParameters)
            {
                var param = element.LookupParameter(name);
                if (param is { HasValue: true })
                {
                    try
                    {
                        dict[name] = param.AsValueString() ?? param.AsString() ?? "";
                    }
                    catch
                    {
                        dict[name] = "";
                    }
                }
            }
            return dict;
        }
    }
}
