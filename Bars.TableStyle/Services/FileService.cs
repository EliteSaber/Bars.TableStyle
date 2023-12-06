using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Bars.TableStyle.Services;

public class FileService : IFilesService
{
    private readonly Window _target;

    public FileService(Window target)
    {
        _target = target;
    }

    public string OpenTitle { get; set; } = "Load";
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var x = new FilePickerFileType("xml") { Patterns = new[] { "*.xml" } };
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = OpenTitle,
            AllowMultiple = false,
            FileTypeFilter = new[] {x}
        });
        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        var x = new FilePickerFileType("xml") { Patterns = new[] { "*.xml" } };
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Сохранить файл ЭФ",
            FileTypeChoices = new[] {x}
        });
    }
}
