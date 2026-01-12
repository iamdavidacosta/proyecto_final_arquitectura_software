namespace FileIngestionService.Application.Exceptions;

public class FileUploadException : Exception
{
    public FileUploadException(string message) : base(message)
    {
    }

    public FileUploadException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class InvalidFileException : Exception
{
    public InvalidFileException(string message) : base(message)
    {
    }
}

public class FileSizeExceededException : Exception
{
    public FileSizeExceededException(string message) : base(message)
    {
    }
}
