using LudusLogicae.Api.Features.Debates.Contracts;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Factories;

public interface IDebateFactory
{
    Debate CreateDebateEntity(CreateDebateRequest request);

    DebateState CreateInitialState(
        Debate debate,
        List<DebateParticipant> entries,
        Dictionary<Guid, string> introductions);
}
