namespace Bizbox.Models;

// Which shelf a BusinessType sits on in the homepage discovery flow.
// This is the only place "small business / big business / hobbyist" lives —
// it's a tag on the existing category model, not a parallel structure, so
// every category keeps using the same Products/Bundles/Admin CRUD already built.
public enum BusinessSegment
{
    SmallBusiness = 0,
    BigBusiness = 1,
    Hobbyist = 2
}
