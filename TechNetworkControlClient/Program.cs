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
var repairRequestById = webApi.GetRepairRequestById(3053);
var repairRequests = webApi.GetAllRepairRequest();
webApi.PutTechEquipment(new TechEquipmentDto(){Id = "348-01", IpAddress = "127.0.0.1", Type = TechType.Computer});
var repairRequest = new RepairRequestDto()
{
    Id = 123,
    TechEquipmentId = "348-01",
    UserFromId = 300,
    Description = "RotEbal"
};
webApi.PostRepairRequest(repairRequest);

public class HttpClientWithJwt
{
    #region Fields
    
    private static HttpClientWithJwt _instance;
    private static readonly object Lock = new object();
    private readonly RestClient _httpClient;
    private const string BaseUrl = "http://192.168.0.107:7119/api";
    
    #endregion

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

    #region Users
    
    /// <summary>
    /// Отправляет запрос серверу на аутентификацию. Хэширование пароля выполняется внутри метода.
    /// </summary>
    /// <param name="email">Почта пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <returns>Аутентифицированного пользователя. Если сервер не смог аутентифицировать пользователя - null.</returns>
    public AuthUserDto Authorization(string email, string password)
    {
        var hashPass = GetHash(password);
        var request = new RestRequest("auth/by-credentials");
        request.AddQueryParameter("email", email);
        request.AddQueryParameter("password", hashPass);
        var response = _httpClient.ExecuteGet<AuthUserDto>(request);

        return !response.IsSuccessful ? null : response.Data;
    }

    /// <summary>
    /// Возвращает пользователя по id.
    /// </summary>
    /// <param name="id">Id интересующего пользователя.</param>
    /// <returns>Пользователя с укзанным id, если пользователь с таким id не был найден, возвращает null</returns>
    public UserDto GetUserById(int id)
    {
        var request = new RestRequest($"users/{id}", Method.Get);
        return ExecuteRequest<UserDto>(request).Item1;
    }

    /// <summary>
    /// Обновляет пароль пользователя. Хэширование пароля выполняется внутри метода.
    /// </summary>
    /// <param name="id">Id пользователя.</param>
    /// <param name="newPassword">Новый пароль пользователя.</param>
    /// <returns>True, если пароль был успешно обновлён, иначе false</returns>
    public bool PathPassword(int id, string newPassword)
    {
        var request = new RestRequest("users", Method.Patch);
        string hashNewPassword = GetHash(newPassword);
        string hasOldPassword = "ab38eadaeb746599f2c1ee90f8267f31f467347462764a24d71ac1843ee77fe3";
        request.AddJsonBody(new {id = id, oldPassword = hasOldPassword, newPassword = hashNewPassword});
        return ExecuteRequest<UserDto>(request).Item2;
    }
    
    #endregion

    #region RepairRequest
    
    /// <summary>
    /// Возвращает заявку на ремонт по указанному id.
    /// </summary>
    /// <param name="id">Id интересующей заявки.</param>
    /// <returns>Заявку на ремонт, если завявка с таким id не найдена, возвращает null.</returns>
    public RepairRequestDto GetRepairRequestById(int id)
    {
        var request = new RestRequest($"repairrequest/{id}", Method.Get);
        return ExecuteRequest<RepairRequestDto>(request).Item1;
    }

    /// <summary>
    /// Возвращает список всех заявок на ремонт.
    /// </summary>
    /// <returns>Список всех заявок на ремонт.</returns>
    public List<RepairRequestDto> GetAllRepairRequest()
    {
        var request = new RestRequest("repairrequest", Method.Get);
        return ExecuteRequest<List<RepairRequestDto>>(request).Item1;
    }
    
    /// <summary>
    /// Добавляет новую заявку на ремонт.
    /// </summary>
    /// <param name="repairRequest">Новая заявка.</param>
    /// <returns>Id добавленной заявки.</returns>
    public int PostRepairRequest(RepairRequestDto repairRequest)
    {
        var request = new RestRequest("repairrequest", Method.Post);
        request.AddJsonBody(repairRequest);
        return ExecuteRequest<int>(request).Item1;
    }
    
    /// <summary>
    /// Обновляет данные заявки. Можно назанчить исполнителя заявки или изменить статус заявки.
    /// </summary>
    /// <param name="newRepairRequest">Обновлённая заявка.</param>
    /// <returns>True, если данные о заявке успешно обновленны, иначе false.</returns>
    public bool PutRepairRequest(RepairRequestDto newRepairRequest)
    {
        var request = new RestRequest("repairrequest", Method.Put);
        request.AddJsonBody(newRepairRequest);
        return ExecuteRequest<RepairRequestDto>(request).Item2;
    }
    
    #endregion

    #region TechEquipment
    
    /// <summary>
    /// Возвращает сетевое оборудование по указанному id.
    /// </summary>
    /// <param name="id">Id интересующего оборудования.</param>
    /// <returns>Сетевое оборудование, если оборудование с таким id не найдено, возвращает null.</returns>
    public TechEquipmentDto GetTechEquipmentById(string id)
    {
        var request = new RestRequest($"techequipment/{id}", Method.Get);
        return ExecuteRequest<TechEquipmentDto>(request).Item1;
    }

    /// <summary>
    /// Возвращает список всего сетевого оборудования.
    /// </summary>
    /// <returns>Список всего сетевого оборудования.</returns>
    public List<TechEquipmentDto> GetAllTechEquipment()
    {
        var request = new RestRequest("techequipment", Method.Get);
        return ExecuteRequest<List<TechEquipmentDto>>(request).Item1;
    }

    /// <summary>
    /// Добавляет новое сетевое оборудование.
    /// </summary>
    /// <param name="techEquipment">Новое оборудование.</param>
    /// <returns>Id созданного оборудования.</returns>
    public string PostTechEquipment(TechEquipmentDto techEquipment)
    {
        var request = new RestRequest("techequipment", Method.Post);
        request.AddJsonBody(techEquipment);
        return ExecuteRequest<string>(request).Item1;
    }

    /// <summary>
    /// Обновляет ip адресс сетевого оборудования.
    /// </summary>
    /// <param name="techEquipment">Оборудование с обновлённым ip адресом.</param>
    /// <returns>True, если данные о оборудовании успешно обновленны, иначе false.</returns>
    public bool PutTechEquipment(TechEquipmentDto techEquipment)
    {
        var request = new RestRequest("techequipment", Method.Put);
        request.AddJsonBody(techEquipment);
        return ExecuteRequest<TechEquipmentDto>(request).Item2;
    }

    /// <summary>
    /// Удаляет сетевое оборудование по id.
    /// </summary>
    /// <param name="id">Id удаляемого оборудования.</param>
    /// <returns>True, если оборудование было успешно удаленно, иначе false.</returns>
    public bool DeleteTechEquipmentById(string id)
    {
        var request = new RestRequest($"techequipment/{id}", Method.Delete);
        return ExecuteRequest<TechEquipmentDto>(request).Item2;
    }
    
    #endregion

    #region PrivateMethods
    
    private (T, bool) ExecuteRequest<T>(RestRequest request)
    {
        var response = _httpClient.Execute<T>(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            UpdateTokens();
            response = _httpClient.Execute<T>(request);
        }
    
        return (response.Data, response.IsSuccessful);
    }
    
    private void UpdateTokens()
    {
        var request = new RestRequest("auth");
        var response = _httpClient.ExecutePut(request);

        if (!response.IsSuccessful)
            throw new Exception("Refresh token is not a valid");
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
    
    #endregion
}