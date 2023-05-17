using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Tests;

public class UserNameSpecs
{
    [Theory]
    [InlineData("TheTyrrr")]
    [InlineData("KannibalenRecords")]
    [InlineData("JClayton1994")]
    public void I_can_parse_a_user_name_from_a_user_name_string(string userName)
    {
        // Act
        var parsed = UserName.Parse(userName);

        // Assert
        parsed.Value.Should().Be(userName);
    }

    [Theory]
    [InlineData("youtube.com/user/ProZD", "ProZD")]
    [InlineData("youtube.com/user/TheTyrrr", "TheTyrrr")]
    public void I_can_parse_a_user_name_from_a_URL_string(string userUrl, string expectedUserName)
    {
        // Act
        var parsed = UserName.Parse(userUrl);

        // Assert
        parsed.Value.Should().Be(expectedUserName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("The_Tyrrr")]
    [InlineData("0123456789ABCDEFGHIJK")]
    [InlineData("A1B2C3-")]
    [InlineData("=0123456789ABCDEF")]
    [InlineData("youtube.com/user/P_roZD")]
    [InlineData("example.com/user/ProZD")]
    public void I_cannot_parse_a_user_name_from_an_invalid_string(string userName)
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => UserName.Parse(userName));
    }
}