#if UNITY_EDITOR
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ET
{
    public static float[] 
    AddArrayElement(float[] floatArray)
    {
        float[] tempArray = new float[floatArray.Length + 1];
        for(int i = 0; i < tempArray.Length - 1; ++i)
        {
            tempArray[i] = floatArray[i];
        }
        return tempArray;
    }

    public static T[] 
    AddArrayElement<T>(T[] floatArray) where T : Object
    {
        T[] tempArray = new T[floatArray.Length + 1];
        for(int i = 0; i < tempArray.Length - 1; ++i)
        {
            tempArray[i] = floatArray[i];
        }
        return tempArray;
    }

    public static float[] 
    RemoveArrayElement(float[] floatArray)
    {
        if(floatArray == null)
            return null;
        float[] tempArray = new float[floatArray.Length - 1];
        for(int i = 0; i < tempArray.Length; ++i)
        {
            tempArray[i] = floatArray[i];
        }
        return tempArray;
    }

    public static T[] 
    RemoveArrayElement<T>(T[] floatArray) where T : Object
    {
        T[] tempArray = new T[floatArray.Length - 1];
        for(int i = 0; i < tempArray.Length; ++i)
        {
            tempArray[i] = floatArray[i];
        }
        return tempArray;
    }

    public static void 
    DrawArray(string label, string elementLabel, ref float[] array)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(label);
        GUILayout.BeginHorizontal();
        
        if(GUILayout.Button("Add"))
        {
            array = AddArrayElement(array);
        }
        
        if(GUILayout.Button("Remove"))
        {
            array = RemoveArrayElement(array);
        }
        
        GUILayout.EndHorizontal();
        
        if(array == null) array = new float[0];
        
        for(int i = 0; i < array.Length; i++)
        {
            array[i] = EditorGUILayout.FloatField(elementLabel + " " + i, array[i]);
        }
        EditorGUILayout.EndVertical();
    }

    public static void 
    DrawArray<T>(string label, string elementLabel, ref T[] array, bool no_buttons = false) where T : Object
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(label);
        GUILayout.BeginHorizontal();
        
        if(!no_buttons && GUILayout.Button("Add"))
        {
            array = AddArrayElement<T>(array);
        }
        
        if(!no_buttons && GUILayout.Button("Remove"))
        {
            array = RemoveArrayElement<T>(array);
        }
        
        GUILayout.EndHorizontal();
        
        if(array == null) array = new T[0];
        
        for(int i = 0; i < array.Length; i++)
        {
            array[i] = (T)EditorGUILayout.ObjectField(elementLabel + " " + i, array[i], typeof(T), true);
        }
        EditorGUILayout.EndVertical();
    }

    public static void 
    DrawObject<T>(
        string label, 
        ref T obj, 
        bool allow_scene_object = false,
        float? element_width = null, 
        float? label_width = null, 
        Color? color = null) 
            where T : Object
    {
        obj = DrawObject(label, obj, allow_scene_object, element_width, label_width, color);
    }

    public static T
    DrawObject<T>(
        string label, 
        T obj, 
        bool allow_scene_object = false,
        float? element_width = null, 
        float? label_width = null, 
        Color? color = null) 
            where T : Object
    {
        ColorWork_Start(color);

        T new_obj;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(label_width != null)
            EditorGUIUtility.labelWidth = (float)label_width;

        if(element_width == null)
            new_obj = (T)EditorGUILayout.ObjectField(label, obj, typeof(T), allow_scene_object);
        else
            new_obj = (T)EditorGUILayout.ObjectField(label, obj, typeof(T), allow_scene_object, GUILayout.MaxWidth((float)element_width));

        EditorGUIUtility.labelWidth = prevWidth;

        ColorWork_End(color);
        return new_obj;
    }

    public static string 
    GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
    {
        return ((MemberExpression)memberAccess.Body).Member.Name;
    }

    public static void 
    SecuredToggle(string label, object obj, string fieldName)
    {
        try
        { 
            PropertyInfo field = obj.GetType().GetProperty(fieldName);
            bool oldResult = (bool)(field.GetValue(obj, null));
            object valueObject = EditorGUILayout.Toggle(label, oldResult);
            field.SetValue(obj, valueObject, null);
        }
        catch(Exception exception)
        {
            Debug.LogError("SecuredToggle(): obj " + obj.ToString() + ". some troubles. Error: " + exception.Message);
        }
    }

    public static void 
    DrawSeparatorLabel(string label, Rect rect)
    {
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(label));
        float separatorWidth = (rect.width - textSize.x) / 2.0f - 5.0f;
		rect.y += 7;

		GUI.Box(new Rect(rect.xMin, rect.yMin, separatorWidth, 1), "");
		GUI.Label(new Rect(rect.xMin + separatorWidth + 5.0f, rect.yMin - 8.0f, textSize.x, 20), label);
		GUI.Box(new Rect(rect.xMin + separatorWidth + 5.0f + textSize.x, rect.yMin, separatorWidth, 1), "");
    }

    public static void DrawHorizontalBox(Action action, bool stylized = false)
    {
        if (stylized)
            EditorGUILayout.BeginHorizontal("box");
        else
            EditorGUILayout.BeginHorizontal();
        action();
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawVerticalBox(Action action, bool stylized = false)
    {
        if(stylized)
            EditorGUILayout.BeginVertical("box");
        else 
            EditorGUILayout.BeginVertical();
        action();
        EditorGUILayout.EndVertical();
    }

    public static void 
    HorizontalBoxBegin()
    {
        EditorGUILayout.BeginHorizontal("box");
    }

    public static void 
    HorizontalBoxEnd()
    {
        EditorGUILayout.EndHorizontal();
    }

    public static void 
    VerticalBoxBegin()
    {
        EditorGUILayout.BeginVertical("box");
    }

    public static void 
    VerticalBoxEnd()
    {
        EditorGUILayout.EndVertical();
    }

    public static void
    LabelField(string label, float? element_width = null, float? label_width = null, Color? color = null, string tip = "")
    {
        ColorWork_Start(color);

        float prevWidth = EditorGUIUtility.labelWidth;
        if(label_width != null)
            EditorGUIUtility.labelWidth = (float)label_width;

        if(element_width == null)
        {
            EditorGUILayout.LabelField(new GUIContent(label, tip));
        }
        else
        {
            EditorGUILayout.LabelField(new GUIContent(label, tip), GUILayout.Width((float)element_width));
        }
        EditorGUIUtility.labelWidth = prevWidth;

        ColorWork_End(color);
    }

    public static void
    Button(string label, Action action, bool? additional_condition = null, Color? color = null, float? width = null, string tip = "")
    {
        ColorWork_Start(color);
        bool consition = additional_condition == null ? true : (bool) additional_condition;
        if (width == null)
        {
            if (GUILayout.Button(new GUIContent(label, tip)) && consition)
            {
                action();
            }
        }
        else
        {
            if (GUILayout.Button(new GUIContent(label, tip), GUILayout.Width((float)width)) && consition)
            {
                action();
            }
        }
        ColorWork_End(color);
    }

    public static void
    DrawString(string label, ref string value, float? labelWidth = null, float? width = null, string tip = "")
    {
        value = DrawString(label, value, labelWidth, width, tip);
    }

    public static string
    DrawString(string label, string value, float? labelWidth = null, float? width = null, string tip = "")
    {
        string new_value;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        if (width != null)
        {
            new_value = EditorGUILayout.TextField(new GUIContent(label, tip), value, GUILayout.Width((float) width));
        }
        else
        {
            new_value = EditorGUILayout.TextField(new GUIContent(label, tip), value);
        }

        EditorGUIUtility.labelWidth = prevWidth;
        return new_value;
    }

    public static void 
    DrawInt(string label, ref int value, float? labelWidth = null, float? width = null, string tip = "", Color? color = null)
    {
        value = DrawInt(label, value, labelWidth, width, tip, color);
    }

    public static int
    DrawInt(string label, int value, float? labelWidth = null, float? width = null, string tip = "", Color? color = null)
    {
        ColorWork_Start(color);
        int new_value;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        if (width != null)
        {
            new_value = EditorGUILayout.IntField(new GUIContent(label, tip), value, GUILayout.Width((float) width));
        }
        else
        {
            new_value = EditorGUILayout.IntField(new GUIContent(label, tip), value);
        }

        EditorGUIUtility.labelWidth = prevWidth;
        ColorWork_End(color);
        return new_value;
    }

    public static void
    DrawFloat(string label, ref float value, float? labelWidth = null, float? width = null, string tip = "")
    {
        value = DrawFloat(label, value, labelWidth, width);
    }

    public static float
    DrawFloat(string label, float value, float? labelWidth = null, float? width = null, string tip = "")
    {
        float new_value;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        if (width != null)
        {
            new_value = EditorGUILayout.FloatField(new GUIContent(label, tip), value, GUILayout.Width((float) width));
        }
        else
        {
            new_value = EditorGUILayout.FloatField(new GUIContent(label, tip), value);
        }

        EditorGUIUtility.labelWidth = prevWidth;
        return new_value;
    }

    public static void
    DrawVector3(string label, ref Vector3 value, float? labelWidth = null, float? width = null, string tip = "")
    {
        value = DrawVector3(label, value, labelWidth, width, tip);
    }

    public static Vector3
    DrawVector3(string label, Vector3 value, float? labelWidth = null, float? width = null, string tip = "")
    {
        Vector3 new_value;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        if (width != null)
        {
            new_value = EditorGUILayout.Vector3Field(new GUIContent(label, tip), value, GUILayout.Width((float) width));
        }
        else
        {
            new_value = EditorGUILayout.Vector3Field(new GUIContent(label, tip), value);
        }

        EditorGUIUtility.labelWidth = prevWidth;
        return new_value;
    }

    public static void 
    DrawBool(string label, ref bool toggle, float? labelWidth = null, float? width = null, bool isLeft = false, string tip = "")
    {
        toggle = DrawBool(label, toggle, labelWidth, width, isLeft, tip);
    }

    public static bool 
    DrawBool(string label, bool toggle, float? labelWidth = null, float? width = null, bool isLeft = false, string tip = "")
    {
        bool new_toggle;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        if (width != null)
        {
            if (!isLeft)
            {
                new_toggle = EditorGUILayout.Toggle(new GUIContent(label, tip), toggle, GUILayout.Width((float) width));
            }
            else
            {
                new_toggle = EditorGUILayout.ToggleLeft(new GUIContent(label, tip), toggle, GUILayout.Width((float) width));
            }
        }
        else
        {
            if (!isLeft)
            {
                new_toggle = EditorGUILayout.Toggle(new GUIContent(label, tip), toggle);
            }
            else
            {
                new_toggle = EditorGUILayout.ToggleLeft(new GUIContent(label, tip), toggle);
            }
        }

        EditorGUIUtility.labelWidth = prevWidth;
        return new_toggle;
    }

    public static void 
    DrawSlider(string label, ref float value, float left_value, float right_value, float? labelWidth = null, float? width = null, string tip = "")
    {
        value = DrawSlider(label, value, left_value, right_value, labelWidth, width, tip);
    }

    public static float 
    DrawSlider(string label, float value, float left_value, float right_value, float? labelWidth = null, float? width = null, string tip = "")
    {
        float new_value;
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        if (width != null)
        {
            new_value = EditorGUILayout.Slider(new GUIContent(label, tip), value, left_value, right_value, GUILayout.Width((float) width));
        }
        else
        {
            new_value = EditorGUILayout.Slider(new GUIContent(label, tip), value, left_value, right_value);
        }

        EditorGUIUtility.labelWidth = prevWidth;
        return new_value;
        
    }

    public static Enum 
    DrawEnum(string label, Enum enumData, float? labelWidth = null, float? width = null, string tip = "")
    {
        float prevWidth = EditorGUIUtility.labelWidth;
        if(labelWidth != null)
            EditorGUIUtility.labelWidth = (float)labelWidth;

        Enum tEnum;

        if(width != null)
            tEnum = EditorGUILayout.EnumPopup(new GUIContent(label, tip), enumData, GUILayout.Width((float)width));
        else
            tEnum = EditorGUILayout.EnumPopup(new GUIContent(label, tip), enumData);

        EditorGUIUtility.labelWidth = prevWidth;
        return tEnum;
    }

    public static bool 
    DrawMessageOkCancel(string text)
    {
        return EditorUtility.DisplayDialog("Warning!", text, "Ok", "Cancel");
    }

    public static bool
    DrawMessageOk(string text)
    {
        return EditorUtility.DisplayDialog("Warning!", text, "Ok");
    }

    public static void Separate()
    {
        EditorGUILayout.Separator();
    }

    private static void ColorWork_Start(Color? color)
    {
        if (color != null)
        {
            GUI.color = (Color)color;
        }
    }

    private static void ColorWork_End(Color? color)
    {
        if (color != null)
        {
            GUI.color = Color.white;
        }
    }
}
#endif