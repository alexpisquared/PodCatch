using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NAudioTrimmer.Logic
{
    public interface IBitmapHelper
    {
        BitmapSource B2bs(Bitmap source); // must be on UI thread!
    }
    public class BitmapHelper : IBitmapHelper
    {
        public BitmapSource B2bs(Bitmap source) // must be on UI thread!
        {
            IntPtr ptr = source.GetHbitmap();
            try { return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); } finally { DeleteObject(ptr); }
        }

        [DllImport("gdi32")] static extern int DeleteObject(IntPtr o);
    }
}
