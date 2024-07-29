using EmphatyWave.Web.Services.Accounts.Logins;

namespace EmphatyWave.Web.Services.Accounts.Passwords
{
    public class ActivationService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        public async Task<ResultOrValue<string>> ActivateAccount(ActivateAccountModel activateEmail)
        {
            var response = await _httpClient.PostAsync($"https://localhost:7481/api/Account/confirmEmail?token={activateEmail.Token}", null);
            var rawContent = await response.Content.ReadAsStringAsync();
            // Parse the raw JSON to extract the necessary values
            var jsonDocument = System.Text.Json.JsonDocument.Parse(rawContent);
            var rootElement = jsonDocument.RootElement;

            if (response.IsSuccessStatusCode)
            {
                var token = rootElement.GetProperty("value").GetString();
                return ResultOrValue<string>.Success(token);
            }
            else
            {
                var errorElement = rootElement.GetProperty("result").GetProperty("error");
                var errorCode = errorElement.GetProperty("code").GetString();
                var errorMessage = errorElement.GetProperty("descripton").GetString();
                var error = new Error { Code = errorCode, Message = errorMessage };
                return ResultOrValue<string>.Failure(new Result { Error = error });
            }

        }
    }
}
