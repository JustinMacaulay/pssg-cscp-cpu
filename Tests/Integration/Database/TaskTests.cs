public class TaskTests(ITaskRepository repository)
{
    // WARNING!!! these are not reliable tests, they will fail, these were shortcuts I used for building a POC, these tests will need to be adjusted in order to be idempotent

    [Fact]
    public void Delete()
    {
        // Arrange
        var id = new Guid("");

        // Act
        var result = repository.TryDelete(id);
        
        // Assert
        Assert.True(result);
    }
}
