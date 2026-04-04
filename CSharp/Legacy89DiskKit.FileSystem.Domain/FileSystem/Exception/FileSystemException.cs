namespace Legacy89DiskKit.FileSystem.Domain.@Exception;

public class FileSystemException : System.Exception
{
    public FileSystemException(string message) : base(message) { }
    public FileSystemException(string message, System.Exception innerException) : base(message, innerException) { }
}
