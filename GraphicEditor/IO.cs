using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Diagnostics;

namespace GraphicEditor
{
    public class IO
    {
        public static void SaveToFile(IEnumerable<IFigure> figures, string filePath)
        {
            var figuresInfo = new List<Dictionary<string, object>>(); // список для хранения информации о всех фигурах

            foreach (var figure in figures)
            {
                var figureInfo = new Dictionary<string, object>
                {
                    { "Name", figure.Name } // Добавляем имя фигуры
                };

                var pointParamsNames = FigureFabric.PointParameters(figure.Name).ToList(); // список имен точечных параметров
                var doubleParamsNames = FigureFabric.DoubleParameters(figure.Name).ToList(); // список имен числовых параметров

                var pointParams = new Dictionary<string, object>();
                foreach (var paramName in pointParamsNames)
                {
                    Point value = figure.GetPointParameter(paramName);
                    pointParams[paramName] = new { value.X, value.Y };
                }

                if (pointParams.Any())
                {
                    figureInfo["PointParameters"] = pointParams;
                }

                var doubleParams = new Dictionary<string, object>();
                foreach (var paramName in doubleParamsNames)
                {

                    double value = figure.GetDoubleParameter(paramName);
                    doubleParams[paramName] = value;
                }

                if (doubleParams.Any())
                {
                    figureInfo["DoubleParameters"] = doubleParams;
                }

                figuresInfo.Add(figureInfo); // добавляем информацию о фигуре в общий список
            }

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true }; // настройки для форматирования JSON
            var jsonString = JsonSerializer.Serialize(figuresInfo, jsonOptions); // сериализация списка фигур в JSON
            File.WriteAllText(filePath, jsonString); // запись в файл
        }

        public static void LoadFromFile(FigureService figures, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            var jsonString = File.ReadAllText(filePath); // чтение JSON-файла
            using var document = JsonDocument.Parse(jsonString); // парсинг
            var root = document.RootElement; // получение корневого элемента

            foreach (var figureElement in root.EnumerateArray()) // перебор всех объектов в JSON-массиве
            {
                if (!figureElement.TryGetProperty("Name", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
                    continue; // пропускаем фигуру, если у нее нет имени

                string name = nameElement.GetString(); // имя фигуры

                var pointParams = new Dictionary<string, Point>();
                var doubleParams = new Dictionary<string, double>();

                if (figureElement.TryGetProperty("PointParameters", out var pointParamsElement) &&
                    pointParamsElement.ValueKind == JsonValueKind.Object) // проверяем, есть ли точечные параметры
                {
                    foreach (var param in pointParamsElement.EnumerateObject()) // перебираем точечные параметры
                    {
                        if (param.Value.TryGetProperty("X", out var xElement) &&
                            param.Value.TryGetProperty("Y", out var yElement) &&
                            xElement.TryGetDouble(out var x) &&
                            yElement.TryGetDouble(out var y)) // проверяем, что параметры X и Y существуют и являются числами
                        {
                            pointParams[param.Name] = new Point(x, y);
                        }
                    }
                }

                if (figureElement.TryGetProperty("DoubleParameters", out var doubleParamsElement) &&
                    doubleParamsElement.ValueKind == JsonValueKind.Object) // аналогично
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
                figures.AddFigure(figure); // создаем фигуру и добавляем ее
            }
        }
    }
}
