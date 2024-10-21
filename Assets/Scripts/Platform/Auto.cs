using UnityEngine;
public class Auto : PlatformControl
{
    [SerializeField] private Vector2 _velocity = new(0, 2);
    private Vector2 _startPosition;
    [SerializeField] private float _yRange = 5f;
    [SerializeField] private float _xRange = 5f;
    private readonly float _tolerance = 0.01f;//small tolerance for avoid jitering
    private bool _boundary = false;
    private void Start()
    {
        _startPosition = transform.position;
        Moving(_velocity.x, _velocity.y);//start moving
    }
    private void FixedUpdate()
    {
        //detect if touched range boundary or even more
        Vector2 currentPosition = transform.position;

        //calculate the distance between current and start position
        float xDistance = Mathf.Abs(currentPosition.x - _startPosition.x);
        float yDistance = Mathf.Abs(currentPosition.y - _startPosition.y);

        // Check if out of bounds and update direction
        if (xDistance >= _xRange - _tolerance || yDistance >= _yRange - _tolerance)
        {
            if (!_boundary)
            {
                _boundary = true; // Mark that boundary is reached
                _velocity *= -1; // Reverse direction
                Moving(_velocity.x, _velocity.y);//call func from the parent
            }
        }
        else
        {
            _boundary = false; // Reset when not at the boundary
        }
    }
}