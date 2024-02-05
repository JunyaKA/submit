using UnityEngine;
using Leap;
using Leap.Unity;

public class CubeRotator2 : MonoBehaviour
{
    // Leap Motion�̃T�[�r�X�v���o�C�_�[���Q�Ƃ��邽�߂̕ϐ�
    public LeapServiceProvider LeapServiceProvider;

    // ��]������L���[�u���Q�Ƃ��邽�߂̕ϐ�
    public GameObject cube;

    // �O�t���[���̎�̕��̈ʒu���L�����邽�߂̕ϐ�
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
                    // �l�����w�Ɛe�w���擾����
                    Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                    Finger thumbFinger = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];

                    // �l�����w�Ɛe�w�̐�[�̈ʒu���擾����
                    Vector3 indexTipPosition = indexFinger.TipPosition;
                    Vector3 thumbTipPosition = thumbFinger.TipPosition;

                    // �l�����w�Ɛe�w�̊Ԃ̋������v�Z����
                    float pinchDistance = Vector3.Distance(indexTipPosition, thumbTipPosition);
                    float pinchThreshold = 0.05f; // �s���`�Ɣ��f���鋗����臒l

                    // �s���`���삪���o���ꂽ�ꍇ�̂݃L���[�u����]������
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

    void RotateCube(float deltaX) // RotateCube���\�b�h���`�Bvoid�͖߂�l�Ȃ�
    {
        // ��̓����Ɋ�Â��ăL���[�u����]������
        // deltaX�͎�̍��E�̓����̑傫��
        // ��]���x�̌W���i500.0f�j�𒲐����āA��]�̑�����ς�����
        float rotationSpeed = deltaX * 500.0f;
        // �L���[�u��y������ɉ�]������
        cube.transform.Rotate(0, rotationSpeed, 0);
    }
}
