namespace UserService.Application.Interfaces;

public interface IPasswordService
{
    public (byte[] hash, byte[] salt) Hash(string password);
    public bool Verify(string password, byte[] hash, byte[] salt);
}
