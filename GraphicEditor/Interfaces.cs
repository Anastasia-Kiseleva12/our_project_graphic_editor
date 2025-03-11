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
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y);
    }

    public interface IDrawing
    {
        void DrawLine(bool selected, Point a, Point b, double strokeThickness, int Color);
        void DrawCircle(bool selected, Point Center, double r, Point PointOnCircle, double strokeThickness, int Color);
        void DrawTriangle(bool IsSelected, Point Point1, Point Point2, Point Point3, double strokeThickness, int Color);
        void DrawRectangle(bool IsSelected, Point TopLeft, Point BottomRight, double strokeThickness, int Color);
    }
    public interface IDrawingFigure
    {

    }

    public interface IFigure
    {
        void Move(Point vector);
        void Rotate(Point center, double angle);
        Point Center { get; }
        string Id { get; }
        string Name { get; }

        bool IsSelected { get; set; }
        public double StrokeThickness { get; set; }
        void SetColor(byte a, byte r, byte g, byte b);
        void Scale(double dx, double dy);
        void Scale(Point center, double dr);
        void Reflection(Point a, Point b);
        IFigure Clone();
        void Draw(IDrawing drawing);
        bool IsIn(Point point, double eps);
        IFigure Intersect(IFigure other);
        IFigure Union(IFigure other);
        IFigure Subtract(IFigure other);
    }

    public interface ILogic
    {
        IEnumerable<IFigure> Figures { get; } //список всех фигур
        IEnumerable<string> FigureNamesToCreate { get; } //список имен фигур доступных для создания
        IEnumerable<(string, Type)> GetParameters(string figure);
        public IFigure Create(string name, IDictionary<string, Point> parameters, IDictionary<string, double> doubleparameters);
        public IFigure CreateDefault(string name);
        void AddFigure(IFigure figure);
        void RemoveFigure(IFigure figure);
        IFigure Find(Point p, double eps);

        void Select(IFigure f);
        void UnSelect(IFigure f);
        IEnumerable<IFigure> Selected();
    }
}
