namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// What dealt the damage. Chain effects only trigger from direct hits
    /// (Dash and Shockwave) so one kill can't set off an endless cascade.
    /// </summary>
    public enum DamageSource { Dash, Shockwave, Trail, Chain, Fragment }

}
