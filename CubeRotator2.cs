using UnityEngine;
using Leap;
using Leap.Unity;

public class CubeRotator2 : MonoBehaviour
{
    // Leap Motionのサービスプロバイダーを参照するための変数
    public LeapServiceProvider LeapServiceProvider;

    // 回転させるキューブを参照するための変数
    public GameObject cube;

    // 前フレームの手の平の位置を記憶するための変数
    private Vector3 previousPalmPosition;

    void Update()
    {
        Frame frame = LeapServiceProvider.CurrentFrame;

        if (frame != null && frame.Hands.Count > 0)
        {
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsLeft)
                {
                    // 人差し指と親指を取得する
                    Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                    Finger thumbFinger = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];

                    // 人差し指と親指の先端の位置を取得する
                    Vector3 indexTipPosition = indexFinger.TipPosition;
                    Vector3 thumbTipPosition = thumbFinger.TipPosition;

                    // 人差し指と親指の間の距離を計算する
                    float pinchDistance = Vector3.Distance(indexTipPosition, thumbTipPosition);
                    float pinchThreshold = 0.05f; // ピンチと判断する距離の閾値

                    // ピンチ動作が検出された場合のみキューブを回転させる
                    if (pinchDistance < pinchThreshold)
                    {
                        Vector3 currentPalmPosition = hand.PalmPosition;
                        float deltaX = currentPalmPosition.x - previousPalmPosition.x;
                        RotateCube(deltaX);
                        previousPalmPosition = currentPalmPosition;
                    }
                }
            }
        }
    }

    void RotateCube(float deltaX) // RotateCubeメソッドを定義。voidは戻り値なし
    {
        // 手の動きに基づいてキューブを回転させる
        // deltaXは手の左右の動きの大きさ
        // 回転速度の係数（500.0f）を調整して、回転の速さを変えられる
        float rotationSpeed = deltaX * 500.0f;
        // キューブをy軸周りに回転させる
        cube.transform.Rotate(0, rotationSpeed, 0);
    }
}
