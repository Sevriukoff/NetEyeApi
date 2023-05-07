using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using TechNetworkControlClient.DTO;

Console.WriteLine("Hello, World!");

var webApi = HttpClientWithJwt.GetInstance();

var user = webApi.Authorization("Tech@yandex.ru", "123321");
var userById = webApi.GetUserById(3);
var repairRequestById = webApi.GetRepairRequestById(3);
var repairRequests = webApi.GetAllRepairRequest();

public class HttpClientWithJwt
{
    private static HttpClientWithJwt _instance;
    private static readonly object Lock = new object();
    private readonly RestClient _httpClient;
    private const string BaseUrl = "http://192.168.0.107:7119/api";

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }

    private HttpClientWithJwt()
    {
        _httpClient = new RestClient(BaseUrl);
    }

    public static HttpClientWithJwt GetInstance()
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new HttpClientWithJwt();
                }
            }
        }

        return _instance;
    }

    /// <summary>
    /// Отправляет запрос серверу на аутентификацию. Хэширование пароля выполняется внутри метода.
    /// </summary>
    /// <param name="email">Почта пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <returns>Возвращает аутентифицированного пользователя. Если сервер не смогу аутентифицировать пользователя метод вернёт null.</returns>
    public AuthUserDto Authorization(string email, string password)
    {
        var hashPass = GetHash(password);
        var request = new RestRequest("auth");
        request.AddQueryParameter("email", email);
        request.AddQueryParameter("password", hashPass);
        var response = _httpClient.Get(request);

        if (!response.IsSuccessful)
            return null;
            
        using (var jsonDocument = JsonDocument.Parse(response.Content))
        {
            AccessToken = jsonDocument.RootElement.GetProperty("accessToken").GetString();
            RefreshToken = jsonDocument.RootElement.GetProperty("refreshToken").GetString();
        }

        _httpClient.Authenticator = new JwtAuthenticator(AccessToken);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var user = JsonSerializer.Deserialize<AuthUserDto>(response.Content, options);

        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UserDto GetUserById(int id)
    {
        var request = new RestRequest($"users/{id}");
        var response = _httpClient.ExecuteGet<UserDto>(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            return GetUserById(id);
        }
        
        return response.Data;
    }

    #region RepairRequest
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public RepairRequestDto GetRepairRequestById(int id)
    {
        var request = new RestRequest($"repairrequest/{id}");
        var response = _httpClient.ExecuteGet<RepairRequestDto>(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            return GetRepairRequestById(id);
        }
        
        return response.Data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<RepairRequestDto> GetAllRepairRequest()
    {
        var request = new RestRequest("repairrequest");
        var response = _httpClient.ExecuteGet<List<RepairRequestDto>>(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            return GetAllRepairRequest();
        }
        
        return response.Data;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="repairRequest"></param>
    public void PostRepairRequest(RepairRequestDto repairRequest)
    {
        var request = new RestRequest("repairrequest");
        request.AddJsonBody(repairRequest);
        var response = _httpClient.ExecutePost(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            PostRepairRequest(repairRequest);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="newRepairRequest"></param>
    public void PutRepairRequest(RepairRequestDto newRepairRequest)
    {
        var request = new RestRequest("repairrequest");
        request.AddJsonBody(newRepairRequest);
        var response = _httpClient.ExecutePut(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            PostRepairRequest(newRepairRequest);
        }
    }
    
    #endregion

    #region TechEquipment
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TechEquipmentDto GetTechEquipmentById(int id)
    {
        var request = new RestRequest($"techequipment/{id}");
        var response = _httpClient.ExecuteGet<TechEquipmentDto>(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            return GetTechEquipmentById(id);
        }
        
        return response.Data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<TechEquipmentDto> GetAllTechEquipment()
    {
        var request = new RestRequest("techequipment");
        var response = _httpClient.ExecuteGet<List<TechEquipmentDto>>(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            return GetAllTechEquipment();
        }
        
        return response.Data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="techEquipment"></param>
    public void PostTechEquipment(TechEquipmentDto techEquipment)
    {
        var request = new RestRequest("techequipment");
        request.AddJsonBody(techEquipment);
        var response = _httpClient.ExecutePost(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            PostTechEquipment(techEquipment);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="techEquipment"></param>
    public void PutTechEquipment(TechEquipmentDto techEquipment)
    {
        var request = new RestRequest("techequipment");
        request.AddJsonBody(techEquipment);
        var response = _httpClient.ExecutePut(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            PutTechEquipment(techEquipment);
        }
    }
    
    #endregion

    #region HttpMethods

    public T Get<T>(RestRequest request)
    {
        return _httpClient.Get<T>(request);
    }

    public void Post()
    {
        
    }

    public void Put()
    {
        
    }

    public void Delete()
    {
        
    }

    #endregion

    private void UpdateTokens()
    {
        var request = new RestRequest("auth");
        request.AddJsonBody(new { accessToken = AccessToken, refreshToken = RefreshToken });
        var response = _httpClient.ExecutePut(request);

        if (!response.IsSuccessful)
            throw new Exception("Refresh token is not a valid");
            
        using (var jsonDoc = JsonDocument.Parse(response.Content))
        {
            AccessToken = jsonDoc.RootElement.GetProperty("accessToken").GetString();
            RefreshToken = jsonDoc.RootElement.GetProperty("refreshToken").GetString();
        }
        
        _httpClient.Authenticator = new JwtAuthenticator(AccessToken);
    }

    private string GetHash(string inputData)
    {
        using (SHA256 sha256 = new SHA256Managed())
        {
            var data = Encoding.UTF8.GetBytes(inputData);
            var hash = sha256.ComputeHash(data);

            StringBuilder builder = new StringBuilder(128);

            foreach (var b in hash)
            {
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString().ToLower();
        }
    }
}