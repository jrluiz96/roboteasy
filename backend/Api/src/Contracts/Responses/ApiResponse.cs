namespace Api.Contracts.Responses;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Status = "success",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Created(T data, string message = "Recurso criado com sucesso")
    {
        return new ApiResponse<T>
        {
            Code = 201,
            Status = "success",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(int code, string message)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Status = "error",
            Message = message,
            Data = default
        };
    }

    public static ApiResponse<T> NotFound(string message = "Recurso não encontrado")
    {
        return new ApiResponse<T>
        {
            Code = 404,
            Status = "error",
            Message = message,
            Data = default
        };
    }

    public static ApiResponse<T> Unauthorized(string message = "Não autorizado")
    {
        return new ApiResponse<T>
        {
            Code = 401,
            Status = "error",
            Message = message,
            Data = default
        };
    }

    public static ApiResponse<T> BadRequest(string message = "Requisição inválida")
    {
        return new ApiResponse<T>
        {
            Code = 400,
            Status = "error",
            Message = message,
            Data = default
        };
    }
}

// Para respostas sem data
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Operação realizada com sucesso")
    {
        return new ApiResponse
        {
            Code = 200,
            Status = "success",
            Message = message,
            Data = null
        };
    }

    public static ApiResponse Fail(int code, string message)
    {
        return new ApiResponse
        {
            Code = code,
            Status = "error",
            Message = message,
            Data = null
        };
    }
}
