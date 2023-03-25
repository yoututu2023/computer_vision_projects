using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using System.IO;

public class main : MonoBehaviour
{
    /// <summary> Joints Name
    /// rShldrBend 0, rForearmBend 1, rHand 2, rThumb2 3, rMid1 4,
    /// lShldrBend 5, lForearmBend 6, lHand 7, lThumb2 8, lMid1 9,
    /// lEar 10, lEye 11, rEar 12, rEye 13, Nose 14,
    /// rThighBend 15, rShin 16, rFoot 17, rToe 18,
    /// lThighBend 19, lShin 20, lFoot 21, lToe 22,    
    /// abdomenUpper 23,
    /// hip 24, head 25, neck 26, spine 27
    /// 
    /// </summary>

    private Vector3 initPos;
    //动画相关
    Animator animator;
    private Transform root, spine, neck, head, leye, reye, lshoulder, lelbow, lhand, lthumb2, lmid1, rshoulder, relbow, rhand, rthumb2, rmid1, lhip, lknee, lfoot, ltoe, rhip, rknee, rfoot, rtoe;
    private Quaternion midRoot, midSpine, midNeck, midHead, midLshoulder, midLelbow, midLhand, midRshoulder, midRelbow, midRhand, midLhip, midLknee, midLfoot, midRhip, midRknee, midRfoot;
    public Transform nose;
    // 接受数据
    public UDPReceive udpReceive;

    //mediapipe 到 posenet
    Dictionary<int, int> media2pose_index = new Dictionary<int, int> {
        {0, 11},
        {1, 13},
        {2, 15},
        {3, 21},
        {4, 17},
        {5, 12},
        {6, 14},
        {7, 16},
        {8, 22},
        {9, 18},
        {10, 7},
        {11, 2},
        {12, 8},
        {13, 5},
        {14, 0},
        {15, 23},
        {16, 25},
        {17, 27},
        {18, 31},
        {19, 24},
        {20, 26},
        {21, 28},
        {22, 32}
    };
    //画骨骼
    public GameObject[] sket;
    void Start()
    {
        // 动画相关
        animator = this.GetComponent<Animator>();
        //躯干
        root = animator.GetBoneTransform(HumanBodyBones.Hips);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        leye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
        reye = animator.GetBoneTransform(HumanBodyBones.RightEye);
        //左臂
        lshoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        lelbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        lhand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        lthumb2 = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        lmid1 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        //右臂
        rshoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        relbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rhand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        rthumb2 = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rmid1 = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        //左腿
        lhip = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        lknee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        lfoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        ltoe = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        //右腿
        rhip = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        rknee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        rfoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        rtoe = animator.GetBoneTransform(HumanBodyBones.RightToes);

        initPos = root.position;
        // 求取中间变换矩阵
        // 当前旋转 = lookforward * 中间矩阵
        Vector3 forward = TriangleNormal(root.position, lhip.position, rhip.position);
        // Root
        midRoot = Quaternion.Inverse(root.rotation) * Quaternion.LookRotation(forward);
        // 躯干
        midSpine = Quaternion.Inverse(spine.rotation) * Quaternion.LookRotation(spine.position - neck.position, forward);
        midNeck = Quaternion.Inverse(neck.rotation) * Quaternion.LookRotation(neck.position - head.position, forward);
        // 头部
        midHead = Quaternion.Inverse(head.rotation) * Quaternion.LookRotation(nose.position - head.position);
        // 左臂
        midLshoulder = Quaternion.Inverse(lshoulder.rotation) * Quaternion.LookRotation(lshoulder.position - lelbow.position, forward);
        midLelbow = Quaternion.Inverse(lelbow.rotation) * Quaternion.LookRotation(lelbow.position - lhand.position, forward);
        midLhand = Quaternion.Inverse(lhand.rotation) * Quaternion.LookRotation(
            lthumb2.position - lmid1.position,
            TriangleNormal(lhand.position, lthumb2.position, lmid1.position)
            );
        // 右臂
        midRshoulder = Quaternion.Inverse(rshoulder.rotation) * Quaternion.LookRotation(rshoulder.position - relbow.position, forward);
        midRelbow = Quaternion.Inverse(relbow.rotation) * Quaternion.LookRotation(relbow.position - rhand.position, forward);
        midRhand = Quaternion.Inverse(rhand.rotation) * Quaternion.LookRotation(
            rthumb2.position - rmid1.position,
            TriangleNormal(rhand.position, rthumb2.position, rmid1.position)
            );
        // 左腿
        midLhip = Quaternion.Inverse(lhip.rotation) * Quaternion.LookRotation(lhip.position - lknee.position, forward);
        midLknee = Quaternion.Inverse(lknee.rotation) * Quaternion.LookRotation(lknee.position - lfoot.position, forward);
        midLfoot = Quaternion.Inverse(lfoot.rotation) * Quaternion.LookRotation(lfoot.position - ltoe.position, lknee.position-lfoot.position);
        // 右腿
        midRhip = Quaternion.Inverse(rhip.rotation) * Quaternion.LookRotation(rhip.position - rknee.position, forward);
        midRknee = Quaternion.Inverse(rknee.rotation) * Quaternion.LookRotation(rknee.position - rfoot.position, forward);
        midRfoot = Quaternion.Inverse(rfoot.rotation) * Quaternion.LookRotation(rfoot.position - rtoe.position, rknee.position-rfoot.position);
    }

