using System;

public class ApplicationUserDTO
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string AccessToken { get; set; }

    public override string ToString()
    {
        return $"Username:{Username}, UserId:{UserId}, AccessToken:{AccessToken}";
    }
}