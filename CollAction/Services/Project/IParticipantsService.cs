using System;
using System.Threading.Tasks;
using CollAction.Models;

namespace CollAction.Services.Project
{
    public interface IParticipantsService
    {
        Task<AddParticipantResult> AddAnonymousParticipant(int projectId, string email);
        Task<AddParticipantResult> AddLoggedInParticipant(int projectId, string userId);
        Task RefreshParticipantCountMaterializedView();
        Task<ProjectParticipant> GetParticipant(string userId, int projectId);
        Task<string> GenerateParticipantsDataExport(int projectId);
    }

    public enum AddParticipantScenario
    {
        LoggedInAndAdded = 0,
        AnonymousCreatedAndAdded = 1,
        AnonymousAlreadyRegisteredAndAdded = 2,
        AnonymousNotRegisteredPresentAndAdded = 3,
        AnonymousAlreadyRegisteredAndAlreadyParticipating = 4,
        AnonymousNotRegisteredPresentAndAlreadyParticipating = 5
    }
}
