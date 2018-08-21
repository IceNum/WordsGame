using UnityEngine;
using System.Collections;

public class Transformer : MonoBehaviour {

    private float p_lerpTime, r_lerpTime, s_lerpTime;
	private float p_currentLerpTime, r_currentLerpTime, s_currentLerpTime;
	private bool needMove, needMoveUI, needRotate, needScale, reverse, pingPong;
	private Vector3 fromPos, toPos, fromScale, toScale;
    private Vector2 fromRectPos, toRectPos;
    private int m_count, s_count, m_currentCount, s_currentCount;
	private Quaternion toRot;
	
	public void Move(Vector3 targetPos, float time, bool pingPong)
    {
		p_lerpTime = time;
		fromPos = transform.position;
		toPos = targetPos;
		needMove = true;
        this.pingPong = pingPong;
        p_currentLerpTime = 0;
	}

    public void MoveImpulse(Vector3 targetPos, float time, int count)
    {
        p_lerpTime = time;
        fromPos = transform.position;
        toPos = targetPos;
        needMove = true;
        reverse = false;
        m_count = count;
        m_currentCount = 0;
        p_currentLerpTime = 0;
    }

    public void MoveUI(Vector2 targetRectPos, float time)
    {
        p_lerpTime = time;
        fromRectPos = GetComponent<RectTransform>().anchoredPosition ;
        toRectPos = targetRectPos;
        needMoveUI = true;
        p_currentLerpTime = 0;
    }

    public void MoveUIImpulse(Vector2 targetRectPos, float time, int count)
    {
        p_lerpTime = time;
        fromRectPos = GetComponent<RectTransform>().anchoredPosition;
        toRectPos = targetRectPos;
        needMoveUI = true;
        reverse = false;
        m_count = count;
        m_currentCount = 0;
        p_currentLerpTime = 0;
    }

    public void Rotate(Vector3 rotAngle, float time){
		if(!needRotate)
			toRot = transform.rotation;
		toRot.eulerAngles += rotAngle;
		needRotate = true;
		r_lerpTime = time;
		r_currentLerpTime = 0;
	}
	
	public void Scale(Vector3 targetScale, float time, bool pingPong){
		s_lerpTime = time;
		fromScale = transform.localScale;
		toScale = targetScale;
		needScale = true;
		reverse = false;
		this.pingPong = pingPong;
		s_currentLerpTime = 0;
	}

    public void ScaleImpulse(Vector3 targetScale, float time, int count)
    {
        s_lerpTime = time;
        fromScale = transform.localScale;
        toScale = targetScale;
        needScale = true;
        reverse = false;
        s_count = count;
        s_currentCount = 0;
        s_currentLerpTime = 0;
    }

    public void StopMove(){
		needMove = false;
	}
	
	void FixedUpdate () {
		if(needMove)
			updatePosition();
		if(needRotate)
			updateRotation();
		if(needScale)
			updateScale();
        if (needMoveUI)
            updateRectPosition();
	}
	
	void updatePosition(){
		p_currentLerpTime += !reverse ? Time.deltaTime : -Time.deltaTime;
        if (p_currentLerpTime > p_lerpTime)
			p_currentLerpTime = p_lerpTime;
		
		float t = p_currentLerpTime/p_lerpTime;
		t = Mathf.Sin(t * Mathf.PI * 0.5f);
		transform.position = Vector3.Lerp(fromPos, toPos, t);
        if (t >= 1 || t <= 0)
        {
            if (t <= 0)
            {
                m_currentCount += 1;
            }
                
            if (pingPong || m_currentCount < m_count)
            {
                reverse = !reverse;
            }
            else {
                needMove = false;
            }
        }

    }

    void updateRectPosition()
    {
        p_currentLerpTime += !reverse ? Time.deltaTime : -Time.deltaTime;
        if (p_currentLerpTime > p_lerpTime)
            p_currentLerpTime = p_lerpTime;

        float t = p_currentLerpTime / p_lerpTime;
        t = Mathf.Sin(t * Mathf.PI * 0.5f);
        GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(fromRectPos, toRectPos, t);
        if (t >= 1 || t <= 0)
        {
            if (t <= 0)
            {
                m_currentCount += 1;
            }

            if (pingPong || m_currentCount < m_count)
            {
                reverse = !reverse;
            }
            else {
                needMoveUI = false;
            }
        }
    }


    void updateRotation(){
		r_currentLerpTime += Time.deltaTime;
		if(r_currentLerpTime > r_lerpTime)
			r_currentLerpTime = r_lerpTime;
			
		float t = r_currentLerpTime/r_lerpTime;
		t = Mathf.Sin(t * Mathf.PI * 0.5f);
		
		transform.rotation = Quaternion.Slerp(transform.rotation, toRot, t);
		if(t == 1)
			needRotate = false;
	}
	
	void updateScale(){
		s_currentLerpTime += !reverse ? Time.deltaTime : -Time.deltaTime;
		if(s_currentLerpTime > s_lerpTime)
			s_currentLerpTime = s_lerpTime;
		
		float t = s_currentLerpTime/s_lerpTime;

		transform.localScale = Vector3.Lerp(fromScale, toScale, t);
		if(t >= 1 || t <= 0) {
            if (t <= 0)
                s_currentCount += 1;
            if (pingPong || s_currentCount < s_count)
            {
				reverse = !reverse;
			} else {
				needScale = false;
			}
		}
			
	}
}
