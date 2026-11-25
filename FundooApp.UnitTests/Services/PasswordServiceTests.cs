using UserService.Application.Services;

namespace FundooApp.UnitTests.Services;

[TestClass]
public class PasswordServiceTests
{
    private PasswordService _service;
    private const string Password = "ExamplePassword123";

    [TestInitialize]
    public void Setup()
    {
        _service = new PasswordService();
    }

    [TestMethod]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var (hash, salt) = _service.Hash(Password);
        var result = _service.Verify(Password, hash, salt);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var (hash, salt) = _service.Hash(Password);

        var result = _service.Verify("password", hash, salt);

        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void Hash_WithBlankPassword_Throws(string blankPassword)
    {
        Assert.ThrowsException<ArgumentException>(() => _service.Hash(blankPassword));
    }
}