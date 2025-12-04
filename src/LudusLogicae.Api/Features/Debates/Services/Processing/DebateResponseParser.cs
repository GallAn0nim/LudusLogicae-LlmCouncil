using System.Text.Json;
using System.Text.Json.Serialization;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Processing;

public sealed class DebateResponseParser(ILogger<DebateResponseParser> logger) : IDebateResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public MemberAnswer[] ParseMemberAnswers(List<RawPhaseResult> rawResults)
    {
        return ParsePhaseResults(rawResults, r => new MemberAnswer
        {
            Slot = r.Slot,
            Text = r.RawText
        });
    }

    public RoundQuestion[] ParseRoundQuestions(List<RawPhaseResult> rawResults, RoundPairing[] pairings)
    {
        return ParsePhaseResults(rawResults, r =>
        {
            var pairing = pairings.FirstOrDefault(p => p.FromSlot == r.Slot);
            return pairing != null
                ? new RoundQuestion
                {
                    FromSlot = r.Slot,
                    ToSlot = pairing.ToSlot,
                    Text = r.RawText
                }
                : null;
        });
    }

    public RoundAnswer[] ParseRoundAnswers(List<RawPhaseResult> rawResults)
    {
        return ParsePhaseResults(rawResults, r => new RoundAnswer
        {
            Slot = r.Slot,
            Text = r.RawText
        });
    }

    public RoundAmendment[] ParseRoundAmendments(List<RawPhaseResult> rawResults)
    {
        return ParsePhaseResults(rawResults, r => new RoundAmendment
        {
            Slot = r.Slot,
            Text = r.RawText
        });
    }

    public MemberBallot[] ParseBallots(List<RawPhaseResult> rawResults)
    {
        return ParsePhaseResults(rawResults, r => ParseBallotFromJson(r.RawText, r.Slot));
    }

    public MemberVote[] ParseTieBreakVotes(List<RawPhaseResult> rawResults)
    {
        return ParsePhaseResults(rawResults, r => ParseTieBreakFromJson(r.RawText, r.Slot));
    }

    private T[] ParsePhaseResults<T>(
        List<RawPhaseResult> results,
        Func<RawPhaseResult, T?> mapper) where T : class
    {
        return results
            .OrderBy(r => r.Slot)
            .Select(result =>
            {
                try
                {
                    return mapper(result);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to parse result for slot {Slot}. Phase result type: {Type}",
                        result.Slot,
                        typeof(T).Name);
                    return null;
                }
            })
            .Where(x => x != null)
            .Cast<T>()
            .ToArray();
    }

    private MemberBallot ParseBallotFromJson(string json, int slot)
    {
        var output = TryParseJson<BallotOutput>(json, slot);
        if (output?.Ranks == null) return new MemberBallot { Slot = slot, Ranks = [] };

        return new MemberBallot
        {
            Slot = slot,
            Ranks = output.Ranks
                .OrderBy(r => r.Rank)
                .Select(r => new RankEntry { Slot = r.Slot, Rank = r.Rank })
                .ToArray()
        };
    }

    private MemberVote ParseTieBreakFromJson(string json, int slot)
    {
        var output = TryParseJson<TieBreakOutput>(json, slot);
        return new MemberVote
        {
            VoterSlot = slot,
            VotedForSlot = output?.Vote ?? 0
        };
    }

    private T? TryParseJson<T>(string json, int slot) where T : class
    {
        try
        {
            var cleanJson = CleanJsonString(json);
            return JsonSerializer.Deserialize<T>(cleanJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "Failed to parse JSON response for slot {Slot}. Type: {Type}, JSON: {Json}",
                slot,
                typeof(T).Name,
                json.Length > 200 ? json[..200] + "..." : json);
            return null;
        }
    }

    private static string CleanJsonString(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return "{}";

        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}');

        if (start >= 0 && end > start) return json.Substring(start, end - start + 1);

        return json;
    }
}
