namespace SpaceTruckers.Domain.Common;

public static class DomainErrorCodes
{
    public const string DRIVER_UNAVAILABLE = "DriverUnavailable";
    public const string VEHICLE_UNAVAILABLE = "VehicleUnavailable";

    public const string INSUFFICIENT_CARGO_CAPACITY = "InsufficientCargoCapacity";

    public const string TRIP_NOT_ACTIVE = "TripNotActive";
    public const string TRIP_NOT_COMPLETABLE = "TripNotCompletable";
    public const string TRIP_NOT_AT_DESTINATION = "TripNotAtDestination";

    public const string TRIP_ALREADY_STARTED = "TripAlreadyStarted";
    public const string TRIP_ALREADY_COMPLETED = "TripAlreadyCompleted";
    public const string TRIP_ALREADY_ABORTED = "TripAlreadyAborted";

    public const string CHECKPOINT_OUT_OF_ORDER = "CheckpointOutOfOrder";
}
