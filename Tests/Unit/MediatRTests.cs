using Task = System.Threading.Tasks.Task;

public class MediatRTests(IMediator mediator)
{
    [Fact]
    public async Task Send_Insert_Command()
    {
        var fake = new FakeDto();
        fake.Id = new Guid();
        var command = new InsertCommand<FakeDto>(fake);
        var response = await mediator.Send(command);

        Assert.Equal(Guid.Empty, response);
    }

    [Fact]
    public async Task Send_Upsert_Command()
    {
        var fake = new FakeDto();
        fake.Id = new Guid();
        var command = new UpsertCommand<FakeDto>(fake);
        var response = await mediator.Send(command);

        Assert.Equal(Guid.Empty, response);
    }

    [Fact]
    public async Task Send_Delete_Command()
    {
        var id = new Guid();
        var command = new DeleteCommand<FakeDto>(id);
        var response = await mediator.Send(command);

        Assert.True(response);
    }

    [Fact]
    public async Task Send_TryDelete_Command()
    {
        var id = new Guid();
        var command = new TryDeleteCommand<FakeDto>(id);
        var response = await mediator.Send(command);

        Assert.True(response);
    }

    [Fact]
    public async Task Send_TryDelete_ByDto_Command()
    {
        var fake = new FakeDto();
        fake.Id = new Guid();
        var command = new TryDeleteByDtoCommand<FakeDto>(fake);
        var response = await mediator.Send(command);

        Assert.True(response);
    }
}

