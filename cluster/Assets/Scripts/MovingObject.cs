using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{

    static public MovingObject instance;

    // A ĳ���� instance = a
    // B ĳ���� instance = a

    public string currentMapName; // transferMap ��ũ���� �ִ� transferMapName ������ ���� ����.

    private BoxCollider2D boxCollider;
    public LayerMask layerMask;

    public float speed; // ĳ���͵��� �������� ����� ���� ����

    private Vector3 vector; // 3���� ���� ���ÿ� ������ �ִ� ����(x, y, z��)

    public float runSpeed;
    private float applyRunSpeed; // Shift Ű�� ���� ��쿡�� ����. ���� ����Ǵ� ��.
    private bool applyRunFlag = false;

    public int walkCount;
    private int currentWalkCount;

    // speed = 2.4, walkCount = 20
    // 2.4 * 20 = 48. �� �� ����Ű�� ���� ������ 48px��ŭ �̵�
    // While������ ������������ current �ʿ�
    // currentWalkCount += 1 , 20,

    private bool canMove = true;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            boxCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    IEnumerator MoveCoroutine()
    {
        while (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            // �����¿� ����Ű�� ������ �� �Ʒ� ���� ����

            if (Input.GetKey(KeyCode.LeftShift))
            {
                applyRunSpeed = runSpeed;
                applyRunFlag = true;
            }
            else
            {
                applyRunSpeed = 0; // ������ �� ��
                applyRunFlag = false;
            }


            vector.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), transform.position.z); // vector�� ���� �� ����(x, y, z). z ���� �ٲ��� ����

            if (vector.x != 0)
                vector.y = 0;

            // vector.x = 1 : �������� �̵�
            // vector.y = 0; y ������ �ʿ� ����

            animator.SetFloat("DirX", vector.x);
            animator.SetFloat("DirY", vector.y);

            RaycastHit2D hit;
            // A����, B����
            // ������
            // hit = Null;
            // hit = ���ع�

            Vector2 start = transform.position; // A����, ĳ������ ���� ��ġ ��
            Vector2 end = start + new Vector2(vector.x * speed * walkCount, vector.y * speed * walkCount); // B����, ĳ���Ͱ� �̵��ϰ��� �ϴ� ��ġ ��

            boxCollider.enabled = false;
            hit = Physics2D.Linecast(start, end, layerMask);
            boxCollider.enabled = true;

            if (hit.transform != null)
                break;

            animator.SetBool("Walking", true);

            while (currentWalkCount < walkCount)
            {
                // vector ���� ���� �����̱�
                if (vector.x != 0) // vector�� ����� x���� 0�� �ƴ� ���
                {
                    transform.Translate(vector.x * (speed + applyRunSpeed), 0, 0); // Vector.x * speed: Vector.x�� ���� +-1�� ���� �ǹǷ� +-2.4�� ��

                    // transform.position = vector; // �����̴� �ٸ� ���
                }
                else if (vector.y != 0)
                {
                    transform.Translate(0, vector.y * (speed + applyRunSpeed), 0); // Translate �Լ�: ���� �ִ� ������ ��ġ��ŭ �����ִ� ��
                }
                if (applyRunFlag)
                    currentWalkCount++; // �� ȸ�� �� ������ current 1�� ����. shift�� ������ 2�� ����
                currentWalkCount++;
                yield return new WaitForSeconds(0.01f); // �ڷ�ƾ �⺻ ����. ���� ó�� ����. �ݺ����� 20���̸� 0.2�� ���
            }
            currentWalkCount = 0; // �ݺ������� ���������� �ٽ� 0���� �ʱ�ȭ


        }
        animator.SetBool("Walking", false);
        canMove = true; // �ڷ�ƾ�� �Ϸ�Ǹ� �ٽ� ����Ű�� ���� �� �ְ� true�� ����
    }



    // Update is called once per frame. �� �����Ӹ��� �Լ��� ����
    void Update()
    {
        if (canMove) // �ߺ� ���� ����.
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) // Horizontal(����): �� ����Ű 1 ����, �� ����Ű -1 ����. Vertical(����): �� ����Ű 1 ����, �� ����Ű -1 ����.
            {
                canMove = false; // ����Ű�� ���� ���� canMove = false
                StartCoroutine(MoveCoroutine()); // �ڷ�ƾ�� �����Ű�� ��ɾ�.
            }
        }

    }
}