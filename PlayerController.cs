using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    //命令的抽象基类
    abstract class Command
    {
        public virtual void Execute()
        {
        }
        public virtual void Undo()
        {
        }
    }

    //移动命令单元，其他操作也可以通过继承Command来实现
    class MoveUnit : Command
    {
        private Vector3 _lastDir = Vector3.zero;
        private Vector3 _newDir = Vector3.zero;
        private CharacterController _controller;
        private float _moveTime;

        public override void Execute()
        {
            _controller.Move(_newDir);
        }

        public override void Undo()
        {
            _controller.Move(_lastDir);
        }

        public void InitController(CharacterController cc)
        {
            _controller = cc;
        }

        public Vector3 NewDir
        {
            get { return _newDir; }
            set
            {
                if (_newDir != value)
                {
                    _newDir = value;
                }
            }
        }
        public Vector3 LastDir
        {
            get { return _lastDir;}
            set 
            {
                if (_lastDir != value)
                {
                    _lastDir = value;
                }
            }
        }

        public float MoveTime
        {
            get { return _moveTime; }
            set
            {
                if (_moveTime != value)
                {
                    _moveTime = value;
                }
            }
        }

    }


    private CharacterController _controller;
    private Vector3 _moveDelta = Vector3.zero;
    private List<MoveUnit> _moveList = new List<MoveUnit>();
    private Vector3 _startPos = Vector3.zero;

	void Start () 
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogWarning("[Start] _controller == null!");
        }
        _startPos = transform.localPosition;
	}
	
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            MoveUnit moveData = new MoveUnit();
            moveData.InitController(_controller);
            moveData.NewDir = input;
            moveData.LastDir= input * -1;                           //
            moveData.MoveTime = Time.realtimeSinceStartup;
            _moveList.Add(moveData);
            Debug.Log("_moveList.Add Data, newPos: " + input + ", lastPos: " + input * -1 + ", moveTime: " + Time.realtimeSinceStartup);
            _controller.Move(input);
        }

	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(50f, 50f, 100f, 20f), "Execute"))
        {
            StartCoroutine(OperateExecute());
        }

        if (GUI.Button(new Rect(50f, 80f, 100f, 20f), "Undo"))
        {
            StartCoroutine(OperateUndo());
        }

        if (GUI.Button(new Rect(50f, 110f, 100f, 20f), "Clear Data"))
        {
            _moveList.Clear();
        }
    }

    private IEnumerator OperateExecute()
    {
        int index = 0;
        MoveUnit data = null;
        float curTime = Time.realtimeSinceStartup;
        float startTime = Time.realtimeSinceStartup;

        transform.localPosition = _startPos;

        while (index < _moveList.Count)
        {
            data = _moveList[index];
            curTime = Time.realtimeSinceStartup;
            if (curTime - startTime >= data.MoveTime)
            {
                Debug.Log("OperateMove MoveTime: " + data.MoveTime + ", curTime: " + curTime + ", frameCount: " + Time.frameCount);
                data.Execute();
                ++index;
            }
            //解决while循环等待时卡死主线程的问题
            yield return null;
        }
    }

    private IEnumerator OperateUndo()
    {
        int index = _moveList.Count - 1;
        MoveUnit data = null;
        float lastTime = 0f;

        while (index >= 0)
        {
            data = _moveList[index];
            if(index == _moveList.Count - 1)
            {
                lastTime = 0f;
                Debug.Log("OperateUndo MoveTime: " + data.MoveTime + ", lastTime: " + lastTime + ", frameCount: " + Time.frameCount);
                data.Undo();
                --index;
                continue;
            }
            else
            {
                lastTime = _moveList[index + 1].MoveTime;
            }
            // 等待下一条命令与当前命令的执行间隔
            yield return new WaitForSeconds(lastTime - data.MoveTime);
            data.Undo();
            --index;
        }
    }
}
