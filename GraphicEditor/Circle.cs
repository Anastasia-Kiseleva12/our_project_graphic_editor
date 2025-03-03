using System;
using System.Collections.Generic;
using System.Composition;
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
            public int NumberOfDoubleParameters => 0;
            public IEnumerable<string> PointParametersNames
            {
                get;
            } = ["Center", "PointOnCircle"];
            public IEnumerable<string> DoubleParametersNames => Enumerable.Empty<string>();

            public IFigure Create(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
            {
                return new Circle(pointParams["Center"], pointParams["PointOnCircle"]);
            }

            public IFigure CreateDefault()
            {
                return new Circle(new Point { X = 250, Y = 250 },
                    new Point { X = 200, Y = 200 });
            }
        }
        public Point Center { get; private set; }
        public Point PointOnCircle { get; private set; }
        public string Name => "Circle";
        public string Id { get; } = Guid.NewGuid().ToString();
        Circle(Point center, Point pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
        }
        public bool IsSelected { get; set; }
        public double StrokeThickness { get; set; } = 2;
        public void Move(Point vector)
        {
            Center = new Point { X = Center.X + vector.X, Y = Center.Y + vector.Y };
            PointOnCircle = new Point { X = PointOnCircle.X + vector.X, Y = PointOnCircle.Y + vector.Y };
        }

        public void Scale(double dx, double dy)
        {
            throw new NotImplementedException();
        }

        public void Scale(Point center, double dr)
        {
            throw new NotImplementedException();
        }

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
        public void Draw(IDrawing drawing)
        {
            drawing.DrawCircle(IsSelected, Center, Radius, PointOnCircle, StrokeThickness);
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
        public void Rotate(Point center, double angle)
        {
            throw new NotImplementedException();
        }

    }
}
