// ==================================================================
// FILE : PaymentStatus.cs
// LAYER: Domain (Entities)
// PATH : payment-service/FinVault.PaymentService.Domain/Entities/
//
// WHAT IS THIS?
// These are CONSTANT names for different stages of a payment.
// Instead of typing "Success" everywhere (and making a typo like "Succes"),
// we use these fixed names.
//
// Published by : Logic inside payment-service
// Consumed by  : Database and API responses
// ==================================================================

namespace FinVault.PaymentService.Domain.Entities;

public static class PaymentStatus
{
    // The payment just started, we are waiting for the bank
    public const string Pending = "Pending";

    // The bank said "Yes! Money is gone"
    public const string Success = "Success";

    // The bank said "No! Not enough money" or "Wrong card"
    public const string Failed = "Failed";

    // The user cancelled the payment mid-way
    public const string Cancelled = "Cancelled";
}
