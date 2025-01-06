using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using System.Text;
using System.Text.Json;

namespace Avelraangame3.Controllers;

[Route("Test")]
public class TestController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _accountId;
    private readonly string _namespaceId;
    private readonly string _apiToken;
    private readonly string _globalApiKey;
    private readonly string _email;

    private readonly IDiceService _diceService;
    private ISnapshot _snapshot;

    public TestController(
        ISnapshot snapshot,
        IDiceService diceService)
    {
        _diceService = diceService;
        _snapshot = snapshot;

        _httpClient = new HttpClient();
        _accountId = "*****";
        _namespaceId = "*****";
        _apiToken = "*****";
        _email = "*****";
        _globalApiKey = "*****";
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("Roll20")]
    public IActionResult Roll20()
    {
        return Ok(_diceService.Rolld20());
    }

    [HttpGet("GetKVValueAsync")]
    public async Task<IActionResult> GetKVValueAsync()
    {
        var key = "Snapshot";
        _httpClient.DefaultRequestHeaders.Add("X-Auth-Email", _email);
        _httpClient.DefaultRequestHeaders.Add("X-Auth-Key", _globalApiKey);

        var response = await _httpClient.GetAsync($"https://api.cloudflare.com/client/v4/accounts/{_accountId}/storage/kv/namespaces/{_namespaceId}/values/{key}");
        var responseBody = await response.Content.ReadAsStringAsync();

        var deserializedSnapshot = JsonSerializer.Deserialize<Snapshot>(responseBody);

        if (deserializedSnapshot is not null)
        {
            _snapshot = JsonSerializer.Deserialize<Snapshot>(responseBody)!;
        }
        
        return Ok(deserializedSnapshot);
    }

    [HttpPut("SaveKVValueAsync")]
    public async Task<IActionResult> SaveKVValueAsync()
    {
        var key = "Snapshot";
        _httpClient.DefaultRequestHeaders.Add("X-Auth-Email", _email);
        _httpClient.DefaultRequestHeaders.Add("X-Auth-Key", _globalApiKey);

        var jsonContent = JsonSerializer.Serialize(_snapshot);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"https://api.cloudflare.com/client/v4/accounts/{_accountId}/storage/kv/namespaces/{_namespaceId}/values/{key}", content);

        return Ok(response);
    }
}
