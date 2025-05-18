using System;

namespace BoodschappenlijstApp
{
    public class ProductInfo
    {
        public string Naam { get; set; } = string.Empty;
        public decimal Prijs { get; set; }
        public string Winkel { get; set; } = string.Empty;
        public DateTime LaatsteUpdate { get; set; } = DateTime.Now;
        public bool IsOnlineOpgehaald { get; set; } = false;
        public decimal KortingPercentage { get; set; } = 0;
        public bool IsInAanbieding { get; set; } = false;

        // Eenvoudige property voor prijs na korting
        public decimal PrijsNaKorting
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

    public class Product
    {
        public string Naam { get; set; } = string.Empty;
    }
}