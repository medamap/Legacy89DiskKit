namespace Legacy89DiskKit.DiskImage.Domain.Exception;

public class DiskImageException : System.Exception
{
    public DiskImageException(string message) : base(message) { }
    public DiskImageException(string message, System.Exception innerException) : base(message, innerException) { }
}

public class SectorNotFoundException : DiskImageException
{
    public SectorNotFoundException(int cylinder, int head, int sector) 
        : base($"Sector not found: C={cylinder}, H={head}, R={sector}") { }
}

public class InvalidDiskFormatException : DiskImageException
{
    public InvalidDiskFormatException(string message) : base(message) { }
    public InvalidDiskFormatException(string message, System.Exception innerException) : base(message, innerException) { }
}

public class ReadOnlyDiskException : DiskImageException
{
    public ReadOnlyDiskException() : base("Cannot write to read-only disk") { }
}

public class DiskFullException : DiskImageException
{
    public DiskFullException(string message) : base(message) { }
    public DiskFullException(string message, System.Exception innerException) : base(message, innerException) { }
}