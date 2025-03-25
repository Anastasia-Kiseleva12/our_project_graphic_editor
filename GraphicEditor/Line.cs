using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;

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
        public Line(Point start, Point end, double strokeThickness)
        {
            Start = start;
            End = end;
            StrokeThickness = strokeThickness;
        }
        public bool IsSelected { get; set; }
        public uint Color { get; set; } = unchecked((uint)0xFF00000);
        public void SetColor(byte a, byte r, byte g, byte b) => Color = (uint)((a << 24) | (r << 16) | (g << 8) | b);
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
        public double GetMinDistanceFromCenterToEnd()
        {
            Point center = Center;
            double d1 = DistanceBetweenPoints(center, Start);
            double d2 = DistanceBetweenPoints(center, End);

            return Math.Min(d1, d2);
        }
        private double DistanceBetweenPoints(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public void Scale(double dr)
        {
            double MinScaleDistance = 10.0;
            double currentMinDistance = GetMinDistanceFromCenterToEnd();
            double newMinDistance = currentMinDistance * dr;

            if (newMinDistance < MinScaleDistance)
            {
                dr = MinScaleDistance / currentMinDistance;
                Debug.WriteLine("Достигнут минимальный размер линии");
            }

            Start = new Point (Center.X + (Start.X - Center.X) * dr, Center.Y + (Start.Y - Center.Y) * dr);
            End = new Point (Center.X + (End.X - Center.X) * dr, Center.Y + (End.Y - Center.Y) * dr);
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
            var clonedLine = new Line(new Point(Start.X + 50, Start.Y + 50), new Point(End.X + 50, End.Y + 50), StrokeThickness);

            clonedLine.Color = this.Color;
            return clonedLine;
        }
        public bool IsIn(Point point, double eps)
        {
            double dx = End.X - Start.X;
            double dy = End.Y - Start.Y;
            double lengthSquared = dx * dx + dy * dy;
            double t = ((point.X - Start.X) * dx + (point.Y - Start.Y) * dy) / lengthSquared;
            if (t < 0 || t > 1)
                return false;
            double closestX = Start.X + t * dx;
            double closestY = Start.Y + t * dy;
            double distance = Math.Sqrt((point.X - closestX) * (point.X - closestX) +
                                        (point.Y - closestY) * (point.Y - closestY));
            return distance <= (eps * 0.1 + StrokeThickness / 2);
        }

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
