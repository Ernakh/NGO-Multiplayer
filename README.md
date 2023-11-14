# NGO-Multiplayer
Jogo multiplayer básico com o Netcode for GameObjects (NGO) da Unity 

## PASSOS:
- Verificar a versão da Unity, tem que ser a 2021.3.10 ou mais recente
- No Package Manager, instalar o "NetCode for GameObjects"
- Criar um GameObject na cena e chama-lo de "NetworkManager"
- Nesse GO, vamos adicionar um componente da NetCode, o "NetworkManager"
- Volte no Package Manager e instale o "Multiplayer Tools"
- No componente "NetworkManager", altere o "Select transport" selecionando a opção "Unity Transport"
- Vamos criar um player
- No Player, colocar o script "NetworkObject"
- Criar um prefab do Player
- Vincular o prefab do player ao objeto "Player Prefab" do "NetworkManager"
- Criar nas Assets, um NetworkPrefabsList e vincular o player e os prefabs futuros nessa lista
- Vincular o NetworkPrefabsList ao "NetworkManager"
- Testar o jogo utilizando os botões do "NetworkManager" (HOST é server e client ao memso tempo)
- Crie um canva com um empty e chame de "NetworkManagerUI" e dentro dele, adicione 3 botões
- crie o script "NetworkManagerUI" e vincule ao "NetworkManagerUI"
- no script, coloque esse código:

    [SerializeField]
    private Button serverBtn;
    [SerializeField]
    private Button clientBtn;
    [SerializeField]
    private Button hostBtn;
	
- Na interface gráfica, vincule cada botão ao atributo adequado
- No script, adicione as seguintes bibliotecas:
	using Unity.Netcode;
	using UnityEngine;
	using UnityEngine.UI;
	
- Adicione esse método ao script:

private void Awake()
{
	serverBtn.onClick.AddListener(() =>
	{
		NetworkManager.Singleton.StartServer();
	});

	clientBtn.onClick.AddListener(() =>
	{
		NetworkManager.Singleton.StartClient();
	});

	hostBtn.onClick.AddListener(() =>
	{
		NetworkManager.Singleton.StartHost();
	});
}

- No "NetworkManager", vamos alterar o Log Level para Developer, para ter um log mais completo do que acontece no jogo
- Testar os botões, olhando a Console
- Em PlayerSettings, vamos mudar o Modo de tela para Windowed.
- Faça uma build do jogo
- Execute na Unity e a Build
- Faça o script do player herdar de NetworkBehavior
- Coloque esse código na primeira linha do método Update:
	if(!IsOwner)
	{
		return;
	}****
- No prefab do Player, vamos adicionar o componente NetworkTransform
- Desmarcar a informação desnecessária, aquela que não vmaos transmitir para todos os jogadores, aquilo que não alteramos por exemplo (escala)
- Execute para entender o problema (o client não se move, pq o server manda eme voltar pra posição)
- Vamos adicionar mais um componente via Package Manager: https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop.git?path=/Packages/com.unity.multiplayer.samples.coop#main
- No prefab do Player, remova o NetworkTransform, e inclua o ClientNetworkTransform
- Desmarcar a informação desnecessária, aquela que não vmaos transmitir para todos os jogadores, aquilo que não alteramos por exemplo (escala)
- Faça nova Build e teste
- No player, declare:
private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1);

- No Update, deixe assim:

void Update()
    {
        Debug.Log(OwnerClientId + " - randomNumber: " + randomNumber.Value);

        if(!IsOwner)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = Random.Range(0, 100);
        }
		...
		
- Teste, veja que o valor é alterado e compartilhado entre os jogadores, mas o Client não consegue alterar esse valor.
- Altere a declaração para: private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
- Teste e veja que o Client agora consegue alterar o valor
- Adicione esse método ao Player
   public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int valorAnterior, int novoValor) =>
        {
            Debug.Log(OwnerClientId + " - randomNumber: " + randomNumber.Value);
        };
    }
- OBS: Podemos fazer com structs
- OBS:

NetworkVariable:

Uso Principal: NetworkVariable é uma abordagem mais recente introduzida na arquitetura NetCode da Unity. É usada para sincronizar variáveis entre clientes na rede.

Vantagens:

Facilita a sincronização de dados simples, como números, booleanos, etc.
É eficiente para atualizações frequentes em variáveis simples.
Desvantagens:

Não é adequado para todas as situações, especialmente para lógicas mais complexas.
Pode não ser tão flexível quanto métodos RPC em alguns casos.
Métodos RPC:

Uso Principal: RPCs são métodos especiais que podem ser chamados remotamente em outros clientes na rede para realizar ações específicas.

Vantagens:

Maior flexibilidade para realizar ações complexas.
Pode ser usado para transmitir mensagens personalizadas entre clientes.
Desvantagens:

Menos eficiente para sincronização de dados simples e frequentes.
Requer uma gestão mais manual para sincronização de variáveis.


- Exemplo com métodos RPC
- Adicione esse método ao Player Script, esse roda apenas no servidor

	[ServerRpc]
    private void TestServerRpc()
    {
        Debug.Log("TestServerRpc " + OwnerClientId);
    }
	
- Coloque esse código no Update:
	if (Input.GetKeyDown(KeyCode.Y))
	{
		TestServerRpc();
	}
- Teste
- Altere:

	[ServerRpc]
    private void TestServerRpc(string message)
    {
        Debug.Log("TestServerRpc " + OwnerClientId + " : " + message);
    }
- Passe uma mensagem para o servidor no clicar em Y
- Teste
- Vamos criar o ClientRpc, onde o server manda mensagem apra os clientes

   [ClientRpc]
    private void TestClientRpc(string message)
    {
        Debug.Log("TestClientRpc " + OwnerClientId + " : " + message);
    }
	
- E usar a tecla U para chamar esse método

	if (Input.GetKeyDown(KeyCode.U))
	{
		TestClientRpc("Mandei mensagem para os Clientes!");
	}

- Vamos criar um objeto para começarmos a fazer spawn, como um cubo ou esfera
- Crie prefab desse(s) objeto(s)
- Adicione os NetworkObject component aos prefabs
- Adicione os prefabs no NetworkPrefabsList
- Adicione ao Player Script:
	    [SerializeField]
		private Transform spawnedObjectPrefab;
		private Transform spawnedObjectTransform;
		
- No Update, adicione:

	if (Input.GetKeyDown(KeyCode.I))
        {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Destroy(spawnedObjectTransform.gameObject);
        }
  
- Adicione ao player, o componente NetworkAnimator
- Arraste o componente Animator do Player para o atributo Animator do NetworkAnimator
- Crie um script chamado OwnerNetworkAnimator
- Deixe o script assim:

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}


- Substitua o NetworkAnimator pelo OwnerNetworkAnimator
- Arraste o componente Animator do Player para o atributo Animator do OwnerNetworkAnimator
- Teste

Referências:
https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.5/api/Unity.Netcode.html
https://www.youtube.com/watch?v=3yuBOB3VrCk

