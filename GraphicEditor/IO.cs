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

        public static void LoadFromFile(FigureService figures, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            var jsonString = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("Incorrect JSON file.");
            }

            foreach (var figureElement in root.EnumerateArray())
            {
                if (!figureElement.TryGetProperty("Name", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
                    continue;

                string name = nameElement.GetString();

                var pointParams = new Dictionary<string, Point>();
                var doubleParams = new Dictionary<string, double>();

                if (figureElement.TryGetProperty("PointParameters", out var pointParamsElement) &&
                    pointParamsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var param in pointParamsElement.EnumerateObject())
                    {
                        if (param.Value.TryGetProperty("X", out var xElement) &&
                            param.Value.TryGetProperty("Y", out var yElement) &&
                            xElement.TryGetDouble(out var x) &&
                            yElement.TryGetDouble(out var y))
                        {
                            pointParams[param.Name] = new Point { X = x, Y = y };
                        }
                    }
                }

                if (figureElement.TryGetProperty("DoubleParameters", out var doubleParamsElement) &&
                    doubleParamsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var param in doubleParamsElement.EnumerateObject())
                    {
                        if (param.Value.TryGetDouble(out var value))
                        {
                            doubleParams[param.Name] = value;
                        }
                    }
                }

                var figure = figures.Create(name, pointParams, doubleParams);
                figures.AddFigure(figure);
            }
        }
    }
}