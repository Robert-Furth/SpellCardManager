using System.Windows.Media;

namespace SpellCardManager.WPFApp;

public static class Extensions {
    public static (double H, double S, double V) GetHSV(this Color color) {
        // https://en.wikipedia.org/wiki/HSL_and_HSV#From_RGB
        double rf = color.R / 255.0;
        double gf = color.G / 255.0;
        double bf = color.B / 255.0;

        double value = Math.Max(rf, Math.Max(gf, bf));
        double xmin = Math.Min(rf, Math.Min(gf, bf));
        double chroma = value - xmin;

        double saturation = value == 0 ? 0 : chroma / value;

        double hue = 0;
        if (chroma == 0) {
            hue = 0;
        } else if (value == rf) {
            hue = ((gf - bf) / chroma) % 6;
        } else if (value == gf) {
            hue = ((bf - rf) / chroma) + 2;
        } else if (value == bf) {
            hue = ((rf - gf) / chroma) + 4;
        }

        hue /= 6;

        return (hue, saturation, value);
    }

    public static Color WithHue(this Color color, double h) {
        var (_, s, v) = color.GetHSV();
        return ColorFromHSV(h, s, v);
    }

    public static Color WithSaturation(this Color color, double s) {
        var (h, _, v) = color.GetHSV();
        return ColorFromHSV(h, s, v);
    }

    public static Color WithValue(this Color color, double v) {
        var (h, s, _) = color.GetHSV();
        return ColorFromHSV(h, s, v);
    }

    public static Color ColorFromHSV(double h, double s, double v) {
        h = Math.Clamp(h, 0, 1);
        s = Math.Clamp(s, 0, 1);
        v = Math.Clamp(v, 0, 1);

        if (h == 1) h = 0;

        // https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
        double huePrime = h * 6;
        double chroma = v * s;
        double x = chroma * (1 - Math.Abs(huePrime % 2 - 1));

        (double rf, double gf, double bf) = huePrime switch {
            < 1 => (chroma, x, 0.0),
            < 2 => (x, chroma, 0.0),
            < 3 => (0.0, chroma, x),
            < 4 => (0.0, x, chroma),
            < 5 => (x, 0.0, chroma),
            < 6 => (chroma, 0.0, x),

            _ => throw new Exception(),
        };

        double m = v - chroma;
        rf += m;
        gf += m;
        bf += m;

        // Convert to bytes
        byte rb = (byte)(rf * 255);
        byte gb = (byte)(gf * 255);
        byte bb = (byte)(bf * 255);
        return Color.FromRgb(rb, gb, bb);
    }
}
