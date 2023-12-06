using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Bars.TableStyle.Services;

public interface IFilesService
{
    public string OpenTitle { get; set; }
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
}
