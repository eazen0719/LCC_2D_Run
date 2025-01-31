﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Dog : MonoBehaviour
{
    #region 欄位
    // 欄位 field (變數)
    // 修飾詞 欄位類型 欄位名稱 (指定 值) 結束
    // private 私人(不顯示) public 公開(顯示)
    [Header("跳躍次數"), Range(1, 10)]
    public int jumpCount = 2;
    [Header("跳躍高度")]
    public int jump = 200;
    [Header("移動速度"), Range(0, 15.5f)]
    public float speed = 10.5f;
    [Header("是否在地板上"), Tooltip("用來判定角色有沒有站在地板上。")]
    public bool isGround;                   // true 是、false 否
    [Header("角色名稱")]
    public string characterName = "KID";
    [Header("血量")]
    public float hp = 500;
    private float maxHp;
    public Image hpBar;
    [Header("障礙物傷害值")]
    public float damage = 20;
    [Header("道具")]
    public int countDiamond;
    public Text textDiamond;
    public AudioClip soundDiamond;
    public int countCherry;
    public Text textCherry;
    public AudioClip soundCherry;

    public AudioClip soundJump, soundSlide;

    [Header("拼接地圖")]
    public Tilemap tileProp;

    [Header("遺失血量大小")]
    public float lose = 1;

    private Transform cam;
    private Animator ani;            // 動畫控制器
    private CapsuleCollider2D cc2d;  // 膠囊碰撞器
    private Rigidbody2D r2d;         // 剛體
    private AudioSource aud;         // 音源
    private SpriteRenderer sr;       // 圖片渲染
    #endregion

    #region 事件
    // 初始事件：遊戲開始執行一次
    private void Start()
    {
        maxHp = hp;

        //print("哈囉，沃德~");
        // GetComponent<T>() 泛型方法<T>
        ani = GetComponent<Animator>();
        cc2d = GetComponent<CapsuleCollider2D>();
        r2d = GetComponent<Rigidbody2D>();
        aud = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();

        cam = GameObject.Find("Main Camera").transform;
    }

    // 更新事件：每一禎執行一次 60fps
    private void Update()
    {
        MoveDog();
        MoveCamera();
        HpLose();
    }

    // 碰撞事件：當物件碰撞開始時執行一次
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果 碰到 物件 名稱 等於 "地板"
        if (collision.gameObject.name == "地板")
        {
            isGround = true;
        }
        if (collision.gameObject.name == "道具")
        {
            EatCherry(collision);
        }
    }

    // 觸發事件：當物件觸發開始時執行一次 (有勾選 Is Trigger 的碰撞器)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "障礙物")
        {
            Damage();
        }
        if (collision.tag == "鑽石")
        {
            EatDiamond(collision);
        }
    }
    #endregion

    #region 方法
    /// <summary>
    /// 吃櫻桃
    /// </summary>
    /// <param name="collision">碰撞資訊</param>
    private void EatCherry(Collision2D collision)
    {
        Vector3 center = Vector3.zero;
        Vector3 point = collision.contacts[0].point;
        Vector3 normal = collision.contacts[0].normal;

        center.x = point.x - normal.x * 0.01f;
        center.y = point.y - normal.y * 0.01f;

        tileProp.SetTile(tileProp.WorldToCell(center), null);

        countCherry++;
        textCherry.text = countCherry.ToString();
        aud.PlayOneShot(soundCherry);
    }

    /// <summary>
    /// 吃鑽石。
    /// </summary>
    /// <param name="collision">碰撞資訊</param>
    private void EatDiamond(Collider2D collision)
    {
        Destroy(collision.gameObject);
        countDiamond++;
        textDiamond.text = countDiamond.ToString();
        aud.PlayOneShot(soundDiamond);
    }

    /// <summary>
    /// 角色受傷
    /// </summary>
    private void Damage()
    {
        //Debug.Log("受傷!!!");
        sr.enabled = false;
        Invoke("ShowSprite", .1f);  // 延遲調用("方法名稱"，延遲時間)

        //hp = hp - damage;
        hp -= damage;
        hpBar.fillAmount = hp / maxHp;
        Dead();
    }

    /// <summary>
    /// 顯示圖片
    /// </summary>
    private void ShowSprite()
    {
        sr.enabled = true;
    }

    /// <summary>
    /// 狗移動方法
    /// </summary>
    private void MoveDog()
    {
        // 物件.移動(x, y, z);
        // Time.delta 為裝置一禎的時間
        //dog.Translate(speed * Time.deltaTime, 0, 0);
        transform.Translate(speed * Time.deltaTime, 0, 0);
    }

    /// <summary>
    /// 攝影機移動方法
    /// </summary>
    private void MoveCamera()
    {
        cam.Translate(speed * Time.deltaTime, 0, 0);
    }

    /// <summary>
    /// 跳躍方法
    /// </summary>
    public void Jump()
    {
        // 如果 在地板上布林值 等於 勾選
        if (isGround == true)
        {
            //print("跳躍!");
            ani.SetBool("跳躍開關", true);
            r2d.AddForce(new Vector2(0, jump)); // 剛體.推力(二維向量)
            isGround = false;                   // 地板布林值 = 取消
            aud.PlayOneShot(soundJump);
        }
    }

    /// <summary>
    /// 滑行方法，縮小設定碰撞器
    /// </summary>
    public void Slide()
    {
        //print("滑行");
	    ani.SetBool("滑行開關", true);
        cc2d.offset = new Vector2(-0.1f, -1.1f);
        cc2d.size = new Vector2(0.95f, 0.9f);
        aud.PlayOneShot(soundSlide, 3);
    }

    /// <summary>
    /// 重新設定跳躍與滑行布林值，重新設定碰撞器
    /// </summary>
    public void ResetAnimator()
    {
        ani.SetBool("跳躍開關", false);
        ani.SetBool("滑行開關", false);

        cc2d.offset = new Vector2(-0.1f, -0.25f);
        cc2d.size = new Vector2(0.95f, 2.5f);
    }

    /// <summary>
    /// 遺失血量
    /// </summary>
    private void HpLose()
    {
        hp -= Time.deltaTime * lose;
        hpBar.fillAmount = hp / maxHp;
        Dead();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        if (hp <= 0)
        {
            speed = 0;
            ani.SetBool("死亡開關", true);
        }
    }
    #endregion
}