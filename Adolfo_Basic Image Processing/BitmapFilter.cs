using Adolfo_Basic_Image_Processing;
using System;
using System.Drawing;
using System.Drawing.Imaging;

public class BitmapFilter
{
    public static bool Conv3x3(Bitmap b, ConvMatrix m)
    {
        if (m.Factor == 0) return false;

        Bitmap bSrc = (Bitmap)b.Clone();

        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        int stride2 = stride * 2;

        IntPtr Scan0 = bmData.Scan0;
        IntPtr SrcScan0 = bmSrc.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;
            byte* pSrc = (byte*)(void*)SrcScan0;

            int nOffset = stride - b.Width * 3;
            int nWidth = b.Width - 2;
            int nHeight = b.Height - 2;

            int nPixel;

            for (int y = 0; y < nHeight; ++y)
            {
                for (int x = 0; x < nWidth; ++x)
                {
                    // Blue
                    nPixel = (((pSrc[2] * m.TopLeft) +
                        (pSrc[5] * m.TopMid) +
                        (pSrc[8] * m.TopRight) +
                        (pSrc[2 + stride] * m.MidLeft) +
                        (pSrc[5 + stride] * m.Pixel) +
                        (pSrc[8 + stride] * m.MidRight) +
                        (pSrc[2 + stride2] * m.BottomLeft) +
                        (pSrc[5 + stride2] * m.BottomMid) +
                        (pSrc[8 + stride2] * m.BottomRight))
                        / m.Factor) + m.Offset;

                    if (nPixel < 0) nPixel = 0;
                    if (nPixel > 255) nPixel = 255;
                    p[5 + stride] = (byte)nPixel;

                    // Green
                    nPixel = (((pSrc[1] * m.TopLeft) +
                        (pSrc[4] * m.TopMid) +
                        (pSrc[7] * m.TopRight) +
                        (pSrc[1 + stride] * m.MidLeft) +
                        (pSrc[4 + stride] * m.Pixel) +
                        (pSrc[7 + stride] * m.MidRight) +
                        (pSrc[1 + stride2] * m.BottomLeft) +
                        (pSrc[4 + stride2] * m.BottomMid) +
                        (pSrc[7 + stride2] * m.BottomRight))
                        / m.Factor) + m.Offset;

                    if (nPixel < 0) nPixel = 0;
                    if (nPixel > 255) nPixel = 255;
                    p[4 + stride] = (byte)nPixel;

                    // Red
                    nPixel = (((pSrc[0] * m.TopLeft) +
                        (pSrc[3] * m.TopMid) +
                        (pSrc[6] * m.TopRight) +
                        (pSrc[0 + stride] * m.MidLeft) +
                        (pSrc[3 + stride] * m.Pixel) +
                        (pSrc[6 + stride] * m.MidRight) +
                        (pSrc[0 + stride2] * m.BottomLeft) +
                        (pSrc[3 + stride2] * m.BottomMid) +
                        (pSrc[6 + stride2] * m.BottomRight))
                        / m.Factor) + m.Offset;

                    if (nPixel < 0) nPixel = 0;
                    if (nPixel > 255) nPixel = 255;
                    p[3 + stride] = (byte)nPixel;

                    p += 3;
                    pSrc += 3;
                }
                p += nOffset;
                pSrc += nOffset;
            }
        }

        b.UnlockBits(bmData);
        bSrc.UnlockBits(bmSrc);
        return true;
    }

    // 📌 Example Filters
    public static bool Smooth(Bitmap b, int weight = 1)
    {
        ConvMatrix m = new ConvMatrix();
        m.SetAll(1);
        m.Pixel = weight;
        m.Factor = weight + 8;
        return Conv3x3(b, m);
    }

    public static bool GaussianBlur(Bitmap b)
    {
        ConvMatrix m = new ConvMatrix();
        m.TopLeft = m.BottomRight = m.TopRight = m.BottomLeft = 1;
        m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 2;
        m.Pixel = 4;
        m.Factor = 16;
        return Conv3x3(b, m);
    }

    public static bool Sharpen(Bitmap b)
    {
        ConvMatrix m = new ConvMatrix();
        m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = -2;
        m.Pixel = 11;
        m.Factor = 3;
        return Conv3x3(b, m);
    }

    public static bool MeanRemoval(Bitmap b)
    {
        ConvMatrix m = new ConvMatrix();
        m.SetAll(-1);
        m.Pixel = 9;
        m.Factor = 1;
        return Conv3x3(b, m);
    }

    public static bool Emboss(Bitmap b)
    {
        ConvMatrix m = new ConvMatrix();
        m.TopLeft = m.TopRight = m.BottomLeft = m.BottomRight = -1;
        m.Pixel = 4;
        m.Offset = 127;
        return Conv3x3(b, m);
    }
}
