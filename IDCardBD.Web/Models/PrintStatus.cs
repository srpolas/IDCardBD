namespace IDCardBD.Web.Models
{
    public enum PrintStatus
    {
        None = 0,
        SentToPrint = 1,
        Processing = 2,
        Printed = 3,
        ReadyForDelivery = 4
    }
}
