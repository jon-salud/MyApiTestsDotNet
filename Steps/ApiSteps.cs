using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Security.Cryptography;
using System.Text;
using TechTalk.SpecFlow;

[Binding]
public class ApiSteps
{
    private readonly IAPIRequestContext _requestContext;
    private string _publicKey;
    private string _privateKey;
    private string _baseUrl;
    private IAPIResponse? _response;

    public ApiSteps()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("Config/appsettings.json")
            .Build();

        _baseUrl = config["ApiSettings:BaseUrl"] ?? throw new ArgumentException("BaseUrl not found in appsettings.json");
        _publicKey = config["ApiSettings:PublicKey"] ?? throw new ArgumentException("PublicKey not found in appsettings.json");
        _privateKey = config["ApiSettings:PrivateKey"] ?? throw new ArgumentException("PrivateKey not found in appsettings.json");

        var playwright = Playwright.CreateAsync().Result;
        _requestContext = playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = _baseUrl
        }).Result;
    }

    [Given(@"I have valid Marvel API credentials")]
    public void GivenIHaveValidMarvelApiCredentials()
    {
        Assert.IsNotEmpty(_publicKey, "Public key should not be empty");
        Assert.IsNotEmpty(_privateKey, "Private key should not be empty");
    }

    [When(@"I send a GET request to ""(.*)"" endpoint")]
    public async Task WhenISendAGetRequestToEndpoint(string endpoint)
    {
        string ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string input = ts + _privateKey + _publicKey;
        string hash = ComputeMd5Hash(input);

        var queryParams = new Dictionary<string, object>
        {
            { "ts", ts },
            { "apikey", _publicKey },
            { "hash", hash }
        };

        // // Construct and log the full URL
        // string fullUrl = $"{_baseUrl}{endpoint}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        // Console.WriteLine($"Request URL: {fullUrl}");

        _response = await _requestContext.GetAsync(endpoint, new APIRequestContextOptions
        {
            Params = queryParams
        });
    }

    [Then(@"I receive a 200 status code")]
    public void ThenIReceiveA200StatusCode()
    {
        Assert.IsNotNull(_response, "Response should not be null");
        Assert.That(_response.Status, Is.EqualTo(200), $"Expected status code 200, but got {_response.Status}");
    }

    [Then(@"the response contains comic book data")]
    public async Task ThenTheResponseContainsComicBookData()
    {
        Assert.IsNotNull(_response, "Response should not be null");
        var responseText = await _response.TextAsync();
        Assert.IsNotEmpty(responseText, "Response should not be empty");
        Assert.IsTrue(responseText.Contains("comics"), "Response should contain comic book data");
        // Console.WriteLine($"Response: {responseText}");
    }

    [AfterScenario]
    public void Cleanup()
    {
        _requestContext.DisposeAsync().GetAwaiter().GetResult();
    }

    private string ComputeMd5Hash(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}