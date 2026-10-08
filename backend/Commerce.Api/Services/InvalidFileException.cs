namespace Commerce.Api.Services;

/// <summary>Error de validación del archivo; el controlador lo convierte en 400 BadRequest.</summary>
public class InvalidFileException : Exception
{
    public InvalidFileException(string message) : base(message) { }
}