using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BlackboardFieldDragger : MouseManipulator
{
    private bool m_Active;
    private readonly float m_DragDelay = 0.2f;
    private Vector2 m_Start;
    private BlackboardField m_BlackboardField;
    private readonly GraphView m_GraphView;

    public BlackboardFieldDragger(GraphView graphView)
    {
        m_GraphView = graphView;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (CanStartManipulation(evt))
        {
            m_Active = true;
            m_Start = evt.localMousePosition;
            m_BlackboardField = target as BlackboardField;
            m_GraphView.CaptureMouse();
            evt.StopPropagation();
            ScheduleDragDelayed(evt);
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (m_Active && CanStopManipulation(evt))
        {
            m_Active = false;
            m_GraphView.ReleaseMouse();
            evt.StopPropagation();
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!m_Active || !CanStartManipulation(evt))
            return;

        if ((evt.localMousePosition - m_Start).sqrMagnitude > m_DragDelay * m_DragDelay)
        {
            m_Active = false;
            m_GraphView.ReleaseMouse();
            DragBlackboardField();
            evt.StopPropagation();
        }
    }

    private void DragBlackboardField()
    {
        if (m_BlackboardField != null)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("BlackboardField", m_BlackboardField);
            DragAndDrop.StartDrag(m_BlackboardField.title);
            
        }
    }

    private void ScheduleDragDelayed(MouseDownEvent evt)
    {
        EditorApplication.delayCall += () =>
        {
            if (m_Active)
            {
                DragBlackboardField();
                m_GraphView.ReleaseMouse();
                evt.StopPropagation();
            }
        };
    }
}
