using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Processing;

public interface IDebateResponseParser
{
    MemberAnswer[] ParseMemberAnswers(List<RawPhaseResult> rawResults);

    RoundQuestion[] ParseRoundQuestions(List<RawPhaseResult> rawResults, RoundPairing[] pairings);

    RoundAnswer[] ParseRoundAnswers(List<RawPhaseResult> rawResults);

    RoundAmendment[] ParseRoundAmendments(List<RawPhaseResult> rawResults);

    MemberBallot[] ParseBallots(List<RawPhaseResult> rawResults);

    MemberVote[] ParseTieBreakVotes(List<RawPhaseResult> rawResults);
}
