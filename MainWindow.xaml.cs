using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace BoodschappenlijstApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, Dictionary<string, ProductInfo>> productPrijzen;
        private bool heeftKlantenkaart = false;

        // Gebruik exact het opgegeven pad
        private readonly string jsonDataPath = @"C:\Users\ibenb\Documents\ICT\tweede semester 2025\project\project 4\boodschappenlijst\bin\Debug\net8.0-windows\productDataaa.json";

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Initialiseer de dictionary
                productPrijzen = new Dictionary<string, Dictionary<string, ProductInfo>>();

                // Laad bestaande data als die er is
                LaadProductData();

                // Selecteer de eerste winkel standaard
                if (WinkelComboBox.Items.Count > 0)
                    WinkelComboBox.SelectedIndex = 0;

                if (NieuweWinkelComboBox.Items.Count > 0)
                    NieuweWinkelComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het initialiseren: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaadProductData()
        {
            try
            {
                if (File.Exists(jsonDataPath))
                {
                    string jsonData = File.ReadAllText(jsonDataPath);
                    var geladen = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ProductInfo>>>(jsonData);

                    if (geladen != null)
                    {
                        productPrijzen = geladen;
                        StatusTextBlock.Text = "Productgegevens succesvol geladen.";
                    }
                    else
                    {
                        StatusTextBlock.Text = "Probleem bij het laden van productgegevens. Productdata is leeg.";
                    }
                }
                else
                {
                    StatusTextBlock.Text = "Het bestand 'productData.json' is niet gevonden.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van productgegevens: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WinkelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToonProductenButton_Click(sender, e);
        }

        // Event handler voor klantenkaart checkbox
        private void KlantenkaartCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            heeftKlantenkaart = KlantenkaartCheckBox.IsChecked ?? false;

            // Update de weergave indien nodig
            if (ProductenListView.ItemsSource != null)
            {
                ToonProductenButton_Click(sender, e);
            }
        }

        private void ToonProductenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WinkelComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Selecteer eerst een winkel.", "Geen winkel geselecteerd", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string geselecteerdeWinkel = ((ComboBoxItem)WinkelComboBox.SelectedItem).Content.ToString();

                if (productPrijzen == null || productPrijzen.Count == 0)
                {
                    StatusTextBlock.Text = "Geen productgegevens beschikbaar.";
                    return;
                }

                // Lijst voor goedkoopste producten
                List<ProductInfo> goedkoopsteProducten = new List<ProductInfo>();

                // Voor elk product, vergelijk prijzen tussen winkels
                foreach (var productEntry in productPrijzen)
                {
                    var winkelPrijzen = productEntry.Value;

                    // Controleer of dit product beschikbaar is in de geselecteerde winkel
                    if (!winkelPrijzen.ContainsKey(geselecteerdeWinkel))
                        continue;

                    // Haal het product uit de geselecteerde winkel
                    var productInWinkel = winkelPrijzen[geselecteerdeWinkel];

                    // Indien klantenkaart aangevinkt, gebruik PremiumProductInfo
                    if (heeftKlantenkaart)
                    {
                        // Maak een PremiumProductInfo-object met dezelfde eigenschappen
                        var premiumProduct = new PremiumProductInfo
                        {
                            Naam = productInWinkel.Naam,
                            Prijs = productInWinkel.Prijs,
                            Winkel = productInWinkel.Winkel,
                            LaatsteUpdate = productInWinkel.LaatsteUpdate,
                            IsOnlineOpgehaald = productInWinkel.IsOnlineOpgehaald,
                            KortingPercentage = productInWinkel.KortingPercentage,
                            IsInAanbieding = productInWinkel.IsInAanbieding,
                            HeeftKlantenkaart = true
                        };

                        productInWinkel = premiumProduct;
                    }

                    // Bereken de effectieve prijs na korting
                    decimal prijsInGeselecteerdeWinkel = productInWinkel.PrijsNaKorting;

                    // Vergelijk met prijzen in alle andere winkels
                    bool isGoedkoopst = true;

                    foreach (var winkel in winkelPrijzen)
                    {
                        if (winkel.Key == geselecteerdeWinkel)
                            continue;

                        // Product in andere winkel, pas ook klantenkaart toe indien nodig
                        var productInAndereWinkel = winkel.Value;
                        if (heeftKlantenkaart)
                        {
                            var premiumProduct = new PremiumProductInfo
                            {
                                Naam = productInAndereWinkel.Naam,
                                Prijs = productInAndereWinkel.Prijs,
                                Winkel = productInAndereWinkel.Winkel,
                                LaatsteUpdate = productInAndereWinkel.LaatsteUpdate,
                                IsOnlineOpgehaald = productInAndereWinkel.IsOnlineOpgehaald,
                                KortingPercentage = productInAndereWinkel.KortingPercentage,
                                IsInAanbieding = productInAndereWinkel.IsInAanbieding,
                                HeeftKlantenkaart = true
                            };

                            productInAndereWinkel = premiumProduct;
                        }

                        // Prijs in andere winkel
                        decimal prijsInAndereWinkel = productInAndereWinkel.PrijsNaKorting;

                        // Als het product ergens anders goedkoper is
                        if (prijsInAndereWinkel < prijsInGeselecteerdeWinkel)
                        {
                            isGoedkoopst = false;
                            break;
                        }
                    }

                    // Als het product het goedkoopst is in deze winkel
                    if (isGoedkoopst)
                    {
                        goedkoopsteProducten.Add(productInWinkel);
                    }
                }

                // Sorteer op prijs (van laag naar hoog)
                goedkoopsteProducten = goedkoopsteProducten.OrderBy(p => p.PrijsNaKorting).ToList();

                // Vernieuw de weergave
                ProductenListView.ItemsSource = goedkoopsteProducten;

                // Update de statusbalk
                string klantenkaartInfo = heeftKlantenkaart ? " (met 5% klantenkaartkorting)" : "";
                StatusTextBlock.Text = $"{goedkoopsteProducten.Count} producten zijn het goedkoopst bij {geselecteerdeWinkel}{klantenkaartInfo}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het zoeken naar goedkoopste producten: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VoegProductToeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Valideer invoer
                if (string.IsNullOrWhiteSpace(NieuwProductTextBox.Text))
                {
                    MessageBox.Show("Vul een productnaam in.", "Ontbrekende gegevens", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (NieuweWinkelComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Selecteer een winkel.", "Ontbrekende gegevens", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prijs valideren
                if (!decimal.TryParse(PrijsTextBox.Text.Replace(',', '.'), out decimal prijs) || prijs <= 0)
                {
                    MessageBox.Show("Voer een geldige prijs in (groter dan 0).", "Ongeldige prijs", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Korting valideren
                if (!decimal.TryParse(KortingTextBox.Text.Replace(',', '.'), out decimal korting) || korting < 0 || korting > 100)
                {
                    MessageBox.Show("Voer een geldige korting in (0-100%).", "Ongeldige korting", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Haal de invoergegevens op
                string productNaam = NieuwProductTextBox.Text.Trim().ToLower();
                string winkelNaam = ((ComboBoxItem)NieuweWinkelComboBox.SelectedItem).Content.ToString();
                bool isInAanbieding = IsInAanbiedingCheckBox.IsChecked ?? false;

                // Creëer nieuw ProductInfo object
                ProductInfo nieuwProduct = new ProductInfo
                {
                    Naam = char.ToUpper(productNaam[0]) + productNaam.Substring(1), // Eerste letter hoofdletter
                    Prijs = prijs,
                    Winkel = winkelNaam,
                    LaatsteUpdate = DateTime.Now,
                    IsOnlineOpgehaald = false,
                    KortingPercentage = korting,
                    IsInAanbieding = isInAanbieding
                };

                // Controleer of productPrijzen geïnitialiseerd is
                if (productPrijzen == null)
                {
                    productPrijzen = new Dictionary<string, Dictionary<string, ProductInfo>>();
                }

                // Voeg toe aan de productPrijzen dictionary
                if (!productPrijzen.ContainsKey(productNaam))
                {
                    // Nieuw product, maak nieuwe dictionary voor alle winkels
                    productPrijzen[productNaam] = new Dictionary<string, ProductInfo>();
                }

                // Voeg toe of update voor deze winkel
                productPrijzen[productNaam][winkelNaam] = nieuwProduct;

                // Sla het bijgewerkte bestand op
                SlaProductDataOp();

                // Geef feedback
                StatusTextBlock.Text = $"Product '{nieuwProduct.Naam}' toegevoegd voor {winkelNaam}.";

                // Reset formulier
                NieuwProductTextBox.Clear();
                PrijsTextBox.Clear();
                KortingTextBox.Text = "0";
                IsInAanbiedingCheckBox.IsChecked = false;

                // Ververs de weergave als de huidige geselecteerde winkel is aangepast
                string huidigeWinkel = ((ComboBoxItem)WinkelComboBox.SelectedItem)?.Content.ToString();
                if (huidigeWinkel == winkelNaam)
                {
                    ToonProductenButton_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het toevoegen van product: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SlaProductDataOp()
        {
            try
            {
                // Zorg ervoor dat de map bestaat
                string directory = Path.GetDirectoryName(jsonDataPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Zorg dat we toegangsrechten hebben
                FileInfo fileInfo = new FileInfo(jsonDataPath);
                if (fileInfo.Exists)
                {
                    // Probeer de lees- en schrijfrechten te wijzigen
                    try
                    {
                        File.SetAttributes(jsonDataPath, FileAttributes.Normal);
                    }
                    catch
                    {
                        // Ignoreer fouten bij het wijzigen van attributen
                    }
                }

                // Serialiseer en sla op
                string jsonData = JsonConvert.SerializeObject(productPrijzen, Formatting.Indented);

                // Gebruik de sterkste manier om het bestand te schrijven
                using (FileStream fs = new FileStream(jsonDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    writer.Write(jsonData);
                    writer.Flush();
                    fs.Flush(true);
                }

                // Verifieer dat het bestand inderdaad is geschreven
                if (File.Exists(jsonDataPath))
                {
                    string content = File.ReadAllText(jsonDataPath);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        // Bestand bestaat en heeft inhoud, dus we zijn geslaagd
                        StatusTextBlock.Text = "Data succesvol opgeslagen in " + jsonDataPath;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Geen toegangsrechten om naar '{jsonDataPath}' te schrijven. Probeer het programma als Administrator uit te voeren.",
                               "Toegang geweigerd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan van productgegevens: {ex.Message}\n\nPad: {jsonDataPath}",
                               "Opslaan mislukt", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}