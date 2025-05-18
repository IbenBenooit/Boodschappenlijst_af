using System;

public class ProductInfo
{
    public string Naam { get; set; } = string.Empty;
    public decimal Prijs { get; set; }
    public string Winkel { get; set; } = string.Empty;
    public DateTime LaatsteUpdate { get; set; } = DateTime.Now;
    public bool IsOnlineOpgehaald { get; set; } = false;
    public decimal KortingPercentage { get; set; } = 0;
    public bool IsInAanbieding { get; set; } = false;

    // Virtual property om overschrijving mogelijk te maken
    public virtual decimal PrijsNaKorting
    {
        get
        {
            if (IsInAanbieding && KortingPercentage > 0)
            {
                decimal korting = Prijs * (KortingPercentage / 100m);
                return Prijs - korting;
            }
            return Prijs;
        }
    }
}

// Afgeleide klasse die PrijsNaKorting overschrijft
public class PremiumProductInfo : ProductInfo
{
    public override decimal PrijsNaKorting
    {
        get
        {
            // Pas een extra 5% korting toe op de berekende prijs
            decimal basisPrijs = base.PrijsNaKorting;
            decimal extraKorting = basisPrijs * 0.05m;
            return basisPrijs - extraKorting;
        }
    }
}
