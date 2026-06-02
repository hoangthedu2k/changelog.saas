using ChangelogSaas.Infrastructure.Auth;

namespace ChangelogSaas.Tests.Auth
{
    public class PasswordHasherTests
    {
        private readonly BcryptPasswordHasher _hasher = new();

        [Fact]
        public void Hash_then_Verify_returns_true()
        {
            const string password = "Sup3rSecret!";
            var hash = _hasher.Hash(password);

            Assert.NotEqual(password, hash);
            Assert.StartsWith("$2", hash);
            Assert.True(_hasher.Verify(password, hash));
        }

        [Fact]
        public void Verify_with_wrong_password_returns_false()
        {
            var hash = _hasher.Hash("CorrectHorseBattery");
            Assert.False(_hasher.Verify("wrong-password", hash));
        }

        [Fact]
        public void Verify_with_invalid_hash_returns_false()
        {
            Assert.False(_hasher.Verify("anything", "not-a-bcrypt-hash"));
        }
    }
}
