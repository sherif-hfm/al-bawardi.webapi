namespace janaez.webapi.Models
{
    public class BasicAuthSettings
    {
        public static string Key { get; set; } = "BasicAuth";
        public string Username { get; set; }
        public string Password { get; set; }

    }
}
