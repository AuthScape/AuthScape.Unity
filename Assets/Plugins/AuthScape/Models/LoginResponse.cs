using System;

[Serializable]
public class LoginResponse
{
    public LoginState state;
    public string access_token;
    public string refresh_token;
    public int expires_in { get; set; }
    public string id_token;
}
