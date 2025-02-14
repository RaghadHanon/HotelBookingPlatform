using AutoFixture;
using AutoFixture.AutoMoq;

namespace HotelBookingBlatform.Application.Tests.Common;

public static class FixtureFactory
{
    public static IFixture CreateFixture()
    {
        IFixture fixture = new Fixture().Customize(new CompositeCustomization(new AutoMoqCustomization()));
        fixture.Customize<TimeOnly>(composer => composer.FromFactory<DateTime>(TimeOnly.FromDateTime));
        fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(behavior => fixture.Behaviors.Remove(behavior));

        return fixture;
    }
}