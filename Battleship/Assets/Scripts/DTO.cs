using System;

[Serializable]
public class ApplicationUserDTO
{
    public string UserName;
    public Guid UserId;

    public override string ToString()
    {
        return $"{UserId}, {UserName}";
    }
}