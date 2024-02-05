using UnityEngine;
using Leap;
using Leap.Unity;

public class CubePlacer8 : MonoBehaviour
{
    public LeapServiceProvider LeapServiceProvider;
    public GameObject cubePrefab;
    private Vector3 lastCubePosition = Vector3.zero;
    public Transform cubesParent;
    private float cubeHeight = 0.0f; // ���݂̃L���[�u�̍�����ǐՂ���ϐ�
    private bool wasCanIncreaseHeight = false; // �O�̃t���[����canIncreaseHeight��true����������ǐՂ���ϐ�

    void Start()
    {
        // �Q�[�����n�܂����Ƃ���LeapServiceProvider���ݒ肳��Ă��Ȃ���΁A�T���Đݒ肷��
        if (!LeapServiceProvider)
        {
            LeapServiceProvider = FindObjectOfType<LeapServiceProvider>();
        }
    }

    void Update()
    {
        // ���ׂĂ̎�ɑ΂��ă��[�v�����s����
        foreach (Hand hand in LeapServiceProvider.CurrentFrame.Hands)
        {
            // �l�����w�Ɛe�w���擾����
            Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
            Finger thumbFinger = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];

            // �l�����w�Ɛe�w�̐�[�̈ʒu���擾����
            Vector3 indexTipPosition = indexFinger.TipPosition;
            Vector3 thumbTipPosition = thumbFinger.TipPosition;

            // �l�����w�Ɛe�w�̊Ԃ̋������v�Z����
            float pinchDistance = Vector3.Distance(indexTipPosition, thumbTipPosition);
            float pinchThreshold = 0.04f; // �s���`�Ɣ��f���鋗����臒l

            // �E��Ńs���`�����ꍇ�A�L���[�u���폜����
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
                // �l�����w�̐�[�̈ʒu�Ɋ�Â��ăL���[�u�𐶐�����
                PlaceCubeAtFingerTip(indexTipPosition);
            }

            else if (hand.IsLeft)
            {
                if (hand.PalmNormal.y > 0.5)
                {
                    if (!wasCanIncreaseHeight) // �O�̃t���[����canIncreaseHeight��false�������ꍇ�̂�maxHeight�𑝂₷
                    {
                        cubeHeight += 1.0f;
                        Debug.Log(cubeHeight);
                    }
                    wasCanIncreaseHeight = true; // ���݂̃t���[����canIncreaseHeight��true�ɂȂ������Ƃ��L�^
                }
                else
                {
                    wasCanIncreaseHeight = false; // ���݂̃t���[����canIncreaseHeight��false�ɂȂ������Ƃ��L�^
                }
            }
        }
    }

    void PlaceCubeAtFingerTip(Vector3 fingerTipPosition)
    {
        // �l�����w�̐�[�̈ʒu���ۂ߂�i�L���[�u�𐮗񂳂��邽�߁j
        Vector3 roundedPosition = new Vector3(
            Mathf.Round(fingerTipPosition.x * 20) / 20,
            //Mathf.Round(fingerTipPosition.y * 20) / 20,
            cubeHeight / 20, // ������cubeHeight�Ɏ�������
            Mathf.Round(fingerTipPosition.z * 20) / 20);

        Debug.Log("�O" + fingerTipPosition + "��" + roundedPosition);

        // �O��̈ʒu�ƈقȂ�ꍇ�̂݃L���[�u�𐶐�����
        if (roundedPosition != lastCubePosition)
        {
            // �w�肳�ꂽ�ʒu�Ɋ��ɑ��̃I�u�W�F�N�g���Ȃ����m�F����
            Collider[] colliders = Physics.OverlapBox(roundedPosition, Vector3.one * 0.025f);
            if (colliders.Length == 0)
            {
                // �L���[�u�𐶐����Đe�I�u�W�F�N�g�ɐݒ肷��
                GameObject newCube = Instantiate(cubePrefab, roundedPosition, cubesParent.rotation);
                // ��R�F�eobject�Ɠ��������ɔz�u���Ă���
                newCube.transform.parent = cubesParent; // �����Ő�������cube�����O�ɐݒ肵���eobject�̎q�ɂ��Ă���
                lastCubePosition = roundedPosition; // �Ō�ɐ��������L���[�u�̈ʒu���L�^����
            }
        }
    }
}
