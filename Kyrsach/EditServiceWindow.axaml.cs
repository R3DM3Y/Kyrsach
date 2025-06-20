using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Kyrsach
{
    public partial class EditServiceWindow : Window
    {
        public ServicePresenter EditedService { get; }

        public EditServiceWindow()
        {
            InitializeComponent();
        }

        public EditServiceWindow(ServicePresenter service) : this()
        {
            EditedService = service;
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Простая валидация
            if (string.IsNullOrWhiteSpace(EditedService.Title))
            {
                return;
            }

            Close(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}