using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetupUIManager : MonoBehaviour
{
    public static SetupUIManager instance;
    public GameObject startMenu;
    public InputField usernameField;
    public Text errorMsg;
    public Toggle redColor;
    public Toggle greenColor;
    public Toggle blueColor;
    public Toggle yellowColor;
    public Toggle lightBlueColor;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>
    /// By clicking the connect button in the menu the game scene is loaded. (Index can be seen in the build settings)
    /// </summary>
    public void ConnectToServer() {
        if (string.IsNullOrEmpty(usernameField.text)) {
            errorMsg.text = "Please enter a name!";
        }
        else {
            startMenu.SetActive(false);
            usernameField.interactable = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
