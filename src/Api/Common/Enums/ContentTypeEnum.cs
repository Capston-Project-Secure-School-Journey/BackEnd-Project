using System.ComponentModel;

namespace Api.Common.Enums;
public enum ContentTypeEnum
{
    [Description("image/jpeg")] ImageJpeg,
    [Description("image/jpg")] ImageJpg,
    [Description("image/png")] ImagePng,
    [Description("image/gif")] ImageGif,
    [Description("image/bmp")] ImageBmp,
    [Description("image/tiff")] ImageTiff,
    [Description("image/webp")] ImageWebp,
    [Description("image/svg+xml")] ImageSvgXml,

    [Description("application/msword")] ApplicationMsWord,
    [Description("application/vnd.openxmlformats-officedocument.wordprocessingml.document")] ApplicationOpenXmlWord,
    [Description("application/pdf")] ApplicationPdf,

    [Description("application/vnd.ms-excel")] ApplicationMsExcel,
    [Description("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")] ApplicationOpenXmlExcel,

    [Description("application/vnd.ms-powerpoint")] ApplicationMsPowerPoint,
    [Description("application/vnd.openxmlformats-officedocument.presentationml.presentation")] ApplicationOpenXmlPowerPoint,

    [Description("text/plain")] TextPlain,
    [Description("text/csv")] TextCsv,
    [Description("text/html")] TextHtml,
    [Description("application/json")] ApplicationJson,
    [Description("application/xml")] ApplicationXml,

    [Description("application/zip")] ApplicationZip,
    [Description("application/x-7z-compressed")] Application7Z,
    [Description("application/x-rar-compressed")] ApplicationRar,
    [Description("application/gzip")] ApplicationGzip,

    [Description("audio/mpeg")] AudioMpeg,
    [Description("audio/wav")] AudioWav,
    [Description("audio/ogg")] AudioOgg,

    [Description("video/mp4")] VideoMp4,
    [Description("video/x-msvideo")] VideoAvi,
    [Description("video/x-matroska")] VideoMkv
}
