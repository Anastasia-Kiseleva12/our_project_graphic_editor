using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;

namespace GraphicEditor
{
    public class FigureMetadata
    {
        public string Name { get; }
        public int NumberOfDoubleParameters
        {
            get;
        }
        public int NumberOfPointParameters
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
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] {"First","Second" })]
    public class Line: IFigure
    {
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public Point Center => new Point { X = (Start.X + End.X) / 2, Y = (Start.Y + End.Y) / 2 };
        public Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }
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
        public IEnumerable<IDrawingFigure> GetAsDrawable() => throw new NotImplementedException();
        public bool IsIn(Point point, double eps) => throw new NotImplementedException();
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
        public IEnumerable<Point> GetLinePoints() 
        {

            int x0 = (int)Math.Round(Start.X);
            int y0 = (int)Math.Round(Start.Y);
            int x1 = (int)Math.Round(End.X);
            int y1 = (int)Math.Round(End.Y);

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return new Point { X = x0, Y = y0 };

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

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
    [ExportMetadata("Name", nameof(Circle))]
    public class Circle : IFigure
    {
        public Point Center { get; }
        public Point PointOnCircle { get; }
        public Circle(Point center, Point pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
        }
        public void Move(Point vector) => throw new NotImplementedException();
        public void Rotate(Point center, double angle) => throw new NotImplementedException();

        public void Scale(double dx, double dy) => throw new NotImplementedException();
        public void Scale(Point center, double dr) => throw new NotImplementedException();
        public void Reflection(Point a, Point b) => throw new NotImplementedException();
        public IFigure Clone() => throw new NotImplementedException();
        public IEnumerable<IDrawingFigure> GetAsDrawable() => throw new NotImplementedException();
        public bool IsIn(Point point, double eps) => throw new NotImplementedException();
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void Draw(IDrawing drawing)
        {
            throw new NotImplementedException();
        }

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, Point> pointParams)
        {
            throw new NotImplementedException();
        }
    }
}
