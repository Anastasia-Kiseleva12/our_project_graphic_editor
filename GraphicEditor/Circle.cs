using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;

namespace GraphicEditor
{
    public class Circle : IFigure
    {
        [Export(typeof(IFigureCreator))]
        [ExportMetadata(nameof(FigureMetadata.Name), "Circle")]
        class CircleCreator:IFigureCreator
        {
            public int NumberOfPointParameters => 2;
            public int NumberOfDoubleParameters => 1;
            public IEnumerable<string> PointParametersNames
            {
                get;
            } = ["Center", "PointOnCircle"];
            public IEnumerable<string> DoubleParametersNames
            {
                get;
            } = ["StrokeThickness"];

            public IFigure Create(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
            {
                return new Circle(pointParams["Center"], pointParams["PointOnCircle"], doubleParams["StrokeThickness"]);
            }

            public IFigure CreateDefault()
            {
                return new Circle(new Point (250, 250),
                    new Point (200, 200), 2);
            }
        }
        public Point Center { get; private set; }
        public Point PointOnCircle { get; private set; }
        public double StrokeThickness { get; set; }
        public string Name => "Circle";
        public string Id { get; } = Guid.NewGuid().ToString();
        Circle(Point center, Point pointOnCircle, double strokeThickness)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
            StrokeThickness = strokeThickness;
        }
        public bool IsSelected { get; set; }
        public uint Color { get; set; } = unchecked((uint)0xFF000000);
        public void SetColor(byte a, byte r, byte g, byte b)
        {
            Color = (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public void Move(Point vector)
        {
            Center += vector;
            PointOnCircle += vector;
        }

        public bool ContainsPoint(Point point)
        {
            double distance = Math.Sqrt(Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2));
            return distance <= Radius;
        }

        public void Scale(double dx, double dy)
        {
            throw new NotImplementedException(); // недопустимый метод, изменится тип фигуры
        }

        public void Scale(double dr) => PointOnCircle = new Point(Center.X + (PointOnCircle.X - Center.X) * dr, Center.Y + (PointOnCircle.Y - Center.Y) * dr);

        public IFigure Clone()
        {
            throw new NotImplementedException();
        }
        public double Radius
        {
            get
            {
                var dx = PointOnCircle.X - Center.X;
                var dy = PointOnCircle.Y - Center.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }
        public void Draw(IDrawing drawing, double Angle)
        {
            drawing.DrawCircle(IsSelected, Center, Radius, PointOnCircle, StrokeThickness, Color, Angle);
        }

        public bool IsIn(Point point, double eps)
        {

            double radius = Math.Sqrt(Math.Pow(PointOnCircle.X - Center.X, 2) + Math.Pow(PointOnCircle.Y - Center.Y, 2));

            double distance = Math.Sqrt(Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2));

            return Math.Abs(distance - radius) <= eps;
        }

        public IFigure Intersect(IFigure other)
        {
            throw new NotImplementedException();
        }

        public IFigure Union(IFigure other)
        {
            throw new NotImplementedException();
        }

        public IFigure Subtract(IFigure other)
        {
            throw new NotImplementedException();
        }

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
        {
            Center = pointParams["Center"];
            PointOnCircle = pointParams["PointOnCircle"];
        }
        public void Reflection(Point a, Point b)
        {
            throw new NotImplementedException();
        }
        public void Rotate(double angle)
        {
            Debug.WriteLine($"Rotate method called with angle: {angle}");

            // Переводим угол в радианы
            double radians = -angle * Math.PI / 180.0;

            // Вычисляем разницу координат
            double dx = PointOnCircle.X - Center.X;
            double dy = PointOnCircle.Y - Center.Y;

            // Применяем матрицу поворота
            double newX = Center.X + dx * Math.Cos(radians) - dy * Math.Sin(radians);
            double newY = Center.Y + dx * Math.Sin(radians) + dy * Math.Cos(radians);

            // Обновляем позицию точки
            PointOnCircle = new Point(newX, newY);
        }

        public Point GetPointParameter(string parameterName)
        {
            return parameterName switch
            {
                "Center" => Center,
                "PointOnCircle" => PointOnCircle,
                _ => new Point(0, 0)
            };
        }

        public double GetDoubleParameter(string parameterName)
        {
            return parameterName switch
            {
                "StrokeThickness" => StrokeThickness,
                _ => 0
            };
        }
    }
}
