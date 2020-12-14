namespace IdentityServer4.Contrib.TokenExchange.Tests.Extensions
{
    using System;

    using Microsoft.Extensions.Logging;

    using Moq;

    public static class MockLoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string message, string failMessage = null)
        {
            loggerMock.VerifyLog(level, message, Times.Once(), failMessage);
        }

        public static void VerifyLogError<T>(this Mock<ILogger<T>> loggerMock, string message, string failMessage = null)
        {
            loggerMock.VerifyLog(LogLevel.Error, message, Times.Once(), failMessage);
        }

        public static void VerifyLogInfo<T>(this Mock<ILogger<T>> loggerMock, string message, string failMessage = null)
        {
            loggerMock.VerifyLog(LogLevel.Information, message, Times.Once(), failMessage);
        }

        private static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string message, Times times, string failMessage = null)
        {
            if (string.IsNullOrEmpty(failMessage))
            {
                failMessage = $"Expected log invocation with message '{message}'";
            }

            loggerMock.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<object>(o => o.ToString() == message),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                times,
                failMessage);
        }
    }
}
