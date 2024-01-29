using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Serialization;
using TestApp.Csv;
using TestApp.json;
using TestApp.Properties;
using TestApp.Settings;
using TestApp.xml;

namespace TestApp.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<CardsRoot> xmlFiles;
        private List<CsvData> csvFiles;
        private FileSystemWatcher fileSystemWatcherXml;
        private FileSystemWatcher fileSystemWatcherCsv;
        private CsvData selectedCsv;
        private CardsRoot selectedXml;
        private CustomSettings settings;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public MainWindowViewModel()
        {
            xmlFiles = new List<CardsRoot>();
            csvFiles = new List<CsvData>();
            IsProcessButtonEnabled = false;
            
        }
        public RelayCommand ProcessCommand => new RelayCommand(async () => { await CreateJson(); });
        public void Initialize()
        {
            settings = ReadSettings();
            if (settings != null)
            {
                fileSystemWatcherXml = new FileSystemWatcher(settings.DirectoryPath);
                fileSystemWatcherXml.Filter = "*.xml";
                fileSystemWatcherXml.Changed += FileSystemWatcherXml_Changed; ;
                fileSystemWatcherXml.Error += FileSystemWatcher_Error;
                fileSystemWatcherXml.EnableRaisingEvents = true;

                fileSystemWatcherCsv = new FileSystemWatcher(settings.DirectoryPath);
                fileSystemWatcherCsv.Filter = "*.csv";
                fileSystemWatcherCsv.Changed += FileSystemWatcherCsv_Changed; ;
                fileSystemWatcherCsv.Error += FileSystemWatcher_Error;
                fileSystemWatcherCsv.EnableRaisingEvents = true;

                FirstTimeCollectFiles();
            }
        }

        DateTime lastReadXml = DateTime.MinValue;
        DateTime lastReadCsv = DateTime.MinValue;//обманка с множественными событиями
        private void FileSystemWatcherCsv_Changed(object sender, FileSystemEventArgs e)
        {
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime != lastReadCsv)
            {
                ParseCsv(e.FullPath);
                lastReadCsv = lastWriteTime;
            }
        }

        private void FileSystemWatcherXml_Changed(object sender, FileSystemEventArgs e)
        {
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime != lastReadXml)
            {
                ParseXml(e.FullPath);
                lastReadXml = lastWriteTime;
            }
        }

        private bool isProcessButtonEnabled;
        public bool IsProcessButtonEnabled
        {
            get => isProcessButtonEnabled;
            set
            {
                isProcessButtonEnabled = value;
                OnPropertyChanged(nameof(IsProcessButtonEnabled));
            }
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        private void FirstTimeCollectFiles()
        {
            var dirInfo = new DirectoryInfo(settings.DirectoryPath);
            var files = dirInfo.GetFiles("*.csv");
            foreach (var file in files)
            {
                ParseCsv(file.FullName);
            }
            files = dirInfo.GetFiles("*.xml");
            foreach (var file in files)
            {
                ParseXml(file.FullName);
            }
        }
        private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
           Message = e.GetException().Message;
        }

        private bool ProcessData(CsvData csvData)
        {
            foreach (var xml in xmlFiles)
            {
                var intersection = xml.Cards.Select(c => c.UserId).Intersect(csvData.Rows.Select(r => r.UserId));
                if (intersection.Any())
                {
                    Message = $"Сопоставление найдено. Будет сформировано {intersection.Count()} записей";
                    selectedCsv = csvData;
                    selectedXml = xml;
                    IsProcessButtonEnabled = true;
                    return true;
                }
            }
            return false;
        }
        private bool ProcessData(CardsRoot xmlData)
        {
            foreach (var csv in csvFiles)
            {
                var intersection = csv.Rows.Select(r => r.UserId).Intersect(xmlData.Cards.Select(c => c.UserId));
                if (intersection.Any())
                {
                    Message = $"Сопоставление найдено. Будет сформировано {intersection.Count()} записей";
                    selectedCsv = csv;
                    selectedXml = xmlData;
                    IsProcessButtonEnabled = true;
                    return true;
                }
            }
            return false;
        }
        private void ParseXml(string path)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            XmlSerializer serializer = new XmlSerializer(typeof(CardsRoot));
            CardsRoot cards = null;
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    try
                    {
                        cards = (CardsRoot)serializer.Deserialize(reader);
                    }
                    catch (Exception e)
                    {
                        Message = $"Файл xml не валиден. {e.Message}";
                        return;
                    }
                    if (!ProcessData(cards))
                    {
                        xmlFiles.Add(cards);
                    }
                }
            }
        }
        private void ParseCsv(string path)
        {
            var csv = string.Empty;
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    csv = reader.ReadToEnd();
                }
            }
            var lines = csv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            lines.RemoveAt(0);
            var csvData = new CsvData();
            foreach (var line in lines)
            {
                var csvRow = new CsvRow();
                var values = line.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                csvRow.UserId = int.Parse(values[0]);
                csvRow.FirstName = values[1];
                csvRow.LastName = values[2];
                csvRow.Number = values[3];
                csvData.Rows.Add(csvRow);
            }
            //
            if (!ProcessData(csvData))
            {
                csvFiles.Add(csvData);
            }
        }
        private CustomSettings ReadSettings()
        {
            
            if(!File.Exists("Settings.json"))
            {
                Message = "Settings.json not found";
                return null;
            }

            var settingsString = string.Empty;
            using (var stream = File.OpenRead("Settings.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    settingsString = reader.ReadToEnd();
                }
            }
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings.CustomSettings>(settingsString);
            }
            catch (Exception)
            {
                Message = "Settings.json is not valid";
                return null;
            }
        }

        private Task CreateJson()
        {
            isProcessButtonEnabled = false;
            var intersection = selectedXml.Cards.Select(c => c.UserId).Intersect(selectedCsv.Rows.Select(r => r.UserId));
            var result = new Root();
            foreach (var item in intersection)
            {
                var card = selectedXml.Cards.FirstOrDefault(c => c.UserId == item);
                var csvRow = selectedCsv.Rows.FirstOrDefault(r => r.UserId == item);
                result.Records.Add(new Record
                {
                    FirstName = csvRow.FirstName,
                    LastName = csvRow.LastName,
                    Phone = csvRow.Number,
                    UserId = csvRow.UserId,
                    Pan = card.Pan,
                    ExpDate = card.ExpDate
                });
            }
            
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            using (var stream = File.OpenWrite(System.IO.Path.Combine(settings.DirectoryPath,"result.json")))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(jsonString);
                }
            }
            Message = "Файл result.json успешно создан";
            selectedCsv = null;
            selectedXml = null;
            return Task.CompletedTask;
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

