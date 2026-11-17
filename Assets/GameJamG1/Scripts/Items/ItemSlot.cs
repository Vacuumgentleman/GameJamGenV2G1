namespace GameJamG1.Items
{
    public enum ItemSlot
    {
        // Mano principal, exclusiva: no se combina con ningun otro objeto (vela).
        Light,
        // Mano principal: objetos que se cargan (martillo, tabla). Compatible con OffHand.
        Carry,
        // Mano secundaria: se lleva a la vez que un objeto Carry (linterna).
        OffHand
    }
}
