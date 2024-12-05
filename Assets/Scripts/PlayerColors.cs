using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerColors
{
    public static Color RED { get; private set; } = new Color(1.0f, 0.0f, 0.0f);
    public static Color CRIMSON { get; private set; } = new Color(0.863f, 0.078f, 0.235f);
    public static Color DARK_RED { get; private set; } = new Color(0.545f, 0.0f, 0.0f);
    public static Color ORANGE_RED { get; private set; } = new Color(1.0f, 0.271f, 0.0f);
    public static Color DARK_ORANGE { get; private set; } = new Color(1.0f, 0.549f, 0.0f);
    public static Color GOLD { get; private set; } = new Color(1.0f, 0.843f, 0.0f);
    public static Color KHAKI { get; private set; } = new Color(0.941f, 0.902f, 0.549f);
    public static Color YELLOW { get; private set; } = new Color(1.0f, 1.0f, 0.0f);
    public static Color LAWN_GREEN { get; private set; } = new Color(0.486f, 0.988f, 0.0f);
    public static Color SPRING_GREEN { get; private set; } = new Color(1.0f, 0.271f, 0.0f);
    public static Color CYAN { get; private set; } = new Color(0.0f, 1.0f, 1.0f);
    public static Color DEEP_SKY_BLUE { get; private set; } = new Color(0.0f, 0.749f, 1.0f);
    public static Color MEDIUM_SLATE_BLUE { get; private set; } = new Color(0.482f, 0.408f, 0.933f);
    public static Color DARK_MAGENTA { get; private set; } = new Color(0.545f, 0.0f, 0.545f);
    public static Color DEEP_PINK { get; private set; } = new Color(1.0f, 0.078f, 0.576f);
    public static Color PINK { get; private set; } = new Color(1.0f, 0.752f, 0.796f);
    public static Color SANDY_BROWN { get; private set; } = new Color(0.957f, 0.643f, 0.376f);
    public static Color LIME { get; private set; } = new Color(0.0f, 1.0f, 0.0f);
    public static Color LIGHT_SKY_BLUE { get; private set; } = new Color(0.529f, 0.808f, 0.980f);
    public static Color BEIGE { get; private set; } = new Color(0.961f, 0.961f, 0.863f);
    public static Color WHITE { get; private set; } = new Color(1.0f, 1.0f, 1.0f);

    public static readonly Color[] PlayerColorsList = {
        RED, CRIMSON, DARK_RED, ORANGE_RED, DARK_ORANGE, GOLD, KHAKI, YELLOW, LAWN_GREEN, SPRING_GREEN, CYAN, DEEP_SKY_BLUE, MEDIUM_SLATE_BLUE, DARK_MAGENTA, DEEP_PINK, PINK, SANDY_BROWN, LIME, LIGHT_SKY_BLUE, BEIGE, WHITE
    };
}

