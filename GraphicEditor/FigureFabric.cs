using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace GraphicEditor
{
    public class FigureMetadata
    {
        public string Name { get; set; }
        public int NumberOfPointParameters { get; set; }
        public int NumberOfDoubleParameters { get; set; }
        public IEnumerable<string> PointParametersNames { get; set; }
        public IEnumerable<string> DoubleParametersNames { get; set; }
    }

    public static class FigureFabric
    {
        class ImportInfo
        {
            [ImportMany]
            public IEnumerable<Lazy<IFigure, FigureMetadata>> AvailableFigures { get; set; } = [];
        }
        static ImportInfo info;
        static FigureFabric()
        {
            var assemblies = new[] { typeof(Circle).Assembly };
            var conf = new ContainerConfiguration();
            try
            {
                conf = conf.WithAssemblies(assemblies);
            }
            catch (Exception)
            {
                // ignored
            }

            var cont = conf.CreateContainer();
            info = new ImportInfo();
            cont.SatisfyImports(info);
        }

        public static IEnumerable<string> AvailableFigures => info.AvailableFigures.Select(f => f.Metadata.Name);
        public static IEnumerable<FigureMetadata> AvailableMetadata => info.AvailableFigures.Select(f => f.Metadata);
        public static IFigure CreateFigure(string FigureName)
        {
            return info.AvailableFigures.First(f => f.Metadata.Name == FigureName).Value;
        }
    }
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), nameof(Line))]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.DoubleParametersNames), new string[] { })]  
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] { "First", "Second" })]
    public class Line: IFigure
    {
        public string Name => "Line";
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public Point Center => new Point { X = (Start.X + End.X) / 2, Y = (Start.Y + End.Y) / 2 };

        public string Id { get; } = Guid.NewGuid().ToString();
        public Line() {}
        public Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }
        public bool IsSelected { get; set; }
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
            Start = new Point { X = center.X + (Start.X - center.X) * cosA - (Start.Y - center.Y) * sinA , Y = center.Y + (Start.X - center.X) * sinA + (Start.Y - center.Y) * cosA };
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
        public void Reflection(Point a, Point b) => throw new NotImplementedException();
        public IFigure Clone()
        {
            return new Line(new Point { X = Start.X, Y = Start.Y }, new Point { X = End.X, Y = End.Y });
        }
        public bool IsIn(Point point, double eps) => Math.Abs((point.Y - Start.Y)*(End.X - Start.X) - (End.Y - Start.Y)*(point.X - Start.X)) < eps;
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
        {
            Start = pointParams["First"];
            End = pointParams["Second"];
        }

        public void Draw(IDrawing drawing)
        {
            throw new NotImplementedException();
        }
    }

    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Circle")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.DoubleParametersNames), new string[] { })]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] { "Center", "PointOnCircle" })]
    public class Circle : IFigure
    {
        public Point Center { get; private set; }
        public Point PointOnCircle { get; private set; }
        public string Name => "Circle";
        public string Id { get; } = Guid.NewGuid().ToString();
        public Circle() { }
        public Circle(Point center, Point pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
        }
        public bool IsSelected { get; set; }
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

        public void Draw(IDrawing drawing)
        {
            throw new NotImplementedException();
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
