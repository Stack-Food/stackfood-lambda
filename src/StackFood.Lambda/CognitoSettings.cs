namespace StackFood.Lambda
{
    public class CognitoSettings
    {
        public string Region { get; set; }
        public string UserPoolId { get; set; }
        public string ClientId { get; set; }
        public string GuestUsername { get; set; }
        public string GuestPassword { get; set; }
        public string DefaultPassword { get; set; }
    }
}
