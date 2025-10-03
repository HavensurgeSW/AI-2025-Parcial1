using UnityEngine;
using KarplusParcial1.RTSElements;

public class MinerView : MonoBehaviour
{
    public Miner miner = new Miner();
    void Update()
    {
        miner.minerFsm.Tick();
        this.transform.position = new Vector3(miner.pos.X, miner.pos.Y, miner.pos.Z);
    }
    private void Start()
    {
        miner.Start();
    }
    private void OnDestroy()
    {
        miner.OnDestroy();
    }   
}
