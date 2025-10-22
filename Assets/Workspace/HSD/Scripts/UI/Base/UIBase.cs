using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        AutoBind();
    }

    private void AutoBind()
    {
        var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
        {
            var attr = field.GetCustomAttribute<UIBindAttribute>();
            if (attr == null) continue;

            string targetName = attr.Path ?? field.Name;

            var target = FindByName(transform, targetName);
            if (target == null)
            {
                Debug.LogError($"[UIBase] 이름으로 오브젝트를 찾을 수 없습니다: {targetName} ({field.Name}) in {GetType().Name}");
                continue;
            }
            
            var component = target.GetComponent(field.FieldType);

            if (component == null)
            {
                Debug.LogError($"[UIBase] 컴포넌트를 찾을 수 없습니다.: {targetName} : {field.Name} 의 {GetType().Name}");
                continue;
            }

            field.SetValue(this, component);
        }
    }
    private Transform FindByName(Transform root, string name)
    {
        if (root.name == name)
            return root;

        foreach (Transform child in root)
        {
            var result = FindByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
