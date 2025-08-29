namespace SpellCardManager.Services;

public interface IFileService {
    public Task<IFile?> OpenDeckFile(string title = "Open File");
    public Task<IFile?> SaveDeckFileAs(string title = "Save As");
    public Task<IFile?> TryGetFile(Uri path);
}

public interface IFile : IDisposable {
    public string Name { get; }
    public Uri Path { get; }

    public Task<Stream> OpenReadAsync();
    public Task<Stream> OpenWriteAsync();
}
