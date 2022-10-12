using System;
using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos.ClosedCaptions;

/// <summary>
/// Language information.
/// </summary>
public readonly partial struct Language
{
    /// <summary>
    /// ISO 639-1 code of the language.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Full international name of the language.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes an instance of <see cref="Language" />.
    /// </summary>
    public Language(string code, string name)
    {
        Code = code;
        Name = name;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Code} ({Name})";
}

public partial struct Language : IEquatable<Language>
{
    /// <inheritdoc />
    public bool Equals(Language other) => StringComparer.OrdinalIgnoreCase.Equals(Code, other.Code);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Language other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Code);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(Language left, Language right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(Language left, Language right) => !(left == right);
}