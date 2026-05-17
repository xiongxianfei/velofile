using System.ComponentModel;
using VeloFile.App.Input;
using VeloFile.App.Ui;
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

    public bool IsHidden => FileItem.IsHidden;

    public bool IsProtectedOperatingSystemFile => FileItem.IsProtectedOperatingSystemFile;

    public bool IsVisuallyDimmed => FileItem.IsVisuallyDimmed;

    public FileListRowVisibilityKind VisibilityKind
    {
        get
        {
            if (IsProtectedOperatingSystemFile)
            {
                return FileListRowVisibilityKind.ProtectedSystem;
            }

            if (IsHidden || IsVisuallyDimmed)
            {
                return FileListRowVisibilityKind.Hidden;
            }

            return FileListRowVisibilityKind.Normal;
        }
    }

    public ThumbnailStatus ThumbnailStatus => Thumbnail.Status;

    public FileListIconKind IconKind => FileListIconKindResolver.Resolve(FileItem, Thumbnail);

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
            OnPropertyChanged(nameof(IsHidden));
            OnPropertyChanged(nameof(IsProtectedOperatingSystemFile));
            OnPropertyChanged(nameof(IsVisuallyDimmed));
            OnPropertyChanged(nameof(VisibilityKind));
            OnPropertyChanged(nameof(IconKind));
            OnPropertyChanged(nameof(ThumbnailDisplayText));
        }

        if (thumbnailChanged)
        {
            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(ThumbnailStatus));
            OnPropertyChanged(nameof(IconKind));
            OnPropertyChanged(nameof(ThumbnailDisplayText));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
