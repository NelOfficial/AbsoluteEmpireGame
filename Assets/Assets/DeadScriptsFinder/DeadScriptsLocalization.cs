﻿// *********************************************************************************
// The MIT License (MIT)
// Copyright (c) 2020 SpiralBlack https://github.com/SpiralBlack15
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// *********************************************************************************
namespace Spiral.EditorToolkit.DeadScriptsSearcher
{
    public static class DeadScriptLocalization
    {
        // SCENE FILE -----------------------------------------------------------------------------
        public readonly static LocalString strSceneFile_ReuploadCurrentSceneText = new LocalString(
            // ru
            "Синхронизировать текст сцены с её файлом",
            // en
            "Re-upload scene text");

        // DEAD SCRIPT SEARCHER EDITOR WINDOW -----------------------------------------------------
        public readonly static LocalString strDeadScriptSearcher_Caption = new LocalString(
            // ru
            "Мёртвые скрипты",
            // en
            "Dead Scripts");
        public readonly static LocalString strDeadScriptSearcher_DebugMode = new LocalString(
            // ru
            "Режим отладки",
            // en
            "Debug mode");
        public readonly static LocalString strDeadScriptSearcher_DebugModeHelp = new LocalString(
            // ru
            "Режим отладки будет выводить в консоль все действия, что может " +
            "существенно замедлить осмотр сцен с большим количеством объектов",
            // en
            "Debug mode will output all actions to the console. This can " +
            "significantly slow down the checkup of scenes " +
            "with a large number of objects");

        // MONO VIEW EDITOR WINDOW ----------------------------------------------------------------
        public readonly static LocalString strMonoView_Caption = new LocalString(
            // ru
            "Ревизор",
            // en
            "Auditor");
        public readonly static LocalString strMonoView_SelectObject = new LocalString(
            // ru
            "Выделите объект(ы) в инспекторе сцены, чтобы посмотреть данные",
            // en
            "Select object(s) in scene inspector to view its/their data");
        public readonly static LocalString strMonoView_ShowObjectInfo = new LocalString(
            // ru
            "Развернуть список компонент",
            // en
            "Show components list");
        public readonly static LocalString strMonoView_HideObjectInfo = new LocalString(
            // ru
            "Свернуть список компонент",
            // en
            "Hide components list");
        public readonly static LocalString strObjectsOnly = new LocalString(
            // ru
            "Только объекты",
            // en
            "Objects only:"
            );
        public readonly static LocalString strObjectsOnlyButton = new LocalString(
            // ru
            "Найти и выделить объекты с мёртвыми скриптами",
            // en
            "Find and select objects with dead scripts"
            );
        public readonly static LocalString strSceneFileCheckout = new LocalString(
            // ru
            "Проверка файла сцены:",
            // en
            "Scene file checkout:"
            );
        public readonly static LocalString strSceneWasChanged = new LocalString(
           // ru
           "СЦЕНА БЫЛА ИЗМЕНЕНА (сохраните изменения)",
           // en
           "SCENE WAS CHANGED (save changes first)"
           );
        public readonly static LocalString strSceneClear = new LocalString(
           // ru
           "СЦЕНА СОХРАНЕНА",
           // en
           "SCENE SAVED"
           );
        public readonly static LocalString strShowHelp = new LocalString(
            // ru
            "Показать/Скрыть справку",
            // en
            "Show/Hide help");
        public readonly static LocalString strSceneHelpWarning = new LocalString(
            // ru
            "Убедитесь, что сцена была сохранена!\n" +
            "Для сцен с большим количеством объектов поиск может идти медленно.\n" +
            "Если объектов больше сотни, это может занять время.",
            // en
            "Make sure the scene has been saved!\n" +
            "For scenes with a large number of objects, the search may go slowly.\n" +
            "If there are more than a hundred objects, this may take some time.");
        public readonly static LocalString strSceneHelpExplanation = new LocalString(
            // ru
            "Поиск идёт по файлу сцены, сопоставляя объекты с " +
            "битыми скриптами с их записями в файле. " +
            "Обратите внимание, что из поиска исключены скрипты, " +
            "не являющиеся MonoBehaviour, а также дочерние объекты в составе префабов.",
            // en
            "The search goes through the scene file, matching objects with broken scripts " +
            "with their entries in the file. " +
            "Note that scripts that are not MonoBehaviour, as well as child objects in prefabs, " +
            "are excluded from the search.");
        public readonly static LocalString strFindDeadGUIDs = new LocalString(
            // ru
            "Найти мёртвые GUID в сцене",
            // en
            "Find dead GUIDs in the scene");
        public readonly static LocalString strFoundGUIDs = new LocalString(
            // ru
            "Найдено уникальных GUID: ",
            // en
            "Unique GUIDs found: "
            );
        public readonly static LocalString strShowList = new LocalString(
            // ru
            "Показать/Скрыть список",
            // en
            "Show/Hide list"
            );
        public readonly static LocalString strDeadObjectsCount = new LocalString(
            // ru
            "Объектов",
            // en
            "Objects count");
        public readonly static LocalString strSelectObjects = new LocalString(
            // ru
            "Выделить все объекты с этим скриптом",
            // en
            "Select all objects with this script");
        public readonly static LocalString strSelectObject = new LocalString(
            // ru
            "Выделить объект",
            // en
            "Select target object");

        // PROGRESS BAR MESSAGES ------------------------------------------------------------------
        public readonly static LocalString strProgressBar_SearchDeadObject = new LocalString(
            // ru
            "Поиск мёртвых объектов на сцене",
            // en
            "Search dead objects");
        public readonly static LocalString strProgressBar_SearchingScene = new LocalString(
            // ru
            "Поиск в сцене: ",
            // en
            "Searching scene: ");
        public readonly static LocalString strProgressBar_SearchingSceneObjects = new LocalString(
            // ru
            "объектов проверено",
            // en
            "objects tested");
        public readonly static LocalString strProgressBar_ObjectsFound = new LocalString(
           // ru
           "Битых объектов найдено: ",
           // en
           "Corrupted objects found: ");
        public readonly static LocalString strProgressBar_InspectedObject = new LocalString(
           // ru
           "Проверяем объект: ",
           // en
           "Inspected object: ");

        // DEBUG AND EXCEPTION MESSAGES -----------------------------------------------------------
        public readonly static LocalString strDebug_GUIDNotFound = new LocalString(
            // ru
            "Строка не содержит GUID. Проверьте строку и/или файл сцены",
            // en
            "Input string does not contain any GUID. Check the string and/or scene file");
        public readonly static LocalString strDebug_SaveSceneWarning = new LocalString(
            // ru
            "Что-то пошло не так. Файл сцены повреждён или не сохранен. Сохраните сцену и повторите попытку",
            // en
            "Somethings go wrong. Scene file may be corrupted or outdated. Please, save your Scene and try again");
        public readonly static LocalString strDebug_ObjectIDNotFound = new LocalString(
            // ru
            "не найден в файле сцены",
            // en
            "not found in the Scene file");
        public readonly static LocalString strDebug_DeadScriptAtThePosition = new LocalString(
           // ru
           "мёртвый скрипт обнаружен на позиции",
           // en
           "dead script detected at the position");
    }
}

