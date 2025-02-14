using UnityEngine;

public class CursorController : MonoBehaviour
{
	Texture2D _attackIcon;
	Texture2D _handIcon;

	enum CursorType
	{
		None,
		Hand,
		Attack
	}

	CursorType _cursortype = CursorType.None;

	int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

    private void Start()
    {
		_attackIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Attack");
		_handIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Hand");
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetMouseButton(0))
			return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100.0f, _mask))
		{
			if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
			{
				if (_cursortype != CursorType.Attack)
				{
					Cursor.SetCursor(_attackIcon, new Vector2(_attackIcon.width / 5, 0), CursorMode.Auto);
					_cursortype = CursorType.Attack;
				}
			}
			else
			{
				if (_cursortype != CursorType.Hand)
				{
					Cursor.SetCursor(_handIcon, new Vector2(_handIcon.width / 3, 0), CursorMode.Auto);
					_cursortype = CursorType.Hand;
				}
			}
		}
	}
}
