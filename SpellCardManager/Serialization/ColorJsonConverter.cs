using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SpellCardManager.Backend.Serialization;

internal class ColorJsonConverter : JsonConverter<Color> {
    public override Color Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {

        var pattern = @"^#([0-9A-Fa-f]{3,8})$";
        var m = Regex.Match(reader.GetString()!, pattern);
        if (!m.Success) throw new JsonException();

        var colorStr = m.Groups[1].Value;

        string sr, sg, sb, sa = "ff";
        switch (colorStr.Length) {
            case 4:
                sa = string.Concat(colorStr[3], colorStr[3]);
                goto case 3;
            case 3:
                sr = string.Concat(colorStr[0], colorStr[0]);
                sg = string.Concat(colorStr[1], colorStr[1]);
                sb = string.Concat(colorStr[2], colorStr[2]);
                break;

            case 8:
                sa = colorStr[6..8];
                goto case 6;
            case 6:
                sr = colorStr[0..2];
                sg = colorStr[2..4];
                sb = colorStr[4..6];
                break;

            default:
                throw new JsonException();
        }

        int r = int.Parse(sr, NumberStyles.HexNumber);
        int g = int.Parse(sg, NumberStyles.HexNumber);
        int b = int.Parse(sb, NumberStyles.HexNumber);
        int a = int.Parse(sa, NumberStyles.HexNumber);
        return Color.FromArgb(a, r, g, b);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Color value,
        JsonSerializerOptions options) {

        var hex = value.A == 255
            ? Convert.ToHexString([value.R, value.G, value.B]).ToLower()
            : Convert.ToHexString([value.R, value.G, value.B, value.A]).ToLower();

        writer.WriteStringValue($"#{hex}");
    }
}


