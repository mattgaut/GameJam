using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;

public class StateMachineEditorWindow : EditorWindow {

    enum SidePanelView { state, parameters }
    SidePanelView side_panel_view;

    StateMachine state_machine;

    Color scroll_view_background_color;

    Vector2 state_size;
    Vector2 state_position;

    Vector2 scroll_view_position;

    Rect view_rect;

    Vector2 last_mouse_position;
    Vector2 mouse_drag_offset;

    int next_id;
    int focused_state_id;

    int focused_transition_id_from;
    int focused_transition_id_to;

    string temp_name;

    private Rect info_panel_rect;
    private Rect top_bar_rect;
    private Rect states_panel_rect;

    protected Dictionary<int, State> states;

    List<State> state_height_list;

    bool making_state_transition, cancel_transition;
    State transition_from;

    ReorderableList transition_list;

    ReorderableList parameter_list;
    int last_selected_parameter;
    int edit_parameter;

    ReorderableList condition_list;

    GenericMenu grid_right_click_menu;
    GenericMenu state_right_click_menu;

    public static void ShowWindow(StateMachine machine) {
        StateMachineEditorWindow editor = GetWindow<StateMachineEditorWindow>();
        editor.LoadStateMachine(machine);
    }

    [MenuItem("Window/State Machine Editor")]
    static void ShowWindow() {
        StateMachineEditorWindow editor = GetWindow<StateMachineEditorWindow>();
        editor.Init();
    }

    void Init() {
        side_panel_view = SidePanelView.state;

        top_bar_rect = new Rect(0, 0, position.width, 18);
        info_panel_rect = new Rect(0, top_bar_rect.height - 1, 400, position.height - top_bar_rect.height);
        states_panel_rect = new Rect(info_panel_rect.width, top_bar_rect.height, position.width - info_panel_rect.width, position.height - top_bar_rect.height);

        scroll_view_position = Vector2.zero;
        scroll_view_background_color = new Color(0.25f, 0.25f, 0.25f, 1);

        view_rect = new Rect(0, 0, 2000, 2000);

        state_size = new Vector2(200, 40);
        state_position = new Vector2(10, 10);
        next_id = 1;

        focused_state_id = -1;

        temp_name = "";

        state_height_list = new List<State>();
        states = new Dictionary<int, State>();

        transition_list = new ReorderableList(new List<Transition>(), typeof(Transition), true, true, false, true);

        transition_list.drawHeaderCallback += (list) => DrawReorderableListHeader(list, "Transitions");
        transition_list.drawElementCallback += DrawReorderableTransitionElement;
        transition_list.onReorderCallbackWithDetails += (list, index_1, index_2) => state_machine.ReorderTransition(states[focused_state_id], index_1, index_2);
        transition_list.onRemoveCallback += (list) => state_machine.DeleteTransition(list.GetCurrent<Transition>());

        condition_list = new ReorderableList(new List<Condition>(), typeof(Condition), true, true, true, true);

        condition_list.drawHeaderCallback += (list) => DrawReorderableListHeader(list, "Conditions");
        condition_list.drawElementCallback += DrawReorderableConditionElement;
        condition_list.onRemoveCallback += RemoveConditionFromFocused;
        condition_list.onAddCallback += AddConditionToFocused;
        condition_list.onReorderCallbackWithDetails += ReorderConditionInFocused;

        parameter_list = new ReorderableList(new List<Parameter>(), typeof(Parameter), true, true, true, true);
        parameter_list.drawHeaderCallback += (list) => DrawReorderableListHeader(list, "Parameters");
        parameter_list.drawElementCallback += DrawReorderableParamteterElement;
        parameter_list.onRemoveCallback += (list) => state_machine.DeleteParameter(list.GetCurrent<Parameter>().name);
        parameter_list.onAddCallback += (list) => state_machine.AddParameter("New Parameter");
        parameter_list.onSelectCallback += SelectReorderableParameter;
        parameter_list.drawElementCallback += ReorderableParameterElementCallback;
        parameter_list.onReorderCallbackWithDetails += (list, index_1, index_2) => state_machine.MoveParameter(index_1, index_2);

        last_selected_parameter = edit_parameter = -1;

        grid_right_click_menu = new GenericMenu();
        grid_right_click_menu.AddItem(new GUIContent("Create State"), false, () => CreateNewState("State " + next_id));
    }

    void LoadStateMachine(StateMachine machine) {
        state_machine = machine;

        Init();
        machine.Init();
        foreach (State s in machine.GetStateList()) {
            s.id = GetNextStateId();

            states.Add(s.id, s);
            state_height_list.Add(s);
            s.rect.size = state_size;
        }
    }

