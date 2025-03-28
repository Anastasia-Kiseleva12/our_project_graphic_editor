using System;
using System.Collections.Generic;

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
        void DrawLine(bool selected, Point a, Point b, double strokeThickness, uint Color, double Angle);
        void DrawCircle(bool selected, Point Center, double r, Point PointOnCircle, double strokeThickness, uint Color, double Angle);
        void DrawTriangle(bool IsSelected, Point Point1, Point Point2, Point Point3, double strokeThickness, uint Color, double Angle);
        void DrawRectangle(bool IsSelected, Point P1, Point P2, Point P3, Point P4, double strokeThickness, uint Color, double Angle);
    }
    public interface IFigure
    {
        void Move(Point vector);
        void Rotate(double angle);
        Point Center { get; }
        string Id { get; }
        string Name { get; }
        uint Color { get; set; }

        bool IsSelected { get; set; }
        public double StrokeThickness { get; set; }
        void SetColor(byte a, byte r, byte g, byte b);
        void Scale(double dr);
        void Reflection(Point a, Point b);
        IFigure Clone();
        void Draw(IDrawing drawing, double Angle);
        bool IsIn(Point point, double eps);
        Point GetPointParameter(string parameterName);
        double GetDoubleParameter(string parameterName);
    }

    public interface ILogic
    {
        IEnumerable<IFigure> Figures { get; } //список всех фигур
        IEnumerable<(string, Type)> GetParameters(string figure);
        public IFigure Create(string name, IDictionary<string, Point> parameters, IDictionary<string, double> doubleparameters);
        public IFigure CreateDefault(string name);
        void AddFigure(IFigure figure);
        void RemoveFigure(IFigure figure);
        IFigure Find(Point p, double eps);

        void Select(IFigure f);
        void UnSelect(IFigure f);
    }
}
