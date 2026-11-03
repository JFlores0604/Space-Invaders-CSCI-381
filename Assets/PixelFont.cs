using System.Collections.Generic;
using UnityEngine;

// A tiny retro 5x7 pixel font drawn with OnGUI rectangles, so no font asset is needed.
public static class PixelFont
{
    public enum Align { Left, Center, Right }

    const int GlyphW = 5, GlyphH = 7, Spacing = 1, LineGap = 3;

    static readonly Dictionary<char, string[]> glyphs = new Dictionary<char, string[]>
    {
        {'A', new[]{" ### ","#   #","#   #","#####","#   #","#   #","#   #"}},
        {'B', new[]{"#### ","#   #","#   #","#### ","#   #","#   #","#### "}},
        {'C', new[]{" ### ","#   #","#    ","#    ","#    ","#   #"," ### "}},
        {'D', new[]{"#### ","#   #","#   #","#   #","#   #","#   #","#### "}},
        {'E', new[]{"#####","#    ","#    ","#### ","#    ","#    ","#####"}},
        {'F', new[]{"#####","#    ","#    ","#### ","#    ","#    ","#    "}},
        {'G', new[]{" ### ","#   #","#    ","# ###","#   #","#   #"," ####"}},
        {'H', new[]{"#   #","#   #","#   #","#####","#   #","#   #","#   #"}},
        {'I', new[]{" ### ","  #  ","  #  ","  #  ","  #  ","  #  "," ### "}},
        {'J', new[]{"  ###","   # ","   # ","   # ","   # ","#  # "," ##  "}},
        {'K', new[]{"#   #","#  # ","# #  ","##   ","# #  ","#  # ","#   #"}},
        {'L', new[]{"#    ","#    ","#    ","#    ","#    ","#    ","#####"}},
        {'M', new[]{"#   #","## ##","# # #","# # #","#   #","#   #","#   #"}},
        {'N', new[]{"#   #","#   #","##  #","# # #","#  ##","#   #","#   #"}},
        {'O', new[]{" ### ","#   #","#   #","#   #","#   #","#   #"," ### "}},
        {'P', new[]{"#### ","#   #","#   #","#### ","#    ","#    ","#    "}},
        {'Q', new[]{" ### ","#   #","#   #","#   #","# # #","#  # "," ## #"}},
        {'R', new[]{"#### ","#   #","#   #","#### ","# #  ","#  # ","#   #"}},
        {'S', new[]{" ####","#    ","#    "," ### ","    #","    #","#### "}},
        {'T', new[]{"#####","  #  ","  #  ","  #  ","  #  ","  #  ","  #  "}},
        {'U', new[]{"#   #","#   #","#   #","#   #","#   #","#   #"," ### "}},
        {'V', new[]{"#   #","#   #","#   #","#   #","#   #"," # # ","  #  "}},
        {'W', new[]{"#   #","#   #","#   #","# # #","# # #","# # #"," # # "}},
        {'X', new[]{"#   #","#   #"," # # ","  #  "," # # ","#   #","#   #"}},
        {'Y', new[]{"#   #","#   #"," # # ","  #  ","  #  ","  #  ","  #  "}},
        {'Z', new[]{"#####","    #","   # ","  #  "," #   ","#    ","#####"}},
        {'0', new[]{" ### ","#   #","#  ##","# # #","##  #","#   #"," ### "}},
        {'1', new[]{"  #  "," ##  ","  #  ","  #  ","  #  ","  #  "," ### "}},
        {'2', new[]{" ### ","#   #","    #","   # ","  #  "," #   ","#####"}},
        {'3', new[]{"#####","   # ","  #  ","   # ","    #","#   #"," ### "}},
        {'4', new[]{"   # ","  ## "," # # ","#  # ","#####","   # ","   # "}},
        {'5', new[]{"#####","#    ","#### ","    #","    #","#   #"," ### "}},
        {'6', new[]{"  ## "," #   ","#    ","#### ","#   #","#   #"," ### "}},
        {'7', new[]{"#####","    #","   # ","  #  "," #   "," #   "," #   "}},
        {'8', new[]{" ### ","#   #","#   #"," ### ","#   #","#   #"," ### "}},
        {'9', new[]{" ### ","#   #","#   #"," ####","    #","   # "," ##  "}},
        {'!', new[]{"  #  ","  #  ","  #  ","  #  ","  #  ","     ","  #  "}},
        {'.', new[]{"     ","     ","     ","     ","     ","     ","  #  "}},
        {',', new[]{"     ","     ","     ","     ","     ","  #  "," #   "}},
        {':', new[]{"     ","  #  ","     ","     ","     ","  #  ","     "}},
        {'-', new[]{"     ","     ","     ","#####","     ","     ","     "}},
        {'/', new[]{"    #","    #","   # ","  #  "," #   ","#    ","#    "}},
    };

    static readonly string[] heart =
    {
        " ## ## ",
        "#######",
        "#######",
        "#######",
        " ##### ",
        "  ###  ",
        "   #   ",
    };

    public const int HeartWidth = 7;

    // Draws a pixel-art heart with its top-left corner at (x, y).
    public static void DrawHeart(float x, float y, float px, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        for (int r = 0; r < heart.Length; r++)
        {
            for (int c = 0; c < heart[r].Length; c++)
            {
                if (heart[r][c] == '#')
                {
                    GUI.DrawTexture(new Rect(x + c * px, y + r * px, px, px), Texture2D.whiteTexture);
                }
            }
        }
        GUI.color = oldColor;
    }

    // Draws text inside `area`, starting at its top. `pixelSize` is shrunk if needed so the widest line fits.
    public static void Draw(string text, Rect area, float pixelSize, Color color, Align align = Align.Center)
    {
        string[] lines = text.ToUpperInvariant().Split('\n');
        int widest = 0;
        foreach (string line in lines) widest = Mathf.Max(widest, line.Length);
        float maxFit = area.width / (widest * (GlyphW + Spacing));
        float px = Mathf.Max(1f, Mathf.Floor(Mathf.Min(pixelSize, maxFit)));   // whole pixels keep it crisp

        Color oldColor = GUI.color;
        GUI.color = color;
        float y = area.y;
        foreach (string line in lines)
        {
            float lineWidth = (line.Length * (GlyphW + Spacing) - Spacing) * px;
            float x = align == Align.Left ? area.x
                    : align == Align.Right ? area.xMax - lineWidth
                    : area.x + (area.width - lineWidth) / 2f;
            x = Mathf.Round(x);

            foreach (char c in line)
            {
                if (glyphs.TryGetValue(c, out string[] rows))
                {
                    for (int r = 0; r < GlyphH; r++)
                    {
                        for (int col = 0; col < GlyphW; col++)
                        {
                            if (rows[r][col] == '#')
                            {
                                GUI.DrawTexture(new Rect(x + col * px, y + r * px, px, px), Texture2D.whiteTexture);
                            }
                        }
                    }
                }
                x += (GlyphW + Spacing) * px;
            }
            y += (GlyphH + LineGap) * px;
        }
        GUI.color = oldColor;
    }
}
