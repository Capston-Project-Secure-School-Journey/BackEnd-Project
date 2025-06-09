using ImageMagick;

namespace Api.Extensions;

public static class ImageConverter
{
    public static Stream ConvertHeic2Png(Stream streamFile)
    {
        using var image = new MagickImage(streamFile);
        image.Format = MagickFormat.Png;
        var outputStream = new MemoryStream();
        image.Write(outputStream);
        outputStream.Position = 0;
        return outputStream;
    }
}