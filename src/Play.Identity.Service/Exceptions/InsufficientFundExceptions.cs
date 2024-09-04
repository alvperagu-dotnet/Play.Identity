using System;

namespace Play.Identity.Service.Exceptions;

[Serializable]
internal class InsufficientFundExceptions : Exception
{
    public InsufficientFundExceptions(Guid userId, decimal gilToDebit):
        base($"Not enough gil to debit {gilToDebit} from user '{userId}'")
    {
        this.UserId = userId;
        this.Gil = gilToDebit;
    }

    public Guid UserId {get;}
    public decimal Gil {get;}
}