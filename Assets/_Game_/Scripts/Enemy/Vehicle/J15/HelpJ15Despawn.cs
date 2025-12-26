using System.Collections;
using UnityEngine;

public class HelpJ15Despawn : MonoBehaviour
{
    public ExplosionVehicleControl ExplosionVehicleControl;

    private void OnEnable()
    {
        StartCoroutine(HelpDespawnJ15Explosion());
    }

    public IEnumerator HelpDespawnJ15Explosion()
    {
        yield return new WaitForSeconds(3f);
        ExplosionVehicleControl.ResetAllExplosions();
    }
}
