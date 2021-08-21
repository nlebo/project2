﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : StateInfo
{
    int GroundLayerMask;
    bool isRide;
    bool ActionBtn;
    [SerializeField]
    bool ActionBtnDwn;
    float ActionBtnPressTime;

    bool MouseLeft;
    float MouseLBtnPressTime;

    bool b_CanMove;
    bool b_CanRoateCam;

    bool b_InvenOn;

    bool isSprint;
    bool SprintBtn;
    public float SprintSpeed = 2f;

    [SerializeField]
    bool isJump;
    bool JumpBtn;
    [SerializeField]
    bool IsJumped;
    public float JumpForce = 5;
    private float JumpNow = 0;
    Vector3 MoveStateWhenJump;
    float MoveSpeedWhenJump = 2f;

    
    bool BoxOn;
    Box OpenBox;

    bool InvenOn;
    bool InvenBtnDown;

    Cart RideCart;

    bool GrabHandle;

    public float MoveSpeed = 5f;
    float RMoveSpeed;
    
    Animator Anim;
    CharacterController CController;
    Vector3 MoveState;
    CapsuleCollider Col;
    UI_Manager UIS;

    [SerializeField]
    SelectBox _SelectBox;

    Inven_Manager InvenM;
    ItemBoard ItemB;

    [SerializeField]
    float DigTime;
    // Start is called before the first frame update
    void Start()
    {
        Anim = GetComponent<Animator>();
        CController = GetComponent<CharacterController>();
        Col = GetComponent<CapsuleCollider>();
        ActionBtn = false;
        isRide = false;
        ActionBtnPressTime = 0;
        UIS = UI_Manager.m_UI_Manager;
        OpenBox = null;
        GroundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        b_CanMove = true;
        b_CanRoateCam = true;
        b_InvenOn = false;
        InvenM = GetComponent<Inven_Manager>();
        ItemB = GameObject.Find("ItemBoard").GetComponent<ItemBoard>();
        SetInven(2,4);
    }

    // Update is called once per frame
    void Update()
    {
        InputManager();
        MouseLBTN();
        
        
        ActionBTN();
        CheckGround();
        
        Inven();
        
    }

    private void FixedUpdate() {
        
        if(!isRide && !b_InvenOn)
            Move();

        AnimationSet();
        MoveState = Vector3.zero;
    }

    void InputManager()
    {
        if(Input.GetKey(KeyCode.W)) MoveState.z = 1;
        if(Input.GetKey(KeyCode.S)) MoveState.z = -1;
        if(Input.GetKey(KeyCode.A)) MoveState.x = -0.5f;
        if(Input.GetKey(KeyCode.D)) MoveState.x = 0.5f;

        MouseLeft = Input.GetMouseButton(0);

        MoveState.Normalize();

        ActionBtn = Input.GetKey(KeyCode.F);
        ActionBtnDwn = Input.GetKeyDown(KeyCode.F);

        SprintBtn = Input.GetKey(KeyCode.LeftShift);

        JumpBtn = !isJump && Input.GetKey(KeyCode.Space);

        InvenBtnDown = Input.GetKeyDown(KeyCode.Tab);
        CameraMove.CanSwing = Input.GetKey(KeyCode.LeftAlt);
    }

    void Move()
    {
        float MouseX = Input.GetAxis("Mouse X");
        RMoveSpeed = MoveSpeed;

        isSprint = false;
        Anim.SetBool("Move",false);

        if(GrabHandle)
        {
            if (MoveState.x > 0)
            {
                RideCart.Handling(-1);
                if (RideCart.DegreeAceleation < 1) RideCart.DegreeAceleation += 0.5f * Time.deltaTime;
                if (RideCart.DegreeAceleation > 1) RideCart.DegreeAceleation = 1;

            }
            else if (MoveState.x < 0)
            {
                RideCart.Handling(1);
                if (RideCart.DegreeAceleation < 1) RideCart.DegreeAceleation += 0.5f * Time.deltaTime;
                if (RideCart.DegreeAceleation > 1) RideCart.DegreeAceleation = 1;
            }
            else
            {
                if(RideCart.DegreeAceleation>0)
                    RideCart.DegreeAceleation -= 1 * Time.deltaTime;
                else if(RideCart.DegreeAceleation< 0)
                    RideCart.DegreeAceleation = 0;
            }
            transform.position = Vector3.SqrMagnitude(RideCart.LeftSide.position - transform.position) <= Vector3.SqrMagnitude(RideCart.RightSide.position - transform.position) ? 
             RideCart.LeftSide.position : RideCart.RightSide.position;
            MoveState = Vector3.zero;
            CameraMove.CanSwing = true;
            return;
        }

        if (!CameraMove.CanSwing) transform.Rotate(Vector3.up * MouseX);
        
        if (!isJump && JumpBtn)
        {
            if (Anim.GetCurrentAnimatorStateInfo(3).IsName("Start") && Anim.GetCurrentAnimatorStateInfo(3).normalizedTime >= 0.1f)
            {
                isJump = true;
                JumpNow = JumpForce;
                MoveStateWhenJump = MoveState;
            }
        }
        else if (isJump)
        {
            JumpNow -= 5 * Time.deltaTime;
            JumpNow = JumpNow < 0 ? 0 : JumpNow; 
            CController.Move(Vector3.up * JumpNow * Time.deltaTime);
            Jump();
        }

        if (MoveState == Vector3.zero && !isJump)
        {
            return;
        }
        else
        {
            Anim.SetBool("Move",true);
        }

        if (MoveState.z > 0 && SprintBtn)
        {
            isSprint = true;
            RMoveSpeed += SprintSpeed;
        }

        
        
        if(!isJump)
        {
            
            //transform.Translate(MoveState * Time.deltaTime * RMoveSpeed);
            Quaternion CRot = Quaternion.Euler(0,transform.eulerAngles.y,0);
            MoveState = CRot * MoveState;
            CController.Move(MoveState * RMoveSpeed * Time.deltaTime);
        }
        else if(IsJumped && MoveStateWhenJump == Vector3.zero)
        {
            CController.Move(MoveState * Time.deltaTime * (RMoveSpeed - MoveSpeedWhenJump - 2));
        }
        else
        {
            //transform.Translate(MoveStateWhenJump * Time.deltaTime * (RMoveSpeed - MoveSpeedWhenJump));
            Quaternion CRot = Quaternion.Euler(0,transform.eulerAngles.y,0);
            MoveState = CRot * MoveState;
            CController.Move(MoveState * RMoveSpeed * Time.deltaTime);
        }
    }

    public void Jump()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position, -transform.up, 10, GroundLayerMask);
        Debug.DrawRay(transform.position, -transform.up, Color.green, 0.2f);

        if (hit.Length >= 1)
        {
            float Dis = 0;

            for(int i = 0, loop = hit.Length; i <loop ; i++)
            {
                if(i == 0 || Dis > Vector3.Distance(transform.position,hit[i].point))
                    Dis = Vector3.Distance(transform.position,hit[i].point);
            }

            if (Anim.GetInteger("JumpState") == 0)
            {
                if (Dis < 1)
                {
                    Anim.SetInteger("JumpState", 1);
                }
            }
            else
            {
                if (Dis >= 1)
                {
                    IsJumped = true;
                    Anim.SetInteger("JumpState", 0);

                }
            }

            if (Dis <= 0.2f && IsJumped)
            {
                IsJumped = false;
                MoveStateWhenJump = Vector3.zero;
            }
            if (Anim.GetCurrentAnimatorStateInfo(3).IsName("ions@JumpEnd01umpEnd01") && Anim.GetCurrentAnimatorStateInfo(3).normalizedTime >= 0.9f) isJump = false;
        }
    }

    public void AnimationSet()
    {
        Anim.SetLayerWeight(2, 0);
        Anim.SetLayerWeight(2, 0);
        Anim.SetLayerWeight(3,0);

        if (isJump)
        {
            Anim.SetInteger("State", 3);
            Anim.SetLayerWeight(3,1);
        }

        else
        {
            if (MoveState == Vector3.zero)
            {
                Anim.SetInteger("State", 0);
            }
            else if (MoveState != Vector3.zero)
            {
                Anim.SetInteger("State", 1);
                Anim.SetFloat("WalkBlend", -MoveState.z + 0.5f);
                Anim.SetLayerWeight(1, 1);

                if (isSprint)
                {
                    Anim.SetInteger("State", 2);
                    Anim.SetLayerWeight(2, 1);
                }
            }
        }
    }

    void ActionBTN()
    {
        if(BoxOn && Vector3.Distance(OpenBox.transform.position,transform.position) > 5)
        {
            UIS.Box_2X2.gameObject.SetActive(false);
            BoxOn = false;
            OpenBox = null;
        }

        if(!ActionBtn)
        {
            ActionBtnPressTime = 0;
            return;
        }


        Ride();
        Box_2x2();
        OnCart();
        Handle();
        PickUpItem();
        
    }

    void MouseLBTN()
    {
        if(Input.GetMouseButtonUp(0))
        {
            MouseLBtnPressTime = 0;
            UIS.Dig_Bar.transform.parent.gameObject.SetActive(false);
        }
        
        if(!MouseLeft) return;

        

        if(_SelectBox.PlaneOn && InvenM.IsEquip(1)) Dig();
        else{
            UIS.Dig_Bar.transform.parent.gameObject.SetActive(false);
            MouseLBtnPressTime = 0;
        }

    }

    void Ride()
    {
        if(!_SelectBox.RideOn)
        {
            if(isRide && ActionBtnDwn)
            {
                Col.isTrigger = false;
                isRide= false;
                transform.parent = null;
                transform.localPosition = new Vector3(transform.localPosition.x,0,transform.localPosition.z);
                return;
            }
            else if(isRide)
            {
                return;
            }
        }

        else
        {

            ActionBtnPressTime += Time.deltaTime;

            if (ActionBtnPressTime >= 2.2f)
            {
                Col.isTrigger = true;
                transform.parent = _SelectBox.RideTransform;
                transform.localPosition = Vector3.zero;
                isRide = true;
            }
        }
    }

    void Box_2x2()
    {
        if(!ActionBtnDwn) return;

        if(!_SelectBox.BoxOn || BoxOn)
        {
            UIS.Box_2X2.gameObject.SetActive(false);
            BoxOn = false;
            OpenBox = null;
            return;
        }
        else
        {
            BoxOn = true;
            UIS.Box_2X2.gameObject.SetActive(true);
            OpenBox = _SelectBox._Box;
        }
    }

    void Inven()
    {
        if(InvenBtnDown)
        {
            UIS.UserInven.gameObject.SetActive(!InvenOn);
            InvenOn = !InvenOn;
            b_InvenOn = !b_InvenOn;

            Cursor.visible = b_InvenOn;

            if (b_InvenOn)
            {
                Cursor.lockState = CursorLockMode.None;
                CameraMove.b_CanSwingCamera = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                CameraMove.b_CanSwingCamera = true;
            }
        }
    }

    void OnCart()
    {
        if(ActionBtnDwn && _SelectBox.MotorOn) _SelectBox._Cart.PushEngine();
    }

    void Handle()
    {
        if(!ActionBtnDwn || (!_SelectBox.HandleOn && !GrabHandle)) return;
        
        GrabHandle = !GrabHandle;
        if(GrabHandle)
        {
             RideCart = _SelectBox._Cart;
             transform.position = Vector3.SqrMagnitude(RideCart.LeftSide.position - transform.position) <= Vector3.SqrMagnitude(RideCart.RightSide.position - transform.position) ? 
             RideCart.LeftSide.position : RideCart.RightSide.position;
        }
        else RideCart = null;
    }
    
    void PickUpItem()
    {
        if(!ActionBtnDwn || !_SelectBox.ItemOn) return;

        InvenM.InsertItem(_SelectBox._Item);
        
        Destroy(_SelectBox._Item.gameObject);
        _SelectBox._Item = null;
        _SelectBox.ItemOn = false;
    }
    
    void Dig()
    {

        if(GetStamina() < 0.5f) return;

        if(Input.GetMouseButtonDown(0)){
            MouseLBtnPressTime = 0;
            UIS.Dig_Bar.transform.parent.gameObject.SetActive(true);
        }

        MouseLBtnPressTime += Time.deltaTime;

        UIS.ChangeBarValue("Dig", MouseLBtnPressTime / DigTime);

        if(MouseLBtnPressTime >= DigTime)
        {
            MouseLBtnPressTime = 0;
            int DigPercent = Random.Range(0,101);

            if(DigPercent<=90) InvenM.InsertItem(ItemB.Items[1]);

            if(DigPercent > 80 && DigPercent <= 90) InvenM.InsertItem(ItemB.Items[1]);
            UseStamina(10f);
            UIS.ChangeBarValue("Stamina",GetStamina() / 100);
        }
    }
    
    void CheckGround()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position,-transform.up,1,GroundLayerMask);
        Debug.DrawRay(transform.position,-transform.up,Color.green,0.2f);

        transform.parent = null;

        for(int i =0 , loop = hit.Length;i<loop;i++)
        {
            if(transform.parent == null || Vector3.Distance(transform.position,hit[i].point) <= Vector3.Distance(transform.position, transform.parent.position))
                transform.parent = hit[i].transform;
        }

    }

    void OnDrawGizmos() {
        // RaycastHit[] hit = Physics.RaycastAll(transform.position,-transform.up,1,GroundLayerMask);
        // if(hit.Length >= 1){
        //     Gizmos.DrawSphere(hit[hit.Length - 1].point , 1f);
        //     Gizmos.color = Color.red;
        // }
    }

    void SetInven(int x,int y)
    {
        InvenM.Set_Grid(y,x);
    }

}
