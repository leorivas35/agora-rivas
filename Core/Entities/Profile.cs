namespace Core.Entities;

public class Profile : AppUser
{
    public required AgoraFile AboutMe { get; set; }
    public required string ProfilePictureUrl { get; set; }
}
