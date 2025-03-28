using System;
using System.Collections.Generic;
using System.Composition;

namespace GraphicEditor
{
    public class Triangle : IFigure
    {
        [Export(typeof(IFigureCreator))]
        [ExportMetadata(nameof(FigureMetadata.Name), nameof(Triangle))]
        class TriangleCreator : IFigureCreator
        {
            public int NumberOfPointParameters => 3;
            public int NumberOfDoubleParameters => 1;
            public IEnumerable<string> PointParametersNames
            {
                get
                {
                    yield return "P1";
                    yield return "P2";
                    yield return "P3";
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
                return new Triangle(pointParams["P1"], pointParams["P2"], pointParams["P3"], doubleParams["StrokeThickness"]);
            }
            public IFigure CreateDefault()
            {
                return new Triangle(
                    new Point(50,50),
                    new Point(150,50),
                    new Point (100, 150),
                    2
                );
            }
        }

        public string Name => "Triangle";
        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public Point P3 { get; private set; }

        public string Id { get; } = Guid.NewGuid().ToString();
        Triangle(Point p1, Point p2, Point p3, double strokeThickness)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            StrokeThickness = strokeThickness;
        }
        public bool IsSelected { get; set; }
        public double StrokeThickness { get; set; } = 2;
        public uint Color { get; set; } = unchecked((uint)0xffffff);
        public void SetColor(byte a, byte r, byte g, byte b)
        {
            Color = (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }
        public Point Center => new Point((P1.X + P2.X + P3.X) / 3, (P1.Y + P2.Y + P3.Y) / 3 );

        public void Move(Point vector)
        {
            P1 += vector;
            P2 += vector;
            P3 += vector;
        }
        
        public void Rotate(double angle)
        {
            double radians = -angle * Math.PI / 180;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            Point center = Center;

            P1 = new Point(center.X + (P1.X - center.X) * cos - (P1.Y - center.Y) * sin, center.Y + (P1.X - center.X) * sin + (P1.Y - center.Y) * cos);
            P2 = new Point(center.X + (P2.X - center.X) * cos - (P2.Y - center.Y) * sin, center.Y + (P2.X - center.X) * sin + (P2.Y - center.Y) * cos);
            P3 = new Point(center.X + (P3.X - center.X) * cos - (P3.Y - center.Y) * sin, center.Y + (P3.X - center.X) * sin + (P3.Y - center.Y) * cos);
        }

        public double GetMinDistanceFromCenterToVertex()
        {
            double distanceToP1 = DistanceBetweenPoints(Center, P1);
            double distanceToP2 = DistanceBetweenPoints(Center, P2);
            double distanceToP3 = DistanceBetweenPoints(Center, P3);

            return Math.Min(distanceToP1, Math.Min(distanceToP2, distanceToP3));
        }

        private double DistanceBetweenPoints(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void Scale(double dr)
        {
            const double MinScaleDistance = 20.0;
            const double MinEdgeLength = 10.0;
            double currentMinDistance = GetMinDistanceFromCenterToVertex();
            double newMinDistance = currentMinDistance * dr;

            if (dr < 1 && newMinDistance < MinScaleDistance)
            {
                return;
            }

            double edge1 = DistanceBetweenPoints(P1, P2) * dr;
            double edge2 = DistanceBetweenPoints(P2, P3) * dr;
            double edge3 = DistanceBetweenPoints(P3, P1) * dr;

            if (dr < 1.0 && edge1 < MinEdgeLength || edge2 < MinEdgeLength || edge3 < MinEdgeLength)
            {
                return;
            }

            Point center = Center;
            P1 = new Point(center.X + (P1.X - center.X) * dr, center.Y + (P1.Y - center.Y) * dr);
            P2 = new Point(center.X + (P2.X - center.X) * dr, center.Y + (P2.Y - center.Y) * dr);
            P3 = new Point(center.X + (P3.X - center.X) * dr, center.Y + (P3.Y - center.Y) * dr);
        }


        public void Reflection(Point a, Point b)
        {
            P1 = ReflectPoint(P1, a, b);
            P2 = ReflectPoint(P2, a, b);
            P3 = ReflectPoint(P3, a, b);
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
            var clonedTriangle = new Triangle(
                new Point(P1.X + 50, P1.Y + 50),
                new Point(P2.X + 50, P2.Y + 50),
                new Point(P3.X + 50, P3.Y + 50),
                StrokeThickness
            );

            clonedTriangle.Color = this.Color;

            return clonedTriangle;
        }

        public bool IsIn(Point point, double eps)
        {
            double a = (P1.X - point.X) * (P2.Y - P1.Y) - (P2.X - P1.X) * (P1.Y - point.Y);
            double b = (P2.X - point.X) * (P3.Y - P2.Y) - (P3.X - P2.X) * (P2.Y - point.Y);
            double c = (P3.X - point.X) * (P1.Y - P3.Y) - (P1.X - P3.X) * (P3.Y - point.Y);

            return (a > -eps && b > -eps && c > -eps) || (a < eps && b < eps && c < eps);
        }

        public void Draw(IDrawing drawing, double Angle)
        {
            drawing.DrawTriangle(IsSelected,  P1, P2, P3, StrokeThickness, Color, Angle);
        }

        public Point GetPointParameter(string parameterName)
        {
            return parameterName switch
            {
                "P1" => P1,
                "P2" => P2,
                "P3" => P3,
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
