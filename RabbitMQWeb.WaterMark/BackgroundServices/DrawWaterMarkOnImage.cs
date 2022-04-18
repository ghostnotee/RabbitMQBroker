using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace RabbitMQWeb.WaterMark.BackgroundServices;

public static class DrawWaterMarkOnImage
{
    public static IImageProcessingContext ApplyScalingWaterMark(this IImageProcessingContext processingContext,
        Font font, string text, Color color, float padding)
    {
        Size imgSize = processingContext.GetCurrentSize();
        float targetWidth = imgSize.Width - (padding * 2);
        float targetHeight = imgSize.Height - (padding * 2);

        float targetMinHeight = imgSize.Height - (padding * 3); // must be with in a margin width of the target heig.ht

        // now we are working i 2 dimensions at once and can't just scale because it will cause the text to
        // reflow we need to just try multiple times

        var scaledFont = font;
        FontRectangle s = new FontRectangle(0, 0, float.MaxValue, float.MaxValue);

        float scaleFactor = (scaledFont.Size / 2); // every time we change direction we half this size
        int trapCount = (int) scaledFont.Size * 2;
        if (trapCount < 10)
        {
            trapCount = 10;
        }

        bool isTooSmall = false;

        while ((s.Height > targetHeight || s.Height < targetMinHeight) && trapCount > 0)
        {
            if (s.Height > targetHeight)
            {
                if (isTooSmall)
                {
                    scaleFactor = scaleFactor / 2;
                }

                scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                isTooSmall = false;
            }

            if (s.Height < targetMinHeight)
            {
                if (!isTooSmall)
                {
                    scaleFactor = scaleFactor / 2;
                }

                scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                isTooSmall = true;
            }

            trapCount--;

            s = TextMeasurer.Measure(text, new TextOptions(scaledFont));

        }

        //var center = new PointF(padding, imgSize.Height / 2);
        var bottomRight = new PointF(imgSize.Width - s.Width, imgSize.Height - s.Height);
        
        return processingContext.DrawText(text, scaledFont, color, bottomRight);
    }
}