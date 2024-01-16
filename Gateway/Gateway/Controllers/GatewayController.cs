using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Gateway.Data;
using Gateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    public GatewayController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] DbUser user)
    {
        var url = "http://localhost:5226/api/Auth/register";

        using (var client = _httpClientFactory.CreateClient())
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, jsonContent);

            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                return Ok(await response.Content.ReadAsStringAsync());
            }
        }

        return BadRequest("Registration failed");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DbUser request)
    {
        var url = "http://localhost:5226/api/Auth/login";

        using (var client = _httpClientFactory.CreateClient())
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                Response.Cookies.Append("AuthCookie", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddMinutes(5),
                });

                return Ok(token);
            }
        }

        return Unauthorized();
    }



    [HttpPost("uploadfile")]
    // [Authorize]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var url = "http://localhost:5064/uploadfile";

        using (var client = _httpClientFactory.CreateClient())
        {
            var authToken = Request.Cookies["AuthCookie"];

            if (!string.IsNullOrEmpty(authToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    var content = new ByteArrayContent(stream.ToArray());
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    var multiContent = new MultipartFormDataContent();
                    multiContent.Add(content, "file", file.FileName);

                    var response = await client.PostAsync(url, multiContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(await response.Content.ReadAsStringAsync());
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Content: {errorContent}");
                }
            }
        }

        return BadRequest("File upload failed");
    }

    [HttpGet("downloadfile/{id}")]
    // [Authorize]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var getFileUrl = $"http://localhost:5064/GetFile/{id}";


        using (var client = _httpClientFactory.CreateClient())
        {
            var authToken = Request.Cookies["AuthCookie"];

            if (!string.IsNullOrEmpty(authToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                var getResponse = await client.GetAsync(getFileUrl);

                if (getResponse.IsSuccessStatusCode)
                {
                    var content = await getResponse.Content.ReadAsStringAsync();
                    var fileModel = JsonSerializer.Deserialize<DbFile>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (fileModel != null)
                    {
                        var downloadUrl = $"http://localhost:5011/view/download";

                        var fileModelJson = JsonSerializer.Serialize(fileModel);

                        var stringContent = new StringContent(fileModelJson, Encoding.UTF8, "application/json");

                        var downloadResponse = await client.PostAsync(downloadUrl, stringContent);

                        if (downloadResponse.IsSuccessStatusCode)
                        {
                            var fileContent = await downloadResponse.Content.ReadAsByteArrayAsync();
                            var fileType = downloadResponse.Content.Headers.ContentType?.ToString() ?? fileModel.FileType;
                            var fileName = downloadResponse.Content.Headers.ContentDisposition?.FileName ?? fileModel.FileName;

                            if (!string.IsNullOrEmpty(fileType))
                            {
                                return File(fileContent, fileType, fileName);
                            }
                            else
                            {
                                return BadRequest("Invalid or missing content type");
                            }
                        }
                    }
                }
                return NotFound();
            }
            return Unauthorized();
        }
    }
}
