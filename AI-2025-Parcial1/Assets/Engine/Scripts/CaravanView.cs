using KarplusParcial1.RTSElements;
using UnityEngine;

public class CaravanView : MonoBehaviour
{
    public Caravan caravan = new Caravan();
    public void Start()
    {
        caravan.Start();
    }

    private void Update()
    {
        caravan.caravanFsm.Tick();
        this.transform.position = new Vector3(caravan.pos.X, caravan.pos.Y, caravan.pos.Z);
    }

    private void OnDestroy()
    {
        caravan.OnDestroy();
    }
}
