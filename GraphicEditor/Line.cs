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
            public int NumberOfDoubleParameters => 0;
            public IEnumerable<string> PointParametersNames
            {
                get
                {
                    yield return "Start";
                    yield return "End";
                }
            }
            public IEnumerable<string> DoubleParametersNames => Enumerable.Empty<string>();
            public IFigure Create(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
            {
                return new Line(pointParams["Start"], pointParams["End"]);
            }
            public IFigure CreateDefault()
            {
                return new Line(new Point { X = 50, Y = 50 },
                new Point { X = 150, Y = 150 });
            }
        }

        public string Name => "Line";
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public Point Center => new Point { X = (Start.X + End.X) / 2, Y = (Start.Y + End.Y) / 2 };

        public string Id { get; } = Guid.NewGuid().ToString();
        Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }
        public bool IsSelected { get; set; }
        public double StrokeThickness { get; set; } = 2;
        public void Move(Point vector)
        {
            Start = new Point { X = Start.X + vector.X, Y = Start.Y + vector.Y };
            End = new Point { X = End.X + vector.X, Y = End.Y + vector.Y };
        }
        public void Rotate(Point center, double angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            Start = new Point { X = center.X + (Start.X - center.X) * cosA - (Start.Y - center.Y) * sinA, Y = center.Y + (Start.X - center.X) * sinA + (Start.Y - center.Y) * cosA };
            End = new Point { X = center.X + (End.X - center.X) * cosA - (End.Y - center.Y) * sinA, Y = center.Y + (End.X - center.X) * sinA + (End.Y - center.Y) * cosA };
        }

        public void Scale(double dx, double dy)
        {
            Start = new Point { X = Start.X * dx, Y = Start.Y * dy };
            End = new Point { X = End.X * dx, Y = End.Y * dy };
        }
        public void Scale(Point center, double dr)
        {
            Start = new Point { X = center.X + (Start.X - center.X) * dr, Y = center.Y + (Start.Y - center.Y) * dr };
            End = new Point { X = center.X + (End.X - center.X) * dr, Y = center.Y + (End.Y - center.Y) * dr };
        }
        public void Reflection(Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double d = dx * dx + dy * dy;
            double x = ((Start.X * dx + Start.Y * dy - a.X * dx - a.Y * dy) * dx + a.X * d) / d * 2 - Start.X;
            double y = ((Start.X * dx + Start.Y * dy - a.X * dx - a.Y * dy) * dy + a.Y * d) / d * 2 - Start.Y;
            Start = new Point { X = x, Y = y };
            double x = ((End.X * dx + End.Y * dy - a.X * dx - a.Y * dy) * dx + a.X * d) / d * 2 - End.X;
            double y = (End.X * dx + End.Y * dy - a.X * dx - a.Y * dy) * dy + a.Y * d) / d * 2 - End.Y;
            End = new Point { X = x, Y = y };
        }
        public IFigure Clone()
        {
            return new Line(new Point { X = Start.X, Y = Start.Y }, new Point { X = End.X, Y = End.Y });
        }
        public bool IsIn(Point point, double eps) => Math.Abs((point.Y - Start.Y) * (End.X - Start.X) - (End.Y - Start.Y) * (point.X - Start.X)) < eps;
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void Draw(IDrawing drawing)
        {
            drawing.DrawLine(IsSelected, Start, End, StrokeThickness);
        }
    }
}
