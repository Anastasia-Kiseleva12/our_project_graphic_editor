using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;

namespace GraphicEditor
{
    class FigureMetadata
    {
        public string Name { get; }
    }
    public static class FigureFabric
    {
        class ImportInfo
        {
            [ImportMany]
            public IEnumerable<Lazy<IFigure, FigureMetadata>> AvailableFigures { get; set; } = []; //коллекция доступных фигур
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

        public static IEnumerable<string> AvailableFigures => info.AvailableFigures.Select(f => f.Metadata.Name); //возвращает имена доступных фигур
        public static IFigure CreateFigure(string FigureName)
        {
            return info.AvailableFigures.First(f => f.Metadata.Name == FigureName).Value;
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
    }
}
