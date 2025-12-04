using LudusLogicae.Api.Features.Debates.Models;

namespace LudusLogicae.Api.Features.Debates.Services.Results;

public interface IDebateWinnerBuilder
{
    DebateWinner BuildWinner(DebateState state, int winnerSlot);
}
