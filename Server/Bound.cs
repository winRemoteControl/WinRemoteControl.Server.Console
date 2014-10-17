using System.Drawing;
using Newtonsoft.Json;

namespace Utils
{
    public class Bound
    {
        public int X, Y, W, H;

        [JsonIgnore]
        public bool IsEmpty
        {
            get { return this.X == 0 & this.Y == 0 & this.W == 0 & this.H == 0; }
        }

        public Bound()
        {            
        }

        public Bound(Rectangle r)
        {
            this.From(r);
        }

        public Bound(Bound r)
        {
            this.From(r);
        }

        public void From(Rectangle r)
        {
            this.X = r.X;
            this.Y = r.Y;
            this.W = r.Width;
            this.H = r.Height;
        }

        public void From(Bound r)
        {
            this.X = r.X;
            this.Y = r.Y;
            this.W = r.W;
            this.H = r.H;
        }

        public Rectangle ToRectangle(int xAdjust = 0, int yAdjust = 0)
        {
            var r = new Rectangle()
            {
                X      = this.X + xAdjust,
                Y      = this.Y + yAdjust,
                Width  = this.W,
                Height = this.H,
            };
            return r;
        }

        public override string ToString()
        {
            return string.Format("x:{0}, y:{1}, w:{2}, h:{3}", this.X, this.Y, this.W, this.H);
        }
    }
}