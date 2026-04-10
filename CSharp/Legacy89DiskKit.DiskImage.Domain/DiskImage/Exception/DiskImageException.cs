namespace Legacy89DiskKit.Domain.DiskImage.Exception;

public class DiskImageException : System.Exception
{
    public DiskImageException(string message) : base(message) { }
    public DiskImageException(string message, System.Exception innerException) : base(message, innerException) { }
}
