using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PersonView { FirstPerson, ThirdPerson };

public class GameManager : MonoBehaviour {

    static public PersonView personView = PersonView.ThirdPerson;

    public Maze mazePrefab;
    private Maze mazeInstance;
    public Player playerPrefab;
    private Player playerInstance;
    private int mission = 0;
    private string serverSendUrl = "http://www.acwing.com:8080/maze/send/";
    private string serverBringUrl = "http://www.acwing.com:8080/maze/bring/";
    private string userid;

    private Maze_engine mazeEngine;

	// Use this for initialization
	void Start ()
    {
#if !UNITY_STANDALONE_WIN
        userid = SystemInfo.deviceUniqueIdentifier;
        //userid = "Android";
#else
        userid = "Windows10";
#endif

        StartCoroutine(CreateMazeEngine());
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Z))
        {
            personView = PersonView.FirstPerson;
            RestartGame();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            personView = PersonView.ThirdPerson;
            RestartGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}

    private IEnumerator CreateMazeEngine()
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", userid);
        WWW serverInfo = new WWW(serverBringUrl, form);

        yield return serverInfo;

        if (serverInfo.error != null)
        {
            Debug.LogError(serverInfo.error);
            mazeEngine = new Maze_engine();
        }
        else if (serverInfo.text == "UserNotExist!")
        {
            mazeEngine = new Maze_engine();
            Debug.Log("UserNotExist!");
        }
        else
        {
            var items = serverInfo.text.Split('#');
            mazeEngine = new Maze_engine(float.Parse(items[0]));
            mission = int.Parse(items[1]);
        }

        BeginGame();
    }

    private void BeginGame()
    {
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.Win(mission);
        if (personView == PersonView.FirstPerson)
        {
            mazeInstance.Generate();
            playerInstance = Instantiate(playerPrefab) as Player;
            playerInstance.name = "Player";
            playerInstance.SetLocation(mazeInstance.GetCell(mazeInstance.RandomCoordinates));
            Camera.main.clearFlags = CameraClearFlags.Depth;
            Camera.main.rect = new Rect(0f, 0.5f, 0.5f, 1f);
        }
        else if (personView == PersonView.ThirdPerson)
        {
            mazeInstance.GenerateAlgorithm(mazeEngine);
            playerInstance = Instantiate(playerPrefab) as Player;   
            playerInstance.SetLocation(mazeInstance.GetCell(new IntVector2(0, 0)));
            //Camera.main.transform.position = new Vector3(0f, 26f, -13f);
            //Camera.main.transform.rotation = Quaternion.Euler(64f, 0f, 0f);
            Camera.main.rect = new Rect(0f, 0f, 01f, 1f);
        }
    }
    private void RestartGame()
    {
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        if (playerInstance != null)
        {
            Destroy(playerInstance.gameObject);
        }
        BeginGame();
    }

    public void EndGame()
    {
        mazeInstance.Win( ++ mission);
        playerInstance.GetComponent<Player>().enabled = false;

        StartCoroutine(FinishGame());
    }

    private IEnumerator FinishGame()
    {
        yield return StartCoroutine(ReportInformation());

        RestartGame();
    }

    private Information CollectInformation()
    {
        mazeEngine.finish_maze(playerInstance.GetRouteLength, mazeInstance.GetDurationTime);

        Information information = new Information();
        information.userid = userid;
        information.width = mazeInstance.size.x;
        information.height = mazeInstance.size.z;
        information.routeLength = playerInstance.GetRouteLength;
        information.totalTime = mazeInstance.GetDurationTime;
        information.stayTime = playerInstance.stayTime;
        information.playerMaxSpeed = playerInstance.m_Speed;
        information.version = "1.0.0";
        information.calculatedDifficulty = 0f;
#if !UNITY_STANDALONE_WIN
        information.platform = "Android";
#else
        information.platform = "Windows 10";
#endif
        information.errorTimes = 0;
        information.userAbility = (float)mazeEngine.user_ability;

        return information;
    }

    private IEnumerator ReportInformation()
    {
        Information newInformation = CollectInformation();

        WWWForm form = new WWWForm();
        form.AddField("userid", newInformation.userid);
        form.AddField("width", newInformation.width);
        form.AddField("height", newInformation.height);
        form.AddField("routeLength", newInformation.routeLength.ToString());
        form.AddField("totalTime", newInformation.totalTime.ToString());
        form.AddField("stayTime", newInformation.stayTime.ToString());
        form.AddField("playerMaxSpeed", newInformation.playerMaxSpeed.ToString());
        form.AddField("calculatedDifficulty", newInformation.calculatedDifficulty.ToString());
        form.AddField("version", newInformation.version);
        form.AddField("platform", newInformation.platform);
        form.AddField("errorTimes", newInformation.errorTimes);
        form.AddField("userAbility", newInformation.userAbility.ToString());

        WWW serverInfo = new WWW(serverSendUrl, form);

        yield return serverInfo;

        //mazeInstance.canvas.winText.text = serverInfo.text;

        if (serverInfo.error != null)
        {
            Debug.LogError(serverInfo.error);
        }
        else if (serverInfo.text == "Successfully!")
        {
            Debug.Log("Report successfully!");
        }
        else
        {
            Debug.LogError("Report error!");
        }
    }
}
