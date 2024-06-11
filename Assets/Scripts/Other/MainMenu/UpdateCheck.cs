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
            yield return new WaitForSeconds(1f);
            UnityWebRequest request = UnityWebRequest.Get(jsonDataURL);
            request.chunkedTransfer = false;
            request.disposeDownloadHandlerOnDispose = true;
            request.timeout = 10;

            if (PlayerPrefs.GetInt("languageId") == 1)
            {
                buttonText.text = "Подключение к серверу...";
            }
            else if (PlayerPrefs.GetInt("languageId") == 0)
            {
                buttonText.text = "Connecting to the server...";
            }
            buttonImage.color = Color.yellow;

            yield return request.Send();
            yield return new WaitForSeconds(1f);
            if (request.isDone)
            {
                isAlreadyCheckedForUpdates = true;

                if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    buttonText.text = "Проверка обновления...";
                }
                else if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    buttonText.text = "Checking updates...";
                }

                buttonImage.color = Color.cyan;
                yield return new WaitForSeconds(1f);

                if (!request.isNetworkError)
                {
                    latestGameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);

                    if (!string.IsNullOrEmpty(latestGameData.Version) && !Application.version.Equals(latestGameData.Version))
                    {
                        // new update is available
                        if (PlayerPrefs.GetInt("languageId") == 1)
                        {
                            buttonText.text = $"Новое обновление: <color=\"white\">{latestGameData.Version}</color>";
                            uiDescriptionText.text = latestGameData.DescriptionRU;
                        }
                        else if (PlayerPrefs.GetInt("languageId") == 0)
                        {
                            buttonText.text = $"New update: <color=\"white\">{latestGameData.Version}</color>";
                            uiDescriptionText.text = latestGameData.DescriptionEN;
                        }

                        buttonImage.color = Color.cyan;
                    }
                    else if(!string.IsNullOrEmpty(latestGameData.Version) && Application.version.Equals(latestGameData.Version))
                    {
                        if (PlayerPrefs.GetInt("languageId") == 1)
                        {
                            uiDescriptionText.text = $"У Вас установлена самая последняя версия игры.\n\nТекущая версия игры: <color=\"white\">{latestGameData.Version}</color>";
                        }
                        else if (PlayerPrefs.GetInt("languageId") == 0)
                        {
                            uiDescriptionText.text = $"You have the latest version of the game installed.\n\nCurrent game version: <color=\"white\">{latestGameData.Version}</color>";
                        }

                        if (PlayerPrefs.GetInt("languageId") == 1)
                        {
                            buttonText.text = $"Актуальное обновление: <color=\"white\">{latestGameData.Version}</color>";
                        }
                        else if (PlayerPrefs.GetInt("languageId") == 0)
                        {
                            buttonText.text = $"Current version: <color=\"white\">{latestGameData.Version}</color>";
                        }
                        buttonImage.color = Color.green;
                    }
                }
                else
                {
                    if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        buttonText.text = "Произошла ошибка";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        buttonText.text = "An error has occurred";
                    }
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