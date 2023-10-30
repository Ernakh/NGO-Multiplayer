using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public float velocidade = 5f;
    public float velocidadeRotacao = 180f;
    private Rigidbody rb;
    private Animator animator;

    [SerializeField]
    private Transform spawnedObjectPrefab;

    private Transform spawnedObjectTransform;

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //private NetworkVariable<MyCustomData> customData = new NetworkVariable<MyCustomData>(new MyCustomData { _int = 1, _bool = true}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int valorAnterior, int novoValor) =>
        {
            Debug.Log(OwnerClientId + " - randomNumber: " + randomNumber.Value);
        };

        //customData.OnValueChanged += (MyCustomData valorAnterior, MyCustomData novoValor) =>
        //{
        //    Debug.Log(OwnerClientId + " - " + novoValor._int);
        //};
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = Random.Range(0, 100);
            //customData.Value = new MyCustomData { _int = Random.Range(0, 100), _bool = false };
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestServerRpc("Mandei mensagem pro servidor!");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            TestClientRpc("Mandei mensagem para os Clientes!");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Destroy(spawnedObjectTransform.gameObject);
        }

        float movimentoHorizontal = Input.GetAxis("Horizontal");
        float movimentoVertical = Input.GetAxis("Vertical");

        Vector3 movimento = new Vector3(movimentoHorizontal, 0f, movimentoVertical);

        rb.velocity = movimento * velocidade;

        if (movimento != Vector3.zero)
        {
            Quaternion rotacaoDesejada = Quaternion.LookRotation(movimento);

            rb.rotation = Quaternion.Slerp(rb.rotation, rotacaoDesejada, Time.deltaTime * velocidadeRotacao);
        }

        //bool estaAndando = movimento.magnitude > 0.1f;
        //animator.SetBool("Walk", estaAndando);

        animator.SetFloat("Walk", movimento.magnitude);
    }

    [ServerRpc]
    private void TestServerRpc(string message)
    {
        Debug.Log("TestServerRpc " + OwnerClientId + " : " + message);
    }

    [ClientRpc]
    private void TestClientRpc(string message)
    {
        Debug.Log("TestClientRpc " + OwnerClientId + " : " + message);
    }
}
