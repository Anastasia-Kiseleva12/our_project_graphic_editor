using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace GraphicEditor
{
    public class Rectangle : IFigure
    {
        [Export(typeof(IFigureCreator))]
        [ExportMetadata(nameof(FigureMetadata.Name), nameof(Rectangle))]
        class RectangleCreator : IFigureCreator
        {
            public int NumberOfPointParameters => 2;
            public int NumberOfDoubleParameters => 1;
            public IEnumerable<string> PointParametersNames
            {
                get
                {
                    yield return "TopLeft";
                    yield return "BottomRight";
                }
            }
            public IEnumerable<string> DoubleParametersNames
            {
                get
                {
                    yield return "StrokeThickness";
                }
            }
            public IFigure Create(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
            {
                return new Rectangle(pointParams["TopLeft"], pointParams["BottomRight"], doubleParams["StrokeThickness"]);
            }
            public IFigure CreateDefault()
            {
                return new Rectangle(
                    new Point(50, 50),
                    new Point(150, 150),
                    2
                );
            }
        }

        public string Name => "Rectangle";
        public Point TopLeft { get; private set; }
        public Point BottomRight { get; private set; }
        public string Id { get; } = Guid.NewGuid().ToString();

        Rectangle(Point topLeft, Point bottomRight, double strokeThickness)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
            StrokeThickness = strokeThickness;
        }

        public bool IsSelected { get; set; }
        public double StrokeThickness { get; set; } = 2;
        public int Color { get; set; } = unchecked((int)0xFF000000);

        public void SetColor(byte a, byte r, byte g, byte b)
        {
            Color = (a << 24) | (r << 16) | (g << 8) | b;
        }

        public Point Center => new Point((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2);

        public void Move(Point vector)
        {
            TopLeft += vector;
            BottomRight += vector;
        }

        public void Rotate(Point center, double angle)
        {
            double radians = angle * Math.PI / 180;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            center = Center;

            TopLeft = RotatePoint(TopLeft, center, cos, sin);
            BottomRight = RotatePoint(BottomRight, center, cos, sin);
        }

        private Point RotatePoint(Point p, Point center, double cos, double sin)
        {
            return new Point(
                center.X + (p.X - center.X) * cos - (p.Y - center.Y) * sin,
                center.Y + (p.X - center.X) * sin + (p.Y - center.Y) * cos
            );
        }

        public void Scale(double dx, double dy)
        {
            BottomRight = new Point(TopLeft.X + (BottomRight.X - TopLeft.X) * dx, TopLeft.Y + (BottomRight.Y - TopLeft.Y) * dy);
        }

        public void Scale(Point center, double dr)
        {
            center = Center;
            TopLeft = new Point(center.X + (TopLeft.X - center.X) * dr, center.Y + (TopLeft.Y - center.Y) * dr);
            BottomRight = new Point(center.X + (BottomRight.X - center.X) * dr, center.Y + (BottomRight.Y - center.Y) * dr);
        }

        public void Reflection(Point a, Point b)
        {
            TopLeft = ReflectPoint(TopLeft, a, b);
            BottomRight = ReflectPoint(BottomRight, a, b);
        }

        private Point ReflectPoint(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSq = dx * dx + dy * dy;
            double dot = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq;

            double rx = 2 * (a.X + dot * dx) - p.X;
            double ry = 2 * (a.Y + dot * dy) - p.Y;

            return new Point(rx, ry);
        }

        public IFigure Clone()
        {
            return new Rectangle(
                new Point(TopLeft.X, TopLeft.Y),
                new Point(BottomRight.X, BottomRight.Y),
                StrokeThickness
            );
        }

        public bool IsIn(Point point, double eps)
        {
            return point.X >= TopLeft.X - eps && point.X <= BottomRight.X + eps &&
                   point.Y >= TopLeft.Y - eps && point.Y <= BottomRight.Y + eps;
        }

        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void Draw(IDrawing drawing)
        {
            drawing.DrawRectangle(IsSelected, TopLeft, BottomRight, StrokeThickness, Color);
        }

        public Point GetPointParameter(string parameterName)
        {
            return parameterName switch
            {
                "TopLeft" => TopLeft,
                "BottomRight" => BottomRight,
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
