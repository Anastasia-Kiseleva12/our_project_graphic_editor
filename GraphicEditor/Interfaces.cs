using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicEditor
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public interface Idrawing
    {
        void DrawLine(Point a, Point b);
        void DrawCircle(Point Center, double r);
    }
    public interface IDrawingFigure
    {

    }

    public interface IFigure
    {
        void Move(Point vector);
        void Rotate(Point center, double angle);
        Point Center { get; }
        void Scale(double dx, double dy);
        void Scale(Point center, double dr);
        void Reflection(Point a, Point b);
        IFigure Clone();
        IEnumerable<IDrawingFigure> GetAsDrawable();
        bool IsIn(Point point, double eps);
        IFigure Intersect(IFigure other);
        IFigure Union(IFigure other);
        IFigure Subtract(IFigure other);
    }

    public interface ILogic
    {
        IEnumerable<IFigure> Figures { get; }
        void Save(string FilePath, string FileFormat);
        void Load(string FilePath, string FileFormat);
        IEnumerable<string> FigureNamesToCreate { get; }
        IEnumerable<(string, Type)> GetParameters(string figure);
        IFigure Create(string name, IDictionary<string, object> parameters);
        void AddFigure(IFigure figure);
        void RemoveFigure(IFigure figure);
        IFigure Find(Point p, double eps);

        void Select(IFigure f);
        void UnSelect(IFigure f);
        IEnumerable<IFigure> Selected();
    }
}
