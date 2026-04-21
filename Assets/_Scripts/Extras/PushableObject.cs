using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
	private Rigidbody _rb;
	[SerializeField] private float _moveSpeed = 5f;

	void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}
	public void Push(Vector3 direction, float force)
	{
		Vector3 targetVelocity = direction * _moveSpeed;
		targetVelocity.y = _rb.velocity.y; // Preserve vertical velocity
		_rb.velocity = targetVelocity;
	}

}