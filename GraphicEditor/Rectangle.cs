using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;

namespace GraphicEditor
{
    public class Rectangle : IFigure
    {
        [Export(typeof(IFigureCreator))]
        [ExportMetadata(nameof(FigureMetadata.Name), nameof(Rectangle))]
        class RectangleCreator : IFigureCreator
        {
            public int NumberOfPointParameters => 4;
            public int NumberOfDoubleParameters => 1;
            public IEnumerable<string> PointParametersNames
            {
                get
                {
                    yield return "P1";
                    yield return "P2";
                    yield return "P3";
                    yield return "P4";
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
                return new Rectangle(
                    pointParams["P1"], pointParams["P2"],
                    pointParams["P3"], pointParams["P4"],
                    doubleParams["StrokeThickness"]
                );
            }
            public IFigure CreateDefault()
            {
                return new Rectangle(
                    new Point(50, 50), new Point(150, 50),
                    new Point(150, 150), new Point(50, 150),
                    2
                );
            }
        }

        public string Name => "Rectangle";
        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public Point P3 { get; private set; }
        public Point P4 { get; private set; }
        public string Id { get; } = Guid.NewGuid().ToString();

        Rectangle(Point p1, Point p2, Point p3, Point p4, double strokeThickness)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            StrokeThickness = strokeThickness;
        }

        public bool IsSelected { get; set; }
        public double StrokeThickness { get; set; } = 2;
        public uint Color { get; set; } = unchecked(0xffffff);

        public void SetColor(byte a, byte r, byte g, byte b)
        {
            Color = (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public void Move(Point vector)
        {
            P1 += vector;
            P2 += vector;
            P3 += vector;
            P4 += vector;
        }

        public void Rotate(double angle)
        {
            double radians = -angle * Math.PI / 180;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            Point center = Center;

            P1 = RotatePoint(P1, center, cos, sin);
            P2 = RotatePoint(P2, center, cos, sin);
            P3 = RotatePoint(P3, center, cos, sin);
            P4 = RotatePoint(P4, center, cos, sin);
        }

        private Point RotatePoint(Point p, Point center, double cos, double sin)
        {
            return new Point(
                center.X + (p.X - center.X) * cos - (p.Y - center.Y) * sin,
                center.Y + (p.X - center.X) * sin + (p.Y - center.Y) * cos
            );
        }

        public double GetMinDistanceFromCenterToVertex()
        {
            Point center = Center;
            double d1 = DistanceBetweenPoints(center, P1);
            double d2 = DistanceBetweenPoints(center, P2);
            double d3 = DistanceBetweenPoints(center, P3);
            double d4 = DistanceBetweenPoints(center, P4);

            return Math.Min(Math.Min(d1, d2), Math.Min(d3, d4));
        }

        private double DistanceBetweenPoints(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

            public void Scale(double dr)
            {
                const double MinScaleDistance = 10.0;
                const double MinEdgeLength = 10.0;
                double currentMinDistance = GetMinDistanceFromCenterToVertex();
                double newMinDistance = currentMinDistance * dr;

                if (newMinDistance < MinScaleDistance)
                {
                    dr = MinScaleDistance / currentMinDistance;
                }

                double newWidth = Width * dr;
                double newHeight = Height * dr;

                if (dr < 1.0 && newWidth < MinEdgeLength || newHeight < MinEdgeLength)
                {
                    return;
                }

                Point center = Center;
                P1 = new Point(center.X + (P1.X - center.X) * dr, center.Y + (P1.Y - center.Y) * dr);
                P2 = new Point(center.X + (P2.X - center.X) * dr, center.Y + (P2.Y - center.Y) * dr);
                P3 = new Point(center.X + (P3.X - center.X) * dr, center.Y + (P3.Y - center.Y) * dr);
                P4 = new Point(center.X + (P4.X - center.X) * dr, center.Y + (P4.Y - center.Y) * dr);
            }

        public void Reflection(Point a, Point b)
        {
            P1 = ReflectPoint(P1, a, b);
            P2 = ReflectPoint(P2, a, b);
            P3 = ReflectPoint(P3, a, b);
            P4 = ReflectPoint(P4, a, b);
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
            var clonedRectangle = new Rectangle(
                new Point(P1.X + 50, P1.Y + 50),
                new Point(P2.X + 50, P2.Y + 50),
                new Point(P3.X + 50, P3.Y + 50),
                new Point(P4.X + 50, P4.Y + 50),
                StrokeThickness);

            clonedRectangle.Color = this.Color;

            return clonedRectangle;
        }

        public bool IsIn(Point point, double eps)
        {
            double minX = Math.Min(Math.Min(P1.X, P2.X), Math.Min(P3.X, P4.X));
            double maxX = Math.Max(Math.Max(P1.X, P2.X), Math.Max(P3.X, P4.X));
            double minY = Math.Min(Math.Min(P1.Y, P2.Y), Math.Min(P3.Y, P4.Y));
            double maxY = Math.Max(Math.Max(P1.Y, P2.Y), Math.Max(P3.Y, P4.Y));

            bool nearLeftEdge = Math.Abs(point.X - minX) <= eps && point.Y >= minY && point.Y <= maxY;
            bool nearRightEdge = Math.Abs(point.X - maxX) <= eps && point.Y >= minY && point.Y <= maxY;
            bool nearTopEdge = Math.Abs(point.Y - minY) <= eps && point.X >= minX && point.X <= maxX;
            bool nearBottomEdge = Math.Abs(point.Y - maxY) <= eps && point.X >= minX && point.X <= maxX;

            return nearLeftEdge || nearRightEdge || nearTopEdge || nearBottomEdge;
        }

        public void Draw(IDrawing drawing, double Angle)
        {
            drawing.DrawRectangle(IsSelected, P1, P2, P3, P4, StrokeThickness, Color, Angle);
        }

        public Point GetPointParameter(string parameterName)
        {
            return parameterName switch
            {
                "P1" => P1,
                "P2" => P2,
                "P3" => P3,
                "P4" => P4,
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

        public Point Center
        {
            get
            {
                return new Point(
                    (P1.X + P2.X + P3.X + P4.X) / 4,
                    (P1.Y + P2.Y + P3.Y + P4.Y) / 4
                );
            }
        }
        public Point GetFirstPoint()
        {
            return new[] { P1, P2, P3, P4 }.OrderBy(p => p.X).ThenBy(p => p.Y).First();
        }
        public double Width => Math.Abs(P2.X - P1.X);
        public double Height => Math.Abs(P3.Y - P1.Y);
    }
}
