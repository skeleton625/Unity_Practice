using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{

    PathCreator creator;
    Path path;

    // Unity Inspector에 기능을 추가하는 함수
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Path 경로를 새로 생성하는 버튼
        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(creator, "Create new");
            creator.CreatePath();
            path = creator.path;
        }

        // Path 경로를 완전히 이어 완료하는 버튼
        if (GUILayout.Button("Toggle closed"))
        {
            Undo.RecordObject(creator, "Toggle closed");
            path.ToggleClosed();
        }

        // 자동 조정 Checkbox
        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set controls");
            path.AutoSetControlPoints = autoSetControlPoints;
        }

        // 앞의 버튼들에 대한 결과를 화면에 재표현
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    // Scene GUI 함수
    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    // GUI 내에서 path 객체에 경로를 추가하는 함수
    void Input()
    {
        Event guiEvent = Event.current;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            // Plane 평면 위와 마우스 커서 위치에 따라 점을 설치
            RaycastHit info;
            if(Physics.Raycast(mouseRay.origin, mouseRay.direction, out info, 1000f))
            {
                Undo.RecordObject(creator, "Add segment");
                path.AddSegment(info.point);
            }
        }
    }

    // path의 기준점들의 경로를 GUI로 표현해주는 함수
    void Draw()
    {
        // 각 점들을 연결한 선에 대한 GUI 표현
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        // 감 점들의 GUI 표현
        Handles.color = Color.red;
        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector3 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .2f, Vector3.zero, Handles.CylinderHandleCap);
            // 경로가 변경된 경우, 위치 조정
            if (path[i] != newPos)
            {
                Undo.RecordObject(creator, "Move point");
                path.MovePoint(i, newPos);
            }
        }
    }

    void OnEnable()
    {
        creator = (PathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }
        path = creator.path;
    }
}
