namespace ProductService.DataAccess.Exceptions;

public class ProductExistException(string message) : Exception(message);