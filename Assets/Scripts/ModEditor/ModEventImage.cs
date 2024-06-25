using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ModEventImage : MonoBehaviour
{
    [HideInInspector] string _eventImageURL;

    [SerializeField] Image _eventImage;
    [SerializeField] GameObject _eventUploadImageTip;
    [SerializeField] TMP_Text _eventUploadImageTipText;

    private EventCreatorManager _eventCreatorManager;
    private MapEditor _mapEditor;

    private string currentPicturePath;

    private void Awake()
    {
        _eventCreatorManager = FindObjectOfType<EventCreatorManager>();
        _mapEditor = FindObjectOfType<MapEditor>();
    }

    public void SetUp()
    {
        _eventImage.sprite = null;
        currentPicturePath = "";
        _eventUploadImageTip.SetActive(true);

        if (_eventImage.sprite == null) // Hasn't image
        {
            string path = Path.Combine(Application.persistentDataPath, "localMods", _mapEditor.nameInputField.text, "events", $"{_eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].id}", $"{_eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].id}.jpg");
            if (File.Exists(path))
            {
                _eventUploadImageTip.SetActive(false);

                Texture2D finalTexture = NativeGallery.LoadImageAtPath(path);

                _eventImage.sprite = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), Vector2.zero);
                _eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].texture = finalTexture;

                currentPicturePath = path;

                _eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].imagePath = path;

            }
            else
            {
                _eventUploadImageTip.SetActive(true);

                _eventUploadImageTipText.text = ReferencesManager.Instance.languageManager.GetTranslation("Editor.PictureHint");
            }
        }
        else // Has image
        {
            _eventUploadImageTip.SetActive(false);
        }
    }

    public void LoadPicture()
    {
        if (!string.IsNullOrEmpty(_mapEditor.nameInputField.text))
        {
            NativeGallery.GetImageFromGallery((path) =>
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture == null)
                {
                    _eventUploadImageTipText.text = $"Couldn't load texture from + {path}";
                    return;
                }

                string fileName = $"{_eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].id}.jpg";

                string eventImageOriginPath = Path.Combine(path);
                string eventImageDestinationPath = Path.Combine(Application.persistentDataPath, "localMods", _mapEditor.nameInputField.text, "events", $"{_eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].id}", fileName);

                _mapEditor.CreateFolder(Path.Combine(Application.persistentDataPath), "localMods");
                _mapEditor.CreateFolder(Path.Combine(Application.persistentDataPath, "localMods"), _mapEditor.nameInputField.text);
                _mapEditor.CreateFolder(Path.Combine(Application.persistentDataPath, "localMods", _mapEditor.nameInputField.text), "events");
                _mapEditor.CreateFolder(Path.Combine(Application.persistentDataPath, "localMods", _mapEditor.nameInputField.text, "events"), $"{_eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].id}");

                if (File.Exists(eventImageOriginPath))
                {
                    File.Copy(eventImageOriginPath, eventImageDestinationPath);
                }

                Texture2D finalTexture = NativeGallery.LoadImageAtPath(eventImageDestinationPath);

                _eventImage.sprite = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), Vector2.zero);
                _eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].texture = finalTexture;

                currentPicturePath = eventImageDestinationPath;

                _eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].imagePath = eventImageDestinationPath;
                this.SetUp();
            });
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.EnterModName"));
        }
    }

    public void RemovePicture()
    {
        if (File.Exists(currentPicturePath))
        {
            File.Delete(currentPicturePath);
        }

        _eventImage.sprite = null;
        SetUp();

        _eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].imagePath = "";
    }
}
