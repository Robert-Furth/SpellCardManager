using Microsoft.Win32;
using SpellCardManager.Services;
using System.IO;

namespace SpellCardManager.WPFApp.Services;

internal class FileService : IFileService {
    public Task<IFile?> OpenDeckFile(string title = "Open File") {
        var ofd = new OpenFileDialog {
            Filter = "Spell Card Deck|*.json;*.scdeck",
        };

        var result = ofd.ShowDialog();
        if (!result.HasValue || result.Value == false) return Task.FromResult<IFile?>(null);

        return Task.FromResult<IFile?>(new FileWrapper(ofd.FileName));
    }

    public Task<IFile?> SaveDeckFileAs(string title = "Save As") {
        var sfd = new SaveFileDialog {
            Filter = "JSON File|*.json|Compressed Deck|*.scdeck",
        };

        var result = sfd.ShowDialog();
        if (!result.HasValue || result.Value == false) return Task.FromResult<IFile?>(null);

        return Task.FromResult<IFile?>(new FileWrapper(sfd.FileName));
    }

    public Task<IFile?> TryGetFile(Uri path) {
        var nullTask = Task.FromResult<IFile?>(null);

        if (!path.IsFile) return nullTask;
        var pathStr = path.LocalPath;

        if (!Path.Exists(pathStr)) return nullTask;
        var attrs = File.GetAttributes(pathStr);

        if (attrs.HasFlag(FileAttributes.Directory)) return nullTask;

        return Task.FromResult<IFile?>(new FileWrapper(pathStr));
    }
}

internal class FileWrapper : IFile {
    public string Name { get; }
    public Uri Path { get; }

    public FileWrapper(string pathStr) {
        Path = new Uri(pathStr);
        Name = System.IO.Path.GetFileName(pathStr);
    }

    public void Dispose() {
        return;
    }

    public Task<Stream> OpenReadAsync() => Task.Run(() => (Stream)File.OpenRead(Path.LocalPath));

    public Task<Stream> OpenWriteAsync() => Task.Run(() => (Stream)File.Create(Path.LocalPath));
}
