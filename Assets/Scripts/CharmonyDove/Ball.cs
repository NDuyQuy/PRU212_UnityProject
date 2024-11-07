using UnityEngine;

public class Ball : DoveProjectile
{
    private Transform _parrent;
    [SerializeField] private float orbitSpeed = 10f;
    private void Start()
    {
        _parrent = transform.parent;
    }
    protected override void Update()
    {
        base.Update();
        if(_parrent!=null)
        {
            transform.RotateAround(_parrent.position,Vector3.forward,orbitSpeed*Time.deltaTime);
        }
    }
}
