using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace GraphicEditor
{
    public class Line : IFigure
    {
        [Export(typeof(IFigureCreator))]
        [ExportMetadata(nameof(FigureMetadata.Name), nameof(Line))]
        class LineCreator : IFigureCreator
        {
            public int NumberOfPointParameters => 2;
            public int NumberOfDoubleParameters => 1;
            public IEnumerable<string> PointParametersNames
            {
                get
                {
                    yield return "Start";
                    yield return "End";
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
                return new Line(pointParams["Start"], pointParams["End"], doubleParams["StrokeThickness"]);
            }
            public IFigure CreateDefault()
            {
                return new Line(new Point (50, 50),
                new Point (150, 150), 2);
            }
        }

        public string Name => "Line";
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public double StrokeThickness { get; set; }
        public Point Center => new Point ((Start.X + End.X) / 2, (Start.Y + End.Y) / 2);

        public string Id { get; } = Guid.NewGuid().ToString();
        Line(Point start, Point end, double strokeThickness)
        {
            Start = start;
            End = end;
            StrokeThickness = strokeThickness;
        }
        public bool IsSelected { get; set; }
        public int Color { get; set; } = unchecked((int)0xFF000000);
        public void SetColor(byte a, byte r, byte g, byte b) => Color = (a << 24) | (r << 16) | (g << 8) | b;
        public void Move(Point vector)
        {
            Start += vector;
            End += vector;
        }
        public void Rotate(double angle)
        {
            double rad = -angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            Point center = Center;
            Start = new Point (center.X + (Start.X - center.X) * cosA - (Start.Y - center.Y) * sinA, center.Y + (Start.X - center.X) * sinA + (Start.Y - center.Y) * cosA);
            End = new Point (center.X + (End.X - center.X) * cosA - (End.Y - center.Y) * sinA, center.Y + (End.X - center.X) * sinA + (End.Y - center.Y) * cosA);
        }

        public void Scale(double dx, double dy)
        {
            Start = new Point (Start.X * dx, Start.Y * dy);
            End = new Point (End.X * dx, End.Y * dy);
        }
        public void Scale(Point center, double dr)
        {
            Start = new Point (center.X + (Start.X - center.X) * dr, center.Y + (Start.Y - center.Y) * dr);
            End = new Point (center.X + (End.X - center.X) * dr, center.Y + (End.Y - center.Y) * dr);
        }
        public void Reflection(Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double d = dx * dx + dy * dy;
            double x = ((Start.X * dx + Start.Y * dy - a.X * dx - a.Y * dy) * dx + a.X * d) / d * 2 - Start.X;
            double y = ((Start.X * dx + Start.Y * dy - a.X * dx - a.Y * dy) * dy + a.Y * d) / d * 2 - Start.Y;
            Start = new Point (x, y);
            x = ((End.X * dx + End.Y * dy - a.X * dx - a.Y * dy) * dx + a.X * d) / d * 2 - End.X;
            y = ((End.X * dx + End.Y * dy - a.X * dx - a.Y * dy) * dy + a.Y * d) / d * 2 - End.Y;
            End = new Point (x, y);
        }
        public IFigure Clone()
        {
            return new Line(new Point (Start.X, Start.Y), new Point (End.X, End.Y), StrokeThickness);
        }
        public bool IsIn(Point point, double tolerance = 5)
        {
            double distance = Math.Abs((End.Y - Start.Y) * point.X - (End.X - Start.X) * point.Y + End.X * Start.Y - End.Y * Start.X) / Math.Sqrt(Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));
            return distance <= tolerance && point.X >= Math.Min(Start.X, End.X) - tolerance && point.X <= Math.Max(Start.X, End.X) + tolerance &&
                   point.Y >= Math.Min(Start.Y, End.Y) - tolerance && point.Y <= Math.Max(Start.Y, End.Y) + tolerance;
        }
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void Draw(IDrawing drawing, double Angle)
        {
            drawing.DrawLine(IsSelected, Start, End, StrokeThickness, Color, Angle);
        }

        public Point GetPointParameter(string parameterName)
        {
            return parameterName switch
            {
                "Start" => Start,
                "End" => End,
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
