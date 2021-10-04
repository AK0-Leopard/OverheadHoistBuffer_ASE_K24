using System.Windows;

namespace VehicleControl_Viewer.Vo
{
    public class Address
    {
        public string ID { get; private set; }
        public double X, Y;
        public Point Point { get; private set; }
        public Address() { }
        public Address(string id, double x, double y)
        {
            ID = id;
            X = x;
            Y = y;
            Point = new Point(x, y);
        }
    }
}