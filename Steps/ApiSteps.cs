using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Reflection;
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

    [Given(@"I have invalid Marvel API credentials")]
    public void GivenIHaveInvalidMarvelApiCredentials()
    {
        // Use a fake private key to generate an invalid hash
        _privateKey = "invalid-private-key";
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

        // Construct and log the full URL
        string fullUrl = $"{_baseUrl}{endpoint}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        Console.WriteLine($"Request URL: {fullUrl}");

        _response = await _requestContext.GetAsync(endpoint, new APIRequestContextOptions
            {
                Params = queryParams
            }
        );
    }

    [Then(@"I receive a 200 status code")]
    public void ThenIReceiveA200StatusCode()
    {
        Assert.IsNotNull(_response, "Response should not be null");
        Assert.That(_response.Status, Is.EqualTo(200), $"Expected status code 200, but got {_response.Status}");
    }

    [Then(@"I receive a 401 status code")]
    public void ThenIReceiveA401StatusCode()
    {
        Assert.IsNotNull(_response, "Response should not be null");
        Assert.That(_response.Status, Is.EqualTo(401), $"Expected status code 401, but got {_response.Status}");
    }

    [Then(@"the response contains ""(.*)"" data")]
    public async Task ThenTheResponseContainsComicBookData(string endpoint)
    {
        Assert.IsNotNull(_response, "Response should not be null");
        string responseText = await _response.TextAsync();
        Assert.IsNotEmpty(responseText, "Response should not be empty");
        Assert.IsTrue(responseText.Contains(endpoint, StringComparison.OrdinalIgnoreCase), $"Response should contain {endpoint} data");

        var json = await _response.JsonAsync();
        Assert.IsNotNull(json, "JSON should not be null");

        if (!json.Value.TryGetProperty("data", out var dataProperty) || !dataProperty.TryGetProperty("results", out var resultsProperty))
        {
            Assert.Fail("JSON does not contain a 'data' or 'results' property.");
            return;
        }
        
        var items = resultsProperty.EnumerateArray().ToList();
        Assert.IsNotEmpty(items, "Should return at least one item in 'results'.");

        if (endpoint.Equals("comics", StringComparison.OrdinalIgnoreCase))
        {
            string title = items.First().GetProperty("title").GetString() ?? "Unknown Title";
            Console.WriteLine($"First comic title: {title}");
        }
        else if (endpoint.Equals("characters", StringComparison.OrdinalIgnoreCase))
        {
            string name = items.First().GetProperty("name").GetString() ?? "Unknown Name";
            Console.WriteLine($"First character name: {name}");
        }
        else
        {
            Assert.Fail($"Endpoint '{endpoint}' is not recognized.");
        }
    }


    [Then(@"the first comic book has a title of ""(.*)""")]
    public async Task ThenTheFirstComicBookTitle(string title)
    {
        Assert.IsNotNull(_response, "Response should not be null");
        var json = await _response.JsonAsync();
        var jsonData = json?.GetProperty("data").GetProperty("results");
        Assert.IsTrue(jsonData?.EnumerateArray().Any() == true, "Should return at least one comic");
        string? actualTitle = jsonData?.EnumerateArray().First().GetProperty("title").GetString();
        Assert.That(actualTitle, Is.EqualTo(title), $"Expected title: {title}, Actual title: {actualTitle}");
    }

    [Then(@"the first character has a name of ""(.*)""")]
    public async Task ThenTheFirstCharacterName(string characterName)
    {
        Assert.IsNotNull(_response, "Response should not be null");
        var json = await _response.JsonAsync();
        var jsonData = json?.GetProperty("data").GetProperty("results");
        Assert.IsTrue(jsonData?.EnumerateArray().Any() == true, "Should return at least one character");
        string? actualCharacterName = jsonData?.EnumerateArray().First().GetProperty("name").GetString();
        Assert.That(actualCharacterName, Is.EqualTo(characterName), $"Expected title: {characterName}, Actual title: {actualCharacterName}");
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