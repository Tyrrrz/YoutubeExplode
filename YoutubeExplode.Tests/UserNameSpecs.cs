using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Tests
{
    public class UserNameSpecs
    {
        [Theory]
        [InlineData("TheTyrrr")]
        [InlineData("KannibalenRecords")]
        [InlineData("JClayton1994")]
        public void I_can_specify_a_valid_user_name(string userName)
        {
            // Act
            var result = new UserName(userName);

            // Assert
            result.Value.Should().Be(userName);
        }

        [Theory]
        [InlineData("youtube.com/user/ProZD", "ProZD")]
        [InlineData("youtube.com/user/TheTyrrr", "TheTyrrr")]
        public void I_can_specify_a_valid_user_url_in_place_of_a_user_name(string userUrl, string expecteduserName)
        {
            // Act
            var result = new UserName(userUrl);

            // Assert
            result.Value.Should().Be(expecteduserName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("The_Tyrrr")]
        [InlineData("0123456789ABCDEFGHIJK")]
        [InlineData("A1B2C3-")]
        [InlineData("=0123456789ABCDEF")]
        public void I_cannot_specify_an_invalid_user_name(string userName)
        {
            // Act & assert
            Assert.Throws<ArgumentException>(() => new UserName(userName));
        }

        [Theory]
        [InlineData("youtube.com/user/P_roZD")]
        [InlineData("example.com/user/ProZD")]
        public void I_cannot_specify_an_invalid_user_url_in_place_of_a_user_name(string userUrl)
        {
            // Act & assert
            Assert.Throws<ArgumentException>(() => new UserName(userUrl));
        }
    }
}