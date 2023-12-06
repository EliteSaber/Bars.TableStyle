using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Bars.TableStyle.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string>? _errorMessages;
    public ViewModelBase()
    {
        ErrorMessages = new ObservableCollection<string>();
    }
}