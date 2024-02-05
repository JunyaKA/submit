using UnityEngine;
using Leap;
using Leap.Unity;

public class CubePlacer8 : MonoBehaviour
{
    public LeapServiceProvider LeapServiceProvider;
    public GameObject cubePrefab;
    private Vector3 lastCubePosition = Vector3.zero;
    public Transform cubesParent;
    private float cubeHeight = 0.0f; // 現在のキューブの高さを追跡する変数
    private bool wasCanIncreaseHeight = false; // 前のフレームでcanIncreaseHeightがtrueだったかを追跡する変数

    void Start()
    {
        // ゲームが始まったときにLeapServiceProviderが設定されていなければ、探して設定する
        if (!LeapServiceProvider)
        {
            LeapServiceProvider = FindObjectOfType<LeapServiceProvider>();
        }
    }

    void Update()
    {
        // すべての手に対してループを実行する
        foreach (Hand hand in LeapServiceProvider.CurrentFrame.Hands)
        {
            // 人差し指と親指を取得する
            Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
            Finger thumbFinger = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];

            // 人差し指と親指の先端の位置を取得する
            Vector3 indexTipPosition = indexFinger.TipPosition;
            Vector3 thumbTipPosition = thumbFinger.TipPosition;

            // 人差し指と親指の間の距離を計算する
            float pinchDistance = Vector3.Distance(indexTipPosition, thumbTipPosition);
            float pinchThreshold = 0.04f; // ピンチと判断する距離の閾値

            // 右手でピンチした場合、キューブを削除する
            if (hand.IsRight && pinchDistance < pinchThreshold)
            {
                Collider[] pinchColliders = Physics.OverlapSphere(indexTipPosition, 0.03f);
                foreach (Collider hitCollider in pinchColliders)
                {
                    if (hitCollider.gameObject.CompareTag("Cube"))
                    {
                        Destroy(hitCollider.gameObject);
                        //Debug.Log("Cube destroyed.");
                    }
                }
            }

            else if (hand.IsRight)
            {
                // 人差し指の先端の位置に基づいてキューブを生成する
                PlaceCubeAtFingerTip(indexTipPosition);
            }

            else if (hand.IsLeft)
            {
                if (hand.PalmNormal.y > 0.5)
                {
                    if (!wasCanIncreaseHeight) // 前のフレームでcanIncreaseHeightがfalseだった場合のみmaxHeightを増やす
                    {
                        cubeHeight += 1.0f;
                        Debug.Log(cubeHeight);
                    }
                    wasCanIncreaseHeight = true; // 現在のフレームでcanIncreaseHeightがtrueになったことを記録
                }
                else
                {
                    wasCanIncreaseHeight = false; // 現在のフレームでcanIncreaseHeightがfalseになったことを記録
                }
            }
        }
    }

    void PlaceCubeAtFingerTip(Vector3 fingerTipPosition)
    {
        // 人差し指の先端の位置を丸める（キューブを整列させるため）
        Vector3 roundedPosition = new Vector3(
            Mathf.Round(fingerTipPosition.x * 20) / 20,
            //Mathf.Round(fingerTipPosition.y * 20) / 20,
            cubeHeight / 20, // 高さをcubeHeightに自動調整
            Mathf.Round(fingerTipPosition.z * 20) / 20);

        Debug.Log("前" + fingerTipPosition + "後" + roundedPosition);

        // 前回の位置と異なる場合のみキューブを生成する
        if (roundedPosition != lastCubePosition)
        {
            // 指定された位置に既に他のオブジェクトがないか確認する
            Collider[] colliders = Physics.OverlapBox(roundedPosition, Vector3.one * 0.025f);
            if (colliders.Length == 0)
            {
                // キューブを生成して親オブジェクトに設定する
                GameObject newCube = Instantiate(cubePrefab, roundedPosition, cubesParent.rotation);
                // 第３：親objectと同じ向きに配置している
                newCube.transform.parent = cubesParent; // ここで生成したcubeを事前に設定した親objectの子にしている
                lastCubePosition = roundedPosition; // 最後に生成したキューブの位置を記録する
            }
        }
    }
}
