namespace Bizbox.Helpers;

// Maps product/business names to a representative icon (emoji) and a soft
// background tint, so every catalog card has a consistent visual anchor
// without depending on external/hotlinked images that could break or be
// irrelevant. Swap this out for real product photography later by setting
// Product.ImagePath — views already prefer ImagePath when it's set.
public static class IconHelper
{
    public static string GetIcon(string name)
    {
        var n = name.ToLowerInvariant();

        if (n.Contains("espresso") || n.Contains("coffee")) return "\u2615";
        if (n.Contains("grinder")) return "\u2699\ufe0f";
        if (n.Contains("pos") || n.Contains("billing")) return "\ud83d\udcb3";
        if (n.Contains("fridge") || n.Contains("refriger") || n.Contains("walk-in")) return "\ud83e\uddca";
        if (n.Contains("oven") || n.Contains("deck")) return "\ud83d\udd25";
        if (n.Contains("mixer") || n.Contains("dough")) return "\ud83e\udd56";
        if (n.Contains("proof")) return "\ud83c\udf5e";
        if (n.Contains("shelv") || n.Contains("display")) return "\ud83c\udf6a";
        if (n.Contains("range") || n.Contains("stove") || n.Contains("gas")) return "\ud83d\udd25";
        if (n.Contains("prep table") || n.Contains("prep")) return "\ud83d\udd2a";
        if (n.Contains("dining") || n.Contains("furniture") || n.Contains("table")) return "\ud83c\udf7d\ufe0f";
        if (n.Contains("styling chair") || n.Contains("chair")) return "\ud83d\udc88";
        if (n.Contains("wash station") || n.Contains("wash")) return "\ud83d\udeb0";
        if (n.Contains("styling tool") || n.Contains("trim") || n.Contains("dryer")) return "\u2702\ufe0f";
        if (n.Contains("mirror")) return "\ud83e\ude9e";

        return "\ud83d\udce6"; // generic equipment box
    }

    public static string GetBusinessIcon(string businessTypeName)
    {
        var n = businessTypeName.ToLowerInvariant();
        if (n.Contains("coffee")) return "\u2615";
        if (n.Contains("bakery")) return "\ud83e\udd50";
        if (n.Contains("restaurant")) return "\ud83c\udf7d\ufe0f";
        if (n.Contains("salon")) return "\ud83d\udc87";
        return "\ud83c\udfea";
    }
}
