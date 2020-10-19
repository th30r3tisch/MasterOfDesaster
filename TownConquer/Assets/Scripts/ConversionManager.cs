using UnityEngine;

public class ConversionManager
{
    public static System.Numerics.Vector3 ToNumericVector(Vector3 v) {
        return new System.Numerics.Vector3(v.x, v.y, v.z);
    }

    public static System.Drawing.Color ToDrawingColor(Color c) {
        return System.Drawing.Color.FromArgb((int)c.a, (int)c.r, (int)c.g, (int)c.b);
    }

    public static Color32 DrawingToColor32(System.Drawing.Color c) {
        return new Color32(c.R, c.G, c.B, c.A);
    }
}
