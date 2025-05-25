namespace Legacy89DiskKit.FileSystem.Domain.Exception;

public class FileSystemException : System.Exception
{
    public FileSystemException(string message) : base(message) { }
    public FileSystemException(string message, System.Exception innerException) : base(message, innerException) { }
}

public class FileNotFoundException : FileSystemException
{
    public FileNotFoundException(string fileName) : base($"File not found: {fileName}") { }
}

public class FileAlreadyExistsException : FileSystemException
{
    public FileAlreadyExistsException(string fileName) : base($"File already exists: {fileName}") { }
}

public class DiskFullException : FileSystemException
{
    public DiskFullException() : base("Disk is full") { }
}

public class InvalidFileNameException : FileSystemException
{
    public InvalidFileNameException(string fileName) : base($"Invalid file name: {fileName}") { }
}

public class FileSystemNotFormattedException : FileSystemException
{
    public FileSystemNotFormattedException() : base("File system is not formatted") { }
}