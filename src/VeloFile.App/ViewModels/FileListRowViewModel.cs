using System.ComponentModel;
using VeloFile.App.Input;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.App.ViewModels;

public sealed class FileListRowViewModel : IFileListRowItem, INotifyPropertyChanged
{
    public FileListRowViewModel(ListedFileItem fileItem, ThumbnailState thumbnail)
    {
        FileItem = fileItem;
        Thumbnail = thumbnail;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ListedFileItem FileItem { get; private set; }

    public ThumbnailState Thumbnail { get; private set; }

    public string FullPath => FileItem.FullPath;

    public string Name => FileItem.Name;

    public string DisplayName => FileItem.DisplayName;

    public FileSystemEntryKind Kind => FileItem.Kind;

    public DateTimeOffset? LastWriteTimeUtc => FileItem.LastWriteTimeUtc;

    public bool IsVisuallyDimmed => FileItem.IsVisuallyDimmed;

    public double RowOpacity => IsVisuallyDimmed ? 0.58 : 1.0;

    public ThumbnailStatus ThumbnailStatus => Thumbnail.Status;

    public string ThumbnailDisplayText => Thumbnail.Artifact?.DisplayText
        ?? (FileItem.Kind is FileSystemEntryKind.Directory ? "DIR" : "...");

    public void Update(ListedFileItem fileItem, ThumbnailState thumbnail)
    {
        var itemChanged = !Equals(FileItem, fileItem);
        var thumbnailChanged = !Equals(Thumbnail, thumbnail);
        if (!itemChanged && !thumbnailChanged)
        {
            return;
        }

        FileItem = fileItem;
        Thumbnail = thumbnail;

        if (itemChanged)
        {
            OnPropertyChanged(nameof(FileItem));
            OnPropertyChanged(nameof(FullPath));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Kind));
            OnPropertyChanged(nameof(LastWriteTimeUtc));
            OnPropertyChanged(nameof(IsVisuallyDimmed));
            OnPropertyChanged(nameof(RowOpacity));
            OnPropertyChanged(nameof(ThumbnailDisplayText));
        }

        if (thumbnailChanged)
        {
            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(ThumbnailStatus));
            OnPropertyChanged(nameof(ThumbnailDisplayText));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
