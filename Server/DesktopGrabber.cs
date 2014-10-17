/*
    WinRemoteControlServer.Console

    Copyright (C) 2014 Frederic Torres

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    associated documentation files (the "Software"), to deal in the Software without restriction, 
    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial 
    portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
/*
 * 
 * vScreen.net
 * https://github.com/fredericaltorres/vScreen.net
 * (C) Torres Frederic 2013
 * Release under MIT license
 * 
 * Some source code base on -> "Capturing the Screen Image in C#"
 * http://www.codeproject.com/Articles/3024/Capturing-the-Screen-Image-in-C 
 * By Agha Ali Raza, 27 Feb 2003
 * 
 * Some source code base on -> "Create a Remote Desktop Viewer using C# and WCF"
 * http://bobcravens.com/2009/04/create-a-remote-desktop-viewer-using-c-and-wcf/
 * By Bob Cravens
 * Creative Commonns Attribution 3.0 United State License
 * 
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DynamicSugar;
using Utils;

namespace vScreen.lib {

    public class DesktopGrabber {

        //private static int __errorCounter;

        /// <summary>
        /// Capture the screen by using the clipboard which is not good
        /// optimized screenshot  http://bobcravens.com/2009/04/create-a-remote-desktop-viewer-using-c-and-wcf/
        /// </summary>
        /// <param name="pngFileName"></param>
        /// <param name="desktopImageWidth"></param>
        /// <returns></returns>
        public static string Capture(string pngFileName, int desktopImageWidth) {  
            try {
                
                Bitmap b = CaptureScreen.CaptureScreen.GetDesktopImage();

                if(System.IO.File.Exists(pngFileName)) 
                    System.IO.File.Delete(pngFileName);

                var imagePercentResize = desktopImageWidth * 100.0 / b.Width;
                var size               = new Size() { Width = desktopImageWidth, Height = b.Height * (int)imagePercentResize / 100 };
                var b2                 = new Bitmap(b, size);

                b2.Save(pngFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                b.Dispose();
                b2.Dispose();
                return pngFileName;
            }
            catch(System.Exception ex)
            {
                Debug.WriteLine("DesktopGrabber.Capture() failed:{0} ", ex.ToString());
            }
            return null;
        }

        public static string SaveBitmapAsJpeg(Bitmap b, string fileName)
        {
            b.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            return fileName;
        }

        public static string SaveBitmapAsBmp(Bitmap b, string fileName)
        {
            b.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
            return fileName;
        }

        public static Bitmap CaptureDesktopAsBitmap(int desktopImageWidth) {  
            try {                
                Bitmap b               = CaptureScreen.CaptureScreen.GetDesktopImage();
                var imagePercentResize = desktopImageWidth * 100.0 / b.Width ;
                var size               = new Size() { Width = desktopImageWidth, Height = b.Height * (int)imagePercentResize / 100 };
                var b2                 = new Bitmap(b, size);                
                return b2;
            }
            catch(System.Exception ex)
            {
                Debug.WriteLine("DesktopGrabber.Capture() failed:{0} ", ex.ToString());
            }
            return null;
        }

        public class SmartCaptureInfo
        {
            public Bound Bound = new Bound();
            public string ImageFileName;
            public string OtherInfo;
            public Exception Exception;
            public bool ChangesDetected;

            public bool Succeeded
            {
                get
                {
                    return this.Exception == null;
                }
            }
        }
        
        private static Utils.SequencingFileName _previousSmartCaptureSeq = new SequencingFileName(Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "WinRemoteControl.Server.Console", "prev_desktop.{0}.bmp"));

        public static SmartCaptureInfo SmartCapture(string pngFileName, int desktopImageWidth, bool full, Bitmap bitmapToUse = null /* Used to debug this code */)
        {
            long imageSize;
            var r = new SmartCaptureInfo() { ChangesDetected = true };

            Bitmap newBitmap;
            if (bitmapToUse == null)
                newBitmap = CaptureDesktopAsBitmap(desktopImageWidth);                
            else
                newBitmap = bitmapToUse;

            if (_previousSmartCaptureSeq.Index == 0 || full)
            {
                SaveBitmapAsBmp(newBitmap, _previousSmartCaptureSeq.GetNextFilename());                
                r.ImageFileName       = SaveBitmapAsJpeg(newBitmap, pngFileName);
                imageSize             = new System.IO.FileInfo(r.ImageFileName).Length;
                r.OtherInfo           = "ImageSize:{0}".FormatString(imageSize);
                newBitmap.Dispose();
            }
            else
            {
                var otherInfo         = new StringBuilder();
                var prevImageFileName = _previousSmartCaptureSeq.GetPreviousFileName();
                
                // Save new image capture to the disk as it seems to make a difference rather than
                var newImageFileName  = SaveBitmapAsBmp(newBitmap, _previousSmartCaptureSeq.GetNextFilename()); // This will become the new last saved image
                newBitmap.Dispose();

                r.Bound.From(GetBoundingBoxForChanges_Fixed(prevImageFileName, newImageFileName));

                if (r.Bound.IsEmpty)
                {
                    r.ChangesDetected = false;    
                    return r;
                }
                
                var newImage       = Bitmap.FromFile(newImageFileName);

                var xpixelSaved    = newImage.Width - r.Bound.W;
                var ypixelSaved    = newImage.Height - r.Bound.H;
                double totalSaved  = xpixelSaved * ypixelSaved;
                double totalMax    = newImage.Width * newImage.Height;
                var savedInPercent = totalSaved / totalMax * 100.00;                

                var diff           = new Bitmap(r.Bound.W, r.Bound.H);
                var g              = Graphics.FromImage(diff);
                
                // Why the -1, -1 I am not sure but else we have a black line
                g.DrawImage(newImage, -1, -1, r.Bound.ToRectangle(), GraphicsUnit.Pixel);
                g.Dispose();
                
                r.ImageFileName       = SaveBitmapAsJpeg(diff, pngFileName);
                imageSize             = new FileInfo(r.ImageFileName).Length;
                newImage.Dispose();
                
                otherInfo.AppendFormat(" ImageSize:{0} x:{1}, y:{2}, pixelSaved%:{3}, pixelCount:{4}, pixelCountMax:{5}", imageSize, xpixelSaved, ypixelSaved, savedInPercent.ToString("0.00"), totalSaved, totalMax).AppendLine();
                
                r.OtherInfo           = otherInfo.ToString();

                Debug.WriteLine(r.OtherInfo);
            }
            _previousSmartCaptureSeq.CleanButKeepLastOne();
            return r;
        }

        public static Rectangle GetBoundingBoxForChanges_Fixed(string _prevBitmap, string _newBitmap)
        {
            var p = Bitmap.FromFile(_prevBitmap) as Bitmap;
            var n = Bitmap.FromFile(_newBitmap) as Bitmap;

            var r = GetBoundingBoxForChanges_Fixed(p, n);

            p.Dispose();
            n.Dispose();

            return r;

        }
        public static Rectangle GetBoundingBoxForChanges_Fixed(Bitmap _prevBitmap, Bitmap _newBitmap)
        {
            // The search algorithm starts by looking
            //    for the top and left bounds. The search
            //    starts in the upper-left corner and scans
            //    left to right and then top to bottom. It uses
            //    an adaptive approach on the pixels it
            //    searches. Another pass is looks for the
            //    lower and right bounds. The search starts
            //    in the lower-right corner and scans right
            //    to left and then bottom to top. Again, an
            //    adaptive approach on the search area is used.
            //
 
            // Note: The GetPixel member of the Bitmap class
            //    is too slow for this purpose. This is a good
            //    case of using unsafe code to access pointers
            //    to increase the speed.
            //
 
            // Validate the images are the same shape and type.
            //
            if (_prevBitmap.Width != _newBitmap.Width ||
                _prevBitmap.Height != _newBitmap.Height ||
                _prevBitmap.PixelFormat != _newBitmap.PixelFormat)
            {
                // Not the same shape...can't do the search.
                //
                return Rectangle.Empty;
            }
 
            // Init the search parameters.
            //
            int width = _newBitmap.Width;
            int height = _newBitmap.Height;
            int left = width;
            int right = 0;
            int top = height;
            int bottom = 0;
 
            BitmapData bmNewData = null;
            BitmapData bmPrevData = null;
            try
            {
                // Lock the bits into memory.
                //
                bmNewData  = _newBitmap.LockBits( new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height), ImageLockMode.ReadOnly, _newBitmap.PixelFormat);
                bmPrevData = _prevBitmap.LockBits( new Rectangle(0, 0, _prevBitmap.Width, _prevBitmap.Height), ImageLockMode.ReadOnly, _prevBitmap.PixelFormat);
 
                // The images are ARGB (4 bytes)
                //
                int numBytesPerPixel = 4;
 
                // Get the number of integers (4 bytes) in each row
                //    of the image.
                //
                int strideNew = bmNewData.Stride / numBytesPerPixel;
                int stridePrev = bmPrevData.Stride / numBytesPerPixel;
 
                // Get a pointer to the first pixel.
                //
                // Note: Another speed up implemented is that I don't
                //    need the ARGB elements. I am only trying to detect
                //    change. So this algorithm reads the 4 bytes as an
                //    integer and compares the two numbers.
                //
                System.IntPtr scanNew0 = bmNewData.Scan0;
                System.IntPtr scanPrev0 = bmPrevData.Scan0;
 
                // Enter the unsafe code.
                //
                unsafe
                {
                    // Cast the safe pointers into unsafe pointers.
                    //
                    int* pNew = (int*)(void*)scanNew0;
                    int* pPrev = (int*)(void*)scanPrev0;
 
                    // First Pass - Find the left and top bounds
                    //    of the minimum bounding rectangle. Adapt the
                    //    number of pixels scanned from left to right so
                    //    we only scan up to the current bound. We also
                    //    initialize the bottom & right. This helps optimize
                    //    the second pass.
                    //
                    // For all rows of pixels (top to bottom)
                    //
                    for (int y = 0; y < _newBitmap.Height; ++y)
                    {
                        // For pixels up to the current bound (left to right)
                        //
                        for (int x = 0; x < _newBitmap.Width/* FRED FIX */; ++x)
                        {
                            // Use pointer arithmetic to index the
                            //    next pixel in this row.
                            //
                            if ((pNew + x)[0] != (pPrev + x)[0])
                            {
                                // Found a change.
                                //
                                if (x < left)
                                {
                                    left = x;
                                }
                                if (x > right)
                                {
                                    right = x;
                                }
                                if (y < top)
                                {
                                    top = y;
                                }
                                if (y > bottom)
                                {
                                    bottom = y;
                                }
                            }
                        }
 
                        // Move the pointers to the next row.
                        //
                        pNew += strideNew;
                        pPrev += stridePrev;
                    }
 
                    // If we did not find any changed pixels
                    //    then no need to do a second pass.
                    //
                    if (left != width)
                    {
                        // Second Pass - The first pass found at
                        //    least one different pixel and has set
                        //    the left & top bounds. In addition, the
                        //    right & bottom bounds have been initialized.
                        //    Adapt the number of pixels scanned from right
                        //    to left so we only scan up to the current bound.
                        //    In addition, there is no need to scan past
                        //    the top bound.
                        //
 
                        // Set the pointers to the first element of the
                        //    bottom row.
                        //
                        pNew = (int*)(void*)scanNew0;
                        pPrev = (int*)(void*)scanPrev0;
                        pNew += (_newBitmap.Height - 1) * strideNew;
                        pPrev += (_prevBitmap.Height - 1) * stridePrev;
 
                        // For each row (bottom to top)
                        //
                        for (int y = _newBitmap.Height - 1; y > top; y--)
                        {
                            // For each column (right to left)
                            //
                            for (int x = _newBitmap.Width - 1; x > 0/*FRED FIX*/; x--)
                            {
                                // Use pointer arithmetic to index the
                                //    next pixel in this row.
                                //
                                if ((pNew + x)[0] != (pPrev + x)[0])
                                {
                                    // Found a change.
                                    //
                                    if (x > right)
                                    {
                                        right = x;
                                    }
                                    if (y > bottom)
                                    {
                                        bottom = y;
                                    }
                                }
                            }
 
                            // Move up one row.
                            //
                            pNew -= strideNew;
                            pPrev -= stridePrev;
                        }
                    }
                }
            }
            catch 
            {
                
            }
            finally
            {
                // Unlock the bits of the image.
                //
                if (bmNewData != null)
                {
                    _newBitmap.UnlockBits(bmNewData);
                }
                if (bmPrevData != null)
                {
                    _prevBitmap.UnlockBits(bmPrevData);
                }
            }
 
            // Validate we found a bounding box. If not
            //    return an empty rectangle.
            //
            int diffImgWidth = right - left + 1;
            int diffImgHeight = bottom - top + 1;
            if (diffImgHeight < 0 || diffImgWidth < 0)
            {
                // Nothing changed
                return Rectangle.Empty;
            }
 
            // Return the bounding box.
            //
            return new Rectangle(left, top, diffImgWidth, diffImgHeight);
        }




        public static Rectangle GetBoundingBoxForChanges(Bitmap _prevBitmap, Bitmap _newBitmap)
        {
            // The search algorithm starts by looking
            //    for the top and left bounds. The search
            //    starts in the upper-left corner and scans
            //    left to right and then top to bottom. It uses
            //    an adaptive approach on the pixels it
            //    searches. Another pass is looks for the
            //    lower and right bounds. The search starts
            //    in the lower-right corner and scans right
            //    to left and then bottom to top. Again, an
            //    adaptive approach on the search area is used.
            //
 
            // Note: The GetPixel member of the Bitmap class
            //    is too slow for this purpose. This is a good
            //    case of using unsafe code to access pointers
            //    to increase the speed.
            //
 
            // Validate the images are the same shape and type.
            //
            if (_prevBitmap.Width != _newBitmap.Width ||
                _prevBitmap.Height != _newBitmap.Height ||
                _prevBitmap.PixelFormat != _newBitmap.PixelFormat)
            {
                // Not the same shape...can't do the search.
                //
                return Rectangle.Empty;
            }
 
            // Init the search parameters.
            //
            int width = _newBitmap.Width;
            int height = _newBitmap.Height;
            int left = width;
            int right = 0;
            int top = height;
            int bottom = 0;
 
            BitmapData bmNewData = null;
            BitmapData bmPrevData = null;
            try
            {
                // Lock the bits into memory.
                //
                bmNewData  = _newBitmap.LockBits( new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height), ImageLockMode.ReadOnly, _newBitmap.PixelFormat);
                bmPrevData = _prevBitmap.LockBits( new Rectangle(0, 0, _prevBitmap.Width, _prevBitmap.Height), ImageLockMode.ReadOnly, _prevBitmap.PixelFormat);
 
                // The images are ARGB (4 bytes)
                //
                int numBytesPerPixel = 4;
 
                // Get the number of integers (4 bytes) in each row
                //    of the image.
                //
                int strideNew = bmNewData.Stride / numBytesPerPixel;
                int stridePrev = bmPrevData.Stride / numBytesPerPixel;
 
                // Get a pointer to the first pixel.
                //
                // Note: Another speed up implemented is that I don't
                //    need the ARGB elements. I am only trying to detect
                //    change. So this algorithm reads the 4 bytes as an
                //    integer and compares the two numbers.
                //
                System.IntPtr scanNew0 = bmNewData.Scan0;
                System.IntPtr scanPrev0 = bmPrevData.Scan0;
 
                // Enter the unsafe code.
                //
                unsafe
                {
                    // Cast the safe pointers into unsafe pointers.
                    //
                    int* pNew = (int*)(void*)scanNew0;
                    int* pPrev = (int*)(void*)scanPrev0;
 
                    // First Pass - Find the left and top bounds
                    //    of the minimum bounding rectangle. Adapt the
                    //    number of pixels scanned from left to right so
                    //    we only scan up to the current bound. We also
                    //    initialize the bottom & right. This helps optimize
                    //    the second pass.
                    //
                    // For all rows of pixels (top to bottom)
                    //
                    for (int y = 0; y < _newBitmap.Height; ++y)
                    {
                        // For pixels up to the current bound (left to right)
                        //
                        for (int x = 0; x < left; ++x)
                        {
                            // Use pointer arithmetic to index the
                            //    next pixel in this row.
                            //
                            if ((pNew + x)[0] != (pPrev + x)[0])
                            {
                                // Found a change.
                                //
                                if (x < left)
                                {
                                    left = x;
                                }
                                if (x > right)
                                {
                                    right = x;
                                }
                                if (y < top)
                                {
                                    top = y;
                                }
                                if (y > bottom)
                                {
                                    bottom = y;
                                }
                            }
                        }
 
                        // Move the pointers to the next row.
                        //
                        pNew += strideNew;
                        pPrev += stridePrev;
                    }
 
                    // If we did not find any changed pixels
                    //    then no need to do a second pass.
                    //
                    if (left != width)
                    {
                        // Second Pass - The first pass found at
                        //    least one different pixel and has set
                        //    the left & top bounds. In addition, the
                        //    right & bottom bounds have been initialized.
                        //    Adapt the number of pixels scanned from right
                        //    to left so we only scan up to the current bound.
                        //    In addition, there is no need to scan past
                        //    the top bound.
                        //
 
                        // Set the pointers to the first element of the
                        //    bottom row.
                        //
                        pNew = (int*)(void*)scanNew0;
                        pPrev = (int*)(void*)scanPrev0;
                        pNew += (_newBitmap.Height - 1) * strideNew;
                        pPrev += (_prevBitmap.Height - 1) * stridePrev;
 
                        // For each row (bottom to top)
                        //
                        for (int y = _newBitmap.Height - 1; y > top; y--)
                        {
                            // For each column (right to left)
                            //
                            for (int x = _newBitmap.Width - 1; x > right; x--)
                            {
                                // Use pointer arithmetic to index the
                                //    next pixel in this row.
                                //
                                if ((pNew + x)[0] != (pPrev + x)[0])
                                {
                                    // Found a change.
                                    //
                                    if (x > right)
                                    {
                                        right = x;
                                    }
                                    if (y > bottom)
                                    {
                                        bottom = y;
                                    }
                                }
                            }
 
                            // Move up one row.
                            //
                            pNew -= strideNew;
                            pPrev -= stridePrev;
                        }
                    }
                }
            }
            catch 
            {
                
            }
            finally
            {
                // Unlock the bits of the image.
                //
                if (bmNewData != null)
                {
                    _newBitmap.UnlockBits(bmNewData);
                }
                if (bmPrevData != null)
                {
                    _prevBitmap.UnlockBits(bmPrevData);
                }
            }
 
            // Validate we found a bounding box. If not
            //    return an empty rectangle.
            //
            int diffImgWidth = right - left + 1;
            int diffImgHeight = bottom - top + 1;
            if (diffImgHeight < 0 || diffImgWidth < 0)
            {
                // Nothing changed
                return Rectangle.Empty;
            }
 
            // Return the bounding box.
            //
            return new Rectangle(left, top, diffImgWidth, diffImgHeight);
        }
    }
    
