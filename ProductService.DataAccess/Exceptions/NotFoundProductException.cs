namespace ProductService.DataAccess.Exceptions;

public class NotFoundProductException(string message) : Exception(message);