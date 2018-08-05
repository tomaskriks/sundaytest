using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace TaxConsumer
{
   

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IEnumerable<string> AvailableMunicipalities { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            RefreshMunicipalityList();
        }

        private void RefreshMunicipalityList()
        {
            try
            {
                var api = new ApiConsumer();
                AvailableMunicipalities = api.GetAllMunicipalities();
                TaxSearchMunicipalityComboBox.Items.Clear();
                TaxInsertMunicipalityComboBox.Items.Clear();
                foreach (var munip in AvailableMunicipalities)
                {
                    TaxSearchMunicipalityComboBox.Items.Add(munip);
                    TaxInsertMunicipalityComboBox.Items.Add(munip);
                }
                TaxSearchMunicipalityComboBox.Items.Refresh();
                TaxInsertMunicipalityComboBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get municipalities");
            }
        }

        private void GetTaxButton_OnClick(object sender, RoutedEventArgs e)
        {
            string municipality = TaxSearchMunicipalityComboBox.SelectedValue.ToString();
            if (string.IsNullOrWhiteSpace(municipality)) {
                MessageBox.Show("You need to enter a municipality name value");
                return;
            }

            DateTime date;
            if (TaxSearchDatePicker.SelectedDate != null)
            {
                date = TaxSearchDatePicker.SelectedDate.Value;
            }
            else
            {
                MessageBox.Show("You need to enter a date value");
                return;
            }

            try
            {
                var api = new ApiConsumer();
                var taxRate = api.GetTaxRate(municipality, date);
                TaxResult.Text = taxRate;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get tax rate with those parameters.");
            }

        }

        private void TaxInsertRateSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TaxInsertRate.Text = TaxInsertRateSlider.Value.ToString("F2");
        }

        private void InsertTaxButton_Click(object sender, RoutedEventArgs e)
        {
            

            string municipalityName = TaxInsertMunicipalityComboBox.SelectedValue.ToString();
            
            string taxType = ((ComboBoxItem) TaxInsertType.SelectedItem).Content.ToString();
            decimal rate = Math.Round(Convert.ToDecimal(TaxInsertRateSlider.Value), 2);
            if (TaxInsertDateFrom.SelectedDate == null || TaxInsertDateTo.SelectedDate == null)
            {
                MessageBox.Show("Select dates for this tax");
                return;
            }
            DateTime dateFrom = TaxInsertDateFrom.SelectedDate.Value;
            DateTime dateTo = TaxInsertDateTo.SelectedDate.Value;

            var api = new ApiConsumer();
            if (api.InsertNewTax(municipalityName, taxType, rate, dateFrom, dateTo))
            {
                MessageBox.Show("Successfully inserted");

            }
            else
            {
                MessageBox.Show("Insertion failed");
            }

        }

        private void MunicipalityImportFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new OpenFileDialog();
            browseDialog.Multiselect = false;
            browseDialog.AddExtension = true;
            browseDialog.DefaultExt = ".txt";
            browseDialog.Filter = "Text Files |*.txt";
            var result = browseDialog.ShowDialog();

            switch (result)
            {
                case true:
                    var filePath = browseDialog.FileName;
                    MunicipalityImportFilePath.Text = filePath;
                    break;
                case false:
                default:
                    break;

            }
        }

        private void ImportMunicipalitiesByNameButton_Click(object sender, RoutedEventArgs e)
        {
            var municipalityList = MunicipalityImportByNameTextBox.Text.Split(Environment.NewLine.ToCharArray()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

            if (!municipalityList.Any())
            {
                MessageBox.Show("You need to enter municipality names separated by new lines to the text field");
                return;
            }

            var api = new ApiConsumer();
            var result = api.ImportMunicipalities(municipalityList);

            MessageBox.Show(result ? "Successfully imported" : "Import failed");
        }

        private void ImportMunicipalitiesFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            var municipalityFilePath = MunicipalityImportFilePath.Text;
            if (string.IsNullOrWhiteSpace(municipalityFilePath) || !File.Exists(municipalityFilePath) || Path.GetExtension(municipalityFilePath) != ".txt")
            {
                MessageBox.Show("You need to select or enter a path to a .txt file");
                return;
            }

            var api = new ApiConsumer();
            MessageBox.Show(api.ImportMunicipalities(municipalityFilePath) ? "Successfully imported" : "Import failed");
        
        }

        private void RefreshMunicipalityListButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshMunicipalityList();
        }
    }
}
