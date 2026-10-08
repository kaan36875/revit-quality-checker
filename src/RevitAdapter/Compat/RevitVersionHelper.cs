using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitQualityChecker.RevitAdapter.Compat
{
    /// <summary>
    /// Handles Revit API differences across versions using reflection.
    /// Revit 2024+ changed ElementId from int-based to long-based.
    /// </summary>
    public static class RevitVersionHelper
    {
        private static readonly PropertyInfo _valueProperty;
        private static readonly PropertyInfo _integerValueProperty;
        private static readonly bool _is2024OrLater;
        private static readonly ConstructorInfo _longConstructor;
        private static readonly ConstructorInfo _intConstructor;

        static RevitVersionHelper()
        {
            var eidType = typeof(ElementId);
            _valueProperty = eidType.GetProperty("Value");
            _integerValueProperty = eidType.GetProperty("IntegerValue");
            _is2024OrLater = _valueProperty != null;

            _longConstructor = eidType.GetConstructor(new[] { typeof(long) });
            _intConstructor = eidType.GetConstructor(new[] { typeof(int) });
        }

        public static bool Is2024OrLater => _is2024OrLater;

        public static string GetRevitVersion(UIApplication app)
        {
            try
            {
                return app.Application.VersionNumber;
            }
            catch
            {
                return "Unknown";
            }
        }

        public static long GetElementIdValue(ElementId id)
        {
            if (_is2024OrLater && _valueProperty != null)
                return (long)_valueProperty.GetValue(id);

            if (_integerValueProperty != null)
                return (int)_integerValueProperty.GetValue(id);

            throw new InvalidOperationException("Cannot read ElementId value — unsupported Revit version");
        }

        public static string GetElementIdString(ElementId id)
        {
            return GetElementIdValue(id).ToString();
        }

        public static ElementId MakeElementId(long value)
        {
            if (_is2024OrLater && _longConstructor != null)
                return (ElementId)_longConstructor.Invoke(new object[] { value });

            if (_intConstructor != null)
                return (ElementId)_intConstructor.Invoke(new object[] { (int)value });

            throw new InvalidOperationException("Cannot create ElementId — unsupported Revit version");
        }

        public static HashSet<long> GetTaggedElementIds(Element tagElement)
        {
            var ids = new HashSet<long>();

            try
            {
                var method = tagElement.GetType().GetMethod("GetTaggedLocalElementIds");
                if (method != null)
                {
                    var result = method.Invoke(tagElement, null);
                    if (result is IEnumerable<ElementId> elementIds)
                    {
                        foreach (var id in elementIds)
                            ids.Add(GetElementIdValue(id));
                    }
                    return ids;
                }

                var prop = tagElement.GetType().GetProperty("TaggedLocalElementId");
                if (prop != null)
                {
                    var id = prop.GetValue(tagElement) as ElementId;
                    if (id != null)
                        ids.Add(GetElementIdValue(id));
                }
            }
            catch
            {
                // Tag API not available in this version
            }

            return ids;
        }
    }
}
