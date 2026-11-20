using System.Text;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Rounds;

public sealed class RoundTranscriptFormatter
{
    public string FormatRoundTranscript(DebateRound round, List<DebateParticipant> participants)
    {
        var transcript = new StringBuilder();
        transcript.AppendLine($"Round {round.RoundNumber} Transcript:");

        foreach (var question in round.Questions)
        {
            AppendQuestionAndAnswer(transcript, question, round.Answers, participants);
            transcript.AppendLine();
        }

        return transcript.ToString();
    }

    private static void AppendQuestionAndAnswer(
        StringBuilder transcript,
        RoundQuestion question,
        RoundAnswer[] answers,
        List<DebateParticipant> participants)
    {
        var askerName = GetParticipantName(participants, question.FromSlot);
        var targetName = GetParticipantName(participants, question.ToSlot);

        transcript.AppendLine($"Q ({askerName} -> {targetName}): {question.Text}");

        var answer = answers.FirstOrDefault(a => a.Slot == question.ToSlot);
        if (answer != null)
        {
            transcript.AppendLine($"A ({targetName}): {answer.Text}");
        }
    }

    private static string GetParticipantName(List<DebateParticipant> participants, int slot)
    {
        return participants.FirstOrDefault(p => p.SlotIndex == slot)?.Name ?? "Unknown";
    }
}
