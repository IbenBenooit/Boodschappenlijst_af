using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoodschappenlijstApp
{
    public class PrijsOphaler
    {
        private string jsonDataPath;
        public event EventHandler<double> VoortgangGewijzigd;

        public PrijsOphaler(string jsonDataPath)
        {
            this.jsonDataPath = jsonDataPath;
        }

        public async Task HaalPrijzenOp()
        {
            for (int i = 0; i <= 100; i += 10)
            {
                VoortgangGewijzigd?.Invoke(this, i);
                await Task.Delay(300);
            }

            try
            {
                // Laad data uit het JSON-bestand
                Dictionary<string, Dictionary<string, ProductInfo>> productData;

                if (File.Exists(jsonDataPath))
                {
                    string jsonData = File.ReadAllText(jsonDataPath);
                    productData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ProductInfo>>>(jsonData);

                    if (productData == null)
                    {
                        // Als deserialisatie mislukt, maak een lege dictionary
                        productData = new Dictionary<string, Dictionary<string, ProductInfo>>();
                    }

                    foreach (var productEntry in productData)
                    {
                        foreach (var winkelEntry in productEntry.Value)
                        {
                            winkelEntry.Value.LaatsteUpdate = DateTime.Now;
                            winkelEntry.Value.IsOnlineOpgehaald = true;
                        }
                    }

                    // opslaan bijgewerkte data
                    string updatedJsonData = JsonConvert.SerializeObject(productData, Formatting.Indented);
                    File.WriteAllText(jsonDataPath, updatedJsonData);
                }
                else
                {
                    throw new FileNotFoundException($"JSON-bestand '{jsonDataPath}' niet gevonden.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fout bij het ophalen van prijzen: {ex.Message}", ex);
            }
        }

        public async Task HaalPrijzenOpVoorProduct(string productNaam)
        {
            for (int i = 0; i <= 100; i += 20)
            {
                VoortgangGewijzigd?.Invoke(this, i);
                await Task.Delay(200);
            }

            try
            {
                Dictionary<string, Dictionary<string, ProductInfo>> productData;

                if (File.Exists(jsonDataPath))
                {
                    string jsonData = File.ReadAllText(jsonDataPath);
                    productData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ProductInfo>>>(jsonData);

                    if (productData == null)
                    {
                        productData = new Dictionary<string, Dictionary<string, ProductInfo>>();
                    }
                }
                else
                {
                    productData = new Dictionary<string, Dictionary<string, ProductInfo>>();
                }

                if (!productData.ContainsKey(productNaam))
                {
                    var nieuwProduct = new Dictionary<string, ProductInfo>();
                    var random = new Random();
                    string productNaamCapitalized = char.ToUpper(productNaam[0]) + productNaam.Substring(1);

                    nieuwProduct.Add("Albert Heijn", new ProductInfo
                    {
                        Naam = productNaamCapitalized,
                        Prijs = Math.Round((decimal)(random.NextDouble() * 3 + 1), 2),
                        Winkel = "Albert Heijn",
                        LaatsteUpdate = DateTime.Now,
                        IsOnlineOpgehaald = true,
                        IsInAanbieding = random.Next(0, 2) == 1,
                        KortingPercentage = random.Next(0, 2) == 1 ? random.Next(5, 31) : 0
                    });

                    nieuwProduct.Add("Jumbo", new ProductInfo
                    {
                        Naam = productNaamCapitalized,
                        Prijs = Math.Round((decimal)(random.NextDouble() * 3 + 1), 2),
                        Winkel = "Jumbo",
                        LaatsteUpdate = DateTime.Now,
                        IsOnlineOpgehaald = true,
                        IsInAanbieding = random.Next(0, 2) == 1,
                        KortingPercentage = random.Next(0, 2) == 1 ? random.Next(5, 31) : 0
                    });

                    nieuwProduct.Add("Lidl", new ProductInfo
                    {
                        Naam = productNaamCapitalized,
                        Prijs = Math.Round((decimal)(random.NextDouble() * 3 + 1), 2),
                        Winkel = "Lidl",
                        LaatsteUpdate = DateTime.Now,
                        IsOnlineOpgehaald = true,
                        IsInAanbieding = random.Next(0, 2) == 1,
                        KortingPercentage = random.Next(0, 2) == 1 ? random.Next(5, 31) : 0
                    });

                    nieuwProduct.Add("Aldi", new ProductInfo
                    {
                        Naam = productNaamCapitalized,
                        Prijs = Math.Round((decimal)(random.NextDouble() * 3 + 1), 2),
                        Winkel = "Aldi",
                        LaatsteUpdate = DateTime.Now,
                        IsOnlineOpgehaald = true,
                        IsInAanbieding = random.Next(0, 2) == 1,
                        KortingPercentage = random.Next(0, 2) == 1 ? random.Next(5, 31) : 0
                    });

                    productData.Add(productNaam, nieuwProduct);
                }

                // opslaan bijgewerkte data
                string updatedJsonData = JsonConvert.SerializeObject(productData, Formatting.Indented);
                File.WriteAllText(jsonDataPath, updatedJsonData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fout bij het ophalen van prijzen voor product '{productNaam}': {ex.Message}", ex);
            }
        }
    }
}