using System.Drawing;
using System.Runtime.InteropServices;

namespace TuColmadoRD.Desktop;

internal static class BrandAssets
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Bitmap CreateLogoBitmap(int size)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        var stroke = Math.Max(2, size / 14);
        var square = Math.Max(10, (int)(size * 0.48f));
        var leftX = Math.Max(1, (int)(size * 0.18f));
        var topY = Math.Max(1, (int)(size * 0.12f));
        var offset = Math.Max(5, (int)(size * 0.2f));

        var blueRect = new Rectangle(leftX, topY, square, square);
        var redRect = new Rectangle(leftX + offset, topY + offset, square, square);

        using var bluePen = new Pen(Color.FromArgb(37, 99, 235), stroke)
        {
            LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
        };
        using var redPen = new Pen(Color.FromArgb(220, 38, 38), stroke)
        {
            LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
        };

        g.DrawRectangle(bluePen, blueRect);
        g.DrawRectangle(redPen, redRect);

        return bmp;
    }

    public static Icon CreateLogoIcon(int size)
    {
        using var bitmap = CreateLogoBitmap(size);
        var handle = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(handle).Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

}