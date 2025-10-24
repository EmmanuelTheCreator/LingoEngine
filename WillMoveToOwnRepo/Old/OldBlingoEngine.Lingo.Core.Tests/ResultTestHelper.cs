using FluentAssertions;

namespace BlingoEngine.Lingo.Core.Tests
{
    public static class ResultTestHelper
    {
        public static void LingoCodeShouldBeIdentical(this string actual, string expected)
        {
            var normalizedActual = actual.Replace("\r\n", "\n").Trim();
            var normalizedExpected = expected.Replace("\r\n", "\n").Trim();
            normalizedActual.Should().Be(normalizedExpected);
        }
    }
}
