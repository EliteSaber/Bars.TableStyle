using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bars.TableStyle.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bars.TableStyle.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _fileText;
    [ObservableProperty] private string? _fileTextOld;
    [ObservableProperty] private bool _isLoaded;

    [RelayCommand]
    private async Task OpenFileCurrent(CancellationToken token)
    {
        await OpenFile(token, "Загрузить файл ЭФ");
        IsLoaded = !string.IsNullOrWhiteSpace(FileText);
    }

    [RelayCommand]
    private async Task OpenFileOld(CancellationToken token)
    {
        (FileText, FileTextOld) = (FileTextOld, FileText);
        await OpenFile(token, "Загрузить файл для сравнения");
        (FileText, FileTextOld) = (FileTextOld, FileText);
    }
    private async Task OpenFile(CancellationToken token, string title = "Load")
    {
        ErrorMessages?.Clear();
        try
        {
            var filesService = (App.Current?.Services?.GetService<IFilesService>()) ?? throw new NullReferenceException("Missing file service instance.");
            filesService.OpenTitle = title;
            var file = await filesService.OpenFileAsync();
            if (file is null)
                return;
            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 10) //10MB
            {
                await using var readStream = await file.OpenReadAsync();
                using var reader = new StreamReader(readStream);
                FileText = await reader.ReadToEndAsync(token);
            }
            else
            {
                throw new Exception("File exceeded 10MB limit.");
            }
        }
        catch (Exception ex)
        {
            ErrorMessages?.Add(ex.Message);
        }
    }
    [RelayCommand]
    private async Task SaveFile()
    {
        ErrorMessages?.Clear();
        try
        {
            var filesService = (App.Current?.Services?.GetService<IFilesService>()) ?? throw new NullReferenceException("Missing file service instance.");
            var file = await filesService.SaveFileAsync();
            if (file is null)
                return;
            if (FileText?.Length <= 1024 * 1024 * 10) //10MB
            {
                var stream = new MemoryStream(Encoding.Default.GetBytes((string)FileText));
                await using var writeStream = await file.OpenWriteAsync();
                await stream.CopyToAsync(writeStream);
            }
            else
            {
                throw new Exception("File exceeded 10MB limit.");
            }
        }
        catch (Exception ex)
        {
            ErrorMessages?.Add(ex.Message);
        }
    }
}