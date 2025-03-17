using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media.Imaging;
using Svg;

namespace GraphicEditor
{
    public class IO
    {
        public static Canvas? CanvasToSave;
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

        public static void SaveToSvg(IEnumerable<IFigure> figures, string filePath)
        {
            int width = (int)CanvasToSave.Bounds.Width;
            int height = (int)CanvasToSave.Bounds.Height;

            var svgDoc = new SvgDocument
            {
                Width = width,
                Height = height
            };

            var background = new SvgRectangle
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height,
                Fill = new SvgColourServer(System.Drawing.Color.White)
            };
            svgDoc.Children.Add(background);

            foreach (var figure in figures)
            {
                switch (figure.Name)
                {
                    case "Line":
                        svgDoc.Children.Add(CreateSvgLine(figure));
                        break;
                    case "Circle":
                        svgDoc.Children.Add(CreateSvgCircle(figure));
                        break;
                    case "Triangle":
                        svgDoc.Children.Add(CreateSvgTriangle(figure));
                        break;
                    case "Rectangle":
                        svgDoc.Children.Add(CreateSvgRectangle(figure));
                        break;
                }
            }

            svgDoc.Write(filePath); // Сохранение SVG в файл
        }
        //метод для преобразования линии в элемент <line> и сохранения его в SVG.
        private static SvgLine CreateSvgLine(IFigure figure)
        {
            if (figure is not Line line)
                throw new ArgumentException("Фигура должна быть типа Line.");

            return new SvgLine
            {
                StartX = (SvgUnit)line.Start.X,
                StartY = (SvgUnit)line.Start.Y,
                EndX = (SvgUnit)line.End.X,
                EndY = (SvgUnit)line.End.Y,
                Stroke = new SvgColourServer(System.Drawing.Color.Black),
                StrokeWidth = (SvgUnit)line.StrokeThickness
            };
        }

        //метод для преобразования круга в элемент <circle> и сохранения его в SVG.
        private static SvgCircle CreateSvgCircle(IFigure figure)
        {
            if (figure is not Circle circle)
                throw new ArgumentException("Фигура должна быть типа Circle.");

            return new SvgCircle
            {
                CenterX = (SvgUnit)circle.Center.X,
                CenterY = (SvgUnit)circle.Center.Y,
                Radius = (SvgUnit)circle.Radius,
                Fill = new SvgColourServer(System.Drawing.Color.Transparent),
                Stroke = new SvgColourServer(System.Drawing.Color.Black),
                StrokeWidth = (SvgUnit)circle.StrokeThickness
            };
        }

        private static SvgPolygon CreateSvgTriangle(IFigure figure)
        {
            if (figure is not Triangle triangle)
                throw new ArgumentException("Фигура должна быть типа Triangle.");

            var polygon = new SvgPolygon
            {
                Fill = new SvgColourServer(System.Drawing.Color.Transparent),
                Stroke = new SvgColourServer(System.Drawing.Color.Black),
                StrokeWidth = (SvgUnit)triangle.StrokeThickness
            };

            // Создаем коллекцию точек и добавляем их
            polygon.Points = new SvgPointCollection
            {
                (SvgUnit)triangle.P1.X, (SvgUnit)triangle.P1.Y,
                (SvgUnit)triangle.P2.X, (SvgUnit)triangle.P2.Y,
                (SvgUnit)triangle.P3.X, (SvgUnit)triangle.P3.Y
            };

            return polygon;
        }

        private static SvgRectangle CreateSvgRectangle(IFigure figure)
        {
            if (figure is not Rectangle rectangle)
                throw new ArgumentException("Фигура должна быть типа Rectangle.");

            return new SvgRectangle
            {
                X = (SvgUnit)rectangle.GetFirstPoint().X,
                Y = (SvgUnit)rectangle.GetFirstPoint().Y,
                Width = (SvgUnit)rectangle.Width,
                Height = (SvgUnit)rectangle.Height,
                Fill = new SvgColourServer(System.Drawing.Color.Transparent),
                Stroke = new SvgColourServer(System.Drawing.Color.Black),
                StrokeWidth = (SvgUnit)rectangle.StrokeThickness
            };
        }
        public static void SaveToPng(string filePath)
        {
            // определяем размер канваса
            var size = new Size(CanvasToSave.Bounds.Width, CanvasToSave.Bounds.Height);

            // создаем RenderTargetBitmap
            var bitmap = new RenderTargetBitmap(new PixelSize((int)size.Width, (int)size.Height));

            // отрисовываем Canvas в битмап
            CanvasToSave.Measure(size);
            CanvasToSave.Arrange(new Rect(size));
            bitmap.Render(CanvasToSave);

            // сохраняем в PNG
            using (var stream = File.Create(filePath))
            {
                bitmap.Save(stream);
            }
        }
    }
}
