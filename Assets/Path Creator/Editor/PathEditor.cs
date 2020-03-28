using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator creator;
    private Path path
    { get => creator.path; }

    private const float segmentSelectDistanceThreshold = .1f;
    private int selectedSegmentIndex = -1;

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
        }

        if(GUILayout.Button("Create River"))
        {
            Undo.RecordObject(creator, "Create River");
            creator.CreateRiver();
        }

        bool isClosed = GUILayout.Toggle(path.IsClosed, "Path closed");
        // Path 경로를 완전히 이어 완료하는 버튼
        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(creator, "Toggle closed");
            path.IsClosed = isClosed;
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
    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    // GUI 내에서 path 객체에 경로를 추가하는 함수
    private void Input()
    {
        Event guiEvent = Event.current;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        RaycastHit info;
        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out info, 1000f))
        {

            if (guiEvent.type.Equals(EventType.MouseMove))
            {
                float minDstToSegment = segmentSelectDistanceThreshold;
                int newSelectedSegmentIndex = -1;

                for (int i = 0; i < path.NumSegments; i++)
                {
                    Vector3[] points = path.GetPointsInSegment(i);
                    float dst = HandleUtility.DistancePointBezier(info.point, points[0], points[3], points[1], points[2]);

                    if (dst < minDstToSegment)
                    {
                        minDstToSegment = dst;
                        newSelectedSegmentIndex = i;
                    }
                }

                if(!newSelectedSegmentIndex.Equals(selectedSegmentIndex))
                {
                    selectedSegmentIndex = newSelectedSegmentIndex;
                    HandleUtility.Repaint();
                }
            }

            if (guiEvent.type.Equals(EventType.MouseDown) && guiEvent.button.Equals(0) && guiEvent.shift)
            {
                // 선택된 선이 있을 경우
                if(selectedSegmentIndex != -1)
                {
                    // Path 경로 선 위에 점 생성
                    Undo.RecordObject(creator, "Split segment");
                    path.SplitSegment(info.point, selectedSegmentIndex);
                }
                // 선이 완성되지 않았을 경우
                else if (!path.IsClosed)
                {
                    // Plane 평면 위와 마우스 커서 위치에 따라 점을 설치
                    Undo.RecordObject(creator, "Add segment");
                    path.AddSegment(info.point);
                }
            }

            if (guiEvent.type.Equals(EventType.MouseDown) && guiEvent.button.Equals(1))
            {
                float minDstToAnchor = creator.anchorDiameter *.5f;
                int closestAnchorIndex = -1;

                // 마우스 커서 위치와 가까운 점을 찾음
                for (int i = 0; i < path.NumPoints; i += 3)
                {
                    float dst = Vector3.Distance(info.point, path[i]);
                    if (dst < minDstToAnchor)
                    {
                        minDstToAnchor = dst;
                        closestAnchorIndex = i;
                    }
                }

                // 찾은 점이 있을 경우, 그 점을 제거
                if (!closestAnchorIndex.Equals(-1))
                {
                    Undo.RecordObject(creator, "Delete segment");
                    path.DeleteSegment(closestAnchorIndex);
                }
            }
        }

    }

    // path의 기준점들의 경로를 GUI로 표현해주는 함수
    private void Draw()
    {
        // 각 점들을 연결한 선에 대한 GUI 표현
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            if(creator.displayControlPoints)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            // Shift를 눌렀을 경우, 마우스가 가깝게 위치한 선 부분의 색을 변화
            Color segmentColor = (i.Equals(selectedSegmentIndex) && Event.current.shift) ?
                                        creator.selectedSegmentCol : creator.segmentCol;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);
        }

        // 각 점들의 GUI 표현
        for (int i = 0; i < path.NumPoints; i++)
        {
            bool isAnchor = (i % 3).Equals(0);
            if(isAnchor || creator.displayControlPoints)
            {
                // 기준 점은 빨강 색, 그 외의 점은 하양 색
                Handles.color = isAnchor ? creator.anchorCol : creator.controlCol;
                float handleSize = isAnchor ? creator.anchorDiameter : creator.controlDiameter;

                Vector3 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, handleSize, Vector3.zero, Handles.SphereHandleCap);
                // 경로가 변경된 경우, 위치 조정
                if (path[i] != newPos)
                {
                    Undo.RecordObject(creator, "Move point");
                    newPos = creator.SetHeights(newPos);
                    path.MovePoint(i, newPos);
                }
            }
        }
    }

    private void OnEnable()
    {
        creator = (PathCreator)target;
        if (creator.path == null)
            creator.CreatePath();
    }
}
