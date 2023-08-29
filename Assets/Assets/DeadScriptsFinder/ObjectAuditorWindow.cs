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

using System;
using System.Collections.Generic;
using UnityEngine;
using static Spiral.EditorToolkit.DeadScriptsSearcher.DeadScriptLocalization;

#if UNITY_EDITOR
using UnityEditor;
namespace Spiral.EditorToolkit.DeadScriptsSearcher
{
    public class ObjectAuditorWindow : SpiralCustomEditorWindow
    {
        private Vector2 scrollPos;

        [NonSerialized]private GUILayoutOption labelOption = GUILayout.Height(20);
        [NonSerialized]private Color colorNormal = new Color(0.5f, 0.8f, 0.5f);
        [NonSerialized]private Color colorAlert  = new Color(0.8f, 0.5f, 0.5f);
        [NonSerialized]private Color colorGood   = new Color(0.9f, 0.9f, 0.9f);

        private readonly List<ObjectID> oids = new List<ObjectID>();

        [MenuItem("Spiral Tools/Object Auditor")]
        public static void Init()
        {
            ObjectAuditorWindow window = (ObjectAuditorWindow)GetWindow(typeof(ObjectAuditorWindow));
            window.Show();
        }

        private void OnEnable()
        {
            CheckAndRepaint();
        }

        private void OnSelectionChange()
        {
            CheckAndRepaint();
        }

        private void OnGUI()
        {
            titleContent.text = strMonoView_Caption;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
            OpenStandartBack();
            SpiralLocalization.DrawLanguageSelect();
            SceneFile.DrawSceneReloadButton();

            if (oids.Count == 0)
            {
                EditorGUILayout.LabelField(strMonoView_SelectObject, labelOption);
            }

            for (int objIDX = 0; objIDX < oids.Count; objIDX++)
            {
                DrawObject(oids[objIDX]);
            }

            CloseStandartBack();
            EditorGUILayout.EndScrollView();
        }

        private void CheckAndRepaint()
        {
            CheckSelection();
            Repaint();
        }

        private void CheckSelection()
        {
            var selected = Selection.gameObjects;
            oids.Clear();
            if (selected != null)
            {
                for (int i = 0; i < selected.Length; i++)
                {
                    ObjectID oid = new ObjectID(selected[i], false);
                    oids.Add(oid);
                }
            }
        }

        private void DrawObject(ObjectID oid)
        {
            bool dead = oid.missingScriptsCount > 0;
            SpiralEditor.BeginPanel($"Game Object: {oid.gameObject.name}", dead ? colorAlert : colorGood, labelOption);
            EditorGUILayout.SelectableLabel($"File ID: {oid.globalID.targetObjectId}", labelOption);

            string captionIDX;
            if (oid.fileCaptionStringIDX > 0)
            {
                captionIDX = oid.fileCaptionStringIDX.ToString();
            }
            else
            {
                if (oid.isPartOfPrefab)
                {
                    captionIDX = "[prefabed]";
                }
                else
                {
                    captionIDX = "[not found]";
                }
            }

            EditorGUILayout.SelectableLabel($"File caption IDX: {captionIDX}", labelOption);

            string strShowInfo = oid.showInfo ? strMonoView_HideObjectInfo : strMonoView_ShowObjectInfo;
            oid.showInfo = EditorGUILayout.Foldout(oid.showInfo, strShowInfo);
            if (oid.showInfo)
            {
                DrawComponentList(oid);
            }

            SpiralEditor.EndPanel();
        }

        private void DrawComponentList(ObjectID oid)
        {
            if (oid.componentIDs != null)
            {
                for (int comIDX = 0; comIDX < oid.componentIDs.Count; comIDX++)
                {
                    var cid = oid.componentIDs[comIDX];
                    SpiralEditor.BeginPanel(GroupType.Vertical, cid.alive ? colorNormal : colorAlert);

                    ulong fileID = oid.componentFileIDs[comIDX]; // соответствует cid.fileID
                    string guid = oid.componentGUIDs[comIDX];
                    // TODO: эта замена временная, так как функционал ещё не перенесён в ComponentID

                    if (cid.component == null)
                    {
                        EditorGUILayout.SelectableLabel($"Component #{comIDX} is missing!", labelOption);
                    }
                    else
                    {
                        EditorGUILayout.SelectableLabel($"Component #{comIDX}: {cid.type.Name}", labelOption);
                    }

                    EditorGUILayout.SelectableLabel($"Fild ID: {fileID}", labelOption);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        EditorGUILayout.SelectableLabel($"Script GUID: {guid}", labelOption);
                    }
                    else
                    {
                        bool isPrefabed = oid.isPartOfPrefab;
                        string message = "No GUID found";
                        if (cid.mScript == null)
                        {
                            message += " [Is not MonoBehaviour]";
                        }
                        if (isPrefabed)
                        {
                            message += " [Prefabed]"; // может одновременно быть и то, и то
                        }
                        EditorGUILayout.LabelField(message, labelOption);
                    }

                    SpiralEditor.EndPanel();
                }
            }
        }
    }
}
#endif