    // 计算三角形法向量
    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    // 更新姿势
    void updatePose()
    {
        //实时传输
        string data = udpReceive.data;
        if (data.Length == 0)
        {
            return;
        }
        string[] points = data.Split(',');
        if (points.Length == 0)
        {
            return;
        }
        List<Vector3> pred3d = new List<Vector3>();
        foreach (KeyValuePair<int, int> kvp in media2pose_index)
        {
            print(kvp.Key);
            print(kvp.Value);
            float x = float.Parse(points[0 + (kvp.Value * 3)]) / 3;
            float y = float.Parse(points[1 + (kvp.Value * 3)]) / 3;
            float z = float.Parse(points[2 + (kvp.Value * 3)]) / 10;
            pred3d.Add(new Vector3(x, y, z));
        }
        /// rShldrBend 0, rForearmBend 1, rHand 2, rThumb2 3, rMid1 4,
        /// lShldrBend 5, lForearmBend 6, lHand 7, lThumb2 8, lMid1 9,
        /// lEar 10, lEye 11, rEar 12, rEye 13, Nose 14,
        /// rThighBend 15, rShin 16, rFoot 17, rToe 18,
        /// lThighBend 19, lShin 20, lFoot 21, lToe 22,    
        /// abdomenUpper 23,
        /// hip 24, head 25, neck 26, spine 27
        //手动计算
        Vector3 necks = new Vector3((pred3d[5].x + pred3d[0].x) / 2, (pred3d[5].y + pred3d[0].y) / 2, (pred3d[0].z + pred3d[5].z) / 2);
        Vector3 hips = new Vector3((pred3d[15].x + pred3d[19].x) / 2, (pred3d[15].y + pred3d[19].y) / 2, (pred3d[15].z + pred3d[19].z) / 2);
        //腹部上部
        pred3d.Add((hips + necks) / 2);
        //臀部
        pred3d.Add((hips + pred3d[23]) / 2);
        Vector3 nhv = Vector3.Normalize((pred3d[10] + pred3d[12]) / 2 - necks);
        Vector3 nv = pred3d[14] - necks;
        Vector3 heads = necks + nhv * Vector3.Dot(nhv, nv);
        pred3d.Add(heads);
        //脖子
        pred3d.Add(necks);
        //脊柱
        pred3d.Add(new Vector3((pred3d[0].x + pred3d[19].x) / 2, (pred3d[0].y + pred3d[19].y) / 2, (pred3d[0].z + pred3d[19].z) / 2));

        for (int i = 0; i <= 32; i++)
        {
            float x = float.Parse(points[0 + (i * 3)]) / 400;
            float y = float.Parse(points[1 + (i * 3)]) / 400;
            float z = float.Parse(points[2 + (i * 3)]) / 1300;
            sket[i].transform.localPosition = new Vector3(x, y, z);
        }
        //根据预测身高更新root位置
        float tallShin = (Vector3.Distance(pred3d[16], pred3d[17]) + Vector3.Distance(pred3d[20], pred3d[21]))/2.0f;
        float tallThigh = (Vector3.Distance(pred3d[15], pred3d[16]) + Vector3.Distance(pred3d[19], pred3d[20]))/2.0f;
        float tallUnity = (Vector3.Distance(lhip.position, lknee.position) + Vector3.Distance(lknee.position, lfoot.position)) / 2.0f +
            (Vector3.Distance(rhip.position, rknee.position) + Vector3.Distance(rknee.position, rfoot.position));
        root.position = pred3d[24] * (tallUnity/(tallThigh+tallShin));

        //更新旋转
        //当前旋转 = lookforward * 中间矩阵
        Vector3 forward = TriangleNormal(pred3d[24], pred3d[19], pred3d[15]);
        // Root
        root.rotation = Quaternion.LookRotation(forward) * Quaternion.Inverse(midRoot);
        ////身体朝向
        // 躯干
        spine.rotation = Quaternion.LookRotation(pred3d[27] - pred3d[26], forward) * Quaternion.Inverse(midSpine);
        neck.rotation = Quaternion.LookRotation(pred3d[26] - pred3d[25], forward) * Quaternion.Inverse(midNeck);
        lshoulder.rotation = Quaternion.LookRotation(pred3d[5] - pred3d[6], forward) * Quaternion.Inverse(midLshoulder);
        lelbow.rotation = Quaternion.LookRotation(pred3d[6] - pred3d[7], forward) * Quaternion.Inverse(midLelbow);
        rshoulder.rotation = Quaternion.LookRotation(pred3d[0] - pred3d[1], forward) * Quaternion.Inverse(midRshoulder);
        relbow.rotation = Quaternion.LookRotation(pred3d[1] - pred3d[2], forward) * Quaternion.Inverse(midRelbow);
        lhip.rotation = Quaternion.LookRotation(pred3d[19] - pred3d[20], forward) * Quaternion.Inverse(midLhip);
        lknee.rotation = Quaternion.LookRotation(pred3d[20] - pred3d[21], forward) * Quaternion.Inverse(midLknee);
        rhip.rotation = Quaternion.LookRotation(pred3d[15] - pred3d[16], forward) * Quaternion.Inverse(midRhip);
        rknee.rotation = Quaternion.LookRotation(pred3d[16] - pred3d[17], forward) * Quaternion.Inverse(midRknee);
        ////
        head.rotation = Quaternion.LookRotation(pred3d[14] - pred3d[25], TriangleNormal(pred3d[14], pred3d[12], pred3d[10])) * Quaternion.Inverse(midHead);        
        lhand.rotation = Quaternion.LookRotation(
            pred3d[8] - pred3d[9],
            TriangleNormal(pred3d[7], pred3d[8], pred3d[9]))*Quaternion.Inverse(midLhand);
        rhand.rotation = Quaternion.LookRotation(
            pred3d[3] - pred3d[4],
            TriangleNormal(pred3d[2], pred3d[3], pred3d[4])) * Quaternion.Inverse(midRhand);
        lfoot.rotation = Quaternion.LookRotation(pred3d[21] - pred3d[22], pred3d[20]-pred3d[21])*Quaternion.Inverse(midLfoot);
        rfoot.rotation = Quaternion.LookRotation(pred3d[17] - pred3d[18], pred3d[16]-pred3d[17])*Quaternion.Inverse(midRfoot);
    }

    // Update is called once per frame
    void Update()
    {
        updatePose();
        
    }
}
