using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace IntegrationTest;

[TestFixture]
public class Tests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task ValidIssuer()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiaXNzIjoiUmF5dmFyeiJ9.M217m1Inmi2o4tdAWNaF2cL8mBTiiN2Ahx8PNmK2XRg");

        var response = await _httpClient.SendAsync(request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Test]
    public async Task ExpiredToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZXhwIjoxNjk2MTE4NDAxfQ.UYXfcgd4xNqZ6G1fRbfk1s-7nqLPPbAcIPOoOpR8U8w");

        var response = await _httpClient.SendAsync(request);
        
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.AreEqual("Token Expired", await response.Content.ReadAsStringAsync());
    }
}