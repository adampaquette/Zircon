using AutoFixture;
using AutoFixture.AutoMoq;

namespace Zircon.Test;

public abstract class AutoMoqFixture<T> where T : class
{
    protected readonly IFixture Fixture = new Fixture()
        .Customize(new AutoMoqCustomization());

    public virtual T Create() => Fixture.Create<T>();
}
