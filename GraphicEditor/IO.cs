using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media.Imaging;
using Svg;
using Svg.Transforms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphicEditor
{
    public class IO
    {
        public static Canvas? CanvasToSave;
        public static void SaveToFile(IEnumerable<IFigure> figures, string filePath)
        {
            var figuresInfo = figures.Select(figure => new
            {
                Name = figure.Name,
                PointParameters = FigureFabric.PointParameters(figure.Name)
                    .ToDictionary(p => p, p => figure.GetPointParameter(p)),
                DoubleParameters = FigureFabric.DoubleParameters(figure.Name)
                    .ToDictionary(p => p, p => figure.GetDoubleParameter(p)),
                Color = figure.Color
            }).ToList();

            var jsonString = JsonConvert.SerializeObject(figuresInfo, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }

        public static void LoadFromFile(FigureService figures, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var jsonString = File.ReadAllText(filePath);
            var figuresInfo = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);

            foreach (var figureInfo in figuresInfo)
            {
                if (!figureInfo.TryGetValue("Name", out var nameObj) || nameObj is not string name)
                    continue;

                var pointParams = new Dictionary<string, Point>();
                var doubleParams = new Dictionary<string, double>();
                uint color = 0xFF000000; // черный цвет по умолчанию

                if (figureInfo.TryGetValue("PointParameters", out var pointParamsObj) && pointParamsObj is JObject pointDict)
                {
                    foreach (var (key, value) in pointDict)
                    {
                        var point = value?.ToObject<Point>();
                        if (point != null)
                            pointParams[key] = point;
                    }
                }

                if (figureInfo.TryGetValue("DoubleParameters", out var doubleParamsObj) && doubleParamsObj is JObject doubleDict)
                {
                    foreach (var (key, value) in doubleDict)
                    {
                        if (value?.Type == JTokenType.Float || value?.Type == JTokenType.Integer)
                        {
                            doubleParams[key] = value.ToObject<double>();
                        }
                    }
                }

                if (figureInfo.TryGetValue("Color", out var colorObj) && colorObj is long colorValue)
                {
                    color = (uint)colorValue;
                }

                var figure = figures.Create(name, pointParams, doubleParams);
                figure.Color = color; // устанавливаем цвет фигуре
                figures.AddFigure(figure);
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

            /*var background = new SvgRectangle
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height,
                Fill = new SvgColourServer(System.Drawing.Color.White)
            };
            svgDoc.Children.Add(background);*/

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

        /*private static SvgRectangle CreateSvgRectangle(IFigure figure)
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
                StrokeWidth = (SvgUnit)rectangle.StrokeThickness,
                Transforms = new SvgTransformCollection { new SvgRotate((float)rectangle.Angle, (float)rectangle.Center.X, (float)rectangle.Center.Y) }
            };
        }*/

        private static SvgPolygon CreateSvgRectangle(IFigure figure)
        {
            if (figure is not Rectangle rectangle)
                throw new ArgumentException("Фигура должна быть типа Rectangle.");

            var polygon = new SvgPolygon
            {
                Fill = new SvgColourServer(System.Drawing.Color.Transparent),
                Stroke = new SvgColourServer(System.Drawing.Color.Black),
                StrokeWidth = (SvgUnit)rectangle.StrokeThickness
            };

            // Создаем коллекцию точек и добавляем их
            polygon.Points = new SvgPointCollection
            {
                (SvgUnit)rectangle.P1.X, (SvgUnit)rectangle.P1.Y,
                (SvgUnit)rectangle.P2.X, (SvgUnit)rectangle.P2.Y,
                (SvgUnit)rectangle.P3.X, (SvgUnit)rectangle.P3.Y,
                (SvgUnit)rectangle.P4.X, (SvgUnit)rectangle.P4.Y
            };

            return polygon;
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
