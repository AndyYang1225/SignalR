namespace SignalR
{
    public class JWTOption
    {
        public const string JWT = "JWT";
        public string Issuer { get; set; }
        public string SignKey { get; set; }
        public int Expires { get; set; }
    }
}