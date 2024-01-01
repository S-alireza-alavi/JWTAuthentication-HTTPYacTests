using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace IntegrationTest;

[TestFixture]
public class Test
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
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiaXNzIjoiUmF5dmFyeiJ9.Dz3haMZfXwHOT4_k4nOecpKqz6aXZCIBCMBXX6q2WiZfrVe0M-OjNWiZaXv7ZZ6UtDpC6phgkYBfXgUu_HUX0x1ND694VNvHt2dF3CFsMLJH6HpVLSdXuapzFKFWYF7HI8sTW0nPSAi7QkEezSxuP7QtUR-JjEVO2iX0RaVixrbBnK8hOWj_Gi-1tBiTd3pt_-6Zc41OlAE0CAW8jCxnU60mAjm-QxdoJfiBcb54kM4h-Ih5EWV8kH_AU1l2wRTBJoN7vOxUqdGkJ5HJUMsTGhwjVrobEnyUYh9htwIAfg6gdDsnB8nopMJO1hmdle1LraApNoxz1PsKIQWVdhxygg");

        var response = await _httpClient.SendAsync(request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Test]
    public async Task ExpiredToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZXhwIjoxNjk2MTE4NDAxfQ.TPpQMjTbAnh__IAwJRZmsK5XX53KJBxsojHxFCNLPvEQbCldcT49TSQATpG7ZhtQJ61-AZLic2oOGyTCg1pLPEstxZukSoKs3osYp0x8cLNCwx0uJ8ILY8KTTvCDEjQZbEaeepoQe-beK26LT1TKCc2jwzhPqMo45OqifPPgnsdYgRm0efw4JFBgPHgb38Q6n5Cyx09S-Vv4hbTeGu1gcHc5vHUaEqVV_8LG5MkPhd1_WN88kZgM2ADbYRl_pQljTUwV4ev8mKANLvMxLQZ3G_6nNjhieOJfhl0EEHo1rinXHt3qH3z95pST_k9zdJKFmu8mT3_6PUxm0Pbs7Nc3tQ");

        var response = await _httpClient.SendAsync(request);
        
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.AreEqual("Token Expired", await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task GetCurrentUser_ReturnsUserName()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/GetCurrentUser");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiaXNzIjoiUmF5dmFyeiIsIlVzZXJJRCI6IjEwMDEifQ.buwnGmi-KrCcohWS1_pLVKsgi9v6fvxe-woYmhKOAD-sMXzNLG8H8k-lXWF2YAn7emydxiP8fGhC7dUK8O8Pg6UoQR5u4XNbx210Y-EIK_DAu_T1Vp6yWSTfo_W_i6pxa2nIIRhSge9S1zjIL3luhtZlI0MBSmBYG3NCnqvC_Dy2KxFjAKzbADK4QbbntK841fpihgD8H-WWGcuuw1I_yrV0_M-AgN6ol_te5D8qg9TJZZ1EH9RbBA-abUSOkV39AzEkBH8hQ-3dNN-fkqB6YMFgXwk6ZAAfOaB9iLWqyWM6i0kl1eCcmks278EOO8UFN4ZI3zn-UeZ8sF1h-d10lw");

        var response = await _httpClient.SendAsync(request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("AlirezaAlavi", await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task GetRoles_ReturnsUserRoles()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/GetRoles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiaXNzIjoiUmF5dmFyeiIsIlVzZXJJRCI6IjEwMDEifQ.3gOITywfQJrzfoBrCF_IMpY-tbHpH1szUY4QvbB2rfs");

        var response = await _httpClient.SendAsync(request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("[\"Admin\",\"Customer\"]", await response.Content.ReadAsStringAsync());
    }
}