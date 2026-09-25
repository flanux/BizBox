namespace Bizbox.Helpers;

using Bizbox.Models;

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

        // ---- Coffee Shop ----
        if (n.Contains("espresso") || n.Contains("coffee")) return "\u2615";
        if (n.Contains("grinder")) return "\u2699\ufe0f";
        if (n.Contains("pos") || n.Contains("billing")) return "\ud83d\udcb3";
        if (n.Contains("fridge") || n.Contains("refriger") || n.Contains("walk-in")) return "\ud83e\uddca";

        // ---- Bakery ----
        if (n.Contains("oven") || n.Contains("deck")) return "\ud83d\udd25";
        if (n.Contains("mixer") || n.Contains("dough")) return "\ud83e\udd56";
        if (n.Contains("proof")) return "\ud83c\udf5e";
        if (n.Contains("shelv") || n.Contains("display")) return "\ud83c\udf6a";

        // ---- Restaurant ----
        if (n.Contains("range") || n.Contains("stove") || n.Contains("gas")) return "\ud83d\udd25";
        if (n.Contains("prep table") || n.Contains("prep")) return "\ud83d\udd2a";
        if (n.Contains("dining") || n.Contains("furniture") || n.Contains("table")) return "\ud83c\udf7d\ufe0f";

        // ---- Salon ----
        if (n.Contains("styling chair") || n.Contains("chair")) return "\ud83d\udc88";
        if (n.Contains("wash station") || n.Contains("wash")) return "\ud83d\udeb0";
        if (n.Contains("styling tool") || n.Contains("trim") || n.Contains("dryer")) return "\u2702\ufe0f";
        if (n.Contains("mirror")) return "\ud83e\ude9e";

        // ---- Tea stall / cloud kitchen / home baking ----
        if (n.Contains("kettle") || n.Contains("tea") || n.Contains("boiler")) return "\ud83c\udffa";
        if (n.Contains("tiffin") || n.Contains("delivery bag") || n.Contains("cloud")) return "\ud83c\udf71";
        if (n.Contains("induction") || n.Contains("burner")) return "\ud83d\udd25";
        if (n.Contains("cake") || n.Contains("piping") || n.Contains("frosting")) return "\ud83c\udf82";
        if (n.Contains("cart")) return "\ud83d\udecd\ufe0f";

        // ---- Freelance repair / computer-mobile ----
        if (n.Contains("screwdriver") || n.Contains("repair kit") || n.Contains("toolkit")) return "\ud83e\udea4";
        if (n.Contains("heat gun") || n.Contains("preheat") || n.Contains("reballing")) return "\ud83d\udd25";
        if (n.Contains("diagnostic") || n.Contains("multimeter")) return "\ud83d\udd0c";

        // ---- Print-on-demand / print shop / stationery ----
        if (n.Contains("sublimation") || n.Contains("heat press")) return "\ud83d\udda8\ufe0f";
        if (n.Contains("dtg") || n.Contains("dtf") || n.Contains("garment printer")) return "\ud83d\udc55";
        if (n.Contains("offset") || n.Contains("commercial printer") || n.Contains("large format")) return "\ud83d\udda8\ufe0f";
        if (n.Contains("binder") || n.Contains("laminat") || n.Contains("cutter") && n.Contains("paper")) return "\ud83d\udcd1";
        if (n.Contains("stationery") || n.Contains("notebook") || n.Contains("shelving")) return "\u270f\ufe0f";

        // ---- Tailoring ----
        if (n.Contains("sewing machine") || n.Contains("overlock")) return "\ud83e\udea1";
        if (n.Contains("iron") && n.Contains("press")) return "\ud83e\uddfa";
        if (n.Contains("mannequin") || n.Contains("dress form")) return "\ud83d\udc57";

        // ---- Coffee roastery ----
        if (n.Contains("roaster")) return "\ud83d\udd25";
        if (n.Contains("cupping") || n.Contains("green bean")) return "\u2615";

        // ---- 3D printing / electronics / soldering / laser / CNC ----
        if (n.Contains("3d printer") || n.Contains("fdm") || n.Contains("resin printer")) return "\ud83d\udda8\ufe0f";
        if (n.Contains("filament")) return "\ud83e\uddf5";
        if (n.Contains("solder") || n.Contains("rework station")) return "\ud83d\udd27";
        if (n.Contains("oscilloscope") || n.Contains("bench supply") || n.Contains("component")) return "\ud83d\udd0c";
        if (n.Contains("laser") && n.Contains("engrav")) return "\ud83d\udd25";
        if (n.Contains("cnc") || n.Contains("router") || n.Contains("spindle")) return "\ud83e\udee9";

        // ---- Home brewing / woodworking / leather / sewing ----
        if (n.Contains("fermenter") || n.Contains("brew") || n.Contains("keg")) return "\ud83c\udf7a";
        if (n.Contains("table saw") || n.Contains("planer") || n.Contains("chisel") || n.Contains("lathe")) return "\ud83e\udea8";
        if (n.Contains("leather") || n.Contains("stitch") || n.Contains("awl")) return "\ud83e\udd7e";

        // ---- Candle / soap / resin / stickers / vinyl / drone ----
        if (n.Contains("candle") || n.Contains("wax") || n.Contains("wick")) return "\ud83d\udd6f\ufe0f";
        if (n.Contains("soap") || n.Contains("mold") && n.Contains("silicone")) return "\ud83e\uddfc";
        if (n.Contains("resin") || n.Contains("epoxy") || n.Contains("uv lamp")) return "\ud83d\udca7";
        if (n.Contains("vinyl") || n.Contains("sticker") || n.Contains("plotter")) return "\ud83c\udff7\ufe0f";
        if (n.Contains("drone") || n.Contains("quadcopter") || n.Contains("fpv")) return "\ud83d\ude81";
        if (n.Contains("transmitter") || n.Contains("receiver") || n.Contains("rc ")) return "\ud83c\udfae";

        return "\ud83d\udce6"; // generic equipment box
    }

    // Prefers whatever icon was set on the category itself (BusinessType.IconOrImagePath,
    // seeded per-category) and only falls back to name-keyword guessing for the
    // original 4 categories, which never had one set.
    public static string GetBusinessIcon(BusinessType businessType) =>
        !string.IsNullOrWhiteSpace(businessType.IconOrImagePath)
            ? businessType.IconOrImagePath!
            : GetBusinessIcon(businessType.Name);

    public static string GetBusinessIcon(string businessTypeName)
    {
        var n = businessTypeName.ToLowerInvariant();
        if (n.Contains("coffee")) return "\u2615";
        if (n.Contains("bakery")) return "\ud83e\udd50";
        if (n.Contains("restaurant")) return "\ud83c\udf7d\ufe0f";
        if (n.Contains("salon")) return "\ud83d\udc87";
        return "\ud83c\udfea";
    }

    // Status pill CSS class + label for a product listing.
    public static (string CssClass, string Label) GetStatusPill(Product p)
    {
        if (p.SupersedesProductId != null) return ("pill-upgrade", "New Model");
        if (p.SellerType == ListingSellerType.Platform) return ("pill-new", "New");

        return p.Condition switch
        {
            ProductCondition.LikeNew => ("pill-likenew", "Like New"),
            ProductCondition.Good => ("pill-good", "Good"),
            ProductCondition.Fair => ("pill-fair", "Fair"),
            _ => ("pill-new", "New")
        };
    }
}
