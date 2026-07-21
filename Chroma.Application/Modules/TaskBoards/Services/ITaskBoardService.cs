using Chroma.Application.Modules.TaskBoards.Dtos;

namespace Chroma.Application.Modules.TaskBoards.Services;

public interface ITaskBoardService
{
    Task<TaskBoardDto> GetDefaultBoardAsync(CancellationToken cancellationToken);
    Task<TaskColumnDto> CreateColumnAsync(Guid boardId, CreateTaskColumnRequest request, CancellationToken cancellationToken);
    Task<TaskColumnDto?> UpdateColumnAsync(Guid columnId, UpdateTaskColumnRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteColumnAsync(Guid columnId, CancellationToken cancellationToken);

    Task<TaskCardDetailDto> CreateCardAsync(CreateTaskCardRequest request, CancellationToken cancellationToken);
    Task<TaskCardDetailDto?> GetCardByIdAsync(Guid cardId, CancellationToken cancellationToken);
    Task<TaskCardDetailDto?> UpdateCardAsync(Guid cardId, UpdateTaskCardRequest request, CancellationToken cancellationToken);
    Task<TaskCardSummaryDto?> MoveCardAsync(Guid cardId, MoveTaskCardRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteCardAsync(Guid cardId, CancellationToken cancellationToken);

    Task<TaskLabelDto> CreateLabelAsync(Guid boardId, CreateTaskLabelRequest request, CancellationToken cancellationToken);
    Task<TaskLabelDto?> UpdateLabelAsync(Guid labelId, UpdateTaskLabelRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteLabelAsync(Guid labelId, CancellationToken cancellationToken);

    Task<TaskCommentDto> AddCommentAsync(Guid cardId, CreateTaskCommentRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken);
}
