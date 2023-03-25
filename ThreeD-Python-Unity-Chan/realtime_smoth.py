import cv2
from cvzone.PoseModule import PoseDetector
import socket
import numpy as np

# cap = cv2.VideoCapture('WeChat_20230218110458.mp4')
# cap = cv2.VideoCapture(0)
cap = cv2.VideoCapture('kaihetiao.mp4')

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5053)      # 定义localhost与端口，当然可以定义其他的host

detector = PoseDetector()
# posList = []    # 保存到txt在unity中读取需要数组列表
jointnum = 33

# 平滑
# kalman filter parameters
KalmanParamQ = 0.001
KalmanParamR = 0.0015
K = np.zeros((jointnum,3),dtype=np.float32)
P = np.zeros((jointnum,3),dtype=np.float32)
X = np.zeros((jointnum,3),dtype=np.float32)
# low pass filter parameters
PrevPose3D = np.zeros((6,jointnum,3),dtype=np.float32)

while True:
    success, img = cap.read()
    # img = cv2.resize(img, (480,860))
    
    if not success:
        cap = cv2.VideoCapture('kaihetiao.mp4')
        PrevPose3D = np.zeros((6,jointnum,3),dtype=np.float32)
        continue
        # break
    img = detector.findPose(img)
    lmList, bboxInfo = detector.findPosition(img)

    if bboxInfo:
        lmString = ''
        currdata = np.squeeze(lmList)
        currdata = currdata[:,1:]
        smooth_kps = np.zeros((jointnum,3),dtype=np.float32)
        '''
        kalman filter
        
        '''
        for i in range(jointnum):
            K[i] = (P[i] + KalmanParamQ) / (P[i] + KalmanParamQ + KalmanParamR)
            P[i] = KalmanParamR * (P[i] + KalmanParamQ) / (P[i] + KalmanParamQ + KalmanParamR)
        for i in range(jointnum):
            smooth_kps[i] = X[i] + (currdata[i] - X[i])*K[i]
            X[i] = smooth_kps[i]

        # datakalman[idx] = smooth_kps # record kalman result

        '''
        low pass filter
        '''    
        LowPassParam = 0.1
        PrevPose3D[0] = smooth_kps
        for j in range(1,6):
            PrevPose3D[j] = PrevPose3D[j] * LowPassParam + PrevPose3D[j - 1] * (1.0 - LowPassParam)
        # datafinal[idx] = PrevPose3D[5] # record kalman+low pass result.
        print(PrevPose3D[5])
        for lm in PrevPose3D[5]:
            print(lm)
            lmString += f'{lm[0]},{img.shape[0] - lm[1]},{lm[2]},'
        # posList.append(lmString)
    
    date = lmString
    sock.sendto(str.encode(str(date)), serverAddressPort)
    
    cv2.namedWindow("Image",0)
    cv2.resizeWindow("Image", 360, 640);
    cv2.imshow("Image", img)
    key = cv2.waitKey(1)
    # 记录数据到本地
    # if key == ord('r'):    
    # with open("MotionData.txt", 'w') as f:
    #     f.writelines(["%s\n" % item for item in posList])
    if key == ord('q'):
        break
