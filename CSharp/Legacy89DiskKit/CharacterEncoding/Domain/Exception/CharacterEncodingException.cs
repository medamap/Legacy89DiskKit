namespace Legacy89DiskKit.CharacterEncoding.Domain.Exception;

public class CharacterEncodingException : System.Exception
{
    public CharacterEncodingException(string message) : base(message)
    {
    }

    public CharacterEncodingException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}