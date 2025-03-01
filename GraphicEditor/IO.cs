using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.IO;

namespace GraphicEditor
{
    public class IO
    {
        public static void SaveToFile(IEnumerable<IFigure> figures, string filePath)
        {
            var figuresInfo = new List<Dictionary<string, object>>();

            foreach (var figure in figures)
            {
                var figureInfo = new Dictionary<string, object>
                {
                    { "Name", figure.Name }
                };

                var pointParamsNames = FigureFabric.PointParameters(figure.Name).ToList();
                var doubleParamsNames = FigureFabric.DoubleParameters(figure.Name).ToList();
                var figureType = figure.GetType();

                var pointParams = new Dictionary<string, object>();
                foreach (var paramName in pointParamsNames)
                {
                    var property = figureType.GetProperty(paramName);
                    if (property != null && property.PropertyType == typeof(Point))
                    {
                        var value = (Point)property.GetValue(figure);
                        pointParams[paramName] = new { value.X, value.Y };
                    }
                }

                if (pointParams.Any())
                {
                    figureInfo["PointParameters"] = pointParams;
                }

                var doubleParams = new Dictionary<string, object>();
                foreach (var paramName in doubleParamsNames)
                {
                    var property = figureType.GetProperty(paramName);
                    if (property != null && property.PropertyType == typeof(double))
                    {
                        var value = (double)property.GetValue(figure);
                        doubleParams[paramName] = value;
                    }
                }

                if (doubleParams.Any())
                {
                    figureInfo["DoubleParameters"] = doubleParams;
                }

                figuresInfo.Add(figureInfo);
            }

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(figuresInfo, jsonOptions);
            File.WriteAllText(filePath, jsonString);
        }
    }
}
