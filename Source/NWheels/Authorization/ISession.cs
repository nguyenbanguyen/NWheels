using System;
using System.Globalization;
using System.Security.Principal;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization
{
    public interface ISession
    {
        string Id { get; }
        IPrincipal UserPrincipal { get; }
        IIdentityInfo UserIdentity { get; }
        IEndpoint Endpoint { get; }
        CultureInfo Culture { get; }
        TimeZoneInfo TimeZone { get; }
        DateTime OpenedAtUtc { get; }
        DateTime? ExpiresAtUtc { get; }
        bool IsGlobalImmutable { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICoreSession
    {
        ITransportConnection EndpointConnection { get; set; }
    }
}
