using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

namespace UpgradeSystem
{

    struct GameData
    {
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

            buttonText.text = "����������� � �������...";
            buttonImage.color = Color.yellow;

            yield return request.Send();
            yield return new WaitForSeconds(1f);
            if (request.isDone)
            {
                isAlreadyCheckedForUpdates = true;
                buttonText.text = "�������� ����������...";
                buttonImage.color = Color.cyan;
                yield return new WaitForSeconds(1f);

                if (!request.isNetworkError)
                {
                    latestGameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(latestGameData.Version) && !Application.version.Equals(latestGameData.Version))
                    {
                        // new update is available
                        buttonText.text = $"����� ����������: <color=\"white\">{ latestGameData.Version }</color>";
                        uiDescriptionText.text = latestGameData.Description;
                        buttonImage.color = Color.cyan;
                    }
                    else if(!string.IsNullOrEmpty(latestGameData.Version) && Application.version.Equals(latestGameData.Version))
                    {
                        uiDescriptionText.text = $"� ��� ����������� ����� ��������� ������ ����.\n\n������� ������ ����: <color=\"white\">{ latestGameData.Version }</color>";
                        buttonText.text = $"���������� ����������: <color=\"white\">{ latestGameData.Version }</color>";
                        buttonImage.color = Color.green;
                    }
                }
                else
                {
                    buttonText.text = "��������� ������";
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