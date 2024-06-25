using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

namespace UpgradeSystem
{

    struct GameData
    {
        public string Name;
        public string DescriptionRU;
        public string DescriptionEN;
        public string Description;
        public string Version;
        public string Url;
    }

    public class UpdateCheck : MonoBehaviour
    {

        [Header("## UI References :")]
        [SerializeField] TMP_Text uiDescriptionText;

        [Space(20f)]
        [Header("## Settings :")]
        [SerializeField][TextArea(1, 5)] string jsonDataURL;

        [SerializeField] Image buttonImage;
        [SerializeField] TMP_Text buttonText;

        private static bool isAlreadyCheckedForUpdates = false;

        private GameData latestGameData;

        [System.Obsolete]
        private void Start()
        {
            CheckUpdateButton();
        }

        [System.Obsolete]
        public void CheckUpdateButton()
        {
            StopAllCoroutines();
            StartCoroutine(CheckForUpdates());
        }

        [System.Obsolete]
        IEnumerator CheckForUpdates()
        {
            UnityWebRequest request = UnityWebRequest.Get(jsonDataURL);
            request.chunkedTransfer = false;
            request.disposeDownloadHandlerOnDispose = true;
            request.timeout = 10;

            buttonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.UpdateCheck.Connecting")}";

            buttonImage.color = Color.yellow;

            yield return request.Send();
            yield return new WaitForSeconds(0.1f);
            if (request.isDone)
            {
                isAlreadyCheckedForUpdates = true;

                buttonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.UpdateCheck.Checking")}";

                buttonImage.color = Color.cyan;
                yield return new WaitForSeconds(1f);

                if (!request.isNetworkError)
                {
                    latestGameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);

                    if (!string.IsNullOrEmpty(latestGameData.Version) && !Application.version.Equals(latestGameData.Version))
                    {
                        buttonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.UpdateCheck.NewUpdate")} <color=\"white\">{latestGameData.Version}</color>";
                        uiDescriptionText.text = ReferencesManager.Instance.languageManager.GetTranslation("UpdateCheck.NewUpdateFound");

                        buttonImage.color = Color.cyan;
                    }
                    else if(!string.IsNullOrEmpty(latestGameData.Version) && Application.version.Equals(latestGameData.Version))
                    {
                        uiDescriptionText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Version.NewVersion")}.\n<color=\"white\">{latestGameData.Version}</color>";

                        buttonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.VersionButton")} <color=\"white\">{latestGameData.Version}</color>";
                        buttonImage.color = Color.green;
                    }
                }
                else
                {
                    buttonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("Error")}";

                    buttonImage.color = Color.red;
                    Debug.LogError("An Error just occured by loading game version. Check the json url server and try again");
                }
            }

            request.Dispose();
        }


        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}