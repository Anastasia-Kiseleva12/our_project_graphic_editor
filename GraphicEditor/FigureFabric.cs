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
}