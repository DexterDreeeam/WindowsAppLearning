using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Management.Core;
using Windows.Management.Deployment;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WidgetsDataReaderPackaged
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string dashboardFamilyName = "MicrosoftWindows.Client.WebExperience_cw5n1h2txyewy";
        private const string wdpFamilyName = "Microsoft.WidgetsPlatformRuntime_8wekyb3d8bbwe";
        private string currentFamilyName = string.Empty;
        private string currentContainer = string.Empty;

        private readonly SynchronizationContext syncContext;
        private ObservableCollection<SettingItem> settingItems;
        private ObservableCollection<SettingItem> SettingItems
        {
            get => settingItems;
            set
            {
                settingItems = value;
                OnPropertyChanged(nameof(SettingItems));
            }
        }

        private string inputContainer;
        public string InputContainer
        {
            get => inputContainer;
            set
            {
                inputContainer = value;
                OnPropertyChanged(nameof(InputContainer));
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            this.currentFamilyName = dashboardFamilyName;
            this.currentContainer = string.Empty;
            this.SettingItems = new ObservableCollection<SettingItem>();
            this.syncContext = SynchronizationContext.Current;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnEdit(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null)
            {
                return;
            }
            var settingItem = (SettingItem)clickedButton.DataContext;
            EditTextBox.Text = settingItem.Value;
            var result = await EditDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                settingItem.Value = EditTextBox.Text;
                ModifySettingItem(settingItem);
                _ = RefreshAsync();
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null)
            {
                return;
            }
            var settingItem = (SettingItem)clickedButton.DataContext;
            DeleteSettingItem(settingItem);
            _ = RefreshAsync();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            _ = RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            await Task.Run(() =>
            {
                RefreshSettingItemsList();
            });
        }

        private List<string> GetPackages()
        {
            var mgr = new PackageManager();
            var pkgs = mgr.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);
            return pkgs.Select(p =>
            {
                return p.DisplayName;
            }).ToList();
        }

        private IPropertySet GetProperties()
        {
            var appData = ApplicationDataManager.CreateForPackageFamily(this.currentFamilyName);
            var localSettings = appData.LocalSettings;
            IPropertySet properties = null;
            if (string.IsNullOrEmpty(this.currentContainer))
            {
                properties = localSettings.Values;
            }
            else
            {
                var container = localSettings.CreateContainer(this.currentContainer, ApplicationDataCreateDisposition.Always);
                properties = container?.Values;
            }
            return properties;
        }

        private void RefreshSettingItemsList()
        {
            this.currentContainer = this.InputContainer;
            var items = new ObservableCollection<SettingItem>();
            try
            {
                IPropertySet properties = GetProperties();
                if (properties != null)
                {
                    foreach (var itr in properties)
                    {
                        if (itr.Value is string s)
                        {
                            items.Add(new SettingItem(itr.Key, s));
                        }
                        else
                        {
                            items.Add(new SettingItem(itr.Key, JsonSerializer.Serialize(itr.Value)));
                        }
                    }
                }

                syncContext.Post(_ =>
                {
                    this.SettingItems = items;
                }, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ModifySettingItem(SettingItem item)
        {
            this.currentContainer = this.InputContainer;
            try
            {
                IPropertySet properties = GetProperties();
                if (properties != null)
                {
                    properties[item.Name] = item.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void DeleteSettingItem(SettingItem item)
        {
            this.currentContainer = this.InputContainer;
            try
            {
                IPropertySet properties = GetProperties();
                if (properties != null)
                {
                    properties.Remove(item.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
