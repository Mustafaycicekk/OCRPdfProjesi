namespace OCRPdf.Helpers;
public class ServiceResponse<T> {
	public T Data { get; set; }
	public bool Success { get; set; }
	public string ErrorMessage { get; set; }
	public string Message { get; set; }
	public static ServiceResponse<T> SuccessResponse(T data, string message = null) {
		return new ServiceResponse<T> {
			Data = data,
			Success = true,
			Message = message
		};
	}
	public static ServiceResponse<T> ErrorResponse(string errorMessage) {
		return new ServiceResponse<T> {
			Data = default,
			Success = false,
			ErrorMessage = errorMessage
		};
	}
}