namespace CaptureScreen
{
	public class PlatformInvokeUSER32
	{
		public  const int SM_CXSCREEN=0;
		public  const int SM_CYSCREEN=1;
	
		[DllImport("user32.dll", EntryPoint="GetDesktopWindow")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll",EntryPoint="GetDC")]
		public static extern IntPtr GetDC(IntPtr ptr);

		[DllImport("user32.dll",EntryPoint="GetSystemMetrics")]
		public static extern int GetSystemMetrics(int abc);

		[DllImport("user32.dll",EntryPoint="GetWindowDC")]
		public static extern IntPtr GetWindowDC(Int32 ptr);

		[DllImport("user32.dll",EntryPoint="ReleaseDC")]
		public static extern IntPtr ReleaseDC(IntPtr hWnd,IntPtr hDc);

		public PlatformInvokeUSER32()
		{
		}
	}
	public struct SIZE
	{
		public int cx;
		public int cy;
	}
	public class PlatformInvokeGDI32
	{
		public  const int SRCCOPY = 13369376;
		[DllImport("gdi32.dll",EntryPoint="DeleteDC")]
		public static extern IntPtr DeleteDC(IntPtr hDc);

		[DllImport("gdi32.dll",EntryPoint="DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr hDc);

		[DllImport("gdi32.dll",EntryPoint="BitBlt")]
		public static extern bool BitBlt(IntPtr hdcDest,int xDest,int yDest,int wDest,int hDest,IntPtr hdcSource,int xSrc,int ySrc,int RasterOp);

		[DllImport ("gdi32.dll",EntryPoint="CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc,	int nWidth, int nHeight);

		[DllImport ("gdi32.dll",EntryPoint="CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport ("gdi32.dll",EntryPoint="SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hdc,IntPtr bmp);
	
		public PlatformInvokeGDI32()
		{
		}
	}
    
	public class CaptureScreen
	{
	    public static Bitmap GetDesktopImageNet()
	    {
            Bitmap desktopBMP = new Bitmap(
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
 
            Graphics g = Graphics.FromImage(desktopBMP);
 
            g.CopyFromScreen(0, 0, 0, 0,
               new Size(
               System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
               System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height));
            g.Dispose();

	        return desktopBMP;
	    }
        /// <summary>
        /// http://www.codeproject.com/Articles/546006/Screen-Capture-on-Multiple-Monitors
        /// http://www.codeproject.com/KB/cs/DesktopCaptureWithMouse.aspx?display=PrintAll
        /// </summary>
        /// <returns></returns>
		public static Bitmap GetDesktopImage()
		{
			SIZE size;
			IntPtr hBitmap;
			IntPtr 	hDC = PlatformInvokeUSER32.GetDC(PlatformInvokeUSER32.GetDesktopWindow()); 
			IntPtr hMemDC = PlatformInvokeGDI32.CreateCompatibleDC(hDC);
			size.cx = PlatformInvokeUSER32.GetSystemMetrics(PlatformInvokeUSER32.SM_CXSCREEN);
			size.cy = PlatformInvokeUSER32.GetSystemMetrics(PlatformInvokeUSER32.SM_CYSCREEN);
			hBitmap = PlatformInvokeGDI32.CreateCompatibleBitmap(hDC, size.cx, size.cy);

			if (hBitmap!=IntPtr.Zero)
			{
				IntPtr hOld = (IntPtr) PlatformInvokeGDI32.SelectObject(hMemDC, hBitmap);
				PlatformInvokeGDI32.BitBlt(hMemDC, 0, 0,size.cx,size.cy, hDC, 0, 0, PlatformInvokeGDI32.SRCCOPY);
				PlatformInvokeGDI32.SelectObject(hMemDC, hOld);
				PlatformInvokeGDI32.DeleteDC(hMemDC);
				PlatformInvokeUSER32.ReleaseDC(PlatformInvokeUSER32.GetDesktopWindow(), hDC);
				Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap); 
				PlatformInvokeGDI32.DeleteObject(hBitmap);
				return bmp;
			}
			return null;
		}
	}
}







}
