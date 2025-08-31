using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace Zircon.Test;

/// <summary>
/// Base class for unit tests using AutoFixture with AutoMoq customization.
/// Provides automatic mocking and dependency injection for the system under test.
/// </summary>
/// <typeparam name="TSut">The type of the system under test.</typeparam>
public abstract class AutoMoqFixture<TSut> where TSut : class
{
    /// <summary>
    /// The AutoFixture instance configured with AutoMoq customization.
    /// </summary>
    protected readonly IFixture Fixture = new Fixture().Customize(new AutoMoqCustomization());
    private TSut? _sut;

    /// <summary>
    /// Gets the system under test, creating it with AutoFixture if not already created.
    /// </summary>
    public TSut Sut => _sut ??= Fixture.Create<TSut>();
    
    /// <summary>
    /// Gets a mock for the specified type, freezing it in the fixture to ensure consistency.
    /// </summary>
    /// <typeparam name="T">The type to mock.</typeparam>
    /// <returns>A <see cref="Mock{T}"/> instance.</returns>
    protected Mock<T> GetMock<T>() where T : class
    {
        return Fixture.Freeze<Mock<T>>();
    }
}
