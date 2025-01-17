using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
	public enum PlayerState                                     //플레이어 상태 선언
	{
		Die,
		Moving,
		Idle,
		Skill,
	}

	int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

	PlayerStat _stat;
	Vector3 _destPos;

	GameObject _lookTarget;

	[SerializeField]
	PlayerState _state = PlayerState.Idle;

	void Start()
    {

		_stat = gameObject.GetComponent<PlayerStat>();

		Managers.Input.MouseAction -= OnMouseEvent;
		Managers.Input.MouseAction += OnMouseEvent;			//마우스 클릭을 이동
	}
	
	public PlayerState State
	{
		get { return _state; }
		set
		{
			_state = value;

			Animator anim = GetComponent<Animator>();
			switch (_state)
			{
				case PlayerState.Die:
					break;
				case PlayerState.Idle:
					anim.CrossFade("WAIT", 0.1f);
					break;
				case PlayerState.Moving:
					anim.CrossFade("RUN", 0.1f);
					break;
				case PlayerState.Skill:
					anim.CrossFade("Attack", 0.1f, -1, 0);
					break;
			}
		}
	}

	void UpdateDie()
	{
		// 사망(아무것도 못함)

	}

	void UpdateMoving()
	{
		//몬스터가 사정거리 안에 있으면 공격
		if (_lookTarget != null)
		{
			float _distance = (_destPos - transform.position).magnitude;
			if (_distance <= 1)
			{
				State = PlayerState.Skill;
				return;
			}
		}

		Vector3 dir = _destPos - transform.position;
		if (dir.magnitude < 0.1f)
		{
			State = PlayerState.Idle;
		}
		else
		{
			float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
			NavMeshAgent nav = gameObject.GetOrAddComponent<NavMeshAgent>();
			
			nav.Move(dir.normalized * moveDist);

			Debug.DrawRay(transform.position, dir.normalized, Color.green);
			if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
            {
				if(Input.GetMouseButton(0) == false)
					State = PlayerState.Idle;
				return;
            }

			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
		}

		
	}

	void UpdateIdle()
	{
		
	}

    private void UpdateSkill()
    {
		if(_lookTarget != null)
        {
			Vector3 dir = _lookTarget.transform.position - transform.position;
			Quaternion qua = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, qua, 20 * Time.deltaTime);
        }
    }

	public void OnHitEvent()
    {
		if(_stopSkill)
        {
			State = PlayerState.Idle;
		}
		else
        {
			State = PlayerState.Skill;
		}

		
	}

    void Update()
    {
		switch (State)
		{
			case PlayerState.Die:
				UpdateDie();
				break;
			case PlayerState.Moving:
				UpdateMoving();
				break;
			case PlayerState.Idle:
				UpdateIdle();
				break;
			case PlayerState.Skill:
				UpdateSkill();
				break;
		}
	}

	bool _stopSkill = false;

	void OnMouseEvent(Define.MouseEvent evt)		//마우스 클릭시 이동하지 못하는 곳을 눌렀을 때 이동하지 않도록 하기
	{
		switch(State)
        {
			case PlayerState.Idle:
				OnMouseEvent_IdleRun(evt);
				break;
			case PlayerState.Moving:
				OnMouseEvent_IdleRun(evt);
				break;
			case PlayerState.Skill:
                {
					if(evt == Define.MouseEvent.PointUp)
                    {
						_stopSkill = true;

					}
                }
				break;
		}
	}

	void OnMouseEvent_IdleRun(Define.MouseEvent evt)
    {
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		bool _rayCastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
		//Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);

		switch (evt)
		{
			case Define.MouseEvent.PointDown:
				{
					if (_rayCastHit)
					{
						_destPos = hit.point;
						State = PlayerState.Moving;
						_stopSkill = false;

						if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
							_lookTarget = hit.collider.gameObject;
						else
							_lookTarget = null;
					}
				}
				break;
			case Define.MouseEvent.Press:
				{
					if (_lookTarget == null && _rayCastHit)
						_destPos = hit.point;
				}
				break;
			case Define.MouseEvent.PointUp:
				_stopSkill = true;
				break;
		}
	}
}
