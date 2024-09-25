public class FakeHandlers(IFakeRepository repository) : BaseHandlers<IFakeRepository, FakeDto>(repository),
    IRequestHandler<InsertCommand<FakeDto>, Guid>,
    IRequestHandler<UpsertCommand<FakeDto>, Guid>,
    IRequestHandler<DeleteCommand<FakeDto>, bool>,
    IRequestHandler<TryDeleteCommand<FakeDto>, bool>,
    IRequestHandler<TryDeleteByDtoCommand<FakeDto>, bool>
{
}
