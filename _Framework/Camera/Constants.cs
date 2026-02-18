using System;
using UnityEngine;

public static class Constants
{
    public enum CameraKey
    {
        PlayerFollow,
        Formation
    }

    [Serializable]
    public sealed class CameraConstants
    {
        public Vector3 LocalPos { get; }
        public Vector3 EulerAngles { get; }
        public float FieldOfView { get; }

        public CameraConstants(Vector3 localPos, Vector3 eulerAngles, float fieldOfView)
        {
            LocalPos = localPos;
            EulerAngles = eulerAngles;
            FieldOfView = fieldOfView;
        }
    }

    // ここにプリセットを追加していく
    public static readonly CameraConstants PlayerFollowCam =
        new CameraConstants(
            localPos: new Vector3(0f, 25f, -17f),
            eulerAngles: new Vector3(70f, 0f, 0f), 
            fieldOfView: 60f
        );

    public static readonly CameraConstants FormationCam =
        new CameraConstants(
            localPos: new Vector3(0f, 22f, -10f),
            eulerAngles: new Vector3(70f, 0f, 0f),
            fieldOfView: 27f
        );

    // キー → 定義取得
    public static CameraConstants GetCamera(CameraKey key)
    {
        return key switch
        {
            CameraKey.PlayerFollow => PlayerFollowCam,
            CameraKey.Formation => FormationCam,
            _ => null
        };
    }
}
