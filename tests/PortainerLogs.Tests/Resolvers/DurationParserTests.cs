using PortainerLogs.Resolvers;

namespace PortainerLogs.Tests.Resolvers;

[TestClass]
public class DurationParserTests
{
    [TestMethod]
    public void Parse_HoursOnly_ReturnsCorrectTimeSpan()
    {
        var result = DurationParser.Parse("2h");
        Assert.AreEqual(TimeSpan.FromHours(2), result);
    }

    [TestMethod]
    public void Parse_MinutesOnly_ReturnsCorrectTimeSpan()
    {
        var result = DurationParser.Parse("30m");
        Assert.AreEqual(TimeSpan.FromMinutes(30), result);
    }

    [TestMethod]
    public void Parse_HoursAndMinutes_ReturnsCorrectTimeSpan()
    {
        var result = DurationParser.Parse("2h30m");
        Assert.AreEqual(new TimeSpan(2, 30, 0), result);
    }

    [TestMethod]
    public void Parse_CaseInsensitive()
    {
        Assert.AreEqual(TimeSpan.FromHours(1), DurationParser.Parse("1H"));
        Assert.AreEqual(TimeSpan.FromMinutes(15), DurationParser.Parse("15M"));
        Assert.AreEqual(new TimeSpan(1, 15, 0), DurationParser.Parse("1H15M"));
    }

    [TestMethod]
    public void Parse_IsoTimestamp_ReturnsElapsedTime()
    {
        var oneHourAgo = DateTimeOffset.UtcNow.AddHours(-1).ToString("o");
        var result = DurationParser.Parse(oneHourAgo);

        // Allow 5 seconds tolerance
        Assert.IsTrue(result.TotalMinutes >= 59.9 && result.TotalMinutes <= 60.1,
            $"Expected ~60 minutes but got {result.TotalMinutes}");
    }

    [TestMethod]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => DurationParser.Parse(""));
    }

    [TestMethod]
    public void Parse_WhitespaceOnly_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => DurationParser.Parse("   "));
    }

    [TestMethod]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => DurationParser.Parse("abc"));
    }

    [TestMethod]
    public void Parse_JustLetters_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => DurationParser.Parse("hm"));
    }

    [TestMethod]
    public void Parse_NegativeNumber_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => DurationParser.Parse("-1h"));
    }

    [TestMethod]
    public void ToUnixTimestamp_ReturnsReasonableValue()
    {
        var before = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
        var result = DurationParser.ToUnixTimestamp("1h");
        var after = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();

        Assert.IsTrue(result >= before && result <= after,
            $"Expected timestamp between {before} and {after} but got {result}");
    }

    [TestMethod]
    public void Parse_OneHour_Exactly()
    {
        var result = DurationParser.Parse("1h");
        Assert.AreEqual(TimeSpan.FromHours(1), result);
    }

    [TestMethod]
    public void Parse_OneMinute_Exactly()
    {
        var result = DurationParser.Parse("1m");
        Assert.AreEqual(TimeSpan.FromMinutes(1), result);
    }

    [TestMethod]
    public void Parse_LargeValues()
    {
        var result = DurationParser.Parse("48h");
        Assert.AreEqual(TimeSpan.FromHours(48), result);

        result = DurationParser.Parse("120m");
        Assert.AreEqual(TimeSpan.FromMinutes(120), result);
    }
}
