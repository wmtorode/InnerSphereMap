using UnityEngine;

public static class TransformExtensions {

    public static Transform FindRecursive(this Transform transform, string checkName) {
        foreach (Transform t in transform) {
            if (t.name == checkName) return t;

            Transform possibleTransform = FindRecursive(t, checkName);
            if (possibleTransform != null) return possibleTransform;
        }

        return null;
    }
}
