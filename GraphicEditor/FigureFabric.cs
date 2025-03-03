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
    }
    public interface IFigureCreator
    {
        public int NumberOfPointParameters
        {
            get; 
        }
        public int NumberOfDoubleParameters
        {
            get;
        }
        public IEnumerable<string> PointParametersNames
        {
            get;
        }
        public IEnumerable<string> DoubleParametersNames
        {
            get;
        }
        IFigure Create(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams);
        IFigure CreateDefault();
    }

    public static class FigureFabric
    {
        class ImportInfo
        {
            [ImportMany]
            public IEnumerable<Lazy<IFigureCreator, FigureMetadata>> AvailableFigures { get; set; } = [];
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
        public static IFigure CreateFigure(string FigureName,
            IDictionary<string, double> doubleParams,
            IDictionary<string, Point> pointParams)
        {
            return info.AvailableFigures
                .First(f => f.Metadata.Name == FigureName)
                .Value
                .Create(doubleParams,pointParams);
        }
        public static IFigure CreateFigureDefault(string FigureName)
        {
            return info.AvailableFigures
                .First(f => f.Metadata.Name == FigureName)
                .Value
                .CreateDefault();
        }

        public static IEnumerable<string> PointParameters(string FigureName) =>
            info.AvailableFigures
                .First(f => f.Metadata.Name == FigureName)
                .Value.PointParametersNames;

        public static IEnumerable<string> DoubleParameters(string FigureName) =>
            info.AvailableFigures
                .First(f => f.Metadata.Name == FigureName)
                .Value.DoubleParametersNames;
    }

    public class Line: IFigure
    {
        [Export(typeof(IFigureCreator))]
        [ExportMetadata(nameof(FigureMetadata.Name), nameof(Line))]
        class LineCreator:IFigureCreator
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
                return new Line(new Point { X = 50, Y = 50 } ,
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

        public void Draw(IDrawing drawing)
        {
            drawing.DrawLine(IsSelected, Start, End, StrokeThickness);
        }
    }
}