    private void OnGUI() {
        PreGUIEventHandler();

        DrawTopBar();
        DrawInfoPanel();
        DrawStatePanel();
        
        wantsMouseMove = making_state_transition;

        PostGUIEventHandler();

        if (state_machine) {
            EditorUtility.SetDirty(state_machine);
        }
    }

    void PreGUIEventHandler() {
        if (side_panel_view == SidePanelView.state) {
            if (GUI.GetNameOfFocusedControl() == "Renamer") {
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
                    state_machine.RenameState(states[focused_state_id], temp_name);
                    Event.current.Use();
                }
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown) {
                    GUI.FocusControl(null);
                    Repaint();
                }
            } else if (states.ContainsKey(focused_state_id)) {
                temp_name = states[focused_state_id].name;
            }
            parameter_list.index = -1;
        }

        if (side_panel_view == SidePanelView.parameters) {
            if (GUI.GetNameOfFocusedControl() == "ParameterTextField") {
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
                    state_machine.RenameParameter(parameter_list.GetCurrent<Parameter>(), temp_name);
                    edit_parameter = -1;
                    Event.current.Use();
                }
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown) {
                    edit_parameter = -1;
                    GUI.FocusControl(null);
                    Repaint();
                }
            }
            condition_list.index = -1;
            transition_list.index = -1;
        }

        if (making_state_transition) {
            if (Event.current.type == EventType.MouseDown) {
                cancel_transition = true;
            }
        }
    }

    void PostGUIEventHandler() {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete) {
            if (states.ContainsKey(focused_transition_id_to) && states.ContainsKey(focused_transition_id_from)) {
                state_machine.DeleteTransition(states[focused_transition_id_from], states[focused_transition_id_to]);
                focused_transition_id_from = focused_transition_id_to = -1;
                Event.current.Use();
            } else if (states.ContainsKey(focused_state_id)) {
                DeleteState(focused_state_id);
                Event.current.Use();
            }
        }

        if (cancel_transition) {
            making_state_transition = false;
            transition_from = null;
            cancel_transition = false;
        }
    }

    void DrawTopBar() {
        top_bar_rect.width = position.width;
        GUI.Box(top_bar_rect, "", EditorStyles.toolbar);

        Rect padded_rect = new Rect(top_bar_rect);
        padded_rect.width = 200;
        padded_rect.x += 10;

        GUILayout.BeginArea(padded_rect);
        GUILayout.BeginHorizontal();

        if (GUILayout.Toggle(side_panel_view == SidePanelView.state, "State Info", EditorStyles.toolbarButton))
            side_panel_view = SidePanelView.state;
        GUILayout.Space(4);
        if (GUILayout.Toggle(side_panel_view == SidePanelView.parameters, "Parameters", EditorStyles.toolbarButton))
            side_panel_view = SidePanelView.parameters;
        GUILayout.Space(4);

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void DrawInfoPanel() {
        info_panel_rect.height = position.height;
        GUI.Box(info_panel_rect, GUIContent.none);

        Rect padded_rect = new Rect(info_panel_rect);
        padded_rect.width -= 10;
        padded_rect.height -= 10;
        padded_rect.position += Vector2.one * 5f;

        GUILayout.BeginArea(padded_rect);

        if (state_machine == null) {
            state_machine = (StateMachine)EditorGUILayout.ObjectField(state_machine, typeof(StateMachine), false);
            if (state_machine != null) {
                LoadStateMachine(state_machine);
            }
        }

        if (side_panel_view == SidePanelView.state) {
            DrawStateInfoPanel();
        } else if (side_panel_view == SidePanelView.parameters) {
            DrawParametersInfoPanel();
        }

        GUILayout.EndArea();
    }

    void DrawParametersInfoPanel() {
        if (GUILayout.Button("Add Parameter")) {
            state_machine.AddParameter("New Parameter");
        }

        parameter_list.list = state_machine.GetParameters();
        parameter_list.DoLayoutList();
    }

    void DrawStateInfoPanel() {
        if (focused_state_id != -1 && states.ContainsKey(focused_state_id)) {
            DrawStateInfo();
        } else if (IsTransitionValid(focused_transition_id_from, focused_transition_id_to)) {
            DrawTransitionInfo(state_machine.GetTransition(states[focused_transition_id_from], states[focused_transition_id_to]));
        } else {
            focused_state_id = -1;
        }
    }

    void DrawStateInfo() {
        GUI.SetNextControlName("Renamer");
        temp_name = GUILayout.TextField(temp_name);

        transition_list.list = state_machine.GetTransitions(states[focused_state_id]);
        transition_list.DoLayoutList();

        if (transition_list.index != -1) DrawConditionList(transition_list.GetCurrent<Transition>());
    }

    void DrawConditionList(Transition t) {
        condition_list.list = t.conditions;

        condition_list.displayAdd = state_machine.GetParameters().Count != 0;

        condition_list.list = t.conditions;
        condition_list.DoLayoutList();
    }

    void DrawReorderableListHeader(Rect rect, string title) {
        GUI.Label(rect, title);
    }

    void DrawReorderableTransitionElement(Rect rect, int index, bool is_active, bool is_focused) {
        Transition t = transition_list.Get<Transition>(index);

        GUI.Label(rect, states[t.from.id].name + " -> " + states[t.to.id].name);
    }

    void DrawReorderableConditionElement(Rect rect, int index, bool is_active, bool is_focused) {
        Condition condition = condition_list.Get<Condition>(index);
        int selected = state_machine.GetParameters().IndexOf(condition.parameter);
        selected = EditorGUI.Popup(new Rect(rect.x, rect.y, (rect.width / 2), rect.height), selected != -1 ? selected : 0, state_machine.GetParameterNames().ToArray());
        condition.parameter = state_machine.GetParameters()[selected];

        selected = EditorGUI.Popup(new Rect(rect.x + (rect.width / 2), rect.y, (rect.width / 2), rect.height), condition.should_parameter_be_true ? 1 : 0, new string[] { "false", "true" });

        condition.should_parameter_be_true = selected == 1;
    }

    void DrawReorderableParamteterElement(Rect rect, int index, bool is_active, bool is_focused) {
        Parameter parameter = parameter_list.Get<Parameter>(index);

        if (index == edit_parameter) {
            GUI.SetNextControlName("ParameterTextField");
            temp_name = GUI.TextField(rect, temp_name);
            EditorGUI.FocusTextInControl("ParameterTextField");
        } else {
            GUI.Label(rect, parameter.name);
        }
    }

    void ReorderableParameterElementCallback(Rect rect, int index, bool is_active, bool is_focused) {
        if (last_selected_parameter == index) {
            if (Event.current.type == EventType.MouseDown) {
                if (rect.Contains(Event.current.mousePosition)) {
                    edit_parameter = index;
                    temp_name = parameter_list.Get<Parameter>(index).name;
                } else {
                    edit_parameter = -1;
                }
            }
        }
    }
    void SelectReorderableParameter(ReorderableList list) {
        last_selected_parameter = list.index;
    }

    void DrawTransitionInfo(Transition t) {
        GUILayout.Label(states[t.from.id].name + " -> " + states[t.to.id].name);

        DrawConditionList(t);
    }

    void DrawStatePanel() {
        states_panel_rect.width = position.width - info_panel_rect.width;
        states_panel_rect.height = position.height - top_bar_rect.height;

        scroll_view_position = GUI.BeginScrollView(states_panel_rect, scroll_view_position, view_rect);

        Color old_color = GUI.backgroundColor;
        GUI.backgroundColor = scroll_view_background_color;
        GUI.Box(view_rect, GUIContent.none);
        GUI.backgroundColor = old_color;

        DrawGrid(view_rect, 25, Color.gray);

        BeginWindows();

        for (int i = state_height_list.Count - 1; i >= 0; i--) {
            State state = state_height_list[i];
            Color color = GUI.backgroundColor;
            if (state_machine.GetEntryState() == state) {
                GUI.backgroundColor = (Color.yellow / 2f + Color.black / 2f);
            }
            state.rect = GUILayout.Window(state.id, state.rect, DrawStateWindow, state.name);

            GUI.backgroundColor = color;

            HandleDragWindowEvents(state.rect, state.id);
        }
        foreach (State state in states.Values) {
            foreach (Transition transition in state_machine.GetTransitions(state)) {
                DrawStateTransition(transition.from, transition.to);
            }
        }
        if (states.ContainsKey(focused_state_id)) GUI.FocusWindow(focused_state_id);

        if (Event.current.button == 1 && Event.current.type == EventType.MouseUp && making_state_transition) {
            making_state_transition = false;
            transition_from = null;

            Event.current.Use();
        }

        if (making_state_transition) {
            DrawConnection(transition_from.rect.center, Event.current.mousePosition);
            Repaint();
        }

        EndWindows();

        GUI.EndScrollView();

        ScrollViewEventHandler();
    }

    void DrawGrid(Rect rect, float grid_spacing, Color grid_color) {
        int width_divs = Mathf.CeilToInt(rect.width / grid_spacing);
        int height_divs = Mathf.CeilToInt(rect.height / grid_spacing);

        Handles.color = new Color(grid_color.r, grid_color.g, grid_color.b, 1f);

        for (int i = 0; i < width_divs; i++) {
            if (i % 5 == 0) {
                Handles.color = new Color(grid_color.r, grid_color.g, grid_color.b, 1f);
            } else {
                Handles.color = new Color(grid_color.r, grid_color.g, grid_color.b, 0.5f);
            }
            Handles.DrawLine(new Vector3(grid_spacing * i, -grid_spacing, 0), new Vector3(grid_spacing * i, rect.height, 0f));
        }

        for (int j = 0; j < height_divs; j++) {
            if (j % 5 == 0) {
                Handles.color = new Color(grid_color.r, grid_color.g, grid_color.b, 1f);
            } else {
                Handles.color = new Color(grid_color.r, grid_color.g, grid_color.b, 0.5f);
            }
            
            Handles.DrawLine(new Vector3(-grid_spacing, grid_spacing * j, 0), new Vector3(rect.width, grid_spacing * j, 0f));
        }
    }

    void ScrollViewEventHandler() {
        if (states_panel_rect.Contains(Event.current.mousePosition)) {
            if (Event.current.button == 2 && Event.current.type == EventType.MouseDown) {
                last_mouse_position = Event.current.mousePosition;

                Event.current.Use();
            }

            if (Event.current.button == 2 && Event.current.type == EventType.MouseDrag) {
                scroll_view_position += (last_mouse_position - Event.current.mousePosition);

                scroll_view_position.x = Mathf.Min(Mathf.Max(scroll_view_position.x, 0), view_rect.width - states_panel_rect.width + GUI.skin.horizontalScrollbar.fixedHeight);
                scroll_view_position.y = Mathf.Min(Mathf.Max(scroll_view_position.y, 0), view_rect.height - states_panel_rect.height + GUI.skin.verticalScrollbar.fixedWidth);

                last_mouse_position = Event.current.mousePosition;

                Event.current.Use();
            }

            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown) {
                grid_right_click_menu.ShowAsContext();
                Event.current.Use();
            }
        }
    }

    void DrawStateWindow(int id) {
        GUILayout.Label("");
    }

    void HandleDragWindowEvents(Rect rect, int id) {
        if (rect.Contains(Event.current.mousePosition)) {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown) {
                if (making_state_transition) {
                    state_machine.CreateTransition(transition_from, states[id]);
                    making_state_transition = false;
                    transition_from = null;
                } else {
                    FocusState(id);

                    mouse_drag_offset = Event.current.mousePosition - rect.position;
                }
                Event.current.Use();
            }
            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown) {
                state_right_click_menu = new GenericMenu();
                state_right_click_menu.AddItem(new GUIContent("Set as Entry State"), false, () => state_machine.SetEntryState(states[id]));
                state_right_click_menu.AddItem(new GUIContent("Make Transition"), false, () => StartTransitionCreation(id));
                state_right_click_menu.ShowAsContext();
                Event.current.Use();
            }
        }
        if (Event.current.button == 0 && Event.current.type == EventType.MouseDrag && focused_state_id == id && mouse_drag_offset != Vector2.zero) {
            states[id].rect.position = (Event.current.mousePosition - mouse_drag_offset);
            Event.current.Use();
        }
        if (mouse_drag_offset != Vector2.zero && Event.current.button == 0 && Event.current.type == EventType.MouseUp && focused_state_id == id) {
            mouse_drag_offset = Vector2.zero;
        }
    }

    void StartTransitionCreation(int state_id) {
        making_state_transition = true;
        transition_from = states[state_id];
    }

    State CreateNewState(string name, bool focus_state = true) {
        State state = state_machine.CreateState(name);
        state.id = GetNextStateId();
        state.rect = new Rect(GetNextWindowPosition(), state_size);

        states.Add(state.id, state);
        state_height_list.Add(state);

        if (focus_state) FocusState(state.id);

        return state;
    }

    void DeleteState(int id) {
        State state = states[id];
        states.Remove(id);
        state_height_list.Remove(state);
        state_machine.DeleteState(state);
    }

    void FocusState(int id) {
        focused_transition_id_from = -1;
        focused_transition_id_to = -1;

        focused_state_id = id;
        state_height_list.Remove(states[id]);
        state_height_list.Add(states[id]);

        temp_name = states[id].name;

        transition_list.index = -1;
        condition_list.index = -1;

        GUI.FocusWindow(id);
    }

    void FocusConnection(int id_from, int id_to, bool unfocus_state = true) {
        focused_transition_id_from = id_from;
        focused_transition_id_to = id_to;

        condition_list.index = -1;

        if (unfocus_state) {
            focused_state_id = -1;
            GUI.UnfocusWindow();
        }
    }

    void ReorderConditionInFocused(ReorderableList condition_list, int index_1, int index_2) {
        if (IsTransitionFocused(focused_transition_id_from, focused_transition_id_to)) {
            state_machine.GetTransition(states[focused_transition_id_from], states[focused_transition_id_to]).ReorderCondition(index_1, index_2);
        } else if (focused_state_id != -1) {
            transition_list.GetCurrent<Transition>().ReorderCondition(index_1, index_2);
        }
    }

    void RemoveConditionFromFocused(ReorderableList condition_list) {
        if (IsTransitionFocused(focused_transition_id_from, focused_transition_id_to)) {
            state_machine.GetTransition(states[focused_transition_id_from], states[focused_transition_id_to]).RemoveCondition(condition_list.GetCurrent<Condition>());
        } else if (focused_state_id != -1) {
            transition_list.GetCurrent<Transition>().RemoveCondition(condition_list.GetCurrent<Condition>());
        }
    }

    void AddConditionToFocused(ReorderableList condition_list) {
        if (IsTransitionFocused(focused_transition_id_from, focused_transition_id_to)) {
            state_machine.GetTransition(states[focused_transition_id_from], states[focused_transition_id_to]).AddCondition(new Condition());
        } else if (focused_state_id != -1) {
            transition_list.GetCurrent<Transition>().AddCondition(new Condition());
        }
    }

    bool IsTransitionFocused(int id_from, int id_to) {
        return id_from == focused_transition_id_from && id_to == focused_transition_id_to && id_from != -1 && id_to != -1;
    }

    bool IsTransitionValid(int id_from, int id_to) {
        return states.ContainsKey(id_from) && states.ContainsKey(id_to);
    }

    void DrawStateTransition(State start_state, State end_state, float offset = 10f) {
        if (start_state == end_state) {
            DrawSameStateTransition(start_state);
            return;
        }
        Vector2 start = start_state.rect.center;
        Vector2 end = end_state.rect.center;

        Vector2 perpindicular = (end - start);
        perpindicular = (Quaternion.Euler(0, 0, 90) * perpindicular).normalized;

        start += perpindicular.normalized * offset;
        end += perpindicular.normalized * offset;

        Vector2 midpoint = (start + end) / 2f;

        if (Handles.Button(midpoint + (end_state.rect.center - start_state.rect.center).normalized * 6, Quaternion.identity, 0, 10, Handles.DotHandleCap)) {
            FocusConnection(start_state.id, end_state.id);
        }

        Handles.color = IsTransitionFocused(start_state.id, end_state.id) ? Color.yellow : new Color(.75f, .75f, .75f, 1);

        Handles.DrawAAPolyLine(5, new Vector3[] { start, end } );

        Handles.DrawAAConvexPolygon(new Vector3[] { (midpoint - perpindicular * 8), (midpoint + perpindicular * 8), (midpoint + (end_state.rect.center - start_state.rect.center).normalized * 11) } );
    }

    void DrawSameStateTransition(State state) {
        Vector2 bottom_center = new Vector2(state.rect.center.x, state.rect.yMax);

        if (Handles.Button(bottom_center + Vector2.up * 6, Quaternion.identity, 0, 10, Handles.DotHandleCap)) {
            FocusConnection(state.id, state.id);
        }

        Handles.color = IsTransitionFocused(state.id, state.id) ? Color.yellow : new Color(.75f, .75f, .75f, 1);
        Handles.DrawAAConvexPolygon(new Vector3[] { bottom_center, bottom_center + new Vector2(8, 11), bottom_center + new Vector2(-8, 11) });
    }

    void DrawConnection(Vector2 start, Vector2 end, float offset = 10f) {
        Vector2 perpindicular = (end - start);
        perpindicular = (Quaternion.Euler(0, 0, 90) * perpindicular).normalized;

        start += perpindicular.normalized * offset;
        end += perpindicular.normalized * offset;

        Vector2 midpoint = (start + end) / 2f;

        Handles.color = new Color(.75f, .75f, .75f, 1);

        Handles.DrawAAPolyLine(5, new Vector3[] { start, end });

        Handles.DrawAAConvexPolygon(new Vector3[] { (midpoint - perpindicular * 8), (midpoint + perpindicular * 8), (midpoint + (end - start).normalized * 11) });
    }

    Vector2 GetNextWindowPosition() {
        state_position += new Vector2(20, 20);
        return state_position;
    }

    int GetNextStateId() {
        return next_id++;
    }
}
