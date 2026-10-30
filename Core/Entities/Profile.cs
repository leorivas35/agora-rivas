namespace Core.Entities;

public class Profile : AppUser
{
    public required Document AboutMe { get; set; }
    public required string ProfilePictureUrl { get; set; }
}
